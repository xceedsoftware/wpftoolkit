/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

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

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  internal class PropertyGridUtilities
  {
    internal static T GetAttribute<T>( PropertyDescriptor property ) where T : Attribute
    {
      return property.Attributes.OfType<T>().FirstOrDefault();
    }


    internal static string GetDefaultPropertyName( object instance )
    {
      AttributeCollection attributes = TypeDescriptor.GetAttributes( instance );
      DefaultPropertyAttribute defaultPropertyAttribute =( DefaultPropertyAttribute )attributes[ typeof( DefaultPropertyAttribute ) ];
      return defaultPropertyAttribute != null ? defaultPropertyAttribute.Name : null;
    }

    internal static PropertyDescriptorCollection GetPropertyDescriptors( object instance )
    {
      PropertyDescriptorCollection descriptors;

      TypeConverter tc = TypeDescriptor.GetConverter( instance );
      if( tc == null || !tc.GetPropertiesSupported() )
      {

        if( instance is ICustomTypeDescriptor )
          descriptors = ( ( ICustomTypeDescriptor )instance ).GetProperties();
        else
          descriptors = TypeDescriptor.GetProperties( instance.GetType() );
      }
      else
      {
        descriptors = tc.GetProperties( instance );
      }

      return descriptors;
    }

    internal static PropertyItem CreatePropertyItem( PropertyDescriptor property, IPropertyParent propertyParent )
    {
      return CreatePropertyItem( property, propertyParent, 0 );
    }

    internal static PropertyItem CreatePropertyItem( PropertyDescriptor property, IPropertyParent propertyParent, int level )
    {
      PropertyItem propertyItem = new PropertyItem( property, propertyParent, level );

      var binding = new Binding( property.Name )
      {
        Source = propertyParent.ValueInstance,
        ValidatesOnExceptions = true,
        ValidatesOnDataErrors = true,
        Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
      };
      propertyItem.SetBinding( PropertyItem.ValueProperty, binding );

      propertyItem.Editor = PropertyGridUtilities.GetTypeEditor( propertyItem, propertyParent.EditorDefinitions );
      return propertyItem;
    }

    internal static FrameworkElement GetTypeEditor( 
      PropertyItem propertyItem, 
      EditorDefinitionCollection editorDefinitions )
    {
      FrameworkElement editor = null;

      //first check for an attribute editor
      if( editor == null )
        editor = PropertyGridUtilities.GetAttibuteEditor( propertyItem );

      //now look for a custom editor based on editor definitions
      if( editor == null )
        editor = PropertyGridUtilities.GetCustomEditor( propertyItem, editorDefinitions );

      //guess we have to use the default editor
      if( editor == null )
        editor = PropertyGridUtilities.CreateDefaultEditor( propertyItem );

      editor.IsEnabled = !propertyItem.PropertyParent.IsReadOnly;
      return editor;
    }

    internal static FrameworkElement GetAttibuteEditor( PropertyItem propertyItem )
    {
      FrameworkElement editor = null;

      var itemsSourceAttribute = GetAttribute<ItemsSourceAttribute>( propertyItem.PropertyDescriptor );
      if( itemsSourceAttribute != null )
        editor = new ItemsSourceAttributeEditor( itemsSourceAttribute ).ResolveEditor( propertyItem );

      var editorAttribute = GetAttribute<EditorAttribute>( propertyItem.PropertyDescriptor );
      if( editorAttribute != null )
      {
        Type type = Type.GetType( editorAttribute.EditorTypeName );
        var instance = Activator.CreateInstance( type );
        if( instance is ITypeEditor )
          editor = ( instance as ITypeEditor ).ResolveEditor( propertyItem );
      }

      return editor;
    }

    internal static FrameworkElement GetCustomEditor( PropertyItem propertyItem, EditorDefinitionCollection customTypeEditors )
    {
      FrameworkElement editor = null;

      //check for custom editor
      if( (customTypeEditors != null) && (customTypeEditors.Count > 0) )
      {
        //first check if the custom editor is type based
        EditorDefinition customEditor = customTypeEditors[ propertyItem.PropertyType ];
        if( customEditor == null )
        {
          //must be property based
          customEditor = customTypeEditors[ propertyItem.Name ];
        }

        if( customEditor != null )
        {
          if( customEditor.EditorTemplate != null )
            editor = customEditor.EditorTemplate.LoadContent() as FrameworkElement;
        }
      }

      return editor;
    }

    internal static FrameworkElement CreateDefaultEditor( PropertyItem propertyItem )
    {
      ITypeEditor editor = null;


      if( propertyItem.IsReadOnly )
        editor = new TextBlockEditor();
      else if( propertyItem.PropertyType == typeof( bool ) || propertyItem.PropertyType == typeof( bool? ) )
        editor = new CheckBoxEditor();
      else if( propertyItem.PropertyType == typeof( decimal ) || propertyItem.PropertyType == typeof( decimal? ) )
        editor = new DecimalUpDownEditor();
      else if( propertyItem.PropertyType == typeof( double ) || propertyItem.PropertyType == typeof( double? ) )
        editor = new DoubleUpDownEditor();
      else if( propertyItem.PropertyType == typeof( int ) || propertyItem.PropertyType == typeof( int? ) )
        editor = new IntegerUpDownEditor();
      else if( propertyItem.PropertyType == typeof( short ) || propertyItem.PropertyType == typeof( short? ) )
        editor = new ShortUpDownEditor();
      else if( propertyItem.PropertyType == typeof( long ) || propertyItem.PropertyType == typeof( long? ) )
        editor = new LongUpDownEditor();
      else if( propertyItem.PropertyType == typeof( float ) || propertyItem.PropertyType == typeof( float? ) )
        editor = new SingleUpDownEditor();
      else if( propertyItem.PropertyType == typeof( byte ) || propertyItem.PropertyType == typeof( byte? ) )
        editor = new ByteUpDownEditor();
      else if( propertyItem.PropertyType == typeof( sbyte ) || propertyItem.PropertyType == typeof( sbyte? ) )
        editor = new UpDownEditor<SByteUpDown,sbyte?>();
      else if( propertyItem.PropertyType == typeof( uint ) || propertyItem.PropertyType == typeof( uint? ) )
        editor = new UpDownEditor<UIntegerUpDown, uint?>();
      else if( propertyItem.PropertyType == typeof( ulong ) || propertyItem.PropertyType == typeof( ulong? ) )
        editor = new UpDownEditor<ULongUpDown, ulong?>();
      else if( propertyItem.PropertyType == typeof( ushort ) || propertyItem.PropertyType == typeof( ushort? ) )
        editor = new UpDownEditor<UShortUpDown, ushort?>();
      else if( propertyItem.PropertyType == typeof( DateTime ) || propertyItem.PropertyType == typeof( DateTime? ) )
        editor = new DateTimeUpDownEditor();
      else if( ( propertyItem.PropertyType == typeof( Color ) ) )
        editor = new ColorEditor();
      else if( propertyItem.PropertyType.IsEnum )
        editor = new EnumComboBoxEditor();
      else if( propertyItem.PropertyType == typeof( TimeSpan ) )
        editor = new TimeSpanEditor();
      else if( propertyItem.PropertyType == typeof( FontFamily ) || propertyItem.PropertyType == typeof( FontWeight ) || propertyItem.PropertyType == typeof( FontStyle ) || propertyItem.PropertyType == typeof( FontStretch ) )
        editor = new FontComboBoxEditor();
      else if( propertyItem.PropertyType == typeof( object ) )
        // If any type of object is possible in the property, default to the TextBoxEditor.
        // Useful in some case (e.g., Button.Content).
        // Can be reconsidered but was the legacy behavior on the PropertyGrid.
        editor = new TextBoxEditor();
      else
      {
        Type listType = CollectionEditor.GetListItemType( propertyItem.PropertyType );

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
          TypeConverter typeConverter = propertyItem.PropertyDescriptor.Converter;
          editor = ( typeConverter != null && typeConverter.CanConvertFrom( typeof( string ) ) )
            ? ( ITypeEditor )new TextBoxEditor()
            : ( ITypeEditor )new TextBlockEditor();
        }
      }

      return editor.ResolveEditor( propertyItem );
    }
  }
}
