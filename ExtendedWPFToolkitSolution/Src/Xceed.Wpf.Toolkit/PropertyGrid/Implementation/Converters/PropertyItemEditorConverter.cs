/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2022 Xceed Software Inc.

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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Converters
{
  public class PropertyItemEditorConverter : IMultiValueConverter
  {
    public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
    {
      if( ( values == null ) || ( values.Length != 3 ) )
        return null;

      var editor = values[ 0 ];
      var isPropertyGridReadOnly = values[ 1 ] as bool?;
      var isPropertyItemReadOnly = values[ 2 ] as bool?;

      if( ( editor == null ) || !isPropertyGridReadOnly.HasValue || !isPropertyItemReadOnly.HasValue )
        return editor;

      // Get Editor.IsReadOnly
      var editorType = editor.GetType();
      var editorIsReadOnlyPropertyInfo = editorType.GetProperty( "IsReadOnly" );
      if( editorIsReadOnlyPropertyInfo != null )
      {
        if( !this.IsPropertySetLocally( editor, TextBoxBase.IsReadOnlyProperty ) )
        {
          // Set Editor.IsReadOnly to PropertyGrid.IsReadOnly & propertyItem.IsReadOnly.
          var isReadOnlyValue = isPropertyGridReadOnly.Value
                                ? true
                                : ( editor is PropertyGridEditorCollectionControl ) ? false : isPropertyItemReadOnly.Value;

          editorIsReadOnlyPropertyInfo.SetValue( editor, isReadOnlyValue, null );

        }
      }
      // No Editor.IsReadOnly property, set the Editor.IsEnabled property.
      else
      {
        var editorIsEnabledPropertyInfo = editorType.GetProperty( "IsEnabled" );
        if( editorIsEnabledPropertyInfo != null )
        {
          if( !this.IsPropertySetLocally( editor, UIElement.IsEnabledProperty ) )
          {
            // Set Editor.IsReadOnly to PropertyGrid.IsReadOnly & propertyItem.IsReadOnly.
            var isEnabledValue = isPropertyGridReadOnly.Value
                                ? false
                                : ( editor is PropertyGridEditorCollectionControl ) ? true : !isPropertyItemReadOnly.Value;

            editorIsEnabledPropertyInfo.SetValue( editor, isEnabledValue, null );
          }
        }
      }

      return editor;
    }

    public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }

    private bool IsPropertySetLocally( object editor, DependencyProperty dp )
    {
      if( dp == null )
        return false;

      var editorObject = editor as DependencyObject;
      if( editorObject == null )
        return false;

      var valueSource = DependencyPropertyHelper.GetValueSource( editorObject, dp );

      return ( valueSource.BaseValueSource == BaseValueSource.Local );
    }
  }
}
