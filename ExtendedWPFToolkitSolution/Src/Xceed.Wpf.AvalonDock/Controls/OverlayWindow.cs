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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Themes;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class OverlayWindow : Window, IOverlayWindow
  {
    #region Members

    private ResourceDictionary currentThemeResourceDictionary; // = null
    private Canvas _mainCanvasPanel;
    private Grid _gridDockingManagerDropTargets;
    private Grid _gridAnchorablePaneDropTargets;
    private Grid _gridDocumentPaneDropTargets;
    private Grid _gridDocumentPaneFullDropTargets;

    private FrameworkElement _dockingManagerDropTargetBottom;
    private FrameworkElement _dockingManagerDropTargetTop;
    private FrameworkElement _dockingManagerDropTargetLeft;
    private FrameworkElement _dockingManagerDropTargetRight;

    private FrameworkElement _anchorablePaneDropTargetBottom;
    private FrameworkElement _anchorablePaneDropTargetTop;
    private FrameworkElement _anchorablePaneDropTargetLeft;
    private FrameworkElement _anchorablePaneDropTargetRight;
    private FrameworkElement _anchorablePaneDropTargetInto;

    private FrameworkElement _documentPaneDropTargetBottom;
    private FrameworkElement _documentPaneDropTargetTop;
    private FrameworkElement _documentPaneDropTargetLeft;
    private FrameworkElement _documentPaneDropTargetRight;
    private FrameworkElement _documentPaneDropTargetInto;

    private FrameworkElement _documentPaneDropTargetBottomAsAnchorablePane;
    private FrameworkElement _documentPaneDropTargetTopAsAnchorablePane;
    private FrameworkElement _documentPaneDropTargetLeftAsAnchorablePane;
    private FrameworkElement _documentPaneDropTargetRightAsAnchorablePane;

    private FrameworkElement _documentPaneFullDropTargetBottom;
    private FrameworkElement _documentPaneFullDropTargetTop;
    private FrameworkElement _documentPaneFullDropTargetLeft;
    private FrameworkElement _documentPaneFullDropTargetRight;
    private FrameworkElement _documentPaneFullDropTargetInto;

    private Path _previewBox;
    private IOverlayWindowHost _host;
    private LayoutFloatingWindowControl _floatingWindow = null;
    private List<IDropArea> _visibleAreas = new List<IDropArea>();

    #endregion

    #region Constructors

    static OverlayWindow()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( OverlayWindow ), new FrameworkPropertyMetadata( typeof( OverlayWindow ) ) );

      OverlayWindow.AllowsTransparencyProperty.OverrideMetadata( typeof( OverlayWindow ), new FrameworkPropertyMetadata( true ) );
      OverlayWindow.WindowStyleProperty.OverrideMetadata( typeof( OverlayWindow ), new FrameworkPropertyMetadata( WindowStyle.None ) );
      OverlayWindow.ShowInTaskbarProperty.OverrideMetadata( typeof( OverlayWindow ), new FrameworkPropertyMetadata( false ) );
      OverlayWindow.ShowActivatedProperty.OverrideMetadata( typeof( OverlayWindow ), new FrameworkPropertyMetadata( false ) );
      OverlayWindow.VisibilityProperty.OverrideMetadata( typeof( OverlayWindow ), new FrameworkPropertyMetadata( Visibility.Hidden ) );
    }

    internal OverlayWindow( IOverlayWindowHost host )
    {
      _host = host;
      UpdateThemeResources();
    }

    #endregion

    #region Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      _mainCanvasPanel = GetTemplateChild( "PART_DropTargetsContainer" ) as Canvas;
      _gridDockingManagerDropTargets = GetTemplateChild( "PART_DockingManagerDropTargets" ) as Grid;
      _gridAnchorablePaneDropTargets = GetTemplateChild( "PART_AnchorablePaneDropTargets" ) as Grid;
      _gridDocumentPaneDropTargets = GetTemplateChild( "PART_DocumentPaneDropTargets" ) as Grid;
      _gridDocumentPaneFullDropTargets = GetTemplateChild( "PART_DocumentPaneFullDropTargets" ) as Grid;

      _gridDockingManagerDropTargets.Visibility = System.Windows.Visibility.Hidden;
      _gridAnchorablePaneDropTargets.Visibility = System.Windows.Visibility.Hidden;
      _gridDocumentPaneDropTargets.Visibility = System.Windows.Visibility.Hidden;
      if( _gridDocumentPaneFullDropTargets != null )
        _gridDocumentPaneFullDropTargets.Visibility = System.Windows.Visibility.Hidden;

      _dockingManagerDropTargetBottom = GetTemplateChild( "PART_DockingManagerDropTargetBottom" ) as FrameworkElement;
      _dockingManagerDropTargetTop = GetTemplateChild( "PART_DockingManagerDropTargetTop" ) as FrameworkElement;
      _dockingManagerDropTargetLeft = GetTemplateChild( "PART_DockingManagerDropTargetLeft" ) as FrameworkElement;
      _dockingManagerDropTargetRight = GetTemplateChild( "PART_DockingManagerDropTargetRight" ) as FrameworkElement;

      _anchorablePaneDropTargetBottom = GetTemplateChild( "PART_AnchorablePaneDropTargetBottom" ) as FrameworkElement;
      _anchorablePaneDropTargetTop = GetTemplateChild( "PART_AnchorablePaneDropTargetTop" ) as FrameworkElement;
      _anchorablePaneDropTargetLeft = GetTemplateChild( "PART_AnchorablePaneDropTargetLeft" ) as FrameworkElement;
      _anchorablePaneDropTargetRight = GetTemplateChild( "PART_AnchorablePaneDropTargetRight" ) as FrameworkElement;
      _anchorablePaneDropTargetInto = GetTemplateChild( "PART_AnchorablePaneDropTargetInto" ) as FrameworkElement;

      _documentPaneDropTargetBottom = GetTemplateChild( "PART_DocumentPaneDropTargetBottom" ) as FrameworkElement;
      _documentPaneDropTargetTop = GetTemplateChild( "PART_DocumentPaneDropTargetTop" ) as FrameworkElement;
      _documentPaneDropTargetLeft = GetTemplateChild( "PART_DocumentPaneDropTargetLeft" ) as FrameworkElement;
      _documentPaneDropTargetRight = GetTemplateChild( "PART_DocumentPaneDropTargetRight" ) as FrameworkElement;
      _documentPaneDropTargetInto = GetTemplateChild( "PART_DocumentPaneDropTargetInto" ) as FrameworkElement;

      _documentPaneDropTargetBottomAsAnchorablePane = GetTemplateChild( "PART_DocumentPaneDropTargetBottomAsAnchorablePane" ) as FrameworkElement;
      _documentPaneDropTargetTopAsAnchorablePane = GetTemplateChild( "PART_DocumentPaneDropTargetTopAsAnchorablePane" ) as FrameworkElement;
      _documentPaneDropTargetLeftAsAnchorablePane = GetTemplateChild( "PART_DocumentPaneDropTargetLeftAsAnchorablePane" ) as FrameworkElement;
      _documentPaneDropTargetRightAsAnchorablePane = GetTemplateChild( "PART_DocumentPaneDropTargetRightAsAnchorablePane" ) as FrameworkElement;

      _documentPaneFullDropTargetBottom = GetTemplateChild( "PART_DocumentPaneFullDropTargetBottom" ) as FrameworkElement;
      _documentPaneFullDropTargetTop = GetTemplateChild( "PART_DocumentPaneFullDropTargetTop" ) as FrameworkElement;
      _documentPaneFullDropTargetLeft = GetTemplateChild( "PART_DocumentPaneFullDropTargetLeft" ) as FrameworkElement;
      _documentPaneFullDropTargetRight = GetTemplateChild( "PART_DocumentPaneFullDropTargetRight" ) as FrameworkElement;
      _documentPaneFullDropTargetInto = GetTemplateChild( "PART_DocumentPaneFullDropTargetInto" ) as FrameworkElement;

      _previewBox = GetTemplateChild( "PART_PreviewBox" ) as Path;
    }

    protected override void OnClosing( System.ComponentModel.CancelEventArgs e )
    {
      base.OnClosing( e );
    }

    #endregion

    #region Internal Methods

    internal void UpdateThemeResources( Theme oldTheme = null )
    {
      if( oldTheme != null )
      {
        if( oldTheme is DictionaryTheme )
        {
          if( currentThemeResourceDictionary != null )
          {
            Resources.MergedDictionaries.Remove( currentThemeResourceDictionary );
            currentThemeResourceDictionary = null;
          }
        }
        else
        {
          var resourceDictionaryToRemove =
              Resources.MergedDictionaries.FirstOrDefault( r => r.Source == oldTheme.GetResourceUri() );
          if( resourceDictionaryToRemove != null )
            Resources.MergedDictionaries.Remove(
                resourceDictionaryToRemove );
        }
      }

      if( _host.Manager.Theme != null )
      {
        if( _host.Manager.Theme is DictionaryTheme )
        {
          currentThemeResourceDictionary = ( ( DictionaryTheme )_host.Manager.Theme ).ThemeResourceDictionary;
          Resources.MergedDictionaries.Add( currentThemeResourceDictionary );
        }
        else
        {
          Resources.MergedDictionaries.Add( new ResourceDictionary() { Source = _host.Manager.Theme.GetResourceUri() } );
        }
      }
    }

    internal void EnableDropTargets()
    {
      if( _mainCanvasPanel != null )
        _mainCanvasPanel.Visibility = System.Windows.Visibility.Visible;
    }

    internal void HideDropTargets()
    {
      if( _mainCanvasPanel != null )
        _mainCanvasPanel.Visibility = System.Windows.Visibility.Hidden;

    }

    #endregion

    #region Private Methods

    /// <summary>
    /// This method controls the DropTargetInto button of the overlay window.
    /// It checks that only 1 of the defined ContentLayouts can be present on the LayoutDocumentPane or LayoutAnchorablePane.
    /// The combination between the ContentLayout Title and the ContentId is the search key, and has to be unique.
    /// If a floating window is dropped on a LayoutDocumentPane or LayoutAnchorablePane, it checks if one of the containing LayoutContents
    /// is already present on the LayoutDocumentPane or LayoutAnchorablePane. If so, then it will disable the DropTargetInto button.
    /// </summary>
    /// <param name="positionableElement">The given LayoutDocumentPane or LayoutAnchorablePane</param>
    private void SetDropTargetIntoVisibility( ILayoutPositionableElement positionableElement )
    {
      if( positionableElement is LayoutAnchorablePane )
      {
        _anchorablePaneDropTargetInto.Visibility = Visibility.Visible;
      }
      else if( positionableElement is LayoutDocumentPane )
      {
        _documentPaneDropTargetInto.Visibility = Visibility.Visible;
      }

      if( positionableElement == null || _floatingWindow.Model == null || positionableElement.AllowDuplicateContent )
      {
        return;
      }

      // Find all content layouts in the anchorable pane (object to drop on)
      var contentLayoutsOnPositionableElementPane = GetAllLayoutContents( positionableElement );

      // Find all content layouts in the floating window (object to drop)
      var contentLayoutsOnFloatingWindow = GetAllLayoutContents( _floatingWindow.Model );

      // If any of the content layouts is present in the drop area, then disable the DropTargetInto button.
      foreach( var content in contentLayoutsOnFloatingWindow )
      {
        if( !contentLayoutsOnPositionableElementPane.Any( item =>
             item.Title == content.Title &&
             item.ContentId == content.ContentId ) )
        {
          continue;
        }

        if( positionableElement is LayoutAnchorablePane )
        {
          _anchorablePaneDropTargetInto.Visibility = Visibility.Hidden;
        }
        else if( positionableElement is LayoutDocumentPane )
        {
          _documentPaneDropTargetInto.Visibility = Visibility.Hidden;
        }
        break;
      }
    }

    /// <summary>
    /// Find any LayoutDocument or LayoutAnchorable from a given source (e.g. LayoutDocumentPane, LayoutAnchorableFloatingWindow, etc.)
    /// </summary>
    /// <param name="source">The given source to search in</param>
    /// <returns>A list of all LayoutContent's</returns>
    private List<LayoutContent> GetAllLayoutContents( object source )
    {
      var result = new List<LayoutContent>();

      var documentFloatingWindow = source as LayoutDocumentFloatingWindow;
      if( documentFloatingWindow != null )
      {
        foreach( var layoutElement in documentFloatingWindow.Children )
        {
          result.AddRange( GetAllLayoutContents( layoutElement ) );
        }
      }

      var anchorableFloatingWindow = source as LayoutAnchorableFloatingWindow;
      if( anchorableFloatingWindow != null )
      {
        foreach( var layoutElement in anchorableFloatingWindow.Children )
        {
          result.AddRange( GetAllLayoutContents( layoutElement ) );
        }
      }

      var documentPaneGroup = source as LayoutDocumentPaneGroup;
      if( documentPaneGroup != null )
      {
        foreach( var layoutDocumentPane in documentPaneGroup.Children )
        {
          result.AddRange( GetAllLayoutContents( layoutDocumentPane ) );
        }
      }

      var anchorablePaneGroup = source as LayoutAnchorablePaneGroup;
      if( anchorablePaneGroup != null )
      {
        foreach( var layoutDocumentPane in anchorablePaneGroup.Children )
        {
          result.AddRange( GetAllLayoutContents( layoutDocumentPane ) );
        }
      }

      var documentPane = source as LayoutDocumentPane;
      if( documentPane != null )
      {
        foreach( var layoutContent in documentPane.Children )
        {
          result.Add( layoutContent );
        }
      }

      var anchorablePane = source as LayoutAnchorablePane;
      if( anchorablePane != null )
      {
        foreach( var layoutContent in anchorablePane.Children )
        {
          result.Add( layoutContent );
        }
      }

      var document = source as LayoutDocument;
      if( document != null )
      {
        result.Add( document );
      }

      var anchorable = source as LayoutAnchorable;
      if( anchorable != null )
      {
        result.Add( anchorable );
      }

      return result;
    }

    #endregion

    #region IOverlayWindow

    IEnumerable<IDropTarget> IOverlayWindow.GetTargets()
    {
      foreach( var visibleArea in _visibleAreas )
      {
        switch( visibleArea.Type )
        {
          case DropAreaType.DockingManager:
            {
              var dropAreaDockingManager = visibleArea as DropArea<DockingManager>;
              yield return new DockingManagerDropTarget( dropAreaDockingManager.AreaElement, _dockingManagerDropTargetLeft.GetScreenArea(), DropTargetType.DockingManagerDockLeft );
              yield return new DockingManagerDropTarget( dropAreaDockingManager.AreaElement, _dockingManagerDropTargetTop.GetScreenArea(), DropTargetType.DockingManagerDockTop );
              yield return new DockingManagerDropTarget( dropAreaDockingManager.AreaElement, _dockingManagerDropTargetBottom.GetScreenArea(), DropTargetType.DockingManagerDockBottom );
              yield return new DockingManagerDropTarget( dropAreaDockingManager.AreaElement, _dockingManagerDropTargetRight.GetScreenArea(), DropTargetType.DockingManagerDockRight );
            }
            break;
          case DropAreaType.AnchorablePane:
            {
              var dropAreaAnchorablePane = visibleArea as DropArea<LayoutAnchorablePaneControl>;
              yield return new AnchorablePaneDropTarget( dropAreaAnchorablePane.AreaElement, _anchorablePaneDropTargetLeft.GetScreenArea(), DropTargetType.AnchorablePaneDockLeft );
              yield return new AnchorablePaneDropTarget( dropAreaAnchorablePane.AreaElement, _anchorablePaneDropTargetTop.GetScreenArea(), DropTargetType.AnchorablePaneDockTop );
              yield return new AnchorablePaneDropTarget( dropAreaAnchorablePane.AreaElement, _anchorablePaneDropTargetRight.GetScreenArea(), DropTargetType.AnchorablePaneDockRight );
              yield return new AnchorablePaneDropTarget( dropAreaAnchorablePane.AreaElement, _anchorablePaneDropTargetBottom.GetScreenArea(), DropTargetType.AnchorablePaneDockBottom );
              if( _anchorablePaneDropTargetInto.IsVisible )
                yield return new AnchorablePaneDropTarget( dropAreaAnchorablePane.AreaElement, _anchorablePaneDropTargetInto.GetScreenArea(), DropTargetType.AnchorablePaneDockInside );

              var parentPaneModel = dropAreaAnchorablePane.AreaElement.Model as LayoutAnchorablePane;
              LayoutAnchorableTabItem lastAreaTabItem = null;
              foreach( var dropAreaTabItem in dropAreaAnchorablePane.AreaElement.FindVisualChildren<LayoutAnchorableTabItem>() )
              {
                var tabItemModel = dropAreaTabItem.Model as LayoutAnchorable;
                lastAreaTabItem = lastAreaTabItem == null || lastAreaTabItem.GetScreenArea().Right < dropAreaTabItem.GetScreenArea().Right ?
                    dropAreaTabItem : lastAreaTabItem;
                int tabIndex = parentPaneModel.Children.IndexOf( tabItemModel );
                yield return new AnchorablePaneDropTarget( dropAreaAnchorablePane.AreaElement, dropAreaTabItem.GetScreenArea(), DropTargetType.AnchorablePaneDockInside, tabIndex );
              }

              if( lastAreaTabItem != null )
              {
                var lastAreaTabItemScreenArea = lastAreaTabItem.GetScreenArea();
                var newAreaTabItemScreenArea = new Rect( lastAreaTabItemScreenArea.TopRight, new Point( lastAreaTabItemScreenArea.Right + lastAreaTabItemScreenArea.Width, lastAreaTabItemScreenArea.Bottom ) );
                if( newAreaTabItemScreenArea.Right < dropAreaAnchorablePane.AreaElement.GetScreenArea().Right )
                  yield return new AnchorablePaneDropTarget( dropAreaAnchorablePane.AreaElement, newAreaTabItemScreenArea, DropTargetType.AnchorablePaneDockInside, parentPaneModel.Children.Count );
              }

              var dropAreaTitle = dropAreaAnchorablePane.AreaElement.FindVisualChildren<AnchorablePaneTitle>().FirstOrDefault();
              if( dropAreaTitle != null )
                yield return new AnchorablePaneDropTarget( dropAreaAnchorablePane.AreaElement, dropAreaTitle.GetScreenArea(), DropTargetType.AnchorablePaneDockInside );
            }
            break;
          case DropAreaType.DocumentPane:
            {
              bool isDraggingAnchorables = _floatingWindow.Model is LayoutAnchorableFloatingWindow;
              if( isDraggingAnchorables && _gridDocumentPaneFullDropTargets != null )
              {
                var dropAreaDocumentPane = visibleArea as DropArea<LayoutDocumentPaneControl>;
                if( _documentPaneFullDropTargetLeft.IsVisible )
                  yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, _documentPaneFullDropTargetLeft.GetScreenArea(), DropTargetType.DocumentPaneDockLeft );
                if( _documentPaneFullDropTargetTop.IsVisible )
                  yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, _documentPaneFullDropTargetTop.GetScreenArea(), DropTargetType.DocumentPaneDockTop );
                if( _documentPaneFullDropTargetRight.IsVisible )
                  yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, _documentPaneFullDropTargetRight.GetScreenArea(), DropTargetType.DocumentPaneDockRight );
                if( _documentPaneFullDropTargetBottom.IsVisible )
                  yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, _documentPaneFullDropTargetBottom.GetScreenArea(), DropTargetType.DocumentPaneDockBottom );
                if( _documentPaneFullDropTargetInto.IsVisible )
                  yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, _documentPaneFullDropTargetInto.GetScreenArea(), DropTargetType.DocumentPaneDockInside );

                var parentPaneModel = dropAreaDocumentPane.AreaElement.Model as LayoutDocumentPane;
                LayoutDocumentTabItem lastAreaTabItem = null;
                foreach( var dropAreaTabItem in dropAreaDocumentPane.AreaElement.FindVisualChildren<LayoutDocumentTabItem>() )
                {
                  var tabItemModel = dropAreaTabItem.Model;
                  lastAreaTabItem = lastAreaTabItem == null || lastAreaTabItem.GetScreenArea().Right < dropAreaTabItem.GetScreenArea().Right ?
                      dropAreaTabItem : lastAreaTabItem;
                  int tabIndex = parentPaneModel.Children.IndexOf( tabItemModel );
                  yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, dropAreaTabItem.GetScreenArea(), DropTargetType.DocumentPaneDockInside, tabIndex );
                }

                if( lastAreaTabItem != null )
                {
                  var lastAreaTabItemScreenArea = lastAreaTabItem.GetScreenArea();
                  var newAreaTabItemScreenArea = new Rect( lastAreaTabItemScreenArea.TopRight, new Point( lastAreaTabItemScreenArea.Right + lastAreaTabItemScreenArea.Width, lastAreaTabItemScreenArea.Bottom ) );
                  if( newAreaTabItemScreenArea.Right < dropAreaDocumentPane.AreaElement.GetScreenArea().Right )
                    yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, newAreaTabItemScreenArea, DropTargetType.DocumentPaneDockInside, parentPaneModel.Children.Count );
                }

                if( _documentPaneDropTargetLeftAsAnchorablePane.IsVisible )
                  yield return new DocumentPaneDropAsAnchorableTarget( dropAreaDocumentPane.AreaElement, _documentPaneDropTargetLeftAsAnchorablePane.GetScreenArea(), DropTargetType.DocumentPaneDockAsAnchorableLeft );
                if( _documentPaneDropTargetTopAsAnchorablePane.IsVisible )
                  yield return new DocumentPaneDropAsAnchorableTarget( dropAreaDocumentPane.AreaElement, _documentPaneDropTargetTopAsAnchorablePane.GetScreenArea(), DropTargetType.DocumentPaneDockAsAnchorableTop );
                if( _documentPaneDropTargetRightAsAnchorablePane.IsVisible )
                  yield return new DocumentPaneDropAsAnchorableTarget( dropAreaDocumentPane.AreaElement, _documentPaneDropTargetRightAsAnchorablePane.GetScreenArea(), DropTargetType.DocumentPaneDockAsAnchorableRight );
                if( _documentPaneDropTargetBottomAsAnchorablePane.IsVisible )
                  yield return new DocumentPaneDropAsAnchorableTarget( dropAreaDocumentPane.AreaElement, _documentPaneDropTargetBottomAsAnchorablePane.GetScreenArea(), DropTargetType.DocumentPaneDockAsAnchorableBottom );
              }
              else
              {

                var dropAreaDocumentPane = visibleArea as DropArea<LayoutDocumentPaneControl>;
                if( _documentPaneDropTargetLeft.IsVisible )
                  yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, _documentPaneDropTargetLeft.GetScreenArea(), DropTargetType.DocumentPaneDockLeft );
                if( _documentPaneDropTargetTop.IsVisible )
                  yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, _documentPaneDropTargetTop.GetScreenArea(), DropTargetType.DocumentPaneDockTop );
                if( _documentPaneDropTargetRight.IsVisible )
                  yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, _documentPaneDropTargetRight.GetScreenArea(), DropTargetType.DocumentPaneDockRight );
                if( _documentPaneDropTargetBottom.IsVisible )
                  yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, _documentPaneDropTargetBottom.GetScreenArea(), DropTargetType.DocumentPaneDockBottom );
                if( _documentPaneDropTargetInto.IsVisible )
                  yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, _documentPaneDropTargetInto.GetScreenArea(), DropTargetType.DocumentPaneDockInside );

                var parentPaneModel = dropAreaDocumentPane.AreaElement.Model as LayoutDocumentPane;
                LayoutDocumentTabItem lastAreaTabItem = null;
                foreach( var dropAreaTabItem in dropAreaDocumentPane.AreaElement.FindVisualChildren<LayoutDocumentTabItem>() )
                {
                  var tabItemModel = dropAreaTabItem.Model;
                  lastAreaTabItem = lastAreaTabItem == null || lastAreaTabItem.GetScreenArea().Right < dropAreaTabItem.GetScreenArea().Right ?
                      dropAreaTabItem : lastAreaTabItem;
                  int tabIndex = parentPaneModel.Children.IndexOf( tabItemModel );
                  yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, dropAreaTabItem.GetScreenArea(), DropTargetType.DocumentPaneDockInside, tabIndex );
                }

                if( lastAreaTabItem != null )
                {
                  var lastAreaTabItemScreenArea = lastAreaTabItem.GetScreenArea();
                  var newAreaTabItemScreenArea = new Rect( lastAreaTabItemScreenArea.TopRight, new Point( lastAreaTabItemScreenArea.Right + lastAreaTabItemScreenArea.Width, lastAreaTabItemScreenArea.Bottom ) );
                  if( newAreaTabItemScreenArea.Right < dropAreaDocumentPane.AreaElement.GetScreenArea().Right )
                    yield return new DocumentPaneDropTarget( dropAreaDocumentPane.AreaElement, newAreaTabItemScreenArea, DropTargetType.DocumentPaneDockInside, parentPaneModel.Children.Count );
                }
              }
            }
            break;
          case DropAreaType.DocumentPaneGroup:
            {
              var dropAreaDocumentPane = visibleArea as DropArea<LayoutDocumentPaneGroupControl>;
              if( _documentPaneDropTargetInto.IsVisible )
                yield return new DocumentPaneGroupDropTarget( dropAreaDocumentPane.AreaElement, _documentPaneDropTargetInto.GetScreenArea(), DropTargetType.DocumentPaneGroupDockInside );
            }
            break;
        }

      }
      yield break;
    }

    void IOverlayWindow.DragEnter( LayoutFloatingWindowControl floatingWindow )
    {
      _floatingWindow = floatingWindow;
      EnableDropTargets();
    }

    void IOverlayWindow.DragLeave( LayoutFloatingWindowControl floatingWindow )
    {
      Visibility = System.Windows.Visibility.Hidden;
      _floatingWindow = null;
    }

    void IOverlayWindow.DragEnter( IDropArea area )
    {
      var floatingWindowManager = _floatingWindow.Model.Root.Manager;

      _visibleAreas.Add( area );

      FrameworkElement areaElement;
      switch( area.Type )
      {
        case DropAreaType.DockingManager:
          var dropAreaDockingManager = area as DropArea<DockingManager>;
          if( dropAreaDockingManager.AreaElement != floatingWindowManager )
          {
            _visibleAreas.Remove( area );
            return;
          }
          areaElement = _gridDockingManagerDropTargets;
          break;
        case DropAreaType.AnchorablePane:
          areaElement = _gridAnchorablePaneDropTargets;

          var dropAreaAnchorablePaneGroup = area as DropArea<LayoutAnchorablePaneControl>;
          var layoutAnchorablePane = dropAreaAnchorablePaneGroup.AreaElement.Model as LayoutAnchorablePane;
          if( layoutAnchorablePane.Root.Manager != floatingWindowManager )
          {
            _visibleAreas.Remove( area );
            return;
          }
          SetDropTargetIntoVisibility( layoutAnchorablePane );
          break;
        case DropAreaType.DocumentPaneGroup:
          {
            areaElement = _gridDocumentPaneDropTargets;
            var dropAreaDocumentPaneGroup = area as DropArea<LayoutDocumentPaneGroupControl>;
            var layoutDocumentPane = ( dropAreaDocumentPaneGroup.AreaElement.Model as LayoutDocumentPaneGroup ).Children.First() as LayoutDocumentPane;
            var parentDocumentPaneGroup = layoutDocumentPane.Parent as LayoutDocumentPaneGroup;
            if( parentDocumentPaneGroup.Root.Manager != floatingWindowManager )
            {
              _visibleAreas.Remove( area );
              return;
            }
            _documentPaneDropTargetLeft.Visibility = Visibility.Hidden;
            _documentPaneDropTargetRight.Visibility = Visibility.Hidden;
            _documentPaneDropTargetTop.Visibility = Visibility.Hidden;
            _documentPaneDropTargetBottom.Visibility = Visibility.Hidden;
          }
          break;
        case DropAreaType.DocumentPane:
        default:
          {
            bool isDraggingAnchorables = _floatingWindow.Model is LayoutAnchorableFloatingWindow;
            if( isDraggingAnchorables && _gridDocumentPaneFullDropTargets != null )
            {
              areaElement = _gridDocumentPaneFullDropTargets;
              var dropAreaDocumentPaneGroup = area as DropArea<LayoutDocumentPaneControl>;
              var layoutDocumentPane = dropAreaDocumentPaneGroup.AreaElement.Model as LayoutDocumentPane;
              var parentDocumentPaneGroup = layoutDocumentPane.Parent as LayoutDocumentPaneGroup;
              if( layoutDocumentPane.Root.Manager != floatingWindowManager )
              {
                _visibleAreas.Remove( area );
                return;
              }

              SetDropTargetIntoVisibility( layoutDocumentPane );

              if( parentDocumentPaneGroup != null &&
                  parentDocumentPaneGroup.Children.Where( c => c.IsVisible ).Count() > 1 )
              {
                var manager = parentDocumentPaneGroup.Root.Manager;
                if( !manager.AllowMixedOrientation )
                {
                  _documentPaneFullDropTargetLeft.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Horizontal ? Visibility.Visible : Visibility.Hidden;
                  _documentPaneFullDropTargetRight.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Horizontal ? Visibility.Visible : Visibility.Hidden;
                  _documentPaneFullDropTargetTop.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Vertical ? Visibility.Visible : Visibility.Hidden;
                  _documentPaneFullDropTargetBottom.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Vertical ? Visibility.Visible : Visibility.Hidden;
                }
                else
                {
                  _documentPaneFullDropTargetLeft.Visibility = Visibility.Visible;
                  _documentPaneFullDropTargetRight.Visibility = Visibility.Visible;
                  _documentPaneFullDropTargetTop.Visibility = Visibility.Visible;
                  _documentPaneFullDropTargetBottom.Visibility = Visibility.Visible;
                }
              }
              else if( parentDocumentPaneGroup == null &&
                  layoutDocumentPane != null &&
                  layoutDocumentPane.ChildrenCount == 0 )
              {
                _documentPaneFullDropTargetLeft.Visibility = Visibility.Hidden;
                _documentPaneFullDropTargetRight.Visibility = Visibility.Hidden;
                _documentPaneFullDropTargetTop.Visibility = Visibility.Hidden;
                _documentPaneFullDropTargetBottom.Visibility = Visibility.Hidden;
              }
              else
              {
                _documentPaneFullDropTargetLeft.Visibility = Visibility.Visible;
                _documentPaneFullDropTargetRight.Visibility = Visibility.Visible;
                _documentPaneFullDropTargetTop.Visibility = Visibility.Visible;
                _documentPaneFullDropTargetBottom.Visibility = Visibility.Visible;
              }

              if( parentDocumentPaneGroup != null &&
                  parentDocumentPaneGroup.Children.Where( c => c.IsVisible ).Count() > 1 )
              {
                int indexOfDocumentPane = parentDocumentPaneGroup.Children.Where( ch => ch.IsVisible ).ToList().IndexOf( layoutDocumentPane );
                bool isFirstChild = indexOfDocumentPane == 0;
                bool isLastChild = indexOfDocumentPane == parentDocumentPaneGroup.ChildrenCount - 1;

                var manager = parentDocumentPaneGroup.Root.Manager;
                if( !manager.AllowMixedOrientation )
                {
                  _documentPaneDropTargetBottomAsAnchorablePane.Visibility =
                  parentDocumentPaneGroup.Orientation == Orientation.Vertical ?
                      ( isLastChild ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden ) :
                      System.Windows.Visibility.Hidden;
                  _documentPaneDropTargetTopAsAnchorablePane.Visibility =
                      parentDocumentPaneGroup.Orientation == Orientation.Vertical ?
                          ( isFirstChild ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden ) :
                          System.Windows.Visibility.Hidden;

                  _documentPaneDropTargetLeftAsAnchorablePane.Visibility =
                      parentDocumentPaneGroup.Orientation == Orientation.Horizontal ?
                          ( isFirstChild ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden ) :
                          System.Windows.Visibility.Hidden;


                  _documentPaneDropTargetRightAsAnchorablePane.Visibility =
                      parentDocumentPaneGroup.Orientation == Orientation.Horizontal ?
                          ( isLastChild ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden ) :
                          System.Windows.Visibility.Hidden;
                }
                else
                {
                  _documentPaneDropTargetBottomAsAnchorablePane.Visibility = System.Windows.Visibility.Visible;
                  _documentPaneDropTargetLeftAsAnchorablePane.Visibility = System.Windows.Visibility.Visible;
                  _documentPaneDropTargetRightAsAnchorablePane.Visibility = System.Windows.Visibility.Visible;
                  _documentPaneDropTargetTopAsAnchorablePane.Visibility = System.Windows.Visibility.Visible;
                }
              }
              else
              {
                _documentPaneDropTargetBottomAsAnchorablePane.Visibility = System.Windows.Visibility.Visible;
                _documentPaneDropTargetLeftAsAnchorablePane.Visibility = System.Windows.Visibility.Visible;
                _documentPaneDropTargetRightAsAnchorablePane.Visibility = System.Windows.Visibility.Visible;
                _documentPaneDropTargetTopAsAnchorablePane.Visibility = System.Windows.Visibility.Visible;
              }
            }
            else
            {
              areaElement = _gridDocumentPaneDropTargets;
              var dropAreaDocumentPaneGroup = area as DropArea<LayoutDocumentPaneControl>;
              var layoutDocumentPane = dropAreaDocumentPaneGroup.AreaElement.Model as LayoutDocumentPane;
              var parentDocumentPaneGroup = layoutDocumentPane.Parent as LayoutDocumentPaneGroup;
              if( layoutDocumentPane.Root.Manager != floatingWindowManager )
              {
                _visibleAreas.Remove( area );
                return;
              }

              SetDropTargetIntoVisibility( layoutDocumentPane );

              if( parentDocumentPaneGroup != null &&
                  parentDocumentPaneGroup.Children.Where( c => c.IsVisible ).Count() > 1 )
              {
                var manager = parentDocumentPaneGroup.Root.Manager;
                if( !manager.AllowMixedOrientation )
                {
                  _documentPaneDropTargetLeft.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Horizontal ? Visibility.Visible : Visibility.Hidden;
                  _documentPaneDropTargetRight.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Horizontal ? Visibility.Visible : Visibility.Hidden;
                  _documentPaneDropTargetTop.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Vertical ? Visibility.Visible : Visibility.Hidden;
                  _documentPaneDropTargetBottom.Visibility = parentDocumentPaneGroup.Orientation == Orientation.Vertical ? Visibility.Visible : Visibility.Hidden;
                }
                else
                {
                  _documentPaneDropTargetLeft.Visibility = Visibility.Visible;
                  _documentPaneDropTargetRight.Visibility = Visibility.Visible;
                  _documentPaneDropTargetTop.Visibility = Visibility.Visible;
                  _documentPaneDropTargetBottom.Visibility = Visibility.Visible;
                }

              }
              else if( parentDocumentPaneGroup == null &&
                  layoutDocumentPane != null &&
                  layoutDocumentPane.ChildrenCount == 0 )
              {
                _documentPaneDropTargetLeft.Visibility = Visibility.Hidden;
                _documentPaneDropTargetRight.Visibility = Visibility.Hidden;
                _documentPaneDropTargetTop.Visibility = Visibility.Hidden;
                _documentPaneDropTargetBottom.Visibility = Visibility.Hidden;
              }
              else
              {
                _documentPaneDropTargetLeft.Visibility = Visibility.Visible;
                _documentPaneDropTargetRight.Visibility = Visibility.Visible;
                _documentPaneDropTargetTop.Visibility = Visibility.Visible;
                _documentPaneDropTargetBottom.Visibility = Visibility.Visible;
              }
            }
          }
          break;
      }

      Canvas.SetLeft( areaElement, area.DetectionRect.Left - Left );
      Canvas.SetTop( areaElement, area.DetectionRect.Top - Top );
      areaElement.Width = area.DetectionRect.Width;
      areaElement.Height = area.DetectionRect.Height;
      areaElement.Visibility = System.Windows.Visibility.Visible;
    }

    void IOverlayWindow.DragLeave( IDropArea area )
    {
      _visibleAreas.Remove( area );

      FrameworkElement areaElement;
      switch( area.Type )
      {
        case DropAreaType.DockingManager:
          areaElement = _gridDockingManagerDropTargets;
          break;
        case DropAreaType.AnchorablePane:
          areaElement = _gridAnchorablePaneDropTargets;
          break;
        case DropAreaType.DocumentPaneGroup:
          areaElement = _gridDocumentPaneDropTargets;
          break;
        case DropAreaType.DocumentPane:
        default:
          {
            bool isDraggingAnchorables = _floatingWindow.Model is LayoutAnchorableFloatingWindow;
            if( isDraggingAnchorables && _gridDocumentPaneFullDropTargets != null )
              areaElement = _gridDocumentPaneFullDropTargets;
            else
              areaElement = _gridDocumentPaneDropTargets;
          }
          break;
      }

      areaElement.Visibility = System.Windows.Visibility.Hidden;
    }

    void IOverlayWindow.DragEnter( IDropTarget target )
    {
      var previewBoxPath = target.GetPreviewPath( this, _floatingWindow.Model as LayoutFloatingWindow );
      if( previewBoxPath != null )
      {
        _previewBox.Data = previewBoxPath;
        _previewBox.Visibility = System.Windows.Visibility.Visible;
      }
    }

    void IOverlayWindow.DragLeave( IDropTarget target )
    {
      _previewBox.Visibility = System.Windows.Visibility.Hidden;
    }

    void IOverlayWindow.DragDrop( IDropTarget target )
    {
      target.Drop( _floatingWindow.Model as LayoutFloatingWindow );
    }

    #endregion
  }
}
