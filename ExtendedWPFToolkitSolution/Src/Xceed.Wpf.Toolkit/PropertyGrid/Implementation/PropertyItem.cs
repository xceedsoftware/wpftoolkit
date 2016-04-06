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
    #region Properties

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
    }

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

    private void OnDefinitionContainerHelperInvalidated( object sender, EventArgs e )
    {
      var helper = this.DescriptorDefinition.CreateContainerHelper( this );
      this.ContainerHelper = helper;
      if( this.IsExpanded )
      {
        helper.GenerateProperties();
      }
    }

    #endregion

    #region Constructors

    internal PropertyItem( DescriptorPropertyDefinitionBase definition )
      : base( definition.IsPropertyGridCategorized, !definition.PropertyType.IsArray )
    {
      if( definition == null )
        throw new ArgumentNullException( "definition" );

      this.DescriptorDefinition = definition;
      this.ContainerHelper = definition.CreateContainerHelper( this );
      definition.ContainerHelperInvalidated += new EventHandler( OnDefinitionContainerHelperInvalidated );
    }

    #endregion //Constructors
  }
}
