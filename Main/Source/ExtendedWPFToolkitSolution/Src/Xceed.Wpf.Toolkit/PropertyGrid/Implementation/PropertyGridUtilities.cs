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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using System.Linq.Expressions;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Controls.Primitives;
using Xceed.Wpf.Toolkit.PropertyGrid.Converters;
using Xceed.Wpf.Toolkit.Primitives;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  internal class PropertyGridUtilities
  {
    #region DefaultNoBorderControlStyle Static Property
    private static Style _noBorderControlStyle;
    internal static Style NoBorderControlStyle
    {
      get
      {
        if( _noBorderControlStyle == null )
        {
          var style = new Style( typeof( Control ) );
          var trigger = new MultiTrigger();
          trigger.Conditions.Add( new Condition( Control.IsKeyboardFocusWithinProperty, false ) );
          trigger.Conditions.Add( new Condition( Control.IsMouseOverProperty, false ) );
          trigger.Setters.Add(
            new Setter( Control.BorderBrushProperty, new SolidColorBrush( Colors.Transparent ) ) );
          style.Triggers.Add( trigger );

          _noBorderControlStyle = style;
        }

        return _noBorderControlStyle;
      }
    }
    #endregion

    #region PropertyGridComboBoxStyle Static Property
    private static Style _propertyGridComboBoxStyle;
    internal static Style ComboBoxStyle
    {
      get
      {
        if( _propertyGridComboBoxStyle == null )
        {
          var style = new Style( typeof( Control ) );
          var trigger = new MultiTrigger();
          trigger.Conditions.Add( new Condition( Control.IsKeyboardFocusWithinProperty, false ) );
          trigger.Conditions.Add( new Condition( Control.IsMouseOverProperty, false ) );
          trigger.Setters.Add( new Setter( Control.BorderBrushProperty, new SolidColorBrush( Colors.Transparent ) ) );
          trigger.Setters.Add( new Setter( Control.BackgroundProperty, new SolidColorBrush( Colors.Transparent ) ) );
          style.Triggers.Add( trigger );

          _propertyGridComboBoxStyle = style;
        }
        return _propertyGridComboBoxStyle;
      }
    }
    #endregion

    #region ColorPickerStyle Static Property
    private static Style _colorPickerStyle;
    internal static Style ColorPickerStyle
    {
      get
      {
        if( _colorPickerStyle == null )
        {
          Style defaultColorPickerStyle = Application.Current.TryFindResource( typeof( ColorPicker ) ) as Style;
          var style = new Style( typeof( ColorPicker ), defaultColorPickerStyle );
          var trigger = new MultiTrigger();
          trigger.Conditions.Add( new Condition( Control.IsKeyboardFocusWithinProperty, false ) );
          trigger.Conditions.Add( new Condition( Control.IsMouseOverProperty, false ) );
          trigger.Setters.Add( new Setter( Control.BorderBrushProperty, new SolidColorBrush( Colors.Transparent ) ) );
          trigger.Setters.Add( new Setter( Control.BorderBrushProperty, new SolidColorBrush( Colors.Transparent ) ) );
          trigger.Setters.Add( new Setter( ColorPicker.ShowDropDownButtonProperty, false ) );
          style.Triggers.Add( trigger );

          _colorPickerStyle = style;
        }

        return _colorPickerStyle;
      }
    }
    #endregion

    #region UpDownBaseStyle Static Getter
    private static Dictionary<Type, Style> _upDownStyles;
    internal static Style GetUpDownBaseStyle<T>()
    {
      if( _upDownStyles == null )
      {
        _upDownStyles = new Dictionary<Type, Style>();
      }

      Style style;
      if( !_upDownStyles.TryGetValue( typeof( T ), out style ) )
      {
        style = new Style( typeof( UpDownBase<T> ) );
        var trigger = new MultiTrigger();
        trigger.Conditions.Add( new Condition( Control.IsKeyboardFocusWithinProperty, false ) );
        trigger.Conditions.Add( new Condition( Control.IsMouseOverProperty, false ) );
        trigger.Setters.Add(
          new Setter( Control.BorderBrushProperty, new SolidColorBrush( Colors.Transparent ) ) );
        trigger.Setters.Add(
          new Setter( UpDownBase<T>.ShowButtonSpinnerProperty, false ) );
        style.Triggers.Add( trigger );

        _upDownStyles.Add( typeof( T ), style );
      }
      return style;
    }
    #endregion

    internal static T GetAttribute<T>( PropertyDescriptor property ) where T : Attribute
    {
      return property.Attributes.OfType<T>().FirstOrDefault();
    }




    internal static ITypeEditor CreateDefaultEditor( Type propertyType, TypeConverter typeConverter )
    {
      ITypeEditor editor = null;

      if( propertyType == typeof( string )  )
        editor = new TextBoxEditor();
      else if( propertyType == typeof( bool ) || propertyType == typeof( bool? ) )
        editor = new CheckBoxEditor();
      else if( propertyType == typeof( decimal ) || propertyType == typeof( decimal? ) )
        editor = new DecimalUpDownEditor();
      else if( propertyType == typeof( double ) || propertyType == typeof( double? ) )
        editor = new DoubleUpDownEditor();
      else if( propertyType == typeof( int ) || propertyType == typeof( int? ) )
        editor = new IntegerUpDownEditor();
      else if( propertyType == typeof( short ) || propertyType == typeof( short? ) )
        editor = new ShortUpDownEditor();
      else if( propertyType == typeof( long ) || propertyType == typeof( long? ) )
        editor = new LongUpDownEditor();
      else if( propertyType == typeof( float ) || propertyType == typeof( float? ) )
        editor = new SingleUpDownEditor();
      else if( propertyType == typeof( byte ) || propertyType == typeof( byte? ) )
        editor = new ByteUpDownEditor();
      else if( propertyType == typeof( sbyte ) || propertyType == typeof( sbyte? ) )
        editor = new UpDownEditor<SByteUpDown,sbyte?>();
      else if( propertyType == typeof( uint ) || propertyType == typeof( uint? ) )
        editor = new UpDownEditor<UIntegerUpDown, uint?>();
      else if( propertyType == typeof( ulong ) || propertyType == typeof( ulong? ) )
        editor = new UpDownEditor<ULongUpDown, ulong?>();
      else if( propertyType == typeof( ushort ) || propertyType == typeof( ushort? ) )
        editor = new UpDownEditor<UShortUpDown, ushort?>();
      else if( propertyType == typeof( DateTime ) || propertyType == typeof( DateTime? ) )
        editor = new DateTimeUpDownEditor();
      else if( ( propertyType == typeof( Color ) ) )
        editor = new ColorEditor();
      else if( propertyType.IsEnum )
        editor = new EnumComboBoxEditor();
      else if( propertyType == typeof( TimeSpan ) )
        editor = new TimeSpanEditor();
      else if( propertyType == typeof( FontFamily ) || propertyType == typeof( FontWeight ) || propertyType == typeof( FontStyle ) || propertyType == typeof( FontStretch ) )
        editor = new FontComboBoxEditor();
      else if( propertyType == typeof( object ) )
        // If any type of object is possible in the property, default to the TextBoxEditor.
        // Useful in some case (e.g., Button.Content).
        // Can be reconsidered but was the legacy behavior on the PropertyGrid.
        editor = new TextBoxEditor();
      else
      {
        Type listType = ListUtilities.GetListItemType( propertyType );

        if( listType != null )
        {
          if( !listType.IsPrimitive && !listType.Equals( typeof( String ) ) )
            editor = new Xceed.Wpf.Toolkit.PropertyGrid.Editors.CollectionEditor();
          else
            editor = new Xceed.Wpf.Toolkit.PropertyGrid.Editors.PrimitiveTypeCollectionEditor();
        }
        else
        {
          // If the type is not supported, check if there is a converter that supports
          // string conversion to the object type. Use TextBox in theses cases.
          // Otherwise, return a TextBlock editor since no valid editor exists.
          editor = ( typeConverter != null && typeConverter.CanConvertFrom( typeof( string ) ) )
            ? ( ITypeEditor )new TextBoxEditor()
            : ( ITypeEditor )new TextBlockEditor();
        }
      }

      return editor;
    }













  }
}
