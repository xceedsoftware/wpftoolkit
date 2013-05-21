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
using System.Windows.Controls;
using System.Windows;
using Xceed.Wpf.DataGrid.Views;
using System.Windows.Input;

namespace Xceed.Wpf.DataGrid
{
  public class GroupNavigationButton : Button
  {
    #region CONSTRUCTORS

    static GroupNavigationButton()
    {
      DefaultStyleKeyProperty.OverrideMetadata( 
        typeof( GroupNavigationButton ), 
        new FrameworkPropertyMetadata( new Markup.ThemeKey( typeof( TableView ), typeof( GroupNavigationButton ) ) ) );

      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata(
        typeof( GroupNavigationButton ), new FrameworkPropertyMetadata( new PropertyChangedCallback( GroupNavigationButton.OnParentGridControlChanged ) ) );
    }

    public GroupNavigationButton()
    {
      this.Focusable = false;
    }

    #endregion CONSTRUCTORS

    #region NavigateToGroup Command

    public static readonly RoutedCommand NavigateToGroup =
      new RoutedCommand( "NavigateToGroup", typeof( GroupNavigationButton ) );

    #endregion NavigateToGroup Command

    #region PROTECTED METHODS

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( GroupNavigationButton ) );
    }

    #endregion PROTECTED METHODS

    #region PRIVATE METHODS

    private static void OnParentGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      GroupNavigationButton button = sender as GroupNavigationButton;

      if( button == null )
        return;

      DataGridControl parentGridControl = e.NewValue as DataGridControl;

      if( parentGridControl == null )
        return;

      button.PrepareDefaultStyleKey( parentGridControl.GetView() );
    }

    #endregion PRIVATE METHODS
  }
}
