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
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Commands;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using Xceed.Wpf.Toolkit.PropertyGrid.Implementation;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  internal class DescriptorPropertyDefinition : DescriptorPropertyDefinitionBase
  {
    #region Members

    private readonly PropertyDescriptor _propertyDescriptor;
    private readonly DependencyPropertyDescriptor _dpDescriptor;
    private readonly MarkupObject _markupObject;

    #endregion

    #region Constructor

    internal DescriptorPropertyDefinition( PropertyDescriptor propertyDescriptor, IPropertyParent propertyParent )
      : base( propertyParent )
    {
      if( propertyDescriptor == null )
        throw new ArgumentNullException( "propertyDescriptor" );

      _propertyDescriptor = propertyDescriptor;
      _dpDescriptor = DependencyPropertyDescriptor.FromProperty( propertyDescriptor );
      _markupObject = MarkupWriter.GetMarkupObjectFor( _propertyParent.ValueInstance );
    }

    #endregion

    #region Custom Properties

    public PropertyDescriptor PropertyDescriptor
    {
      get
      {
        return _propertyDescriptor;
      }
    }

    #endregion

    #region Override Methods

    protected override void SetBinding()
    {
      //Bind the value property with the source object.
      var binding = new Binding( PropertyDescriptor.Name )
      {
        Source = _propertyParent.ValueInstance,
        Mode = PropertyDescriptor.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
        ValidatesOnDataErrors = true,
        ValidatesOnExceptions = true
      };

      BindingOperations.SetBinding( this, DescriptorPropertyDefinition.ValueProperty, binding );
    }


    protected override string GetPropertyName()
    {
      return PropertyDescriptor.Name;
    }

    protected override Type GetPropertyType()
    {
      return PropertyDescriptor.PropertyType;
    }

    protected override string GetPropertyDisplayName()
    {
      return PropertyDescriptor.DisplayName;
    }

    protected override bool IsPropertyDescriptorReadOnly()
    {
      return PropertyDescriptor.IsReadOnly;
    }

    internal override PropertyDescriptor GetPropertyDescriptor()
    {
      return PropertyDescriptor;
    }

    protected override ITypeEditor CreateDefaultEditor()
    {
      return PropertyGridUtilities.CreateDefaultEditor( PropertyDescriptor.PropertyType, PropertyDescriptor.Converter );
    }

    protected override bool ComputeCanResetValue()
    {
      return PropertyDescriptor.CanResetValue( _propertyParent.ValueInstance )
        && !PropertyDescriptor.IsReadOnly;
    }

    protected override AdvancedOptionsValues ComputeAdvancedOptionsValues()
    {
      string imageName;
      object tooltip;
      UpdateAdvanceOptionsForItem( _markupObject, _propertyParent.ValueInstance as DependencyObject, _dpDescriptor, out imageName, out tooltip );

      return CreateAdvanceOptionValues( imageName, tooltip );
    }

    protected override string ComputeCategory()
    {
      return PropertyDescriptor.Category;
    }

    protected override string ComputeCategoryValue()
    {
      return PropertyDescriptor.Category;
    }

    protected override bool ComputeIsExpandable()
    {
      bool isExpandable = false;
      var attribute = GetAttribute<ExpandableObjectAttribute>();
      if( attribute != null )
      {
        isExpandable = true;
      }

      return isExpandable;
    }

    protected override string ComputeDescription()
    {
      return ( string )ComputeDescriptionForItem( GetPropertyDescriptor() );
    }

    protected override int ComputeDisplayOrder()
    {
      return ( int )ComputeDisplayOrderForItem( GetPropertyDescriptor() );
    }

    protected override IEnumerable<IPropertyDefinition> GenerateChildrenProperties()
    {
      object value = this.ValueInstance;

      if( value == null )
        return new IPropertyDefinition[ 0 ];

      var propertyDefs = new List<IPropertyDefinition>();

      try
      {
        PropertyDescriptorCollection descriptors = PropertyGridUtilities.GetPropertyDescriptors( value );

        foreach( PropertyDescriptor descriptor in descriptors )
        {
          if( descriptor.IsBrowsable )
          {
            DescriptorPropertyDefinition def = new DescriptorPropertyDefinition( descriptor, this );
            def.InitProperties();
            propertyDefs.Add( def );
          }
        }
      }
      catch( Exception )
      {
        //TODO: handle this some how
      }

      return propertyDefs;
    }

    protected override void ResetValue()
    {
      PropertyDescriptor.ResetValue( _propertyParent.ValueInstance );
    }

    protected override ITypeEditor GetAttributeEditor()
    {
      var editorAttribute = GetAttribute<EditorAttribute>();
      if( editorAttribute != null )
      {
        Type type = Type.GetType( editorAttribute.EditorTypeName );
        var instance = Activator.CreateInstance( type );
        if( instance is ITypeEditor )
          return ( ITypeEditor )instance;
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
