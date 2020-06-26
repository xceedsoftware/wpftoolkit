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
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class SourceComboBoxEditor : ComboBoxEditor
  {
    internal static string ComboBoxNullValue = "Null";

    ICollection _collection;
    TypeConverter _typeConverter;

    public SourceComboBoxEditor( ICollection collection, TypeConverter typeConverter )
    {
      // Add a "Null" input value in the ComboBox when using a NullableConverter.
      _collection = (typeConverter is NullableConverter) 
                    ? collection.Cast<object>().Select( x => x ?? SourceComboBoxEditor.ComboBoxNullValue ).ToArray() 
                    : collection;
      _typeConverter = typeConverter;
    }

    protected override IEnumerable CreateItemsSource( PropertyItem propertyItem )
    {
      return _collection;
    }

    protected override IValueConverter CreateValueConverter()
    {
      if( _typeConverter != null )
      {
        //When using a stringConverter, we need to convert the value
        if( _typeConverter is StringConverter )
          return new SourceComboBoxEditorStringConverter( _typeConverter );
        //When using a NullableConverter, we need to convert the null value
        if( _typeConverter is NullableConverter )
          return new SourceComboBoxEditorNullableConverter();
      }
      return null;
    }
  }

  internal class SourceComboBoxEditorStringConverter : IValueConverter
  {
    private TypeConverter _typeConverter;

    internal SourceComboBoxEditorStringConverter( TypeConverter typeConverter )
    {
      _typeConverter = typeConverter;
    }

    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( _typeConverter != null )
      {
        if( _typeConverter.CanConvertTo( typeof(string) ) )
          return _typeConverter.ConvertTo( value, typeof(string) );
      }
      return value;
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( _typeConverter != null )
      {
        if( _typeConverter.CanConvertFrom( value.GetType() ) )
          return _typeConverter.ConvertFrom( value );
      }
      return value;
    }
  }

  internal class SourceComboBoxEditorNullableConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      return value ?? SourceComboBoxEditor.ComboBoxNullValue;
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      return value.Equals( SourceComboBoxEditor.ComboBoxNullValue ) ? null : value;
    }
  }
}
