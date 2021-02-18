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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
#if !VS2008
using System.ComponentModel.DataAnnotations;
#endif
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using System.Globalization;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  internal class DescriptorPropertyDefinition : DescriptorPropertyDefinitionBase
  {
    #region Members

    private object _selectedObject;
    private PropertyDescriptor _propertyDescriptor;
    private DependencyPropertyDescriptor _dpDescriptor;
    private static Dictionary<string, Type> _dictEditorTypeName = new Dictionary<string, Type>();

    #endregion

    #region Constructor

    internal DescriptorPropertyDefinition( PropertyDescriptor propertyDescriptor, object selectedObject, IPropertyContainer propertyContainer )
       : base( propertyContainer.IsCategorized
             )
    {
      this.Init( propertyDescriptor, selectedObject );
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
      var selectedObject = this.SelectedObject;
      var propertyName = this.PropertyDescriptor.Name;

      //Bind the value property with the source object.
      var binding = new Binding( propertyName )
      {
        Source = this.GetValueInstance( selectedObject ),
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
      if( !PropertyDescriptor.IsReadOnly )
      {
        var defaultValue = this.ComputeDefaultValueAttribute();
        if( defaultValue != null)
          return !defaultValue.Equals( this.Value ); // can Reset if different from defaultValue.

        return PropertyDescriptor.CanResetValue( SelectedObject );
      }

      return false;
    }

    protected override object ComputeAdvancedOptionsTooltip()
    {
      object tooltip;
      UpdateAdvanceOptionsForItem( SelectedObject as DependencyObject, _dpDescriptor, out tooltip );

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

    protected override object ComputeDefaultValueAttribute()
    {
      return this.ComputeDefaultValueAttributeForItem( PropertyDescriptor );
    }

    protected override bool ComputeIsExpandable()
    {
      return ( this.Value != null )
        ;
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
      this.PropertyDescriptor.ResetValue( this.SelectedObject );
      base.ResetValue();
    }

    internal override ITypeEditor CreateAttributeEditor()
    {
      var editorAttribute = GetAttribute<EditorAttribute>();
      if( editorAttribute != null )
      {
        Type type = null;
        if( !_dictEditorTypeName.TryGetValue( editorAttribute.EditorTypeName, out type ) )
        {
#if VS2008
        type = Type.GetType( editorAttribute.EditorTypeName );
#else
          try
          {
            var typeDef = editorAttribute.EditorTypeName.Split( new char[] { ',' } );
            if( typeDef.Length >= 2 )
            {
              var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault( a => a.FullName.Contains( typeDef[ 1 ].Trim() ) );
              if( assembly != null )
              {
                type = assembly.GetTypes().FirstOrDefault( t => (t != null) && (t.FullName != null) && t.FullName.Contains( typeDef[ 0 ] ) );
              }
            }
          }
          catch( Exception )
          {
          }
#endif
          if( type == null )
          {
            type = Type.GetType( editorAttribute.EditorTypeName );
          }
          _dictEditorTypeName.Add( editorAttribute.EditorTypeName, type );
        }

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

    private void Init( PropertyDescriptor propertyDescriptor, object selectedObject )
    {
      if( propertyDescriptor == null )
        throw new ArgumentNullException( "propertyDescriptor" );

      if( selectedObject == null )
        throw new ArgumentNullException( "selectedObject" );

      _propertyDescriptor = propertyDescriptor;
      _selectedObject = selectedObject;
      _dpDescriptor = DependencyPropertyDescriptor.FromProperty( propertyDescriptor );
    }

    #endregion //Private Methods

  }
}
