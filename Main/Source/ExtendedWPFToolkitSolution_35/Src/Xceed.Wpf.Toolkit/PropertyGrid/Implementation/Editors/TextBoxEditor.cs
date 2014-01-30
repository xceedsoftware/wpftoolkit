/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows.Controls;
using System.Windows;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class TextBoxEditor : TypeEditor<WatermarkTextBox>
  {
    protected override WatermarkTextBox CreateEditor()
    {
      return new PropertyGridEditorTextBox();
    }

    protected override void SetValueDependencyProperty()
    {
      ValueProperty = TextBox.TextProperty;
    }
  }

  public class PropertyGridEditorTextBox : WatermarkTextBox
  {
    static PropertyGridEditorTextBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorTextBox ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorTextBox ) ) );
    }
  }
}
