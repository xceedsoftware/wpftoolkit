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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class MaskedTextBoxEditor : TypeEditor<MaskedTextBox>
  {
    public string Mask
    {
      get;
      set;
    }

    public Type ValueDataType
    {
      get;
      set;
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      Editor.BorderThickness = new System.Windows.Thickness( 0 );
      this.Editor.ValueDataType = this.ValueDataType;
      this.Editor.Mask = this.Mask;
    }

    protected override void SetValueDependencyProperty()
    {
      this.ValueProperty = MaskedTextBox.ValueProperty;
    }
  }
}
