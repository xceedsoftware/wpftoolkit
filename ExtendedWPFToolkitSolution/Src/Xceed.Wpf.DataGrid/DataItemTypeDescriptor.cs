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
using System.Linq;

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
      var replaceEvents = default( Dictionary<EventDescriptor, DataItemEventDescriptorBase> );

      lock( entry )
      {
        replaceEvents = entry.Events;

        if( replaceEvents == null )
        {
          replaceEvents = new Dictionary<EventDescriptor, DataItemEventDescriptorBase>();
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

        DataItemEventDescriptorBase destinationEvent;
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
      var replaceProperties = default( Dictionary<PropertyDescriptor, DataItemPropertyDescriptorBase> );

      lock( entry )
      {
        replaceProperties = entry.Properties;

        if( replaceProperties == null )
        {
          replaceProperties = new Dictionary<PropertyDescriptor, DataItemPropertyDescriptorBase>();
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

        DataItemPropertyDescriptorBase destinationProperty;
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

      internal Dictionary<EventDescriptor, DataItemEventDescriptorBase> Events
      {
        get;
        set;
      }

      internal Dictionary<PropertyDescriptor, DataItemPropertyDescriptorBase> Properties
      {
        get;
        set;
      }
    }

    #endregion
  }
}
