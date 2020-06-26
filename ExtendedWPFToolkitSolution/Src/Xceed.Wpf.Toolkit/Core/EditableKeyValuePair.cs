/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Xceed.Wpf.Toolkit.Core
{
  public class EditableKeyValuePair<TKey, TValue> : CustomTypeDescriptor
  {
    #region Members

    private PropertyDescriptorCollection _properties;

    #endregion

    #region Properties

    public TKey Key
    {
      get;
      set;
    }

    public TValue Value
    {
      get;
      set;
    }

    #endregion

    #region Constructors

    //Necessary for adding new items in CollectionEditor
    public EditableKeyValuePair()
    {
      var propertyList = new List<PropertyDescriptor>();

      var KeyDescriptor = TypeDescriptor.CreateProperty( this.GetType(), "Key", typeof( TKey ) );
      propertyList.Add( KeyDescriptor );

      var ValueDescriptor = TypeDescriptor.CreateProperty( this.GetType(), "Value", typeof( TValue ) );
      propertyList.Add( ValueDescriptor );

      _properties = new PropertyDescriptorCollection( propertyList.ToArray() );
    }

    public EditableKeyValuePair( TKey key, TValue value )
      : this()
    {
      this.Key = key;
      this.Value = value;
    }

    #endregion

    #region Overrides

    public override PropertyDescriptorCollection GetProperties( Attribute[] attributes )
    {
      return this.GetProperties();
    }

    public override PropertyDescriptorCollection GetProperties()
    {
      return _properties;
    }

    public override object GetPropertyOwner( PropertyDescriptor pd )
    {
      return this;
    }

    public override string ToString()
    {
      return "[" + this.Key + "," + this.Value + "]";
    }

    #endregion
  }
}
