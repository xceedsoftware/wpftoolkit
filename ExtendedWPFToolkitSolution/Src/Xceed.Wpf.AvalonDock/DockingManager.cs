/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2024 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Markup;
using System.ComponentModel;
using System.Windows.Interop;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Controls;
using System.Windows.Input;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Themes;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Reflection;

namespace Xceed.Wpf.AvalonDock
{
  [ContentProperty( "Layout" )]
  [TemplatePart( Name = "PART_AutoHideArea" )]
  public class DockingManager : Control, IOverlayWindowHost, IWeakEventListener//, ILogicalChildrenContainer
  {
    #region Members

    private ResourceDictionary currentThemeResourceDictionary; // = null
    private AutoHideWindowManager _autoHideWindowManager;
    private FrameworkElement _autohideArea;
    private List<LayoutFloatingWindowControl> _fwList = new List<LayoutFloatingWindowControl>();
    private OverlayWindow _overlayWindow = null;
    private List<IDropArea> _areas = null;
    private bool _insideInternalSetActiveContent = false;
    private List<LayoutItem> _layoutItems = new List<LayoutItem>();
    private bool _suspendLayoutItemCreation = false;
    private DispatcherOperation _collectLayoutItemsOperations = null;
    private NavigatorWindow _navigatorWindow = null;

    internal bool SuspendDocumentsSourceBinding = false;
    internal bool SuspendAnchorablesSourceBinding = false;

    #endregion

    #region Constructors

    static DockingManager()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( DockingManager ), new FrameworkPropertyMetadata( typeof( DockingManager ) ) );
      FocusableProperty.OverrideMetadata( typeof( DockingManager ), new FrameworkPropertyMetadata( false ) );
      HwndSource.DefaultAcquireHwndFocusInMenuMode = false;
    }


    public DockingManager()
    {

#if !VS2008
      Layout = new LayoutRoot() { RootPanel = new LayoutPanel( new LayoutDocumentPaneGroup( new LayoutDocumentPane() ) ) };
#else
          this.SetCurrentValue( DockingManager.LayoutProperty, new LayoutRoot() { RootPanel = new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane())) } );
#endif
      this.Loaded += new RoutedEventHandler( DockingManager_Loaded );
      this.Unloaded += new RoutedEventHandler( DockingManager_Unloaded );
    }

    #endregion

    #region Properties















    #region AllowMovingFloatingWindowWithKeyboard

    public static readonly DependencyProperty AllowMovingFloatingWindowWithKeyboardProperty = DependencyProperty.Register( "AllowMovingFloatingWindowWithKeyboard", typeof( bool ), typeof( DockingManager ),
              new FrameworkPropertyMetadata( ( bool )false ) );

    public bool AllowMovingFloatingWindowWithKeyboard
    {
      get
      {
        return ( bool )GetValue( AllowMovingFloatingWindowWithKeyboardProperty );
      }
      private set
      {
        SetValue( AllowMovingFloatingWindowWithKeyboardProperty, value );
      }
    }

    #endregion

    #region Layout

    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register( "Layout", typeof( LayoutRoot ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnLayoutChanged ), new CoerceValueCallback( CoerceLayoutValue ) ) );

    public LayoutRoot Layout
    {
      get
      {
        return ( LayoutRoot )GetValue( LayoutProperty );
      }
      set
      {
        SetValue( LayoutProperty, value );
      }
    }

    private static object CoerceLayoutValue( DependencyObject d, object value )
    {
      if( value == null )
        return new LayoutRoot() { RootPanel = new LayoutPanel( new LayoutDocumentPaneGroup( new LayoutDocumentPane() ) ) };

      ( ( DockingManager )d ).OnLayoutChanging( value as LayoutRoot );

      return value;
    }

    private static void OnLayoutChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLayoutChanged( e.OldValue as LayoutRoot, e.NewValue as LayoutRoot );
    }

    protected virtual void OnLayoutChanged( LayoutRoot oldLayout, LayoutRoot newLayout )
    {
      if( oldLayout != null )
      {
        oldLayout.PropertyChanged -= new PropertyChangedEventHandler( OnLayoutRootPropertyChanged );
        oldLayout.Updated -= new EventHandler( OnLayoutRootUpdated );
      }

      foreach( var fwc in _fwList.ToArray() )
      {
        fwc.KeepContentVisibleOnClose = true;
        fwc.InternalClose();
      }

      _fwList.Clear();

      DetachDocumentsSource( oldLayout, DocumentsSource );
      DetachAnchorablesSource( oldLayout, AnchorablesSource );

      if( oldLayout != null &&
          oldLayout.Manager == this )
        oldLayout.Manager = null;

      ClearLogicalChildrenList();
      DetachLayoutItems();

      Layout.Manager = this;

      AttachLayoutItems();
      AttachDocumentsSource( newLayout, DocumentsSource );
      AttachAnchorablesSource( newLayout, AnchorablesSource );

      if( IsLoaded )
      {
        LayoutRootPanel = CreateUIElementForModel( Layout.RootPanel ) as LayoutPanelControl;
        LeftSidePanel = CreateUIElementForModel( Layout.LeftSide ) as LayoutAnchorSideControl;
        TopSidePanel = CreateUIElementForModel( Layout.TopSide ) as LayoutAnchorSideControl;
        RightSidePanel = CreateUIElementForModel( Layout.RightSide ) as LayoutAnchorSideControl;
        BottomSidePanel = CreateUIElementForModel( Layout.BottomSide ) as LayoutAnchorSideControl;

        foreach( var fw in Layout.FloatingWindows.ToArray() )
        {
          if( fw.IsValid )
            _fwList.Add( CreateUIElementForModel( fw ) as LayoutFloatingWindowControl );
        }

        foreach( var fw in _fwList )
        {
          //fw.Owner = Window.GetWindow(this);
          //fw.SetParentToMainWindowOf(this);
        }
      }


      if( newLayout != null )
      {
        newLayout.PropertyChanged += new PropertyChangedEventHandler( OnLayoutRootPropertyChanged );
        newLayout.Updated += new EventHandler( OnLayoutRootUpdated );
      }

      if( LayoutChanged != null )
        LayoutChanged( this, EventArgs.Empty );

      //if (Layout != null)
      //    Layout.CollectGarbage();

      CommandManager.InvalidateRequerySuggested();
    }

    #endregion

    #region LayoutUpdateStrategy

    public static readonly DependencyProperty LayoutUpdateStrategyProperty = DependencyProperty.Register( "LayoutUpdateStrategy", typeof( ILayoutUpdateStrategy ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ILayoutUpdateStrategy )null ) );

    public ILayoutUpdateStrategy LayoutUpdateStrategy
    {
      get
      {
        return ( ILayoutUpdateStrategy )GetValue( LayoutUpdateStrategyProperty );
      }
      set
      {
        SetValue( LayoutUpdateStrategyProperty, value );
      }
    }

    #endregion

    #region AnchorSideTemplate

    public static readonly DependencyProperty AnchorSideTemplateProperty = DependencyProperty.Register( "AnchorSideTemplate", typeof( ControlTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ControlTemplate )null ) );

    public ControlTemplate AnchorSideTemplate
    {
      get
      {
        return ( ControlTemplate )GetValue( AnchorSideTemplateProperty );
      }
      set
      {
        SetValue( AnchorSideTemplateProperty, value );
      }
    }

    #endregion

    #region AnchorGroupTemplate

    public static readonly DependencyProperty AnchorGroupTemplateProperty = DependencyProperty.Register( "AnchorGroupTemplate", typeof( ControlTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ControlTemplate )null ) );

    public ControlTemplate AnchorGroupTemplate
    {
      get
      {
        return ( ControlTemplate )GetValue( AnchorGroupTemplateProperty );
      }
      set
      {
        SetValue( AnchorGroupTemplateProperty, value );
      }
    }

    #endregion

    #region AnchorTemplate

    public static readonly DependencyProperty AnchorTemplateProperty = DependencyProperty.Register( "AnchorTemplate", typeof( ControlTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ControlTemplate )null ) );

    public ControlTemplate AnchorTemplate
    {
      get
      {
        return ( ControlTemplate )GetValue( AnchorTemplateProperty );
      }
      set
      {
        SetValue( AnchorTemplateProperty, value );
      }
    }

    #endregion

    #region DocumentPaneControlStyle

    public static readonly DependencyProperty DocumentPaneControlStyleProperty = DependencyProperty.Register( "DocumentPaneControlStyle", typeof( Style ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( Style )null, new PropertyChangedCallback( OnDocumentPaneControlStyleChanged ) ) );

    public Style DocumentPaneControlStyle
    {
      get
      {
        return ( Style )GetValue( DocumentPaneControlStyleProperty );
      }
      set
      {
        SetValue( DocumentPaneControlStyleProperty, value );
      }
    }

    private static void OnDocumentPaneControlStyleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentPaneControlStyleChanged( e );
    }

    protected virtual void OnDocumentPaneControlStyleChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    #endregion

    #region AnchorablePaneControlStyle

    public static readonly DependencyProperty AnchorablePaneControlStyleProperty = DependencyProperty.Register( "AnchorablePaneControlStyle", typeof( Style ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( Style )null, new PropertyChangedCallback( OnAnchorablePaneControlStyleChanged ) ) );

    public Style AnchorablePaneControlStyle
    {
      get
      {
        return ( Style )GetValue( AnchorablePaneControlStyleProperty );
      }
      set
      {
        SetValue( AnchorablePaneControlStyleProperty, value );
      }
    }

    private static void OnAnchorablePaneControlStyleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAnchorablePaneControlStyleChanged( e );
    }

    protected virtual void OnAnchorablePaneControlStyleChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    #endregion

    #region DocumentHeaderTemplate

    public static readonly DependencyProperty DocumentHeaderTemplateProperty = DependencyProperty.Register( "DocumentHeaderTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null, new PropertyChangedCallback( OnDocumentHeaderTemplateChanged ), new CoerceValueCallback( CoerceDocumentHeaderTemplateValue ) ) );

    public DataTemplate DocumentHeaderTemplate
    {
      get
      {
        return ( DataTemplate )GetValue( DocumentHeaderTemplateProperty );
      }
      set
      {
        SetValue( DocumentHeaderTemplateProperty, value );
      }
    }

    private static void OnDocumentHeaderTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentHeaderTemplateChanged( e );
    }

    protected virtual void OnDocumentHeaderTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceDocumentHeaderTemplateValue( DependencyObject d, object value )
    {
      if( value != null &&
          d.GetValue( DocumentHeaderTemplateSelectorProperty ) != null )
        return null;
      return value;
    }

    #endregion

    #region DocumentHeaderTemplateSelector

    public static readonly DependencyProperty DocumentHeaderTemplateSelectorProperty = DependencyProperty.Register( "DocumentHeaderTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null, new PropertyChangedCallback( OnDocumentHeaderTemplateSelectorChanged ), new CoerceValueCallback( CoerceDocumentHeaderTemplateSelectorValue ) ) );

    public DataTemplateSelector DocumentHeaderTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )GetValue( DocumentHeaderTemplateSelectorProperty );
      }
      set
      {
        SetValue( DocumentHeaderTemplateSelectorProperty, value );
      }
    }

    private static void OnDocumentHeaderTemplateSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentHeaderTemplateSelectorChanged( e );
    }

    protected virtual void OnDocumentHeaderTemplateSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.NewValue != null &&
          DocumentHeaderTemplate != null )
        DocumentHeaderTemplate = null;

      if( DocumentPaneMenuItemHeaderTemplateSelector == null )
        DocumentPaneMenuItemHeaderTemplateSelector = DocumentHeaderTemplateSelector;

    }

    private static object CoerceDocumentHeaderTemplateSelectorValue( DependencyObject d, object value )
    {
      return value;
    }

    #endregion

    #region DocumentTitleTemplate

    public static readonly DependencyProperty DocumentTitleTemplateProperty = DependencyProperty.Register( "DocumentTitleTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null, new PropertyChangedCallback( OnDocumentTitleTemplateChanged ), new CoerceValueCallback( CoerceDocumentTitleTemplateValue ) ) );

    public DataTemplate DocumentTitleTemplate
    {
      get
      {
        return ( DataTemplate )GetValue( DocumentTitleTemplateProperty );
      }
      set
      {
        SetValue( DocumentTitleTemplateProperty, value );
      }
    }

    private static void OnDocumentTitleTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentTitleTemplateChanged( e );
    }

    protected virtual void OnDocumentTitleTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceDocumentTitleTemplateValue( DependencyObject d, object value )
    {
      if( value != null &&
          d.GetValue( DocumentTitleTemplateSelectorProperty ) != null )
        return null;

      return value;
    }

    #endregion

    #region DocumentTitleTemplateSelector

    public static readonly DependencyProperty DocumentTitleTemplateSelectorProperty = DependencyProperty.Register( "DocumentTitleTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null, new PropertyChangedCallback( OnDocumentTitleTemplateSelectorChanged ), new CoerceValueCallback( CoerceDocumentTitleTemplateSelectorValue ) ) );

    public DataTemplateSelector DocumentTitleTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )GetValue( DocumentTitleTemplateSelectorProperty );
      }
      set
      {
        SetValue( DocumentTitleTemplateSelectorProperty, value );
      }
    }

    private static void OnDocumentTitleTemplateSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentTitleTemplateSelectorChanged( e );
    }

    protected virtual void OnDocumentTitleTemplateSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.NewValue != null )
        DocumentTitleTemplate = null;
    }

    private static object CoerceDocumentTitleTemplateSelectorValue( DependencyObject d, object value )
    {
      return value;
    }

    #endregion

    #region AnchorableTitleTemplate

    public static readonly DependencyProperty AnchorableTitleTemplateProperty = DependencyProperty.Register( "AnchorableTitleTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null, new PropertyChangedCallback( OnAnchorableTitleTemplateChanged ), new CoerceValueCallback( CoerceAnchorableTitleTemplateValue ) ) );

    public DataTemplate AnchorableTitleTemplate
    {
      get
      {
        return ( DataTemplate )GetValue( AnchorableTitleTemplateProperty );
      }
      set
      {
        SetValue( AnchorableTitleTemplateProperty, value );
      }
    }

    private static void OnAnchorableTitleTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAnchorableTitleTemplateChanged( e );
    }

    protected virtual void OnAnchorableTitleTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceAnchorableTitleTemplateValue( DependencyObject d, object value )
    {
      if( value != null &&
          d.GetValue( AnchorableTitleTemplateSelectorProperty ) != null )
        return null;
      return value;
    }

    #endregion

    #region AnchorableTitleTemplateSelector

    public static readonly DependencyProperty AnchorableTitleTemplateSelectorProperty = DependencyProperty.Register( "AnchorableTitleTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null, new PropertyChangedCallback( OnAnchorableTitleTemplateSelectorChanged ) ) );

    public DataTemplateSelector AnchorableTitleTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )GetValue( AnchorableTitleTemplateSelectorProperty );
      }
      set
      {
        SetValue( AnchorableTitleTemplateSelectorProperty, value );
      }
    }

    private static void OnAnchorableTitleTemplateSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAnchorableTitleTemplateSelectorChanged( e );
    }

    protected virtual void OnAnchorableTitleTemplateSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.NewValue != null &&
          AnchorableTitleTemplate != null )
        AnchorableTitleTemplate = null;
    }

    #endregion

    #region AnchorableHeaderTemplate

    public static readonly DependencyProperty AnchorableHeaderTemplateProperty = DependencyProperty.Register( "AnchorableHeaderTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null, new PropertyChangedCallback( OnAnchorableHeaderTemplateChanged ), new CoerceValueCallback( CoerceAnchorableHeaderTemplateValue ) ) );

    public DataTemplate AnchorableHeaderTemplate
    {
      get
      {
        return ( DataTemplate )GetValue( AnchorableHeaderTemplateProperty );
      }
      set
      {
        SetValue( AnchorableHeaderTemplateProperty, value );
      }
    }

    private static void OnAnchorableHeaderTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAnchorableHeaderTemplateChanged( e );
    }

    protected virtual void OnAnchorableHeaderTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceAnchorableHeaderTemplateValue( DependencyObject d, object value )
    {
      if( value != null &&
          d.GetValue( AnchorableHeaderTemplateSelectorProperty ) != null )
        return null;

      return value;
    }

    #endregion

    #region AnchorableHeaderTemplateSelector

    public static readonly DependencyProperty AnchorableHeaderTemplateSelectorProperty = DependencyProperty.Register( "AnchorableHeaderTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null, new PropertyChangedCallback( OnAnchorableHeaderTemplateSelectorChanged ) ) );

    public DataTemplateSelector AnchorableHeaderTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )GetValue( AnchorableHeaderTemplateSelectorProperty );
      }
      set
      {
        SetValue( AnchorableHeaderTemplateSelectorProperty, value );
      }
    }

    private static void OnAnchorableHeaderTemplateSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAnchorableHeaderTemplateSelectorChanged( e );
    }

    protected virtual void OnAnchorableHeaderTemplateSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.NewValue != null )
        AnchorableHeaderTemplate = null;
    }

    #endregion

    #region LayoutRootPanel

    public static readonly DependencyProperty LayoutRootPanelProperty = DependencyProperty.Register( "LayoutRootPanel", typeof( LayoutPanelControl ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( LayoutPanelControl )null, new PropertyChangedCallback( OnLayoutRootPanelChanged ) ) );

    public LayoutPanelControl LayoutRootPanel
    {
      get
      {
        return ( LayoutPanelControl )GetValue( LayoutRootPanelProperty );
      }
      set
      {
        SetValue( LayoutRootPanelProperty, value );
      }
    }

    private static void OnLayoutRootPanelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLayoutRootPanelChanged( e );
    }

    protected virtual void OnLayoutRootPanelChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.OldValue != null )
        InternalRemoveLogicalChild( e.OldValue );
      if( e.NewValue != null )
        InternalAddLogicalChild( e.NewValue );
    }

    #endregion

    #region RightSidePanel

    public static readonly DependencyProperty RightSidePanelProperty = DependencyProperty.Register( "RightSidePanel", typeof( LayoutAnchorSideControl ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( LayoutAnchorSideControl )null, new PropertyChangedCallback( OnRightSidePanelChanged ) ) );

    public LayoutAnchorSideControl RightSidePanel
    {
      get
      {
        return ( LayoutAnchorSideControl )GetValue( RightSidePanelProperty );
      }
      set
      {
        SetValue( RightSidePanelProperty, value );
      }
    }

    private static void OnRightSidePanelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnRightSidePanelChanged( e );
    }

    protected virtual void OnRightSidePanelChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.OldValue != null )
        InternalRemoveLogicalChild( e.OldValue );
      if( e.NewValue != null )
        InternalAddLogicalChild( e.NewValue );
    }

    #endregion

    #region LeftSidePanel

    public static readonly DependencyProperty LeftSidePanelProperty = DependencyProperty.Register( "LeftSidePanel", typeof( LayoutAnchorSideControl ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( LayoutAnchorSideControl )null, new PropertyChangedCallback( OnLeftSidePanelChanged ) ) );

    public LayoutAnchorSideControl LeftSidePanel
    {
      get
      {
        return ( LayoutAnchorSideControl )GetValue( LeftSidePanelProperty );
      }
      set
      {
        SetValue( LeftSidePanelProperty, value );
      }
    }

    private static void OnLeftSidePanelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLeftSidePanelChanged( e );
    }

    protected virtual void OnLeftSidePanelChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.OldValue != null )
        InternalRemoveLogicalChild( e.OldValue );
      if( e.NewValue != null )
        InternalAddLogicalChild( e.NewValue );
    }

    #endregion

    #region TopSidePanel

    public static readonly DependencyProperty TopSidePanelProperty = DependencyProperty.Register( "TopSidePanel", typeof( LayoutAnchorSideControl ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( LayoutAnchorSideControl )null, new PropertyChangedCallback( OnTopSidePanelChanged ) ) );

    public LayoutAnchorSideControl TopSidePanel
    {
      get
      {
        return ( LayoutAnchorSideControl )GetValue( TopSidePanelProperty );
      }
      set
      {
        SetValue( TopSidePanelProperty, value );
      }
    }

    private static void OnTopSidePanelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnTopSidePanelChanged( e );
    }

    protected virtual void OnTopSidePanelChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.OldValue != null )
        InternalRemoveLogicalChild( e.OldValue );
      if( e.NewValue != null )
        InternalAddLogicalChild( e.NewValue );
    }

    #endregion

    #region BottomSidePanel

    public static readonly DependencyProperty BottomSidePanelProperty = DependencyProperty.Register( "BottomSidePanel", typeof( LayoutAnchorSideControl ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( LayoutAnchorSideControl )null, new PropertyChangedCallback( OnBottomSidePanelChanged ) ) );

    public LayoutAnchorSideControl BottomSidePanel
    {
      get
      {
        return ( LayoutAnchorSideControl )GetValue( BottomSidePanelProperty );
      }
      set
      {
        SetValue( BottomSidePanelProperty, value );
      }
    }

    private static void OnBottomSidePanelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnBottomSidePanelChanged( e );
    }

    protected virtual void OnBottomSidePanelChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.OldValue != null )
        InternalRemoveLogicalChild( e.OldValue );
      if( e.NewValue != null )
        InternalAddLogicalChild( e.NewValue );
    }

    #endregion

    #region LogicalChildren

    List<WeakReference> _logicalChildren = new List<WeakReference>();

    protected override System.Collections.IEnumerator LogicalChildren
    {
      get
      {
        return _logicalChildren.Select( ch => ch.GetValueOrDefault<object>() ).GetEnumerator();
      }
    }

    public System.Collections.IEnumerator LogicalChildrenPublic
    {
      get
      {
        return this.LogicalChildren;
      }
    }


    internal void InternalAddLogicalChild( object element )
    {
#if DEBUG
      if( _logicalChildren.Select( ch => ch.GetValueOrDefault<object>() ).Contains( element ) )
        new InvalidOperationException();
#endif
      if( _logicalChildren.Select( ch => ch.GetValueOrDefault<object>() ).Contains( element ) )
        return;

      _logicalChildren.Add( new WeakReference( element ) );
      AddLogicalChild( element );
    }

    internal void InternalRemoveLogicalChild( object element )
    {
      var wrToRemove = _logicalChildren.FirstOrDefault( ch => ch.GetValueOrDefault<object>() == element );
      if( wrToRemove != null )
      {
        _logicalChildren.Remove( wrToRemove );
      }
      this.RemoveLogicalChild( element );
    }

    private void ClearLogicalChildrenList()
    {
      foreach( var child in _logicalChildren.Select( ch => ch.GetValueOrDefault<object>() ).ToArray() )
      {
        this.RemoveLogicalChild( child );
      }
      _logicalChildren.Clear();
    }

    #endregion

    #region AutoHideWindow

    private static readonly DependencyPropertyKey AutoHideWindowPropertyKey = DependencyProperty.RegisterReadOnly( "AutoHideWindow", typeof( LayoutAutoHideWindowControl ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( LayoutAutoHideWindowControl )null, new PropertyChangedCallback( OnAutoHideWindowChanged ) ) );

    public static readonly DependencyProperty AutoHideWindowProperty = AutoHideWindowPropertyKey.DependencyProperty;

    public LayoutAutoHideWindowControl AutoHideWindow
    {
      get
      {
        return ( LayoutAutoHideWindowControl )GetValue( AutoHideWindowProperty );
      }
    }

    protected void SetAutoHideWindow( LayoutAutoHideWindowControl value )
    {
      SetValue( AutoHideWindowPropertyKey, value );
    }

    private static void OnAutoHideWindowChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAutoHideWindowChanged( e );
    }

    protected virtual void OnAutoHideWindowChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.OldValue != null )
        InternalRemoveLogicalChild( e.OldValue );
      if( e.NewValue != null )
        InternalAddLogicalChild( e.NewValue );

    }

    #endregion

    #region AutoHideWindowClosingTimer

    public static readonly DependencyProperty AutoHideWindowClosingTimerProperty = DependencyProperty.Register( "AutoHideWindowClosingTimer", typeof( int ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( int )1500, new PropertyChangedCallback( OnAutoHideWindowClosingTimerChanged ), new CoerceValueCallback( CoerceAutoHideWindowClosingTimer ) ) );


    public int AutoHideWindowClosingTimer
    {
      get
      {
        return ( int )GetValue( AutoHideWindowClosingTimerProperty );
      }
      set
      {
        SetValue( AutoHideWindowClosingTimerProperty, value );
      }
    }

    private static void OnAutoHideWindowClosingTimerChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAutoHideWindowClosingTimerChanged( e );
    }

    protected virtual void OnAutoHideWindowClosingTimerChanged( DependencyPropertyChangedEventArgs e )
    {
      if( _autoHideWindowManager != null )
      {
        _autoHideWindowManager.UpdateCloseTimerInterval( ( int )e.NewValue );
      }
    }

    private static object CoerceAutoHideWindowClosingTimer( DependencyObject d, object value )
    {
      if( ( ( int )value ) >= 0 )
        return value;

      return 1500;
    }

    #endregion

    #region Floating Windows

    public IEnumerable<LayoutFloatingWindowControl> FloatingWindows
    {
      get
      {
        return _fwList;
      }
    }

    #endregion

    #region LayoutItemTemplate

    public static readonly DependencyProperty LayoutItemTemplateProperty = DependencyProperty.Register( "LayoutItemTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null, new PropertyChangedCallback( OnLayoutItemTemplateChanged ) ) );

    public DataTemplate LayoutItemTemplate
    {
      get
      {
        return ( DataTemplate )GetValue( LayoutItemTemplateProperty );
      }
      set
      {
        SetValue( LayoutItemTemplateProperty, value );
      }
    }

    private static void OnLayoutItemTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLayoutItemTemplateChanged( e );
    }

    protected virtual void OnLayoutItemTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    #endregion

    #region LayoutItemTemplateSelector

    public static readonly DependencyProperty LayoutItemTemplateSelectorProperty = DependencyProperty.Register( "LayoutItemTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null, new PropertyChangedCallback( OnLayoutItemTemplateSelectorChanged ) ) );

    public DataTemplateSelector LayoutItemTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )GetValue( LayoutItemTemplateSelectorProperty );
      }
      set
      {
        SetValue( LayoutItemTemplateSelectorProperty, value );
      }
    }

    private static void OnLayoutItemTemplateSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLayoutItemTemplateSelectorChanged( e );
    }

    protected virtual void OnLayoutItemTemplateSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    #endregion

    #region DocumentsSource

    public static readonly DependencyProperty DocumentsSourceProperty = DependencyProperty.Register( "DocumentsSource", typeof( IEnumerable ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( IEnumerable )null, new PropertyChangedCallback( OnDocumentsSourceChanged ) ) );

    public IEnumerable DocumentsSource
    {
      get
      {
        return ( IEnumerable )GetValue( DocumentsSourceProperty );
      }
      set
      {
        SetValue( DocumentsSourceProperty, value );
      }
    }

    private static void OnDocumentsSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentsSourceChanged( e );
    }

    protected virtual void OnDocumentsSourceChanged( DependencyPropertyChangedEventArgs e )
    {
      DetachDocumentsSource( Layout, e.OldValue as IEnumerable );
      AttachDocumentsSource( Layout, e.NewValue as IEnumerable );
    }

    #endregion

    #region DocumentContextMenu

    public static readonly DependencyProperty DocumentContextMenuProperty = DependencyProperty.Register( "DocumentContextMenu", typeof( ContextMenu ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ContextMenu )null ) );

    public ContextMenu DocumentContextMenu
    {
      get
      {
        return ( ContextMenu )GetValue( DocumentContextMenuProperty );
      }
      set
      {
        SetValue( DocumentContextMenuProperty, value );
      }
    }

    #endregion

    #region AnchorablesSource

    public static readonly DependencyProperty AnchorablesSourceProperty = DependencyProperty.Register( "AnchorablesSource", typeof( IEnumerable ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( IEnumerable )null, new PropertyChangedCallback( OnAnchorablesSourceChanged ) ) );

    public IEnumerable AnchorablesSource
    {
      get
      {
        return ( IEnumerable )GetValue( AnchorablesSourceProperty );
      }
      set
      {
        SetValue( AnchorablesSourceProperty, value );
      }
    }

    private static void OnAnchorablesSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAnchorablesSourceChanged( e );
    }

    protected virtual void OnAnchorablesSourceChanged( DependencyPropertyChangedEventArgs e )
    {
      DetachAnchorablesSource( Layout, e.OldValue as IEnumerable );
      AttachAnchorablesSource( Layout, e.NewValue as IEnumerable );
    }



    #endregion

    #region ActiveContent

    public static readonly DependencyProperty ActiveContentProperty = DependencyProperty.Register( "ActiveContent", typeof( object ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( object )null, new PropertyChangedCallback( OnActiveContentChanged ) ) );

    public object ActiveContent
    {
      get
      {
        return ( object )GetValue( ActiveContentProperty );
      }
      set
      {
        SetValue( ActiveContentProperty, value );
      }
    }

    private static void OnActiveContentChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).InternalSetActiveContent( e.NewValue );
      ( ( DockingManager )d ).OnActiveContentChanged( e );
    }

    protected virtual void OnActiveContentChanged( DependencyPropertyChangedEventArgs e )
    {
      if( ActiveContentChanged != null )
        ActiveContentChanged( this, EventArgs.Empty );
    }

    #endregion

    #region AnchorableContextMenu

    public static readonly DependencyProperty AnchorableContextMenuProperty = DependencyProperty.Register( "AnchorableContextMenu", typeof( ContextMenu ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ContextMenu )null ) );

    public ContextMenu AnchorableContextMenu
    {
      get
      {
        return ( ContextMenu )GetValue( AnchorableContextMenuProperty );
      }
      set
      {
        SetValue( AnchorableContextMenuProperty, value );
      }
    }

    #endregion

    #region Theme

    public static readonly DependencyProperty ThemeProperty = DependencyProperty.Register( "Theme", typeof( Theme ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnThemeChanged ) ) );

    public Theme Theme
    {
      get
      {
        return ( Theme )GetValue( ThemeProperty );
      }
      set
      {
        SetValue( ThemeProperty, value );
      }
    }

    private static void OnThemeChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnThemeChanged( e );
    }

    protected virtual void OnThemeChanged( DependencyPropertyChangedEventArgs e )
    {
      var oldTheme = e.OldValue as Theme;
      var newTheme = e.NewValue as Theme;
      var resources = this.Resources;
      if( oldTheme != null )
      {
        if( oldTheme is DictionaryTheme )
        {
          if( currentThemeResourceDictionary != null )
          {
            resources.MergedDictionaries.Remove( currentThemeResourceDictionary );
            currentThemeResourceDictionary = null;
          }
        }
        else
        {
          var resourceDictionaryToRemove =
             resources.MergedDictionaries.FirstOrDefault( r => r.Source == oldTheme.GetResourceUri() );
          if( resourceDictionaryToRemove != null )
            resources.MergedDictionaries.Remove(
                resourceDictionaryToRemove );
        }
      }

      if( newTheme != null )
      {
        if( newTheme is DictionaryTheme )
        {
          currentThemeResourceDictionary = ( ( DictionaryTheme )newTheme ).ThemeResourceDictionary;
          resources.MergedDictionaries.Add( currentThemeResourceDictionary );
        }
        else
        {
          resources.MergedDictionaries.Add( new ResourceDictionary() { Source = newTheme.GetResourceUri() } );
        }
      }

      foreach( var fwc in _fwList )
        fwc.UpdateThemeResources( oldTheme );

      if( _navigatorWindow != null )
        _navigatorWindow.UpdateThemeResources( oldTheme );

      if( _overlayWindow != null )
        _overlayWindow.UpdateThemeResources( oldTheme );
    }

    #endregion

    #region GridSplitterWidth

    public static readonly DependencyProperty GridSplitterWidthProperty = DependencyProperty.Register( "GridSplitterWidth", typeof( double ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( double )6.0 ) );

    public double GridSplitterWidth
    {
      get
      {
        return ( double )GetValue( GridSplitterWidthProperty );
      }
      set
      {
        SetValue( GridSplitterWidthProperty, value );
      }
    }

    #endregion

    #region GridSplitterHeight

    public static readonly DependencyProperty GridSplitterHeightProperty = DependencyProperty.Register( "GridSplitterHeight", typeof( double ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( double )6.0 ) );

    public double GridSplitterHeight
    {
      get
      {
        return ( double )GetValue( GridSplitterHeightProperty );
      }
      set
      {
        SetValue( GridSplitterHeightProperty, value );
      }
    }

    #endregion

    #region DocumentPaneMenuItemHeaderTemplate

    public static readonly DependencyProperty DocumentPaneMenuItemHeaderTemplateProperty = DependencyProperty.Register( "DocumentPaneMenuItemHeaderTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null, new PropertyChangedCallback( OnDocumentPaneMenuItemHeaderTemplateChanged ), new CoerceValueCallback( CoerceDocumentPaneMenuItemHeaderTemplateValue ) ) );

    public DataTemplate DocumentPaneMenuItemHeaderTemplate
    {
      get
      {
        return ( DataTemplate )GetValue( DocumentPaneMenuItemHeaderTemplateProperty );
      }
      set
      {
        SetValue( DocumentPaneMenuItemHeaderTemplateProperty, value );
      }
    }

    private static void OnDocumentPaneMenuItemHeaderTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentPaneMenuItemHeaderTemplateChanged( e );
    }

    protected virtual void OnDocumentPaneMenuItemHeaderTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceDocumentPaneMenuItemHeaderTemplateValue( DependencyObject d, object value )
    {
      if( value != null &&
          d.GetValue( DocumentPaneMenuItemHeaderTemplateSelectorProperty ) != null )
        return null;
      if( value == null )
        return d.GetValue( DocumentHeaderTemplateProperty );

      return value;
    }

    #endregion

    #region DocumentPaneMenuItemHeaderTemplateSelector

    public static readonly DependencyProperty DocumentPaneMenuItemHeaderTemplateSelectorProperty = DependencyProperty.Register( "DocumentPaneMenuItemHeaderTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null, new PropertyChangedCallback( OnDocumentPaneMenuItemHeaderTemplateSelectorChanged ), new CoerceValueCallback( CoerceDocumentPaneMenuItemHeaderTemplateSelectorValue ) ) );

    public DataTemplateSelector DocumentPaneMenuItemHeaderTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )GetValue( DocumentPaneMenuItemHeaderTemplateSelectorProperty );
      }
      set
      {
        SetValue( DocumentPaneMenuItemHeaderTemplateSelectorProperty, value );
      }
    }

    private static void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentPaneMenuItemHeaderTemplateSelectorChanged( e );
    }

    protected virtual void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.NewValue != null &&
          DocumentPaneMenuItemHeaderTemplate != null )
        DocumentPaneMenuItemHeaderTemplate = null;

    }

    private static object CoerceDocumentPaneMenuItemHeaderTemplateSelectorValue( DependencyObject d, object value )
    {
      return value;
    }

    #endregion

    #region IconContentTemplate

    public static readonly DependencyProperty IconContentTemplateProperty = DependencyProperty.Register( "IconContentTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null ) );

    public DataTemplate IconContentTemplate
    {
      get
      {
        return ( DataTemplate )GetValue( IconContentTemplateProperty );
      }
      set
      {
        SetValue( IconContentTemplateProperty, value );
      }
    }

    #endregion

    #region IconContentTemplateSelector

    public static readonly DependencyProperty IconContentTemplateSelectorProperty = DependencyProperty.Register( "IconContentTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null ) );

    public DataTemplateSelector IconContentTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )GetValue( IconContentTemplateSelectorProperty );
      }
      set
      {
        SetValue( IconContentTemplateSelectorProperty, value );
      }
    }

    #endregion

    #region LayoutItemContainerStyle

    public static readonly DependencyProperty LayoutItemContainerStyleProperty = DependencyProperty.Register( "LayoutItemContainerStyle", typeof( Style ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( Style )null, new PropertyChangedCallback( OnLayoutItemContainerStyleChanged ) ) );

    public Style LayoutItemContainerStyle
    {
      get
      {
        return ( Style )GetValue( LayoutItemContainerStyleProperty );
      }
      set
      {
        SetValue( LayoutItemContainerStyleProperty, value );
      }
    }

    private static void OnLayoutItemContainerStyleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLayoutItemContainerStyleChanged( e );
    }

    protected virtual void OnLayoutItemContainerStyleChanged( DependencyPropertyChangedEventArgs e )
    {
      AttachLayoutItems();
    }

    #endregion

    #region LayoutItemContainerStyleSelector

    public static readonly DependencyProperty LayoutItemContainerStyleSelectorProperty = DependencyProperty.Register( "LayoutItemContainerStyleSelector", typeof( StyleSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( StyleSelector )null, new PropertyChangedCallback( OnLayoutItemContainerStyleSelectorChanged ) ) );

    public StyleSelector LayoutItemContainerStyleSelector
    {
      get
      {
        return ( StyleSelector )GetValue( LayoutItemContainerStyleSelectorProperty );
      }
      set
      {
        SetValue( LayoutItemContainerStyleSelectorProperty, value );
      }
    }

    private static void OnLayoutItemContainerStyleSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLayoutItemContainerStyleSelectorChanged( e );
    }

    protected virtual void OnLayoutItemContainerStyleSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
      AttachLayoutItems();
    }

    #endregion

    #region ShowSystemMenu

    public static readonly DependencyProperty ShowSystemMenuProperty = DependencyProperty.Register( "ShowSystemMenu", typeof( bool ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( bool )true ) );

    public bool ShowSystemMenu
    {
      get
      {
        return ( bool )GetValue( ShowSystemMenuProperty );
      }
      set
      {
        SetValue( ShowSystemMenuProperty, value );
      }
    }

    #endregion

    #region AllowMixedOrientation

    public static readonly DependencyProperty AllowMixedOrientationProperty = DependencyProperty.Register( "AllowMixedOrientation", typeof( bool ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( bool )false ) );

    public bool AllowMixedOrientation
    {
      get
      {
        return ( bool )GetValue( AllowMixedOrientationProperty );
      }
      set
      {
        SetValue( AllowMixedOrientationProperty, value );
      }
    }

    #endregion

    #endregion

    #region Internal Properties

    internal bool IsNavigatorWindowActive
    {
      get
      {
        return _navigatorWindow != null;
      }
    }

    #endregion

    #region Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();


      _autohideArea = GetTemplateChild( "PART_AutoHideArea" ) as FrameworkElement;
    }

    protected override void OnInitialized( EventArgs e )
    {
      base.OnInitialized( e );
    }


    protected override Size ArrangeOverride( Size arrangeBounds )
    {
      _areas = null;
      return base.ArrangeOverride( arrangeBounds );
    }

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      if( Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl ) )
      {
        if( e.IsDown && e.Key == Key.Tab )
        {
          if( !IsNavigatorWindowActive )
          {
            ShowNavigatorWindow();
            e.Handled = true;
          }
        }
      }

      base.OnPreviewKeyDown( e );
    }

    protected override void OnKeyDown( KeyEventArgs e )
    {
      if( Keyboard.IsKeyDown( Key.LeftShift ) || Keyboard.IsKeyDown( Key.RightShift ) )
      {
        if( e.IsDown && Keyboard.IsKeyDown( Key.F10 ) )
        {
          var current = this.ActiveContent as DependencyObject;
          while( current != null )
          {
            current = VisualTreeHelper.GetParent( current );
            if( ( current is LayoutDocumentControl ) || ( current is LayoutAnchorableControl ) )
              break;
          }

          if( current != null )
          {
            var ldc = current as LayoutDocumentControl;
            if( ( ldc != null ) && ( this.DocumentContextMenu != null ) )
            {
              this.DocumentContextMenu.PlacementTarget = ldc;
              this.DocumentContextMenu.Placement = PlacementMode.Relative;
              this.DocumentContextMenu.DataContext = ldc.LayoutItem;
              this.DocumentContextMenu.IsOpen = true;
            }
            else
            {
              var lac = current as LayoutAnchorableControl;
              if( ( lac != null ) && ( this.AnchorableContextMenu != null ) )
              {
                this.AnchorableContextMenu.PlacementTarget = lac;
                this.AnchorableContextMenu.Placement = PlacementMode.Relative;
                this.AnchorableContextMenu.DataContext = lac.LayoutItem;
                this.AnchorableContextMenu.Opened += this.AnchorableContextMenu_Opened;
                this.AnchorableContextMenu.IsOpen = true;
              }
            }
          }
        }
      }
      base.OnKeyDown( e );
    }

    #endregion

    #region Public Methods

    public virtual NavigatorWindow CreateNavigatorWindow()
    {
      return new NavigatorWindow( this );
    }

    public LayoutItem GetLayoutItemFromModel( LayoutContent content )
    {
      if( _layoutItems == null )
        return null;
      return _layoutItems.FirstOrDefault( item => item.LayoutElement == content );
    }

    public LayoutFloatingWindowControl CreateFloatingWindow( LayoutContent contentModel, bool isContentImmutable )
    {
      LayoutFloatingWindowControl lfwc = null;

      if( contentModel is LayoutAnchorable )
      {
        var parent = contentModel.Parent as ILayoutPane;
        if( parent == null )
        {
          var pane = new LayoutAnchorablePane( contentModel as LayoutAnchorable )
          {
            FloatingTop = contentModel.FloatingTop,
            FloatingLeft = contentModel.FloatingLeft,
            FloatingWidth = contentModel.FloatingWidth,
            FloatingHeight = contentModel.FloatingHeight
          };
          lfwc = this.CreateFloatingWindowForLayoutAnchorableWithoutParent( pane, isContentImmutable );
        }
      }

      if( lfwc == null )
      {
        lfwc = this.CreateFloatingWindowCore( contentModel, isContentImmutable );
      }

      return lfwc;
    }

    #endregion

    #region Internal Methods

    internal UIElement CreateUIElementForModel( ILayoutElement model )
    {
      if( model is LayoutPanel )
        return new LayoutPanelControl( model as LayoutPanel );
      if( model is LayoutAnchorablePaneGroup )
        return new LayoutAnchorablePaneGroupControl( model as LayoutAnchorablePaneGroup );
      if( model is LayoutDocumentPaneGroup )
        return new LayoutDocumentPaneGroupControl( model as LayoutDocumentPaneGroup );

      if( model is LayoutAnchorSide )
      {
        var templateModelView = new LayoutAnchorSideControl( model as LayoutAnchorSide );
        templateModelView.SetBinding( LayoutAnchorSideControl.TemplateProperty, new Binding( DockingManager.AnchorSideTemplateProperty.Name ) { Source = this } );
        return templateModelView;
      }
      if( model is LayoutAnchorGroup )
      {
        var templateModelView = new LayoutAnchorGroupControl( model as LayoutAnchorGroup );
        templateModelView.SetBinding( LayoutAnchorGroupControl.TemplateProperty, new Binding( DockingManager.AnchorGroupTemplateProperty.Name ) { Source = this } );
        return templateModelView;
      }

      if( model is LayoutDocumentPane )
      {
        var templateModelView = new LayoutDocumentPaneControl( model as LayoutDocumentPane );
        templateModelView.SetBinding( LayoutDocumentPaneControl.StyleProperty, new Binding( DockingManager.DocumentPaneControlStyleProperty.Name ) { Source = this } );
        return templateModelView;
      }
      if( model is LayoutAnchorablePane )
      {
        var templateModelView = new LayoutAnchorablePaneControl( model as LayoutAnchorablePane );
        templateModelView.SetBinding( LayoutAnchorablePaneControl.StyleProperty, new Binding( DockingManager.AnchorablePaneControlStyleProperty.Name ) { Source = this } );
        return templateModelView;
      }

      if( model is LayoutAnchorableFloatingWindow )
      {
        if( DesignerProperties.GetIsInDesignMode( this ) )
          return null;
        var modelFW = model as LayoutAnchorableFloatingWindow;
        var newFW = new LayoutAnchorableFloatingWindowControl( modelFW )
        {
          //Owner = Window.GetWindow(this) 
        };
        newFW.SetParentToMainWindowOf( this );

        var parent = this.Parent as FrameworkElement;
        while( parent != null )
        {
          newFW.InputBindings.AddRange( parent.InputBindings );
          parent = parent.Parent as FrameworkElement;
        }

        var paneForExtensions = modelFW.RootPanel.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault();
        if( paneForExtensions != null )
        {
          //ensure that floating window position is inside current (or nearest) monitor
          paneForExtensions.KeepInsideNearestMonitor();

          newFW.Left = paneForExtensions.FloatingLeft;
          newFW.Top = paneForExtensions.FloatingTop;
          newFW.Width = paneForExtensions.FloatingWidth;
          newFW.Height = paneForExtensions.FloatingHeight;
        }

        newFW.ShowInTaskbar = false;

        Dispatcher.BeginInvoke( new Action( () =>
        {
          if( newFW.IsClosing() )
            return;

          newFW.Show();

          // Do not set the WindowState before showing or it will be lost
          if( paneForExtensions != null && paneForExtensions.IsMaximized )
          {
            newFW.WindowState = WindowState.Maximized;
          }
        } ), DispatcherPriority.DataBind );

        return newFW;
      }

      if( model is LayoutDocumentFloatingWindow )
      {
        if( DesignerProperties.GetIsInDesignMode( this ) )
          return null;
        var modelFW = model as LayoutDocumentFloatingWindow;
        var newFW = new LayoutDocumentFloatingWindowControl( modelFW )
        {
          //Owner = Window.GetWindow(this) 
        };
        newFW.SetParentToMainWindowOf( this );

        var parent = this.Parent as FrameworkElement;
        while( parent != null )
        {
          newFW.InputBindings.AddRange( parent.InputBindings );
          parent = parent.Parent as FrameworkElement;
        }

        var paneForExtensions = modelFW.RootDocument;
        if( paneForExtensions != null )
        {
          //ensure that floating window position is inside current (or nearest) monitor
          paneForExtensions.KeepInsideNearestMonitor();

          newFW.Left = paneForExtensions.FloatingLeft;
          newFW.Top = paneForExtensions.FloatingTop;
          newFW.Width = paneForExtensions.FloatingWidth;
          newFW.Height = paneForExtensions.FloatingHeight;
        }

        newFW.ShowInTaskbar = false;
        newFW.Show();
        // Do not set the WindowState before showing or it will be lost
        if( paneForExtensions != null && paneForExtensions.IsMaximized )
        {
          newFW.WindowState = WindowState.Maximized;
        }
        return newFW;
      }

      if( model is LayoutDocument )
      {
        var templateModelView = new LayoutDocumentControl() { Model = model as LayoutDocument };

        templateModelView.SetResourcesFromObject( this );

        return templateModelView;
      }

      return null;
    }

    internal void ShowAutoHideWindow( LayoutAnchorControl anchor )
    {
      _autoHideWindowManager.ShowAutoHideWindow( anchor );
      //if (_autohideArea == null)
      //    return;

      //if (AutoHideWindow != null && AutoHideWindow.Model == anchor.Model)
      //    return;

      //Trace.WriteLine("ShowAutoHideWindow()");

      //_currentAutohiddenAnchor = new WeakReference(anchor);

      //HideAutoHideWindow(anchor);

      //SetAutoHideWindow(new LayoutAutoHideWindowControl(anchor));
      //AutoHideWindow.Show();
    }

    internal void HideAutoHideWindow( LayoutAnchorControl anchor )
    {
      _autoHideWindowManager.HideAutoWindow( anchor );
    }

    internal FrameworkElement GetAutoHideAreaElement()
    {
      return _autohideArea;
    }

    internal void StartDraggingFloatingWindowForContent( LayoutContent contentModel, bool startDrag = true )
    {
      if( ( contentModel == null ) || !contentModel.CanFloat )
        return;

      var fwc = this.CreateFloatingWindow( contentModel, false );
      if( fwc != null )
      {
        Dispatcher.BeginInvoke( new Action( () =>
        {
          if( fwc.IsClosing() )
            return;

          if( startDrag )
            fwc.AttachDrag();
          fwc.Show();
        } ), DispatcherPriority.Send );
      }
    }

    internal void StartDraggingFloatingWindowForPane( LayoutAnchorablePane paneModel )
    {
      var fwc = this.CreateFloatingWindowForLayoutAnchorableWithoutParent( paneModel, false );
      if( fwc != null )
      {
        fwc.AttachDrag();
        fwc.Show();
      }
    }

    internal IEnumerable<LayoutFloatingWindowControl> GetFloatingWindowsByZOrder()
    {
      IntPtr windowParentHanlde;
      var parentWindow = Window.GetWindow( this );
      if( parentWindow != null )
      {
        windowParentHanlde = new WindowInteropHelper( parentWindow ).Handle;
      }
      else
      {
        var mainProcess = Process.GetCurrentProcess();
        if( mainProcess == null )
          yield break;

        windowParentHanlde = mainProcess.MainWindowHandle;
      }

      IntPtr currentHandle = Win32Helper.GetWindow( windowParentHanlde, ( uint )Win32Helper.GetWindow_Cmd.GW_HWNDFIRST );
      while( currentHandle != IntPtr.Zero )
      {
        LayoutFloatingWindowControl ctrl = _fwList.FirstOrDefault( fw => new WindowInteropHelper( fw ).Handle == currentHandle );
        if( ctrl != null && ctrl.Model.Root.Manager == this )
          yield return ctrl;

        currentHandle = Win32Helper.GetWindow( currentHandle, ( uint )Win32Helper.GetWindow_Cmd.GW_HWNDNEXT );
      }
    }

    internal IEnumerable<Window> GetWindowsByZOrder()
    {
      IntPtr windowParentHanlde;
      var parentWindow = Window.GetWindow( this );
      if( parentWindow != null )
      {
        windowParentHanlde = new WindowInteropHelper( parentWindow ).Handle;
      }
      else
      {
        var mainProcess = Process.GetCurrentProcess();
        if( mainProcess == null )
          yield break;

        windowParentHanlde = mainProcess.MainWindowHandle;
      }

      IntPtr currentHandle = Win32Helper.GetWindow( windowParentHanlde, ( uint )Win32Helper.GetWindow_Cmd.GW_HWNDFIRST );
      while( currentHandle != IntPtr.Zero )
      {
        if( windowParentHanlde == currentHandle )
        {
          yield return parentWindow;
        }
        else
        {
          LayoutFloatingWindowControl ctrl = _fwList.FirstOrDefault( fw => new WindowInteropHelper( fw ).Handle == currentHandle );
          if( ctrl != null && ctrl.Model.Root.Manager == this )
            yield return ctrl;
        }

        currentHandle = Win32Helper.GetWindow( currentHandle, ( uint )Win32Helper.GetWindow_Cmd.GW_HWNDNEXT );
      }
    }

    internal void RemoveFloatingWindow( LayoutFloatingWindowControl floatingWindow )
    {
      _fwList.Remove( floatingWindow );
    }

    internal void _ExecuteCloseCommand( LayoutDocument document )
    {
      if( DocumentClosing != null )
      {
        var evargs = new DocumentClosingEventArgs( document );
        DocumentClosing( this, evargs );
        if( evargs.Cancel )
          return;
      }

      if( document.CloseDocument() )
      {
        this.RemoveViewFromLogicalChild( document );

        if( DocumentClosed != null )
        {
          var evargs = new DocumentClosedEventArgs( document );
          DocumentClosed( this, evargs );
        }
      }
    }

    internal void _ExecuteCloseAllButThisCommand( LayoutContent contentSelected )
    {
      foreach( var contentToClose in Layout.Descendents().OfType<LayoutContent>().Where( d => d != contentSelected && ( d.Parent is LayoutDocumentPane || d.Parent is LayoutDocumentFloatingWindow ) ).ToArray() )
      {
        this.Close( contentToClose );
      }
    }

    internal void _ExecuteCloseAllCommand( LayoutContent contentSelected )
    {
      foreach( var contentToClose in Layout.Descendents().OfType<LayoutContent>().Where( d => ( d.Parent is LayoutDocumentPane || d.Parent is LayoutDocumentFloatingWindow ) ).ToArray() )
      {
        this.Close( contentToClose );
      }
    }

    internal void _ExecuteCloseCommand( LayoutAnchorable anchorable )
    {
      var model = anchorable as LayoutAnchorable;
      if( model != null )
      {
        if( model.CloseAnchorable() )
        {
          this.RemoveViewFromLogicalChild( anchorable );
        }
      }
    }

    internal void _ExecuteHideCommand( LayoutAnchorable anchorable )
    {
      var model = anchorable as LayoutAnchorable;
      if( model != null )
      {
        model.Hide();
      }
    }

    internal void _ExecuteAutoHideCommand( LayoutAnchorable _anchorable )
    {
      _anchorable.ToggleAutoHide();
    }


    internal void _ExecuteFloatCommand( LayoutContent contentToFloat )
    {
      contentToFloat.Float();
    }

    internal void _ExecuteDockCommand( LayoutAnchorable anchorable )
    {
      anchorable.Dock();
    }

    internal void _ExecuteDockAsDocumentCommand( LayoutContent content )
    {
      content.DockAsDocument();
    }

    internal void _ExecuteContentActivateCommand( LayoutContent content )
    {
      content.IsActive = true;
    }

    protected internal virtual void ShowNavigatorWindow()
    {
      if( _navigatorWindow == null )
      {
        _navigatorWindow = this.CreateNavigatorWindow();
        _navigatorWindow.Owner = Window.GetWindow( this );
        _navigatorWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      }

      _navigatorWindow.ShowDialog();
      _navigatorWindow = null;
    }

    #endregion

    #region Private Methods

    private void OnLayoutRootPropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      if( e.PropertyName == "RootPanel" )
      {
        if( IsInitialized )
        {
          var layoutRootPanel = CreateUIElementForModel( Layout.RootPanel ) as LayoutPanelControl;
          LayoutRootPanel = layoutRootPanel;
        }
      }
      else if( e.PropertyName == "ActiveContent" )
      {
        if( Layout.ActiveContent != null )
        {
          //set focus on active element only after a layout pass is completed
          //it's possible that it is not yet visible in the visual tree
          //if (_setFocusAsyncOperation == null)
          //{
          //    _setFocusAsyncOperation = Dispatcher.BeginInvoke(new Action(() =>
          // {
          if( Layout.ActiveContent != null )
            FocusElementManager.SetFocusOnLastElement( Layout.ActiveContent );
          //_setFocusAsyncOperation = null;
          //  } ), DispatcherPriority.Input );
          //}
        }

        if( !_insideInternalSetActiveContent )
        {
#if VS2008
          this.ActiveContent = ( Layout.ActiveContent != null ) ? Layout.ActiveContent.Content : null;
#else
          this.SetCurrentValue( DockingManager.ActiveContentProperty, ( Layout.ActiveContent != null ) ? Layout.ActiveContent : null );
#endif
        }
      }
    }

    private void OnLayoutRootUpdated( object sender, EventArgs e )
    {
      CommandManager.InvalidateRequerySuggested();
    }

    private void OnLayoutChanging( LayoutRoot newLayout )
    {
      if( LayoutChanging != null )
        LayoutChanging( this, EventArgs.Empty );
    }

    private void DockingManager_Loaded( object sender, RoutedEventArgs e )
    {
      if( !DesignerProperties.GetIsInDesignMode( this ) )
      {
        if( Layout.Manager == this )
        {
          LayoutRootPanel = CreateUIElementForModel( Layout.RootPanel ) as LayoutPanelControl;
          LeftSidePanel = CreateUIElementForModel( Layout.LeftSide ) as LayoutAnchorSideControl;
          TopSidePanel = CreateUIElementForModel( Layout.TopSide ) as LayoutAnchorSideControl;
          RightSidePanel = CreateUIElementForModel( Layout.RightSide ) as LayoutAnchorSideControl;
          BottomSidePanel = CreateUIElementForModel( Layout.BottomSide ) as LayoutAnchorSideControl;
        }

        SetupAutoHideWindow();

        //load windows not already loaded!
        foreach( var fw in Layout.FloatingWindows.Where( fw => !_fwList.Any( fwc => fwc.Model == fw ) ) )
          _fwList.Add( CreateUIElementForModel( fw ) as LayoutFloatingWindowControl );

        //create the overlaywindow if it's possible
        if( IsVisible )
          CreateOverlayWindow();
        FocusElementManager.SetupFocusManagement( this );
      }
    }

    private void DockingManager_Unloaded( object sender, RoutedEventArgs e )
    {
      if( !DesignerProperties.GetIsInDesignMode( this ) )
      {
        if( _autoHideWindowManager != null )
        {
          _autoHideWindowManager.HideAutoWindow();
        }

        if( AutoHideWindow != null )
        {
          AutoHideWindow.Dispose();
        }

        foreach( var fw in _fwList.ToArray() )
        {
          //fw.Owner = null;
          fw.SetParentWindowToNull();
          fw.KeepContentVisibleOnClose = true;

          // Added Dispatcher to prevent InvalidOperationException issue in reference to bug case 
          // DevOps #2106
          Dispatcher.BeginInvoke( new Action( () =>
          {
            fw.Close();
          } ), DispatcherPriority.Send );

        }
        _fwList.Clear();

        DestroyOverlayWindow();
        FocusElementManager.FinalizeFocusManagement( this );
      }
    }

    private void SetupAutoHideWindow()
    {
      if( _autoHideWindowManager != null )
        _autoHideWindowManager.HideAutoWindow();
      else
        _autoHideWindowManager = new AutoHideWindowManager( this );

      if( AutoHideWindow != null )
      {
        AutoHideWindow.Dispose();
      }

      SetAutoHideWindow( new LayoutAutoHideWindowControl() );
    }

    private void CreateOverlayWindow()
    {
      if( _overlayWindow == null )
      {
        _overlayWindow = new OverlayWindow( this );
      }
      Rect rectWindow = new Rect( this.PointToScreenDPIWithoutFlowDirection( new Point() ), this.TransformActualSizeToAncestor() );
      _overlayWindow.Left = rectWindow.Left;
      _overlayWindow.Top = rectWindow.Top;
      _overlayWindow.Width = rectWindow.Width;
      _overlayWindow.Height = rectWindow.Height;
    }

    private void DestroyOverlayWindow()
    {
      if( _overlayWindow != null )
      {
        _overlayWindow.Close();
        _overlayWindow = null;
      }
    }

    private void AttachDocumentsSource( LayoutRoot layout, IEnumerable documentsSource )
    {
      if( documentsSource == null )
        return;

      if( layout == null )
        return;

      //if (layout.Descendents().OfType<LayoutDocument>().Any())
      //    throw new InvalidOperationException("Unable to set the DocumentsSource property if LayoutDocument objects are already present in the model");
      var documentsImported = layout.Descendents().OfType<LayoutDocument>().Select( d => d.Content ).ToArray();
      var documents = documentsSource as IEnumerable;
      var listOfDocumentsToImport = new List<object>( documents.OfType<object>() );

      foreach( var document in listOfDocumentsToImport.ToArray() )
      {
        if( documentsImported.Contains( document ) )
          listOfDocumentsToImport.Remove( document );
      }


      LayoutDocumentPane documentPane = null;
      if( layout.LastFocusedDocument != null )
      {
        documentPane = layout.LastFocusedDocument.Parent as LayoutDocumentPane;
      }

      if( documentPane == null )
      {
        documentPane = layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
      }

      //if (documentPane == null)
      //    throw new InvalidOperationException("Layout must contains at least one LayoutDocumentPane in order to host documents");

      _suspendLayoutItemCreation = true;
      foreach( var documentContentToImport in listOfDocumentsToImport )
      {
        var documentToImport = new LayoutDocument()
        {
          Content = documentContentToImport,
          ContentId = Guid.NewGuid().ToString()
        };

        bool added = false;
        if( LayoutUpdateStrategy != null )
        {
          added = LayoutUpdateStrategy.BeforeInsertDocument( layout, documentToImport, documentPane );
        }

        if( !added )
        {
          if( documentPane == null )
            throw new InvalidOperationException( "Layout must contains at least one LayoutDocumentPane in order to host documents" );

          documentPane.Children.Add( documentToImport );
          added = true;
        }

        if( LayoutUpdateStrategy != null )
          LayoutUpdateStrategy.AfterInsertDocument( layout, documentToImport );


        CreateDocumentLayoutItem( documentToImport );

      }
      _suspendLayoutItemCreation = false;


      var documentsSourceAsNotifier = documentsSource as INotifyCollectionChanged;
      if( documentsSourceAsNotifier != null )
      {
        CollectionChangedEventManager.AddListener( documentsSourceAsNotifier, this );
      }
    }

    private void DocumentsSourceElementsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( Layout == null )
        return;

      //When deserializing documents are created automatically by the deserializer
      if( SuspendDocumentsSourceBinding )
        return;

      //handle remove
      if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
          e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace )
      {
        if( e.OldItems != null )
        {
          var documentsToRemove = Layout.Descendents().OfType<LayoutDocument>().Where( d => e.OldItems.Contains( d.Content ) ).ToArray();
          for( int i = 0; i < documentsToRemove.Count(); ++i )
          {
            var documentToRemove = documentsToRemove[ i ];

            ( documentToRemove.Parent as ILayoutContainer ).RemoveChild( documentToRemove );
            this.RemoveViewFromLogicalChild( documentToRemove );

            this.RemoveDocumentLayoutItem( documentToRemove );

            documentToRemove.Content = null;
          }
        }
      }

      //handle add
      if( e.NewItems != null &&
          ( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
          e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace ) )
      {
        if( e.NewItems != null )
        {
          LayoutDocumentPane documentPane = null;
          if( Layout.LastFocusedDocument != null )
          {
            documentPane = Layout.LastFocusedDocument.Parent as LayoutDocumentPane;
          }

          if( documentPane == null )
          {
            documentPane = Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
          }

          //if (documentPane == null)
          //    throw new InvalidOperationException("Layout must contains at least one LayoutDocumentPane in order to host documents");

          _suspendLayoutItemCreation = true;

          foreach( var documentContentToImport in e.NewItems )
          {
            var documentToImport = new LayoutDocument()
            {
              Content = documentContentToImport
            };

            bool added = false;
            if( LayoutUpdateStrategy != null )
            {
              added = LayoutUpdateStrategy.BeforeInsertDocument( Layout, documentToImport, documentPane );
            }

            if( !added )
            {
              if( documentPane == null )
                throw new InvalidOperationException( "Layout must contains at least one LayoutDocumentPane in order to host documents" );

              documentPane.Children.Add( documentToImport );
              added = true;
            }

            if( LayoutUpdateStrategy != null )
            {
              LayoutUpdateStrategy.AfterInsertDocument( Layout, documentToImport );
            }


            var root = documentToImport.Root;

            if( root != null && root.Manager == this )
            {
              CreateDocumentLayoutItem( documentToImport );
            }
          }
          _suspendLayoutItemCreation = false;
        }
      }

      if( e.Action == NotifyCollectionChangedAction.Reset )
      {
        //NOTE: I'm going to clear every document present in layout but
        //some documents may have been added directly to the layout, for now I clear them too
        var documentsToRemove = Layout.Descendents().OfType<LayoutDocument>().ToArray();
        foreach( var documentToRemove in documentsToRemove )
        {
          ( documentToRemove.Parent as ILayoutContainer ).RemoveChild( documentToRemove );
          this.RemoveViewFromLogicalChild( documentToRemove );
          this.RemoveDocumentLayoutItem( documentToRemove );

          documentToRemove.Content = null;
        }
      }

      if( Layout != null )
      {
        Layout.CollectGarbage();
      }
    }

    private void DetachDocumentsSource( LayoutRoot layout, IEnumerable documentsSource )
    {
      if( documentsSource == null )
        return;

      if( layout == null )
        return;

      var documentsToRemove = layout.Descendents().OfType<LayoutDocument>()
          .Where( d => documentsSource.Contains( d.Content ) ).ToArray();

      foreach( var documentToRemove in documentsToRemove )
      {
        ( documentToRemove.Parent as ILayoutContainer ).RemoveChild( documentToRemove );
        this.RemoveViewFromLogicalChild( documentToRemove );
      }

      var documentsSourceAsNotifier = documentsSource as INotifyCollectionChanged;
      if( documentsSourceAsNotifier != null )
      {
        CollectionChangedEventManager.RemoveListener( documentsSourceAsNotifier, this );
      }

      this.Layout.CollectGarbage();
    }

    private void Close( LayoutContent contentToClose )
    {
      if( !contentToClose.CanClose )
        return;

      var layoutItem = GetLayoutItemFromModel( contentToClose );
      if( layoutItem.CloseCommand != null )
      {
        if( layoutItem.CloseCommand.CanExecute( null ) )
          layoutItem.CloseCommand.Execute( null );
      }
      else
      {
        if( contentToClose is LayoutDocument )
          _ExecuteCloseCommand( contentToClose as LayoutDocument );
        else if( contentToClose is LayoutAnchorable )
          _ExecuteCloseCommand( contentToClose as LayoutAnchorable );
      }
    }

    private void AttachAnchorablesSource( LayoutRoot layout, IEnumerable anchorablesSource )
    {
      if( anchorablesSource == null )
        return;

      if( layout == null )
        return;

      //if (layout.Descendents().OfType<LayoutAnchorable>().Any())
      //    throw new InvalidOperationException("Unable to set the AnchorablesSource property if LayoutAnchorable objects are already present in the model");
      var anchorablesImported = layout.Descendents().OfType<LayoutAnchorable>().Select( d => d.Content ).ToArray();
      var anchorables = anchorablesSource as IEnumerable;
      var listOfAnchorablesToImport = new List<object>( anchorables.OfType<object>() );

      foreach( var document in listOfAnchorablesToImport.ToArray() )
      {
        if( anchorablesImported.Contains( document ) )
          listOfAnchorablesToImport.Remove( document );
      }

      LayoutAnchorablePane anchorablePane = null;
      if( layout.ActiveContent != null )
      {
        //look for active content parent pane
        anchorablePane = layout.ActiveContent.Parent as LayoutAnchorablePane;
      }

      if( anchorablePane == null )
      {
        //look for a pane on the right side
        anchorablePane = layout.Descendents().OfType<LayoutAnchorablePane>().Where( pane => !pane.IsHostedInFloatingWindow && pane.GetSide() == AnchorSide.Right ).FirstOrDefault();
      }

      if( anchorablePane == null )
      {
        //look for an available pane
        anchorablePane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault();
      }

      _suspendLayoutItemCreation = true;
      foreach( var anchorableContentToImport in listOfAnchorablesToImport )
      {
        var anchorableToImport = new LayoutAnchorable()
        {
          Content = anchorableContentToImport,
          ContentId = Guid.NewGuid().ToString()
        };

        bool added = false;
        if( LayoutUpdateStrategy != null )
        {
          added = LayoutUpdateStrategy.BeforeInsertAnchorable( layout, anchorableToImport, anchorablePane );
        }

        if( !added )
        {
          if( anchorablePane == null )
          {
            var mainLayoutPanel = new LayoutPanel() { Orientation = Orientation.Horizontal };
            if( layout.RootPanel != null )
            {
              mainLayoutPanel.Children.Add( layout.RootPanel );
            }

            layout.RootPanel = mainLayoutPanel;
            anchorablePane = new LayoutAnchorablePane() { DockWidth = new GridLength( 200.0, GridUnitType.Pixel ) };
            mainLayoutPanel.Children.Add( anchorablePane );
          }

          anchorablePane.Children.Add( anchorableToImport );
          added = true;
        }

        if( LayoutUpdateStrategy != null )
          LayoutUpdateStrategy.AfterInsertAnchorable( layout, anchorableToImport );


        CreateAnchorableLayoutItem( anchorableToImport );
      }

      _suspendLayoutItemCreation = false;

      var anchorablesSourceAsNotifier = anchorablesSource as INotifyCollectionChanged;
      if( anchorablesSourceAsNotifier != null )
      {
        CollectionChangedEventManager.AddListener( anchorablesSourceAsNotifier, this );
      }
    }

    private void anchorablesSourceElementsChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      if( Layout == null )
        return;

      //When deserializing documents are created automatically by the deserializer
      if( SuspendAnchorablesSourceBinding )
        return;

      //handle remove
      if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
          e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace )
      {
        if( e.OldItems != null )
        {
          var anchorablesToRemove = Layout.Descendents().OfType<LayoutAnchorable>().Where( d => e.OldItems.Contains( d.Content ) ).ToArray();
          foreach( var anchorableToRemove in anchorablesToRemove )
          {
            anchorableToRemove.Content = null;
            ( anchorableToRemove.Parent as ILayoutContainer ).RemoveChild(
                anchorableToRemove );
            this.RemoveViewFromLogicalChild( anchorableToRemove );
          }
        }
      }

      //handle add
      if( e.NewItems != null &&
          ( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
          e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace ) )
      {
        if( e.NewItems != null )
        {
          LayoutAnchorablePane anchorablePane = null;

          if( Layout.ActiveContent != null )
          {
            //look for active content parent pane
            anchorablePane = Layout.ActiveContent.Parent as LayoutAnchorablePane;
          }

          if( anchorablePane == null )
          {
            //look for a pane on the right side
            anchorablePane = Layout.Descendents().OfType<LayoutAnchorablePane>().Where( pane => !pane.IsHostedInFloatingWindow && pane.GetSide() == AnchorSide.Right ).FirstOrDefault();
          }

          if( anchorablePane == null )
          {
            //look for an available pane
            anchorablePane = Layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault();
          }

          _suspendLayoutItemCreation = true;
          foreach( var anchorableContentToImport in e.NewItems )
          {
            var anchorableToImport = new LayoutAnchorable()
            {
              Content = anchorableContentToImport
            };

            bool added = false;
            if( LayoutUpdateStrategy != null )
            {
              added = LayoutUpdateStrategy.BeforeInsertAnchorable( Layout, anchorableToImport, anchorablePane );
            }

            if( !added )
            {
              if( anchorablePane == null )
              {
                var mainLayoutPanel = new LayoutPanel() { Orientation = Orientation.Horizontal };
                if( Layout.RootPanel != null )
                {
                  mainLayoutPanel.Children.Add( Layout.RootPanel );
                }

                Layout.RootPanel = mainLayoutPanel;
                anchorablePane = new LayoutAnchorablePane() { DockWidth = new GridLength( 200.0, GridUnitType.Pixel ) };
                mainLayoutPanel.Children.Add( anchorablePane );
              }

              anchorablePane.Children.Add( anchorableToImport );
              added = true;
            }

            if( LayoutUpdateStrategy != null )
            {
              LayoutUpdateStrategy.AfterInsertAnchorable( Layout, anchorableToImport );
            }

            var root = anchorableToImport.Root;

            if( root != null && root.Manager == this )
            {
              CreateAnchorableLayoutItem( anchorableToImport );
            }

          }
          _suspendLayoutItemCreation = false;
        }
      }

      if( e.Action == NotifyCollectionChangedAction.Reset )
      {
        //NOTE: I'm going to clear every anchorable present in layout but
        //some anchorable may have been added directly to the layout, for now I clear them too
        var anchorablesToRemove = Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
        foreach( var anchorableToRemove in anchorablesToRemove )
        {
          ( anchorableToRemove.Parent as ILayoutContainer ).RemoveChild(
              anchorableToRemove );
          this.RemoveViewFromLogicalChild( anchorableToRemove );
        }
      }

      if( Layout != null )
        Layout.CollectGarbage();
    }

    private void DetachAnchorablesSource( LayoutRoot layout, IEnumerable anchorablesSource )
    {
      if( anchorablesSource == null )
        return;

      if( layout == null )
        return;

      var anchorablesToRemove = layout.Descendents().OfType<LayoutAnchorable>()
          .Where( d => anchorablesSource.Contains( d.Content ) ).ToArray();

      foreach( var anchorableToRemove in anchorablesToRemove )
      {
        ( anchorableToRemove.Parent as ILayoutContainer ).RemoveChild(
            anchorableToRemove );
        this.RemoveViewFromLogicalChild( anchorableToRemove );
      }

      var anchorablesSourceAsNotifier = anchorablesSource as INotifyCollectionChanged;
      if( anchorablesSourceAsNotifier != null )
      {
        CollectionChangedEventManager.RemoveListener( anchorablesSourceAsNotifier, this );
      }

      this.Layout.CollectGarbage();
    }

    private void RemoveViewFromLogicalChild( LayoutContent layoutContent )
    {
      if( layoutContent == null )
        return;

      var layoutItem = this.GetLayoutItemFromModel( layoutContent );
      if( layoutItem != null )
      {
        if( layoutItem.IsViewExists() )
        {
          BindingOperations.ClearAllBindings( layoutItem.View );

          this.InternalRemoveLogicalChild( layoutItem.View );
          layoutItem._view = null;
        }
      }
    }

    private void InternalSetActiveContent( object activeContent )
    {
      var activeLayoutContent = activeContent as LayoutContent;
      var layoutContent = this.Layout.Descendents().OfType<LayoutContent>().FirstOrDefault( lc =>
      {
        if( activeLayoutContent != null )
        {
          if( activeLayoutContent.Content != null )
            return ( ( lc == activeLayoutContent.Content ) || ( lc.Content == activeLayoutContent.Content ) );

          if( activeLayoutContent.ContentId != null )
            return ( lc.ContentId == activeLayoutContent.ContentId );
        }
        else
          return ( ( lc == activeContent ) || ( lc.Content == activeContent ) );

        return ( ( lc == null ) || ( lc.Content == null ) );
      } );
      _insideInternalSetActiveContent = true;
      this.Layout.ActiveContent = layoutContent;
      _insideInternalSetActiveContent = false;
    }

    private void DetachLayoutItems()
    {
      if( Layout != null )
      {
        _layoutItems.ForEach<LayoutItem>( i => i.Detach() );
        _layoutItems.Clear();
        Layout.ElementAdded -= new EventHandler<LayoutElementEventArgs>( Layout_ElementAdded );
        Layout.ElementRemoved -= new EventHandler<LayoutElementEventArgs>( Layout_ElementRemoved );
      }
    }

    private void Layout_ElementRemoved( object sender, LayoutElementEventArgs e )
    {
      if( _suspendLayoutItemCreation )
        return;

      CollectLayoutItemsDeleted();
    }

    private void Layout_ElementAdded( object sender, LayoutElementEventArgs e )
    {
      if( _suspendLayoutItemCreation )
        return;

      foreach( var content in Layout.Descendents().OfType<LayoutContent>() )
      {
        if( content is LayoutDocument )
          CreateDocumentLayoutItem( content as LayoutDocument );
        else //if (content is LayoutAnchorable)
          CreateAnchorableLayoutItem( content as LayoutAnchorable );
      }

      CollectLayoutItemsDeleted();
    }

    private void CollectLayoutItemsDeleted()
    {
      if( _collectLayoutItemsOperations != null )
        return;
      _collectLayoutItemsOperations = Dispatcher.BeginInvoke( new Action( () =>
      {
        _collectLayoutItemsOperations = null;
        var layoutItems = _layoutItems.Where( item => item.LayoutElement.Root != Layout ).ToArray();
        for( int i = 0; i < layoutItems.Count(); ++i )
        {
          var itemToRemove = layoutItems[ i ];
          this.RemoveDocumentLayoutItem( itemToRemove.LayoutElement as LayoutDocument );
        }
      } ) );
    }

    private void AttachLayoutItems()
    {
      if( Layout != null )
      {
        foreach( var document in Layout.Descendents().OfType<LayoutDocument>().ToArray() )
        {
          CreateDocumentLayoutItem( document );
          //var documentItem = new LayoutDocumentItem();
          //documentItem.Attach(document);
          //ApplyStyleToLayoutItem(documentItem);
          //_layoutItems.Add(documentItem);
        }
        foreach( var anchorable in Layout.Descendents().OfType<LayoutAnchorable>().ToArray() )
        {
          CreateAnchorableLayoutItem( anchorable );
          //var anchorableItem = new LayoutAnchorableItem();
          //anchorableItem.Attach(anchorable);
          //ApplyStyleToLayoutItem(anchorableItem);
          //_layoutItems.Add(anchorableItem);
        }

        Layout.ElementAdded += new EventHandler<LayoutElementEventArgs>( Layout_ElementAdded );
        Layout.ElementRemoved += new EventHandler<LayoutElementEventArgs>( Layout_ElementRemoved );
      }
    }

    private void ApplyStyleToLayoutItem( LayoutItem layoutItem )
    {
      layoutItem._ClearDefaultBindings();
      if( LayoutItemContainerStyle != null )
        layoutItem.Style = LayoutItemContainerStyle;
      else if( LayoutItemContainerStyleSelector != null )
        layoutItem.Style = LayoutItemContainerStyleSelector.SelectStyle( layoutItem.Model, layoutItem );
      layoutItem._SetDefaultBindings();
    }

    private void CreateAnchorableLayoutItem( LayoutAnchorable contentToAttach )
    {
      if( _layoutItems.Any( item => item.LayoutElement == contentToAttach ) )
      {
        foreach( var item in _layoutItems )
        {
          ApplyStyleToLayoutItem( item );
        }
        return;
      }

      var layoutItem = new LayoutAnchorableItem();
      layoutItem.Attach( contentToAttach );
      ApplyStyleToLayoutItem( layoutItem );
      _layoutItems.Add( layoutItem );

      if( contentToAttach != null &&
          contentToAttach.Content != null &&
          contentToAttach.Content is UIElement )
      {
        InternalAddLogicalChild( contentToAttach.Content );
      }

    }

    private void CreateDocumentLayoutItem( LayoutDocument contentToAttach )
    {
      if( _layoutItems.Any( item => item.LayoutElement == contentToAttach ) )
      {
        foreach( var item in _layoutItems )
        {
          ApplyStyleToLayoutItem( item );
        }
        return;
      }

      var layoutItem = new LayoutDocumentItem();
      layoutItem.Attach( contentToAttach );
      ApplyStyleToLayoutItem( layoutItem );
      _layoutItems.Add( layoutItem );

      if( contentToAttach != null &&
          contentToAttach.Content != null &&
          contentToAttach.Content is UIElement )
      {
        InternalAddLogicalChild( contentToAttach.Content );
      }
    }

    private void RemoveDocumentLayoutItem( LayoutDocument contentToRemove )
    {
      var layoutItem = _layoutItems.FirstOrDefault( item => item.LayoutElement == contentToRemove );
      if( layoutItem != null )
      {
        layoutItem._ClearDefaultBindings();
        layoutItem.Detach();
        _layoutItems.Remove( layoutItem );

        if( contentToRemove != null &&
          contentToRemove.Content != null &&
          contentToRemove.Content is UIElement )
        {
          InternalRemoveLogicalChild( contentToRemove.Content );
        }
      }
    }

    private LayoutFloatingWindowControl CreateFloatingWindowForLayoutAnchorableWithoutParent( LayoutAnchorablePane paneModel, bool isContentImmutable )
    {
      var selectedlayoutContent = paneModel.SelectedContent;
      this.RaisePreviewFloatEvent( selectedlayoutContent );

      if( paneModel.Children.Any( c => !c.CanFloat ) )
        return null;
      var paneAsPositionableElement = paneModel as ILayoutPositionableElement;
      var paneAsWithActualSize = paneModel as ILayoutPositionableElementWithActualSize;

      double fwWidth = paneAsPositionableElement.FloatingWidth;
      double fwHeight = paneAsPositionableElement.FloatingHeight;
      double fwLeft = paneAsPositionableElement.FloatingLeft;
      double fwTop = paneAsPositionableElement.FloatingTop;

      if( fwWidth == 0.0 )
        fwWidth = paneAsWithActualSize.ActualWidth + 10;       //10 includes BorderThickness and Margins inside LayoutAnchorableFloatingWindowControl.
      if( fwHeight == 0.0 )
        fwHeight = paneAsWithActualSize.ActualHeight + 10;   //10 includes BorderThickness and Margins inside LayoutAnchorableFloatingWindowControl.

      var destPane = new LayoutAnchorablePane()
      {
        DockWidth = paneAsPositionableElement.DockWidth,
        DockHeight = paneAsPositionableElement.DockHeight,
        DockMinHeight = paneAsPositionableElement.DockMinHeight,
        DockMinWidth = paneAsPositionableElement.DockMinWidth,
        FloatingLeft = paneAsPositionableElement.FloatingLeft,
        FloatingTop = paneAsPositionableElement.FloatingTop,
        FloatingWidth = paneAsPositionableElement.FloatingWidth,
        FloatingHeight = paneAsPositionableElement.FloatingHeight,
      };

      bool savePreviousContainer = paneModel.FindParent<LayoutFloatingWindow>() == null;
      int currentSelectedContentIndex = paneModel.SelectedContentIndex;
      while( paneModel.Children.Count > 0 )
      {
        var contentModel = paneModel.Children[ paneModel.Children.Count - 1 ] as LayoutAnchorable;

        if( savePreviousContainer )
        {
          var contentModelAsPreviousContainer = contentModel as ILayoutPreviousContainer;
          contentModelAsPreviousContainer.PreviousContainer = paneModel;
          contentModel.PreviousContainerIndex = paneModel.Children.Count - 1;
        }

        paneModel.RemoveChildAt( paneModel.Children.Count - 1 );
        destPane.Children.Insert( 0, contentModel );
      }

      if( destPane.Children.Count > 0 )
      {
        destPane.SelectedContentIndex = currentSelectedContentIndex;
      }


      LayoutFloatingWindow fw;
      LayoutFloatingWindowControl fwc;
      fw = new LayoutAnchorableFloatingWindow()
      {
        RootPanel = new LayoutAnchorablePaneGroup(
              destPane )
        {
          DockHeight = destPane.DockHeight,
          DockWidth = destPane.DockWidth,
          DockMinHeight = destPane.DockMinHeight,
          DockMinWidth = destPane.DockMinWidth,
        }
      };

      Layout.FloatingWindows.Add( fw );

      fwc = new LayoutAnchorableFloatingWindowControl(
          fw as LayoutAnchorableFloatingWindow, isContentImmutable )
      {
        Width = fwWidth,
        Height = fwHeight,
        Top = fwTop,
        Left = fwLeft
      };

      this.ShowInTaskbar( fwc );

      var parent = this.Parent as FrameworkElement;
      while( parent != null )
      {
        fwc.InputBindings.AddRange( parent.InputBindings );
        parent = parent.Parent as FrameworkElement;
      }

      foreach( var layoutContent in destPane.Children )
      {
        layoutContent.IsFloating = true;
      }

      _fwList.Add( fwc );

      this.RaiseFloatedEvent( selectedlayoutContent );

      Layout.CollectGarbage();

      InvalidateArrange();

      return fwc;
    }

    private LayoutFloatingWindowControl CreateFloatingWindowCore( LayoutContent contentModel, bool isContentImmutable )
    {
      this.RaisePreviewFloatEvent( contentModel );

      if( !contentModel.CanFloat )
        return null;

      var contentModelAsAnchorable = contentModel as LayoutAnchorable;
      if( contentModelAsAnchorable != null && contentModelAsAnchorable.IsAutoHidden )
        contentModelAsAnchorable.ToggleAutoHide();

      this.UpdateStarSize( contentModel );

      var parentPane = contentModel.Parent as ILayoutPane;
      var parentPaneAsPositionableElement = contentModel.Parent as ILayoutPositionableElement;
      var parentPaneAsWithActualSize = contentModel.Parent as ILayoutPositionableElementWithActualSize;
      var contentModelParentChildrenIndex = parentPane.Children.ToList().IndexOf( contentModel );


      if( contentModel.FindParent<LayoutFloatingWindow>() == null )
      {
        ( ( ILayoutPreviousContainer )contentModel ).PreviousContainer = parentPane;
        contentModel.PreviousContainerIndex = contentModelParentChildrenIndex;
      }

      parentPane.RemoveChildAt( contentModelParentChildrenIndex );

      while( ( parentPane != null ) && ( parentPane.ChildrenCount == 0 ) )
      {
        var grandParent = parentPane.Parent as ILayoutPane;
        if( grandParent != null )
        {
          var greatGrandParent = grandParent.Parent;
          // Case 2934 - Do not remove the Last Parent in the LayoutRoot in order to keep the view of the LayoutRoot by default
          if( greatGrandParent != null && greatGrandParent == this.Layout.RootPanel && greatGrandParent.ChildrenCount == 1 )
          {
            break;
          }

          grandParent.RemoveChild( parentPane );
        }

        parentPane = grandParent;
      }

      double fwWidth = contentModel.FloatingWidth;
      double fwHeight = contentModel.FloatingHeight;

      if( fwWidth == 0.0 )
        fwWidth = parentPaneAsPositionableElement.FloatingWidth;
      if( fwHeight == 0.0 )
        fwHeight = parentPaneAsPositionableElement.FloatingHeight;

      if( fwWidth == 0.0 )
        fwWidth = parentPaneAsWithActualSize.ActualWidth + 10;      //10 includes BorderThickness and Margins inside LayoutDocumentFloatingWindowControl.
      if( fwHeight == 0.0 )
        fwHeight = parentPaneAsWithActualSize.ActualHeight + 10;    //10 includes BorderThickness and Margins inside LayoutDocumentFloatingWindowControl.

      LayoutFloatingWindow fw;
      LayoutFloatingWindowControl fwc;
      if( contentModel is LayoutAnchorable )
      {
        var anchorableContent = contentModel as LayoutAnchorable;
        fw = new LayoutAnchorableFloatingWindow()
        {
          RootPanel = new LayoutAnchorablePaneGroup(
                new LayoutAnchorablePane( anchorableContent )
                {
                  DockWidth = parentPaneAsPositionableElement.DockWidth,
                  DockHeight = parentPaneAsPositionableElement.DockHeight,
                  DockMinHeight = parentPaneAsPositionableElement.DockMinHeight,
                  DockMinWidth = parentPaneAsPositionableElement.DockMinWidth,
                  FloatingLeft = parentPaneAsPositionableElement.FloatingLeft,
                  FloatingTop = parentPaneAsPositionableElement.FloatingTop,
                  FloatingWidth = parentPaneAsPositionableElement.FloatingWidth,
                  FloatingHeight = parentPaneAsPositionableElement.FloatingHeight,
                } )
        };

        Layout.FloatingWindows.Add( fw );

        // Must be done after Layout.FloatingWindows.Add( fw ) to be able to modify the values in _dockingManager.Layout.FloatingWindows.CollectionChanged.
        var fwSize = this.UpdateFloatingDimensions( contentModel, new Size( fwWidth, fwHeight ) );

        fwc = new LayoutAnchorableFloatingWindowControl(
            fw as LayoutAnchorableFloatingWindow, isContentImmutable )
        {
          Width = fwSize.Width,
          Height = fwSize.Height,
          Left = contentModel.FloatingLeft,
          Top = contentModel.FloatingTop
        };
      }
      else
      {
        var anchorableDocument = contentModel as LayoutDocument;
        fw = new LayoutDocumentFloatingWindow()
        {
          RootDocument = anchorableDocument
        };

        Layout.FloatingWindows.Add( fw );

        // Must be done after Layout.FloatingWindows.Add( fw ) to be able to modify the values in _dockingManager.Layout.FloatingWindows.CollectionChanged.
        var fwSize = this.UpdateFloatingDimensions( contentModel, new Size( fwWidth, fwHeight ) );

        fwc = new LayoutDocumentFloatingWindowControl(
            fw as LayoutDocumentFloatingWindow, isContentImmutable )
        {
          Width = fwSize.Width,
          Height = fwSize.Height,
          Left = contentModel.FloatingLeft,
          Top = contentModel.FloatingTop
        };

      }

      this.ShowInTaskbar( fwc );
      contentModel.IsFloating = true;

      var parent = this.Parent as FrameworkElement;
      while( parent != null )
      {
        fwc.InputBindings.AddRange( parent.InputBindings );
        parent = parent.Parent as FrameworkElement;
      }

      _fwList.Add( fwc );

      this.RaiseFloatedEvent( contentModel );

      Layout.CollectGarbage();

      UpdateLayout();

      return fwc;
    }

    private void ShowInTaskbar( LayoutFloatingWindowControl fwc )
    {
      var layouts = fwc.Model.Descendents().OfType<LayoutContent>().Where( l => l != null );

      if( layouts != null )
      {
        fwc.ShowInTaskbar = true;
        if( layouts.Count() > 1 )
        {
          var selectedLayout = layouts.FirstOrDefault( l => l.IsSelected );
          fwc.Title = ( selectedLayout != null ) ? selectedLayout.Title : "";
        }
        else
        {
          fwc.Title = layouts.ElementAt( 0 ).Title ?? "";
        }
      }

      this.RenameWindowTitleForMultipleDockingManagerRunningInstances( fwc );
    }

    private void RenameWindowTitleForMultipleDockingManagerRunningInstances( LayoutFloatingWindowControl fwc )
    {
      var entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly() ?? Assembly.GetCallingAssembly();
      if( entryAssembly != null )
      {
        var processName = Path.GetFileNameWithoutExtension( entryAssembly.Location );
        var processes = Process.GetProcessesByName( processName );

        // Check if others applications run DockingManager
        var exists = processes.Length > 1;

        if( exists )
        {
          var mainWindowTitle = Window.GetWindow( this ).Title;
          if( !string.IsNullOrEmpty( mainWindowTitle ) )
          {
            fwc.Title = mainWindowTitle + " - " + fwc.Title;
          }
        }
      }
    }

    private Size UpdateFloatingDimensions( ILayoutElementForFloatingWindow contentModel, Size currentSize )
    {
      if( contentModel.FloatingWidth != 0d )
        currentSize.Width = contentModel.FloatingWidth;
      if( contentModel.FloatingHeight != 0d )
        currentSize.Height = contentModel.FloatingHeight;

      return currentSize;
    }

    private void UpdateStarSize( LayoutContent contentModel )
    {
      if( contentModel == null )
        return;

      var parentPane = contentModel.Parent as ILayoutPositionableElement;
      if( parentPane != null )
      {
        var parentLayoutContainer = parentPane as ILayoutContainer;
        if( ( parentLayoutContainer != null ) && ( parentLayoutContainer.ChildrenCount == 1 ) )
        {
          // Reset Dock Size of floating LayoutContent
          if( parentPane.DockWidth.IsStar )
          {
            parentPane.DockWidth = new GridLength( 1d, GridUnitType.Star );
          }
          if( parentPane.DockHeight.IsStar )
          {
            parentPane.DockHeight = new GridLength( 1d, GridUnitType.Star );
          }
        }

        var grandParentPaneOrientation = parentPane.Parent as ILayoutOrientableGroup;
        var grandParentPane = parentPane.Parent as ILayoutPositionableElement;
        if( ( grandParentPaneOrientation != null ) && ( grandParentPane != null ) )
        {
          if( grandParentPaneOrientation.Orientation == Orientation.Horizontal )
          {
            // Reset Dock Width of remaining LayoutContent
            if( grandParentPane.DockWidth.IsStar )
            {
              var grandParentPaneContainer = parentPane.Parent as ILayoutContainer;
              if( grandParentPaneContainer != null )
              {
                var children = grandParentPaneContainer.Children.Where( child => ( child.Equals( parentPane ) && ( parentPane is ILayoutContainer ) && ( ( ( ILayoutContainer )parentPane ).ChildrenCount > 1 ) )
                                                                                || ( !child.Equals( parentPane ) && ( child is ILayoutContainer ) && ( ( ( ILayoutContainer )child ).ChildrenCount > 0 ) ) )
                                                                .Cast<ILayoutPositionableElement>()
                                                                .Where( child => child.DockWidth.IsStar );
                var childrenTotalWidth = children.Sum( child => child.DockWidth.Value );
                foreach( var child in children )
                {
                  child.DockWidth = new GridLength( child.DockWidth.Value / childrenTotalWidth, GridUnitType.Star );
                }
              }
            }
          }
          else
          {
            // Reset Dock Height of remaining LayoutContent
            if( grandParentPane.DockHeight.IsStar )
            {
              var grandParentPaneContainer = parentPane.Parent as ILayoutContainer;
              if( grandParentPaneContainer != null )
              {
                var children = grandParentPaneContainer.Children.Where( child => ( child.Equals( parentPane ) && ( parentPane is ILayoutContainer ) && ( ( ( ILayoutContainer )parentPane ).ChildrenCount > 1 ) )
                                                                                || !child.Equals( parentPane ) )
                                                                .Cast<ILayoutPositionableElement>()
                                                                .Where( child => child.DockHeight.IsStar );
                var childrenTotalHeight = children.Sum( child => child.DockHeight.Value );
                foreach( var child in children )
                {
                  child.DockHeight = new GridLength( child.DockHeight.Value / childrenTotalHeight, GridUnitType.Star );
                }
              }
            }
          }
        }
      }
    }

    private void AnchorableContextMenu_Opened( object sender, RoutedEventArgs e )
    {
      var anchorableContextMenu = sender as ContextMenu;
      if( anchorableContextMenu != null )
      {
        if( anchorableContextMenu.PlacementTarget is Control )
        {
          anchorableContextMenu.VerticalOffset = ( ( Control )anchorableContextMenu.PlacementTarget ).ActualHeight - anchorableContextMenu.ActualHeight;
        }
        this.AnchorableContextMenu.Opened -= this.AnchorableContextMenu_Opened;
      }
    }

    #endregion

    #region Events

    public event EventHandler LayoutChanged;

    public event EventHandler LayoutChanging;

    public event EventHandler<DocumentClosingEventArgs> DocumentClosing;

    public event EventHandler<DocumentClosedEventArgs> DocumentClosed;

    public event EventHandler ActiveContentChanged;

    #endregion

    #region IOverlayWindowHost Interface

    bool IOverlayWindowHost.HitTest( Point dragPoint )
    {
      Rect detectionRect = new Rect( this.PointToScreenDPIWithoutFlowDirection( new Point() ), this.TransformActualSizeToAncestor() );
      return detectionRect.Contains( dragPoint );
    }

    DockingManager IOverlayWindowHost.Manager
    {
      get
      {
        return this;
      }
    }

    IOverlayWindow IOverlayWindowHost.ShowOverlayWindow( LayoutFloatingWindowControl draggingWindow )
    {
      CreateOverlayWindow();
      _overlayWindow.Owner = draggingWindow;
      _overlayWindow.EnableDropTargets();
      _overlayWindow.Show();
      return _overlayWindow;
    }

    void IOverlayWindowHost.HideOverlayWindow()
    {
      _areas = null;
      _overlayWindow.Owner = null;
      _overlayWindow.HideDropTargets();
    }

    IEnumerable<IDropArea> IOverlayWindowHost.GetDropAreas( LayoutFloatingWindowControl draggingWindow )
    {
      if( _areas != null )
        return _areas;

      var draggingWindowManager = draggingWindow.Model.Root.Manager;
      bool isDraggingDocuments = draggingWindow.Model is LayoutDocumentFloatingWindow;

      _areas = new List<IDropArea>();

      if( !isDraggingDocuments )
      {
        if( draggingWindowManager == this )
        {
          _areas.Add( new DropArea<DockingManager>(
          this,
          DropAreaType.DockingManager ) );
        }

        foreach( var areaHost in this.FindVisualChildren<LayoutAnchorablePaneControl>() )
        {
          if( areaHost.Model.Descendents().Any() )
          {
            if( draggingWindowManager == areaHost.Model.Root.Manager )
            {
              _areas.Add( new DropArea<LayoutAnchorablePaneControl>(
                areaHost,
                DropAreaType.AnchorablePane ) );
            }
          }
        }
      }

      foreach( var areaHost in this.FindVisualChildren<LayoutDocumentPaneControl>() )
      {
        if( isDraggingDocuments
          || ( !isDraggingDocuments && draggingWindow.Model.Descendents().OfType<LayoutAnchorable>().All( x => x.CanDockAsTabbedDocument ) ) )
        {
          if( draggingWindowManager == areaHost.Model.Root.Manager )
          {
            _areas.Add( new DropArea<LayoutDocumentPaneControl>(
              areaHost,
              DropAreaType.DocumentPane ) );
          }
        }
      }

      foreach( var areaHost in this.FindVisualChildren<LayoutDocumentPaneGroupControl>() )
      {
        var documentGroupModel = areaHost.Model as LayoutDocumentPaneGroup;
        if( documentGroupModel.Children.Where( c => c.IsVisible ).Count() == 0 )
        {
          if( draggingWindowManager == areaHost.Model.Root.Manager )
          {
            _areas.Add( new DropArea<LayoutDocumentPaneGroupControl>(
              areaHost,
              DropAreaType.DocumentPaneGroup ) );
          }
        }
      }

      return _areas;
    }

    public static readonly RoutedEvent PreviewFloatEvent = EventManager.RegisterRoutedEvent( "PreviewFloat", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( DockingManager ) );

    public event RoutedEventHandler PreviewFloat
    {
      add
      {
        AddHandler( PreviewFloatEvent, value );
      }
      remove
      {
        RemoveHandler( PreviewFloatEvent, value );
      }
    }

    protected virtual void RaisePreviewFloatEvent( LayoutContent layoutContent )
    {
      var args = new RoutedEventArgs( DockingManager.PreviewFloatEvent, layoutContent );
      RaiseEvent( args );
    }

    public static readonly RoutedEvent FloatedEvent = EventManager.RegisterRoutedEvent( "Floated", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( DockingManager ) );

    public event RoutedEventHandler Floated
    {
      add
      {
        AddHandler( FloatedEvent, value );
      }
      remove
      {
        RemoveHandler( FloatedEvent, value );
      }
    }

    protected virtual void RaiseFloatedEvent( LayoutContent layoutContent )
    {
      var args = new RoutedEventArgs( DockingManager.FloatedEvent, layoutContent );
      RaiseEvent( args );
    }


    public static readonly RoutedEvent PreviewDockEvent = EventManager.RegisterRoutedEvent( "PreviewDock", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( DockingManager ) );

    public event RoutedEventHandler PreviewDock
    {
      add
      {
        AddHandler( PreviewDockEvent, value );
      }
      remove
      {
        RemoveHandler( PreviewDockEvent, value );
      }
    }

    protected internal virtual void RaisePreviewDockEvent( LayoutContent layoutContent )
    {
      var args = new RoutedEventArgs( DockingManager.PreviewDockEvent, layoutContent );
      RaiseEvent( args );
    }

    public static readonly RoutedEvent DockedEvent = EventManager.RegisterRoutedEvent( "Docked", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( DockingManager ) );

    public event RoutedEventHandler Docked
    {
      add
      {
        AddHandler( DockedEvent, value );
      }
      remove
      {
        RemoveHandler( DockedEvent, value );
      }
    }

    protected internal virtual void RaiseDockedEvent( LayoutContent layoutContent )
    {
      var args = new RoutedEventArgs( DockingManager.DockedEvent, layoutContent );
      RaiseEvent( args );
    }

    #endregion

    #region IWeakEventListener

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( typeof( CollectionChangedEventManager ) == managerType )
      {
        var args = ( NotifyCollectionChangedEventArgs )e;
        if( sender == this.DocumentsSource )
        {
          this.DocumentsSourceElementsChanged( sender, args );
        }
        else if( sender == this.AnchorablesSource )
        {
          this.anchorablesSourceElementsChanged( sender, args );
        }
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

  }


}


