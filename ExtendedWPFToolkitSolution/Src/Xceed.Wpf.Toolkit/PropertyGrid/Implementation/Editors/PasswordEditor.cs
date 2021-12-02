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
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class PasswordEditor : TypeEditor<WatermarkPasswordBox>
  {
    protected override WatermarkPasswordBox CreateEditor()
    {
      return new PropertyGridEditorWatermarkPasswordBox();
    }

    protected override void SetValueDependencyProperty()
    {
      ValueProperty = WatermarkPasswordBox.TextProperty;
    }

    protected override IValueConverter CreateValueConverter()
    {
      return new PasswordToStringConverter(this.Editor);
    }
  }

  public class PasswordToStringConverter : IValueConverter
  {
    private WatermarkPasswordBox _editor;

    public PasswordToStringConverter( WatermarkPasswordBox editor )
    {
      _editor = editor;
    }

    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      _editor.Password = value as string;
      return _editor.Text;
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      return this._editor.Password;
    }
  }

  public class PropertyGridEditorWatermarkPasswordBox : WatermarkPasswordBox
  {
    static PropertyGridEditorWatermarkPasswordBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorWatermarkPasswordBox ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorWatermarkPasswordBox ) ) );
    }
  }
}
