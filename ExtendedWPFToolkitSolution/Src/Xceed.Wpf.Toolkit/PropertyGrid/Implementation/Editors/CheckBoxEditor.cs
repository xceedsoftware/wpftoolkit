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

using System.Windows;
using System.Windows.Controls;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class CheckBoxEditor : TypeEditor<CheckBox>
  {
    protected override CheckBox CreateEditor()
    {
      return new PropertyGridEditorCheckBox();
    }

    protected override void SetValueDependencyProperty()
    {
      ValueProperty = CheckBox.IsCheckedProperty;
    }
  }

  public class PropertyGridEditorCheckBox : CheckBox
  {
    static PropertyGridEditorCheckBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorCheckBox ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorCheckBox ) ) );
    }
  }
}
