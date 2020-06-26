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

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Threading;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public abstract class LayoutGridControl<T> : Grid, ILayoutControl where T : class, ILayoutPanelElement
  {
    #region Members

    private LayoutPositionableGroup<T> _model;
    private Orientation _orientation;
    private bool _initialized;
    private ChildrenTreeChange? _asyncRefreshCalled;
    private ReentrantFlag _fixingChildrenDockLengths = new ReentrantFlag();
    private Border _resizerGhost = null;
    private Window _resizerWindowHost = null;
    private Vector _initialStartPoint;

    #endregion

    #region Constructors

    static LayoutGridControl()
    {
    }

    internal LayoutGridControl( LayoutPositionableGroup<T> model, Orientation orientation )
    {
      if( model == null )
        throw new ArgumentNullException( "model" );

      _model = model;
      _orientation = orientation;

      FlowDirection = System.Windows.FlowDirection.LeftToRight;
    }

    #endregion

    #region Properties

    public ILayoutElement Model
    {
      get
      {
        return _model;
      }
    }   

    public Orientation Orientation
    {
      get
      {
        return ( _model as ILayoutOrientableGroup ).Orientation;
      }
    } 

    private bool AsyncRefreshCalled
    {
      get
      {
        return _asyncRefreshCalled != null;
      }
    }

    #endregion

    #region Overrides

    protected override void OnInitialized( EventArgs e )
    {
      base.OnInitialized( e );

      _model.ChildrenTreeChanged += ( s, args ) =>
          {
            if( args.Change != ChildrenTreeChange.DirectChildrenChanged )
              return;
            if( _asyncRefreshCalled.HasValue &&
                      _asyncRefreshCalled.Value == args.Change )
              return;
            _asyncRefreshCalled = args.Change;
            Dispatcher.BeginInvoke( new Action( () =>
                  {
                    _asyncRefreshCalled = null;
                    UpdateChildren();
                  } ), DispatcherPriority.Normal, null );
          };

      this.LayoutUpdated += new EventHandler( OnLayoutUpdated );
    }

    #endregion

    #region Internal Methods

    protected void FixChildrenDockLengths()
    {
      using( _fixingChildrenDockLengths.Enter() )
        OnFixChildrenDockLengths();
    }

    protected abstract void OnFixChildrenDockLengths();

    #endregion

    #region Private Methods

    private void OnLayoutUpdated( object sender, EventArgs e )
    {
      var modelWithAtcualSize = _model as ILayoutPositionableElementWithActualSize;
      modelWithAtcualSize.ActualWidth = ActualWidth;
      modelWithAtcualSize.ActualHeight = ActualHeight;

      if( !_initialized )
      {
        _initialized = true;
        UpdateChildren();
      }
    }

    private void UpdateChildren()
    {
      var alreadyContainedChildren = Children.OfType<ILayoutControl>().ToArray();

      DetachOldSplitters();
      DetachPropertChangeHandler();

      Children.Clear();
      ColumnDefinitions.Clear();
      RowDefinitions.Clear();

      if( _model == null ||
          _model.Root == null )
        return;

      var manager = _model.Root.Manager;
      if( manager == null )
        return;


      foreach( ILayoutElement child in _model.Children )
      {
        var foundContainedChild = alreadyContainedChildren.FirstOrDefault( chVM => chVM.Model == child );
        if( foundContainedChild != null )
          Children.Add( foundContainedChild as UIElement );
        else
          Children.Add( manager.CreateUIElementForModel( child ) );
      }

      CreateSplitters();

      UpdateRowColDefinitions();

      AttachNewSplitters();
      AttachPropertyChangeHandler();
    }

    private void AttachPropertyChangeHandler()
    {
      foreach( var child in InternalChildren.OfType<ILayoutControl>() )
      {
        child.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler( this.OnChildModelPropertyChanged );
      }
    }

    private void DetachPropertChangeHandler()
    {
      foreach( var child in InternalChildren.OfType<ILayoutControl>() )
      {
        child.Model.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler( this.OnChildModelPropertyChanged );
      }
    }

    private void OnChildModelPropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
    {
      if( AsyncRefreshCalled )
        return;

      if( _fixingChildrenDockLengths.CanEnter && e.PropertyName == "DockWidth" && Orientation == System.Windows.Controls.Orientation.Horizontal )
      {
        if( ColumnDefinitions.Count == InternalChildren.Count )
        {
          var changedElement = sender as ILayoutPositionableElement;
          var childFromModel = InternalChildren.OfType<ILayoutControl>().First( ch => ch.Model == changedElement ) as UIElement;
          int indexOfChild = InternalChildren.IndexOf( childFromModel );
          ColumnDefinitions[ indexOfChild ].Width = changedElement.DockWidth;
        }
      }
      else if( _fixingChildrenDockLengths.CanEnter && e.PropertyName == "DockHeight" && Orientation == System.Windows.Controls.Orientation.Vertical )
      {
        if( RowDefinitions.Count == InternalChildren.Count )
        {
          var changedElement = sender as ILayoutPositionableElement;
          var childFromModel = InternalChildren.OfType<ILayoutControl>().First( ch => ch.Model == changedElement ) as UIElement;
          int indexOfChild = InternalChildren.IndexOf( childFromModel );
          RowDefinitions[ indexOfChild ].Height = changedElement.DockHeight;
        }
      }
      else if( e.PropertyName == "IsVisible" )
      {
        UpdateRowColDefinitions();
      }
    }

    private void UpdateRowColDefinitions()
    {
      var root = _model.Root;
      if( root == null )
        return;
      var manager = root.Manager;
      if( manager == null )
        return;

      FixChildrenDockLengths();

      //Debug.Assert(InternalChildren.Count == _model.ChildrenCount + (_model.ChildrenCount - 1));

      #region Setup GridRows/Cols
      RowDefinitions.Clear();
      ColumnDefinitions.Clear();
      if( Orientation == Orientation.Horizontal )
      {
        int iColumn = 0;
        int iChild = 0;
        for( int iChildModel = 0; iChildModel < _model.Children.Count; iChildModel++, iColumn++, iChild++ )
        {
          var childModel = _model.Children[ iChildModel ] as ILayoutPositionableElement;
          ColumnDefinitions.Add( new ColumnDefinition()
          {
            Width = childModel.IsVisible ? childModel.DockWidth : new GridLength( 0.0, GridUnitType.Pixel ),
            MinWidth = childModel.IsVisible ? childModel.DockMinWidth : 0.0
          } );
          Grid.SetColumn( InternalChildren[ iChild ], iColumn );

          //append column for splitter
          if( iChild < InternalChildren.Count - 1 )
          {
            iChild++;
            iColumn++;

            bool nextChildModelVisibleExist = false;
            for( int i = iChildModel + 1; i < _model.Children.Count; i++ )
            {
              var nextChildModel = _model.Children[ i ] as ILayoutPositionableElement;
              if( nextChildModel.IsVisible )
              {
                nextChildModelVisibleExist = true;
                break;
              }
            }

            ColumnDefinitions.Add( new ColumnDefinition()
            {
              Width = childModel.IsVisible && nextChildModelVisibleExist ? new GridLength( double.IsNaN( manager.GridSplitterWidth ) ? 6d : manager.GridSplitterWidth ) : new GridLength( 0.0, GridUnitType.Pixel )
            } );
            Grid.SetColumn( InternalChildren[ iChild ], iColumn );
          }
        }
      }
      else //if (_model.Orientation == Orientation.Vertical)
      {
        int iRow = 0;
        int iChild = 0;
        for( int iChildModel = 0; iChildModel < _model.Children.Count; iChildModel++, iRow++, iChild++ )
        {
          var childModel = _model.Children[ iChildModel ] as ILayoutPositionableElement;
          RowDefinitions.Add( new RowDefinition()
          {
            Height = childModel.IsVisible ? childModel.DockHeight : new GridLength( 0.0, GridUnitType.Pixel ),
            MinHeight = childModel.IsVisible ? childModel.DockMinHeight : 0.0
          } );
          Grid.SetRow( InternalChildren[ iChild ], iRow );

          //if (RowDefinitions.Last().Height.Value == 0.0)
          //    System.Diagnostics.Debugger.Break();

          //append row for splitter (if necessary)
          if( iChild < InternalChildren.Count - 1 )
          {
            iChild++;
            iRow++;

            bool nextChildModelVisibleExist = false;
            for( int i = iChildModel + 1; i < _model.Children.Count; i++ )
            {
              var nextChildModel = _model.Children[ i ] as ILayoutPositionableElement;
              if( nextChildModel.IsVisible )
              {
                nextChildModelVisibleExist = true;
                break;
              }
            }

            RowDefinitions.Add( new RowDefinition()
            {
              Height = childModel.IsVisible && nextChildModelVisibleExist ? new GridLength( double.IsNaN( manager.GridSplitterHeight ) ? 6d : manager.GridSplitterHeight ) : new GridLength( 0.0, GridUnitType.Pixel )
            } );
            //if (RowDefinitions.Last().Height.Value == 0.0)
            //    System.Diagnostics.Debugger.Break();
            Grid.SetRow( InternalChildren[ iChild ], iRow );
          }
        }
      }

      #endregion
    }

    private void CreateSplitters()
    {
      for( int iChild = 1; iChild < Children.Count; iChild++ )
      {
        var splitter = new LayoutGridResizerControl();
        splitter.Cursor = this.Orientation == Orientation.Horizontal ? Cursors.SizeWE : Cursors.SizeNS;
        Children.Insert( iChild, splitter );
        iChild++;
      }
    }

    private void DetachOldSplitters()
    {
      foreach( var splitter in Children.OfType<LayoutGridResizerControl>() )
      {
        splitter.DragStarted -= new System.Windows.Controls.Primitives.DragStartedEventHandler( OnSplitterDragStarted );
        splitter.DragDelta -= new System.Windows.Controls.Primitives.DragDeltaEventHandler( OnSplitterDragDelta );
        splitter.DragCompleted -= new System.Windows.Controls.Primitives.DragCompletedEventHandler( OnSplitterDragCompleted );
      }
    }

    private void AttachNewSplitters()
    {
      foreach( var splitter in Children.OfType<LayoutGridResizerControl>() )
      {
        splitter.DragStarted += new System.Windows.Controls.Primitives.DragStartedEventHandler( OnSplitterDragStarted );
        splitter.DragDelta += new System.Windows.Controls.Primitives.DragDeltaEventHandler( OnSplitterDragDelta );
        splitter.DragCompleted += new System.Windows.Controls.Primitives.DragCompletedEventHandler( OnSplitterDragCompleted );
      }
    }

    private void OnSplitterDragStarted( object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e )
    {
      var resizer = sender as LayoutGridResizerControl;
      ShowResizerOverlayWindow( resizer );
    }

    private void OnSplitterDragDelta( object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e )
    {
      LayoutGridResizerControl splitter = sender as LayoutGridResizerControl;
      var rootVisual = this.FindVisualTreeRoot() as Visual;

      var trToWnd = TransformToAncestor( rootVisual );
      Vector transformedDelta = trToWnd.Transform( new Point( e.HorizontalChange, e.VerticalChange ) ) -
          trToWnd.Transform( new Point() );

      if( Orientation == System.Windows.Controls.Orientation.Horizontal )
      {
        Canvas.SetLeft( _resizerGhost, MathHelper.MinMax( _initialStartPoint.X + transformedDelta.X, 0.0, _resizerWindowHost.Width - _resizerGhost.Width ) );
      }
      else
      {
        Canvas.SetTop( _resizerGhost, MathHelper.MinMax( _initialStartPoint.Y + transformedDelta.Y, 0.0, _resizerWindowHost.Height - _resizerGhost.Height ) );
      }
    }

    private void OnSplitterDragCompleted( object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e )
    {
      LayoutGridResizerControl splitter = sender as LayoutGridResizerControl;
      var rootVisual = this.FindVisualTreeRoot() as Visual;

      var trToWnd = TransformToAncestor( rootVisual );
      Vector transformedDelta = trToWnd.Transform( new Point( e.HorizontalChange, e.VerticalChange ) ) -
          trToWnd.Transform( new Point() );

      double delta;
      if( Orientation == System.Windows.Controls.Orientation.Horizontal )
        delta = Canvas.GetLeft( _resizerGhost ) - _initialStartPoint.X;
      else
        delta = Canvas.GetTop( _resizerGhost ) - _initialStartPoint.Y;

      int indexOfResizer = InternalChildren.IndexOf( splitter );

      var prevChild = InternalChildren[ indexOfResizer - 1 ] as FrameworkElement;
      var nextChild = GetNextVisibleChild( indexOfResizer );

      var prevChildActualSize = prevChild.TransformActualSizeToAncestor();
      var nextChildActualSize = (nextChild != null) ? nextChild.TransformActualSizeToAncestor() : new Size();

      var prevChildModel = ( ILayoutPositionableElement )( prevChild as ILayoutControl ).Model;
      var nextChildModel = (nextChild != null) ? ( ILayoutPositionableElement )( nextChild as ILayoutControl ).Model : null;

      if( Orientation == System.Windows.Controls.Orientation.Horizontal )
      {
        if( prevChildModel.DockWidth.IsStar )
        {
          prevChildModel.DockWidth = new GridLength( prevChildActualSize.Width + delta, GridUnitType.Star );
        }
        else
        {
          var width = ( prevChildModel.DockWidth.IsAuto ) ? prevChildActualSize.Width : prevChildModel.DockWidth.Value;
          width += delta;
          var widthToSet = Math.Max( width, prevChildModel.DockMinWidth );
          prevChildModel.DockWidth = new GridLength( widthToSet, GridUnitType.Pixel );
        }

        if( nextChildModel != null )
        {
          if( nextChildModel.DockWidth.IsStar )
          {
            nextChildModel.DockWidth = new GridLength( nextChildActualSize.Width - delta, GridUnitType.Star );
          }
          else
          {
            var width = ( nextChildModel.DockWidth.IsAuto ) ? nextChildActualSize.Width : nextChildModel.DockWidth.Value;
            width -= delta;
            var widthToSet = Math.Max( width, nextChildModel.DockMinWidth );
            nextChildModel.DockWidth = new GridLength( widthToSet, GridUnitType.Pixel );
          }
        }
      }
      else
      {
        if( prevChildModel.DockHeight.IsStar )
        {
          prevChildModel.DockHeight = new GridLength( prevChildActualSize.Height + delta, GridUnitType.Star );
        }
        else
        {
          var height = ( prevChildModel.DockHeight.IsAuto ) ? prevChildActualSize.Height : prevChildModel.DockHeight.Value;
          height += delta;
          var heightToSet = Math.Max( height, prevChildModel.DockMinHeight );
          prevChildModel.DockHeight = new GridLength( heightToSet, GridUnitType.Pixel );
        }

        if( nextChildModel != null )
        {
          if( nextChildModel.DockHeight.IsStar )
          {
            nextChildModel.DockHeight = new GridLength( nextChildActualSize.Height - delta, GridUnitType.Star );
          }
          else
          {
            var height = ( nextChildModel.DockHeight.IsAuto ) ? nextChildActualSize.Height : nextChildModel.DockHeight.Value;
            height -= delta;
            var heightToSet = Math.Max( height, nextChildModel.DockMinHeight );
            nextChildModel.DockHeight = new GridLength( heightToSet, GridUnitType.Pixel );
          }
        }
      }

      HideResizerOverlayWindow();
    }

    private FrameworkElement GetNextVisibleChild( int index )
    {
      for( int i = index + 1; i < InternalChildren.Count; i++ )
      {
        if( InternalChildren[ i ] is LayoutGridResizerControl )
          continue;

        if( Orientation == System.Windows.Controls.Orientation.Horizontal )
        {
          if( ColumnDefinitions[ i ].Width.IsStar || ColumnDefinitions[ i ].Width.Value > 0 )
            return InternalChildren[ i ] as FrameworkElement;
        }
        else
        {
          if( RowDefinitions[ i ].Height.IsStar || RowDefinitions[ i ].Height.Value > 0 )
            return InternalChildren[ i ] as FrameworkElement;
        }
      }

      return null;
    }

    private void ShowResizerOverlayWindow( LayoutGridResizerControl splitter )
    {
      _resizerGhost = new Border()
      {
        Background = splitter.BackgroundWhileDragging,
        Opacity = splitter.OpacityWhileDragging
      };

      int indexOfResizer = InternalChildren.IndexOf( splitter );

      var prevChild = InternalChildren[ indexOfResizer - 1 ] as FrameworkElement;
      var nextChild = GetNextVisibleChild( indexOfResizer );

      var prevChildActualSize = prevChild.TransformActualSizeToAncestor();
      var nextChildActualSize = (nextChild != null) ? nextChild.TransformActualSizeToAncestor() : new Size();

      var prevChildModel = ( ILayoutPositionableElement )( prevChild as ILayoutControl ).Model;
      var nextChildModel = (nextChild != null) ? ( ILayoutPositionableElement )( nextChild as ILayoutControl ).Model : null;

      Point ptTopLeftScreen = prevChild.PointToScreenDPIWithoutFlowDirection( new Point() );

      Size actualSize;

      if( Orientation == System.Windows.Controls.Orientation.Horizontal )
      {
        var nextChildDockMinWidth = (nextChildModel != null) ? nextChildModel.DockMinWidth : 0d;

        actualSize = new Size(
            prevChildActualSize.Width - prevChildModel.DockMinWidth + splitter.ActualWidth + nextChildActualSize.Width - nextChildDockMinWidth,
            nextChildActualSize.Height );

        _resizerGhost.Width = splitter.ActualWidth;
        _resizerGhost.Height = actualSize.Height;
        ptTopLeftScreen.Offset( prevChildModel.DockMinWidth, 0.0 );
      }
      else
      {
        var nextChildDockMinHeight = ( nextChildModel != null ) ? nextChildModel.DockMinHeight : 0d;

        actualSize = new Size(
            prevChildActualSize.Width,
            prevChildActualSize.Height - prevChildModel.DockMinHeight + splitter.ActualHeight + nextChildActualSize.Height - nextChildDockMinHeight );

        _resizerGhost.Height = splitter.ActualHeight;
        _resizerGhost.Width = actualSize.Width;

        ptTopLeftScreen.Offset( 0.0, prevChildModel.DockMinHeight );
      }

      _initialStartPoint = splitter.PointToScreenDPIWithoutFlowDirection( new Point() ) - ptTopLeftScreen;

      if( Orientation == System.Windows.Controls.Orientation.Horizontal )
      {
        Canvas.SetLeft( _resizerGhost, _initialStartPoint.X );
      }
      else
      {
        Canvas.SetTop( _resizerGhost, _initialStartPoint.Y );
      }

      Canvas panelHostResizer = new Canvas()
      {
        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
        VerticalAlignment = System.Windows.VerticalAlignment.Stretch
      };

      panelHostResizer.Children.Add( _resizerGhost );


      _resizerWindowHost = new Window()
      {
        SizeToContent = System.Windows.SizeToContent.Manual,
        ResizeMode = ResizeMode.NoResize,
        WindowStyle = System.Windows.WindowStyle.None,
        ShowInTaskbar = false,
        AllowsTransparency = true,
        Background = null,
        Width = actualSize.Width,
        Height = actualSize.Height,
        Left = ptTopLeftScreen.X,
        Top = ptTopLeftScreen.Y,
        ShowActivated = false,
        //Owner = Window.GetWindow(this),
        Content = panelHostResizer
      };
      _resizerWindowHost.Loaded += ( s, e ) =>
          {
            _resizerWindowHost.SetParentToMainWindowOf( this );
          };
      _resizerWindowHost.Show();
    }

    private  void HideResizerOverlayWindow()
    {
      if( _resizerWindowHost != null )
      {
        _resizerWindowHost.Close();
        _resizerWindowHost = null;
      }
    }

    #endregion
  }
}
