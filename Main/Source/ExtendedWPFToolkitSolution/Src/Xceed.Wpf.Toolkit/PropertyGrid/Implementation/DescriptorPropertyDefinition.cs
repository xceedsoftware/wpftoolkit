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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#if !VS2008
using System.ComponentModel.DataAnnotations;
#endif
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Commands;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using System.Reflection;
using System.Globalization;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  internal class DescriptorPropertyDefinition : DescriptorPropertyDefinitionBase
  {
    #region Members

    private readonly object _selectedObject;
    private readonly PropertyDescriptor _propertyDescriptor;
    private readonly DependencyPropertyDescriptor _dpDescriptor;
    private readonly MarkupObject _markupObject;

    #endregion

    #region Constructor

    internal DescriptorPropertyDefinition( PropertyDescriptor propertyDescriptor, object selectedObject, bool isPropertyGridCategorized )
      : base( isPropertyGridCategorized )
    {
      if( propertyDescriptor == null )
        throw new ArgumentNullException( "propertyDescriptor" );

      if( selectedObject == null )
        throw new ArgumentNullException( "selectedObject" );

      _propertyDescriptor = propertyDescriptor;
      _selectedObject = selectedObject;
      _dpDescriptor = DependencyPropertyDescriptor.FromProperty( propertyDescriptor );
      _markupObject = MarkupWriter.GetMarkupObjectFor( SelectedObject );
    }

    #endregion

    #region Custom Properties

    internal override PropertyDescriptor PropertyDescriptor
    {
      get
      {
        return _propertyDescriptor;
      }
    }

    private object SelectedObject
    {
      get
      {
        return _selectedObject;
      }
    }

    #endregion

    #region Override Methods

    internal override ObjectContainerHelperBase CreateContainerHelper( IPropertyContainer parent )
    {
      return new ObjectContainerHelper( parent, this.Value );
    }

    internal override void OnValueChanged( object oldValue, object newValue )
    {
      base.OnValueChanged( oldValue, newValue );
      this.RaiseContainerHelperInvalidated();
    }

    protected override BindingBase CreateValueBinding()
    {
      //Bind the value property with the source object.
      var binding = new Binding( PropertyDescriptor.Name )
      {
        Source = this.GetValueInstance( SelectedObject ),
        Mode = PropertyDescriptor.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
        ValidatesOnDataErrors = true,
        ValidatesOnExceptions = true,
        ConverterCulture = CultureInfo.CurrentCulture 
      };

      return binding;
    }

    protected override bool ComputeIsReadOnly()
    {
      return PropertyDescriptor.IsReadOnly;
    }

    internal override ITypeEditor CreateDefaultEditor( PropertyItem propertyItem )
    {
      return PropertyGridUtilities.CreateDefaultEditor( PropertyDescriptor.PropertyType, PropertyDescriptor.Converter, propertyItem );
    }

    protected override bool ComputeCanResetValue()
    {
      return PropertyDescriptor.CanResetValue( SelectedObject )
        && !PropertyDescriptor.IsReadOnly;
    }

    protected override object ComputeAdvancedOptionsTooltip()
    {
      object tooltip;
      UpdateAdvanceOptionsForItem( _markupObject, SelectedObject as DependencyObject, _dpDescriptor, out tooltip );

      return tooltip;
    }

    protected override string ComputeCategory()
    {
#if VS2008
        return PropertyDescriptor.Category;
#else
      var displayAttribute = PropertyGridUtilities.GetAttribute<DisplayAttribute>( PropertyDescriptor );
      return ( (displayAttribute != null) && (displayAttribute.GetGroupName() != null) ) ? displayAttribute.GetGroupName() : PropertyDescriptor.Category;
#endif
    }

    protected override string ComputeCategoryValue()
    {
      return PropertyDescriptor.Category;
    }

    protected override bool ComputeExpandableAttribute()
    {
      return ( bool )this.ComputeExpandableAttributeForItem( PropertyDescriptor );
    }

    protected override bool ComputeIsExpandable()
    {
      return ( this.Value != null );
    }

    protected override IList<Type> ComputeNewItemTypes()
    {
      return ( IList<Type> )ComputeNewItemTypesForItem( PropertyDescriptor );
    }
    protected override string ComputeDescription()
    {
      return ( string )ComputeDescriptionForItem( PropertyDescriptor );
    }

    protected override int ComputeDisplayOrder( bool isPropertyGridCategorized )
    {
      this.IsPropertyGridCategorized = isPropertyGridCategorized;
      return ( int )ComputeDisplayOrderForItem( PropertyDescriptor );
    }

    protected override void ResetValue()
    {
      PropertyDescriptor.ResetValue( SelectedObject );
    }

    internal override ITypeEditor CreateAttributeEditor()
    {
      var editorAttribute = GetAttribute<EditorAttribute>();
      if( editorAttribute != null )
      {
        Type type = Type.GetType( editorAttribute.EditorTypeName );

        // If the editor does not have any public parameterless constructor, forget it.
        if( typeof( ITypeEditor ).IsAssignableFrom( type )
          && ( type.GetConstructor( new Type[ 0 ] ) != null ) )
        {
          var instance = Activator.CreateInstance( type ) as ITypeEditor;
          Debug.Assert( instance != null, "Type was expected to be ITypeEditor with public constructor." );
          if( instance != null )
            return instance;
        }
      }

      var itemsSourceAttribute = GetAttribute<ItemsSourceAttribute>();
      if( itemsSourceAttribute != null )
        return new ItemsSourceAttributeEditor( itemsSourceAttribute );

      return null;
    }

    #endregion

    #region Private Methods

    private T GetAttribute<T>() where T : Attribute
    {
      return PropertyGridUtilities.GetAttribute<T>( PropertyDescriptor );
    }

    #endregion //Private Methods

  }
}
