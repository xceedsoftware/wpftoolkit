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

using System.Collections.Generic;
using System.Collections;
using System;
using System.Windows;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  internal abstract class ContainerHelperBase
  {
    protected readonly IPropertyContainer PropertyContainer;

    public ContainerHelperBase(IPropertyContainer propertyContainer)
    {
      if( propertyContainer == null )
        throw new ArgumentNullException( "propertyContainer" );

      PropertyContainer = propertyContainer;

      var propChange = propertyContainer as INotifyPropertyChanged;
      if( propChange != null )
      {
        propChange.PropertyChanged += new PropertyChangedEventHandler( OnPropertyContainerPropertyChanged );
      }
    }

    #region IsGenerated attached property

    internal static readonly DependencyProperty IsGeneratedProperty = DependencyProperty.RegisterAttached(
      "IsGenerated",
      typeof( bool ),
      typeof( ContainerHelperBase ),
      new PropertyMetadata( false ) );

    internal static bool GetIsGenerated( DependencyObject obj )
    {
      return ( bool )obj.GetValue( ContainerHelperBase.IsGeneratedProperty );
    }

    internal static void SetIsGenerated( DependencyObject obj, bool value )
    {
      obj.SetValue( ContainerHelperBase.IsGeneratedProperty, value );
    }

    #endregion IsGenerated attached property

    public abstract IList Properties
    {
      get;
    }

    internal ItemsControl ChildrenItemsControl
    {
      get;
      set;
    }

    internal bool IsCleaning
    {
      get;
      private set;
    }

    public virtual void ClearHelper()
    {
      this.IsCleaning = true;

      var propChange = PropertyContainer as INotifyPropertyChanged;
      if( propChange != null )
      {
        propChange.PropertyChanged -= new PropertyChangedEventHandler( OnPropertyContainerPropertyChanged );
      }

      // Calling RemoveAll() will force the ItemsContol displaying the
      // properties to clear all the current container (i.e., ClearContainerForItem).
      // This will make the call at "ClearChildrenPropertyItem" for every prepared
      // container. Fortunately, the ItemsContainer will not re-prepare the items yet
      // (i.e., probably made on next measure pass), allowing us to set up the new
      // parent helper.
      if( ChildrenItemsControl != null )
      {
        ( ( IItemContainerGenerator )ChildrenItemsControl.ItemContainerGenerator ).RemoveAll();
      }
      this.IsCleaning = false;
    }

    public virtual void PrepareChildrenPropertyItem( PropertyItemBase propertyItem, object item ) 
    {
      // Initialize the parent node
      propertyItem.ParentNode = PropertyContainer;

      PropertyGrid.RaisePreparePropertyItemEvent( ( UIElement )PropertyContainer, propertyItem, item );
    }

    public virtual void ClearChildrenPropertyItem( PropertyItemBase propertyItem, object item )
    {

      propertyItem.ParentNode = null;

      PropertyGrid.RaiseClearPropertyItemEvent( ( UIElement )PropertyContainer, propertyItem, item );
    }

    protected FrameworkElement GenerateCustomEditingElement( Type definitionKey, PropertyItemBase propertyItem )
    {
      return ( PropertyContainer.EditorDefinitions != null )
        ? this.CreateCustomEditor( PropertyContainer.EditorDefinitions.GetRecursiveBaseTypes( definitionKey ), propertyItem )
        : null;
    }

    protected FrameworkElement GenerateCustomEditingElement( object definitionKey, PropertyItemBase propertyItem )
    {
      return ( PropertyContainer.EditorDefinitions != null )
        ? this.CreateCustomEditor( PropertyContainer.EditorDefinitions[ definitionKey ], propertyItem )
        : null;
    }

    protected FrameworkElement CreateCustomEditor( EditorDefinitionBase customEditor, PropertyItemBase propertyItem )
    {
      return ( customEditor != null )
        ? customEditor.GenerateEditingElementInternal( propertyItem )
        : null;
    }

    protected virtual void OnPropertyContainerPropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      var propertyName = e.PropertyName;
      IPropertyContainer ps = null;
      if( propertyName == ReflectionHelper.GetPropertyOrFieldName( () => ps.FilterInfo ) )
      {
        this.OnFilterChanged();
      }
      else if( propertyName == ReflectionHelper.GetPropertyOrFieldName( () => ps.IsCategorized ) )
      {
        this.OnCategorizationChanged();
      }
      else if( propertyName == ReflectionHelper.GetPropertyOrFieldName( () => ps.AutoGenerateProperties ) )
      {
        this.OnAutoGeneratePropertiesChanged();
      }
      else if( propertyName == ReflectionHelper.GetPropertyOrFieldName( () => ps.HideInheritedProperties ) )
      {
        this.OnHideInheritedPropertiesChanged();
      }
      else if(propertyName == ReflectionHelper.GetPropertyOrFieldName( () => ps.EditorDefinitions ))
      {
        this.OnEditorDefinitionsChanged();
      }
      else if(propertyName == ReflectionHelper.GetPropertyOrFieldName( () => ps.PropertyDefinitions ))
      {
        this.OnPropertyDefinitionsChanged();
      }
    }

    protected virtual void OnCategorizationChanged() { }

    protected virtual void OnFilterChanged() { }

    protected virtual void OnAutoGeneratePropertiesChanged() { }

    protected virtual void OnHideInheritedPropertiesChanged() { }

    protected virtual void OnEditorDefinitionsChanged() { }

    protected virtual void OnPropertyDefinitionsChanged() { }


    public virtual void OnEndInit() { }

    public abstract PropertyItemBase ContainerFromItem( object item );

    public abstract object ItemFromContainer( PropertyItemBase container );

    public abstract Binding CreateChildrenDefaultBinding( PropertyItemBase propertyItem );

    public virtual void NotifyEditorDefinitionsCollectionChanged() { }
    public virtual void NotifyPropertyDefinitionsCollectionChanged() { }

    public abstract void UpdateValuesFromSource();

    protected internal virtual void SetPropertiesExpansion( bool isExpanded )
    {
      foreach( var item in this.Properties )
      {
        var propertyItem = item as PropertyItemBase;
        if( (propertyItem != null) && propertyItem.IsExpandable )
        {
          if( propertyItem.ContainerHelper != null )
          {
            propertyItem.ContainerHelper.SetPropertiesExpansion( isExpanded );
          }
          propertyItem.IsExpanded = isExpanded;
        }
      }
    }

    protected internal virtual void SetPropertiesExpansion( string propertyName, bool isExpanded )
    {
      foreach( var item in this.Properties )
      {
        var propertyItem = item as PropertyItemBase;
        if( (propertyItem != null) && propertyItem.IsExpandable )
        {
          if( propertyItem.DisplayName == propertyName )
          {
            propertyItem.IsExpanded = isExpanded;
            break;
          }

          if( propertyItem.ContainerHelper != null )
          {
            propertyItem.ContainerHelper.SetPropertiesExpansion( propertyName, isExpanded );
          }
        }
      }
    }
  }
}
