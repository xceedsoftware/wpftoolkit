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
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Security;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xceed.Wpf.Toolkit.Core.Input;
using Xceed.Wpf.Toolkit.Core;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit.Zoombox
{
  [TemplatePart( Name = PART_VerticalScrollBar, Type = typeof( ScrollBar ) )]
  [TemplatePart( Name = PART_HorizontalScrollBar, Type = typeof( ScrollBar ) )]
  public sealed class Zoombox : ContentControl
  {
    private const string PART_VerticalScrollBar = "PART_VerticalScrollBar";
    private const string PART_HorizontalScrollBar = "PART_HorizontalScrollBar";
    private bool _isUpdatingVisualTree = false;

    #region Constructors

    static Zoombox()
    {
      Zoombox.DefaultStyleKeyProperty.OverrideMetadata( typeof( Zoombox ), new FrameworkPropertyMetadata( typeof( Zoombox ) ) );
      Zoombox.ClipToBoundsProperty.OverrideMetadata( typeof( Zoombox ), new FrameworkPropertyMetadata( true ) );
      Zoombox.FocusableProperty.OverrideMetadata( typeof( Zoombox ), new FrameworkPropertyMetadata( true ) );
      Zoombox.HorizontalContentAlignmentProperty.OverrideMetadata( typeof( Zoombox ), new FrameworkPropertyMetadata( HorizontalAlignment.Center, new PropertyChangedCallback( Zoombox.RefocusView ) ) );
      Zoombox.VerticalContentAlignmentProperty.OverrideMetadata( typeof( Zoombox ), new FrameworkPropertyMetadata( VerticalAlignment.Center, new PropertyChangedCallback( Zoombox.RefocusView ) ) );
      Zoombox.ContentProperty.OverrideMetadata( typeof( Zoombox ), new FrameworkPropertyMetadata( ( PropertyChangedCallback )null, new CoerceValueCallback( Zoombox.CoerceContentValue ) ) );
    }

    public Zoombox()
      : base()
    {
      try
      {
        new UIPermission( PermissionState.Unrestricted ).Demand();
        _cacheBits[ ( int )CacheBits.HasUIPermission ] = true;
      }
      catch( SecurityException )
      {
      }

      this.InitCommands();

      // use the LayoutUpdated event to keep the Viewport in sync
      this.LayoutUpdated += new EventHandler( this.OnLayoutUpdated );
      this.AddHandler( FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler( this.OnSizeChanged ), true );

      this.CoerceValue( Zoombox.ViewStackModeProperty );

      this.Loaded += this.Zoombox_Loaded;
    }

    #endregion

    #region AnimationAccelerationRatio Property

    public static readonly DependencyProperty AnimationAccelerationRatioProperty =
      DependencyProperty.Register( "AnimationAccelerationRatio", typeof( double ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( 0d ), 
          new ValidateValueCallback( Zoombox.ValidateAccelerationRatio ) );

    public double AnimationAccelerationRatio
    {
      get
      {
        return ( double )this.GetValue( Zoombox.AnimationAccelerationRatioProperty );
      }
      set
      {
        this.SetValue( Zoombox.AnimationAccelerationRatioProperty, value );
      }
    }

    private static bool ValidateAccelerationRatio( object value )
    {
      double newValue = ( double )value;
      if( newValue < 0 || newValue > 1 || DoubleHelper.IsNaN( newValue ) )
        throw new ArgumentException( ErrorMessages.GetMessage( "AnimationAccelerationRatioOOR" ) );

      return true;
    }

    #endregion

    #region AnimationDecelerationRatio Property

    public static readonly DependencyProperty AnimationDecelerationRatioProperty =
      DependencyProperty.Register( "AnimationDecelerationRatio", typeof( double ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( 0d ), 
          new ValidateValueCallback( Zoombox.ValidateDecelerationRatio ) );

    public double AnimationDecelerationRatio
    {
      get
      {
        return ( double )this.GetValue( Zoombox.AnimationDecelerationRatioProperty );
      }
      set
      {
        this.SetValue( Zoombox.AnimationDecelerationRatioProperty, value );
      }
    }

    private static bool ValidateDecelerationRatio( object value )
    {
      double newValue = ( double )value;
      if( newValue < 0 || newValue > 1 || DoubleHelper.IsNaN( newValue ) )
        throw new ArgumentException( ErrorMessages.GetMessage( "AnimationDecelerationRatioOOR" ) );

      return true;
    }

    #endregion

    #region AnimationDuration Property

    public static readonly DependencyProperty AnimationDurationProperty =
      DependencyProperty.Register( "AnimationDuration", typeof( Duration ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( new Duration( TimeSpan.FromMilliseconds( 300 ) ) ) );

    public Duration AnimationDuration
    {
      get
      {
        return ( Duration )this.GetValue( Zoombox.AnimationDurationProperty );
      }
      set
      {
        this.SetValue( Zoombox.AnimationDurationProperty, value );
      }
    }

    #endregion

    #region AreDragModifiersActive Property

    private static readonly DependencyPropertyKey AreDragModifiersActivePropertyKey =
      DependencyProperty.RegisterReadOnly( "AreDragModifiersActive", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( false ) );

    public static readonly DependencyProperty AreDragModifiersActiveProperty = Zoombox.AreDragModifiersActivePropertyKey.DependencyProperty;

    public bool AreDragModifiersActive
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.AreDragModifiersActiveProperty );
      }
    }

    private void SetAreDragModifiersActive( bool value )
    {
      this.SetValue( Zoombox.AreDragModifiersActivePropertyKey, value );
    }

    #endregion

    #region AreRelativeZoomModifiersActive Property

    private static readonly DependencyPropertyKey AreRelativeZoomModifiersActivePropertyKey =
      DependencyProperty.RegisterReadOnly( "AreRelativeZoomModifiersActive", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( false ) );

    public static readonly DependencyProperty AreRelativeZoomModifiersActiveProperty = Zoombox.AreRelativeZoomModifiersActivePropertyKey.DependencyProperty;

    public bool AreRelativeZoomModifiersActive
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.AreRelativeZoomModifiersActiveProperty );
      }
    }

    private void SetAreRelativeZoomModifiersActive( bool value )
    {
      this.SetValue( Zoombox.AreRelativeZoomModifiersActivePropertyKey, value );
    }

    #endregion

    #region AreZoomModifiersActive Property

    private static readonly DependencyPropertyKey AreZoomModifiersActivePropertyKey =
      DependencyProperty.RegisterReadOnly( "AreZoomModifiersActive", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( false ) );

    public static readonly DependencyProperty AreZoomModifiersActiveProperty = Zoombox.AreZoomModifiersActivePropertyKey.DependencyProperty;

    public bool AreZoomModifiersActive
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.AreZoomModifiersActiveProperty );
      }
    }

    private void SetAreZoomModifiersActive( bool value )
    {
      this.SetValue( Zoombox.AreZoomModifiersActivePropertyKey, value );
    }

    #endregion

    #region AreZoomToSelectionModifiersActive Property

    private static readonly DependencyPropertyKey AreZoomToSelectionModifiersActivePropertyKey =
      DependencyProperty.RegisterReadOnly( "AreZoomToSelectionModifiersActive", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( false ) );

    public static readonly DependencyProperty AreZoomToSelectionModifiersActiveProperty = Zoombox.AreZoomToSelectionModifiersActivePropertyKey.DependencyProperty;

    public bool AreZoomToSelectionModifiersActive
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.AreZoomToSelectionModifiersActiveProperty );
      }
    }

    private void SetAreZoomToSelectionModifiersActive( bool value )
    {
      this.SetValue( Zoombox.AreZoomToSelectionModifiersActivePropertyKey, value );
    }

    #endregion

    #region AutoWrapContentWithViewbox Property

    public static readonly DependencyProperty AutoWrapContentWithViewboxProperty =
      DependencyProperty.Register( "AutoWrapContentWithViewbox", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( true, 
          new PropertyChangedCallback( Zoombox.OnAutoWrapContentWithViewboxChanged ) ) );

    public bool AutoWrapContentWithViewbox
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.AutoWrapContentWithViewboxProperty );
      }
      set
      {
        this.SetValue( Zoombox.AutoWrapContentWithViewboxProperty, value );
      }
    }

    private static void OnAutoWrapContentWithViewboxChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      o.CoerceValue( Zoombox.ContentProperty );
    }

    private static object CoerceContentValue( DependencyObject d, object value )
    {
      return ( ( Zoombox )d ).CoerceContentValue( value );
    }

    private object CoerceContentValue( object value )
    {
      if( value != null && !( value is UIElement ) && !( ( bool )this.GetValue( DesignerProperties.IsInDesignModeProperty ) ) )
        throw new InvalidContentException( ErrorMessages.GetMessage( "ZoomboxContentMustBeUIElement" ) );

      object oldContent = _content;
      if( value != _trueContent || ( this.IsContentWrapped != this.AutoWrapContentWithViewbox ) )
      {
        // check whether the content is currently wrapped and needs to be unwrapped
        if( this.IsContentWrapped && _content is Viewbox && _content != _trueContent )
        {
          Viewbox viewbox = ( Viewbox )_content;

          BindingOperations.ClearAllBindings( viewbox );
          if( viewbox.Child is FrameworkElement )
          {
            ( viewbox.Child as FrameworkElement ).RemoveHandler( FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler( this.OnContentSizeChanged ) );
          }
          ( viewbox as Viewbox ).Child = null;

          this.RemoveLogicalChild( viewbox );
        }

        // make sure the view finder's visual brush is null
        if( _viewFinderDisplay != null && _viewFinderDisplay.VisualBrush != null )
        {
          _viewFinderDisplay.VisualBrush.Visual = null;
          _viewFinderDisplay.VisualBrush = null;
        }

        // update the cached content and true content values
        _content = value as UIElement;
        _trueContent = value as UIElement;

        // if necessary, unparent the existing content
        if( _contentPresenter != null && _contentPresenter.Content != null )
        {
          _contentPresenter.Content = null;
        }

        // if necessary, wrap the content
        this.IsContentWrapped = false;
        if( this.AutoWrapContentWithViewbox )
        {
          // create a viewbox and make it the logical child of the Zoombox
          Viewbox viewbox = new Viewbox();
          this.AddLogicalChild( viewbox );

          // now set the new parent to be the viewbox
          viewbox.Child = value as UIElement;
          _content = viewbox;
          viewbox.HorizontalAlignment = HorizontalAlignment.Left;
          viewbox.VerticalAlignment = VerticalAlignment.Top;
          this.IsContentWrapped = true;
        }

        if( ( _content is Viewbox ) && ( this.IsContentWrapped ) && ( _trueContent is FrameworkElement ) )
        {
          ( _trueContent as FrameworkElement ).AddHandler( FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler( this.OnContentSizeChanged ), true );
        }

        if( _contentPresenter != null )
        {
          _contentPresenter.Content = _content;
        }

        if( _viewFinderDisplay != null )
        {
          this.CreateVisualBrushForViewFinder( _content );
        }
        this.UpdateViewFinderDisplayContentBounds();
      }

      // if the content changes, we need to reset the flags used to first render and arrange the content
      if( oldContent != _content
          && this.HasArrangedContentPresenter
          && this.HasRenderedFirstView )
      {
        this.HasArrangedContentPresenter = false;
        this.HasRenderedFirstView = false;
        this.RefocusViewOnFirstRender = true;
        _contentPresenter.LayoutUpdated += new EventHandler( this.ContentPresenterFirstArranged );
      }
      return _content;
    }

    private UIElement _trueContent; //null

    #endregion

    #region CurrentView Property

    private static readonly DependencyPropertyKey CurrentViewPropertyKey =
      DependencyProperty.RegisterReadOnly( "CurrentView", typeof( ZoomboxView ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( ZoomboxView.Empty, 
          new PropertyChangedCallback( Zoombox.OnCurrentViewChanged ) ) );

    public static readonly DependencyProperty CurrentViewProperty = Zoombox.CurrentViewPropertyKey.DependencyProperty;

    public ZoomboxView CurrentView
    {
      get
      {
        return ( ZoomboxView )this.GetValue( Zoombox.CurrentViewProperty );
      }
    }

    private void SetCurrentView( ZoomboxView value )
    {
      this.SetValue( Zoombox.CurrentViewPropertyKey, value );
    }

    private static void OnCurrentViewChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      Zoombox zoombox = ( Zoombox )o;
      if( !zoombox.IsUpdatingView )
      {
        zoombox.ZoomTo( zoombox.CurrentView );
      }
      zoombox.RaiseEvent( new ZoomboxViewChangedEventArgs( e.OldValue as ZoomboxView, e.NewValue as ZoomboxView, zoombox._lastViewIndex, zoombox.CurrentViewIndex ) );
    }

    #endregion

    #region CurrentViewIndex Property

    private static readonly DependencyPropertyKey CurrentViewIndexPropertyKey =
      DependencyProperty.RegisterReadOnly( "CurrentViewIndex", typeof( int ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( -1 ) );

    public static readonly DependencyProperty CurrentViewIndexProperty = Zoombox.CurrentViewIndexPropertyKey.DependencyProperty;

    public int CurrentViewIndex
    {
      get
      {
        return ( int )this.GetValue( Zoombox.CurrentViewIndexProperty );
      }
    }

    internal void SetCurrentViewIndex( int value )
    {
      this.SetValue( Zoombox.CurrentViewIndexPropertyKey, value );
    }

    #endregion

    #region DragModifiers Property

    public static readonly DependencyProperty DragModifiersProperty =
      DependencyProperty.Register( "DragModifiers", typeof( KeyModifierCollection ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( Zoombox.GetDefaultDragModifiers() ) );

    [TypeConverter( typeof( KeyModifierCollectionConverter ) )]
    public KeyModifierCollection DragModifiers
    {
      get
      {
        return ( KeyModifierCollection )this.GetValue( Zoombox.DragModifiersProperty );
      }
      set
      {
        this.SetValue( Zoombox.DragModifiersProperty, value );
      }
    }

    private static KeyModifierCollection GetDefaultDragModifiers()
    {
      KeyModifierCollection result = new KeyModifierCollection();
      result.Add( KeyModifier.Ctrl );
      result.Add( KeyModifier.Exact );
      return result;
    }

    #endregion

    #region DragOnPreview Property

    public static readonly DependencyProperty DragOnPreviewProperty =
      DependencyProperty.Register( "DragOnPreview", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( false ) );

    public bool DragOnPreview
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.DragOnPreviewProperty );
      }
      set
      {
        this.SetValue( Zoombox.DragOnPreviewProperty, value );
      }
    }

    #endregion

    #region EffectiveViewStackMode Property

    private static readonly DependencyPropertyKey EffectiveViewStackModePropertyKey =
      DependencyProperty.RegisterReadOnly( "EffectiveViewStackMode", typeof( ZoomboxViewStackMode ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( ZoomboxViewStackMode.Auto ) );

    public static readonly DependencyProperty EffectiveViewStackModeProperty = Zoombox.EffectiveViewStackModePropertyKey.DependencyProperty;

    public ZoomboxViewStackMode EffectiveViewStackMode
    {
      get
      {
        return ( ZoomboxViewStackMode )this.GetValue( Zoombox.EffectiveViewStackModeProperty );
      }
    }

    private void SetEffectiveViewStackMode( ZoomboxViewStackMode value )
    {
      this.SetValue( Zoombox.EffectiveViewStackModePropertyKey, value );
    }

    #endregion

    #region HasBackStack Property

    private static readonly DependencyPropertyKey HasBackStackPropertyKey =
      DependencyProperty.RegisterReadOnly( "HasBackStack", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( false ) );

    public static readonly DependencyProperty HasBackStackProperty = Zoombox.HasBackStackPropertyKey.DependencyProperty;

    public bool HasBackStack
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.HasBackStackProperty );
      }
    }

    #endregion

    #region HasForwardStack Property

    private static readonly DependencyPropertyKey HasForwardStackPropertyKey =
      DependencyProperty.RegisterReadOnly( "HasForwardStack", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( false ) );

    public static readonly DependencyProperty HasForwardStackProperty = Zoombox.HasForwardStackPropertyKey.DependencyProperty;

    public bool HasForwardStack
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.HasForwardStackProperty );
      }
    }

    #endregion

    #region IsAnimated Property

    public static readonly DependencyProperty IsAnimatedProperty =
      DependencyProperty.Register( "IsAnimated", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( true, 
          ( PropertyChangedCallback )null, new CoerceValueCallback( Zoombox.CoerceIsAnimatedValue ) ) );

    public bool IsAnimated
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.IsAnimatedProperty );
      }
      set
      {
        this.SetValue( Zoombox.IsAnimatedProperty, value );
      }
    }

    private static object CoerceIsAnimatedValue( DependencyObject d, object value )
    {
      Zoombox zoombox = ( Zoombox )d;
      bool result = ( bool )value;
      if( !zoombox.IsInitialized )
      {
        result = false;
      }
      return result;
    }

    #endregion

    #region IsDraggingContent Property

    private static readonly DependencyPropertyKey IsDraggingContentPropertyKey =
      DependencyProperty.RegisterReadOnly( "IsDraggingContent", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( false ) );

    public static readonly DependencyProperty IsDraggingContentProperty = Zoombox.IsDraggingContentPropertyKey.DependencyProperty;

    public bool IsDraggingContent
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.IsDraggingContentProperty );
      }
    }

    private void SetIsDraggingContent( bool value )
    {
      this.SetValue( Zoombox.IsDraggingContentPropertyKey, value );
    }

    #endregion

    #region IsSelectingRegion Property

    private static readonly DependencyPropertyKey IsSelectingRegionPropertyKey =
      DependencyProperty.RegisterReadOnly( "IsSelectingRegion", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( false ) );

    public static readonly DependencyProperty IsSelectingRegionProperty = Zoombox.IsSelectingRegionPropertyKey.DependencyProperty;

    public bool IsSelectingRegion
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.IsSelectingRegionProperty );
      }
    }

    private void SetIsSelectingRegion( bool value )
    {
      this.SetValue( Zoombox.IsSelectingRegionPropertyKey, value );
    }

    #endregion

    #region IsUsingScrollBars Property

    public static readonly DependencyProperty IsUsingScrollBarsProperty =
      DependencyProperty.Register( "IsUsingScrollBars", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( false, ( PropertyChangedCallback )null ) );

    public bool IsUsingScrollBars
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.IsUsingScrollBarsProperty );
      }
      set
      {
        this.SetValue( Zoombox.IsUsingScrollBarsProperty, value );
      }
    }

    #endregion

    #region MaxScale Property

    public static readonly DependencyProperty MaxScaleProperty =
      DependencyProperty.Register( "MaxScale", typeof( double ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( 100d, FrameworkPropertyMetadataOptions.AffectsMeasure,
          new PropertyChangedCallback( Zoombox.OnMaxScaleChanged ), new CoerceValueCallback( Zoombox.CoerceMaxScaleValue ) ) );

    public double MaxScale
    {
      get
      {
        return ( double )this.GetValue( Zoombox.MaxScaleProperty );
      }
      set
      {
        this.SetValue( Zoombox.MaxScaleProperty, value );
      }
    }

    private static void OnMaxScaleChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      Zoombox zoombox = ( Zoombox )o;
      zoombox.CoerceValue( Zoombox.MinScaleProperty );
      zoombox.CoerceValue( Zoombox.ScaleProperty );
    }

    private static object CoerceMaxScaleValue( DependencyObject d, object value )
    {
      Zoombox zoombox = ( Zoombox )d;
      double result = ( double )value;
      if( result < zoombox.MinScale )
      {
        result = zoombox.MinScale;
      }
      return result;
    }

    #endregion

    #region MinScale Property

    public static readonly DependencyProperty MinScaleProperty =
      DependencyProperty.Register( "MinScale", typeof( double ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( 0.01d, FrameworkPropertyMetadataOptions.AffectsMeasure,
          new PropertyChangedCallback( Zoombox.OnMinScaleChanged ), new CoerceValueCallback( Zoombox.CoerceMinScaleValue ) ) );

    public double MinScale
    {
      get
      {
        return ( double )this.GetValue( Zoombox.MinScaleProperty );
      }
      set
      {
        this.SetValue( Zoombox.MinScaleProperty, value );
      }
    }

    private static void OnMinScaleChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      Zoombox zoombox = ( Zoombox )o;
      zoombox.CoerceValue( Zoombox.MinScaleProperty );
      zoombox.CoerceValue( Zoombox.ScaleProperty );
    }

    private static object CoerceMinScaleValue( DependencyObject d, object value )
    {
      Zoombox zoombox = ( Zoombox )d;
      double result = ( double )value;
      if( result > zoombox.MaxScale )
      {
        result = zoombox.MaxScale;
      }
      return result;
    }

    #endregion

    #region NavigateOnPreview Property

    public static readonly DependencyProperty NavigateOnPreviewProperty =
      DependencyProperty.Register( "NavigateOnPreview", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( false ) );

    public bool NavigateOnPreview
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.NavigateOnPreviewProperty );
      }
      set
      {
        this.SetValue( Zoombox.NavigateOnPreviewProperty, value );
      }
    }

    #endregion

    #region PanDistance Property

    public static readonly DependencyProperty PanDistanceProperty =
      DependencyProperty.Register( "PanDistance", typeof( double ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( 5d ) );

    public double PanDistance
    {
      get
      {
        return ( double )this.GetValue( Zoombox.PanDistanceProperty );
      }
      set
      {
        this.SetValue( Zoombox.PanDistanceProperty, value );
      }
    }

    #endregion

    #region Position Property

    public static readonly DependencyProperty PositionProperty =
      DependencyProperty.Register( "Position", typeof( Point ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( PointHelper.Empty, new PropertyChangedCallback( Zoombox.OnPositionChanged ) ) );

    public Point Position
    {
      get
      {
        return ( Point )this.GetValue( Zoombox.PositionProperty );
      }
      set
      {
        this.SetValue( Zoombox.PositionProperty, value );
      }
    }

    private static void OnPositionChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      Zoombox zoombox = ( Zoombox )o;
      if( !zoombox.IsUpdatingViewport )
      {
        Point newPosition = ( Point )e.NewValue;
        double scale = zoombox.Scale;
        if( scale > 0 )
        {
          zoombox.ZoomTo( new Point( -newPosition.X, -newPosition.Y ) );
        }
      }
    }

    #endregion

    #region RelativeZoomModifiers Property

    public static readonly DependencyProperty RelativeZoomModifiersProperty =
      DependencyProperty.Register( "RelativeZoomModifiers", typeof( KeyModifierCollection ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( Zoombox.GetDefaultRelativeZoomModifiers() ) );

    [TypeConverter( typeof( KeyModifierCollectionConverter ) )]
    public KeyModifierCollection RelativeZoomModifiers
    {
      get
      {
        return ( KeyModifierCollection )this.GetValue( Zoombox.RelativeZoomModifiersProperty );
      }
      set
      {
        this.SetValue( Zoombox.RelativeZoomModifiersProperty, value );
      }
    }

    private static KeyModifierCollection GetDefaultRelativeZoomModifiers()
    {
      KeyModifierCollection result = new KeyModifierCollection();
      result.Add( KeyModifier.Ctrl );
      result.Add( KeyModifier.Alt );
      result.Add( KeyModifier.Exact );
      return result;
    }

    #endregion

    #region Scale Property

    public static readonly DependencyProperty ScaleProperty =
      DependencyProperty.Register( "Scale", typeof( double ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( double.NaN, 
          new PropertyChangedCallback( Zoombox.OnScaleChanged ), new CoerceValueCallback( Zoombox.CoerceScaleValue ) ) );

    public double Scale
    {
      get
      {
        return ( double )this.GetValue( Zoombox.ScaleProperty );
      }
      set
      {
        this.SetValue( Zoombox.ScaleProperty, value );
      }
    }

    private static void OnScaleChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      Zoombox zoombox = ( Zoombox )o;
      if( !zoombox.IsUpdatingView )
      {
        double newScale = ( double )e.NewValue;
        zoombox.ZoomTo( newScale );
      }
    }

    private static object CoerceScaleValue( DependencyObject d, object value )
    {
      Zoombox zoombox = ( Zoombox )d;
      double result = ( double )value;

      if( result < zoombox.MinScale )
      {
        result = zoombox.MinScale;
      }

      if( result > zoombox.MaxScale )
      {
        result = zoombox.MaxScale;
      }

      return result;
    }

    #endregion

    #region ViewFinder Property

    private static readonly DependencyPropertyKey ViewFinderPropertyKey =
      DependencyProperty.RegisterReadOnly( "ViewFinder", typeof( FrameworkElement ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( null, new PropertyChangedCallback( Zoombox.OnViewFinderChanged ) ) );

    public static readonly DependencyProperty ViewFinderProperty = Zoombox.ViewFinderPropertyKey.DependencyProperty;

    public FrameworkElement ViewFinder
    {
      get
      {
        return ( FrameworkElement )this.GetValue( Zoombox.ViewFinderProperty );
      }
      set
      {
        this.SetValue( Zoombox.ViewFinderPropertyKey, value );
      }
    }

    private static void OnViewFinderChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( Zoombox )d ).OnViewFinderChanged( e );
    }

    private void OnViewFinderChanged( DependencyPropertyChangedEventArgs e )
    {
      this.AttachToVisualTree();
    }

    #endregion

    #region ViewFinderVisibility Attached Property

    public static readonly DependencyProperty ViewFinderVisibilityProperty =
      DependencyProperty.RegisterAttached( "ViewFinderVisibility", typeof( Visibility ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( Visibility.Visible ) );

    public static Visibility GetViewFinderVisibility( DependencyObject d )
    {
      return ( Visibility )( d.GetValue( Zoombox.ViewFinderVisibilityProperty ) );
    }

    public static void SetViewFinderVisibility( DependencyObject d, Visibility value )
    {
      d.SetValue( Zoombox.ViewFinderVisibilityProperty, value );
    }

    #endregion

    #region Viewport Property

    private static readonly DependencyPropertyKey ViewportPropertyKey =
      DependencyProperty.RegisterReadOnly( "Viewport", typeof( Rect ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( Rect.Empty, 
          new PropertyChangedCallback( Zoombox.OnViewportChanged ) ) );

    public static readonly DependencyProperty ViewportProperty = Zoombox.ViewportPropertyKey.DependencyProperty;

    public Rect Viewport
    {
      get
      {
        return ( Rect )this.GetValue( Zoombox.ViewportProperty );
      }
    }

    private static void OnViewportChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      // keep the Position property in sync with the Viewport
      Zoombox zoombox = ( Zoombox )o;
      zoombox.Position = new Point( -zoombox.Viewport.Left * zoombox.Scale / zoombox._viewboxFactor, -zoombox.Viewport.Top * zoombox.Scale / zoombox._viewboxFactor );
    }

    #endregion

    #region ViewStackCount Property

    private static readonly DependencyPropertyKey ViewStackCountPropertyKey =
      DependencyProperty.RegisterReadOnly( "ViewStackCount", typeof( int ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( -1,
          new PropertyChangedCallback( Zoombox.OnViewStackCountChanged ) ) );

    public static readonly DependencyProperty ViewStackCountProperty = Zoombox.ViewStackCountPropertyKey.DependencyProperty;

    public int ViewStackCount
    {
      get
      {
        return ( int )this.GetValue( Zoombox.ViewStackCountProperty );
      }
    }

    internal void SetViewStackCount( int value )
    {
      this.SetValue( Zoombox.ViewStackCountPropertyKey, value );
    }

    private static void OnViewStackCountChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( Zoombox )d ).OnViewStackCountChanged( e );
    }

    private void OnViewStackCountChanged( DependencyPropertyChangedEventArgs e )
    {
      if( this.EffectiveViewStackMode == ZoomboxViewStackMode.Disabled )
        return;

      this.UpdateStackProperties();
    }

    #endregion

    #region ViewStackIndex Property

    public static readonly DependencyProperty ViewStackIndexProperty =
      DependencyProperty.Register( "ViewStackIndex", typeof( int ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( -1,
          new PropertyChangedCallback( Zoombox.OnViewStackIndexChanged ), new CoerceValueCallback( Zoombox.CoerceViewStackIndexValue ) ) );

    public int ViewStackIndex
    {
      get
      {
        return ( int )this.GetValue( Zoombox.ViewStackIndexProperty );
      }
      set
      {
        this.SetValue( Zoombox.ViewStackIndexProperty, value );
      }
    }

    private static void OnViewStackIndexChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( Zoombox )d ).OnViewStackIndexChanged( e );
    }

    private void OnViewStackIndexChanged( DependencyPropertyChangedEventArgs e )
    {
      if( this.EffectiveViewStackMode == ZoomboxViewStackMode.Disabled )
        return;

      if( !this.IsUpdatingView )
      {
        int viewIndex = this.ViewStackIndex;
        if( viewIndex >= 0 && viewIndex < ViewStack.Count )
        {
          // update the current view, but don't allow the new view 
          // to be added to the view stack
          this.UpdateView( this.ViewStack[ viewIndex ], true, false, viewIndex );
        }
      }

      this.UpdateStackProperties();
      this.RaiseEvent( new IndexChangedEventArgs( Zoombox.ViewStackIndexChangedEvent, ( int )e.OldValue, ( int )e.NewValue ) );
    }

    private static object CoerceViewStackIndexValue( DependencyObject d, object value )
    {
      Zoombox zoombox = d as Zoombox;
      return ( zoombox.EffectiveViewStackMode == ZoomboxViewStackMode.Disabled ) ? -1 : value;
    }

    #endregion

    #region ViewStackMode Property

    public static readonly DependencyProperty ViewStackModeProperty = 
      DependencyProperty.Register( "ViewStackMode", typeof( ZoomboxViewStackMode ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( ZoomboxViewStackMode.Default,
          new PropertyChangedCallback( Zoombox.OnViewStackModeChanged ), new CoerceValueCallback( Zoombox.CoerceViewStackModeValue ) ) );

    public ZoomboxViewStackMode ViewStackMode
    {
      get
      {
        return ( ZoomboxViewStackMode )this.GetValue( Zoombox.ViewStackModeProperty );
      }
      set
      {
        this.SetValue( Zoombox.ViewStackModeProperty, value );
      }
    }

    private static void OnViewStackModeChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( Zoombox )d ).OnViewStackModeChanged( e );
    }

    private void OnViewStackModeChanged( DependencyPropertyChangedEventArgs e )
    {
      if( ( ZoomboxViewStackMode )e.NewValue == ZoomboxViewStackMode.Disabled && _viewStack != null )
      {
        _viewStack.ClearViewStackSource();
        _viewStack = null;
      }
    }

    private static object CoerceViewStackModeValue( DependencyObject d, object value )
    {
      Zoombox zoombox = d as Zoombox;
      ZoomboxViewStackMode effectiveMode = ( ZoomboxViewStackMode )value;

      // if the effective mode is currently disabled, it must be updated first
      if( zoombox.EffectiveViewStackMode == ZoomboxViewStackMode.Disabled )
      {
        zoombox.SetEffectiveViewStackMode( effectiveMode );
      }

      // now determine the correct effective mode
      if( effectiveMode != ZoomboxViewStackMode.Disabled )
      {
        if( effectiveMode == ZoomboxViewStackMode.Default )
        {
          effectiveMode = ( zoombox.ViewStack.AreViewsFromSource ? ZoomboxViewStackMode.Manual : ZoomboxViewStackMode.Auto );
        }
        if( zoombox.ViewStack.AreViewsFromSource && ( ZoomboxViewStackMode )effectiveMode != ZoomboxViewStackMode.Manual )
        {
          throw new InvalidOperationException( ErrorMessages.GetMessage( "ViewModeInvalidForSource" ) );
        }
      }

      // update the effective mode
      zoombox.SetEffectiveViewStackMode( effectiveMode );
      return value;
    }

    #endregion

    #region ViewStackSource Property

    public static readonly DependencyProperty ViewStackSourceProperty =
      DependencyProperty.Register( "ViewStackSource", typeof( IEnumerable ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( ( IEnumerable )null,
          new PropertyChangedCallback( Zoombox.OnViewStackSourceChanged ) ) );

    [Bindable( true )]
    public IEnumerable ViewStackSource
    {
      get
      {
        return ( _viewStack == null ) ? null : ViewStack.Source;
      }
      set
      {
        if( value == null )
        {
          this.ClearValue( Zoombox.ViewStackSourceProperty );
        }
        else
        {
          this.SetValue( Zoombox.ViewStackSourceProperty, value );
        }
      }
    }

    private static void OnViewStackSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      Zoombox zoombox = ( Zoombox )d;
      IEnumerable oldValue = ( IEnumerable )e.OldValue;
      IEnumerable newValue = ( IEnumerable )e.NewValue;

      // We need to know whether the new value represents an explicit null value 
      // or whether it came from a binding. The latter indicates that we stay in ViewStackSource mode,
      // but with a null collection.
      if( e.NewValue == null && !BindingOperations.IsDataBound( d, Zoombox.ViewStackSourceProperty ) )
      {
        if( zoombox.ViewStack != null )
        {
          zoombox.ViewStack.ClearViewStackSource();
        }
      }
      else
      {
        zoombox.ViewStack.SetViewStackSource( newValue );
      }

      zoombox.CoerceValue( Zoombox.ViewStackModeProperty );
    }

    #endregion

    #region ZoomModifiers Property

    public static readonly DependencyProperty ZoomModifiersProperty =
      DependencyProperty.Register( "ZoomModifiers", typeof( KeyModifierCollection ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( Zoombox.GetDefaultZoomModifiers() ) );

    [TypeConverter( typeof( KeyModifierCollectionConverter ) )]
    public KeyModifierCollection ZoomModifiers
    {
      get
      {
        return ( KeyModifierCollection )this.GetValue( Zoombox.ZoomModifiersProperty );
      }
      set
      {
        this.SetValue( Zoombox.ZoomModifiersProperty, value );
      }
    }

    private static KeyModifierCollection GetDefaultZoomModifiers()
    {
      KeyModifierCollection result = new KeyModifierCollection();
      result.Add( KeyModifier.Shift );
      result.Add( KeyModifier.Exact );
      return result;
    }

    #endregion

    #region ZoomOnPreview Property

    public static readonly DependencyProperty ZoomOnPreviewProperty =
      DependencyProperty.Register( "ZoomOnPreview", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( true ) );

    public bool ZoomOnPreview
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.ZoomOnPreviewProperty );
      }
      set
      {
        this.SetValue( Zoombox.ZoomOnPreviewProperty, value );
      }
    }

    #endregion

    #region ZoomOrigin Property

    public static readonly DependencyProperty ZoomOriginProperty =
      DependencyProperty.Register( "ZoomOrigin", typeof( Point ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( new Point( 0.5d, 0.5d ) ) );

    public Point ZoomOrigin
    {
      get
      {
        return ( Point )this.GetValue( Zoombox.ZoomOriginProperty );
      }
      set
      {
        this.SetValue( Zoombox.ZoomOriginProperty, value );
      }
    }

    #endregion

    #region ZoomPercentage Property

    public static readonly DependencyProperty ZoomPercentageProperty =
      DependencyProperty.Register( "ZoomPercentage", typeof( double ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( 5d ) );

    public double ZoomPercentage
    {
      get
      {
        return ( double )this.GetValue( Zoombox.ZoomPercentageProperty );
      }
      set
      {
        this.SetValue( Zoombox.ZoomPercentageProperty, value );
      }
    }

    #endregion

    #region ZoomOn Property

    public static readonly DependencyProperty ZoomOnProperty =
      DependencyProperty.Register( "ZoomOn", typeof( ZoomboxZoomOn ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( ZoomboxZoomOn.Content ) );

    public ZoomboxZoomOn ZoomOn
    {
      get
      {
        return ( ZoomboxZoomOn )this.GetValue( Zoombox.ZoomOnProperty );
      }
      set
      {
        this.SetValue( Zoombox.ZoomOnProperty, value );
      }
    }

    #endregion

    #region ZoomToSelectionModifiers Property

    public static readonly DependencyProperty ZoomToSelectionModifiersProperty =
      DependencyProperty.Register( "ZoomToSelectionModifiers", typeof( KeyModifierCollection ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( Zoombox.GetDefaultZoomToSelectionModifiers() ) );

    [TypeConverter( typeof( KeyModifierCollectionConverter ) )]
    public KeyModifierCollection ZoomToSelectionModifiers
    {
      get
      {
        return ( KeyModifierCollection )this.GetValue( Zoombox.ZoomToSelectionModifiersProperty );
      }
      set
      {
        this.SetValue( Zoombox.ZoomToSelectionModifiersProperty, value );
      }
    }

    private static KeyModifierCollection GetDefaultZoomToSelectionModifiers()
    {
      KeyModifierCollection result = new KeyModifierCollection();
      result.Add( KeyModifier.Alt );
      result.Add( KeyModifier.Exact );
      return result;
    }

    #endregion

    #region KeepContentInBounds Property

    public static readonly DependencyProperty KeepContentInBoundsProperty =
      DependencyProperty.Register( "KeepContentInBounds", typeof( bool ), typeof( Zoombox ),
        new FrameworkPropertyMetadata( false, 
          new PropertyChangedCallback( Zoombox.OnKeepContentInBoundsChanged ) ) );

    public bool KeepContentInBounds
    {
      get
      {
        return ( bool )this.GetValue( Zoombox.KeepContentInBoundsProperty );
      }
      set
      {
        this.SetValue( Zoombox.KeepContentInBoundsProperty, value );
      }
    }

    private static void OnKeepContentInBoundsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( Zoombox )d ).OnKeepContentInBoundsChanged( e );
    }

    private void OnKeepContentInBoundsChanged( DependencyPropertyChangedEventArgs e )
    {
      //
      // Update view and see if we need to reposition the content
      //
      bool oldIsAnimated = this.IsAnimated;
      this.IsAnimated = false;
      try
      {
        this.UpdateView( this.CurrentView, false, false, this.ViewStackIndex );
      }
      finally
      {
        this.IsAnimated = oldIsAnimated;
      }
    }

    #endregion

    #region ViewStack Property

    public ZoomboxViewStack ViewStack
    {
      get
      {
        if( _viewStack == null && this.EffectiveViewStackMode != ZoomboxViewStackMode.Disabled )
        {
          _viewStack = new ZoomboxViewStack( this );
        }
        return _viewStack;
      }
    }

    #endregion

    #region HasArrangedContentPresenter Internal Property

    internal bool HasArrangedContentPresenter
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.HasArrangedContentPresenter ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.HasArrangedContentPresenter ] = value;
      }
    }

    #endregion

    #region IsUpdatingView Internal Property

    internal bool IsUpdatingView
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsUpdatingView ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.IsUpdatingView ] = value;
      }
    }

    #endregion

    #region ContentOffset Private Property

    private Vector ContentOffset
    {
      get
      {
        // auto-wrapped content is always left and top aligned
        if( this.IsContentWrapped || _content == null || !( _content is FrameworkElement ) )
          return new Vector( 0, 0 );

        double x = 0;
        double y = 0;
        Size contentSize = this.ContentRect.Size;

        switch( ( _content as FrameworkElement ).HorizontalAlignment )
        {
          case HorizontalAlignment.Center:
          case HorizontalAlignment.Stretch:
            x = ( this.RenderSize.Width - contentSize.Width ) / 2;
            break;

          case HorizontalAlignment.Right:
            x = this.RenderSize.Width - contentSize.Width;
            break;
        }

        switch( ( _content as FrameworkElement ).VerticalAlignment )
        {
          case VerticalAlignment.Center:
          case VerticalAlignment.Stretch:
            y = ( this.RenderSize.Height - contentSize.Height ) / 2;
            break;

          case VerticalAlignment.Bottom:
            y = this.RenderSize.Height - contentSize.Height;
            break;
        }

        return new Vector( x, y );
      }
    }

    #endregion

    #region ContentRect Private Property

    private Rect ContentRect
    {
      get
      {
        return ( _content == null ) ? Rect.Empty
            : new Rect( new Size( _content.RenderSize.Width / _viewboxFactor, _content.RenderSize.Height / _viewboxFactor ) );
      }
    }

    #endregion

    #region HasRenderedFirstView Private Property

    private bool HasRenderedFirstView
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.HasRenderedFirstView ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.HasRenderedFirstView ] = value;
      }
    }

    #endregion

    #region HasUIPermission Private Property

    private bool HasUIPermission
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.HasUIPermission ];
      }
    }

    #endregion

    #region IsContentWrapped Private Property

    private bool IsContentWrapped
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsContentWrapped ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.IsContentWrapped ] = value;
      }
    }

    #endregion

    #region IsDraggingViewport Private Property

    private bool IsDraggingViewport
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsDraggingViewport ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.IsDraggingViewport ] = value;
      }
    }

    #endregion

    #region IsMonitoringInput Private Property

    private bool IsMonitoringInput
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsMonitoringInput ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.IsMonitoringInput ] = value;
      }
    }

    #endregion

    #region IsResizingViewport Private Property

    private bool IsResizingViewport
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsResizingViewport ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.IsResizingViewport ] = value;
      }
    }

    #endregion

    #region IsUpdatingViewport Private Property

    private bool IsUpdatingViewport
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsUpdatingViewport ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.IsUpdatingViewport ] = value;
      }
    }

    #endregion

    #region RefocusViewOnFirstRender Private Property

    private bool RefocusViewOnFirstRender
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.RefocusViewOnFirstRender ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.RefocusViewOnFirstRender ] = value;
      }
    }

    #endregion

    #region ViewFinderDisplayRect Private Property

    private Rect ViewFinderDisplayRect
    {
      get
      {
        return ( _viewFinderDisplay == null ) ? Rect.Empty
            : new Rect( new Point( 0, 0 ), new Point( _viewFinderDisplay.RenderSize.Width, _viewFinderDisplay.RenderSize.Height ) );
      }
    }

    #endregion

    #region AnimationBeginning Event

    public static readonly RoutedEvent AnimationBeginningEvent = EventManager.RegisterRoutedEvent( "AnimationBeginning", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Zoombox ) );

    public event RoutedEventHandler AnimationBeginning
    {
      add
      {
        this.AddHandler( Zoombox.AnimationBeginningEvent, value );
      }
      remove
      {
        this.RemoveHandler( Zoombox.AnimationBeginningEvent, value );
      }
    }

    #endregion

    #region AnimationCompleted Event

    public static readonly RoutedEvent AnimationCompletedEvent = EventManager.RegisterRoutedEvent( "AnimationCompleted", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Zoombox ) );

    public event RoutedEventHandler AnimationCompleted
    {
      add
      {
        this.AddHandler( Zoombox.AnimationCompletedEvent, value );
      }
      remove
      {
        this.RemoveHandler( Zoombox.AnimationCompletedEvent, value );
      }
    }

    #endregion

    #region CurrentViewChanged Event

    public static readonly RoutedEvent CurrentViewChangedEvent = EventManager.RegisterRoutedEvent( "CurrentViewChanged", RoutingStrategy.Bubble, typeof( ZoomboxViewChangedEventHandler ), typeof( Zoombox ) );

    public event ZoomboxViewChangedEventHandler CurrentViewChanged
    {
      add
      {
        this.AddHandler( Zoombox.CurrentViewChangedEvent, value );
      }
      remove
      {
        this.RemoveHandler( Zoombox.CurrentViewChangedEvent, value );
      }
    }

    #endregion

    public event EventHandler<ScrollEventArgs> Scroll;

    #region ViewStackIndexChanged Event

    public static readonly RoutedEvent ViewStackIndexChangedEvent = EventManager.RegisterRoutedEvent( "ViewStackIndexChanged", RoutingStrategy.Bubble, typeof( IndexChangedEventHandler ), typeof( Zoombox ) );

    public event IndexChangedEventHandler ViewStackIndexChanged
    {
      add
      {
        this.AddHandler( Zoombox.ViewStackIndexChangedEvent, value );
      }
      remove
      {
        this.RemoveHandler( Zoombox.ViewStackIndexChangedEvent, value );
      }
    }

    #endregion

    #region Back Command

    public static RoutedUICommand Back = new RoutedUICommand( "Go Back", "GoBack", typeof( Zoombox ) );

    private void CanGoBack( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = ( this.EffectiveViewStackMode != ZoomboxViewStackMode.Disabled )
                  && ( this.ViewStackIndex > 0 );
    }

    private void GoBack( object sender, ExecutedRoutedEventArgs e )
    {
      this.GoBack();
    }

    #endregion

    #region Center Command

    public static RoutedUICommand Center = new RoutedUICommand( "Center Content", "Center", typeof( Zoombox ) );

    private void CenterContent( object sender, ExecutedRoutedEventArgs e )
    {
      this.CenterContent();
    }

    #endregion

    #region Fill Command

    public static RoutedUICommand Fill = new RoutedUICommand( "Fill Bounds with Content", "FillToBounds", typeof( Zoombox ) );

    private void FillToBounds( object sender, ExecutedRoutedEventArgs e )
    {
      this.FillToBounds();
    }

    #endregion

    #region Fit Command

    public static RoutedUICommand Fit = new RoutedUICommand( "Fit Content within Bounds", "FitToBounds", typeof( Zoombox ) );

    private void FitToBounds( object sender, ExecutedRoutedEventArgs e )
    {
      this.FitToBounds();
    }

    #endregion

    #region Forward Command

    public static RoutedUICommand Forward = new RoutedUICommand( "Go Forward", "GoForward", typeof( Zoombox ) );

    private void CanGoForward( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = ( this.EffectiveViewStackMode != ZoomboxViewStackMode.Disabled )
                  && ( this.ViewStackIndex < this.ViewStack.Count - 1 );
    }

    private void GoForward( object sender, ExecutedRoutedEventArgs e )
    {
      this.GoForward();
    }

    #endregion

    #region Home Command

    public static RoutedUICommand Home = new RoutedUICommand( "Go Home", "GoHome", typeof( Zoombox ) );

    private void CanGoHome( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = ( this.EffectiveViewStackMode != ZoomboxViewStackMode.Disabled )
                  && ( this.ViewStack.Count > 0 )
                  && ( this.ViewStackIndex != 0 );
    }

    private void GoHome( object sender, ExecutedRoutedEventArgs e )
    {
      this.GoHome();
    }

    #endregion

    #region PanDown Command

    public static RoutedUICommand PanDown = new RoutedUICommand( "Pan Down", "PanDown", typeof( Zoombox ) );

    private void PanDownExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      this.Position = new Point( _basePosition.X, _basePosition.Y + PanDistance );
    }

    #endregion

    #region PanLeft Command

    public static RoutedUICommand PanLeft = new RoutedUICommand( "Pan Left", "PanLeft", typeof( Zoombox ) );

    private void PanLeftExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      this.Position = new Point( _basePosition.X - this.PanDistance, _basePosition.Y );
    }

    #endregion

    #region PanRight Command

    public static RoutedUICommand PanRight = new RoutedUICommand( "Pan Right", "PanRight", typeof( Zoombox ) );

    private void PanRightExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      this.Position = new Point( _basePosition.X + this.PanDistance, _basePosition.Y );
    }

    #endregion

    #region PanUp Command

    public static RoutedUICommand PanUp = new RoutedUICommand( "Pan Up", "PanUp", typeof( Zoombox ) );

    private void PanUpExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      this.Position = new Point( _basePosition.X, _basePosition.Y - this.PanDistance );
    }

    #endregion

    #region Refocus Command

    public static RoutedUICommand Refocus = new RoutedUICommand( "Refocus View", "Refocus", typeof( Zoombox ) );

    private void CanRefocusView( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = ( this.EffectiveViewStackMode == ZoomboxViewStackMode.Manual )
                  && ( this.ViewStackIndex >= 0 && this.ViewStackIndex < this.ViewStack.Count )
                  && ( this.CurrentView != this.ViewStack[ this.ViewStackIndex ] );
    }

    private void RefocusView( object sender, ExecutedRoutedEventArgs e )
    {
      this.RefocusView();
    }

    #endregion

    #region ZoomIn Command

    public static RoutedUICommand ZoomIn = new RoutedUICommand( "Zoom In", "ZoomIn", typeof( Zoombox ) );

    private void ZoomInExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      this.Zoom( this.ZoomPercentage / 100 );
    }

    #endregion

    #region ZoomOut Command

    public static RoutedUICommand ZoomOut = new RoutedUICommand( "Zoom Out", "ZoomOut", typeof( Zoombox ) );

    private void ZoomOutExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      this.Zoom( -this.ZoomPercentage / 100 );
    }

    #endregion

    public void CenterContent()
    {
      if( _content != null )
      {
        this.SetScrollBars();
        this.ZoomTo( ZoomboxView.Center );
      }
    }

    public void FillToBounds()
    {
      if( _content != null )
      {
        this.SetScrollBars();
        this.ZoomTo( ZoomboxView.Fill );
      }
    }

    public void FitToBounds()
    {
      if( _content != null )
      {
        this.SetScrollBars();
        this.ZoomTo( ZoomboxView.Fit );
      }
    }

    public void GoBack()
    {
      if( this.EffectiveViewStackMode == ZoomboxViewStackMode.Disabled )
        return;

      if( this.ViewStackIndex > 0 )
      {
        this.ViewStackIndex--;
      }
    }

    public void GoForward()
    {
      if( this.EffectiveViewStackMode == ZoomboxViewStackMode.Disabled )
        return;

      if( this.ViewStackIndex < this.ViewStack.Count - 1 )
      {
        this.ViewStackIndex++;
      }
    }

    public void GoHome()
    {
      if( this.EffectiveViewStackMode == ZoomboxViewStackMode.Disabled )
        return;

      if( this.ViewStackIndex > 0 )
      {
        this.ViewStackIndex = 0;
      }
    }

    public override void OnApplyTemplate()
    {
      this.AttachToVisualTree();
      base.OnApplyTemplate();
    }

    public void RefocusView()
    {
      if( this.EffectiveViewStackMode == ZoomboxViewStackMode.Disabled )
        return;

      if( this.ViewStackIndex >= 0 && this.ViewStackIndex < this.ViewStack.Count
          && this.CurrentView != this.ViewStack[ this.ViewStackIndex ] )
      {
        this.UpdateView( this.ViewStack[ this.ViewStackIndex ], true, false, this.ViewStackIndex );
      }
    }

    public void Zoom( double percentage )
    {
      // if there is nothing to scale, just return
      if( _content == null )
        return;

      this.Zoom( percentage, this.GetZoomRelativePoint() );
    }

    public void Zoom( double percentage, Point relativeTo )
    {
      // if there is nothing to scale, just return
      if( _content == null )
        return;

      // adjust the current scale relative to the given point
      double scale = this.Scale * ( 1 + percentage );
      this.ZoomTo( scale, relativeTo );
    }

    public void ZoomTo( Point position )
    {
      // if there is nothing to pan, just return
      if( _content == null )
        return;

      // zoom to the new region
      this.ZoomTo( new ZoomboxView( new Point( -position.X, -position.Y ) ) );
    }

    public void ZoomTo( Rect region )
    {
      if( _content == null )
        return;

      // adjust the current scale and position
      this.UpdateView( new ZoomboxView( region ), true, true );
    }

    public void ZoomTo( double scale )
    {
      // if there is nothing to scale, just return
      if( _content == null )
        return;

      // adjust the current scale relative to the center of the content within the control
      this.ZoomTo( scale, true );
    }

    public void ZoomTo( double scale, Point relativeTo )
    {
      this.ZoomTo( scale, relativeTo, true, true );
    }

    public void ZoomTo( ZoomboxView view )
    {
      this.UpdateView( view, true, true );
    }

    internal void UpdateStackProperties()
    {
      this.SetValue( Zoombox.HasBackStackPropertyKey, this.ViewStackIndex > 0 );
      this.SetValue( Zoombox.HasForwardStackPropertyKey, this.ViewStack.Count > this.ViewStackIndex + 1 );
      CommandManager.InvalidateRequerySuggested();
    }

    protected override Size MeasureOverride( Size constraint )
    {
      if( _content != null )
      {
        // measure visuals according to supplied constraint
        Size size = base.MeasureOverride( constraint );

        // now re-measure content to let the child be whatever size it desires
        _content.Measure( new Size( double.PositiveInfinity, double.PositiveInfinity ) );
        return size;
      }


      // avoid returning infinity
      if( double.IsInfinity( constraint.Height ) )
      {
        constraint.Height = 0;
      }

      if( double.IsInfinity( constraint.Width ) )
      {
        constraint.Width = 0;
      }
      return constraint;
    }

    protected override void OnContentChanged( object oldContent, object newContent )
    {
      // disconnect SizeChanged handler from old content
      if( oldContent is FrameworkElement )
      {
        ( oldContent as FrameworkElement ).RemoveHandler( FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler( this.OnContentSizeChanged ) );
      }
      else
      {
        this.RemoveHandler( FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler( this.OnContentSizeChanged ) );
      }

      // connect SizeChanged handler to new content
      if( _content is FrameworkElement )
      {
        ( _content as FrameworkElement ).AddHandler( FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler( this.OnContentSizeChanged ), true );
      }
      else
      {
        this.AddHandler( FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler( this.OnContentSizeChanged ), true );
      }

      // update the Visual property of the view finder display panel's VisualBrush
      if( _viewFinderDisplay != null && _viewFinderDisplay.VisualBrush != null )
      {
        _viewFinderDisplay.VisualBrush.Visual = _content;
      }
    }

    protected override void OnGotKeyboardFocus( KeyboardFocusChangedEventArgs e )
    {
      this.MonitorInput();
      base.OnGotKeyboardFocus( e );
    }

    protected override void OnLostKeyboardFocus( KeyboardFocusChangedEventArgs e )
    {
      this.MonitorInput();
      base.OnLostKeyboardFocus( e );
    }

    protected override void OnInitialized( EventArgs e )
    {
      base.OnInitialized( e );
      this.CoerceValue( Zoombox.IsAnimatedProperty );
    }

    protected override void OnRender( DrawingContext drawingContext )
    {
      if( this.HasArrangedContentPresenter && !this.HasRenderedFirstView )
      {
        this.HasRenderedFirstView = true;
        if( this.RefocusViewOnFirstRender )
        {
          this.RefocusViewOnFirstRender = false;
          bool oldAnimated = this.IsAnimated;
          this.IsAnimated = false;
          try
          {
            this.RefocusView();
          }
          finally
          {
            this.IsAnimated = oldAnimated;
          }
        }
      }

      base.OnRender( drawingContext );
    }




    private static void RefocusView( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      Zoombox zoombox = o as Zoombox;
      zoombox.UpdateView( zoombox.CurrentView, true, false, zoombox.ViewStackIndex );
    }

    private void AttachToVisualTree()
    {
      if( _isUpdatingVisualTree )
        return;

      _isUpdatingVisualTree = true;

      // detach from the old tree
      this.DetachFromVisualTree();

      // create the drag adorner for selection operations
      _dragAdorner = new DragAdorner( this );

      // check the template for a SelectionBrush resource
      if( this.Template.Resources.Contains( "SelectionBrush" ) )
        _dragAdorner.Brush = this.Template.Resources[ "SelectionBrush" ] as Brush;

      // check the template for a SelectionPen resource
      if( this.Template.Resources.Contains( "SelectionPen" ) )
        _dragAdorner.Pen = this.Template.Resources[ "SelectionPen" ] as Pen;

      // check the template for key bindings
      if( this.Template.Resources.Contains( "InputBindings" ) )
      {
        InputBindingCollection inputBindings = this.Template.Resources[ "InputBindings" ] as InputBindingCollection;
        if( inputBindings != null )
        {
          this.InputBindings.AddRange( inputBindings );
        }
      }

      // locate the content presenter
      _contentPresenter = VisualTreeHelperEx.FindDescendantByType( this, typeof( ContentPresenter ) ) as ContentPresenter;
      if( _contentPresenter == null )
        throw new InvalidTemplateException( ErrorMessages.GetMessage( "ZoomboxTemplateNeedsContent" ) );

      _verticalScrollBar = this.GetTemplateChild( PART_VerticalScrollBar ) as ScrollBar;
      if( _verticalScrollBar == null )
        throw new InvalidTemplateException( ErrorMessages.GetMessage( "Zoombox vertical scrollBar not found." ) );

      _verticalScrollBar.Scroll += this.VerticalScrollBar_Scroll;

      _horizontalScrollBar = this.GetTemplateChild( PART_HorizontalScrollBar ) as ScrollBar;
      if( _horizontalScrollBar == null )
        throw new InvalidTemplateException( ErrorMessages.GetMessage( "Zoombox horizontal scrollBar not found." ) );

      _horizontalScrollBar.Scroll += this.HorizontalScrollBar_Scroll;

      // check the template for an AdornerDecorator
      AdornerLayer al = null;
      AdornerDecorator ad = VisualTreeHelperEx.FindDescendantByType( this, typeof( AdornerDecorator ) ) as AdornerDecorator;
      if( ad != null )
      {
        al = ad.AdornerLayer;
      }
      else
      {
        // look for an inherited adorner layer
        try
        {
          al = AdornerLayer.GetAdornerLayer( this );
        }
        catch( Exception )
        {
        }
      }

      // add the drag adorner to the adorner layer
      if( al != null )
      {
        al.Add( _dragAdorner );
      }

      // TODO: Why is it necessary to walk the visual tree the first time through?
      // If we don't do the following, the content is not laid out correctly (centered) initially.
      VisualTreeHelperEx.FindDescendantWithPropertyValue( this, Button.IsPressedProperty, true );

      // User has not defined a ViewFinder, use the one from this template
      if( this.GetValue( Zoombox.ViewFinderPropertyKey.DependencyProperty ) == null )
      {
        // set a reference to the ViewFinder element, if present
        this.SetValue( Zoombox.ViewFinderPropertyKey, this.Template.FindName( "ViewFinder", this ) as FrameworkElement );
        Zoombox.SetViewFinderVisibility( this, Visibility.Collapsed );
      }
      else
      {
        //user has defined a ViewFinder, hide the one from this template
        Zoombox.SetViewFinderVisibility( this, Visibility.Hidden );
      }

      // locate the view finder display panel
      if( this.ViewFinder != null )
      {
        _viewFinderDisplay = VisualTreeHelperEx.FindDescendantByType( this.ViewFinder, typeof( ZoomboxViewFinderDisplay ) ) as ZoomboxViewFinderDisplay;
      }

      // if a ViewFinder was specified but no display panel is present, throw an exception
      if( this.ViewFinder != null && _viewFinderDisplay == null )
        throw new InvalidTemplateException( ErrorMessages.GetMessage( "ZoomboxHasViewFinderButNotDisplay" ) );

      // set up the VisualBrush and adorner for the display panel
      if( _viewFinderDisplay != null )
      {
        // create VisualBrush for the view finder display panel
        this.CreateVisualBrushForViewFinder( _content );

        // hook up event handlers for dragging and resizing the viewport
        _viewFinderDisplay.MouseMove += new MouseEventHandler( this.ViewFinderDisplayMouseMove );
        _viewFinderDisplay.MouseLeftButtonDown += new MouseButtonEventHandler( this.ViewFinderDisplayBeginCapture );
        _viewFinderDisplay.MouseLeftButtonUp += new MouseButtonEventHandler( this.ViewFinderDisplayEndCapture );

        // bind the ViewportRect property of the display to the Viewport of the Zoombox
        Binding binding = new Binding( "Viewport" );
        binding.Mode = BindingMode.OneWay;
        binding.Converter = new ViewFinderSelectionConverter( this );
        binding.Source = this;
        _viewFinderDisplay.SetBinding( ZoomboxViewFinderDisplay.ViewportRectProperty, binding );
      }

      this.UpdateViewFinderDisplayContentBounds();

      // set up event handler to run once the content presenter has been arranged
      _contentPresenter.LayoutUpdated += new EventHandler( this.ContentPresenterFirstArranged );

      _isUpdatingVisualTree = false;
    }

    private void CreateVisualBrushForViewFinder( Visual visual )
    {
      _viewFinderDisplay.VisualBrush = new VisualBrush( visual );
      _viewFinderDisplay.VisualBrush.Stretch = Stretch.Uniform;
      _viewFinderDisplay.VisualBrush.AlignmentX = AlignmentX.Left;
      _viewFinderDisplay.VisualBrush.AlignmentY = AlignmentY.Top;
    }

    private void ContentPresenterFirstArranged( object sender, EventArgs e )
    {
      // remove the handler
      _contentPresenter.LayoutUpdated -= new EventHandler( this.ContentPresenterFirstArranged );

      // it's now safe to update the view
      this.HasArrangedContentPresenter = true;
      this.InvalidateVisual();

      // temporarily disable animations
      bool oldAnimated = IsAnimated;
      this.IsAnimated = false;
      try
      {
        // set the initial view
        double scale = this.Scale;
        Point position = this.Position;

        // if there are items in the ViewStack and a ViewStackIndex is set, use it
        if( this.EffectiveViewStackMode != ZoomboxViewStackMode.Disabled )
        {
          bool isInitialViewSet = false;
          if( this.ViewStack.Count > 0 )
          {
            if( this.ViewStackIndex >= 0 )
            {
              if( this.ViewStackIndex > this.ViewStack.Count - 1 )
              {
                this.ViewStackIndex = this.ViewStack.Count - 1;
              }
              else
              {
                this.UpdateView( this.ViewStack[ this.ViewStackIndex ], false, false, this.ViewStackIndex );
              }
            }
            else if( this.EffectiveViewStackMode != ZoomboxViewStackMode.Auto )
            {
              // if this is not an auto-stack, then ensure the index is set to a valid value
              if( this.ViewStackIndex < 0 )
              {
                this.ViewStackIndex = 0;
              }
            }

            // if a ViewStackIndex has been set, apply the scale and position values, iff different than the default values
            if( this.ViewStackIndex >= 0 )
            {
              isInitialViewSet = true;
              if( !DoubleHelper.IsNaN( scale ) || !PointHelper.IsEmpty( position ) )
              {
                this.UpdateView( new ZoomboxView( scale, position ), false, false );
              }
            }
          }

          if( !isInitialViewSet )
          {
            // set initial view according to the scale and position values and push it on the stack, as necessary
            ZoomboxView initialView = new ZoomboxView( DoubleHelper.IsNaN( Scale ) ? 1.0 : Scale,
              PointHelper.IsEmpty( position ) ? new Point() : position );

            if( this.EffectiveViewStackMode == ZoomboxViewStackMode.Auto )
            {
              this.ViewStack.PushView( initialView );
              this.ViewStackIndex = 0;
            }
            else
            {
              this.UpdateView( initialView, false, false );
            }
          }
        }
        else
        {
          // just set initial view according to the scale and position values
          ZoomboxView initialView = new ZoomboxView( DoubleHelper.IsNaN( Scale ) ? 1.0 : this.Scale, position );
          this.UpdateView( initialView, false, false );
        }
      }
      finally
      {
        IsAnimated = oldAnimated;
      }
      //When ViewFinder is modified, this will refresh the ZoomboxViewFinderDisplay
      this.ZoomTo( this.Scale );
    }

    private void DetachFromVisualTree()
    {
      // remove the drag adorner
      if( (_dragAdorner != null) && ( AdornerLayer.GetAdornerLayer( this ) != null ) )
        AdornerLayer.GetAdornerLayer( this ).Remove( _dragAdorner );

      // remove the layout updated handler, if present
      if( _contentPresenter != null )
      {
        _contentPresenter.LayoutUpdated -= new EventHandler( this.ContentPresenterFirstArranged );
      }

      //locate the vertical scrollBar
      if( _verticalScrollBar != null )
      {
        _verticalScrollBar.Scroll -= this.VerticalScrollBar_Scroll;
      }

      //locate the horizontal scrollBar
      if( _horizontalScrollBar != null )
      {
        _horizontalScrollBar.Scroll -= this.HorizontalScrollBar_Scroll;
      }

      // remove the view finder display panel's visual brush and adorner
      if( _viewFinderDisplay != null )
      {
        _viewFinderDisplay.MouseMove -= new MouseEventHandler( this.ViewFinderDisplayMouseMove );
        _viewFinderDisplay.MouseLeftButtonDown -= new MouseButtonEventHandler( this.ViewFinderDisplayBeginCapture );
        _viewFinderDisplay.MouseLeftButtonUp -= new MouseButtonEventHandler( this.ViewFinderDisplayEndCapture );
        BindingOperations.ClearBinding( _viewFinderDisplay, ZoomboxViewFinderDisplay.ViewportRectProperty );
        _viewFinderDisplay = null;
      }

      // set object references to null
      _contentPresenter = null;
    }

    private void Zoombox_Loaded( object sender, RoutedEventArgs e )
    {
      this.SetScrollBars();
    }

    private void VerticalScrollBar_Scroll( object sender, ScrollEventArgs e )
    {
      double diff = -(e.NewValue + _relativePosition.Y);

      if( e.ScrollEventType == ScrollEventType.LargeIncrement )
      {
        diff = -_verticalScrollBar.ViewportSize;
      }
      else if( e.ScrollEventType == ScrollEventType.LargeDecrement )
      {
        diff = _verticalScrollBar.ViewportSize;
      }

      this.OnDrag( new DragDeltaEventArgs( 0d, diff / this.Scale ), false );

      // Raise the Scroll event to user
      EventHandler<ScrollEventArgs> handler = this.Scroll;
      if( handler != null )
      {
        handler( this, e );
      }
    }

    private void HorizontalScrollBar_Scroll( object sender, ScrollEventArgs e )
    {
      double diff = -( e.NewValue + _relativePosition.X );
      if( e.ScrollEventType == ScrollEventType.LargeIncrement )
      {
        diff = -_horizontalScrollBar.ViewportSize;
      }
      else if( e.ScrollEventType == ScrollEventType.LargeDecrement )
      {
        diff = _horizontalScrollBar.ViewportSize;
      }

      this.OnDrag( new DragDeltaEventArgs( diff / this.Scale, 0d ), false );

      // Raise the Scroll event to user
      EventHandler<ScrollEventArgs> handler = this.Scroll;
      if( handler != null )
      {
        handler( this, e );
      }
    }

    private void DragDisplayViewport( DragDeltaEventArgs e, bool end )
    {
      // get the scale of the view finder display panel, the selection rect, and the VisualBrush rect
      double scale = _viewFinderDisplay.Scale;
      Rect viewportRect = _viewFinderDisplay.ViewportRect;
      Rect vbRect = _viewFinderDisplay.ContentBounds;

      // if the entire content is visible, do nothing
      if( viewportRect.Contains( vbRect ) )
        return;

      // ensure that we stay within the bounds of the VisualBrush
      double dx = e.HorizontalChange;
      double dy = e.VerticalChange;

      // check left boundary
      if( viewportRect.Left < vbRect.Left )
      {
        dx = Math.Max( 0, dx );
      }
      else if( viewportRect.Left + dx < vbRect.Left )
      {
        dx = vbRect.Left - viewportRect.Left;
      }

      // check right boundary
      if( viewportRect.Right > vbRect.Right )
      {
        dx = Math.Min( 0, dx );
      }
      else if( viewportRect.Right + dx > vbRect.Left + vbRect.Width )
      {
        dx = vbRect.Left + vbRect.Width - viewportRect.Right;
      }

      // check top boundary
      if( viewportRect.Top < vbRect.Top )
      {
        dy = Math.Max( 0, dy );
      }
      else if( viewportRect.Top + dy < vbRect.Top )
      {
        dy = vbRect.Top - viewportRect.Top;
      }

      // check bottom boundary
      if( viewportRect.Bottom > vbRect.Bottom )
      {
        dy = Math.Min( 0, dy );
      }
      else if( viewportRect.Bottom + dy > vbRect.Top + vbRect.Height )
      {
        dy = vbRect.Top + vbRect.Height - viewportRect.Bottom;
      }

      // call the main OnDrag handler that is used when dragging the content directly
      this.OnDrag( new DragDeltaEventArgs( -dx / scale / _viewboxFactor, -dy / scale / _viewboxFactor ), end );

      // for a drag operation, update the origin with each delta
      _originPoint = _originPoint + new Vector( dx, dy );
    }

    private void InitCommands()
    {
      CommandBinding binding = new CommandBinding( Zoombox.Back, this.GoBack, this.CanGoBack );
      this.CommandBindings.Add( binding );

      binding = new CommandBinding( Zoombox.Center, this.CenterContent );
      this.CommandBindings.Add( binding );

      binding = new CommandBinding( Zoombox.Fill, this.FillToBounds );
      this.CommandBindings.Add( binding );

      binding = new CommandBinding( Zoombox.Fit, this.FitToBounds );
      this.CommandBindings.Add( binding );

      binding = new CommandBinding( Zoombox.Forward, this.GoForward, this.CanGoForward );
      this.CommandBindings.Add( binding );

      binding = new CommandBinding( Zoombox.Home, this.GoHome, this.CanGoHome );
      this.CommandBindings.Add( binding );

      binding = new CommandBinding( Zoombox.PanDown, this.PanDownExecuted );
      this.CommandBindings.Add( binding );

      binding = new CommandBinding( Zoombox.PanLeft, this.PanLeftExecuted );
      this.CommandBindings.Add( binding );

      binding = new CommandBinding( Zoombox.PanRight, this.PanRightExecuted );
      this.CommandBindings.Add( binding );

      binding = new CommandBinding( Zoombox.PanUp, this.PanUpExecuted );
      this.CommandBindings.Add( binding );

      binding = new CommandBinding( Zoombox.Refocus, this.RefocusView, this.CanRefocusView );
      this.CommandBindings.Add( binding );

      binding = new CommandBinding( Zoombox.ZoomIn, this.ZoomInExecuted );
      this.CommandBindings.Add( binding );

      binding = new CommandBinding( Zoombox.ZoomOut, this.ZoomOutExecuted );
      this.CommandBindings.Add( binding );
    }

    private void MonitorInput()
    {
      // cannot pre-process input in partial trust
      if( this.HasUIPermission )
      {
        this.PreProcessInput();
      }
    }

    private void OnContentSizeChanged( object sender, SizeChangedEventArgs e )
    {
      this.UpdateViewFinderDisplayContentBounds();

      if( this.HasArrangedContentPresenter )
      {
        if( this.HasRenderedFirstView )
        {
          this.SetScrollBars();
          this.UpdateView( this.CurrentView, true, false, this.CurrentViewIndex );
        }
        else
        {
          // if the content size changes after the content presenter has been arranged,
          // but before the first view is rendered, invalidate the render so we can refocus 
          // the view on the first render
          this.RefocusViewOnFirstRender = true;
          this.InvalidateVisual();
        }
      }
    }

    private void OnDrag( DragDeltaEventArgs e, bool end )
    {
      Point relativePosition = _relativePosition;
      double scale = this.Scale;
      Point newPosition = relativePosition + ( this.ContentOffset * scale ) + new Vector( e.HorizontalChange * scale, e.VerticalChange * scale );
      if( this.IsUsingScrollBars )
      {
        newPosition.X = Math.Max( Math.Min( newPosition.X, 0d ), -_horizontalScrollBar.Maximum );
        newPosition.Y = Math.Max( Math.Min( newPosition.Y, 0d ), -_verticalScrollBar.Maximum );
      }

      // update the transform
      this.UpdateView( new ZoomboxView( scale, newPosition ), false, end );
    }

    private void OnLayoutUpdated( object sender, EventArgs e )
    {
      this.UpdateViewport();
    }

    private void OnSelectRegion( DragDeltaEventArgs e, bool end )
    {
      // draw adorner rect
      if( end )
      {
        _dragAdorner.Rect = Rect.Empty;

        if( _trueContent != null )
        {
          // get the selected region (in the content's coordinate space) based on the adorner's last position and size
          Rect selection =
            new Rect(
              this.TranslatePoint( _dragAdorner.LastPosition, _trueContent ),
              this.TranslatePoint( _dragAdorner.LastPosition + new Vector( _dragAdorner.LastSize.Width, _dragAdorner.LastSize.Height ), _trueContent ) );

          // zoom to the selection
          this.ZoomTo( selection );
        }
      }
      else
      {
        _dragAdorner.Rect =
          Rect.Intersect(
            new Rect(
              _originPoint,
              new Vector( e.HorizontalChange, e.VerticalChange ) ),
            new Rect(
              new Point( 0, 0 ),
              new Point( this.RenderSize.Width, this.RenderSize.Height ) ) );
      }
    }

    private void OnSizeChanged( object sender, SizeChangedEventArgs e )
    {
      if( !this.HasArrangedContentPresenter )
        return;

      this.SetScrollBars();

      // when the size is changing, the viewbox factor must be updated before updating the view
      this.UpdateViewboxFactor();

      bool oldIsAnimated = this.IsAnimated;
      this.IsAnimated = false;
      try
      {
        this.UpdateView( this.CurrentView, false, false, this.ViewStackIndex );
      }
      finally
      {
        this.IsAnimated = oldIsAnimated;
      }
    }

    private void SetScrollBars()
    {
      if( _content == null || _verticalScrollBar == null || _horizontalScrollBar == null )
        return;

      var contentSize = ( _content is Viewbox ) ? ( ( Viewbox )_content ).Child.DesiredSize : this.RenderSize;

      _verticalScrollBar.SmallChange = 10d;
      _verticalScrollBar.LargeChange = 10d;
      _verticalScrollBar.Minimum = 0d;
      _verticalScrollBar.ViewportSize = this.RenderSize.Height;
      _verticalScrollBar.Maximum = contentSize.Height - _verticalScrollBar.ViewportSize;

      _horizontalScrollBar.SmallChange = 10d;
      _horizontalScrollBar.LargeChange = 10d;
      _horizontalScrollBar.Minimum = 0d;
      _horizontalScrollBar.ViewportSize = this.RenderSize.Width;
      _horizontalScrollBar.Maximum = contentSize.Width - _horizontalScrollBar.ViewportSize;
    }

    private void PreProcessInput()
    {
      // if mouse is over the Zoombox element or if it has keyboard focus, pre-process input 
      // to update the KeyModifier trigger properties (e.g., DragModifiersAreActive)
      if( this.IsMouseOver || this.IsKeyboardFocusWithin )
      {
        if( !this.IsMonitoringInput )
        {
          this.IsMonitoringInput = true;
          InputManager.Current.PreNotifyInput += new NotifyInputEventHandler( this.PreProcessInput );
          this.UpdateKeyModifierTriggerProperties();
        }
      }
      else
      {
        if( this.IsMonitoringInput )
        {
          this.IsMonitoringInput = false;
          InputManager.Current.PreNotifyInput -= new NotifyInputEventHandler( this.PreProcessInput );

          this.SetAreDragModifiersActive( false );
          this.SetAreRelativeZoomModifiersActive( false );
          this.SetAreZoomModifiersActive( false );
          this.SetAreZoomToSelectionModifiersActive( false );
        }
      }
    }

    private void PreProcessInput( object sender, NotifyInputEventArgs e )
    {
      if( e.StagingItem.Input is KeyEventArgs )
      {
        this.UpdateKeyModifierTriggerProperties();
      }
    }

    private void ProcessMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      if( this.ZoomToSelectionModifiers.AreActive )
      {
        this.SetIsDraggingContent( false );
        this.SetIsSelectingRegion( true );
      }
      else if( this.DragModifiers.AreActive )
      {
        this.SetIsSelectingRegion( false );
        this.SetIsDraggingContent( true );
      }
      else
      {
        this.SetIsSelectingRegion( false );
        this.SetIsDraggingContent( false );
      }

      // if nothing to do, just return
      if( !this.IsSelectingRegion && !this.IsDraggingContent )
        return;

      // set the origin point and capture the mouse
      _originPoint = e.GetPosition( this );
      _contentPresenter.CaptureMouse();
      e.Handled = true;
      if( this.IsDraggingContent )
      {
        // execute the Drag operation
        this.OnDrag( new DragDeltaEventArgs( 0, 0 ), false );
      }
      else if( this.IsSelectingRegion )
      {
        this.OnSelectRegion( new DragDeltaEventArgs( 0, 0 ), false );
      }
    }

    private void ProcessMouseLeftButtonUp( MouseButtonEventArgs e )
    {
      if( !this.IsDraggingContent && !this.IsSelectingRegion )
        return;

      bool endDrag = this.IsDraggingContent;

      this.SetIsDraggingContent( false );
      this.SetIsSelectingRegion( false );

      _originPoint = new Point();
      _contentPresenter.ReleaseMouseCapture();
      e.Handled = true;

      if( endDrag )
      {
        this.OnDrag( new DragDeltaEventArgs( 0, 0 ), true );
      }
      else
      {
        this.OnSelectRegion( new DragDeltaEventArgs( 0, 0 ), true );
      }
    }

    private void ProcessMouseMove( MouseEventArgs e )
    {
      if( e.MouseDevice.LeftButton != MouseButtonState.Pressed )
        return;

      if( !this.IsDraggingContent && !this.IsSelectingRegion )
        return;

      Point pos = e.GetPosition( this );
      e.Handled = true;

      if( this.IsDraggingContent )
      {
        Vector delta = ( pos - _originPoint ) / this.Scale;
        this.OnDrag( new DragDeltaEventArgs( delta.X, delta.Y ), false );
        _originPoint = pos;
      }
      else if( this.IsSelectingRegion )
      {
        Vector delta = pos - _originPoint;
        this.OnSelectRegion( new DragDeltaEventArgs( delta.X, delta.Y ), false );
      }
    }

    private void ProcessMouseWheelZoom( MouseWheelEventArgs e )
    {
      if( _content == null )
        return;

      // check modifiers to see if there's work to do
      bool doZoom = this.ZoomModifiers.AreActive;
      bool doRelativeZoom = this.RelativeZoomModifiers.AreActive;

      // can't do both, so assume relative zoom
      if( doZoom && doRelativeZoom )
      {
        doZoom = false;
      }

      if( !( doZoom || doRelativeZoom ) )
        return;

      e.Handled = true;
      double percentage = ( ( e.Delta / Zoombox.MOUSE_WHEEL_DELTA ) * this.ZoomPercentage ) / 100;

      // Are we doing a zoom relative to the current mouse position?
      if( doRelativeZoom )
      {
        this.Zoom( percentage, Mouse.GetPosition( _content ) );
      }
      else
      {
        this.Zoom( percentage );
      }
    }

    private void ProcessNavigationButton( RoutedEventArgs e )
    {
      if( e is MouseButtonEventArgs )
      {
        MouseButtonEventArgs mbea = e as MouseButtonEventArgs;
        if( mbea.ChangedButton == MouseButton.XButton1
            || mbea.ChangedButton == MouseButton.XButton2 )
        {
          if( mbea.ChangedButton == MouseButton.XButton2 )
          {
            this.GoForward();
          }
          else
          {
            this.GoBack();
          }
          mbea.Handled = true;
        }
      }
      else if( e is KeyEventArgs )
      {
        KeyEventArgs kea = e as KeyEventArgs;
        if( kea.Key == Key.Back || kea.Key == Key.BrowserBack || kea.Key == Key.BrowserForward )
        {
          if( kea.Key == Key.BrowserForward )
          {
            this.GoForward();
          }
          else
          {
            this.GoBack();
          }
          kea.Handled = true;
        }
      }
    }

    private void ResizeDisplayViewport( DragDeltaEventArgs e, ResizeEdge relativeTo )
    {
      // get the existing viewport rect and scale
      Rect viewportRect = _viewFinderDisplay.ViewportRect;
      double scale = _viewFinderDisplay.Scale;

      // ensure that we stay within the bounds of the VisualBrush
      double x = Math.Max( _resizeViewportBounds.Left, Math.Min( _resizeDraggingPoint.X + e.HorizontalChange, _resizeViewportBounds.Right ) );
      double y = Math.Max( _resizeViewportBounds.Top, Math.Min( _resizeDraggingPoint.Y + e.VerticalChange, _resizeViewportBounds.Bottom ) );

      // get the selected region in the coordinate space of the content
      Point anchorPoint = new Point( _resizeAnchorPoint.X / scale, _resizeAnchorPoint.Y / scale );
      Vector newRegionVector = new Vector( ( x - _resizeAnchorPoint.X ) / scale / _viewboxFactor, ( y - _resizeAnchorPoint.Y ) / scale / _viewboxFactor );
      Rect region = new Rect( anchorPoint, newRegionVector );

      // now translate the region from the coordinate space of the content 
      // to the coordinate space of the content presenter
      region =
        new Rect(
          _content.TranslatePoint( region.TopLeft, _contentPresenter ),
          _content.TranslatePoint( region.BottomRight, _contentPresenter ) );

      // calculate actual scale value
      double aspectX = this.RenderSize.Width / region.Width;
      double aspectY = this.RenderSize.Height / region.Height;
      scale = aspectX < aspectY ? aspectX : aspectY;

      // scale relative to the anchor point
      this.ZoomTo( scale, anchorPoint, false, false );
    }

    private void UpdateKeyModifierTriggerProperties()
    {
      this.SetAreDragModifiersActive( this.DragModifiers.AreActive );
      this.SetAreRelativeZoomModifiersActive( this.RelativeZoomModifiers.AreActive );
      this.SetAreZoomModifiersActive( this.ZoomModifiers.AreActive );
      this.SetAreZoomToSelectionModifiersActive( this.ZoomToSelectionModifiers.AreActive );
    }

    private void UpdateView( ZoomboxView view, bool allowAnimation, bool allowStackAddition )
    {
      this.UpdateView( view, allowAnimation, allowStackAddition, -1 );
    }

    private void UpdateView( ZoomboxView view, bool allowAnimation, bool allowStackAddition, int stackIndex )
    {
      if( _contentPresenter == null || _content == null || !this.HasArrangedContentPresenter )
        return;

      // if an absolute view is being used and only a Scale value has been specified,
      // use the ZoomTo() function to perform a relative zoom
      if( view.ViewKind == ZoomboxViewKind.Absolute && PointHelper.IsEmpty( view.Position ) )
      {
        this.ZoomTo( view.Scale, allowStackAddition );
        return;
      }

      // disallow reentrancy
      if( !this.IsUpdatingView )
      {
        this.IsUpdatingView = true;
        try
        {
          // determine the new scale and position
          double newRelativeScale = _viewboxFactor;
          Point newRelativePosition = new Point();
          Rect region = Rect.Empty;
          switch( view.ViewKind )
          {
            case ZoomboxViewKind.Empty:
              break;

            case ZoomboxViewKind.Absolute:
              newRelativeScale = DoubleHelper.IsNaN( view.Scale ) ? _relativeScale : view.Scale;
              newRelativePosition = PointHelper.IsEmpty( view.Position ) ? _relativePosition
                  : new Point( view.Position.X, view.Position.Y ) - this.ContentOffset * newRelativeScale;
              break;

            case ZoomboxViewKind.Region:
              region = view.Region;
              break;

            case ZoomboxViewKind.Center:
              {
                // get the current ContentRect in the coordinate space of the Zoombox control
                Rect currentContentRect =
                  new Rect(
                    _content.TranslatePoint( this.ContentRect.TopLeft, this ),
                    _content.TranslatePoint( this.ContentRect.BottomRight, this ) );

                // inflate (or deflate) the rect by the appropriate amounts in the x & y directions
                region = Rect.Inflate( currentContentRect,
                    ( this.RenderSize.Width / _viewboxFactor - currentContentRect.Width ) / 2,
                    ( this.RenderSize.Height / _viewboxFactor - currentContentRect.Height ) / 2 );

                // now translate the centered rect back to the coordinate space of the content
                region = new Rect( this.TranslatePoint( region.TopLeft, _content ), this.TranslatePoint( region.BottomRight, _content ) );
              }
              break;

            case ZoomboxViewKind.Fit:
              region = this.ContentRect;
              break;

            case ZoomboxViewKind.Fill:
              region = this.CalculateFillRect();
              break;
          }

          if( view.ViewKind != ZoomboxViewKind.Empty )
          {
            if( !region.IsEmpty )
            {   // ZOOMING TO A REGION
              this.CalculatePositionAndScale( region, ref newRelativePosition, ref newRelativeScale );
            }
            else if( view != ZoomboxView.Empty )
            {   // USING ABSOLUTE POSITION AND SCALE VALUES

              // ensure that the scale value falls within the valid range
              if( newRelativeScale > MaxScale )
              {
                newRelativeScale = MaxScale;
              }
              else if( newRelativeScale < MinScale )
              {
                newRelativeScale = MinScale;
              }
            }

            double currentScale = _relativeScale;
            double currentX = _relativePosition.X;
            double currentY = _relativePosition.Y;

            ScaleTransform st = null;
            TranslateTransform tt = null;
            TransformGroup tg = null;

            if( _contentPresenter.RenderTransform != Transform.Identity )
            {
              tg = _contentPresenter.RenderTransform as TransformGroup;
              st = tg.Children[ 0 ] as ScaleTransform;
              tt = tg.Children[ 1 ] as TranslateTransform;
              currentScale = st.ScaleX;
              currentX = tt.X;
              currentY = tt.Y;
            }

            if( KeepContentInBounds == true )
            {
              Rect boundsRect = new Rect( new Size( this.ContentRect.Width * newRelativeScale, this.ContentRect.Height * newRelativeScale ) );

              // Calc viewport rect (should be inside bounds content rect)
              Point viewportPosition = new Point( -newRelativePosition.X, -newRelativePosition.Y );
              Rect viewportRect = new Rect( viewportPosition, _contentPresenter.RenderSize );

              if( DoubleHelper.AreVirtuallyEqual( _relativeScale, newRelativeScale ) ) // we are positioning the content, not scaling
              {
                // Handle the width and height seperately since the content extent 
                // could be contained only partially in the viewport.  Also if the viewport is only 
                // partially contained within the content extent.

                //
                // Content extent width is greater than the viewport's width (Zoomed in).  Make sure we restrict
                // the viewport X inside the content.
                //
                if( this.IsGreaterThanOrClose( boundsRect.Width, viewportRect.Width ) )
                {
                  if( boundsRect.Right < viewportRect.Right )
                  {
                    newRelativePosition.X = -( boundsRect.Width - viewportRect.Width );
                  }

                  if( boundsRect.Left > viewportRect.Left )
                  {
                    newRelativePosition.X = 0;
                  }
                }
                //
                // Viewport width is greater than the content extent's width (Zoomed out).  Make sure we restrict
                // the content X inside the viewport.
                //
                else if( this.IsGreaterThanOrClose( viewportRect.Width, boundsRect.Width ) )
                {
                  if( viewportRect.Right < boundsRect.Right )
                  {
                    newRelativePosition.X = viewportRect.Width - boundsRect.Width;
                  }

                  if( viewportRect.Left > boundsRect.Left )
                  {
                    newRelativePosition.X = 0;
                  }
                }

                //
                // Content extent height is greater than the viewport's height (Zoomed in).  Make sure we restrict
                // the viewport Y inside the content.
                //
                if( this.IsGreaterThanOrClose( boundsRect.Height, viewportRect.Height ) )
                {
                  if( boundsRect.Bottom < viewportRect.Bottom )
                  {
                    newRelativePosition.Y = -( boundsRect.Height - viewportRect.Height );
                  }

                  if( boundsRect.Top > viewportRect.Top )
                  {
                    newRelativePosition.Y = 0;
                  }
                }
                //
                // Viewport height is greater than the content extent's height (Zoomed out).  Make sure we restrict
                // the content Y inside the viewport.
                //
                else if( this.IsGreaterThanOrClose( viewportRect.Height, boundsRect.Height ) )
                {
                  if( viewportRect.Bottom < boundsRect.Bottom )
                  {
                    newRelativePosition.Y = viewportRect.Height - boundsRect.Height;
                  }

                  if( viewportRect.Top > boundsRect.Top )
                  {
                    newRelativePosition.Y = 0;
                  }
                }
              }
            }

            st = new ScaleTransform( newRelativeScale / _viewboxFactor, newRelativeScale / _viewboxFactor );
            tt = new TranslateTransform( newRelativePosition.X, newRelativePosition.Y );
            tg = new TransformGroup();
            tg.Children.Add( st );
            tg.Children.Add( tt );

            _contentPresenter.RenderTransform = tg;

            var initialContentSize = ( _content is Viewbox ) ? ( ( Viewbox )_content ).Child.DesiredSize : this.RenderSize;
            var scaledContentSize = new Size( initialContentSize.Width * newRelativeScale, initialContentSize.Height * newRelativeScale );

            if( allowAnimation && IsAnimated )
            {
              DoubleAnimation daScale = new DoubleAnimation( currentScale, newRelativeScale / _viewboxFactor, AnimationDuration );
              daScale.AccelerationRatio = this.AnimationAccelerationRatio;
              daScale.DecelerationRatio = this.AnimationDecelerationRatio;

              DoubleAnimation daTranslateX = new DoubleAnimation( currentX, newRelativePosition.X, AnimationDuration );
              daTranslateX.AccelerationRatio = this.AnimationAccelerationRatio;
              daTranslateX.DecelerationRatio = this.AnimationDecelerationRatio;

              DoubleAnimation daTranslateY = new DoubleAnimation( currentY, newRelativePosition.Y, AnimationDuration );
              daTranslateY.AccelerationRatio = this.AnimationAccelerationRatio;
              daTranslateY.DecelerationRatio = this.AnimationDecelerationRatio;
              daTranslateY.CurrentTimeInvalidated += new EventHandler( this.UpdateViewport );
              daTranslateY.CurrentStateInvalidated += new EventHandler( this.ZoomAnimationCompleted );

              // raise animation beginning event before beginning the animations
              RaiseEvent( new RoutedEventArgs( AnimationBeginningEvent, this ) );

              st.BeginAnimation( ScaleTransform.ScaleXProperty, daScale );
              st.BeginAnimation( ScaleTransform.ScaleYProperty, daScale );
              tt.BeginAnimation( TranslateTransform.XProperty, daTranslateX );
              tt.BeginAnimation( TranslateTransform.YProperty, daTranslateY );

              if( this.IsUsingScrollBars )
              {
                //Vertical scrollBar animations
                DoubleAnimation verticalMaxAnimation = new DoubleAnimation();
                verticalMaxAnimation.From = _verticalScrollBar.Maximum;
                verticalMaxAnimation.To = scaledContentSize.Height - _verticalScrollBar.ViewportSize;
                verticalMaxAnimation.Duration = AnimationDuration;
                _verticalScrollBar.BeginAnimation( ScrollBar.MaximumProperty, verticalMaxAnimation );

                DoubleAnimation verticalValueAnimation = new DoubleAnimation();
                verticalValueAnimation.From = _verticalScrollBar.Value;
                verticalValueAnimation.To = -newRelativePosition.Y;
                verticalValueAnimation.Duration = AnimationDuration;
                verticalValueAnimation.Completed += this.VerticalValueAnimation_Completed;
                _verticalScrollBar.BeginAnimation( ScrollBar.ValueProperty, verticalValueAnimation );

                //Horizontal scrollBar animations
                DoubleAnimation horizontalMaxAnimation = new DoubleAnimation();
                horizontalMaxAnimation.From = _horizontalScrollBar.Maximum;
                horizontalMaxAnimation.To = scaledContentSize.Width - _horizontalScrollBar.ViewportSize;
                horizontalMaxAnimation.Duration = AnimationDuration;
                _horizontalScrollBar.BeginAnimation( ScrollBar.MaximumProperty, horizontalMaxAnimation );

                DoubleAnimation horizontalValueAnimation = new DoubleAnimation();
                horizontalValueAnimation.From = _horizontalScrollBar.Value;
                horizontalValueAnimation.To = -newRelativePosition.X;
                horizontalValueAnimation.Duration = AnimationDuration;
                horizontalValueAnimation.Completed += this.HorizontalValueAnimation_Completed;
                _horizontalScrollBar.BeginAnimation( ScrollBar.ValueProperty, horizontalValueAnimation );
              }
            }
            else if( this.IsUsingScrollBars )
            {
              //Vertical scrollBar
              _verticalScrollBar.Maximum = scaledContentSize.Height - _verticalScrollBar.ViewportSize;
              _verticalScrollBar.Value = -newRelativePosition.Y;
              //Horizontal scrollBar
              _horizontalScrollBar.Maximum = scaledContentSize.Width - _horizontalScrollBar.ViewportSize;
              _horizontalScrollBar.Value = -newRelativePosition.X;
            }

            // maintain the relative scale and position for dragging and animating purposes
            _relativePosition = newRelativePosition;
            _relativeScale = newRelativeScale;

            // update the Scale and Position properties to keep them in sync with the current view
            this.Scale = newRelativeScale;
            _basePosition = newRelativePosition + this.ContentOffset * newRelativeScale;
            this.UpdateViewport();
          }

          // add the current view to the view stack
          if( this.EffectiveViewStackMode == ZoomboxViewStackMode.Auto && allowStackAddition )
          {
            // if the last view was pushed on the stack within the last 300 milliseconds, discard it
            // since proximally close views are probably the result of a mouse wheel scroll
            if( this.ViewStack.Count > 1
              && Math.Abs( DateTime.Now.Ticks - _lastStackAddition.Ticks ) < TimeSpan.FromMilliseconds( 300 ).Ticks )
            {
              this.ViewStack.RemoveAt( this.ViewStack.Count - 1 );
              _lastStackAddition = DateTime.Now - TimeSpan.FromMilliseconds( 300 );
            }

            // if the current view is the same as the last view, do nothing
            if( this.ViewStack.Count > 0 && view == this.ViewStack.SelectedView )
            {
              // do nothing
            }
            else
            {
              // otherwise, push the current view on stack
              this.ViewStack.PushView( view );
              this.ViewStackIndex++;
              stackIndex = this.ViewStackIndex;

              // update the timestamp for the last stack addition
              _lastStackAddition = DateTime.Now;
            }
          }

          // update the stack indices used by CurrentViewChanged event
          _lastViewIndex = this.CurrentViewIndex;
          this.SetCurrentViewIndex( stackIndex );

          // set the new view parameters
          // NOTE: this is the only place where the CurrentView member should be set
          this.SetCurrentView( view );
        }
        finally
        {
          this.IsUpdatingView = false;
        }
      }
    }

    private bool IsGreaterThanOrClose( double value1, double value2 )
    {
      return value1 <= value2 ? DoubleHelper.AreVirtuallyEqual( value1, value2 ) : true;
    }

    private Rect CalculateFillRect()
    {
      // determine the x-y ratio of the current Viewport
      double xyRatio = this.RenderSize.Width / this.RenderSize.Height;

      // now find the maximal rect within the ContentRect that has the same ratio
      double x = 0;
      double y = 0;
      double width = this.ContentRect.Width;
      double height = this.ContentRect.Height;
      if( xyRatio > width / height )
      {
        height = width / xyRatio;
        y = ( this.ContentRect.Height - height ) / 2;
      }
      else
      {
        width = height * xyRatio;
        x = ( this.ContentRect.Width - width ) / 2;
      }

      return new Rect( x, y, width, height );
    }

    private void CalculatePositionAndScale( Rect region, ref Point newRelativePosition, ref double newRelativeScale )
    {
      // if there is nothing to scale, just return
      // if the region has no area, just return
      if( region.Width == 0 || region.Height == 0 )
        return;

      // verify that the selected region intersects with the content, which prevents 
      // the scale operation from zooming the content out of the current view
      if( !this.ContentRect.IntersectsWith( region ) )
        return;

      // translate the region from the coordinate space of the content 
      // to the coordinate space of the content presenter
      region =
        new Rect(
          _content.TranslatePoint( region.TopLeft, _contentPresenter ),
          _content.TranslatePoint( region.BottomRight, _contentPresenter ) );

      // calculate actual zoom, which must fit the entire selection 
      // while maintaining a 1:1 ratio
      double aspectX = this.RenderSize.Width / region.Width;
      double aspectY = this.RenderSize.Height / region.Height;
      newRelativeScale = aspectX < aspectY ? aspectX : aspectY;

      // ensure that the scale value falls within the valid range
      if( newRelativeScale > this.MaxScale )
      {
        newRelativeScale = this.MaxScale;
      }
      else if( newRelativeScale < this.MinScale )
      {
        newRelativeScale = this.MinScale;
      }

      // determine the new content position for this zoom operation based 
      // on HorizontalContentAlignment and VerticalContentAlignment
      double horizontalOffset = 0;
      double verticalOffset = 0;
      switch( this.HorizontalContentAlignment )
      {
        case HorizontalAlignment.Center:
        case HorizontalAlignment.Stretch:
          horizontalOffset = ( this.RenderSize.Width - region.Width * newRelativeScale ) / 2;
          break;

        case HorizontalAlignment.Right:
          horizontalOffset = ( this.RenderSize.Width - region.Width * newRelativeScale );
          break;
      }
      switch( VerticalContentAlignment )
      {
        case VerticalAlignment.Center:
        case VerticalAlignment.Stretch:
          verticalOffset = ( this.RenderSize.Height - region.Height * newRelativeScale ) / 2;
          break;

        case VerticalAlignment.Bottom:
          verticalOffset = ( this.RenderSize.Height - region.Height * newRelativeScale );
          break;
      }
      newRelativePosition =
        new Point( -region.TopLeft.X * newRelativeScale, -region.TopLeft.Y * newRelativeScale )
          + new Vector( horizontalOffset, verticalOffset );
    }

    private void UpdateViewFinderDisplayContentBounds()
    {
      if( _content == null || _trueContent == null || _viewFinderDisplay == null || _viewFinderDisplay.AvailableSize.IsEmpty )
        return;

      this.UpdateViewboxFactor();

      // ensure the display panel has a size
      Size contentSize = _content.RenderSize;
      Size viewFinderSize = _viewFinderDisplay.AvailableSize;
      if( viewFinderSize.Width > 0d && DoubleHelper.AreVirtuallyEqual( viewFinderSize.Height, 0d ) )
      {
        // update height to accomodate width, while keeping a ratio equal to the actual content
        viewFinderSize = new Size( viewFinderSize.Width, contentSize.Height * viewFinderSize.Width / contentSize.Width );
      }
      else if( viewFinderSize.Height > 0d && DoubleHelper.AreVirtuallyEqual( viewFinderSize.Width, 0d ) )
      {
        // update width to accomodate height, while keeping a ratio equal to the actual content
        viewFinderSize = new Size( contentSize.Width * viewFinderSize.Height / contentSize.Height, viewFinderSize.Width );
      }

      // determine the scale of the view finder display panel
      double aspectX = viewFinderSize.Width / contentSize.Width;
      double aspectY = viewFinderSize.Height / contentSize.Height;
      double scale = aspectX < aspectY ? aspectX : aspectY;

      // determine the rect of the VisualBrush
      double vbWidth = contentSize.Width * scale;
      double vbHeight = contentSize.Height * scale;

      // set the ContentBounds and Scale properties on the view finder display panel
      _viewFinderDisplay.Scale = scale;
      _viewFinderDisplay.ContentBounds = new Rect( new Size( vbWidth, vbHeight ) );
    }

    private void UpdateViewboxFactor()
    {
      if( _content == null || _trueContent == null )
        return;

      double contentWidth = _content.RenderSize.Width;
      double trueContentWidth = _trueContent.RenderSize.Width;

      if( DoubleHelper.AreVirtuallyEqual( contentWidth, 0d ) || DoubleHelper.AreVirtuallyEqual( trueContentWidth, 0d ) )
      {
        _viewboxFactor = 1d;
      }
      else
      {
        _viewboxFactor = contentWidth / trueContentWidth;
      }
    }

    private void UpdateViewport()
    {
      // if we haven't attached to the visual tree yet or we don't have content, just return
      if( _contentPresenter == null || _trueContent == null )
        return;

      this.IsUpdatingViewport = true;
      try
      {
        // calculate the current viewport
        Rect viewport =
          new Rect(
            this.TranslatePoint( new Point( 0d, 0d ), _trueContent ),
            this.TranslatePoint( new Point( RenderSize.Width, RenderSize.Height ), _trueContent ) );

        // if the viewport has changed, set the Viewport dependency property
        if( !DoubleHelper.AreVirtuallyEqual( viewport, this.Viewport ) )
        {
          this.SetValue( Zoombox.ViewportPropertyKey, viewport );
        }
      }
      finally
      {
        this.IsUpdatingViewport = false;
      }
    }

    private void UpdateViewport( object sender, EventArgs e )
    {
      this.UpdateViewport();
    }

    private void ViewFinderDisplayBeginCapture( object sender, MouseButtonEventArgs e )
    {
      const double ARBITRARY_LARGE_VALUE = 10000000000;

      // if we need to acquire capture, the Tag property of the view finder display panel
      // will be a ResizeEdge value.
      if( _viewFinderDisplay.Tag is ResizeEdge )
      {
        // if the Tag is ResizeEdge.None, then its a drag operation; otherwise, its a resize
        if( ( ResizeEdge )_viewFinderDisplay.Tag == ResizeEdge.None )
        {
          this.IsDraggingViewport = true;
        }
        else
        {
          this.IsResizingViewport = true;
          Vector direction = new Vector();
          switch( ( ResizeEdge )_viewFinderDisplay.Tag )
          {
            case ResizeEdge.TopLeft:
              _resizeDraggingPoint = _viewFinderDisplay.ViewportRect.TopLeft;
              _resizeAnchorPoint = _viewFinderDisplay.ViewportRect.BottomRight;
              direction = new Vector( -1, -1 );
              break;

            case ResizeEdge.TopRight:
              _resizeDraggingPoint = _viewFinderDisplay.ViewportRect.TopRight;
              _resizeAnchorPoint = _viewFinderDisplay.ViewportRect.BottomLeft;
              direction = new Vector( 1, -1 );
              break;

            case ResizeEdge.BottomLeft:
              _resizeDraggingPoint = _viewFinderDisplay.ViewportRect.BottomLeft;
              _resizeAnchorPoint = _viewFinderDisplay.ViewportRect.TopRight;
              direction = new Vector( -1, 1 );
              break;

            case ResizeEdge.BottomRight:
              _resizeDraggingPoint = _viewFinderDisplay.ViewportRect.BottomRight;
              _resizeAnchorPoint = _viewFinderDisplay.ViewportRect.TopLeft;
              direction = new Vector( 1, 1 );
              break;
            case ResizeEdge.Left:
              _resizeDraggingPoint = new Point( _viewFinderDisplay.ViewportRect.Left,
                  _viewFinderDisplay.ViewportRect.Top + ( _viewFinderDisplay.ViewportRect.Height / 2 ) );
              _resizeAnchorPoint = new Point( _viewFinderDisplay.ViewportRect.Right,
                  _viewFinderDisplay.ViewportRect.Top + ( _viewFinderDisplay.ViewportRect.Height / 2 ) );
              direction = new Vector( -1, 0 );
              break;
            case ResizeEdge.Top:
              _resizeDraggingPoint = new Point( _viewFinderDisplay.ViewportRect.Left + ( _viewFinderDisplay.ViewportRect.Width / 2 ),
                  _viewFinderDisplay.ViewportRect.Top );
              _resizeAnchorPoint = new Point( _viewFinderDisplay.ViewportRect.Left + ( _viewFinderDisplay.ViewportRect.Width / 2 ),
                  _viewFinderDisplay.ViewportRect.Bottom );
              direction = new Vector( 0, -1 );
              break;
            case ResizeEdge.Right:
              _resizeDraggingPoint = new Point( _viewFinderDisplay.ViewportRect.Right,
                  _viewFinderDisplay.ViewportRect.Top + ( _viewFinderDisplay.ViewportRect.Height / 2 ) );
              _resizeAnchorPoint = new Point( _viewFinderDisplay.ViewportRect.Left,
                  _viewFinderDisplay.ViewportRect.Top + ( _viewFinderDisplay.ViewportRect.Height / 2 ) );
              direction = new Vector( 1, 0 );
              break;
            case ResizeEdge.Bottom:
              _resizeDraggingPoint = new Point( _viewFinderDisplay.ViewportRect.Left + ( _viewFinderDisplay.ViewportRect.Width / 2 ),
                  _viewFinderDisplay.ViewportRect.Bottom );
              _resizeAnchorPoint = new Point( _viewFinderDisplay.ViewportRect.Left + ( _viewFinderDisplay.ViewportRect.Width / 2 ),
                  _viewFinderDisplay.ViewportRect.Top );
              direction = new Vector( 0, 1 );
              break;
          }
          double scale = _viewFinderDisplay.Scale;
          Rect contentRect = _viewFinderDisplay.ContentBounds;
          Vector minVector = new Vector( direction.X * ARBITRARY_LARGE_VALUE, direction.Y * ARBITRARY_LARGE_VALUE );
          Vector maxVector = new Vector( direction.X * contentRect.Width / MaxScale, direction.Y * contentRect.Height / MaxScale );
          _resizeViewportBounds = new Rect( _resizeAnchorPoint + minVector, _resizeAnchorPoint + maxVector );
        }

        // store the origin of the operation and acquire capture
        _originPoint = e.GetPosition( _viewFinderDisplay );
        _viewFinderDisplay.CaptureMouse();
        e.Handled = true;
      }
    }

    private void ViewFinderDisplayEndCapture( object sender, MouseButtonEventArgs e )
    {
      // if a drag or resize is in progress, end it and release capture
      if( this.IsDraggingViewport || this.IsResizingViewport )
      {
        // call the DragDisplayViewport method to end the operation
        // and store the current position on the stack
        this.DragDisplayViewport( new DragDeltaEventArgs( 0, 0 ), true );

        // reset the dragging state variables and release capture
        this.IsDraggingViewport = false;
        this.IsResizingViewport = false;
        _originPoint = new Point();
        _viewFinderDisplay.ReleaseMouseCapture();
        e.Handled = true;
      }
    }

    private void ViewFinderDisplayMouseMove( object sender, MouseEventArgs e )
    {
      // if a drag operation is in progress, update the operation
      if( e.MouseDevice.LeftButton == MouseButtonState.Pressed
          && ( this.IsDraggingViewport || this.IsResizingViewport ) )
      {
        Point pos = e.GetPosition( _viewFinderDisplay );
        Vector delta = pos - _originPoint;
        if( this.IsDraggingViewport )
        {
          this.DragDisplayViewport( new DragDeltaEventArgs( delta.X, delta.Y ), false );
        }
        else
        {
          this.ResizeDisplayViewport( new DragDeltaEventArgs( delta.X, delta.Y ), ( ResizeEdge )_viewFinderDisplay.Tag );
        }
        e.Handled = true;
      }
      else
      {
        // update the cursor based on the nearest corner
        Point mousePos = e.GetPosition( _viewFinderDisplay );
        Rect viewportRect = _viewFinderDisplay.ViewportRect;
        double cornerDelta = viewportRect.Width * viewportRect.Height > 100 ? 5.0
            : Math.Sqrt( viewportRect.Width * viewportRect.Height ) / 2;

        // if the mouse is within the Rect and the Rect does not encompass the entire content, set the appropriate cursor
        if( viewportRect.Contains( mousePos )
            && !DoubleHelper.AreVirtuallyEqual( Rect.Intersect( viewportRect, _viewFinderDisplay.ContentBounds ), _viewFinderDisplay.ContentBounds ) )
        {
          if( PointHelper.DistanceBetween( mousePos, viewportRect.TopLeft ) < cornerDelta )
          {
            _viewFinderDisplay.Tag = ResizeEdge.TopLeft;
            _viewFinderDisplay.Cursor = Cursors.SizeNWSE;
          }
          else if( PointHelper.DistanceBetween( mousePos, viewportRect.BottomRight ) < cornerDelta )
          {
            _viewFinderDisplay.Tag = ResizeEdge.BottomRight;
            _viewFinderDisplay.Cursor = Cursors.SizeNWSE;
          }
          else if( PointHelper.DistanceBetween( mousePos, viewportRect.TopRight ) < cornerDelta )
          {
            _viewFinderDisplay.Tag = ResizeEdge.TopRight;
            _viewFinderDisplay.Cursor = Cursors.SizeNESW;
          }
          else if( PointHelper.DistanceBetween( mousePos, viewportRect.BottomLeft ) < cornerDelta )
          {
            _viewFinderDisplay.Tag = ResizeEdge.BottomLeft;
            _viewFinderDisplay.Cursor = Cursors.SizeNESW;
          }
          else if( mousePos.X <= viewportRect.Left + cornerDelta )
          {
            _viewFinderDisplay.Tag = ResizeEdge.Left;
            _viewFinderDisplay.Cursor = Cursors.SizeWE;
          }
          else if( mousePos.Y <= viewportRect.Top + cornerDelta )
          {
            _viewFinderDisplay.Tag = ResizeEdge.Top;
            _viewFinderDisplay.Cursor = Cursors.SizeNS;
          }
          else if( mousePos.X >= viewportRect.Right - cornerDelta )
          {
            _viewFinderDisplay.Tag = ResizeEdge.Right;
            _viewFinderDisplay.Cursor = Cursors.SizeWE;
          }
          else if( mousePos.Y >= viewportRect.Bottom - cornerDelta )
          {
            _viewFinderDisplay.Tag = ResizeEdge.Bottom;
            _viewFinderDisplay.Cursor = Cursors.SizeNS;
          }
          else
          {
            _viewFinderDisplay.Tag = ResizeEdge.None;
            _viewFinderDisplay.Cursor = Cursors.SizeAll;
          }
        }
        else
        {
          _viewFinderDisplay.Tag = null;
          _viewFinderDisplay.Cursor = Cursors.Arrow;
        }
      }
    }

    private void ZoomAnimationCompleted( object sender, EventArgs e )
    {
      if( ( sender as AnimationClock ).CurrentState != ClockState.Active )
      {
        // remove the event handlers
        ( sender as AnimationClock ).CurrentStateInvalidated -= new EventHandler( this.ZoomAnimationCompleted );
        ( sender as AnimationClock ).CurrentTimeInvalidated -= new EventHandler( this.UpdateViewport );

        // raise animation completed event
        this.RaiseEvent( new RoutedEventArgs( Zoombox.AnimationCompletedEvent, this ) );
      }
    }

    private void VerticalValueAnimation_Completed( object sender, EventArgs e )
    {
      //When the animaton is completed, with the FillBehavior to HoldEnd, 
      //the ScrollBarValue will be overriden with the final animation value, preventing future scroll.
      //To remove it use BeginAnimation with null.
      //http://msdn.microsoft.com/en-us/library/aa970493(v=vs.110).aspx      

      // only do this when all the overlapped animations are done or limits values reached.
      if( ( _verticalScrollBar.Value == -_relativePosition.Y )
        || ( _verticalScrollBar.Value == _verticalScrollBar.Maximum )
        || ( _verticalScrollBar.Value == _verticalScrollBar.Minimum ) )
      {
        var finalValue = _verticalScrollBar.Value;
        //this will reset Value to original value of animation
        _verticalScrollBar.BeginAnimation( ScrollBar.ValueProperty, null );
        //this will set to last value of the animation.
        _verticalScrollBar.Value = finalValue;
      }
    }

    private void HorizontalValueAnimation_Completed( object sender, EventArgs e )
    {
      //When the animaton is completed, with the FillBehavior to HoldEnd, 
      //the ScrollBarValue will be overriden with the final animation value, preventing future scroll.
      //To remove it use BeginAnimation with null.
      //http://msdn.microsoft.com/en-us/library/aa970493(v=vs.110).aspx      

      // only do this when all the overlapped animations are done or limits values reached.
      if( ( _horizontalScrollBar.Value == -_relativePosition.X )
        || ( _horizontalScrollBar.Value == _horizontalScrollBar.Maximum )
        || ( _horizontalScrollBar.Value == _horizontalScrollBar.Minimum ) )
      {
        var finalValue = _horizontalScrollBar.Value;
        //this will reset Value to original value of animation
        _horizontalScrollBar.BeginAnimation( ScrollBar.ValueProperty, null );
        //this will set to last value of the animation.
        _horizontalScrollBar.Value = finalValue;
      }
    }

    private void ZoomTo( double scale, bool allowStackAddition )
    {
      // if there is nothing to scale, just return
      if( _content == null )
        return;

      // adjust the current scale relative to the zoom origin
      this.ZoomTo( scale, this.GetZoomRelativePoint(), true, allowStackAddition );
    }

    private void ZoomTo( double scale, Point relativeTo, bool restrictRelativePointToContent, bool allowStackAddition )
    {
      // if there is nothing to scale, just return
      if( _content == null )
        return;

      if( double.IsNaN( scale ) )
        return;

      // if necessary, verify that the relativeTo point falls within the content
      if( restrictRelativePointToContent && !( new Rect( _content.RenderSize ) ).Contains( relativeTo ) )
        return;

      // ensure that the scale value falls within the valid range
      if( scale > this.MaxScale )
      {
        scale = this.MaxScale;
      }
      else if( scale < this.MinScale )
      {
        scale = this.MinScale;
      }

      // internally, updates are always relative to the Zoombox control
      Point translateFrom = relativeTo;
      if( this.HasRenderedFirstView )
      {
        // Note that this TranslatePoint approach will not work until the first render occurs
        relativeTo = _content.TranslatePoint( relativeTo, this );

        // adjust translateFrom based on relativeTo
        translateFrom = this.TranslatePoint( relativeTo, _contentPresenter );
      }
      else if( _contentPresenter != null )
      {
        // prior to the first render, just use the ContentPresenter's transform and do not adjust translateFrom
        if( _contentPresenter.RenderTransform == Transform.Identity )
        {
          // in order for this approach to work, we must at least make one pass to update a generic view
          // with Scale = 1.0 and Position = 0,0
          this.UpdateView( new ZoomboxView( 1, new Point( 0, 0 ) ), false, false );
        }

        // now there should be a valid RenderTransform
        relativeTo = ( _contentPresenter.RenderTransform as Transform ).Transform( relativeTo );
      }

      // determine the new content position for this zoom operation
      Point translateTo = new Point( relativeTo.X - ( translateFrom.X * scale / _viewboxFactor ),
          relativeTo.Y - ( translateFrom.Y * scale / _viewboxFactor ) )
          + this.ContentOffset * scale / _viewboxFactor;
      this.UpdateView( new ZoomboxView( scale, translateTo ), !this.IsResizingViewport, allowStackAddition );
    }

    private Point GetZoomRelativePoint()
    {
      Point zoomPoint;

      if( ZoomOn == ZoomboxZoomOn.View )
      {
        // Transform the viewport point to the content
        Point viewportZoomOrigin = new Point();

        viewportZoomOrigin.X = this.Viewport.X + ( this.Viewport.Width * this.ZoomOrigin.X );
        viewportZoomOrigin.Y = this.Viewport.Y + ( this.Viewport.Height * this.ZoomOrigin.Y );

        Point contentZoomOrigin = _trueContent.TranslatePoint( viewportZoomOrigin, _content );

        if( contentZoomOrigin.X < 0 )
        {
          contentZoomOrigin.X = 0;
        }
        else if( contentZoomOrigin.X > _content.RenderSize.Width )
        {
          contentZoomOrigin.X = _content.RenderSize.Width;
        }

        if( contentZoomOrigin.Y < 0 )
        {
          contentZoomOrigin.Y = 0;
        }
        else if( contentZoomOrigin.Y > _content.RenderSize.Height )
        {
          contentZoomOrigin.Y = _content.RenderSize.Height;
        }

        zoomPoint = contentZoomOrigin;
      }
      else
      {
        zoomPoint = new Point( _content.RenderSize.Width * ZoomOrigin.X, _content.RenderSize.Height * ZoomOrigin.Y );
      }

      return zoomPoint;
    }

    #region OnKeyDown Methods

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      if( this.NavigateOnPreview && !e.Handled )
      {
        this.ProcessNavigationButton( e );
      }

      base.OnPreviewKeyDown( e );
    }

    protected override void OnKeyDown( KeyEventArgs e )
    {
      if( !this.NavigateOnPreview && !e.Handled )
      {
        this.ProcessNavigationButton( e );
      }

      base.OnKeyDown( e );
    }

    #endregion

    #region OnMouseDown Methods

    protected override void OnPreviewMouseDown( MouseButtonEventArgs e )
    {
      if( this.NavigateOnPreview && !e.Handled )
      {
        this.ProcessNavigationButton( e );
      }

      base.OnPreviewMouseDown( e );
    }

    protected override void OnMouseDown( MouseButtonEventArgs e )
    {
      if( !this.NavigateOnPreview && !e.Handled )
      {
        this.ProcessNavigationButton( e );
      }

      base.OnMouseDown( e );
    }

    #endregion

    #region OnMouseEnter Methods

    protected override void OnMouseEnter( MouseEventArgs e )
    {
      this.MonitorInput();

      base.OnMouseEnter( e );
    }

    #endregion

    #region OnMouseLeave Methods

    protected override void OnMouseLeave( MouseEventArgs e )
    {
      this.MonitorInput();

      base.OnMouseLeave( e );
    }

    #endregion

    #region OnMouseLeftButton Methods

    protected override void OnPreviewMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      if( this.DragOnPreview && !e.Handled && _contentPresenter != null )
      {
        this.ProcessMouseLeftButtonDown( e );
      }

      base.OnPreviewMouseLeftButtonDown( e );
    }

    protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      if( !this.DragOnPreview && !e.Handled && _contentPresenter != null )
      {
        this.ProcessMouseLeftButtonDown( e );
      }

      base.OnMouseLeftButtonDown( e );
    }

    protected override void OnPreviewMouseLeftButtonUp( MouseButtonEventArgs e )
    {
      if( this.DragOnPreview && !e.Handled && _contentPresenter != null )
      {
        this.ProcessMouseLeftButtonUp( e );
      }

      base.OnPreviewMouseLeftButtonUp( e );
    }

    protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
    {
      if( !this.DragOnPreview && !e.Handled && _contentPresenter != null )
      {
        this.ProcessMouseLeftButtonUp( e );
      }

      base.OnMouseLeftButtonUp( e );
    }

    #endregion

    #region OnMouseMove Methods

    protected override void OnPreviewMouseMove( MouseEventArgs e )
    {
      if( this.DragOnPreview && !e.Handled && _contentPresenter != null )
      {
        this.ProcessMouseMove( e );
      }

      base.OnPreviewMouseMove( e );
    }

    protected override void OnMouseMove( MouseEventArgs e )
    {
      if( !this.DragOnPreview && !e.Handled && _contentPresenter != null )
      {
        this.ProcessMouseMove( e );
      }

      base.OnMouseMove( e );
    }

    #endregion

    #region OnMouseWheel Methods

    protected override void OnPreviewMouseWheel( MouseWheelEventArgs e )
    {
      if( this.ZoomOnPreview && !e.Handled && _contentPresenter != null )
      {
        this.ProcessMouseWheelZoom( e );
      }

      base.OnPreviewMouseWheel( e );
    }

    protected override void OnMouseWheel( MouseWheelEventArgs e )
    {
      if( !this.ZoomOnPreview && !e.Handled && _contentPresenter != null )
      {
        this.ProcessMouseWheelZoom( e );
      }

      base.OnMouseWheel( e );
    }

    #endregion

    #region Private Fields

    // the default value for a single mouse wheel delta appears to be 28
    private static int MOUSE_WHEEL_DELTA = 28;

    // the content control's one and only content presenter
    private ContentPresenter _contentPresenter = null;

    //The Scrollbars
    private ScrollBar _verticalScrollBar = null;
    private ScrollBar _horizontalScrollBar = null;

    // the content of the Zoombox (cast as a UIElement)
    private UIElement _content = null;

    // the drag adorner used for selecting a region in a zoom-to-selection operation
    private DragAdorner _dragAdorner = null;

    // the view stack
    private ZoomboxViewStack _viewStack = null;

    // the view finder display panel
    // this is used to show the current viewport
    private ZoomboxViewFinderDisplay _viewFinderDisplay = null;

    // state variables used during drag and select operations
    private Rect _resizeViewportBounds = Rect.Empty;
    private Point _resizeAnchorPoint = new Point( 0, 0 );
    private Point _resizeDraggingPoint = new Point( 0, 0 );
    private Point _originPoint = new Point( 0, 0 );

    private double _viewboxFactor = 1.0;
    private double _relativeScale = 1.0;
    private Point _relativePosition = new Point();
    private Point _basePosition = new Point();

    // used to track the time delta between stack operations
    private DateTime _lastStackAddition;

    // used to provide stack index when view changes
    private int _lastViewIndex = -1;

    private BitVector32 _cacheBits = new BitVector32( 0 );

    #endregion

    #region ViewFinderSelectionConverter Nested Type

    private sealed class ViewFinderSelectionConverter : IValueConverter
    {
      public ViewFinderSelectionConverter( Zoombox zoombox )
      {
        _zoombox = zoombox;
      }

      public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
      {
        Rect viewport = ( Rect )value;
        if( viewport.IsEmpty )
          return viewport;

        // adjust the viewport from the coordinate space of the Content element
        // to the coordinate space of the view finder display panel
        double scale = _zoombox._viewFinderDisplay.Scale * _zoombox._viewboxFactor;
        Rect result = new Rect( viewport.Left * scale, viewport.Top * scale,
            viewport.Width * scale, viewport.Height * scale );
        result.Offset( _zoombox._viewFinderDisplay.ContentBounds.Left,
            _zoombox._viewFinderDisplay.ContentBounds.Top );
        return result;
      }

      public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
      {
        return null;
      }

      private readonly Zoombox _zoombox;
    }

    #endregion

    #region DragAdorner Nested Type

    internal sealed class DragAdorner : Adorner
    {
      public DragAdorner( UIElement adornedElement )
        : base( adornedElement )
      {
        this.ClipToBounds = true;
      }

      public static readonly DependencyProperty BrushProperty =
        DependencyProperty.Register( "Brush", typeof( Brush ), typeof( DragAdorner ),
          new FrameworkPropertyMetadata( Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender ) );

      public Brush Brush
      {
        get
        {
          return ( Brush )this.GetValue( DragAdorner.BrushProperty );
        }
        set
        {
          this.SetValue( DragAdorner.BrushProperty, value );
        }
      }

      public static readonly DependencyProperty PenProperty =
        DependencyProperty.Register( "Pen", typeof( Pen ), typeof( DragAdorner ),
          new FrameworkPropertyMetadata( new Pen( new SolidColorBrush( Color.FromArgb( 0x7F, 0x3F, 0x3F, 0x3F ) ), 2d ), FrameworkPropertyMetadataOptions.AffectsRender ) );

      public Pen Pen
      {
        get
        {
          return ( Pen )this.GetValue( DragAdorner.PenProperty );
        }
        set
        {
          this.SetValue( DragAdorner.PenProperty, value );
        }
      }

      public static readonly DependencyProperty RectProperty =
        DependencyProperty.Register( "Rect", typeof( Rect ), typeof( DragAdorner ),
          new FrameworkPropertyMetadata( Rect.Empty, FrameworkPropertyMetadataOptions.AffectsRender,
            new PropertyChangedCallback( DragAdorner.OnRectChanged ) ) );

      public Rect Rect
      {
        get
        {
          return ( Rect )this.GetValue( DragAdorner.RectProperty );
        }
        set
        {
          this.SetValue( DragAdorner.RectProperty, value );
        }
      }

      private static void OnRectChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
      {
        DragAdorner dragAdorner = ( DragAdorner )d;
        Rect rect = ( Rect )e.NewValue;

        // ignore empty values
        if( rect.IsEmpty )
          return;

        // if the value is not empty, cache the position and size
        dragAdorner._cachedPosition = ( ( Rect )e.NewValue ).TopLeft;
        dragAdorner._cachedSize = ( ( Rect )e.NewValue ).Size;
      }

      public Point LastPosition
      {
        get
        {
          return _cachedPosition;
        }
      }

      public Size LastSize
      {
        get
        {
          return _cachedSize;
        }
      }

      protected override void OnRender( DrawingContext drawingContext )
      {
        drawingContext.DrawRectangle( Brush, Pen, Rect );
      }

      private Point _cachedPosition;
      private Size _cachedSize;
    }

    #endregion

    #region CacheBits Nested Type

    private enum CacheBits
    {
      IsUpdatingView = 0x00000001,
      IsUpdatingViewport = 0x00000002,
      IsDraggingViewport = 0x00000004,
      IsResizingViewport = 0x00000008,
      IsMonitoringInput = 0x00000010,
      IsContentWrapped = 0x00000020,
      HasArrangedContentPresenter = 0x00000040,
      HasRenderedFirstView = 0x00000080,
      RefocusViewOnFirstRender = 0x00000100,
      HasUIPermission = 0x00000200,
    }

    #endregion

    #region ResizeEdge Nested Type

    private enum ResizeEdge
    {
      None,
      TopLeft,
      TopRight,
      BottomLeft,
      BottomRight,
      Left,
      Top,
      Right,
      Bottom,
    }

    #endregion
  }
}
