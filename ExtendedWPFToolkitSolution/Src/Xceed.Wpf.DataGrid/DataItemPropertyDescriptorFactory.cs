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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DataItemPropertyDescriptorFactory
  {
    #region Static Fields

    private static readonly Type s_reflectPropertyDescriptorType;

    #endregion

    #region Constructor

    static DataItemPropertyDescriptorFactory()
    {
      s_reflectPropertyDescriptorType = Assembly.GetAssembly( typeof( PropertyDescriptor ) ).GetType( "System.ComponentModel.ReflectPropertyDescriptor" );
    }

    internal DataItemPropertyDescriptorFactory( DataItemTypeDescriptor typeDescriptor )
    {
      if( typeDescriptor == null )
        throw new ArgumentNullException( "typeDescriptor" );

      m_typeDescriptor = typeDescriptor;
    }

    #endregion

    internal DataItemPropertyDescriptor CreatePropertyDescriptor( PropertyDescriptor source )
    {
      if( !DataItemPropertyDescriptorFactory.IsReflected( source ) )
        return null;

      var componentType = source.ComponentType;

      PropertyInfo propertyInfo;
      try
      {
        propertyInfo = componentType.GetProperty( source.Name, BindingFlags.Public | BindingFlags.Instance );
        if( ( propertyInfo == null ) || DataItemPropertyDescriptorFactory.IsIndexed( propertyInfo ) )
          return null;
      }
      catch( AmbiguousMatchException )
      {
        return null;
      }

      var propertyType = propertyInfo.PropertyType;
      Debug.Assert( propertyType == source.PropertyType );

      var creatorHelper = DelegateCreatorHelperFactory.CreateHelper( componentType, propertyType );

      var getter = DataItemPropertyDescriptorFactory.CreateGetter( propertyInfo, creatorHelper );
      if( getter == null )
        return null;

      var setter = DataItemPropertyDescriptorFactory.CreateSetter( propertyInfo, creatorHelper );
      var resetter = DataItemPropertyDescriptorFactory.CreateResetter( componentType, propertyInfo, creatorHelper );

      var propertyChangedEvent = this.GetPropertyChangedEvent( propertyInfo.Name );
      if( propertyChangedEvent != null )
        return new ValueChangeDataItemPropertyDescriptor( m_typeDescriptor, source, componentType, propertyType, getter, setter, resetter, propertyChangedEvent );

      return new DataItemPropertyDescriptor( m_typeDescriptor, source, componentType, propertyType, getter, setter, resetter );
    }

    internal DataItemIndexerDescriptor CreateIndexerDescriptor( IndexerDescriptor source )
    {
      var propertyChangedEvent = this.GetPropertyChangedEvent( null );
      if( propertyChangedEvent != null )
        return new ValueChangeDataItemIndexerDescriptor( m_typeDescriptor, source, propertyChangedEvent );

      return new DataItemIndexerDescriptor( m_typeDescriptor, source );
    }

    private void InitializeEvents()
    {
      if( m_eventDescriptors != null )
        return;

      m_eventDescriptors = m_typeDescriptor.GetEvents();
      m_notifyPropertyChangedEventDescriptor = ( from ed in m_eventDescriptors.Cast<EventDescriptor>()
                                                 where ( ed != null )
                                                    && ( ed.Name == "PropertyChanged" )
                                                    && ( typeof( INotifyPropertyChanged ).IsAssignableFrom( ed.ComponentType ) )
                                                    && ( typeof( PropertyChangedEventHandler ).IsAssignableFrom( ed.EventType ) )
                                                 select ed ).FirstOrDefault();
    }

    private EventDescriptor GetNotifyPropertyChangedEvent()
    {
      this.InitializeEvents();

      return m_notifyPropertyChangedEventDescriptor;
    }

    private EventDescriptor GetSinglePropertyChangedEvent( string propertyName )
    {
      this.InitializeEvents();

      if( string.IsNullOrEmpty( propertyName ) )
        return null;

      var eventName = string.Format( CultureInfo.InvariantCulture, "{0}Changed", propertyName );

      return ( from ed in m_eventDescriptors.Cast<EventDescriptor>()
               where ( ed != null )
                  && ( ed.Name == eventName )
                  && ( typeof( EventHandler ).IsAssignableFrom( ed.EventType ) )
               select ed ).FirstOrDefault();
    }

    private EventDescriptor GetPropertyChangedEvent( string propertyName )
    {
      var descriptor = this.GetSinglePropertyChangedEvent( propertyName );
      if( descriptor != null )
        return descriptor;

      return this.GetNotifyPropertyChangedEvent();
    }

    private static bool IsReflected( PropertyDescriptor descriptor )
    {
      return ( descriptor != null )
          && ( descriptor.GetType() == s_reflectPropertyDescriptorType );
    }

    private static bool IsIndexed( PropertyInfo propertyInfo )
    {
      var parameters = propertyInfo.GetIndexParameters();

      return ( parameters != null )
          && ( parameters.Length > 0 );
    }

    private static Func<object, object> CreateGetter( PropertyInfo propertyInfo, IDelegateCreatorHelper helper )
    {
      if( !propertyInfo.CanRead )
        return null;

      var methodInfo = propertyInfo.GetGetMethod();
      if( methodInfo == null )
        return null;

      Debug.Assert( methodInfo.IsPublic );


      return helper.CreateGetter( methodInfo );
    }

    private static Action<object, object> CreateSetter( PropertyInfo propertyInfo, IDelegateCreatorHelper helper )
    {
      if( !propertyInfo.CanWrite )
        return null;

      var methodInfo = propertyInfo.GetSetMethod();
      if( methodInfo == null )
        return null;

      Debug.Assert( methodInfo.IsPublic );


      return helper.CreateSetter( methodInfo );
    }

    private static Action<object> CreateResetter( Type componentType, PropertyInfo propertyInfo, IDelegateCreatorHelper helper )
    {
      MethodInfo methodInfo;
      try
      {
        methodInfo = componentType.GetMethod( string.Format( CultureInfo.InvariantCulture, "Reset{0}", propertyInfo.Name ), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null );
        if( ( methodInfo == null ) || methodInfo.IsGenericMethod )
          return null;
      }
      catch( AmbiguousMatchException )
      {
        return null;
      }

      Debug.Assert( methodInfo.IsPublic );


      return helper.CreateResetter( methodInfo );
    }

    #region Private Fields

    private readonly DataItemTypeDescriptor m_typeDescriptor;
    private EventDescriptorCollection m_eventDescriptors; //null
    private EventDescriptor m_notifyPropertyChangedEventDescriptor; //null

    #endregion

    #region IDelegateCreatorHelper Private Interface

    private interface IDelegateCreatorHelper
    {
      Func<object, object> CreateGetter( MethodInfo methodInfo );
      Action<object, object> CreateSetter( MethodInfo methodInfo );
      Action<object> CreateResetter( MethodInfo methodInfo );
    }

    #endregion

    #region DelegateCreatorHelperFactory Private Class

    private static class DelegateCreatorHelperFactory
    {
      internal static IDelegateCreatorHelper CreateHelper( Type componentType, Type propertyType )
      {
        return ( IDelegateCreatorHelper )typeof( DelegateCreatorHelper<,> ).MakeGenericType( componentType, propertyType ).GetConstructor( Type.EmptyTypes ).Invoke( null );
      }
    }

    #endregion

    #region DelegateCreatorHelper<> Private Class

    private sealed class DelegateCreatorHelper<TSource, TProperty> : IDelegateCreatorHelper
    {
      public Func<object, object> CreateGetter( MethodInfo methodInfo )
      {
        Debug.Assert( methodInfo != null );
        Debug.Assert( methodInfo.IsPublic );
        Debug.Assert( methodInfo.GetParameters().Length == 0 );
        Debug.Assert( methodInfo.ReturnType == typeof( TProperty ) );

        if( typeof( TSource ).IsValueType )
        {
          var d = ( ValueTypeGetter )Delegate.CreateDelegate( typeof( ValueTypeGetter ), methodInfo, false );
          if( d != null )
            return ( source ) =>
            {
              if( !( source is TSource ) )
                return default( TProperty );

              var s = ( TSource )source;

              return d.Invoke( ref s );
            };
        }
        else
        {
          var d = ( Func<TSource, TProperty> )Delegate.CreateDelegate( typeof( Func<TSource, TProperty> ), methodInfo, false );
          if( d != null )
            return ( source ) =>
            {
              if( !( source is TSource ) )
                return default( TProperty );

              return d.Invoke( ( TSource )source );
            };
        }

        return ( source ) =>
        {
          if( !( source is TSource ) )
            return default( TProperty );

          return methodInfo.Invoke( source, null );
        };
      }

      public Action<object, object> CreateSetter( MethodInfo methodInfo )
      {
        Debug.Assert( methodInfo != null );
        Debug.Assert( methodInfo.IsPublic );
        Debug.Assert( methodInfo.GetParameters().Length == 1 );
        Debug.Assert( methodInfo.GetParameters()[ 0 ].ParameterType == typeof( TProperty ) );

        if( !typeof( TSource ).IsValueType )
        {
          var d = ( Action<TSource, TProperty> )Delegate.CreateDelegate( typeof( Action<TSource, TProperty> ), methodInfo );
          if( d != null )
            return ( source, value ) =>
            {
              if( !( source is TSource ) )
                return;

              d.Invoke( ( TSource )source, ( TProperty )value );
            };
        }

        return ( source, value ) =>
        {
          if( !( source is TSource ) )
            return;

          methodInfo.Invoke( source, new object[] { value } );
        };
      }

      public Action<object> CreateResetter( MethodInfo methodInfo )
      {
        Debug.Assert( methodInfo != null );
        Debug.Assert( methodInfo.IsPublic );
        Debug.Assert( methodInfo.GetParameters().Length == 0 );

        if( !typeof( TSource ).IsValueType )
        {
          var d = ( Action<TSource> )Delegate.CreateDelegate( typeof( Action<TSource> ), methodInfo );
          if( d != null )
            return ( source ) =>
            {
              if( !( source is TSource ) )
                return;

              d.Invoke( ( TSource )source );
            };
        }

        return ( source ) =>
        {
          if( !( source is TSource ) )
            return;

          methodInfo.Invoke( source, null );
        };
      }

      private delegate TProperty ValueTypeGetter( ref TSource source );
    }

    #endregion
  }
}
