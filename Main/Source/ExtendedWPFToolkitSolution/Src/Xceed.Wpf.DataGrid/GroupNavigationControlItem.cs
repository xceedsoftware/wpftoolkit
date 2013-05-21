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

namespace Xceed.Wpf.DataGrid
{
  public class GroupNavigationControlItem : ContentControl
  {
    #region CONSTRUCTORS

    static GroupNavigationControlItem()
    {
      GroupNavigationControlItem.GroupProperty = GroupHeaderControl.GroupProperty.AddOwner( typeof( GroupNavigationControlItem ) );

      DefaultStyleKeyProperty.OverrideMetadata(
        typeof( GroupNavigationControlItem ),
        new FrameworkPropertyMetadata( new Markup.ThemeKey( typeof( TableView ), typeof( GroupNavigationControlItem ) ) ) );

      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata(
        typeof( GroupNavigationControlItem ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( GroupNavigationControlItem.OnParentGridControlChanged ) ) );
    }

    public GroupNavigationControlItem( GroupNavigationControl parentNavigationControl )
    {
      this.ParentNavigationControl = parentNavigationControl;
    }

    #endregion CONSTRUCTORS

    #region ParentGroupNavigationControl Internal Property

    internal GroupNavigationControl ParentNavigationControl
    {
      get;
      private set;
    }

    #endregion ParentGroupNavigationControl Internal Property

    #region Group Property

    public static readonly DependencyProperty GroupProperty;

    public Group Group
    {
      get
      {
        return GroupHeaderControl.GetGroup( this );
      }
    }

    #endregion Group Property

    #region OVERRIDES

    protected override void OnMouseLeftButtonDown( System.Windows.Input.MouseButtonEventArgs e )
    {
      e.Handled = true;

      if( this.ParentNavigationControl != null )
      {
        this.ParentNavigationControl.NotifyGroupNavigationControlItemMouseDown( this );
      }

      base.OnMouseLeftButtonDown( e );
    }

    protected override void OnMouseLeftButtonUp( System.Windows.Input.MouseButtonEventArgs e )
    {
      e.Handled = true;

      if( this.ParentNavigationControl != null )
      {
        this.ParentNavigationControl.NotifyGroupNavigationControlItemMouseUp( this );
      }

      base.OnMouseLeftButtonUp( e );
    }

    #endregion OVERRIDES

    #region EVENT HANDLERS

    private static void OnParentGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      GroupNavigationControlItem groupNavigationControlItem = sender as GroupNavigationControlItem;

      if( groupNavigationControlItem == null )
        return;

      DataGridControl parentGridControl = e.NewValue as DataGridControl;

      if( parentGridControl == null )
        return;

      groupNavigationControlItem.PrepareDefaultStyleKey( parentGridControl.GetView() );
    }

    #endregion EVENT HANDLERS

    #region PROTECTED METHODS

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( GroupNavigationControlItem ) );
    }

    #endregion PROTECTED METHODS
  }
}
