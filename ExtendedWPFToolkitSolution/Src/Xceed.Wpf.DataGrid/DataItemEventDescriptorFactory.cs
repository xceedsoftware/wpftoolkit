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
using System.Reflection;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DataItemEventDescriptorFactory
  {
    #region Static Fields

    private static readonly Type s_reflectEventDescriptorType;

    #endregion

    static DataItemEventDescriptorFactory()
    {
      s_reflectEventDescriptorType = Assembly.GetAssembly( typeof( EventDescriptor ) ).GetType( "System.ComponentModel.ReflectEventDescriptor" );
    }

    internal DataItemEventDescriptorFactory( DataItemTypeDescriptor typeDescriptor )
    {
      if( typeDescriptor == null )
        throw new ArgumentNullException( "typeDescriptor" );

      m_typeDescriptor = typeDescriptor;
    }

    internal DataItemEventDescriptor CreateEventDescriptor( EventDescriptor source )
    {
      if( !DataItemEventDescriptorFactory.IsReflected( source ) )
        return null;

      var componentType = source.ComponentType;

      EventInfo eventInfo;
      try
      {
        eventInfo = componentType.GetEvent( source.Name, BindingFlags.Public | BindingFlags.Instance );
        if( eventInfo == null )
          return null;
      }
      catch( AmbiguousMatchException )
      {
        return null;
      }

      var handlerType = eventInfo.EventHandlerType;
      Debug.Assert( handlerType == source.EventType );

      var creatorHelper = DelegateCreatorHelperFactory.CreateHelper( componentType, handlerType );

      var addHandler = DataItemEventDescriptorFactory.CreateAddHandler( eventInfo, creatorHelper );
      if( addHandler == null )
        return null;

      var removeHandler = DataItemEventDescriptorFactory.CreateRemoveHandler( eventInfo, creatorHelper );
      if( removeHandler == null )
        return null;

      return new DataItemEventDescriptor( m_typeDescriptor, source, componentType, handlerType, addHandler, removeHandler );
    }

    private static bool IsReflected( EventDescriptor descriptor )
    {
      return ( descriptor != null )
          && ( descriptor.GetType() == s_reflectEventDescriptorType );
    }

    private static Action<object, object> CreateAddHandler( EventInfo eventInfo, IDelegateCreatorHelper helper )
    {
      var methodInfo = eventInfo.GetAddMethod();
      if( methodInfo == null )
        return null;

      Debug.Assert( methodInfo.IsPublic );


      return helper.CreateAddOrRemoveHandler( methodInfo );
    }

    private static Action<object, object> CreateRemoveHandler( EventInfo eventInfo, IDelegateCreatorHelper helper )
    {
      var methodInfo = eventInfo.GetRemoveMethod();
      if( methodInfo == null )
        return null;

      Debug.Assert( methodInfo.IsPublic );


      return helper.CreateAddOrRemoveHandler( methodInfo );
    }

    #region Private Fields

    private readonly DataItemTypeDescriptor m_typeDescriptor;

    #endregion

    #region IDelegateCreatorHelper Private Interface

    private interface IDelegateCreatorHelper
    {
      Action<object, object> CreateAddOrRemoveHandler( MethodInfo methodInfo );
    }

    #endregion

    #region DelegateCreatorHelperFactory Private Class

    private static class DelegateCreatorHelperFactory
    {
      internal static IDelegateCreatorHelper CreateHelper( Type componentType, Type handleType )
      {
        return ( IDelegateCreatorHelper )typeof( DelegateCreatorHelper<,> ).MakeGenericType( componentType, handleType ).GetConstructor( Type.EmptyTypes ).Invoke( null );
      }
    }

    #endregion

    #region DelegateCreatorHelper<> Private Class

    private sealed class DelegateCreatorHelper<TSource, THandler> : IDelegateCreatorHelper
    {
      public Action<object, object> CreateAddOrRemoveHandler( MethodInfo methodInfo )
      {
        Debug.Assert( methodInfo != null );
        Debug.Assert( methodInfo.IsPublic );
        Debug.Assert( methodInfo.GetParameters().Length == 1 );
        Debug.Assert( methodInfo.GetParameters()[ 0 ].ParameterType == typeof( THandler ) );

        if( !typeof( TSource ).IsValueType )
        {
          var d = ( Action<TSource, THandler> )Delegate.CreateDelegate( typeof( Action<TSource, THandler> ), methodInfo, false );
          if( d != null )
            return ( source, handler ) =>
              {
                if( !( source is TSource ) )
                  return;

                d.Invoke( ( TSource )source, ( THandler )handler );
              };
        }

        return ( source, handler ) =>
          {
            if( !( source is TSource ) )
              return;

            methodInfo.Invoke( source, new object[] { handler } );
          };
      }
    }

    #endregion
  }
}
