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
using System.Globalization;

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











    #region Layout

    /// <summary>
    /// Layout Dependency Property
    /// </summary>
    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register( "Layout", typeof( LayoutRoot ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnLayoutChanged ), new CoerceValueCallback( CoerceLayoutValue ) ) );

    /// <summary>
    /// Gets or sets the Layout property.  This dependency property 
    /// indicates layout tree.
    /// </summary>
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

    /// <summary>
    /// Coerces the <see cref="DockingManager.Layout"/> value.
    /// </summary>
    private static object CoerceLayoutValue( DependencyObject d, object value )
    {
      if( value == null )
        return new LayoutRoot() { RootPanel = new LayoutPanel( new LayoutDocumentPaneGroup( new LayoutDocumentPane() ) ) };

      ( ( DockingManager )d ).OnLayoutChanging( value as LayoutRoot );

      return value;
    }

    /// <summary>
    /// Handles changes to the Layout property.
    /// </summary>
    private static void OnLayoutChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLayoutChanged( e.OldValue as LayoutRoot, e.NewValue as LayoutRoot );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the <see cref="DockingManager.Layout"/> property.
    /// </summary>
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

    /// <summary>
    /// LayoutUpdateStrategy Dependency Property
    /// </summary>
    public static readonly DependencyProperty LayoutUpdateStrategyProperty = DependencyProperty.Register( "LayoutUpdateStrategy", typeof( ILayoutUpdateStrategy ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ILayoutUpdateStrategy )null ) );

    /// <summary>
    /// Gets or sets the LayoutUpdateStrategy property.  This dependency property 
    /// indicates the strategy class to call when AvalonDock needs to positionate a LayoutAnchorable inside an existing layout.
    /// </summary>
    /// <remarks>Sometimes it's impossible to automatically insert an anchorable in the layout without specifing the target parent pane.
    /// Set this property to an object that will be asked to insert the anchorable to the desidered position.</remarks>
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

    #region DocumentPaneTemplate

    /// <summary>
    /// DocumentPaneTemplate Dependency Property
    /// </summary>
    public static readonly DependencyProperty DocumentPaneTemplateProperty = DependencyProperty.Register( "DocumentPaneTemplate", typeof( ControlTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ControlTemplate )null, new PropertyChangedCallback( OnDocumentPaneTemplateChanged ) ) );

    /// <summary>
    /// Gets or sets the DocumentPaneDataTemplate property.  This dependency property 
    /// indicates .
    /// </summary>
    public ControlTemplate DocumentPaneTemplate
    {
      get
      {
        return ( ControlTemplate )GetValue( DocumentPaneTemplateProperty );
      }
      set
      {
        SetValue( DocumentPaneTemplateProperty, value );
      }
    }

    /// <summary>
    /// Handles changes to the DocumentPaneTemplate property.
    /// </summary>
    private static void OnDocumentPaneTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentPaneTemplateChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the DocumentPaneTemplate property.
    /// </summary>
    protected virtual void OnDocumentPaneTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    #endregion

    #region AnchorablePaneTemplate

    /// <summary>
    /// AnchorablePaneTemplate Dependency Property
    /// </summary>
    public static readonly DependencyProperty AnchorablePaneTemplateProperty = DependencyProperty.Register( "AnchorablePaneTemplate", typeof( ControlTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ControlTemplate )null, new PropertyChangedCallback( OnAnchorablePaneTemplateChanged ) ) );

    /// <summary>
    /// Gets or sets the AnchorablePaneTemplate property.  This dependency property 
    /// indicates ....
    /// </summary>
    public ControlTemplate AnchorablePaneTemplate
    {
      get
      {
        return ( ControlTemplate )GetValue( AnchorablePaneTemplateProperty );
      }
      set
      {
        SetValue( AnchorablePaneTemplateProperty, value );
      }
    }

    /// <summary>
    /// Handles changes to the AnchorablePaneDataTemplate property.
    /// </summary>
    private static void OnAnchorablePaneTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAnchorablePaneTemplateChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the AnchorablePaneDataTemplate property.
    /// </summary>
    protected virtual void OnAnchorablePaneTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    #endregion

    #region AnchorSideTemplate

    /// <summary>
    /// AnchorSideTemplate Dependency Property
    /// </summary>
    public static readonly DependencyProperty AnchorSideTemplateProperty = DependencyProperty.Register( "AnchorSideTemplate", typeof( ControlTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ControlTemplate )null ) );

    /// <summary>
    /// Gets or sets the AnchorSideTemplate property.  This dependency property 
    /// indicates ....
    /// </summary>
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

    /// <summary>
    /// AnchorGroupTemplate Dependency Property
    /// </summary>
    public static readonly DependencyProperty AnchorGroupTemplateProperty = DependencyProperty.Register( "AnchorGroupTemplate", typeof( ControlTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ControlTemplate )null ) );

    /// <summary>
    /// Gets or sets the AnchorGroupTemplate property.  This dependency property 
    /// indicates the template used to render the AnchorGroup control.
    /// </summary>
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

    /// <summary>
    /// AnchorTemplate Dependency Property
    /// </summary>
    public static readonly DependencyProperty AnchorTemplateProperty = DependencyProperty.Register( "AnchorTemplate", typeof( ControlTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ControlTemplate )null ) );

    /// <summary>
    /// Gets or sets the AnchorTemplate property.  This dependency property 
    /// indicates ....
    /// </summary>
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

    /// <summary>
    /// DocumentPaneControlStyle Dependency Property
    /// </summary>
    public static readonly DependencyProperty DocumentPaneControlStyleProperty = DependencyProperty.Register( "DocumentPaneControlStyle", typeof( Style ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( Style )null, new PropertyChangedCallback( OnDocumentPaneControlStyleChanged ) ) );

    /// <summary>
    /// Gets or sets the DocumentPaneControlStyle property.  This dependency property 
    /// indicates ....
    /// </summary>
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

    /// <summary>
    /// Handles changes to the DocumentPaneControlStyle property.
    /// </summary>
    private static void OnDocumentPaneControlStyleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentPaneControlStyleChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the DocumentPaneControlStyle property.
    /// </summary>
    protected virtual void OnDocumentPaneControlStyleChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    #endregion

    #region AnchorablePaneControlStyle

    /// <summary>
    /// AnchorablePaneControlStyle Dependency Property
    /// </summary>
    public static readonly DependencyProperty AnchorablePaneControlStyleProperty = DependencyProperty.Register( "AnchorablePaneControlStyle", typeof( Style ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( Style )null, new PropertyChangedCallback( OnAnchorablePaneControlStyleChanged ) ) );

    /// <summary>
    /// Gets or sets the AnchorablePaneControlStyle property.  This dependency property 
    /// indicates the style to apply to AnchorablePaneControl.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the AnchorablePaneControlStyle property.
    /// </summary>
    private static void OnAnchorablePaneControlStyleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAnchorablePaneControlStyleChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the AnchorablePaneControlStyle property.
    /// </summary>
    protected virtual void OnAnchorablePaneControlStyleChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    #endregion

    #region DocumentHeaderTemplate

    /// <summary>
    /// DocumentHeaderTemplate Dependency Property
    /// </summary>
    public static readonly DependencyProperty DocumentHeaderTemplateProperty = DependencyProperty.Register( "DocumentHeaderTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null, new PropertyChangedCallback( OnDocumentHeaderTemplateChanged ), new CoerceValueCallback( CoerceDocumentHeaderTemplateValue ) ) );

    /// <summary>
    /// Gets or sets the DocumentHeaderTemplate property.  This dependency property 
    /// indicates data template to use for document header.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the DocumentHeaderTemplate property.
    /// </summary>
    private static void OnDocumentHeaderTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentHeaderTemplateChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the DocumentHeaderTemplate property.
    /// </summary>
    protected virtual void OnDocumentHeaderTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    /// <summary>
    /// Coerces the DocumentHeaderTemplate value.
    /// </summary>
    private static object CoerceDocumentHeaderTemplateValue( DependencyObject d, object value )
    {
      if( value != null &&
          d.GetValue( DocumentHeaderTemplateSelectorProperty ) != null )
        return null;
      return value;
    }

    #endregion

    #region DocumentHeaderTemplateSelector

    /// <summary>
    /// DocumentHeaderTemplateSelector Dependency Property
    /// </summary>
    public static readonly DependencyProperty DocumentHeaderTemplateSelectorProperty = DependencyProperty.Register( "DocumentHeaderTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null, new PropertyChangedCallback( OnDocumentHeaderTemplateSelectorChanged ), new CoerceValueCallback( CoerceDocumentHeaderTemplateSelectorValue ) ) );

    /// <summary>
    /// Gets or sets the DocumentHeaderTemplateSelector property.  This dependency property 
    /// indicates the template selector that is used when selcting the data template for the header.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the DocumentHeaderTemplateSelector property.
    /// </summary>
    private static void OnDocumentHeaderTemplateSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentHeaderTemplateSelectorChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the DocumentHeaderTemplateSelector property.
    /// </summary>
    protected virtual void OnDocumentHeaderTemplateSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.NewValue != null &&
          DocumentHeaderTemplate != null )
        DocumentHeaderTemplate = null;

      if( DocumentPaneMenuItemHeaderTemplateSelector == null )
        DocumentPaneMenuItemHeaderTemplateSelector = DocumentHeaderTemplateSelector;

    }

    /// <summary>
    /// Coerces the DocumentHeaderTemplateSelector value.
    /// </summary>
    private static object CoerceDocumentHeaderTemplateSelectorValue( DependencyObject d, object value )
    {
      return value;
    }

    #endregion

    #region DocumentTitleTemplate

    /// <summary>
    /// DocumentTitleTemplate Dependency Property
    /// </summary>
    public static readonly DependencyProperty DocumentTitleTemplateProperty = DependencyProperty.Register( "DocumentTitleTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null, new PropertyChangedCallback( OnDocumentTitleTemplateChanged ), new CoerceValueCallback( CoerceDocumentTitleTemplateValue ) ) );

    /// <summary>
    /// Gets or sets the DocumentTitleTemplate property.  This dependency property 
    /// indicates the datatemplate to use when creating the title for a document.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the DocumentTitleTemplate property.
    /// </summary>
    private static void OnDocumentTitleTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentTitleTemplateChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the DocumentTitleTemplate property.
    /// </summary>
    protected virtual void OnDocumentTitleTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    /// <summary>
    /// Coerces the DocumentTitleTemplate value.
    /// </summary>
    private static object CoerceDocumentTitleTemplateValue( DependencyObject d, object value )
    {
      if( value != null &&
          d.GetValue( DocumentTitleTemplateSelectorProperty ) != null )
        return null;

      return value;
    }

    #endregion

    #region DocumentTitleTemplateSelector

    /// <summary>
    /// DocumentTitleTemplateSelector Dependency Property
    /// </summary>
    public static readonly DependencyProperty DocumentTitleTemplateSelectorProperty = DependencyProperty.Register( "DocumentTitleTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null, new PropertyChangedCallback( OnDocumentTitleTemplateSelectorChanged ), new CoerceValueCallback( CoerceDocumentTitleTemplateSelectorValue ) ) );

    /// <summary>
    /// Gets or sets the DocumentTitleTemplateSelector property.  This dependency property 
    /// indicates the data template selector to use when creating the data template for the title.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the DocumentTitleTemplateSelector property.
    /// </summary>
    private static void OnDocumentTitleTemplateSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentTitleTemplateSelectorChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the DocumentTitleTemplateSelector property.
    /// </summary>
    protected virtual void OnDocumentTitleTemplateSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.NewValue != null )
        DocumentTitleTemplate = null;
    }

    /// <summary>
    /// Coerces the DocumentTitleTemplateSelector value.
    /// </summary>
    private static object CoerceDocumentTitleTemplateSelectorValue( DependencyObject d, object value )
    {
      return value;
    }

    #endregion

    #region AnchorableTitleTemplate

    /// <summary>
    /// AnchorableTitleTemplate Dependency Property
    /// </summary>
    public static readonly DependencyProperty AnchorableTitleTemplateProperty = DependencyProperty.Register( "AnchorableTitleTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null, new PropertyChangedCallback( OnAnchorableTitleTemplateChanged ), new CoerceValueCallback( CoerceAnchorableTitleTemplateValue ) ) );

    /// <summary>
    /// Gets or sets the AnchorableTitleTemplate property.  This dependency property 
    /// indicates the data template to use for anchorables title.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the AnchorableTitleTemplate property.
    /// </summary>
    private static void OnAnchorableTitleTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAnchorableTitleTemplateChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the AnchorableTitleTemplate property.
    /// </summary>
    protected virtual void OnAnchorableTitleTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    /// <summary>
    /// Coerces the AnchorableTitleTemplate value.
    /// </summary>
    private static object CoerceAnchorableTitleTemplateValue( DependencyObject d, object value )
    {
      if( value != null &&
          d.GetValue( AnchorableTitleTemplateSelectorProperty ) != null )
        return null;
      return value;
    }

    #endregion

    #region AnchorableTitleTemplateSelector

    /// <summary>
    /// AnchorableTitleTemplateSelector Dependency Property
    /// </summary>
    public static readonly DependencyProperty AnchorableTitleTemplateSelectorProperty = DependencyProperty.Register( "AnchorableTitleTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null, new PropertyChangedCallback( OnAnchorableTitleTemplateSelectorChanged ) ) );

    /// <summary>
    /// Gets or sets the AnchorableTitleTemplateSelector property.  This dependency property 
    /// indicates selctor to use when selecting data template for the title of anchorables.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the AnchorableTitleTemplateSelector property.
    /// </summary>
    private static void OnAnchorableTitleTemplateSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAnchorableTitleTemplateSelectorChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the AnchorableTitleTemplateSelector property.
    /// </summary>
    protected virtual void OnAnchorableTitleTemplateSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.NewValue != null &&
          AnchorableTitleTemplate != null )
        AnchorableTitleTemplate = null;
    }

    #endregion

    #region AnchorableHeaderTemplate

    /// <summary>
    /// AnchorableHeaderTemplate Dependency Property
    /// </summary>
    public static readonly DependencyProperty AnchorableHeaderTemplateProperty = DependencyProperty.Register( "AnchorableHeaderTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null, new PropertyChangedCallback( OnAnchorableHeaderTemplateChanged ), new CoerceValueCallback( CoerceAnchorableHeaderTemplateValue ) ) );

    /// <summary>
    /// Gets or sets the AnchorableHeaderTemplate property.  This dependency property 
    /// indicates the data template to use for anchorable templates.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the AnchorableHeaderTemplate property.
    /// </summary>
    private static void OnAnchorableHeaderTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAnchorableHeaderTemplateChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the AnchorableHeaderTemplate property.
    /// </summary>
    protected virtual void OnAnchorableHeaderTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    /// <summary>
    /// Coerces the AnchorableHeaderTemplate value.
    /// </summary>
    private static object CoerceAnchorableHeaderTemplateValue( DependencyObject d, object value )
    {
      if( value != null &&
          d.GetValue( AnchorableHeaderTemplateSelectorProperty ) != null )
        return null;

      return value;
    }

    #endregion

    #region AnchorableHeaderTemplateSelector

    /// <summary>
    /// AnchorableHeaderTemplateSelector Dependency Property
    /// </summary>
    public static readonly DependencyProperty AnchorableHeaderTemplateSelectorProperty = DependencyProperty.Register( "AnchorableHeaderTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null, new PropertyChangedCallback( OnAnchorableHeaderTemplateSelectorChanged ) ) );

    /// <summary>
    /// Gets or sets the AnchorableHeaderTemplateSelector property.  This dependency property 
    /// indicates the selector to use when selecting the data template for anchorable headers.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the AnchorableHeaderTemplateSelector property.
    /// </summary>
    private static void OnAnchorableHeaderTemplateSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAnchorableHeaderTemplateSelectorChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the AnchorableHeaderTemplateSelector property.
    /// </summary>
    protected virtual void OnAnchorableHeaderTemplateSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.NewValue != null )
        AnchorableHeaderTemplate = null;
    }

    #endregion

    #region LayoutRootPanel

    /// <summary>
    /// LayoutRootPanel Dependency Property
    /// </summary>
    public static readonly DependencyProperty LayoutRootPanelProperty = DependencyProperty.Register( "LayoutRootPanel", typeof( LayoutPanelControl ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( LayoutPanelControl )null, new PropertyChangedCallback( OnLayoutRootPanelChanged ) ) );

    /// <summary>
    /// Gets or sets the LayoutRootPanel property.  This dependency property 
    /// indicates the layout panel control which is attached to the Layout.Root property.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the LayoutRootPanel property.
    /// </summary>
    private static void OnLayoutRootPanelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLayoutRootPanelChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the LayoutRootPanel property.
    /// </summary>
    protected virtual void OnLayoutRootPanelChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.OldValue != null )
        InternalRemoveLogicalChild( e.OldValue );
      if( e.NewValue != null )
        InternalAddLogicalChild( e.NewValue );
    }

    #endregion

    #region RightSidePanel

    /// <summary>
    /// RightSidePanel Dependency Property
    /// </summary>
    public static readonly DependencyProperty RightSidePanelProperty = DependencyProperty.Register( "RightSidePanel", typeof( LayoutAnchorSideControl ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( LayoutAnchorSideControl )null, new PropertyChangedCallback( OnRightSidePanelChanged ) ) );

    /// <summary>
    /// Gets or sets the RightSidePanel property.  This dependency property 
    /// indicates right side anchor panel.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the RightSidePanel property.
    /// </summary>
    private static void OnRightSidePanelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnRightSidePanelChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the RightSidePanel property.
    /// </summary>
    protected virtual void OnRightSidePanelChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.OldValue != null )
        InternalRemoveLogicalChild( e.OldValue );
      if( e.NewValue != null )
        InternalAddLogicalChild( e.NewValue );
    }

    #endregion

    #region LeftSidePanel

    /// <summary>
    /// LeftSidePanel Dependency Property
    /// </summary>
    public static readonly DependencyProperty LeftSidePanelProperty = DependencyProperty.Register( "LeftSidePanel", typeof( LayoutAnchorSideControl ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( LayoutAnchorSideControl )null, new PropertyChangedCallback( OnLeftSidePanelChanged ) ) );

    /// <summary>
    /// Gets or sets the LeftSidePanel property.  This dependency property 
    /// indicates the left side panel control.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the LeftSidePanel property.
    /// </summary>
    private static void OnLeftSidePanelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLeftSidePanelChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the LeftSidePanel property.
    /// </summary>
    protected virtual void OnLeftSidePanelChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.OldValue != null )
        InternalRemoveLogicalChild( e.OldValue );
      if( e.NewValue != null )
        InternalAddLogicalChild( e.NewValue );
    }

    #endregion

    #region TopSidePanel

    /// <summary>
    /// TopSidePanel Dependency Property
    /// </summary>
    public static readonly DependencyProperty TopSidePanelProperty = DependencyProperty.Register( "TopSidePanel", typeof( LayoutAnchorSideControl ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( LayoutAnchorSideControl )null, new PropertyChangedCallback( OnTopSidePanelChanged ) ) );

    /// <summary>
    /// Gets or sets the TopSidePanel property.  This dependency property 
    /// indicates top side control panel.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the TopSidePanel property.
    /// </summary>
    private static void OnTopSidePanelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnTopSidePanelChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the TopSidePanel property.
    /// </summary>
    protected virtual void OnTopSidePanelChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.OldValue != null )
        InternalRemoveLogicalChild( e.OldValue );
      if( e.NewValue != null )
        InternalAddLogicalChild( e.NewValue );
    }

    #endregion

    #region BottomSidePanel

    /// <summary>
    /// BottomSidePanel Dependency Property
    /// </summary>
    public static readonly DependencyProperty BottomSidePanelProperty = DependencyProperty.Register( "BottomSidePanel", typeof( LayoutAnchorSideControl ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( LayoutAnchorSideControl )null, new PropertyChangedCallback( OnBottomSidePanelChanged ) ) );

    /// <summary>
    /// Gets or sets the BottomSidePanel property.  This dependency property 
    /// indicates bottom side panel control.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the BottomSidePanel property.
    /// </summary>
    private static void OnBottomSidePanelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnBottomSidePanelChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the BottomSidePanel property.
    /// </summary>
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
        _logicalChildren.Remove( wrToRemove );
      RemoveLogicalChild( element );
    }

    private void ClearLogicalChildrenList()
    {
      foreach( var child in _logicalChildren.Select( ch => ch.GetValueOrDefault<object>() ).ToArray() )
        RemoveLogicalChild( child );
      _logicalChildren.Clear();
    }

    #endregion

    #region AutoHideWindow

    /// <summary>
    /// AutoHideWindow Read-Only Dependency Property
    /// </summary>
    private static readonly DependencyPropertyKey AutoHideWindowPropertyKey = DependencyProperty.RegisterReadOnly( "AutoHideWindow", typeof( LayoutAutoHideWindowControl ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( LayoutAutoHideWindowControl )null, new PropertyChangedCallback( OnAutoHideWindowChanged ) ) );

    public static readonly DependencyProperty AutoHideWindowProperty = AutoHideWindowPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the AutoHideWindow property.  This dependency property 
    /// indicates the currently shown autohide window.
    /// </summary>
    public LayoutAutoHideWindowControl AutoHideWindow
    {
      get
      {
        return ( LayoutAutoHideWindowControl )GetValue( AutoHideWindowProperty );
      }
    }

    /// <summary>
    /// Provides a secure method for setting the AutoHideWindow property.  
    /// This dependency property indicates the currently shown autohide window.
    /// </summary>
    /// <param name="value">The new value for the property.</param>
    protected void SetAutoHideWindow( LayoutAutoHideWindowControl value )
    {
      SetValue( AutoHideWindowPropertyKey, value );
    }

    /// <summary>
    /// Handles changes to the AutoHideWindow property.
    /// </summary>
    private static void OnAutoHideWindowChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAutoHideWindowChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the AutoHideWindow property.
    /// </summary>
    protected virtual void OnAutoHideWindowChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.OldValue != null )
        InternalRemoveLogicalChild( e.OldValue );
      if( e.NewValue != null )
        InternalAddLogicalChild( e.NewValue );

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

    /// <summary>
    /// LayoutItemTemplate Dependency Property
    /// </summary>
    public static readonly DependencyProperty LayoutItemTemplateProperty = DependencyProperty.Register( "LayoutItemTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null, new PropertyChangedCallback( OnLayoutItemTemplateChanged ) ) );

    /// <summary>
    /// Gets or sets the AnchorableTemplate property.  This dependency property 
    /// indicates the template to use to render anchorable and document contents.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the AnchorableTemplate property.
    /// </summary>
    private static void OnLayoutItemTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLayoutItemTemplateChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the AnchorableTemplate property.
    /// </summary>
    protected virtual void OnLayoutItemTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    #endregion

    #region LayoutItemTemplateSelector

    /// <summary>
    /// LayoutItemTemplateSelector Dependency Property
    /// </summary>
    public static readonly DependencyProperty LayoutItemTemplateSelectorProperty = DependencyProperty.Register( "LayoutItemTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null, new PropertyChangedCallback( OnLayoutItemTemplateSelectorChanged ) ) );

    /// <summary>
    /// Gets or sets the LayoutItemTemplateSelector property.  This dependency property 
    /// indicates selector object to use for anchorable templates.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the LayoutItemTemplateSelector property.
    /// </summary>
    private static void OnLayoutItemTemplateSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLayoutItemTemplateSelectorChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the LayoutItemTemplateSelector property.
    /// </summary>
    protected virtual void OnLayoutItemTemplateSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    #endregion

    #region DocumentsSource

    /// <summary>
    /// DocumentsSource Dependency Property
    /// </summary>
    public static readonly DependencyProperty DocumentsSourceProperty = DependencyProperty.Register( "DocumentsSource", typeof( IEnumerable ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( IEnumerable )null, new PropertyChangedCallback( OnDocumentsSourceChanged ) ) );

    /// <summary>
    /// Gets or sets the DocumentsSource property.  This dependency property 
    /// indicates the source collection of documents.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the DocumentsSource property.
    /// </summary>
    private static void OnDocumentsSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentsSourceChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the DocumentsSource property.
    /// </summary>
    protected virtual void OnDocumentsSourceChanged( DependencyPropertyChangedEventArgs e )
    {
      DetachDocumentsSource( Layout, e.OldValue as IEnumerable );
      AttachDocumentsSource( Layout, e.NewValue as IEnumerable );
    }

    #endregion

    #region DocumentContextMenu

    /// <summary>
    /// DocumentContextMenu Dependency Property
    /// </summary>
    public static readonly DependencyProperty DocumentContextMenuProperty = DependencyProperty.Register( "DocumentContextMenu", typeof( ContextMenu ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ContextMenu )null ) );

    /// <summary>
    /// Gets or sets the DocumentContextMenu property.  This dependency property 
    /// indicates context menu to show for documents.
    /// </summary>
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

    /// <summary>
    /// AnchorablesSource Dependency Property
    /// </summary>
    public static readonly DependencyProperty AnchorablesSourceProperty = DependencyProperty.Register( "AnchorablesSource", typeof( IEnumerable ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( IEnumerable )null, new PropertyChangedCallback( OnAnchorablesSourceChanged ) ) );

    /// <summary>
    /// Gets or sets the AnchorablesSource property.  This dependency property 
    /// indicates source collection of anchorables.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the AnchorablesSource property.
    /// </summary>
    private static void OnAnchorablesSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnAnchorablesSourceChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the AnchorablesSource property.
    /// </summary>
    protected virtual void OnAnchorablesSourceChanged( DependencyPropertyChangedEventArgs e )
    {
      DetachAnchorablesSource( Layout, e.OldValue as IEnumerable );
      AttachAnchorablesSource( Layout, e.NewValue as IEnumerable );
    }



    #endregion

    #region ActiveContent

    /// <summary>
    /// ActiveContent Dependency Property
    /// </summary>
    public static readonly DependencyProperty ActiveContentProperty = DependencyProperty.Register( "ActiveContent", typeof( object ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( object )null, new PropertyChangedCallback( OnActiveContentChanged ) ) );

    /// <summary>
    /// Gets or sets the ActiveContent property.  This dependency property 
    /// indicates the content currently active.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the ActiveContent property.
    /// </summary>
    private static void OnActiveContentChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).InternalSetActiveContent( e.NewValue );
      ( ( DockingManager )d ).OnActiveContentChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the ActiveContent property.
    /// </summary>
    protected virtual void OnActiveContentChanged( DependencyPropertyChangedEventArgs e )
    {
      if( ActiveContentChanged != null )
        ActiveContentChanged( this, EventArgs.Empty );
    }

    #endregion

    #region AnchorableContextMenu

    /// <summary>
    /// AnchorableContextMenu Dependency Property
    /// </summary>
    public static readonly DependencyProperty AnchorableContextMenuProperty = DependencyProperty.Register( "AnchorableContextMenu", typeof( ContextMenu ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( ContextMenu )null ) );

    /// <summary>
    /// Gets or sets the AnchorableContextMenu property.  This dependency property 
    /// indicates the context menu to show up for anchorables.
    /// </summary>
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

    /// <summary>
    /// Theme Dependency Property
    /// </summary>
    public static readonly DependencyProperty ThemeProperty = DependencyProperty.Register( "Theme", typeof( Theme ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnThemeChanged ) ) );

    /// <summary>
    /// Gets or sets the Theme property.  This dependency property 
    /// indicates the theme to use for AvalonDock controls.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the Theme property.
    /// </summary>
    private static void OnThemeChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnThemeChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the Theme property.
    /// </summary>
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
        _navigatorWindow.UpdateThemeResources();

      if( _overlayWindow != null )
        _overlayWindow.UpdateThemeResources();
    }

    #endregion

    #region GridSplitterWidth

    /// <summary>
    /// GridSplitterWidth Dependency Property
    /// </summary>
    public static readonly DependencyProperty GridSplitterWidthProperty = DependencyProperty.Register( "GridSplitterWidth", typeof( double ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( double )6.0 ) );

    /// <summary>
    /// Gets or sets the GridSplitterWidth property.  This dependency property 
    /// indicates width of grid splitters.
    /// </summary>
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

    /// <summary>
    /// GridSplitterHeight Dependency Property
    /// </summary>
    public static readonly DependencyProperty GridSplitterHeightProperty = DependencyProperty.Register( "GridSplitterHeight", typeof( double ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( double )6.0 ) );

    /// <summary>
    /// Gets or sets the GridSplitterHeight property.  This dependency property 
    /// indicates height of grid splitters.
    /// </summary>
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

    /// <summary>
    /// DocumentPaneMenuItemHeaderTemplate Dependency Property
    /// </summary>
    public static readonly DependencyProperty DocumentPaneMenuItemHeaderTemplateProperty = DependencyProperty.Register( "DocumentPaneMenuItemHeaderTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null, new PropertyChangedCallback( OnDocumentPaneMenuItemHeaderTemplateChanged ), new CoerceValueCallback( CoerceDocumentPaneMenuItemHeaderTemplateValue ) ) );

    /// <summary>
    /// Gets or sets the DocumentPaneMenuItemHeaderTemplate property.  This dependency property 
    /// indicates the header template to use while creating menu items for the document panes.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the DocumentPaneMenuItemHeaderTemplate property.
    /// </summary>
    private static void OnDocumentPaneMenuItemHeaderTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentPaneMenuItemHeaderTemplateChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the DocumentPaneMenuItemHeaderTemplate property.
    /// </summary>
    protected virtual void OnDocumentPaneMenuItemHeaderTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    /// <summary>
    /// Coerces the DocumentPaneMenuItemHeaderTemplate value.
    /// </summary>
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

    /// <summary>
    /// DocumentPaneMenuItemHeaderTemplateSelector Dependency Property
    /// </summary>
    public static readonly DependencyProperty DocumentPaneMenuItemHeaderTemplateSelectorProperty = DependencyProperty.Register( "DocumentPaneMenuItemHeaderTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null, new PropertyChangedCallback( OnDocumentPaneMenuItemHeaderTemplateSelectorChanged ), new CoerceValueCallback( CoerceDocumentPaneMenuItemHeaderTemplateSelectorValue ) ) );

    /// <summary>
    /// Gets or sets the DocumentPaneMenuItemHeaderTemplateSelector property.  This dependency property 
    /// indicates the data template selector to use for the menu items show when user select the DocumentPane document switch context menu.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the DocumentPaneMenuItemHeaderTemplateSelector property.
    /// </summary>
    private static void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnDocumentPaneMenuItemHeaderTemplateSelectorChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the DocumentPaneMenuItemHeaderTemplateSelector property.
    /// </summary>
    protected virtual void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
      if( e.NewValue != null &&
          DocumentPaneMenuItemHeaderTemplate != null )
        DocumentPaneMenuItemHeaderTemplate = null;

    }

    /// <summary>
    /// Coerces the DocumentPaneMenuItemHeaderTemplateSelector value.
    /// </summary>
    private static object CoerceDocumentPaneMenuItemHeaderTemplateSelectorValue( DependencyObject d, object value )
    {
      return value;
    }

    #endregion

    #region IconContentTemplate

    /// <summary>
    /// IconContentTemplate Dependency Property
    /// </summary>
    public static readonly DependencyProperty IconContentTemplateProperty = DependencyProperty.Register( "IconContentTemplate", typeof( DataTemplate ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplate )null ) );

    /// <summary>
    /// Gets or sets the IconContentTemplate property.  This dependency property 
    /// indicates the data template to use while extracting the icon from model.
    /// </summary>
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

    /// <summary>
    /// IconContentTemplateSelector Dependency Property
    /// </summary>
    public static readonly DependencyProperty IconContentTemplateSelectorProperty = DependencyProperty.Register( "IconContentTemplateSelector", typeof( DataTemplateSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( DataTemplateSelector )null ) );

    /// <summary>
    /// Gets or sets the IconContentTemplateSelector property.  This dependency property 
    /// indicates data template selector to use while selecting the datatamplate for content icons.
    /// </summary>
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

    /// <summary>
    /// LayoutItemContainerStyle Dependency Property
    /// </summary>
    public static readonly DependencyProperty LayoutItemContainerStyleProperty = DependencyProperty.Register( "LayoutItemContainerStyle", typeof( Style ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( Style )null, new PropertyChangedCallback( OnLayoutItemContainerStyleChanged ) ) );

    /// <summary>
    /// Gets or sets the LayoutItemContainerStyle property.  This dependency property 
    /// indicates the style to apply to LayoutDocumentItem objects. A LayoutDocumentItem object is created when a new LayoutDocument is created inside the current Layout.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the LayoutItemContainerStyle property.
    /// </summary>
    private static void OnLayoutItemContainerStyleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLayoutItemContainerStyleChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the LayoutItemContainerStyle property.
    /// </summary>
    protected virtual void OnLayoutItemContainerStyleChanged( DependencyPropertyChangedEventArgs e )
    {
      AttachLayoutItems();
    }

    #endregion

    #region LayoutItemContainerStyleSelector

    /// <summary>
    /// LayoutItemContainerStyleSelector Dependency Property
    /// </summary>
    public static readonly DependencyProperty LayoutItemContainerStyleSelectorProperty = DependencyProperty.Register( "LayoutItemContainerStyleSelector", typeof( StyleSelector ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( StyleSelector )null, new PropertyChangedCallback( OnLayoutItemContainerStyleSelectorChanged ) ) );

    /// <summary>
    /// Gets or sets the LayoutItemContainerStyleSelector property.  This dependency property 
    /// indicates style selector of the LayoutDocumentItemStyle.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the LayoutItemContainerStyleSelector property.
    /// </summary>
    private static void OnLayoutItemContainerStyleSelectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( DockingManager )d ).OnLayoutItemContainerStyleSelectorChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the LayoutItemContainerStyleSelector property.
    /// </summary>
    protected virtual void OnLayoutItemContainerStyleSelectorChanged( DependencyPropertyChangedEventArgs e )
    {
      AttachLayoutItems();
    }

    #endregion

    #region ShowSystemMenu

    /// <summary>
    /// ShowSystemMenu Dependency Property
    /// </summary>
    public static readonly DependencyProperty ShowSystemMenuProperty = DependencyProperty.Register( "ShowSystemMenu", typeof( bool ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( bool )true ) );

    /// <summary>
    /// Gets or sets the ShowSystemMenu property.  This dependency property 
    /// indicates if floating windows should show the system menu when a custom context menu is not defined.
    /// </summary>
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

    /// <summary>
    /// AllowMixedOrientation Dependency Property
    /// </summary>
    public static readonly DependencyProperty AllowMixedOrientationProperty = DependencyProperty.Register( "AllowMixedOrientation", typeof( bool ), typeof( DockingManager ),
            new FrameworkPropertyMetadata( ( bool )false ) );

    /// <summary>
    /// Gets or sets the AllowMixedOrientation property.  This dependency property 
    /// indicates if the manager should allow mixed orientation for document panes.
    /// </summary>
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
              if( ( lac != null ) && ( this.AnchorableContextMenu != null) )
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

    /// <summary>
    /// Return the LayoutItem wrapper for the content passed as argument
    /// </summary>
    /// <param name="content">LayoutContent to search</param>
    /// <returns>Either a LayoutAnchorableItem or LayoutDocumentItem which contains the LayoutContent passed as argument</returns>
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
           newFW.Show();

          // Do not set the WindowState before showing or it will be lost
          if( paneForExtensions != null && paneForExtensions.IsMaximized )
          {
            newFW.WindowState = WindowState.Maximized;
          }
        } ), DispatcherPriority.Send );

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
      if( ( contentModel == null) || !contentModel.CanFloat )
        return;

      var fwc = this.CreateFloatingWindow( contentModel, false );
      if( fwc != null )
      {
        Dispatcher.BeginInvoke( new Action( () =>
        {
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
        model.CloseAnchorable();
        this.RemoveViewFromLogicalChild( anchorable );
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

    internal void ShowNavigatorWindow()
    {
      if( _navigatorWindow == null )
      {
        _navigatorWindow = new NavigatorWindow( this )
        {
          Owner = Window.GetWindow( this ),
          WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
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
          this.SetCurrentValue( DockingManager.ActiveContentProperty, ( Layout.ActiveContent != null ) ? Layout.ActiveContent.Content : null );
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
          fw.Close();
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

        //documentPane.Children.Add(new LayoutDocument() { Content = documentToImport });

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

    private void documentsSourceElementsChanged( object sender, NotifyCollectionChangedEventArgs e )
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
          foreach( var documentToRemove in documentsToRemove )
          {
            //documentToRemove.Content = null;
            ( documentToRemove.Parent as ILayoutContainer ).RemoveChild(
                documentToRemove );
            this.RemoveViewFromLogicalChild( documentToRemove );
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
          ( documentToRemove.Parent as ILayoutContainer ).RemoveChild(
              documentToRemove );
          this.RemoveViewFromLogicalChild( documentToRemove );
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
        ( documentToRemove.Parent as ILayoutContainer ).RemoveChild(
            documentToRemove );
        this.RemoveViewFromLogicalChild( documentToRemove );
      }

      var documentsSourceAsNotifier = documentsSource as INotifyCollectionChanged;
      if( documentsSourceAsNotifier != null )
      {
        CollectionChangedEventManager.RemoveListener( documentsSourceAsNotifier, this );
      }
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
          this.InternalRemoveLogicalChild( layoutItem.View );
        }
      }
    }

    private void InternalSetActiveContent( object contentObject )
    {
      var layoutContent = Layout.Descendents().OfType<LayoutContent>().FirstOrDefault( lc => lc == contentObject || lc.Content == contentObject );
      _insideInternalSetActiveContent = true;
      Layout.ActiveContent = layoutContent;
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
        foreach( var itemToRemove in _layoutItems.Where( item => item.LayoutElement.Root != Layout ).ToArray() )
        {

          if( itemToRemove != null &&
                  itemToRemove.Model != null &&
                  itemToRemove.Model is UIElement )
          {
            //((ILogicalChildrenContainer)this).InternalRemoveLogicalChild(itemToRemove.Model as UIElement);
          }

          itemToRemove.Detach();
          _layoutItems.Remove( itemToRemove );

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

    private LayoutFloatingWindowControl CreateFloatingWindowForLayoutAnchorableWithoutParent( LayoutAnchorablePane paneModel, bool isContentImmutable )
    {
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



      //fwc.Owner = Window.GetWindow(this);
      //fwc.SetParentToMainWindowOf(this);


      _fwList.Add( fwc );

      Layout.CollectGarbage();

      InvalidateArrange();

      return fwc;
    }

    private LayoutFloatingWindowControl CreateFloatingWindowCore( LayoutContent contentModel, bool isContentImmutable )
    {
      if( !contentModel.CanFloat )
        return null;
      var contentModelAsAnchorable = contentModel as LayoutAnchorable;
      if( contentModelAsAnchorable != null &&
          contentModelAsAnchorable.IsAutoHidden )
        contentModelAsAnchorable.ToggleAutoHide();

      var parentPane = contentModel.Parent as ILayoutPane;
      var parentPaneAsPositionableElement = contentModel.Parent as ILayoutPositionableElement;
      var parentPaneAsWithActualSize = contentModel.Parent as ILayoutPositionableElementWithActualSize;
      var contentModelParentChildrenIndex = parentPane.Children.ToList().IndexOf( contentModel );

      if( contentModel.FindParent<LayoutFloatingWindow>() == null )
      {
        ( (ILayoutPreviousContainer)contentModel ).PreviousContainer = parentPane;
        contentModel.PreviousContainerIndex = contentModelParentChildrenIndex;
      }

      parentPane.RemoveChildAt( contentModelParentChildrenIndex );

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

        fwc = new LayoutAnchorableFloatingWindowControl(
            fw as LayoutAnchorableFloatingWindow, isContentImmutable )
        {
          Width = fwWidth,
          Height = fwHeight,
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

        fwc = new LayoutDocumentFloatingWindowControl(
            fw as LayoutDocumentFloatingWindow, isContentImmutable )
        {
          Width = fwWidth,
          Height = fwHeight,
          Left = contentModel.FloatingLeft,
          Top = contentModel.FloatingTop
        };
      }


      //fwc.Owner = Window.GetWindow(this);
      //fwc.SetParentToMainWindowOf(this);


      _fwList.Add( fwc );

      Layout.CollectGarbage();

      UpdateLayout();

      return fwc;
    }

    private void AnchorableContextMenu_Opened( object sender, RoutedEventArgs e )
    {
      var anchorableContextMenu = sender as ContextMenu;
      if( anchorableContextMenu != null )
      {
        if( anchorableContextMenu.PlacementTarget is Control )
        {
          anchorableContextMenu.VerticalOffset = ( (Control)anchorableContextMenu.PlacementTarget ).ActualHeight - anchorableContextMenu.ActualHeight;
        }
        this.AnchorableContextMenu.Opened -= this.AnchorableContextMenu_Opened;
      }
    }

    #endregion

    #region Events

    /// <summary>
    /// Event fired when <see cref="DockingManager.Layout"/> property changes
    /// </summary>
    public event EventHandler LayoutChanged;

    /// <summary>
    /// Event fired when <see cref="DockingManager.Layout"/> property is about to be changed
    /// </summary>
    public event EventHandler LayoutChanging;

    /// <summary>
    /// Event fired when a document is about to be closed
    /// </summary>
    /// <remarks>Subscribers have the opportuniy to cancel the operation.</remarks>
    public event EventHandler<DocumentClosingEventArgs> DocumentClosing;

    /// <summary>
    /// Event fired after a document is closed
    /// </summary>
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
        var args = (NotifyCollectionChangedEventArgs)e;
        if( sender == this.DocumentsSource )
        {
          this.documentsSourceElementsChanged( sender, args );
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
