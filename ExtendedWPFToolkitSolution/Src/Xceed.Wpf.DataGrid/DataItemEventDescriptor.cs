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

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DataItemEventDescriptor : DataItemEventDescriptorBase
  {
    #region Constructor

    internal DataItemEventDescriptor(
      DataItemTypeDescriptor owner,
      EventDescriptor parent,
      Type componentType,
      Type eventType,
      Action<object, object> addHandler,
      Action<object, object> removeHandler )
      : base( owner, parent )
    {
      if( componentType == null )
        throw new ArgumentNullException( "componentType" );

      if( eventType == null )
        throw new ArgumentNullException( "eventType" );

      if( addHandler == null )
        throw new ArgumentNullException( "addHandler" );

      if( removeHandler == null )
        throw new ArgumentNullException( "removeHandler" );

      m_componentType = componentType;
      m_eventType = eventType;
      m_addHandler = addHandler;
      m_removeHandler = removeHandler;
    }

    #endregion

    #region ComponentType Property

    public override Type ComponentType
    {
      get
      {
        return m_componentType;
      }
    }

    private readonly Type m_componentType;

    #endregion

    #region EventType Property

    public override Type EventType
    {
      get
      {
        return m_eventType;
      }
    }

    private readonly Type m_eventType;

    #endregion

    public override void AddEventHandler( object component, Delegate value )
    {
      if( component == null )
        return;

      m_addHandler.Invoke( component, value );
    }

    public override void RemoveEventHandler( object component, Delegate value )
    {
      if( component == null )
        return;

      m_removeHandler.Invoke( component, value );
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override bool Equals( object obj )
    {
      return ( obj is DataItemEventDescriptor )
          && ( base.Equals( obj ) );
    }

    #region Private Fields

    private readonly Action<object, object> m_addHandler;
    private readonly Action<object, object> m_removeHandler;

    #endregion
  }
}
