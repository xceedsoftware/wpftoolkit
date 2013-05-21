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

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  [TemplatePart( Name = "content", Type = typeof( ContentControl ) )]
  public class PropertyItem : CustomPropertyItem
  {
    private int _categoryOrder;

    #region Properties

    #region CategoryOrder

    public int CategoryOrder
    {
      get
      {
        return _categoryOrder;
      }
      internal set
      {
        if( _categoryOrder != value )
        {
          _categoryOrder = value;
          // Notify the parent helper since this property may affect ordering.
          this.RaisePropertyChanged( () => this.CategoryOrder );
        }
      }
    }

    #endregion //CategoryOrder

    #region IsReadOnly

    /// <summary>
    /// Identifies the IsReadOnly dependency property
    /// </summary>
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register( "IsReadOnly", typeof( bool ), typeof( PropertyItem ), new UIPropertyMetadata( false ) );

    public bool IsReadOnly
    {
      get { return ( bool )GetValue( IsReadOnlyProperty ); }
      set { SetValue( IsReadOnlyProperty, value ); }
    }

    #endregion //IsReadOnly

    #region PropertyOrder

    public static readonly DependencyProperty PropertyOrderProperty =
        DependencyProperty.Register( "PropertyOrder", typeof( int ), typeof( PropertyItem ), new UIPropertyMetadata( 0 ) );

    public int PropertyOrder
    {
      get { return ( int )GetValue( PropertyOrderProperty ); }
      set { SetValue( PropertyOrderProperty, value ); }
    }

    #endregion //PropertyOrder

    #region PropertyDescriptor

    public PropertyDescriptor PropertyDescriptor
    {
      get;
      internal set;
    }

    #endregion //PropertyDescriptor

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

    #region Methods

    protected override void OnIsExpandedChanged( bool oldValue, bool newValue )
    {
      if( newValue )
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

    protected override void OnEditorChanged( FrameworkElement oldValue, FrameworkElement newValue )
    {
      if( oldValue != null )
        oldValue.DataContext = null;

      if( newValue != null )
        newValue.DataContext = this;
    }

    protected override object OnCoerceValueChanged( object baseValue )
    {
      // Propagate error from DescriptorPropertyDefinitionBase to PropertyItem.Value
      // to see the red error rectangle in the propertyGrid.
      BindingExpression be = GetBindingExpression( PropertyItem.ValueProperty );
      if( ( be != null ) && be.DataItem is DescriptorPropertyDefinitionBase )
      {
        DescriptorPropertyDefinitionBase descriptor = be.DataItem as DescriptorPropertyDefinitionBase;
        if( Validation.GetHasError( descriptor ) )
        {
          ReadOnlyObservableCollection<ValidationError> errors = Validation.GetErrors( descriptor );
          Validation.MarkInvalid( be, errors[ 0 ] );
        }
      }
      return baseValue;
    }

    protected override void OnValueChanged( object oldValue, object newValue )
    {
      base.OnValueChanged( oldValue, newValue );

      // Update the ObjectContainerHelper this depends on 
      var helper = new ObjectContainerHelper( this, newValue );
      this.ContainerHelper = helper;
      if( this.IsExpanded )
      {
        helper.GenerateProperties();
      }
    }

    #endregion

    #region Constructors

    internal PropertyItem( DescriptorPropertyDefinitionBase definition )
      : base()
    {
      if( definition == null )
        throw new ArgumentNullException( "definition" );

      this.DescriptorDefinition = definition;
    }

    #endregion //Constructors
  }
}
