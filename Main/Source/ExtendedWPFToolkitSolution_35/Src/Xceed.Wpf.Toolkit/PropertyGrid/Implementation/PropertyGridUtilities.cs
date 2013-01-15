/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

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
using System.Linq.Expressions;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Xceed.Wpf.Toolkit.PropertyGrid.Implementation;
using System.Windows.Controls;

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
      DescriptorPropertyDefinition definition = new DescriptorPropertyDefinition( property, propertyParent );
      definition.InitProperties();
      PropertyItem propertyItem = new PropertyItem();
      propertyItem.PropertyDescriptor = property;
      propertyItem.Instance = propertyParent.ValueInstance;
      PropertyGridUtilities.InitializePropertyItem( propertyItem, definition );
      return propertyItem;
    }

    internal static PropertyItem CreatePropertyItem( IPropertyDefinition pd )
    {
      PropertyItem propertyItem = new PropertyItem();

      var descriptorBase = pd as DescriptorPropertyDefinitionBase;
      propertyItem.PropertyDescriptor = ( descriptorBase != null ) ? descriptorBase.GetPropertyDescriptor() : null;
      propertyItem.Instance = ( descriptorBase != null ) ? descriptorBase.PropertyParent.ValueInstance : null;
      PropertyGridUtilities.InitializePropertyItem( propertyItem, pd );
      return propertyItem;
    }

    private static void InitializePropertyItem( PropertyItem propertyItem, IPropertyDefinition pd )
    {
      propertyItem.ChildrenDefinitions = pd.ChildrenDefinitions;


      // Assign a shorter name, for code clarity only.
      PropertyGridUtilities.SetupDefinitionBinding( propertyItem, PropertyItem.DisplayNameProperty, pd, () => pd.DisplayName );
      PropertyGridUtilities.SetupDefinitionBinding( propertyItem, PropertyItem.DescriptionProperty, pd, () => pd.Description );
      PropertyGridUtilities.SetupDefinitionBinding( propertyItem, PropertyItem.CategoryProperty, pd, () => pd.Category );
      PropertyGridUtilities.SetupDefinitionBinding( propertyItem, PropertyItem.PropertyOrderProperty, pd, () => pd.DisplayOrder );
      PropertyGridUtilities.SetupDefinitionBinding( propertyItem, PropertyItem.AdvancedOptionsIconProperty, pd, () => pd.AdvancedOptionsIcon );
      PropertyGridUtilities.SetupDefinitionBinding( propertyItem, PropertyItem.AdvancedOptionsTooltipProperty, pd, () => pd.AdvancedOptionsTooltip );
      PropertyGridUtilities.SetupDefinitionBinding( propertyItem, PropertyItem.IsExpandableProperty, pd, () => pd.IsExpandable );

      Binding valueBinding = new Binding( "Value" )
      {
        Source = pd,
        Mode = BindingMode.TwoWay
      };
      propertyItem.SetBinding( PropertyItem.ValueProperty, valueBinding );

      propertyItem.Editor = pd.GenerateEditorElement( propertyItem );

      if( pd.CommandBindings != null )
      {
        foreach( CommandBinding commandBinding in pd.CommandBindings )
        {
          propertyItem.CommandBindings.Add( commandBinding );
        }
      }
    }

    private static void SetupDefinitionBinding<T>( 
      PropertyItem propertyItem, 
      DependencyProperty itemProperty, 
      IPropertyDefinition pd,
      Expression<Func<T>> definitionProperty )
    {
      string sourceProperty = ReflectionHelper.GetPropertyOrFieldName( definitionProperty );
      Binding binding = new Binding( sourceProperty )
      {
        Source = pd,
        Mode = BindingMode.OneWay
      };

      propertyItem.SetBinding( itemProperty, binding );
    }

    internal static ITypeEditor CreateDefaultEditor( Type propertyType, TypeConverter typeConverter )
    {
      ITypeEditor editor = null;

      if( propertyType == typeof( bool ) || propertyType == typeof( bool? ) )
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
        Type listType = CollectionControl.GetListItemType( propertyType );

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
