/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2022 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace Xceed.Wpf.Toolkit.Primitives
{
  [TemplatePart( Name = PART_SelectAllSelectorItem, Type = typeof( SelectAllSelectorItem ) )]
  public class SelectAllSelector : Selector
  {
    private const string PART_SelectAllSelectorItem = "PART_SelectAllSelectorItem";

    #region Members

    private SelectAllSelectorItem _selectAllSelecotrItem;

    #endregion

    #region Properties

    #region AllItemsSelectedContent

    public static readonly DependencyProperty AllItemsSelectedContentProperty = DependencyProperty.Register( "AllItemsSelectedContent", typeof( string ), typeof( SelectAllSelector )
      , new UIPropertyMetadata( "All", OnAllItemsSelectedContentChanged ) );
    public string AllItemsSelectedContent
    {
      get
      {
        return ( string )GetValue( AllItemsSelectedContentProperty );
      }
      set
      {
        SetValue( AllItemsSelectedContentProperty, value );
      }
    }

    private static void OnAllItemsSelectedContentChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      var selectAllSelector = o as SelectAllSelector;
      if( selectAllSelector != null )
        selectAllSelector.OnAllItemsSelectedContentChanged( ( string )e.OldValue, ( string )e.NewValue );
    }

    protected virtual void OnAllItemsSelectedContentChanged( string oldValue, string newValue )
    {
    }

    #endregion // SelectAllText

    #region IsSelectAllActive

    public static readonly DependencyProperty IsSelectAllActiveProperty = DependencyProperty.Register( "IsSelectAllActive", typeof( bool ), typeof( SelectAllSelector ), new UIPropertyMetadata( false, OnIsSelectAllActiveChanged ) );
    public bool IsSelectAllActive
    {
      get
      {
        return ( bool )GetValue( IsSelectAllActiveProperty );
      }
      set
      {
        SetValue( IsSelectAllActiveProperty, value );
      }
    }

    private static void OnIsSelectAllActiveChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      var selector = o as SelectAllSelector;
      if( selector != null )
        selector.OnIsSelectAllActiveChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnIsSelectAllActiveChanged( bool oldValue, bool newValue )
    {
      if( newValue && ( this.Items.Count > 0 ) )
      {
        this.UpdateSelectAllSelectorItem();
      }
    }

    #endregion //IsSelectAllActive

    #region SelectAllContent

    public static readonly DependencyProperty SelectAllContentProperty = DependencyProperty.Register( "SelectAllContent", typeof( object ), typeof( SelectAllSelector ), new UIPropertyMetadata( "Select All" ) );
    public object SelectAllContent
    {
      get
      {
        return ( object )GetValue( SelectAllContentProperty );
      }
      set
      {
        SetValue( SelectAllContentProperty, value );
      }
    }

    #endregion

    #endregion

    #region Overrides

    protected override void OnSelectedItemsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      base.OnSelectedItemsCollectionChanged( sender, e );

      this.UpdateSelectAllSelectorItem();
    }

    protected override void OnItemsChanged( NotifyCollectionChangedEventArgs e )
    {
      base.OnItemsChanged( e );

      this.UpdateSelectAllSelectorItem();
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      _selectAllSelecotrItem = this.GetTemplateChild( PART_SelectAllSelectorItem ) as SelectAllSelectorItem;
    }

    #endregion

    #region Public Methods

    public void SelectAll()
    {
      var currentSelectedItems = new List<object>( this.SelectedItems as IEnumerable<object> );
      var items = this.ItemsCollection.Cast<object>();

      // Have a faster selection when there are more than 200 items.
      this.UpdateSelectedItemsWithoutNotifications( items.ToList() );

      // Raise SelectionChanged for new selected items.
      var newSelectedItems = items.Except( currentSelectedItems );
      foreach( var item in newSelectedItems )
      {
        this.OnItemSelectionChanged( new ItemSelectionChangedEventArgs( Selector.ItemSelectionChangedEvent, this, item, true ) );
      }
    }

    public void UnSelectAll()
    {
      var currentSelectedItems = new List<object>( this.SelectedItems as IEnumerable<object> );

      this.SelectedItems.Clear();

      // Raise SelectionChanged for selected items.
      foreach( var item in currentSelectedItems )
      {
        this.OnItemSelectionChanged( new ItemSelectionChangedEventArgs( Selector.ItemSelectionChangedEvent, this, item, false ) );
      }
    }

    #endregion

    #region Private Methods

    private void UpdateSelectAllSelectorItem()
    {
      if( _selectAllSelecotrItem != null )
      {
        // All items are selected; select the SelectAll option.
        if( this.Items.Count == this.SelectedItems.Count )
        {
          _selectAllSelecotrItem.ModifyCurrentSelection( true );
        }
        // Some items are selected; set the SelectAll option to null.
        else if( this.SelectedItems.Count > 0 )
        {
          _selectAllSelecotrItem.ModifyCurrentSelection( null );
        }
        // No items are selected; unselect the SelectAll option.
        else
        {
          _selectAllSelecotrItem.ModifyCurrentSelection( false );
        }
      }
    }

    #endregion
  }
}
