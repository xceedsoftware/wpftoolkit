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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

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

    protected override MaskedTextBox CreateEditor()
    {
      return new PropertyGridEditorMaskedTextBox();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      // Do not set Editor properties which could not be overriden in a user style.

      this.Editor.ValueDataType = this.ValueDataType;
      this.Editor.Mask = this.Mask;
    }

    protected override void SetValueDependencyProperty()
    {
      this.ValueProperty = MaskedTextBox.ValueProperty;
    }
  }

  public class PropertyGridEditorMaskedTextBox : MaskedTextBox
  {
    static PropertyGridEditorMaskedTextBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorMaskedTextBox ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorMaskedTextBox ) ) );
    }
  }
}
