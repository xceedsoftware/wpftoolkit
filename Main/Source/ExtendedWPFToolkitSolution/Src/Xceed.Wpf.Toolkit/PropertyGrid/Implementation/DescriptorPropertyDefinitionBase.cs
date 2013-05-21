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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.PropertyGrid.Commands;
using System.Windows.Media;
using System.Collections;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Markup.Primitives;
using System.Windows.Data;
using System.Windows.Media.Imaging;
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
    private int _displayOrder;
    private bool _isExpandable;
    private bool _isReadOnly;
    private IList<Type> _newItemTypes;
    private IEnumerable<CommandBinding> _commandBindings;

    #endregion

    internal abstract PropertyDescriptor PropertyDescriptor
    {
      get;
    }

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
    protected virtual int ComputeDisplayOrder()
    {
      return int.MaxValue;
    }

    protected virtual bool ComputeIsExpandable()
    {
      return false;
    }

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

    protected virtual AdvancedOptionsValues ComputeAdvancedOptionsValues()
    {
      return new AdvancedOptionsValues()
      {
        ImageSource = null,
        Tooltip = null
      };
    }

    protected virtual void ResetValue()
    {
    }

    protected abstract BindingBase CreateValueBinding();

    #endregion

    #region Internal Methods

    internal virtual ITypeEditor CreateDefaultEditor()
    {
      return null;
    }

    internal virtual ITypeEditor CreateAttributeEditor()
    {
      return null;
    }

    internal void UpdateAdvanceOptionsForItem( MarkupObject markupObject, DependencyObject dependencyObject, DependencyPropertyDescriptor dpDescriptor, 
                                                out string imageName, out object tooltip )
    {
      imageName = "AdvancedProperties11";
      tooltip = "Advanced Properties";

      bool isResource = false;
      bool isDynamicResource = false;

      var markupProperty = markupObject.Properties.Where( p => p.Name == PropertyName ).FirstOrDefault();
      if( markupProperty != null )
      {
        //TODO: need to find a better way to determine if a StaticResource has been applied to any property not just a style
        isResource = ( markupProperty.Value is Style );
        isDynamicResource = ( markupProperty.Value is DynamicResourceExtension );
      }

      if( isResource || isDynamicResource )
      {
        imageName = "Resource11";
        tooltip = "Resource";
      }
      else
      {
        if( ( dependencyObject != null ) && ( dpDescriptor != null ) )
        {
          if( BindingOperations.GetBindingExpressionBase( dependencyObject, dpDescriptor.DependencyProperty ) != null )
          {
            imageName = "Database11";
            tooltip = "Databinding";
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
                imageName = "Inheritance11";
                tooltip = "Inheritance";
                break;
              case BaseValueSource.DefaultStyleTrigger:
                break;
              case BaseValueSource.Style:
                imageName = "Style11";
                tooltip = "Style Setter";
                break;

              case BaseValueSource.Local:
                imageName = "Local11";
                tooltip = "Local";
                break;
            }
          }
        }
      }
    }

    internal AdvancedOptionsValues CreateAdvanceOptionValues( string imageName, object tooltip )
    {
      string uriPrefix = "../Images/";

      AdvancedOptionsValues values = new AdvancedOptionsValues();
      values.Tooltip = tooltip;
      values.ImageSource = new BitmapImage( new Uri( String.Format( "{0}{1}.png", uriPrefix, imageName ), UriKind.Relative ) );

      return values;
    }

    internal void UpdateAdvanceOptions()
    {
      AdvancedOptionsValues advancedOptions = ComputeAdvancedOptionsValues();
      AdvancedOptionsIcon = advancedOptions.ImageSource;
      AdvancedOptionsTooltip = advancedOptions.Tooltip;
    }

    internal void UpdateValueFromSource()
    {
      BindingOperations.GetBindingExpressionBase( this, DescriptorPropertyDefinitionBase.ValueProperty ).UpdateTarget();
    }




    internal object ComputeDescriptionForItem( object item )
    {
      PropertyDescriptor pd = item as PropertyDescriptor;

      //We do not simply rely on the "Description" property of PropertyDescriptor
      //since this value is cached by PropertyDescriptor and the localized version 
      //(e.g., LocalizedDescriptionAttribute) value can dynamicaly change.
      DescriptionAttribute descriptionAtt = PropertyGridUtilities.GetAttribute<DescriptionAttribute>( pd );
      return ( descriptionAtt != null )
              ? descriptionAtt.Description
              : pd.Description;
    }

    internal object ComputeNewItemTypesForItem( object item )
    {
      PropertyDescriptor pd = item as PropertyDescriptor;
      var attribute = PropertyGridUtilities.GetAttribute<NewItemTypesAttribute>( pd );

      return ( attribute != null ) 
              ? attribute.Types 
              : null;
    }

    internal object ComputeDisplayOrderForItem( object item )
    {
      PropertyDescriptor pd = item as PropertyDescriptor;
      var attribute = PropertyGridUtilities.GetAttribute<PropertyOrderAttribute>( pd );

      // Max Value. Properties with no order will be displayed last.
      return ( attribute != null )
              ? attribute.Order
              : int.MaxValue;
    }

    #endregion

    #region Private Methods

    private void ExecuteResetValueCommand( object sender, ExecutedRoutedEventArgs e )
    {
      if( ComputeCanResetValue() )
        ResetValue();
    }

    private void CanExecuteResetValueCommand( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = ComputeCanResetValue();
    }

    private string ComputeDisplayName()
    {
      string displayName = PropertyDescriptor.DisplayName;
      var attribute = PropertyGridUtilities.GetAttribute<ParenthesizePropertyNameAttribute>( PropertyDescriptor );
      if( ( attribute != null ) && attribute.NeedParenthesis )
      {
        displayName = "(" + displayName + ")";
      }

      return displayName;
    }

    #endregion

    #region AdvancedOptionsIcon (DP)

    public static readonly DependencyProperty AdvancedOptionsIconProperty =
        DependencyProperty.Register( "AdvancedOptionsIcon", typeof( ImageSource ), typeof( DescriptorPropertyDefinitionBase ), new UIPropertyMetadata( null ) );

    public ImageSource AdvancedOptionsIcon
    {
      get
      {
        return ( ImageSource )GetValue( AdvancedOptionsIconProperty );
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
        return ( object )GetValue( AdvancedOptionsTooltipProperty );
      }
      set
      {
        SetValue( AdvancedOptionsTooltipProperty, value );
      }
    }

    #endregion //AdvancedOptionsTooltip

    public string Category
    {
      get
      {
        return _category;
      }
    }

    public string CategoryValue
    {
      get
      {
        return _categoryValue;
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
    }
    public string Description
    {
      get
      {
        return _description;
      }
    }

    public int DisplayOrder
    {
      get
      {
        return _displayOrder;
      }
    }

    public bool IsExpandable
    {
      get
      {
        return _isExpandable;
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
      ( ( DescriptorPropertyDefinitionBase )o ).OnValueChanged( e.OldValue, e.NewValue );
    }

    private void OnValueChanged( object oldValue, object newValue )
    {
      UpdateAdvanceOptions();

      // Reset command also affected.
      CommandManager.InvalidateRequerySuggested();
    }

    #endregion //Value Property

    public void InitProperties()
    {
      // Do "IsReadOnly" and PropertyName first since the others may need that value.
      _isReadOnly = ComputeIsReadOnly();
      _category = ComputeCategory();
      _categoryValue = ComputeCategoryValue();
      _description = ComputeDescription();
      _displayName = ComputeDisplayName();
      _displayOrder = ComputeDisplayOrder();
      _isExpandable = ComputeIsExpandable();
      _newItemTypes = ComputeNewItemTypes();
      _commandBindings = new CommandBinding[] { new CommandBinding( PropertyItemCommands.ResetValue, ExecuteResetValueCommand, CanExecuteResetValueCommand ) };

      UpdateAdvanceOptions();

      BindingBase valueBinding = this.CreateValueBinding();
      BindingOperations.SetBinding( this, DescriptorPropertyDefinitionBase.ValueProperty, valueBinding );
    }

    #region AdvancedOptionsValues struct (Internal )

    internal struct AdvancedOptionsValues
    {
      public ImageSource ImageSource;
      public object Tooltip;
    }

    #endregion
  }
}
