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
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Windows.Controls;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  internal abstract class ObjectContainerHelperBase : ContainerHelperBase
  {
    // This is needed to work around the ItemsControl behavior.
    // When ItemsControl is preparing its containers, it appears 
    // that calling Refresh() on the CollectionView bound to
    // the ItemsSource prevents the items from being displayed.
    // This patch is to avoid such a behavior.
    private bool _isPreparingItemFlag = false;
    private PropertyItemCollection _propertyItemCollection;

    public ObjectContainerHelperBase( IPropertyContainer propertyContainer)
      : base( propertyContainer )
    {
      _propertyItemCollection = new PropertyItemCollection( new ObservableCollection<PropertyItem>() );
      UpdateFilter();
      UpdateCategorization( false );

    }

    public override IList Properties
    {
      get { return _propertyItemCollection; }
    }

    private PropertyItem DefaultProperty
    {
      get 
      {
        PropertyItem defaultProperty = null;
        var defaultName = this.GetDefaultPropertyName();
        if( defaultName != null )
        {
          defaultProperty = _propertyItemCollection
            .FirstOrDefault( ( prop ) => object.Equals( defaultName, prop.PropertyDescriptor.Name ) );
        }

        return defaultProperty;
      }
    }


    protected PropertyItemCollection PropertyItems
    {
      get
      {
        return _propertyItemCollection;
      }
    }

    public override PropertyItemBase ContainerFromItem( object item )
    {
      if( item == null )
        return null;
      // Exception case for ObjectContainerHelperBase. The "Item" may sometimes
      // be identified as a string representing the property name or
      // the PropertyItem itself.
      Debug.Assert( item is PropertyItem || item is string );

      var propertyItem = item as PropertyItem;
      if( propertyItem != null )
        return propertyItem;


      var propertyStr = item as string;
      if( propertyStr != null )
        return PropertyItems.FirstOrDefault( ( prop ) => propertyStr == prop.PropertyDescriptor.Name );

      return null;
    }

    public override object ItemFromContainer( PropertyItemBase container )
    {
      // Since this call is only used to update the PropertyGrid.SelectedProperty property,
      // return the PropertyName.
      var propertyItem = container as PropertyItem;
      if( propertyItem == null )
        return null;

      return propertyItem.PropertyDescriptor.Name;
    }

    public override void  UpdateValuesFromSource()
    {
      foreach( PropertyItem item in PropertyItems )
      {
        item.DescriptorDefinition.UpdateValueFromSource();
        item.ContainerHelper.UpdateValuesFromSource();
      }
    }

    public void GenerateProperties()
    {
      if( (PropertyItems.Count == 0)
        )
      {
        this.RegenerateProperties();
      }
    }

    protected override void OnFilterChanged()
    {
      this.UpdateFilter();
    }

    protected override void OnCategorizationChanged()
    {
      UpdateCategorization( true );
    }

    protected override void OnAutoGeneratePropertiesChanged()
    {
      this.RegenerateProperties();
    }

    protected override void OnHideInheritedPropertiesChanged()
    {
      this.RegenerateProperties();
    }

    protected override void OnEditorDefinitionsChanged()
    {
      this.RegenerateProperties();
    }

    protected override void OnPropertyDefinitionsChanged()
    {
      this.RegenerateProperties();
    }






    protected internal override void SetPropertiesExpansion( bool isExpanded )
    {
      if( this.Properties.Count == 0 )
      {
        this.GenerateProperties();
      }

      base.SetPropertiesExpansion( isExpanded );
    }

    protected internal override void SetPropertiesExpansion( string propertyName, bool isExpanded )
    {
      if( this.Properties.Count == 0 )
      {
        this.GenerateProperties();
      }

      base.SetPropertiesExpansion( propertyName, isExpanded );
    }

    private void UpdateFilter()
    {
      FilterInfo filterInfo = this.PropertyContainer.FilterInfo;

      this.PropertyItems.FilterPredicate = filterInfo.Predicate
        ?? PropertyItemCollection.CreateFilter( filterInfo.InputString, this.PropertyItems, this.PropertyContainer );
    }

    private void UpdateCategorization( bool updateSubPropertiesCategorization )
    {
      _propertyItemCollection.UpdateCategorization( this.ComputeCategoryGroupDescription(), this.PropertyContainer.IsCategorized, this.PropertyContainer.IsSortedAlphabetically );
      if( updateSubPropertiesCategorization && (_propertyItemCollection.Count > 0) )
      {
        foreach( PropertyItem propertyItem in _propertyItemCollection )
        {
          PropertyItemCollection subPropertyItemsCollection = propertyItem.Properties as PropertyItemCollection;
          if( subPropertyItemsCollection != null )
          {
            subPropertyItemsCollection.UpdateCategorization( this.ComputeCategoryGroupDescription(), this.PropertyContainer.IsCategorized, this.PropertyContainer.IsSortedAlphabetically );
          }
        }
      }
    }

    private GroupDescription ComputeCategoryGroupDescription()
    {
      if( !PropertyContainer.IsCategorized )
        return null;
      return new PropertyGroupDescription( PropertyItemCollection.CategoryPropertyName );
    }

    private string GetCategoryGroupingPropertyName()
    {
      var propGroup = this.ComputeCategoryGroupDescription() as PropertyGroupDescription;
      return ( propGroup != null ) ? propGroup.PropertyName : null;
    }

    private void OnChildrenPropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      if( ObjectContainerHelperBase.IsItemOrderingProperty( e.PropertyName )
        || this.GetCategoryGroupingPropertyName() == e.PropertyName )
      {
        // Refreshing the view while Containers are generated will throw an exception
        if( this.ChildrenItemsControl.ItemContainerGenerator.Status != GeneratorStatus.GeneratingContainers
          && !_isPreparingItemFlag )
        {
          PropertyItems.RefreshView();
        }
      }
    }

    protected abstract string GetDefaultPropertyName();

    protected abstract void GenerateSubPropertiesCore( Action<IEnumerable<PropertyItem>> updatePropertyItemsCallback );

    private void RegenerateProperties()
    {
      this.GenerateSubPropertiesCore( this.UpdatePropertyItemsCallback );
    }

    protected internal virtual void UpdatePropertyItemsCallback( IEnumerable<PropertyItem> subProperties )
    {
      foreach( var propertyItem in subProperties )
      {
        this.InitializePropertyItem( propertyItem );
      }

      //Remove the event callback from the previous children (if any)
      foreach( var propertyItem in PropertyItems )
      {
        propertyItem.PropertyChanged -= OnChildrenPropertyChanged;
      }

      PropertyItems.UpdateItems( subProperties );

      //Add the event callback to the new childrens
      foreach( var propertyItem in PropertyItems )
      {
        propertyItem.PropertyChanged += OnChildrenPropertyChanged;
      }

      // Update the selected property on the property grid only.
      PropertyGrid propertyGrid = PropertyContainer as PropertyGrid;
      if( propertyGrid != null )
      {
        propertyGrid.SelectedPropertyItem = this.DefaultProperty;
      }

      if( ObjectsGenerated != null )
      {
        ObjectsGenerated( this, EventArgs.Empty );
      }
    }

    protected static List<PropertyDescriptor> GetPropertyDescriptors( object instance, bool hideInheritedProperties )
    {
      PropertyDescriptorCollection descriptors = null;

      TypeConverter tc = TypeDescriptor.GetConverter( instance );
      if( tc == null || !tc.GetPropertiesSupported() )
      {
        if( instance is ICustomTypeDescriptor )
        {
          descriptors = ((ICustomTypeDescriptor)instance).GetProperties();
        }
        //ICustomTypeProvider is only available in .net 4.5 and over. Use reflection so the .net 4.0 and .net 3.5 still works.
        else if( instance.GetType().GetInterface( "ICustomTypeProvider", true ) != null )
        {
          var methodInfo = instance.GetType().GetMethod( "GetCustomType" );
          var result = methodInfo.Invoke( instance, null ) as Type;
          descriptors = TypeDescriptor.GetProperties( result );
        }
        else
        {
          descriptors = TypeDescriptor.GetProperties( instance.GetType() );
        }
      }
      else
      {
        try
        {
          descriptors = tc.GetProperties( instance );
        }
        catch( Exception )
        {
        }
      }

      if( ( descriptors != null ) )
      {
        var descriptorsProperties = descriptors.Cast<PropertyDescriptor>();
        if( hideInheritedProperties )
        {
          var properties = from p in descriptorsProperties
                           where p.ComponentType == instance.GetType()
                           select p;
          return properties.ToList();
        }
        else
        {
          return descriptorsProperties.ToList();
        }
      }

      return null;
    }




    protected bool GetWillRefreshPropertyGrid( PropertyDescriptor propertyDescriptor )
    {
      if( propertyDescriptor == null )
        return false;

      var attribute = PropertyGridUtilities.GetAttribute<RefreshPropertiesAttribute>( propertyDescriptor );
      if( attribute != null )
        return attribute.RefreshProperties != RefreshProperties.None;

      return false;
    }

    internal void InitializeDescriptorDefinition(
      DescriptorPropertyDefinitionBase descriptorDef,
      PropertyDefinition propertyDefinition )
    {
      if( descriptorDef == null )
        throw new ArgumentNullException( "descriptorDef" );

      if( propertyDefinition == null )
        return;

      // Values defined on PropertyDefinition have priority on the attributes
      if( propertyDefinition != null )
      {
        if( propertyDefinition.Category != null )
        {
          descriptorDef.Category = propertyDefinition.Category;
          descriptorDef.CategoryValue = propertyDefinition.Category;
        }

        if( propertyDefinition.Description != null )
        {
          descriptorDef.Description = propertyDefinition.Description;
        }
        if( propertyDefinition.DisplayName != null )
        {
          descriptorDef.DisplayName = propertyDefinition.DisplayName;
        }

        if( propertyDefinition.DisplayOrder != null )
        {
          descriptorDef.DisplayOrder = propertyDefinition.DisplayOrder.Value;
        }

        if( propertyDefinition.IsExpandable != null )
        {
          descriptorDef.ExpandableAttribute = propertyDefinition.IsExpandable.Value;
        }
      }
    }

    private void InitializePropertyItem( PropertyItem propertyItem )
    {
      DescriptorPropertyDefinitionBase pd = propertyItem.DescriptorDefinition;
      propertyItem.PropertyDescriptor = pd.PropertyDescriptor;

      propertyItem.IsReadOnly = pd.IsReadOnly;
      propertyItem.DisplayName = pd.DisplayName;
      propertyItem.Description = pd.Description;

      propertyItem.Category = pd.Category;
      propertyItem.PropertyOrder = pd.DisplayOrder;

      //These properties can vary with the value. They need to be bound.
      if( pd.PropertyDescriptor.Converter is ExpandableObjectConverter )
      {
        propertyItem.IsExpandable = true;
      }
      else
      {
        SetupDefinitionBinding( propertyItem, PropertyItemBase.IsExpandableProperty, pd, () => pd.IsExpandable, BindingMode.OneWay );
      }
      SetupDefinitionBinding( propertyItem, PropertyItemBase.AdvancedOptionsIconProperty, pd, () => pd.AdvancedOptionsIcon, BindingMode.OneWay );
      SetupDefinitionBinding( propertyItem, PropertyItemBase.AdvancedOptionsTooltipProperty, pd, () => pd.AdvancedOptionsTooltip, BindingMode.OneWay );
      SetupDefinitionBinding( propertyItem, PropertyItem.ValueProperty, pd, () => pd.Value, BindingMode.TwoWay );

      if( pd.CommandBindings != null )
      {
        foreach( CommandBinding commandBinding in pd.CommandBindings )
        {
          propertyItem.CommandBindings.Add( commandBinding );
        }
      }
    }

    private object GetTypeDefaultValue( Type type )
    {
      if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
      {
        type = type.GetProperty( "Value" ).PropertyType;
      }

      return ( type.IsValueType ? Activator.CreateInstance( type ) : null ) ;
    }

    private void SetupDefinitionBinding<T>(
      PropertyItem propertyItem,
      DependencyProperty itemProperty,
      DescriptorPropertyDefinitionBase pd,
      Expression<Func<T>> definitionProperty,
      BindingMode bindingMode )
    {
      string sourceProperty = ReflectionHelper.GetPropertyOrFieldName( definitionProperty );
      Binding binding = new Binding( sourceProperty )
      {
        Source = pd,
        Mode = bindingMode
      };

      propertyItem.SetBinding( itemProperty, binding );
    }

    internal FrameworkElement GenerateChildrenEditorElement( PropertyItem propertyItem )
    {
      FrameworkElement editorElement = null;
      DescriptorPropertyDefinitionBase pd = propertyItem.DescriptorDefinition;
      object definitionKey = null;
      Type definitionKeyAsType = definitionKey as Type;
      ITypeEditor editor = null;

      if( editor == null )
        editor = pd.CreateAttributeEditor();

      if( editor != null )
        editorElement = editor.ResolveEditor( propertyItem );


      if( (editorElement == null) && (definitionKey == null) && ( propertyItem.PropertyDescriptor != null ) )
        editorElement = this.GenerateCustomEditingElement( propertyItem.PropertyDescriptor.Name, propertyItem );

      if( editorElement == null && definitionKeyAsType == null )
        editorElement = this.GenerateCustomEditingElement( propertyItem.PropertyType, propertyItem );

      if( editorElement == null )
      {
        if( propertyItem.IsReadOnly )
          editor = new TextBlockEditor();

        // Fallback: Use a default type editor.
        if( editor == null )
        {
          editor = ( definitionKeyAsType != null )
          ? PropertyGridUtilities.CreateDefaultEditor( definitionKeyAsType, null, propertyItem )
          : pd.CreateDefaultEditor( propertyItem );
        }

        Debug.Assert( editor != null );

        editorElement = editor.ResolveEditor( propertyItem );
      }

      return editorElement;
    }

    internal PropertyDefinition GetPropertyDefinition( PropertyDescriptor descriptor )
    {
      PropertyDefinition def = null;

      var propertyDefs = this.PropertyContainer.PropertyDefinitions;
      if( propertyDefs != null )
      {
        def = propertyDefs[ descriptor.Name ];
        if( def == null )
        {
          def = propertyDefs.GetRecursiveBaseTypes( descriptor.PropertyType );
        }
      }

      return def;
    }


    public override void PrepareChildrenPropertyItem( PropertyItemBase propertyItem, object item )
    {
      _isPreparingItemFlag = true;
      base.PrepareChildrenPropertyItem( propertyItem, item );

      if( propertyItem.Editor == null )
      {
        FrameworkElement editor = this.GenerateChildrenEditorElement( ( PropertyItem )propertyItem );
        if( editor != null )
        {
          // Tag the editor as generated to know if we should clear it.
          ContainerHelperBase.SetIsGenerated( editor, true );
          propertyItem.Editor = editor;
        }
      }
      _isPreparingItemFlag = false;
    }

    public override void ClearChildrenPropertyItem( PropertyItemBase propertyItem, object item )
    {
      if( propertyItem.Editor != null 
        && ContainerHelperBase.GetIsGenerated( propertyItem.Editor ) )
      {
        propertyItem.Editor = null;
      }

      base.ClearChildrenPropertyItem( propertyItem, item );
    }

    public override Binding CreateChildrenDefaultBinding( PropertyItemBase propertyItem )
    {
      Binding binding = new Binding( "Value" );
      binding.Mode = ( ( ( PropertyItem )propertyItem ).IsReadOnly ) ? BindingMode.OneWay : BindingMode.TwoWay;
      return binding;
    }

    protected static string GetDefaultPropertyName( object instance )
    {
      AttributeCollection attributes = TypeDescriptor.GetAttributes( instance );
      DefaultPropertyAttribute defaultPropertyAttribute = ( DefaultPropertyAttribute )attributes[ typeof( DefaultPropertyAttribute ) ];
      return defaultPropertyAttribute != null ? defaultPropertyAttribute.Name : null;
    }

    private static bool IsItemOrderingProperty( string propertyName )
    {
      return string.Equals( propertyName, PropertyItemCollection.DisplayNamePropertyName )
        || string.Equals( propertyName, PropertyItemCollection.CategoryOrderPropertyName )
        || string.Equals( propertyName, PropertyItemCollection.PropertyOrderPropertyName );
    }

    internal event EventHandler ObjectsGenerated;
  }
}
