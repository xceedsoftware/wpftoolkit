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
  internal class DataItemPropertyDescriptor : DataItemPropertyDescriptorBase
  {
    #region Constructor

    internal DataItemPropertyDescriptor(
      DataItemTypeDescriptor owner,
      PropertyDescriptor parent,
      Type componentType,
      Type propertyType,
      Func<object, object> getter,
      Action<object, object> setter,
      Action<object> resetter )
      : base( owner, parent, getter, setter, resetter )
    {
      if( componentType == null )
        throw new ArgumentNullException( "componentType" );

      if( propertyType == null )
        throw new ArgumentNullException( "propertyType" );

      m_componentType = componentType;
      m_propertyType = propertyType;
    }

    #endregion

    #region ComponentType Property

    public sealed override Type ComponentType
    {
      get
      {
        return m_componentType;
      }
    }

    private readonly Type m_componentType;

    #endregion

    #region PropertyType Property

    public sealed override Type PropertyType
    {
      get
      {
        return m_propertyType;
      }
    }

    private readonly Type m_propertyType;

    #endregion

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override bool Equals( object obj )
    {
      return ( obj is DataItemPropertyDescriptor )
          && ( base.Equals( obj ) );
    }

    protected override bool MustInhibitValueChanged( object component )
    {
      return false;
    }

    protected override bool IsValueChangedInhibited( object component )
    {
      return false;
    }

    protected override void InhibitValueChanged( object component )
    {
    }

    protected override void ResetInhibitValueChanged( object component )
    {
    }
  }
}
