/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2023 Xceed Software Inc.

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
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class EnumCheckComboBoxEditor : TypeEditor<CheckComboBox>
  {
    protected override void SetValueDependencyProperty()
    {
      ValueProperty = CheckComboBox.SelectedValueProperty;
    }

    protected override CheckComboBox CreateEditor()
    {
      return new PropertyGridEditorEnumCheckComboBox();
    }

    protected override void ResolveValueBinding( PropertyItem propertyItem )
    {
      SetItemsSource( propertyItem );
      base.ResolveValueBinding( propertyItem );
    }

    private void SetItemsSource( PropertyItem propertyItem )
    {
      Editor.ItemsSource = CreateItemsSource( propertyItem );
    }

    protected IEnumerable CreateItemsSource( PropertyItem propertyItem )
    {
      return GetValues( propertyItem.PropertyType );
    }

    protected override IValueConverter CreateValueConverter()
    {
      return new SourceComboBoxEditorMultiStringConverter();
    }

    private static object[] GetValues( Type enumType )
    {
      List<object> values = new List<object>();

      if( enumType != null )
      {
        var fields = enumType.GetFields().Where( x => x.IsLiteral );
        foreach( FieldInfo field in fields )
        {
          // Get array of BrowsableAttribute attributes
          object[] attrs = field.GetCustomAttributes( typeof( BrowsableAttribute ), false );

          if( attrs.Length == 1 )
          {
            // If attribute exists and its value is false continue to the next field...
            BrowsableAttribute brAttr = ( BrowsableAttribute )attrs[ 0 ];
            if( brAttr.Browsable == false )
            {
              continue;
            }
          }

          values.Add( field.GetValue( enumType ) );
        }
      }

      return values.ToArray();
    }
  }

  public class PropertyGridEditorEnumCheckComboBox : CheckComboBox
  {
    static PropertyGridEditorEnumCheckComboBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorEnumCheckComboBox ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorEnumCheckComboBox ) ) );
    }
  }

  internal class SourceComboBoxEditorMultiStringConverter : IValueConverter
  {
    Type enumType;

    // Enum flags(A | B | C) to string ("A,B,C")
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      enumType = value.GetType();

      var flags = SourceComboBoxEditorMultiStringConverter.GetFlags( value as Enum );

      return string.Join( ",", flags );
    }

    // string("A,B") to Enum flags (A|B)
    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      var stringList = ( value as string ).Split( new[] { ',' } ).ToList();

      // Convert the List containing "A" and "B" to Total Flag Value 3.
      if( !stringList.Any( x => !string.IsNullOrEmpty( x ) ) )
      {
        return Enum.ToObject( enumType, 0 );
      }

      var mkc = stringList.Select( x => Enum.Parse( enumType, x ) )
                          .Aggregate( ( prev, next ) => ( int )prev | ( int )next );

      // Convert Total flag Value(3) to Enum flags(A | B)
      return Enum.ToObject( enumType, mkc );
    }

    private static IEnumerable<Enum> GetFlags( Enum input )
    {
      foreach( Enum value in Enum.GetValues( input.GetType() ) )
      {
        if( input.HasFlag( value ) )
          yield return value;
      }
    }
  }
}
