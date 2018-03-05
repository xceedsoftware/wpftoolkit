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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Utils.Wpf;

namespace Xceed.Wpf.DataGrid.Views
{
  [TemplatePart( Name = "PART_RowSelectorPane", Type = typeof( RowSelectorPane ) )]
  public class TableViewScrollViewer : DataGridScrollViewer
  {
    static TableViewScrollViewer()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( TableViewScrollViewer ), new FrameworkPropertyMetadata( typeof( TableViewScrollViewer ) ) );

      TableViewScrollViewer.HorizontalScrollBarVisibilityHintProperty = TableViewScrollViewer.HorizontalScrollBarVisibilityHintPropertyKey.DependencyProperty;
      TableViewScrollViewer.StoredFixedTransformProperty = TableViewScrollViewer.StoredFixedTransformPropertyKey.DependencyProperty;
    }

    public TableViewScrollViewer()
    {
      ExecutedRoutedEventHandler executedRoutedEventHandler = new ExecutedRoutedEventHandler( this.OnScrollCommand );
      CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = new CanExecuteRoutedEventHandler( this.OnQueryScrollCommand );

      this.CommandBindings.Add(
        new CommandBinding( ScrollBar.PageLeftCommand,
        executedRoutedEventHandler,
        canExecuteRoutedEventHandler ) );

      this.CommandBindings.Add(
        new CommandBinding( ScrollBar.PageRightCommand,
        executedRoutedEventHandler,
        canExecuteRoutedEventHandler ) );
    }

    #region RowSelectorPaneWidth Property

    public static readonly DependencyProperty RowSelectorPaneWidthProperty = DependencyProperty.Register(
      "RowSelectorPaneWidth",
      typeof( double ),
      typeof( TableViewScrollViewer ),
      new FrameworkPropertyMetadata( 20d ) );

    public double RowSelectorPaneWidth
    {
      get
      {
        return ( double )this.GetValue( TableViewScrollViewer.RowSelectorPaneWidthProperty );
      }
      set
      {
        this.SetValue( TableViewScrollViewer.RowSelectorPaneWidthProperty, value );
      }
    }

    #endregion RowSelectorPaneWidth Property

    #region ShowRowSelectorPane Property

    public static readonly DependencyProperty ShowRowSelectorPaneProperty = DependencyProperty.Register(
      "ShowRowSelectorPane",
      typeof( bool ),
      typeof( TableViewScrollViewer ),
      new FrameworkPropertyMetadata( true ) );

    public bool ShowRowSelectorPane
    {
      get
      {
        return ( bool )this.GetValue( TableViewScrollViewer.ShowRowSelectorPaneProperty );
      }
      set
      {
        this.SetValue( TableViewScrollViewer.ShowRowSelectorPaneProperty, value );
      }
    }

    #endregion ShowRowSelectorPane Property

    #region HorizontalScrollBarVisibilityHint Property

    private static readonly DependencyPropertyKey HorizontalScrollBarVisibilityHintPropertyKey = DependencyProperty.RegisterReadOnly(
      "HorizontalScrollBarVisibilityHint",
      typeof( ScrollBarVisibility ),
      typeof( TableViewScrollViewer ),
      new FrameworkPropertyMetadata( ScrollBarVisibility.Auto ) );

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static readonly DependencyProperty HorizontalScrollBarVisibilityHintProperty;

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public ScrollBarVisibility HorizontalScrollBarVisibilityHint
    {
      get
      {
        return ( ScrollBarVisibility )this.GetValue( TableViewScrollViewer.HorizontalScrollBarVisibilityHintProperty );
      }
    }

    private void SetHorizontalScrollBarVisibilityHint( ScrollBarVisibility value )
    {
      this.SetValue( TableViewScrollViewer.HorizontalScrollBarVisibilityHintPropertyKey, value );
    }

    private void ResetHorizontalScrollBarVisibilityHint()
    {
      this.ClearValue( TableViewScrollViewer.HorizontalScrollBarVisibilityHintPropertyKey );
    }

    #endregion HorizontalScrollBarVisibilityHint Property

    #region StoredFixedTransform Attached Property

    internal static readonly DependencyPropertyKey StoredFixedTransformPropertyKey = DependencyProperty.RegisterAttachedReadOnly( "StoredFixedTransform",
      typeof( Transform ),
      typeof( TableViewScrollViewer ),
      new UIPropertyMetadata( null ) );

    internal static readonly DependencyProperty StoredFixedTransformProperty;

    internal static Transform GetStoredFixedTransform( ScrollViewer obj )
    {
      Transform actualValue = ( Transform )obj.GetValue( TableViewScrollViewer.StoredFixedTransformProperty );

      if( actualValue == null )
      {
        actualValue = TableViewScrollViewer.CreateFixedPanelTransform( obj );
        TableViewScrollViewer.SetStoredFixedTransform( obj, actualValue );
      }

      return actualValue;
    }

    internal static void SetStoredFixedTransform( ScrollViewer obj, Transform value )
    {
      obj.SetValue( TableViewScrollViewer.StoredFixedTransformPropertyKey, value );
    }

    #endregion StoredFixedTransform Attached Property

    #region RowSelectorPane Property

    internal RowSelectorPane RowSelectorPane
    {
      get
      {
        return m_rowSelectorPane;
      }
    }

    private RowSelectorPane m_rowSelectorPane;

    #endregion

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      m_rowSelectorPane = this.Template.FindName( "PART_RowSelectorPane", this ) as RowSelectorPane;
    }

    protected override Size MeasureOverride( Size constraint )
    {
      this.UpdateHorizontalScrollBarVisibilityHint( constraint );

      return base.MeasureOverride( constraint );
    }

    internal static void SetFixedTranslateTransform( FrameworkElement element, bool canScrollHorizontally )
    {
      if( element == null )
        throw new ArgumentNullException( "element" );

      if( !TableViewScrollViewer.SetFixedTranslateTransformCore( element, canScrollHorizontally ) )
      {
        if( !element.IsLoaded )
        {
          // The method failed to apply the translate transform because it could not find a ScrollViewer
          // among its ancestors.  Try again when the element will be loaded in case it wasn't in the
          // VisualTree.
          element.Loaded += new RoutedEventHandler( TableViewScrollViewer.OnFixedElementLoaded );
        }
      }
    }

    internal static ScrollViewer GetParentScrollViewer( DependencyObject obj )
    {
      DependencyObject parent = TreeHelper.GetParent( obj );
      ScrollViewer parentScrollViewer = parent as ScrollViewer;

      while( ( parent != null ) && ( parentScrollViewer == null ) )
      {
        parent = TreeHelper.GetParent( parent );
        parentScrollViewer = parent as ScrollViewer;
      }

      return parentScrollViewer;
    }

    internal static TableViewScrollViewer GetParentTableViewScrollViewer( DependencyObject obj )
    {
      DependencyObject parent = TreeHelper.GetParent( obj );
      TableViewScrollViewer parentScrollViewer = parent as TableViewScrollViewer;

      while( ( parent != null ) && ( parentScrollViewer == null ) )
      {
        parent = TreeHelper.GetParent( parent );
        parentScrollViewer = parent as TableViewScrollViewer;
      }

      return parentScrollViewer;
    }

    internal static Transform CreateFixedPanelTransform( ScrollViewer parentScrollViewer )
    {
      TranslateTransform newTransform = new TranslateTransform();

      Binding horizontalOffsetBinding = new Binding();
      horizontalOffsetBinding.Source = parentScrollViewer;
      horizontalOffsetBinding.Path = new PropertyPath( ScrollViewer.HorizontalOffsetProperty );

      BindingOperations.SetBinding( newTransform, TranslateTransform.XProperty, horizontalOffsetBinding );

      return newTransform;
    }

    private static bool SetFixedTranslateTransformCore( FrameworkElement element, bool canScrollHorizontally )
    {
      Debug.Assert( element != null );

      var parentScrollViewer = TableViewScrollViewer.GetParentScrollViewer( element ) as ScrollViewer;
      if( parentScrollViewer == null )
        return false;

      var fixedTransform = TableViewScrollViewer.GetStoredFixedTransform( parentScrollViewer );
      Debug.Assert( fixedTransform != null );

      if( canScrollHorizontally )
      {
        if( element.RenderTransform == fixedTransform )
        {
          element.ClearValue( UIElement.RenderTransformProperty );
        }
      }
      else
      {
        element.RenderTransform = fixedTransform;
      }

      return true;
    }

    private static void OnFixedElementLoaded( object sender, RoutedEventArgs e )
    {
      var element = sender as FrameworkElement;
      if( element == null )
        return;

      element.Loaded -= new RoutedEventHandler( TableViewScrollViewer.OnFixedElementLoaded );

      TableViewScrollViewer.SetFixedTranslateTransformCore( element, TableView.GetCanScrollHorizontally( element ) );
    }

    private void OnQueryScrollCommand( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = true;
    }

    private void OnScrollCommand( object sender, ExecutedRoutedEventArgs e )
    {
      double scrollingViewport = this.ViewportWidth;
      FixedCellPanel cellPanel = this.LocateFixedCellPanel( this );

      if( cellPanel != null )
      {
        scrollingViewport -= cellPanel.GetFixedWidth();
      }

      if( e.Command == ScrollBar.PageLeftCommand )
      {
        double newOffset = Math.Max( this.HorizontalOffset - scrollingViewport, 0d );
        this.ScrollToHorizontalOffset( newOffset );
        e.Handled = true;
      }
      else if( e.Command == ScrollBar.PageRightCommand )
      {
        double newOffset = Math.Min( this.HorizontalOffset + scrollingViewport, this.ScrollableWidth );
        this.ScrollToHorizontalOffset( newOffset );
        e.Handled = true;
      }
      else
      {
        System.Diagnostics.Debug.Assert( false );
      }
    }

    private FixedCellPanel LocateFixedCellPanel( DependencyObject obj )
    {
      if( obj == null )
        return null;

      for( int i = VisualTreeHelper.GetChildrenCount( obj ) - 1; i >= 0; i-- )
      {
        var child = VisualTreeHelper.GetChild( obj, i );
        var foundPanel = child as FixedCellPanel;

        if( foundPanel != null )
          return foundPanel;

        foundPanel = this.LocateFixedCellPanel( child );

        if( foundPanel != null )
        {
          // The FixedCellPanel found can be one of the recycled container in the visual tree
          var dataGridContext = DataGridControl.GetDataGridContext( foundPanel );
          if( dataGridContext != null )
          {
            if( dataGridContext.GetItemFromContainer( foundPanel ) != null )
              return foundPanel;
          }
        }
      }

      return null;
    }

    private void UpdateHorizontalScrollBarVisibilityHint( Size viewport )
    {
      bool isContentLargerThanViewport = false;

      var viewportWidth = viewport.Width;
      if( !double.IsInfinity( viewportWidth ) )
      {
        var dataGridContext = DataGridControl.GetDataGridContext( this );
        if( dataGridContext != null )
        {
          var columnVirtualizationManager = dataGridContext.GetColumnVirtualizationManagerOrNull() as TableViewColumnVirtualizationManagerBase;
          if( columnVirtualizationManager != null )
          {
            isContentLargerThanViewport = ( columnVirtualizationManager.VisibleColumnsWidth > viewportWidth );
          }
        }
      }

      if( isContentLargerThanViewport )
      {
        this.SetHorizontalScrollBarVisibilityHint( ScrollBarVisibility.Visible );
      }
      else
      {
        this.ResetHorizontalScrollBarVisibilityHint();
      }
    }
  }
}
