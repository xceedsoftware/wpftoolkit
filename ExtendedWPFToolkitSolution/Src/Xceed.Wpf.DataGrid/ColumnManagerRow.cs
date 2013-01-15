/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System.Windows;
using Xceed.Wpf.DataGrid.Views;
using System.Windows.Automation.Peers;
using Xceed.Wpf.DataGrid.Automation;
using Xceed.Utils.Wpf.DragDrop;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  public class ColumnManagerRow : Row, IDropTarget
  {
    static ColumnManagerRow()
    {
      NavigationBehaviorProperty.OverrideMetadata( typeof( ColumnManagerRow ), new FrameworkPropertyMetadata( NavigationBehavior.None ) );
    }

    public ColumnManagerRow()
    {
      //ensure that all ColumnManagerRows are ReadOnly
      this.ReadOnly = true; //This is safe to perform since there is nowhere a callback installed on the DP... 

      //ensure that all ColumnManagerRows are not navigable
      this.NavigationBehavior = NavigationBehavior.None; //This is safe to perform since there is nowhere a callback installed on the DP... 
    }

    #region AllowColumnReorder Property

    public static readonly DependencyProperty AllowColumnReorderProperty =
        DependencyProperty.Register( "AllowColumnReorder", typeof( bool ), typeof( ColumnManagerRow ), new UIPropertyMetadata( true ) );

    public bool AllowColumnReorder
    {
      get
      {
        return ( bool )this.GetValue( ColumnManagerRow.AllowColumnReorderProperty );
      }
      set
      {
        this.SetValue( ColumnManagerRow.AllowColumnReorderProperty, value );
      }
    }

    #endregion AllowColumnReorder Property

    #region AllowColumnResize Property

    public static readonly DependencyProperty AllowColumnResizeProperty =
        DependencyProperty.Register( "AllowColumnResize", typeof( bool ), typeof( ColumnManagerRow ), new UIPropertyMetadata( true ) );

    public bool AllowColumnResize
    {
      get
      {
        return ( bool )this.GetValue( ColumnManagerRow.AllowColumnResizeProperty );
      }
      set
      {
        this.SetValue( ColumnManagerRow.AllowColumnResizeProperty, value );
      }
    }

    #endregion AllowColumnResize Property

    #region AllowSort Property

    public static readonly DependencyProperty AllowSortProperty =
        DependencyProperty.Register( "AllowSort", typeof( bool ), typeof( ColumnManagerRow ), new UIPropertyMetadata( true ) );

    public bool AllowSort
    {
      get
      {
        return ( bool )this.GetValue( ColumnManagerRow.AllowSortProperty );
      }
      set
      {
        this.SetValue( ColumnManagerRow.AllowSortProperty, value );
      }
    }

    #endregion AllowSort Property

    protected override AutomationPeer OnCreateAutomationPeer()
    {
      return new ColumnManagerRowAutomationPeer( this );
    }

    protected override Cell CreateCell( ColumnBase column )
    {
      return new ColumnManagerCell();
    }

    protected override bool IsValidCellType( Cell cell )
    {
      return ( cell is ColumnManagerCell );
    }

    protected internal override void PrepareDefaultStyleKey( ViewBase view )
    {
      object currentThemeKey = view.GetDefaultStyleKey( typeof( ColumnManagerRow ) );

      if( currentThemeKey.Equals( this.DefaultStyleKey ) == false )
      {
        this.DefaultStyleKey = currentThemeKey;
      }
    }

    protected override void OnPreviewMouseLeftButtonDown( System.Windows.Input.MouseButtonEventArgs e )
    {
      //Do not call the base class implementation to prevent SetCurrent from being called... 
      //This is because we do not want the ColumnManager Row to be selectable through the Mouse
    }

    #region IDropTarget Members

    bool IDropTarget.CanDropElement( UIElement draggedElement )
    {
      bool canDrop = false;

      ColumnManagerCell draggedCell = draggedElement as ColumnManagerCell;

      if( draggedCell == null )
        return false;

      ColumnReorderingDragSourceManager manager =
        draggedCell.DragSourceManager as ColumnReorderingDragSourceManager;

      if( ( manager != null ) && ( manager.IsAnimatedColumnReorderingEnabled ) )
      {
        ColumnManagerCell cell = draggedElement as ColumnManagerCell;

        if( ( cell != null )
            && ( cell.IsBeingDragged ) )
        {
          DataGridContext rowDataGridContext = DataGridControl.GetDataGridContext( this );

          if( ( rowDataGridContext != null )
              && ( rowDataGridContext.Columns.Contains( cell.ParentColumn ) ) )
          {
            canDrop = true;
          }
        }
      }

      return canDrop;
    }

    void IDropTarget.DragEnter( UIElement draggedElement )
    {
    }

    void IDropTarget.DragOver( UIElement draggedElement, Point mousePosition )
    {
    }

    void IDropTarget.DragLeave( UIElement draggedElement )
    {
    }

    void IDropTarget.Drop( UIElement draggedElement )
    {
      ColumnManagerCell draggedCell = draggedElement as ColumnManagerCell;

      if( draggedCell == null )
        return;

      ColumnReorderingDragSourceManager manager =
        draggedCell.DragSourceManager as ColumnReorderingDragSourceManager;

      if( ( manager != null ) && ( manager.IsAnimatedColumnReorderingEnabled ) )
      {
        manager.CommitReordering();
      }
    }

    #endregion
  }
}
