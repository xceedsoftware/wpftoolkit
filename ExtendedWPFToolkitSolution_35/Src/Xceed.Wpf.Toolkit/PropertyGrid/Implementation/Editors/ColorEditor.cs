/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;
namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class ColorEditor : TypeEditor<ColorPicker>
  {
    protected override ColorPicker CreateEditor()
    {
      return new PropertyGridEditorColorPicker();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      Editor.BorderThickness = new System.Windows.Thickness( 0 );
      Editor.DisplayColorAndName = true;
    }
    protected override void SetValueDependencyProperty()
    {
      ValueProperty = ColorPicker.SelectedColorProperty;
    }
  }

  public class PropertyGridEditorColorPicker : ColorPicker
  {
    static PropertyGridEditorColorPicker()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorColorPicker ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorColorPicker ) ) );
    }
  }
}
