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

namespace Xceed.Wpf.DataGrid
{
  internal class DataItemIndexerDescriptor : DataItemPropertyDescriptorBase
  {
    internal DataItemIndexerDescriptor( DataItemTypeDescriptor owner, IndexerDescriptor parent )
      : base( owner, parent, parent.GetValue, ( !parent.IsReadOnly ) ? parent.SetValue : default( Action<object, object> ), null )
    {
      m_descriptor = parent;
    }

    #region ComponentType Property

    public sealed override Type ComponentType
    {
      get
      {
        return m_descriptor.ComponentType;
      }
    }

    #endregion

    #region PropertyType Property

    public sealed override Type PropertyType
    {
      get
      {
        return m_descriptor.PropertyType;
      }
    }

    #endregion

    #region DisplayName Property

    public override string DisplayName
    {
      get
      {
        return m_descriptor.DisplayName;
      }
    }

    #endregion

    #region IndexerParameters Internal Property

    internal string IndexerParameters
    {
      get
      {
        return m_descriptor.IndexerParameters;
      }
    }

    #endregion

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override bool Equals( object obj )
    {
      return ( obj is DataItemIndexerDescriptor )
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

    private readonly IndexerDescriptor m_descriptor;
  }
}
