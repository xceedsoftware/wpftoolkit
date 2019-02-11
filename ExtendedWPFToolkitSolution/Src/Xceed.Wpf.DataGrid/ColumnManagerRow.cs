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
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Xceed.Utils.Wpf;
using Xceed.Utils.Wpf.DragDrop;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  public class ColumnManagerRow : Row, IDropTarget
  {
    static ColumnManagerRow()
    {
      Row.NavigationBehaviorProperty.OverrideMetadata( typeof( ColumnManagerRow ), new FrameworkPropertyMetadata( NavigationBehavior.None ) );
      FrameworkElement.ContextMenuProperty.OverrideMetadata( typeof( ColumnManagerRow ), new FrameworkPropertyMetadata( null, new CoerceValueCallback( ColumnManagerRow.CoerceContextMenu ) ) );
    }

    public ColumnManagerRow()
    {
      //ensure that all ColumnManagerRows are ReadOnly
      this.ReadOnly = true; //This is safe to perform since there is nowhere a callback installed on the DP... 

      //ensure that all ColumnManagerRows are not navigable
      this.NavigationBehavior = NavigationBehavior.None; //This is safe to perform since there is nowhere a callback installed on the DP...
    }

    #region AllowColumnReorder Property

    public static readonly DependencyProperty AllowColumnReorderProperty = DependencyProperty.Register(
      "AllowColumnReorder",
      typeof( bool ),
      typeof( ColumnManagerRow ),
      new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback( ColumnManagerRow.OnAllowColumnReorderChanged ) ) );

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

    private static void OnAllowColumnReorderChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as ColumnManagerRow;
      if( self == null )
        return;

      var configuration = self.Configuration;
      if( configuration == null )
        return;

      configuration.AllowColumnReorder = ( bool )e.NewValue;
    }

    #endregion

    #region AllowColumnResize Property

    public static readonly DependencyProperty AllowColumnResizeProperty = DependencyProperty.Register(
      "AllowColumnResize",
      typeof( bool ),
      typeof( ColumnManagerRow ),
      new UIPropertyMetadata( true ) );

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

    #endregion

    #region AllowSort Property

    public static readonly DependencyProperty AllowSortProperty = DependencyProperty.Register(
      "AllowSort",
      typeof( bool ),
      typeof( ColumnManagerRow ),
      new UIPropertyMetadata( true ) );

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

    #endregion

    #region IsUnfocusable Property

    internal override bool IsUnfocusable
    {
      get
      {
        return true;
      }
    }

    #endregion

    #region Configuration Internal Property

    internal ColumnManagerRowConfiguration Configuration
    {
      get
      {
        return m_configuration;
      }
      set
      {
        if( value == m_configuration )
          return;

        if( m_configuration != null )
        {
          PropertyChangedEventManager.RemoveListener( m_configuration, this, string.Empty );
        }

        m_configuration = value;

        this.PushAllowColumnReorder();

        if( m_configuration != null )
        {
          PropertyChangedEventManager.AddListener( m_configuration, this, string.Empty );
        }

        this.UpdateAllowColumnReorder();
      }
    }

    private ColumnManagerRowConfiguration m_configuration;

    #endregion


    protected override Cell CreateCell( ColumnBase column )
    {
      return new ColumnManagerCell();
    }

    protected override bool IsValidCellType( Cell cell )
    {
      return ( cell is ColumnManagerCell );
    }

    protected internal override void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      var currentThemeKey = view.GetDefaultStyleKey( typeof( ColumnManagerRow ) );
      if( currentThemeKey.Equals( this.DefaultStyleKey ) )
        return;

      this.DefaultStyleKey = currentThemeKey;
    }

    protected override void PrepareContainer( DataGridContext dataGridContext, object item )
    {
      base.PrepareContainer( dataGridContext, item );

      this.Configuration = dataGridContext.ColumnManagerRowConfiguration;
    }

    protected override void ClearContainer()
    {
      this.Configuration = null;

      base.ClearContainer();
    }

    protected override void OnPreviewMouseLeftButtonDown( System.Windows.Input.MouseButtonEventArgs e )
    {
      //Do not call the base class implementation to prevent SetCurrent from being called... 
      //This is because we do not want the ColumnManager Row to be selectable through the Mouse
    }

    protected override void OnMouseRightButtonUp( MouseButtonEventArgs e )
    {
      if( e.Handled )
        return;

      base.OnMouseRightButtonUp( e );
    }

    private static object CoerceContextMenu( DependencyObject sender, object value )
    {
      if( value == null )
        return value;

      var self = sender as ColumnManagerRow;
      if( ( self == null ) )
        return value;

      return null;
    }

    private void PushAllowColumnReorder()
    {
      var configuration = this.Configuration;
      if( configuration == null )
        return;

      if( DependencyPropertyHelper.GetValueSource( this, ColumnManagerRow.AllowColumnReorderProperty ).BaseValueSource == BaseValueSource.Default )
        return;

      configuration.AllowColumnReorder = this.AllowColumnReorder;
    }

    private void UpdateAllowColumnReorder()
    {
      var configuration = this.Configuration;
      if( configuration == null )
        return;

      this.AllowColumnReorder = configuration.AllowColumnReorder;
    }

    private void OnConfigurationPropertyChanged( PropertyChangedEventArgs e )
    {
      var propertyName = e.PropertyName;

      if( string.IsNullOrEmpty( propertyName ) || propertyName == ColumnManagerRow.AllowColumnReorderProperty.Name )
      {
        this.UpdateAllowColumnReorder();
      }
    }

    #region IDropTarget Members

    bool IDropTarget.CanDropElement( UIElement draggedElement, RelativePoint mousePosition )
    {
      var draggedCell = draggedElement as ColumnManagerCell;
      if( draggedCell == null )
        return false;

      var manager = draggedCell.DragSourceManager as ColumnReorderingDragSourceManager;
      if( ( manager != null ) && manager.IsAnimatedColumnReorderingEnabled && draggedCell.IsBeingDragged )
      {
        var dataGridContext = DataGridControl.GetDataGridContext( this );
        if( ( dataGridContext == null ) || !dataGridContext.Columns.Contains( draggedCell.ParentColumn ) )
          return false;

        // We are not interested by the y-axis.
        var relativePoint = mousePosition.GetPoint( this );
        var xPosition = new RelativePoint( this, new Point( relativePoint.X, this.ActualHeight / 2d ) );

        var targetCell = ( from dropTarget in manager.GetDropTargetsAtPoint( xPosition )
                           let dropCell = dropTarget as ColumnManagerCell
                           where ( dropCell != null )
                           select dropCell ).FirstOrDefault();
        if( ( targetCell == null ) || ( targetCell == draggedCell ) || ( ( IDropTarget )targetCell ).CanDropElement( draggedElement, xPosition ) )
          return true;
      }

      return false;
    }

    void IDropTarget.DragEnter( UIElement draggedElement )
    {
    }

    void IDropTarget.DragOver( UIElement draggedElement, RelativePoint mousePosition )
    {
    }

    void IDropTarget.DragLeave( UIElement draggedElement )
    {
    }

    void IDropTarget.Drop( UIElement draggedElement, RelativePoint mousePosition )
    {
      var draggedCell = draggedElement as ColumnManagerCell;
      if( draggedCell == null )
        return;

      var manager = draggedCell.DragSourceManager as ColumnReorderingDragSourceManager;
      if( ( manager != null ) && ( manager.IsAnimatedColumnReorderingEnabled ) )
      {
        manager.CommitReordering();
      }
    }

    #endregion

    #region IWeakEventListener Members

    protected override bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      var result = base.OnReceiveWeakEvent( managerType, sender, e );

      if( typeof( PropertyChangedEventManager ) == managerType )
      {
        if( sender == m_configuration )
        {
          this.OnConfigurationPropertyChanged( ( PropertyChangedEventArgs )e );
        }
      }
      else
      {
        return result;
      }

      return true;
    }

    #endregion
  }
}
