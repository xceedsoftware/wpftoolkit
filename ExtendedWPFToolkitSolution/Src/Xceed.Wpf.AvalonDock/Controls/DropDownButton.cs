/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2023 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class DropDownButton : ToggleButton
  {
    #region Constructors

    public DropDownButton()
    {
      this.Unloaded += new RoutedEventHandler( DropDownButton_Unloaded );
    }

    #endregion

    #region Properties

    #region DropDownContextMenu

    public static readonly DependencyProperty DropDownContextMenuProperty = DependencyProperty.Register( "DropDownContextMenu", typeof( ContextMenu ), typeof( DropDownButton ),
            new FrameworkPropertyMetadata( ( ContextMenu )null, new PropertyChangedCallback( OnDropDownContextMenuChanged ) ) );

    public ContextMenu DropDownContextMenu
    {
      get
      {
        return ( ContextMenu )GetValue( DropDownContextMenuProperty );
      }
      set
      {
        SetValue( DropDownContextMenuProperty, value );
      }
    }

    private static void OnDropDownContextMenuChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DropDownButton )d ).OnDropDownContextMenuChanged( e );
    }

    protected virtual void OnDropDownContextMenuChanged( DependencyPropertyChangedEventArgs e )
    {
      var oldContextMenu = e.OldValue as ContextMenu;
      if( oldContextMenu != null && IsChecked.GetValueOrDefault() )
        oldContextMenu.Closed -= new RoutedEventHandler( OnContextMenuClosed );
    }

    #endregion

    #region DropDownContextMenuDataContext

    public static readonly DependencyProperty DropDownContextMenuDataContextProperty = DependencyProperty.Register( "DropDownContextMenuDataContext", typeof( object ), typeof( DropDownButton ),
            new FrameworkPropertyMetadata( ( object )null ) );

    public object DropDownContextMenuDataContext
    {
      get
      {
        return ( object )GetValue( DropDownContextMenuDataContextProperty );
      }
      set
      {
        SetValue( DropDownContextMenuDataContextProperty, value );
      }
    }

    #endregion

    #endregion

    #region Overrides

    protected override void OnClick()
    {
      if( DropDownContextMenu != null )
      {
        //IsChecked = true;
        DropDownContextMenu.PlacementTarget = this;
        DropDownContextMenu.Placement = PlacementMode.Bottom;
        DropDownContextMenu.DataContext = DropDownContextMenuDataContext;
        DropDownContextMenu.Closed += new RoutedEventHandler( OnContextMenuClosed );
        DropDownContextMenu.IsOpen = true;
      }

      base.OnClick();
    }

    #endregion

    #region Private Methods

    private void OnContextMenuClosed( object sender, RoutedEventArgs e )
    {
      //Debug.Assert(IsChecked.GetValueOrDefault());
      var ctxMenu = sender as ContextMenu;
      ctxMenu.Closed -= new RoutedEventHandler( OnContextMenuClosed );
      IsChecked = false;
    }

    private void DropDownButton_Unloaded( object sender, RoutedEventArgs e )
    {
      // When changing theme, Unloaded event is called, erasing the DropDownContextMenu.
      // Prevent this on theme changes.
      if( this.IsLoaded )
      {
        DropDownContextMenu = null;
      }
    }

    #endregion
  }
}
