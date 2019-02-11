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
using System.Globalization;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Converters
{
  public class PropertyItemEditorConverter : IMultiValueConverter
  {
    public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
    {
      if( ( values == null ) || ( values.Length != 2 ) )
        return null;

      var editor = values[ 0 ];
      var isReadOnly = values[ 1 ] as bool?;

      if( ( editor == null ) || !isReadOnly.HasValue )
        return editor;

      // Get Editor.IsReadOnly
      var editorType = editor.GetType();
      var editorIsReadOnlyPropertyInfo = editorType.GetProperty( "IsReadOnly" );
      if( editorIsReadOnlyPropertyInfo != null )
      {
        // Set Editor.IsReadOnly to PropertyGrid.IsReadOnly.
        editorIsReadOnlyPropertyInfo.SetValue( editor, isReadOnly, null );
      }
      // No Editor.IsReadOnly property, set the Editor.IsEnabled property.
      else
      {
        var editorIsEnabledPropertyInfo = editorType.GetProperty( "IsEnabled" );
        if( editorIsEnabledPropertyInfo != null )
        {
          // Set Editor.IsEnabled to !PropertyGrid.IsReadOnly.
          editorIsEnabledPropertyInfo.SetValue( editor, !isReadOnly, null );
        }
      }

      return editor;
    }

    public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
