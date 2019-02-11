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
  internal abstract class DataItemEventDescriptorBase : EventDescriptor
  {
    protected DataItemEventDescriptorBase( DataItemTypeDescriptor owner, EventDescriptor parent )
      : base( parent )
    {
      if( owner == null )
        throw new ArgumentNullException( "owner" );

      m_owner = owner;
    }

    #region IsMulticast Property

    public override bool IsMulticast
    {
      get
      {
        return typeof( MulticastDelegate ).IsAssignableFrom( this.EventType );
      }
    }

    #endregion

    #region Owner Protected Property

    protected DataItemTypeDescriptor Owner
    {
      get
      {
        return m_owner;
      }
    }

    private readonly DataItemTypeDescriptor m_owner;

    #endregion

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override bool Equals( object obj )
    {
      var descriptor = obj as DataItemEventDescriptorBase;
      if( object.ReferenceEquals( descriptor, null ) )
        return false;

      return ( base.Equals( obj ) )
          && ( descriptor.ComponentType == this.ComponentType )
          && ( descriptor.EventType == this.EventType )
          && ( descriptor.IsMulticast == this.IsMulticast );
    }
  }
}
