/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics;
using System.Windows.Resources;
using System.Windows.Input;

namespace Xceed.Wpf.DataGrid.Views
{
  public class TableView : UIViewBase
  {
    #region Static Fields

    internal static Cursor DefaultRowSelectorResizeNorthSouthCursor = Cursors.SizeNS;
    internal static Cursor DefaultRowSelectorResizeWestEastCursor = Cursors.SizeWE;

    #endregion

    #region Contructors

    static TableView()
    {
      FrameworkContentElement.DefaultStyleKeyProperty.OverrideMetadata(
        typeof( TableView ),
        new FrameworkPropertyMetadata( TableView.GetDefaultStyleKey( typeof( TableView ), null ) ) );

      TableView.CompensationOffsetProperty = TableView.CompensationOffsetPropertyKey.DependencyProperty;

      TableView.DefaultColumnManagerRowTemplate = new DataTemplate();
      TableView.DefaultColumnManagerRowTemplate.VisualTree = new FrameworkElementFactory( typeof( ColumnManagerRow ) );
      TableView.DefaultColumnManagerRowTemplate.Seal();

      TableView.DefaultGroupByControlTemplate = new DataTemplate();
      FrameworkElementFactory groupByControl = new FrameworkElementFactory( typeof( HierarchicalGroupByControl ) );
      groupByControl.SetValue( TableView.CanScrollHorizontallyProperty, false );
      TableView.DefaultGroupByControlTemplate.VisualTree = groupByControl;
      TableView.DefaultGroupByControlTemplate.Seal();
    }

    public TableView()
    {
    }

    #endregion

    #region AutoFillLastPage Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty AutoFillLastPageProperty =
      DependencyProperty.RegisterAttached( "AutoFillLastPage",
      typeof( bool ),
      typeof( TableView ),
      new PropertyMetadata( true ) );

    [EditorBrowsable( EditorBrowsableState.Never )]
    public bool AutoFillLastPage
    {
      get
      {
        return ( bool )this.GetValue( TableView.AutoFillLastPageProperty );
      }
      set
      {
        this.SetValue( TableView.AutoFillLastPageProperty, value );
      }
    }

    [EditorBrowsable( EditorBrowsableState.Never )]
    public static bool GetAutoFillLastPage( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableView.AutoFillLastPageProperty );
    }

    [EditorBrowsable( EditorBrowsableState.Never )]
    public static void SetAutoFillLastPage( DependencyObject obj, bool value )
    {
      obj.SetValue( TableView.AutoFillLastPageProperty, value );
    }

    #endregion AutoFillLastPage Property

    #region CanScrollHorizontally Attached Property

    public static readonly DependencyProperty CanScrollHorizontallyProperty =
        DependencyProperty.RegisterAttached( "CanScrollHorizontally", typeof( bool ), typeof( TableView ), new UIPropertyMetadata( true, new PropertyChangedCallback( TableView.OnCanScrollHorizontallyChanged ) ) );

    public static bool GetCanScrollHorizontally( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableView.CanScrollHorizontallyProperty );
    }

    public static void SetCanScrollHorizontally( DependencyObject obj, bool value )
    {
      obj.SetValue( TableView.CanScrollHorizontallyProperty, value );
    }

    private static void OnCanScrollHorizontallyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      // This property makes sense (only works) for potentially visible element; UIElement.
      UIElement element = sender as UIElement;

      if( element != null )
      {
        TableViewScrollViewer.SetFixedTranslateTransform( element, ( bool )e.NewValue );
      }
    }

    #endregion CanScrollHorizontally Attached Property

    #region CompensationOffset Attached Property

    internal static readonly DependencyPropertyKey CompensationOffsetPropertyKey = DependencyProperty.RegisterAttachedReadOnly( 
      "CompensationOffset", typeof( double ), typeof( TableView ), 
      new FrameworkPropertyMetadata( 0d, FrameworkPropertyMetadataOptions.Inherits ) );

    internal static readonly DependencyProperty CompensationOffsetProperty;

    internal static double GetCompensationOffset( DependencyObject obj )
    {
      return ( double )obj.GetValue( TableView.CompensationOffsetProperty );
    }

    internal static void SetCompensationOffset( DependencyObject obj, double value )
    {
      obj.SetValue( TableView.CompensationOffsetPropertyKey, value );
    }

    #endregion CompensationOffset Attached Property

    // View properties

    #region ColumnStretchMinWidth Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty ColumnStretchMinWidthProperty =
      DependencyProperty.RegisterAttached( "ColumnStretchMinWidth", typeof( double ), typeof( TableView ),
      new UIPropertyMetadata( 50d ),
      new ValidateValueCallback( TableView.ValidateColumnStretchMinWidthCallback ) );

    public double ColumnStretchMinWidth
    {
      get
      {
        return ( double )this.GetValue( TableView.ColumnStretchMinWidthProperty );
      }
      set
      {
        this.SetValue( TableView.ColumnStretchMinWidthProperty, value );
      }
    }

    public static double GetColumnStretchMinWidth( DependencyObject obj )
    {
      return ( double )obj.GetValue( TableView.ColumnStretchMinWidthProperty );
    }

    public static void SetColumnStretchMinWidth( DependencyObject obj, double value )
    {
      obj.SetValue( TableView.ColumnStretchMinWidthProperty, value );
    }

    internal static bool ValidateColumnStretchMinWidthCallback( object value )
    {
      double doubleValue = ( double )value;

      if( doubleValue < 0d )
        return false;

      if( double.IsInfinity( doubleValue ) )
        return false;

      if( double.IsNaN( doubleValue ) )
        return false;

      return true;
    }

    #endregion ColumnStretchMinWidth Property

    #region ColumnStretchMode Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty ColumnStretchModeProperty =
      DependencyProperty.RegisterAttached( "ColumnStretchMode", typeof( ColumnStretchMode ), typeof( TableView ),
      new UIPropertyMetadata( ColumnStretchMode.None, new PropertyChangedCallback( TableView.ColumnStretchModeChanged ) ) );

    public ColumnStretchMode ColumnStretchMode
    {
      get
      {
        return ( ColumnStretchMode )this.GetValue( TableView.ColumnStretchModeProperty );
      }
      set
      {
        this.SetValue( TableView.ColumnStretchModeProperty, value );
      }
    }

    public static ColumnStretchMode GetColumnStretchMode( DependencyObject obj )
    {
      return ( ColumnStretchMode )obj.GetValue( TableView.ColumnStretchModeProperty );
    }

    public static void SetColumnStretchMode( DependencyObject obj, ColumnStretchMode value )
    {
      obj.SetValue( TableView.ColumnStretchModeProperty, value );
    }

    private static void ColumnStretchModeChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridContext dataGridContext = sender as DataGridContext;

      System.Diagnostics.Debug.Assert( ( dataGridContext != null ) || ( sender is TableView ) || ( sender is DetailConfiguration ), "ColumnStretchMode is currently only handled on TableView, DetailConfiguration and DataGridContext." );

      if( ( dataGridContext != null ) && ( dataGridContext.VisibleColumns.Count > 0 ) )
      {
        // When ColumnStretchMode changes, the DesiredWidth columns will not be the
        // same. Reset all DesiredWidth.
        foreach( ColumnBase column in dataGridContext.VisibleColumns )
        {
          column.ClearValue( Column.DesiredWidthProperty );
        }

        // The previous ClearValues don't necessary have an effect. For instance, if a 
        // MaxWidth was in effect for the previous stretch mode or if there are no * in 
        // "None" stretch mode. Force a refresh by assigning a dummy value to the 
        // DesiredWidth of the new stretch mode related Column that will be ultimately 
        // modified by VirtualizingStackPanel.MeasureOverride.
        switch( ( ColumnStretchMode )e.NewValue )
        {
          case ColumnStretchMode.First:
            TableView.TouchColumnWidth( dataGridContext.VisibleColumns[ 0 ] );
            break;

          case ColumnStretchMode.Last:
            TableView.TouchColumnWidth( dataGridContext.VisibleColumns[ dataGridContext.VisibleColumns.Count - 1 ] );
            break;

          case ColumnStretchMode.All:
            {
              foreach( ColumnBase column in dataGridContext.VisibleColumns )
              {
                // One column.ActualWidth modification is enough to trigger a new 
                // Measure pass. Stop at the first effective "touch".
                if( TableView.TouchColumnWidth( column ) )
                  break;
              }
            }
            break;

          case ColumnStretchMode.None:
            {
              foreach( ColumnBase column in dataGridContext.VisibleColumns )
              {
                if( column.Width.UnitType == ColumnWidthUnitType.Star )
                {
                  // One column.ActualWidth modification is enough to trigger a new 
                  // Measure pass. Stop at the first effective "touch".
                  if( TableView.TouchColumnWidth( column ) )
                    break;
                }
              }
            }
            break;
        }
      }
    }

    private static bool TouchColumnWidth( ColumnBase column )
    {
      bool modified = false;

      if( column != null )
      {
        double oldActualWidth = column.ActualWidth;

        // Assign a temporary value to DesiredWidth (different from ActualWidth) 
        // to force a new Measure pass.
        if( column.ActualWidth > column.MinWidth )
        {
          column.DesiredWidth = column.MinWidth;
        }
        else
        {
          column.DesiredWidth = column.ActualWidth + 1d;
        }

        modified = !Xceed.Utils.Math.DoubleUtil.AreClose( oldActualWidth, column.ActualWidth );
      }

      return modified;
    }

    #endregion ColumnStretchMode Property

    #region RemoveColumnStretchingOnResize Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty RemoveColumnStretchingOnResizeProperty =
      DependencyProperty.RegisterAttached( "RemoveColumnStretchingOnResize", typeof( bool ), typeof( TableView ),
      new UIPropertyMetadata( false ) );

    public bool RemoveColumnStretchingOnResize
    {
      get
      {
        return ( bool )this.GetValue( TableView.RemoveColumnStretchingOnResizeProperty );
      }
      set
      {
        this.SetValue( TableView.RemoveColumnStretchingOnResizeProperty, value );
      }
    }

    public static bool GetRemoveColumnStretchingOnResize( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableView.RemoveColumnStretchingOnResizeProperty );
    }

    public static void SetRemoveColumnStretchingOnResize( DependencyObject obj, bool value )
    {
      obj.SetValue( TableView.RemoveColumnStretchingOnResizeProperty, value );
    }

    #endregion RemoveColumnStretchingOnResize Property

    #region FixedColumnCount Property

    [ViewProperty( ViewPropertyMode.RoutedNoFallback )]
    public static readonly DependencyProperty FixedColumnCountProperty =
      DependencyProperty.RegisterAttached( "FixedColumnCount", typeof( int ), typeof( TableView ),
      new UIPropertyMetadata( 0 ),
      new ValidateValueCallback( TableView.ValidateFixedColumnCountCallback ) );

    public int FixedColumnCount
    {
      get
      {
        return ( int )this.GetValue( TableView.FixedColumnCountProperty );
      }
      set
      {
        this.SetValue( TableView.FixedColumnCountProperty, value );
      }
    }

    private static bool ValidateFixedColumnCountCallback( object value )
    {
      return ( ( int )value >= 0 );
    }

    public static int GetFixedColumnCount( DependencyObject obj )
    {
      return ( int )obj.GetValue( TableView.FixedColumnCountProperty );
    }

    public static void SetFixedColumnCount( DependencyObject obj, int value )
    {
      obj.SetValue( TableView.FixedColumnCountProperty, value );
    }

    #endregion FixedColumnCount Property

    #region FixedColumnDropMarkPen Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty FixedColumnDropMarkPenProperty =
        DependencyProperty.RegisterAttached( "FixedColumnDropMarkPen", typeof( Pen ), typeof( TableView ) );

    public Pen FixedColumnDropMarkPen
    {
      get
      {
        return ( Pen )this.GetValue( TableView.FixedColumnDropMarkPenProperty );
      }
      set
      {
        this.SetValue( TableView.FixedColumnDropMarkPenProperty, value );
      }
    }

    public static Pen GetFixedColumnDropMarkPen( DependencyObject obj )
    {
      return ( Pen )obj.GetValue( TableView.FixedColumnDropMarkPenProperty );
    }

    public static void SetFixedColumnDropMarkPen( DependencyObject obj, Pen value )
    {
      obj.SetValue( TableView.FixedColumnDropMarkPenProperty, value );
    }

    #endregion FixedColumnDropMarkPen Property

    #region HorizontalGridLineBrush Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty HorizontalGridLineBrushProperty =
        DependencyProperty.RegisterAttached( "HorizontalGridLineBrush", typeof( Brush ), typeof( TableView ), new PropertyMetadata( null ) );

    public Brush HorizontalGridLineBrush
    {
      get
      {
        return ( Brush )this.GetValue( TableView.HorizontalGridLineBrushProperty );
      }
      set
      {
        this.SetValue( TableView.HorizontalGridLineBrushProperty, value );
      }
    }

    public static Brush GetHorizontalGridLineBrush( DependencyObject obj )
    {
      return ( Brush )obj.GetValue( TableView.HorizontalGridLineBrushProperty );
    }

    public static void SetHorizontalGridLineBrush( DependencyObject obj, Brush value )
    {
      obj.SetValue( TableView.HorizontalGridLineBrushProperty, value );
    }

    #endregion HorizontalGridLineBrush Property

    #region HorizontalGridLineThickness Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty HorizontalGridLineThicknessProperty =
        DependencyProperty.RegisterAttached( "HorizontalGridLineThickness", typeof( double ), typeof( TableView ), new PropertyMetadata() );

    public double HorizontalGridLineThickness
    {
      get
      {
        return ( double )this.GetValue( TableView.HorizontalGridLineThicknessProperty );
      }
      set
      {
        this.SetValue( TableView.HorizontalGridLineThicknessProperty, value );
      }
    }

    public static double GetHorizontalGridLineThickness( DependencyObject obj )
    {
      return ( double )obj.GetValue( TableView.HorizontalGridLineThicknessProperty );
    }

    public static void SetHorizontalGridLineThickness( DependencyObject obj, double value )
    {
      obj.SetValue( TableView.HorizontalGridLineThicknessProperty, value );
    }

    #endregion HorizontalGridLineThickness Property

    #region VerticalGridLineBrush Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty VerticalGridLineBrushProperty =
        DependencyProperty.RegisterAttached( "VerticalGridLineBrush", typeof( Brush ), typeof( TableView ), new PropertyMetadata( null ) );

    public Brush VerticalGridLineBrush
    {
      get
      {
        return ( Brush )this.GetValue( TableView.VerticalGridLineBrushProperty );
      }
      set
      {
        this.SetValue( TableView.VerticalGridLineBrushProperty, value );
      }
    }

    public static Brush GetVerticalGridLineBrush( DependencyObject obj )
    {
      return ( Brush )obj.GetValue( TableView.VerticalGridLineBrushProperty );
    }

    public static void SetVerticalGridLineBrush( DependencyObject obj, Brush value )
    {
      obj.SetValue( TableView.VerticalGridLineBrushProperty, value );
    }

    #endregion VerticalGridLineBrush Property

    #region VerticalGridLineThickness Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty VerticalGridLineThicknessProperty =
        DependencyProperty.RegisterAttached( "VerticalGridLineThickness", typeof( double ), typeof( TableView ), new PropertyMetadata() );

    public double VerticalGridLineThickness
    {
      get
      {
        return ( double )this.GetValue( TableView.VerticalGridLineThicknessProperty );
      }
      set
      {
        this.SetValue( TableView.VerticalGridLineThicknessProperty, value );
      }
    }

    public static double GetVerticalGridLineThickness( DependencyObject obj )
    {
      return ( double )obj.GetValue( TableView.VerticalGridLineThicknessProperty );
    }

    public static void SetVerticalGridLineThickness( DependencyObject obj, double value )
    {
      obj.SetValue( TableView.VerticalGridLineThicknessProperty, value );
    }

    #endregion VerticalGridLineThickness Property

    #region AllowRowResize Property

    //Note: This property is exceptional, effectivelly, while this is a property that configures the View, it is not an attached property
    //      This has the result that it is not settable on any other thing than a TableView (cannot be configured differently for details).
    //      The property keeps on working as intended since it is a DP and we rely on the "Attached" property mechanism only so that the 
    //      property can be set on DetailConfigurations.

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty AllowRowResizeProperty = DependencyProperty.Register(
      "AllowRowResize",
      typeof( bool ),
      typeof( TableView ),
      new FrameworkPropertyMetadata( true ) );

    public bool AllowRowResize
    {
      get
      {
        return ( bool )this.GetValue( TableView.AllowRowResizeProperty );
      }
      set
      {
        this.SetValue( TableView.AllowRowResizeProperty, value );
      }
    }

    #endregion AllowRowResize Property

    #region RowSelectorPaneWidth Property

    //Note: This property is exceptional, effectivelly, while this is a property that configures the View, it is not an attached property
    //      This has the result that it is not settable on any other thing than a TableView (cannot be configured differently for details).
    //      The property keeps on working as intended since it is a DP and we rely on the "Attached" property mechanism only so that the 
    //      property can be set on DetailConfigurations.

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty RowSelectorPaneWidthProperty =
      TableViewScrollViewer.RowSelectorPaneWidthProperty.AddOwner( typeof( TableView ) );

    public double RowSelectorPaneWidth
    {
      get
      {
        return ( double )this.GetValue( TableView.RowSelectorPaneWidthProperty );
      }
      set
      {
        this.SetValue( TableView.RowSelectorPaneWidthProperty, value );
      }
    }

    #endregion RowSelectorPaneWidth Property

    #region ShowRowSelectorPane Property

    //Note: This property is exceptional, effectivelly, while this is a property that configures the View, it is not an attached property
    //      This has the result that it is not settable on any other thing than a TableView (cannot be configured differently for details).
    //      The property keeps on working as intended since it is a DP and we rely on the "Attached" property mechanism only so that the 
    //      property can be set on DetailConfigurations.

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty ShowRowSelectorPaneProperty =
        DependencyProperty.Register( "ShowRowSelectorPane", typeof( bool ), typeof( TableView ), new PropertyMetadata( true ) );

    public bool ShowRowSelectorPane
    {
      get
      {
        return ( bool )this.GetValue( TableView.ShowRowSelectorPaneProperty );
      }
      set
      {
        this.SetValue( TableView.ShowRowSelectorPaneProperty, value );
      }
    }

    #endregion ShowRowSelectorPane Property

    #region GroupLevelIndicatorWidth Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty GroupLevelIndicatorWidthProperty =
        DependencyProperty.RegisterAttached( "GroupLevelIndicatorWidth", typeof( double ), typeof( TableView ), new UIPropertyMetadata() );

    public double GroupLevelIndicatorWidth
    {
      get
      {
        return ( double )this.GetValue( TableView.GroupLevelIndicatorWidthProperty );
      }
      set
      {
        this.SetValue( TableView.GroupLevelIndicatorWidthProperty, value );
      }
    }

    public static double GetGroupLevelIndicatorWidth( DependencyObject obj )
    {
      return ( double )obj.GetValue( TableView.GroupLevelIndicatorWidthProperty );
    }

    public static void SetGroupLevelIndicatorWidth( DependencyObject obj, double value )
    {
      obj.SetValue( TableView.GroupLevelIndicatorWidthProperty, value );
    }

    #endregion GroupLevelIndicatorWidth Property

    #region DetailIndicatorWidth Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty DetailIndicatorWidthProperty =
        DependencyProperty.RegisterAttached( "DetailIndicatorWidth", typeof( double ), typeof( TableView ), new UIPropertyMetadata() );

    public double DetailIndicatorWidth
    {
      get
      {
        return ( double )this.GetValue( TableView.DetailIndicatorWidthProperty );
      }
      set
      {
        this.SetValue( TableView.DetailIndicatorWidthProperty, value );
      }
    }

    public static double GetDetailIndicatorWidth( DependencyObject obj )
    {
      return ( double )obj.GetValue( TableView.DetailIndicatorWidthProperty );
    }

    public static void SetDetailIndicatorWidth( DependencyObject obj, double value )
    {
      obj.SetValue( TableView.DetailIndicatorWidthProperty, value );
    }

    #endregion DetailIndicatorWidth Property

    #region IsColumnVirtualizationEnabled Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty IsColumnVirtualizationEnabledProperty =
        DependencyProperty.RegisterAttached( "IsColumnVirtualizationEnabled",
          typeof( bool ),
          typeof( TableView ),
          new UIPropertyMetadata( true ) );

    public bool IsColumnVirtualizationEnabled
    {
      get
      {
        return ( bool )this.GetValue( TableView.IsColumnVirtualizationEnabledProperty );
      }
      set
      {
        this.SetValue( TableView.IsColumnVirtualizationEnabledProperty, value );
      }
    }

    public static bool GetIsColumnVirtualizationEnabled( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableView.IsColumnVirtualizationEnabledProperty );
    }

    public static void SetIsColumnVirtualizationEnabled( DependencyObject obj, bool value )
    {
      obj.SetValue( TableView.IsColumnVirtualizationEnabledProperty, value );
    }

    #endregion

    #region IsAlternatingRowStyleEnabled Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty IsAlternatingRowStyleEnabledProperty =
      DependencyProperty.RegisterAttached( "IsAlternatingRowStyleEnabled", typeof( bool ), typeof( TableView ), new UIPropertyMetadata( false ) );

    public bool IsAlternatingRowStyleEnabled
    {
      get
      {
        return TableView.GetIsAlternatingRowStyleEnabled( this );
      }
      set
      {
        TableView.SetIsAlternatingRowStyleEnabled( this, value );
      }
    }

    public static bool GetIsAlternatingRowStyleEnabled( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableView.IsAlternatingRowStyleEnabledProperty );
    }

    public static void SetIsAlternatingRowStyleEnabled( DependencyObject obj, bool value )
    {
      obj.SetValue( TableView.IsAlternatingRowStyleEnabledProperty, value );
    }

    #endregion IsAlternatingRowStyleEnabled Property

    #region AutoScrollInterval Attached Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty AutoScrollIntervalProperty = DependencyProperty.RegisterAttached(
      "AutoScrollInterval",
      typeof( int ),
      typeof( TableView ),
      new FrameworkPropertyMetadata( 100 ) );

    public static int GetAutoScrollInterval( DependencyObject obj )
    {
      return ( int )obj.GetValue( TableView.AutoScrollIntervalProperty );
    }

    public static void SetAutoScrollInterval( DependencyObject obj, int value )
    {
      obj.SetValue( TableView.AutoScrollIntervalProperty, value );
    }

    #endregion

    #region AutoScrollTreshold Attached Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    internal static readonly DependencyProperty AutoScrollTresholdProperty = DependencyProperty.RegisterAttached(
      "AutoScrollTreshold",
      typeof( int ),
      typeof( TableView ),
      new FrameworkPropertyMetadata( 5 ) );

    internal static int GetAutoScrollTreshold( DependencyObject obj )
    {
      return ( int )obj.GetValue( TableView.AutoScrollTresholdProperty );
    }

    internal static void SetAutoScrollTreshold( DependencyObject obj, int value )
    {
      obj.SetValue( TableView.AutoScrollTresholdProperty, value );
    }

    #endregion

    #region RowSelectorResizeNorthSouthCursor Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty RowSelectorResizeNorthSouthCursorProperty = DependencyProperty.Register(
      "RowSelectorResizeNorthSouthCursor",
      typeof( Cursor ),
      typeof( TableView ),
      new FrameworkPropertyMetadata( TableView.DefaultRowSelectorResizeNorthSouthCursor ) );

    public Cursor RowSelectorResizeNorthSouthCursor
    {
      get
      {
        return ( Cursor )this.GetValue( TableView.RowSelectorResizeNorthSouthCursorProperty );
      }
      set
      {
        this.SetValue( TableView.RowSelectorResizeNorthSouthCursorProperty, value );
      }
    }

    #endregion

    #region RowSelectorResizeWestEastCursor Property

    [ViewProperty( ViewPropertyMode.Routed )]
    public static readonly DependencyProperty RowSelectorResizeWestEastCursorProperty = DependencyProperty.Register(
      "RowSelectorResizeWestEastCursor",
      typeof( Cursor ),
      typeof( TableView ),
      new FrameworkPropertyMetadata( TableView.DefaultRowSelectorResizeWestEastCursor ) );

    public Cursor RowSelectorResizeWestEastCursor
    {
      get
      {
        return ( Cursor )this.GetValue( TableView.RowSelectorResizeWestEastCursorProperty );
      }
      set
      {
        this.SetValue( TableView.RowSelectorResizeWestEastCursorProperty, value );
      }
    }

    #endregion

    #region Internal Methods

    internal override ColumnVirtualizationManager CreateColumnVirtualizationManager( DataGridContext dataGridContext )
    {
      Debug.Assert( dataGridContext != null );

      return new TableViewColumnVirtualizationManager( dataGridContext );
    }

    #endregion

    #region Protected Methods

    protected override void AddDefaultHeadersFooters()
    {
      this.FixedHeaders.Insert( 0, TableView.DefaultColumnManagerRowTemplate );
      this.FixedHeaders.Insert( 0, TableView.DefaultGroupByControlTemplate );
    }

    #endregion

    #region Private Fields

    private static readonly DataTemplate DefaultColumnManagerRowTemplate;
    private static readonly DataTemplate DefaultGroupByControlTemplate;

    #endregion
  }
}
