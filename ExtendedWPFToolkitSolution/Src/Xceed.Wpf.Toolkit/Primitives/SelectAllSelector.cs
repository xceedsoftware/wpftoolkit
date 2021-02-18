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

using System.Collections.Specialized;
using System.Windows;
using System.Linq;

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
      // Have a faster selection when there are more than 200 items.
      this.UpdateSelectedItemsWithoutNotifications( this.ItemsCollection.Cast<object>().ToList() );
      // Raise SelectionChanged for every items.
      foreach( var item in this.ItemsCollection )
      {
        this.OnItemSelectionChanged( new ItemSelectionChangedEventArgs( Selector.ItemSelectionChangedEvent, this, item, true ) );
      }
    }

    public void UnSelectAll()
    {
      this.SelectedItems.Clear();
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
