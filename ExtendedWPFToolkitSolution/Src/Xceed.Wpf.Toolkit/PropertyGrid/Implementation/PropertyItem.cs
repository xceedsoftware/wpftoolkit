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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using System.Collections;
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Globalization;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  [TemplatePart( Name = "content", Type = typeof( ContentControl ) )]
  public class PropertyItem : CustomPropertyItem
  {
    #region Properties

    #region IsReadOnly

    /// <summary>
    /// Identifies the IsReadOnly dependency property
    /// </summary>
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register( "IsReadOnly", typeof( bool ), typeof( PropertyItem ), new UIPropertyMetadata( false, OnIsReadOnlyChanged ) );

    public bool IsReadOnly
    {
      get { return ( bool )GetValue( IsReadOnlyProperty ); }
      set { SetValue( IsReadOnlyProperty, value ); }
    }

    private static void OnIsReadOnlyChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      var propertyItem = o as PropertyItem;
      if( propertyItem != null )
        propertyItem.OnIsReadOnlyChanged( (bool)e.OldValue, (bool)e.NewValue );
    }

    protected virtual void OnIsReadOnlyChanged( bool oldValue, bool newValue )
    {
      if( this.IsLoaded )
      {
        this.RebuildEditor();
      }
    }


    #endregion //IsReadOnly

    #region IsInValid

    /// <summary>
    /// Identifies the IsInvalid dependency property
    /// </summary>
    public static readonly DependencyProperty IsInvalidProperty =
        DependencyProperty.Register( "IsInvalid", typeof( bool ), typeof( PropertyItem ), new UIPropertyMetadata( false, OnIsInvalidChanged ) );

    public bool IsInvalid
    {
      get
      {
        return ( bool )GetValue( IsInvalidProperty );
      }
      internal set
      {
        SetValue( IsInvalidProperty, value );
      }
    }

    private static void OnIsInvalidChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      var propertyItem = o as PropertyItem;
      if( propertyItem != null )
        propertyItem.OnIsInvalidChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnIsInvalidChanged( bool oldValue, bool newValue )
    {
      var be = this.GetBindingExpression( PropertyItem.ValueProperty );

      if( newValue )
      {
        var validationError = new ValidationError( new InvalidValueValidationRule(), be );
        validationError.ErrorContent = "Value could not be converted.";
        Validation.MarkInvalid( be, validationError );
      }
      else
      {
        Validation.ClearInvalid( be );
      }
    }


    #endregion // IsInvalid

    #region PropertyDescriptor

    public PropertyDescriptor PropertyDescriptor
    {
      get;
      internal set;
    }

    #endregion //PropertyDescriptor

    #region PropertyName

    public string PropertyName
    {
      get
      {
        return (this.DescriptorDefinition != null) ? this.DescriptorDefinition.PropertyName : null;
      }
    }

    #endregion

    #region PropertyType

    public Type PropertyType
    {
      get
      {
        return ( PropertyDescriptor != null )
          ? PropertyDescriptor.PropertyType
          : null;
      }
    }

    #endregion //PropertyType

    #region DescriptorDefinition

    internal DescriptorPropertyDefinitionBase DescriptorDefinition
    {
      get;
      private set;
    }

    #endregion DescriptorDefinition    

    #region Instance

    public object Instance
    {
      get;
      internal set;
    }

    #endregion //Instance

    #endregion //Properties

    #region Overrides

    protected override string GetPropertyItemName()
    {
      return this.PropertyName;
    }

    protected override Type GetPropertyItemType()
    {
      return this.PropertyType;
    }

    protected override void OnIsExpandedChanged( bool oldValue, bool newValue )
    {
      if( newValue && this.IsLoaded )
      {
        this.GenerateExpandedPropertyItems();
      }
    }

    protected override object OnCoerceValueChanged( object baseValue )
    {
      // Propagate error from DescriptorPropertyDefinitionBase to PropertyItem.Value
      // to see the red error rectangle in the propertyGrid.
      BindingExpression be = this.GetBindingExpression( PropertyItem.ValueProperty );
      this.SetRedInvalidBorder( be );
      return baseValue;
    }

    protected override void OnValueChanged( object oldValue, object newValue )
    {
      base.OnValueChanged( oldValue, newValue );

      // A Default Value is defined and newValue is null => set the Default Value
      if( ( newValue == null ) && ( this.DescriptorDefinition != null ) && ( this.DescriptorDefinition.DefaultValue != null ) )
      {
#if VS2008
        this.Value = this.DescriptorDefinition.DefaultValue;
#else
        this.SetCurrentValue( PropertyItem.ValueProperty, this.DescriptorDefinition.DefaultValue );
#endif
      }
    }

    #endregion

    #region Internal Methods

    internal void SetRedInvalidBorder( BindingExpression be )
    {
      if( (be != null) && be.DataItem is DescriptorPropertyDefinitionBase )
      {
        DescriptorPropertyDefinitionBase descriptor = be.DataItem as DescriptorPropertyDefinitionBase;
        if( Validation.GetHasError( descriptor ) )
        {
          ReadOnlyObservableCollection<ValidationError> errors = Validation.GetErrors( descriptor );
          Validation.MarkInvalid( be, errors[ 0 ] );
        }
      }
    }

    internal void RebuildEditor()
    {
      var objectContainerHelperBase = this.ContainerHelper as ObjectContainerHelperBase;
      //Re-build the editor to update this propertyItem
      var editor = objectContainerHelperBase.GenerateChildrenEditorElement( this );
      if( editor != null )
      {
        // Tag the editor as generated to know if we should clear it.
        ContainerHelperBase.SetIsGenerated( editor, true );
        this.Editor = editor;

        //Update Source of binding and Validation of PropertyItem to update
        var be = this.GetBindingExpression( PropertyItem.ValueProperty );
        if( be != null )
        {
          be.UpdateSource();
          this.SetRedInvalidBorder( be );
        }
      }
    }

    #endregion

    #region Private Methods

    private void OnDefinitionContainerHelperInvalidated( object sender, EventArgs e )
    {
      if( this.ContainerHelper != null )
      {
        this.ContainerHelper.ClearHelper();
      }
      var helper = this.DescriptorDefinition.CreateContainerHelper( this );
      this.ContainerHelper = helper;
      if( this.IsExpanded )
      {
        helper.GenerateProperties();
      }
    }

    private void Init( DescriptorPropertyDefinitionBase definition )
    {
      if( definition == null )
        throw new ArgumentNullException( "definition" );

      if( this.ContainerHelper != null )
      {
        this.ContainerHelper.ClearHelper();
      }
      this.DescriptorDefinition = definition;
      this.ContainerHelper = definition.CreateContainerHelper( this );
      definition.ContainerHelperInvalidated += new EventHandler( OnDefinitionContainerHelperInvalidated );
      this.Loaded += this.PropertyItem_Loaded;
    }

    private void GenerateExpandedPropertyItems()
    {
      if( this.IsExpanded )
      {
        // This withholds the generation of all PropertyItem instances (recursively)
        // until the PropertyItem is expanded.
        var objectContainerHelper = ContainerHelper as ObjectContainerHelperBase;
        if( objectContainerHelper != null )
        {
          objectContainerHelper.GenerateProperties();
        }
      }
    }

    #endregion

    #region Event Handlers

    private void PropertyItem_Loaded( object sender, RoutedEventArgs e )
    {
      this.GenerateExpandedPropertyItems();
    }

    #endregion

    #region Constructors

    internal PropertyItem( DescriptorPropertyDefinitionBase definition )
      : base( definition.IsPropertyGridCategorized, !definition.PropertyType.IsArray )
    {
      this.Init( definition );
    }

    #endregion //Constructors

    private class InvalidValueValidationRule : ValidationRule
    {
      public override ValidationResult Validate( object value, CultureInfo cultureInfo )
      {
        // Will always return an error.
        return new ValidationResult( false, null );
      }
    }
  }
}
