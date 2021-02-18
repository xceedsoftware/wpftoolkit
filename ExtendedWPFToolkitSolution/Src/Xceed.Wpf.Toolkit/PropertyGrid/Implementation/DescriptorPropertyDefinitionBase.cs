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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.PropertyGrid.Commands;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Markup.Primitives;
using System.Windows.Data;
#if !VS2008
using System.ComponentModel.DataAnnotations;
#endif
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  internal abstract class DescriptorPropertyDefinitionBase : DependencyObject
  {
    #region Members

    private string _category;
    private string _categoryValue;
    private string _description;
    private string _displayName;
    private object _defaultValue;
    private int _displayOrder;
    private bool _expandableAttribute;
    private bool _isReadOnly;
    private IList<Type> _newItemTypes;
    private IEnumerable<CommandBinding> _commandBindings;

    #endregion

    internal abstract PropertyDescriptor PropertyDescriptor
    {
      get;
    }

    #region Initialization


    internal DescriptorPropertyDefinitionBase( bool isPropertyGridCategorized
                                             )
    {
      this.IsPropertyGridCategorized = isPropertyGridCategorized;
    }

    #endregion

    #region Virtual Methods

    protected virtual string ComputeCategory()
    {
      return null;
    }

    protected virtual string ComputeCategoryValue()
    {
      return null;
    }

    protected virtual string ComputeDescription()
    {
      return null;
    }



    protected virtual int ComputeDisplayOrder( bool isPropertyGridCategorized )
    {
      return int.MaxValue;
    }

    protected virtual bool ComputeExpandableAttribute()
    {
      return false;
    }

    protected virtual object ComputeDefaultValueAttribute()
    {
      return null;
    }

    protected abstract bool ComputeIsExpandable();

    protected virtual IList<Type> ComputeNewItemTypes()
    {
      return null;
    }

    protected virtual bool ComputeIsReadOnly()
    {
      return false;
    }

    protected virtual bool ComputeCanResetValue()
    {
      return false;
    }

    protected virtual object ComputeAdvancedOptionsTooltip()
    {
      return null;
    }

    protected virtual void ResetValue()
    {
      var binding = BindingOperations.GetBindingExpressionBase( this, DescriptorPropertyDefinition.ValueProperty );
      if( binding != null )
      {
        binding.UpdateTarget();
      }
    }

    protected abstract BindingBase CreateValueBinding();

    #endregion

    #region Internal Methods

    internal abstract ObjectContainerHelperBase CreateContainerHelper( IPropertyContainer parent );

    internal void RaiseContainerHelperInvalidated()
    {
      if( this.ContainerHelperInvalidated != null )
      {
        this.ContainerHelperInvalidated( this, EventArgs.Empty );
      }
    }

    internal virtual ITypeEditor CreateDefaultEditor( PropertyItem propertyItem )
    {
      return null;
    }

    internal virtual ITypeEditor CreateAttributeEditor()
    {
      return null;
    }

    internal void UpdateAdvanceOptionsForItem( DependencyObject dependencyObject, DependencyPropertyDescriptor dpDescriptor, out object tooltip )
    {
      tooltip = StringConstants.Default;

      bool isResource = false;
      bool isDynamicResource = false;

      //TODO: need to find a better way to determine if a StaticResource has been applied to any property not just a style(maybe with StaticResourceExtension)
      isResource = typeof( Style ).IsAssignableFrom( this.PropertyType );
      isDynamicResource = typeof( DynamicResourceExtension ).IsAssignableFrom( this.PropertyType );

      if( isResource || isDynamicResource )
      {
        tooltip = StringConstants.Resource;
      }
      else
      {
        if( ( dependencyObject != null ) && ( dpDescriptor != null ) )
        {
          if( BindingOperations.GetBindingExpressionBase( dependencyObject, dpDescriptor.DependencyProperty ) != null )
          {
            tooltip = StringConstants.Databinding;
          }
          else
          {
            BaseValueSource bvs =
              DependencyPropertyHelper
              .GetValueSource( dependencyObject, dpDescriptor.DependencyProperty )
              .BaseValueSource;

            switch( bvs )
            {
              case BaseValueSource.Inherited:
              case BaseValueSource.DefaultStyle:
              case BaseValueSource.ImplicitStyleReference:
                tooltip = StringConstants.Inheritance;
                break;
              case BaseValueSource.DefaultStyleTrigger:
                break;
              case BaseValueSource.Style:
                tooltip = StringConstants.StyleSetter;
                break;

              case BaseValueSource.Local:
                tooltip = StringConstants.Local;
                break;
            }
          }
        }
        else
        {
          // When the Value is diferent from the DefaultValue, use the local icon.
          if( !object.Equals( this.Value, this.DefaultValue ) )
          {
            if( this.DefaultValue != null )
            {
              tooltip = StringConstants.Local;
            }
            else
            {
              if( this.PropertyType.IsValueType )
              {
                var defaultValue = Activator.CreateInstance( this.PropertyType );
                // When the Value is diferent from the DefaultValue, use the local icon.
                if( !object.Equals( this.Value, defaultValue ) )
                {
                  tooltip = StringConstants.Local;
                }
              }
              else
              {
                // When the Value is diferent from null, use the local icon.
                if( this.Value != null )
                {
                  tooltip = StringConstants.Local;
                }
              }
            }
          }
        }
      }
    }

    internal void UpdateAdvanceOptions()
    {
      // Only set the Tooltip. The Icon will be added in XAML based on the Tooltip.
      this.AdvancedOptionsTooltip = this.ComputeAdvancedOptionsTooltip();
    }

    internal void UpdateIsExpandable()
    {
      this.IsExpandable = this.ComputeIsExpandable() 
                          && ( this.ExpandableAttribute
                             );
    }

    internal void UpdateValueFromSource()
    {
      var bindingExpr = BindingOperations.GetBindingExpressionBase( this, DescriptorPropertyDefinitionBase.ValueProperty );
      if( bindingExpr != null )
      {
        bindingExpr.UpdateTarget();
      }
    }





    internal object ComputeDescriptionForItem( object item )
    {
      PropertyDescriptor pd = item as PropertyDescriptor;

      //We do not simply rely on the "Description" property of PropertyDescriptor
      //since this value is cached by PropertyDescriptor and the localized version 
      //(e.g., LocalizedDescriptionAttribute) value can dynamicaly change.
#if !VS2008
      var displayAttribute = PropertyGridUtilities.GetAttribute<DisplayAttribute>( pd );
      if( displayAttribute != null )
      {
        return displayAttribute.GetDescription();
      }
#endif

      var descriptionAtt = PropertyGridUtilities.GetAttribute<DescriptionAttribute>( pd );
      return (descriptionAtt != null)
              ? descriptionAtt.Description
              : pd.Description;
    }

    internal object ComputeNewItemTypesForItem( object item )
    {
      PropertyDescriptor pd = item as PropertyDescriptor;
      var attribute = PropertyGridUtilities.GetAttribute<NewItemTypesAttribute>( pd );

      return (attribute != null)
              ? attribute.Types
              : null;
    }





    internal object ComputeDisplayOrderForItem( object item )
    {
      PropertyDescriptor pd = item as PropertyDescriptor;
#if !VS2008
      var displayAttribute = PropertyGridUtilities.GetAttribute<DisplayAttribute>( PropertyDescriptor );
      if( displayAttribute != null )
      {
        var order = displayAttribute.GetOrder();
        if( order.HasValue )
          return displayAttribute.GetOrder();
      }
#endif

      List<PropertyOrderAttribute> list = pd.Attributes.OfType<PropertyOrderAttribute>().ToList();

      if( list.Count > 0 )
      {
        this.ValidatePropertyOrderAttributes( list );

        if( this.IsPropertyGridCategorized )
        {
          var attribute = list.FirstOrDefault( x => ((x.UsageContext == UsageContextEnum.Categorized)
                                                    || (x.UsageContext == UsageContextEnum.Both)) );
          if( attribute != null )
            return attribute.Order;
        }
        else
        {
          var attribute = list.FirstOrDefault( x => ((x.UsageContext == UsageContextEnum.Alphabetical)
                                                    || (x.UsageContext == UsageContextEnum.Both)) );
          if( attribute != null )
            return attribute.Order;
        }
      }

      // Max Value. Properties with no order will be displayed last.
      return int.MaxValue;
    }

    internal object ComputeExpandableAttributeForItem( object item )
    {
      var pd = (PropertyDescriptor)item;

      var attribute = PropertyGridUtilities.GetAttribute<ExpandableObjectAttribute>( pd );
      return (attribute != null);
    }

    internal int ComputeDisplayOrderInternal( bool isPropertyGridCategorized )
    {
      return this.ComputeDisplayOrder( isPropertyGridCategorized );
    }

    internal object GetValueInstance( object sourceObject )
    {
      ICustomTypeDescriptor customTypeDescriptor = sourceObject as ICustomTypeDescriptor;
      if( customTypeDescriptor != null )
        sourceObject = customTypeDescriptor.GetPropertyOwner( PropertyDescriptor );

      return sourceObject;
    }

    internal object ComputeDefaultValueAttributeForItem( object item )
    {
      var pd = ( PropertyDescriptor )item;

      var defaultValue = PropertyGridUtilities.GetAttribute<DefaultValueAttribute>( pd );
      return ( defaultValue != null ) ? defaultValue.Value : null;
    }

    #endregion

    #region Private Methods

    private static void ExecuteResetValueCommand( object sender, ExecutedRoutedEventArgs e )
    {
      var affectedPropertyItem = e.Parameter as PropertyItem;
      if( affectedPropertyItem == null )
      {
        affectedPropertyItem = sender as PropertyItem;
      }

      if( ( affectedPropertyItem != null ) && ( affectedPropertyItem.DescriptorDefinition != null ) )
      {
        if( affectedPropertyItem.DescriptorDefinition.ComputeCanResetValue() )
        {
          affectedPropertyItem.DescriptorDefinition.ResetValue();
        }
      }
    }

    private static void CanExecuteResetValueCommand( object sender, CanExecuteRoutedEventArgs e )
    {
      var affectedPropertyItem = e.Parameter as PropertyItem;
      if( affectedPropertyItem == null )
      {
        affectedPropertyItem = sender as PropertyItem;
      }

      e.CanExecute = ( (affectedPropertyItem != null) && ( affectedPropertyItem.DescriptorDefinition != null) ) 
                      ? affectedPropertyItem.DescriptorDefinition.ComputeCanResetValue()
                      : false;
    }

    private string ComputeDisplayName()
    {
#if VS2008
        var displayName = PropertyDescriptor.DisplayName;
#else
      var displayAttribute = PropertyGridUtilities.GetAttribute<DisplayAttribute>( PropertyDescriptor );
      var displayName = (displayAttribute != null) ? displayAttribute.GetName() : PropertyDescriptor.DisplayName;
#endif

      var attribute = PropertyGridUtilities.GetAttribute<ParenthesizePropertyNameAttribute>( PropertyDescriptor );
      if( (attribute != null) && attribute.NeedParenthesis )
      {
        displayName = "(" + displayName + ")";
      }

      return displayName;
    }

    private void ValidatePropertyOrderAttributes( List<PropertyOrderAttribute> list )
    {
      if( list.Count > 0 )
      {
        PropertyOrderAttribute both = list.FirstOrDefault( x => x.UsageContext == UsageContextEnum.Both );
        if( (both != null) && (list.Count > 1) )
          Debug.Assert( false, "A PropertyItem can't have more than 1 PropertyOrderAttribute when it has UsageContext : Both" );
      }
    }

    #endregion

    #region Events

    public event EventHandler ContainerHelperInvalidated;

    #endregion

    #region AdvancedOptionsIcon (DP)

    public static readonly DependencyProperty AdvancedOptionsIconProperty =
        DependencyProperty.Register( "AdvancedOptionsIcon", typeof( ImageSource ), typeof( DescriptorPropertyDefinitionBase ), new UIPropertyMetadata( null ) );

    public ImageSource AdvancedOptionsIcon
    {
      get
      {
        return (ImageSource)GetValue( AdvancedOptionsIconProperty );
      }
      set
      {
        SetValue( AdvancedOptionsIconProperty, value );
      }
    }

    #endregion

    #region AdvancedOptionsTooltip (DP)

    public static readonly DependencyProperty AdvancedOptionsTooltipProperty =
        DependencyProperty.Register( "AdvancedOptionsTooltip", typeof( object ), typeof( DescriptorPropertyDefinitionBase ), new UIPropertyMetadata( null ) );

    public object AdvancedOptionsTooltip
    {
      get
      {
        return (object)GetValue( AdvancedOptionsTooltipProperty );
      }
      set
      {
        SetValue( AdvancedOptionsTooltipProperty, value );
      }
    }

    #endregion //AdvancedOptionsTooltip

    #region IsExpandable (DP)

    public static readonly DependencyProperty IsExpandableProperty =
        DependencyProperty.Register( "IsExpandable", typeof( bool ), typeof( DescriptorPropertyDefinitionBase ), new UIPropertyMetadata( false ) );

    public bool IsExpandable
    {
      get
      {
        return (bool)GetValue( IsExpandableProperty );
      }
      set
      {
        SetValue( IsExpandableProperty, value );
      }
    }

    #endregion //IsExpandable

    public string Category
    {
      get
      {
        return _category;
      }
      internal set
      {
        _category = value;
      }
    }

    public string CategoryValue
    {
      get
      {
        return _categoryValue;
      }
      internal set
      {
        _categoryValue = value;
      }
    }

    public IEnumerable<CommandBinding> CommandBindings
    {
      get
      {
        return _commandBindings;
      }
    }

    public string DisplayName
    {
      get
      {
        return _displayName;
      }
      internal set
      {
        _displayName = value;
      }
    }

    public object DefaultValue
    {
      get
      {
        return _defaultValue;
      }
      set
      {
        _defaultValue = value;
      }
    }



    public string Description
    {
      get
      {
        return _description;
      }
      internal set
      {
        _description = value;
      }
    }

    public int DisplayOrder
    {
      get
      {
        return _displayOrder;
      }
      internal set
      {
        _displayOrder = value;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return _isReadOnly;
      }
    }

    public IList<Type> NewItemTypes
    {
      get
      {
        return _newItemTypes;
      }
    }

    public string PropertyName
    {
      get
      {
        // A common property which is present in all selectedObjects will always have the same name.
        return PropertyDescriptor.Name;
      }
    }

    public Type PropertyType
    {
      get
      {
        return PropertyDescriptor.PropertyType;
      }
    }

    internal bool ExpandableAttribute
    {
      get
      {
        return _expandableAttribute;
      }
      set
      {
        _expandableAttribute = value;
        this.UpdateIsExpandable();
      }
    }


    internal bool IsPropertyGridCategorized
    {
      get;
      set;
    }



    #region Value Property (DP)

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof( object ), typeof( DescriptorPropertyDefinitionBase ), new UIPropertyMetadata( null, OnValueChanged ) );
    public object Value
    {
      get
      {
        return GetValue( ValueProperty );
      }
      set
      {
        SetValue( ValueProperty, value );
      }
    }

    private static void OnValueChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ((DescriptorPropertyDefinitionBase)o).OnValueChanged( e.OldValue, e.NewValue );
    }

    internal virtual void OnValueChanged( object oldValue, object newValue )
    {
      UpdateIsExpandable();
      UpdateAdvanceOptions();

      // Reset command also affected.
      CommandManager.InvalidateRequerySuggested();
    }

    #endregion //Value Property

    public virtual void InitProperties()
    {
      // Do "IsReadOnly" and PropertyName first since the others may need that value.
      _isReadOnly = ComputeIsReadOnly();
      _category = ComputeCategory();
      _categoryValue = ComputeCategoryValue();
      _description = ComputeDescription();
      _displayName = ComputeDisplayName();
      _defaultValue = ComputeDefaultValueAttribute();
      _displayOrder = ComputeDisplayOrder( this.IsPropertyGridCategorized );
      _expandableAttribute = ComputeExpandableAttribute();


      _newItemTypes = ComputeNewItemTypes();
      _commandBindings = new CommandBinding[] { new CommandBinding( PropertyItemCommands.ResetValue, ExecuteResetValueCommand, CanExecuteResetValueCommand ) };


      BindingBase valueBinding = this.CreateValueBinding();
      BindingOperations.SetBinding( this, DescriptorPropertyDefinitionBase.ValueProperty, valueBinding );
    }




  }
}
