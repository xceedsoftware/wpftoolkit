/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DataItemTypeDescriptor : CustomTypeDescriptor
  {
    #region Static Fields

    private static readonly List<WeakReference> s_entries = new List<WeakReference>();

    #endregion

    #region Constructors

    internal DataItemTypeDescriptor( ICustomTypeDescriptor parent, Type objectType )
      : base( parent )
    {
      if( parent == null )
        throw new ArgumentNullException( "parent" );

      m_targetType = objectType;
      m_descriptorType = parent.GetType();
    }

    #endregion

    public override EventDescriptorCollection GetEvents()
    {
      return this.GetEvents( null );
    }

    public override EventDescriptorCollection GetEvents( Attribute[] attributes )
    {
      var sourceEvents = base.GetEvents( attributes );
      if( ( sourceEvents == null ) || ( sourceEvents.Count == 0 ) )
        return sourceEvents;

      var entry = this.GetEntry();
      var replaceEvents = default( Dictionary<EventDescriptor, DataItemEventDescriptor> );

      lock( entry )
      {
        replaceEvents = entry.Events;

        if( replaceEvents == null )
        {
          replaceEvents = new Dictionary<EventDescriptor, DataItemEventDescriptor>();
          entry.Events = replaceEvents;

          var factory = new DataItemEventDescriptorFactory( this );

          foreach( var sourceEvent in sourceEvents.Cast<EventDescriptor>() )
          {
            var descriptor = factory.CreateEventDescriptor( sourceEvent );
            if( descriptor != null )
            {
              replaceEvents.Add( sourceEvent, descriptor );
            }
          }
        }

        if( replaceEvents.Count == 0 )
          return sourceEvents;
      }

      var destinationEvents = new EventDescriptor[ sourceEvents.Count ];

      for( int i = 0; i < sourceEvents.Count; i++ )
      {
        var sourceEvent = sourceEvents[ i ];

        DataItemEventDescriptor destinationEvent;
        if( replaceEvents.TryGetValue( sourceEvent, out destinationEvent ) )
        {
          destinationEvents[ i ] = destinationEvent;
        }
        else
        {
          destinationEvents[ i ] = sourceEvent;
        }
      }

      return new EventDescriptorCollection( destinationEvents );
    }

    public override PropertyDescriptorCollection GetProperties()
    {
      return this.GetProperties( null );
    }

    public override PropertyDescriptorCollection GetProperties( Attribute[] attributes )
    {
      var sourceProperties = base.GetProperties( attributes );
      if( ( sourceProperties == null ) || ( sourceProperties.Count == 0 ) )
        return sourceProperties;

      var entry = this.GetEntry();
      var replaceProperties = default( Dictionary<PropertyDescriptor, DataItemPropertyDescriptor> );

      lock( entry )
      {
        replaceProperties = entry.Properties;

        if( replaceProperties == null )
        {
          replaceProperties = new Dictionary<PropertyDescriptor, DataItemPropertyDescriptor>();
          entry.Properties = replaceProperties;

          var factory = new DataItemPropertyDescriptorFactory( this );

          foreach( var sourceProperty in sourceProperties.Cast<PropertyDescriptor>() )
          {
            var descriptor = factory.CreatePropertyDescriptor( sourceProperty );
            if( descriptor != null )
            {
              replaceProperties.Add( sourceProperty, descriptor );
            }
          }
        }

        if( replaceProperties.Count == 0 )
          return sourceProperties;
      }

      var destinationProperties = new PropertyDescriptor[ sourceProperties.Count ];

      for( int i = 0; i < sourceProperties.Count; i++ )
      {
        var sourceProperty = sourceProperties[ i ];

        DataItemPropertyDescriptor destinationProperty;
        if( replaceProperties.TryGetValue( sourceProperty, out destinationProperty ) )
        {
          destinationProperties[ i ] = destinationProperty;
        }
        else
        {
          destinationProperties[ i ] = sourceProperty;
        }
      }

      return new PropertyDescriptorCollection( destinationProperties );
    }

    internal DataItemIndexerDescriptor GetIndexer( object[] indexValues )
    {
      if( ( indexValues == null ) || ( indexValues.Length <= 0 ) || ( m_targetType == null ) )
        return null;

      var entry = this.GetEntry();

      lock( entry )
      {
        var indexers = entry.Indexers;
        var descriptor = default( DataItemIndexerDescriptor );
        var key = new IndexerKey( indexValues );

        if( ( indexers != null ) && ( indexers.TryGetValue( key, out descriptor ) ) )
          return descriptor;

        var indexer = this.CreateIndexerDescriptor( indexValues );
        if( indexer == null )
          return null;

        var factory = new DataItemPropertyDescriptorFactory( this );

        descriptor = factory.CreateIndexerDescriptor( indexer );
        if( descriptor == null )
          return null;

        if( indexers == null )
        {
          indexers = new Dictionary<IndexerKey, DataItemIndexerDescriptor>();
          entry.Indexers = indexers;
        }

        indexers.Add( key, descriptor );

        return descriptor;
      }
    }

    private IndexerDescriptor CreateIndexerDescriptor( object[] indexValues )
    {
      if( ( indexValues == null ) || ( indexValues.Length <= 0 ) || ( m_targetType == null ) )
        return null;

      var best = default( IndexerChoice );

      foreach( var propertyInfo in m_targetType.GetProperties() )
      {
        var candidate = IndexerChoice.GetMatch( propertyInfo, indexValues );
        if( candidate == null )
          continue;

        if( ( best == null ) || ( candidate.CompareTo( best ) > 0 ) )
        {
          best = candidate;
        }
      }

      // No matching indexer was found.
      if( best == null )
        return null;

      return IndexerDescriptor.Create( best.Property, indexValues, best.Values );
    }

    private Entry GetEntry()
    {
      if( m_entry == null )
      {
        m_entry = DataItemTypeDescriptor.GetEntry( m_targetType, m_descriptorType );
      }

      return m_entry;
    }

    private static Entry GetEntry( Type targetType, Type descriptorType )
    {
      Debug.Assert( targetType != null );
      Debug.Assert( descriptorType != null );

      lock( ( ( ICollection )s_entries ).SyncRoot )
      {
        for( int i = s_entries.Count - 1; i >= 0; i-- )
        {
          var entry = ( Entry )s_entries[ i ].Target;

          if( entry != null )
          {
            if( object.Equals( targetType, entry.TargetType ) && object.Equals( descriptorType, entry.DescriptorType ) )
              return entry;
          }
          else
          {
            s_entries.RemoveAt( i );
          }
        }

        var result = new Entry( targetType, descriptorType );

        s_entries.Add( new WeakReference( result ) );

        return result;
      }
    }

    private static bool IsNullableType( Type type )
    {
      return ( !type.IsValueType )
          || ( type.IsGenericType && ( type.GetGenericTypeDefinition() == typeof( Nullable<> ) ) );
    }

    #region Private Fields

    private readonly Type m_targetType;
    private readonly Type m_descriptorType;
    private Entry m_entry; //null

    #endregion

    #region Entry Private Class

    private sealed class Entry
    {
      internal Entry( Type targetType, Type descriptorType )
      {
        Debug.Assert( targetType != null );
        Debug.Assert( descriptorType != null );

        this.TargetType = targetType;
        this.DescriptorType = descriptorType;
      }

      internal Type TargetType
      {
        get;
        private set;
      }

      internal Type DescriptorType
      {
        get;
        private set;
      }

      internal Dictionary<EventDescriptor, DataItemEventDescriptor> Events
      {
        get;
        set;
      }

      internal Dictionary<PropertyDescriptor, DataItemPropertyDescriptor> Properties
      {
        get;
        set;
      }

      internal Dictionary<IndexerKey, DataItemIndexerDescriptor> Indexers
      {
        get;
        set;
      }
    }

    #endregion

    #region IndexerKey Private Class

    private sealed class IndexerKey
    {
      internal IndexerKey( object[] parameters )
      {
        Debug.Assert( ( parameters != null ) && ( parameters.Length > 0 ) );

        m_parameters = parameters;
      }

      public override int GetHashCode()
      {
        var count = Math.Min( m_parameters.Length, 3 );
        var hashCode = 0;

        for( int i = 0; i < count; i++ )
        {
          unchecked
          {
            hashCode *= 13;
            hashCode += m_parameters[ i ].GetHashCode();
          }
        }

        return hashCode;
      }

      public override bool Equals( object obj )
      {
        var target = obj as IndexerKey;
        if( target == null )
          return false;

        var targetParameters = target.m_parameters;
        if( targetParameters.Length != m_parameters.Length )
          return false;

        for( int i = 0; i < m_parameters.Length; i++ )
        {
          if( !object.Equals( targetParameters[ i ], m_parameters[ i ] ) )
            return false;
        }

        return true;
      }

      private readonly object[] m_parameters;
    }

    #endregion

    #region IndexerChoice Private Class

    private sealed class IndexerChoice
    {
      private IndexerChoice( PropertyInfo propertyInfo, object[] values, FitType[] fitTypes )
      {
        m_propertyInfo = propertyInfo;
        m_values = values;
        m_fitTypes = fitTypes;
      }

      internal PropertyInfo Property
      {
        get
        {
          return m_propertyInfo;
        }
      }

      internal object[] Values
      {
        get
        {
          return m_values;
        }
      }

      internal static IndexerChoice GetMatch( PropertyInfo propertyInfo, object[] parameterValues )
      {
        if( ( propertyInfo == null ) || ( parameterValues == null ) )
          return null;

        var parameters = propertyInfo.GetIndexParameters();
        if( ( parameters == null ) || ( parameters.Length != parameterValues.Length ) )
          return null;

        var values = new object[ parameters.Length ];
        var fitTypes = new FitType[ parameters.Length ];

        for( int i = 0; i < parameters.Length; i++ )
        {
          var parameterType = parameters[ i ].ParameterType;
          var parameterValue = parameterValues[ i ];
          var parameterValueType = ( parameterValue != null ) ? parameterValue.GetType() : typeof( object );

          if( parameterValue == null )
          {
            if( !DataItemTypeDescriptor.IsNullableType( parameterType ) )
              return null;

            fitTypes[ i ] = FitType.KeepAsType;
          }
          else if( !parameterType.IsAssignableFrom( parameterValueType ) )
          {
            var converter = TypeDescriptor.GetConverter( parameterType );
            if( ( converter == null ) || !converter.CanConvertFrom( parameterValueType ) )
              return null;

            try
            {
              parameterValue = converter.ConvertFrom( null, CultureInfo.InvariantCulture, parameterValue );
            }
            catch
            {
              return null;
            }

            // An indexer that takes a parameter of any other type than string is considered a better candidate than
            // an indexer that takes a parameter of type string.
            fitTypes[ i ] = ( typeof( string ).IsAssignableFrom( parameterType ) ) ? FitType.ConvertToString : FitType.ConvertToType;
          }
          else
          {
            if( typeof( string ) == parameterType )
            {
              fitTypes[ i ] = FitType.KeepAsString;
            }
            else if( typeof( object ) == parameterType )
            {
              fitTypes[ i ] = FitType.KeepAsObject;
            }
            else
            {
              fitTypes[ i ] = FitType.KeepAsType;
            }
          }

          values[ i ] = parameterValue;
        }

        return new IndexerChoice( propertyInfo, values, fitTypes );
      }

      internal int CompareTo( IndexerChoice comparand )
      {
        if( comparand == null )
          throw new ArgumentNullException( "comparand" );

        if( comparand.m_fitTypes.Length != m_fitTypes.Length )
          throw new ArgumentException( "The indexer does not have the same number of parameters.", "comparand" );

        for( int i = 0; i < m_fitTypes.Length; i++ )
        {
          var compare = m_fitTypes[ i ].CompareTo( comparand.m_fitTypes[ i ] );
          if( compare != 0 )
            return compare;
        }

        return 0;
      }

      private readonly PropertyInfo m_propertyInfo;
      private readonly object[] m_values;
      private readonly FitType[] m_fitTypes;

      private enum FitType
      {
        None = 0,
        ConvertToString,
        KeepAsObject,
        KeepAsString,
        ConvertToType,
        KeepAsType,
      }
    }

    #endregion
  }
}
