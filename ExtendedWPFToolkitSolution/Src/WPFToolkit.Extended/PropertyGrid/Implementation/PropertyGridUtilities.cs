/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

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
      foreach( Attribute att in property.Attributes )
      {
        var tAtt = att as T;
        if( tAtt != null )
          return tAtt;
      }
      return null;
    }

    internal static PropertyItemCollection GetAlphabetizedProperties( List<PropertyItem> propertyItems )
    {
      PropertyItemCollection propertyCollection = new PropertyItemCollection( propertyItems );
      propertyCollection.SortBy( "DisplayName", ListSortDirection.Ascending );
      return propertyCollection;
    }

    internal static PropertyItemCollection GetCategorizedProperties( List<PropertyItem> propertyItems )
    {
      PropertyItemCollection propertyCollection = new PropertyItemCollection( propertyItems );
      propertyCollection.GroupBy( "Category" );
      propertyCollection.SortBy( "Category", ListSortDirection.Ascending );
      propertyCollection.SortBy( "PropertyOrder", ListSortDirection.Ascending );
      propertyCollection.SortBy( "DisplayName", ListSortDirection.Ascending );
      return propertyCollection;
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

    internal static PropertyItem CreatePropertyItem( PropertyDescriptor property, object instance, PropertyGrid grid, string bindingPath, int level )
    {
      PropertyItem item = CreatePropertyItem( property, instance, grid, bindingPath );
      item.Level = level;
      return item;
    }

    internal static PropertyItem CreatePropertyItem( PropertyDescriptor property, object instance, PropertyGrid grid, string bindingPath )
    {
      PropertyItem propertyItem = new PropertyItem( instance, property, grid, bindingPath );

      var binding = new Binding( bindingPath )
      {
        Source = instance,
        ValidatesOnExceptions = true,
        ValidatesOnDataErrors = true,
        Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
      };
      propertyItem.SetBinding( PropertyItem.ValueProperty, binding );

      propertyItem.Editor = PropertyGridUtilities.GetTypeEditor( propertyItem, grid.EditorDefinitions );

      return propertyItem;
    }

    internal static FrameworkElement GetTypeEditor( PropertyItem propertyItem, EditorDefinitionCollection editorDefinitions )
    {
      //first check for an attribute editor
      FrameworkElement editor = PropertyGridUtilities.GetAttibuteEditor( propertyItem );

      //now look for a custom editor based on editor definitions
      if( editor == null )
        editor = PropertyGridUtilities.GetCustomEditor( propertyItem, editorDefinitions );

      //guess we have to use the default editor
      if( editor == null )
        editor = PropertyGridUtilities.CreateDefaultEditor( propertyItem );

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
      if( customTypeEditors.Count > 0 )
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
      else if( propertyItem.PropertyType.IsGenericType )
      {
        if( propertyItem.PropertyType.GetInterface( typeof( IList ).FullName ) != null )
        {
          var t = propertyItem.PropertyType.GetGenericArguments()[ 0 ];
          if( !t.IsPrimitive && !t.Equals( typeof( String ) ) )
            editor = new Xceed.Wpf.Toolkit.PropertyGrid.Editors.CollectionEditor();
          else
            editor = new Xceed.Wpf.Toolkit.PropertyGrid.Editors.PrimitiveTypeCollectionEditor();
        }
        else
          editor = new TextBlockEditor();
      }
      else
        editor = new TextBoxEditor();

      return editor.ResolveEditor( propertyItem );
    }
  }
}
