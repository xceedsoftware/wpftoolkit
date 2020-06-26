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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Media.Animation;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Xceed.Wpf.Toolkit.Core;

namespace Xceed.Wpf.Toolkit.Panels
{
  public abstract class AnimationPanel : PanelBase
  {
    #region Constructors

    public AnimationPanel()
    {
#if DEBUG
      Type derivedType = GetType();

      FieldInfo[] fields = derivedType.GetFields( BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );

      foreach( FieldInfo field in fields )
      {
        if( field.FieldType == typeof( DependencyProperty ) )
        {
          DependencyProperty prop = ( DependencyProperty )field.GetValue( null );
          PropertyMetadata metaData = prop.GetMetadata( this );

          if( metaData is FrameworkPropertyMetadata )
          {
            FrameworkPropertyMetadata frameworkData = ( FrameworkPropertyMetadata )metaData;

            if( frameworkData.AffectsArrange == true || frameworkData.AffectsMeasure == true ||
                frameworkData.AffectsParentArrange == true || frameworkData.AffectsParentMeasure == true )
            {
              System.Console.WriteLine( "AnimationPanel: " + derivedType.Name + "." + field.Name +
                  " - You should not set dependency property metadata flags that " +
                  "affect measure or arrange, instead call AnimationPanel's InvalidateMeasure or " +
                  "InvalidateArrange directly." );
            }
          }
        }
      }
#endif

      this.Loaded += new RoutedEventHandler( this.OnLoaded );
    }

    #endregion

    #region ChildState Private Property

    private static readonly DependencyPropertyKey ChildStatePropertyKey =
      DependencyProperty.RegisterAttachedReadOnly( "ChildState", typeof( ChildState ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( null ) );

    private static ChildState GetChildState( DependencyObject d )
    {
      return ( ChildState )d.GetValue( AnimationPanel.ChildStatePropertyKey.DependencyProperty );
    }

    private static void SetChildState( DependencyObject d, ChildState value )
    {
      d.SetValue( AnimationPanel.ChildStatePropertyKey, value );
    }

    #endregion

    #region DefaultAnimationRate Property

    public static readonly DependencyProperty DefaultAnimationRateProperty =
      DependencyProperty.Register( "DefaultAnimationRate", typeof( AnimationRate ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( ( AnimationRate )1d ),
          new ValidateValueCallback( AnimationPanel.ValidateDefaultAnimationRate ) );

    public AnimationRate DefaultAnimationRate
    {
      get
      {
        return ( AnimationRate )this.GetValue( AnimationPanel.DefaultAnimationRateProperty );
      }
      set
      {
        this.SetValue( AnimationPanel.DefaultAnimationRateProperty, value );
      }
    }

    private static bool ValidateDefaultAnimationRate( object value )
    {
      if( ( AnimationRate )value == AnimationRate.Default )
        throw new ArgumentException( ErrorMessages.GetMessage( ErrorMessages.DefaultAnimationRateAnimationRateDefault ) );

      return true;
    }

    #endregion

    #region DefaultAnimator Property

    public static readonly DependencyProperty DefaultAnimatorProperty =
      DependencyProperty.Register( "DefaultAnimator", typeof( IterativeAnimator ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( Animators.Linear ),
          new ValidateValueCallback( AnimationPanel.ValidateDefaultAnimator ) );

    public IterativeAnimator DefaultAnimator
    {
      get
      {
        return ( IterativeAnimator )this.GetValue( AnimationPanel.DefaultAnimatorProperty );
      }
      set
      {
        this.SetValue( AnimationPanel.DefaultAnimatorProperty, value );
      }
    }

    private static bool ValidateDefaultAnimator( object value )
    {
      if( value == IterativeAnimator.Default )
        throw new ArgumentException( ErrorMessages.GetMessage( ErrorMessages.DefaultAnimatorIterativeAnimationDefault ) );

      return true;
    }

    #endregion

    #region EnterAnimationRate Property

    public static readonly DependencyProperty EnterAnimationRateProperty =
      DependencyProperty.Register( "EnterAnimationRate", typeof( AnimationRate ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( AnimationRate.Default ) );

    public AnimationRate EnterAnimationRate
    {
      get
      {
        return ( AnimationRate )this.GetValue( AnimationPanel.EnterAnimationRateProperty );
      }
      set
      {
        this.SetValue( AnimationPanel.EnterAnimationRateProperty, value );
      }
    }

    #endregion

    #region EnterAnimator Property

    public static readonly DependencyProperty EnterAnimatorProperty =
      DependencyProperty.Register( "EnterAnimator", typeof( IterativeAnimator ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( IterativeAnimator.Default ) );

    public IterativeAnimator EnterAnimator
    {
      get
      {
        return ( IterativeAnimator )this.GetValue( AnimationPanel.EnterAnimatorProperty );
      }
      set
      {
        this.SetValue( AnimationPanel.EnterAnimatorProperty, value );
      }
    }

    #endregion

    #region EnterFrom Attached Property

    public static readonly DependencyProperty EnterFromProperty =
      DependencyProperty.RegisterAttached( "EnterFrom", typeof( Rect? ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( ( Rect? )null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static Rect? GetEnterFrom( DependencyObject d )
    {
      return ( Rect? )d.GetValue( AnimationPanel.EnterFromProperty );
    }

    public static void SetEnterFrom( DependencyObject d, Rect? value )
    {
      d.SetValue( AnimationPanel.EnterFromProperty, value );
    }

    #endregion

    #region ExitAnimationRate Property

    public static readonly DependencyProperty ExitAnimationRateProperty =
      DependencyProperty.Register( "ExitAnimationRate", typeof( AnimationRate ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( AnimationRate.Default ) );

    public AnimationRate ExitAnimationRate
    {
      get
      {
        return ( AnimationRate )this.GetValue( AnimationPanel.ExitAnimationRateProperty );
      }
      set
      {
        this.SetValue( AnimationPanel.ExitAnimationRateProperty, value );
      }
    }

    #endregion

    #region ExitAnimator Property

    public static readonly DependencyProperty ExitAnimatorProperty =
      DependencyProperty.Register( "ExitAnimator", typeof( IterativeAnimator ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( IterativeAnimator.Default ) );

    public IterativeAnimator ExitAnimator
    {
      get
      {
        return ( IterativeAnimator )this.GetValue( AnimationPanel.ExitAnimatorProperty );
      }
      set
      {
        this.SetValue( AnimationPanel.ExitAnimatorProperty, value );
      }
    }

    #endregion

    #region ExitTo Attached Property

    public static readonly DependencyProperty ExitToProperty =
      DependencyProperty.RegisterAttached( "ExitTo", typeof( Rect? ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( ( Rect? )null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static Rect? GetExitTo( DependencyObject d )
    {
      return ( Rect? )d.GetValue( AnimationPanel.ExitToProperty );
    }

    public static void SetExitTo( DependencyObject d, Rect? value )
    {
      d.SetValue( AnimationPanel.ExitToProperty, value );
    }

    #endregion

    #region LayoutAnimationRate Property

    public static readonly DependencyProperty LayoutAnimationRateProperty =
      DependencyProperty.Register( "LayoutAnimationRate", typeof( AnimationRate ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( AnimationRate.Default ) );

    public AnimationRate LayoutAnimationRate
    {
      get
      {
        return ( AnimationRate )this.GetValue( AnimationPanel.LayoutAnimationRateProperty );
      }
      set
      {
        this.SetValue( AnimationPanel.LayoutAnimationRateProperty, value );
      }
    }

    #endregion

    #region LayoutAnimator Property

    public static readonly DependencyProperty LayoutAnimatorProperty =
      DependencyProperty.Register( "LayoutAnimator", typeof( IterativeAnimator ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( IterativeAnimator.Default ) );

    public IterativeAnimator LayoutAnimator
    {
      get
      {
        return ( IterativeAnimator )this.GetValue( AnimationPanel.LayoutAnimatorProperty );
      }
      set
      {
        this.SetValue( AnimationPanel.LayoutAnimatorProperty, value );
      }
    }

    #endregion

    #region SwitchAnimationRate Property

    public static readonly DependencyProperty SwitchAnimationRateProperty =
      DependencyProperty.Register( "SwitchAnimationRate", typeof( AnimationRate ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( AnimationRate.Default ) );

    public AnimationRate SwitchAnimationRate
    {
      get
      {
        return ( AnimationRate )this.GetValue( AnimationPanel.SwitchAnimationRateProperty );
      }
      set
      {
        this.SetValue( AnimationPanel.SwitchAnimationRateProperty, value );
      }
    }

    #endregion

    #region SwitchAnimator Property

    public static readonly DependencyProperty SwitchAnimatorProperty =
      DependencyProperty.Register( "SwitchAnimator", typeof( IterativeAnimator ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( IterativeAnimator.Default ) );

    public IterativeAnimator SwitchAnimator
    {
      get
      {
        return ( IterativeAnimator )this.GetValue( AnimationPanel.SwitchAnimatorProperty );
      }
      set
      {
        this.SetValue( AnimationPanel.SwitchAnimatorProperty, value );
      }
    }

    #endregion

    #region SwitchParent Property

    private static readonly DependencyPropertyKey SwitchParentPropertyKey =
      DependencyProperty.RegisterReadOnly( "SwitchParent", typeof( SwitchPanel ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( null,
          new PropertyChangedCallback( AnimationPanel.OnSwitchParentChanged ) ) );

    public static readonly DependencyProperty SwitchParentProperty = AnimationPanel.SwitchParentPropertyKey.DependencyProperty;

    public SwitchPanel SwitchParent
    {
      get
      {
        return ( SwitchPanel )this.GetValue( AnimationPanel.SwitchParentProperty );
      }
    }

    protected internal void SetSwitchParent( SwitchPanel value )
    {
      this.SetValue( AnimationPanel.SwitchParentPropertyKey, value );
    }

    private static void OnSwitchParentChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( AnimationPanel )d ).OnSwitchParentChanged( e );
    }

    protected virtual void OnSwitchParentChanged( DependencyPropertyChangedEventArgs e )
    {
      _switchParent = e.NewValue as SwitchPanel;
    }

    #endregion

    #region SwitchTemplate Property

    public static readonly DependencyProperty SwitchTemplateProperty =
      DependencyProperty.Register( "SwitchTemplate", typeof( DataTemplate ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( null,
          new PropertyChangedCallback( AnimationPanel.OnSwitchTemplateChanged ) ) );

    public DataTemplate SwitchTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( AnimationPanel.SwitchTemplateProperty );
      }
      set
      {
        this.SetValue( AnimationPanel.SwitchTemplateProperty, value );
      }
    }

    private static void OnSwitchTemplateChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( AnimationPanel )d ).OnSwitchTemplateChanged( e );
    }

    protected virtual void OnSwitchTemplateChanged( DependencyPropertyChangedEventArgs e )
    {
      if( _switchParent != null && _switchParent.ActiveLayout == this )
      {
        _switchParent.UpdateSwitchTemplate();
      }
    }

    #endregion

    #region TemplateAnimationRate Property

    public static readonly DependencyProperty TemplateAnimationRateProperty =
      DependencyProperty.Register( "TemplateAnimationRate", typeof( AnimationRate ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( AnimationRate.Default ) );

    public AnimationRate TemplateAnimationRate
    {
      get
      {
        return ( AnimationRate )this.GetValue( AnimationPanel.TemplateAnimationRateProperty );
      }
      set
      {
        this.SetValue( AnimationPanel.TemplateAnimationRateProperty, value );
      }
    }

    #endregion

    #region TemplateAnimator Property

    public static readonly DependencyProperty TemplateAnimatorProperty =
      DependencyProperty.Register( "TemplateAnimator", typeof( IterativeAnimator ), typeof( AnimationPanel ),
        new FrameworkPropertyMetadata( IterativeAnimator.Default ) );

    public IterativeAnimator TemplateAnimator
    {
      get
      {
        return ( IterativeAnimator )this.GetValue( AnimationPanel.TemplateAnimatorProperty );
      }
      set
      {
        this.SetValue( AnimationPanel.TemplateAnimatorProperty, value );
      }
    }

    #endregion

    #region DesiredSize Property

    public new Size DesiredSize
    {
      get
      {
        return ( _switchParent != null ) ? _switchParent.DesiredSize : base.DesiredSize;
      }
    }

    #endregion

    #region RenderSize Property

    public new Size RenderSize
    {
      get
      {
        return ( _switchParent != null ) ? _switchParent.RenderSize : base.RenderSize;
      }
      set
      {
        base.RenderSize = value;
      }
    }

    #endregion

    #region IsActiveLayout Property

    public bool IsActiveLayout
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsActiveLayout ];
      }
      private set
      {
        _cacheBits[ ( int )CacheBits.IsActiveLayout ] = value;
      }
    }

    #endregion

    #region InternalChildren Protected Property

    protected internal new UIElementCollection InternalChildren
    {
      get
      {
        return ( _switchParent == null ) ? base.InternalChildren : _switchParent.ChildrenInternal;
      }
    }

    #endregion

    #region VisualChildrenCount Protected Property

    protected override int VisualChildrenCount
    {
      get
      {
        return this.HasLoaded ? this.InternalChildren.Count + this.ExitingChildren.Count : base.VisualChildrenCount;
      }
    }

    #endregion

    #region ChildrensParent Protected Property

    protected PanelBase ChildrensParent
    {
      get
      {
        return _switchParent != null ? ( PanelBase )_switchParent : this;
      }
    }

    #endregion

    #region VisualChildrenCountInternal Internal Property

    internal int VisualChildrenCountInternal
    {
      get
      {
        return this.VisualChildrenCount;
      }
    }

    #endregion

    #region PhysicalScrollOffset Internal Property

    internal Vector PhysicalScrollOffset
    {
      get
      {
        return _physicalScrollOffset;
      }
      set
      {
        _physicalScrollOffset = value;
      }
    }

    private Vector _physicalScrollOffset = new Vector();

    #endregion

    #region HasLoaded Internal Property

    internal bool IsRemovingInternalChild
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsRemovingInternalChild ];
      }
      private set
      {
        _cacheBits[ ( int )CacheBits.IsRemovingInternalChild ] = value;
      }
    }

    #endregion

    #region AnimatingChildCount Private Property

    private int AnimatingChildCount
    {
      get
      {
        return _animatingChildCount;
      }
      set
      {
        // start the animation pump if the value goes positive
        if( _animatingChildCount == 0 && value > 0 )
        {
          CompositionTarget.Rendering += new EventHandler( OnRendering );
          RaiseAnimationBegunEvent();
        }

        // stop the animation pump if the value goes to 0
        if( _animatingChildCount != 0 && value == 0 )
        {
          if( EndSwitchOnAnimationCompleted && _switchParent != null )
          {
            EndSwitchOnAnimationCompleted = false;
            _switchParent.EndLayoutSwitch();
          }

          CompositionTarget.Rendering -= new EventHandler( OnRendering );
          RaiseAnimationCompletedEvent();
        }

        _animatingChildCount = value;
      }
    }

    private int _animatingChildCount;

    #endregion

    #region EndSwitchOnAnimationCompleted Private Property

    private bool EndSwitchOnAnimationCompleted
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.EndSwitchOnAnimationCompleted ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.EndSwitchOnAnimationCompleted ] = value;
      }
    }

    #endregion

    #region HasArranged Private Property

    private bool HasArranged
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.HasArranged ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.HasArranged ] = value;
      }
    }

    #endregion

    #region HasLoaded Private Property

    protected bool HasLoaded
    {
      get
      {
        return _switchParent == null ? _cacheBits[ ( int )CacheBits.HasLoaded ] : _switchParent.HasLoaded;
      }
      private set
      {
        _cacheBits[ ( int )CacheBits.HasLoaded ] = value;
      }
    }

    #endregion

    #region IsSwitchInProgress Private Property

    private bool IsSwitchInProgress
    {
      get
      {
        return _cacheBits[ ( int )CacheBits.IsSwitchInProgress ];
      }
      set
      {
        _cacheBits[ ( int )CacheBits.IsSwitchInProgress ] = value;
      }
    }

    #endregion

    #region ItemsOwner Private Property

    private ItemsControl ItemsOwner
    {
      get
      {
        return ItemsControl.GetItemsOwner( _switchParent == null ? ( Panel )this : ( Panel )_switchParent );
      }
    }

    #endregion

    #region ExitingChildren Private Property

    private List<UIElement> ExitingChildren
    {
      get
      {
        if( _switchParent != null )
          return _switchParent.ExitingChildren;

        if( _exitingChildren == null )
        {
          _exitingChildren = new List<UIElement>();
        }

        return _exitingChildren;
      }
    }

    #endregion

    #region AnimationBegun Event

    public static readonly RoutedEvent AnimationBegunEvent =
      EventManager.RegisterRoutedEvent( "AnimationBegun", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( AnimationPanel ) );

    public event RoutedEventHandler AnimationBegun
    {
      add
      {
        this.AddHandler( AnimationPanel.AnimationBegunEvent, value );
      }
      remove
      {
        this.RemoveHandler( AnimationPanel.AnimationBegunEvent, value );
      }
    }

    protected RoutedEventArgs RaiseAnimationBegunEvent()
    {
      return AnimationPanel.RaiseAnimationBegunEvent( ( this._switchParent != null ) ? ( UIElement )this._switchParent : ( UIElement )this );
    }

    private static RoutedEventArgs RaiseAnimationBegunEvent( UIElement target )
    {
      if( target == null )
        return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = AnimationPanel.AnimationBegunEvent;
      RoutedEventHelper.RaiseEvent( target, args );
      return args;
    }

    #endregion

    #region AnimationCompleted Event

    public static readonly RoutedEvent AnimationCompletedEvent =
      EventManager.RegisterRoutedEvent( "AnimationCompleted", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( AnimationPanel ) );

    public event RoutedEventHandler AnimationCompleted
    {
      add
      {
        this.AddHandler( AnimationPanel.AnimationCompletedEvent, value );
      }
      remove
      {
        this.RemoveHandler( AnimationPanel.AnimationCompletedEvent, value );
      }
    }

    protected RoutedEventArgs RaiseAnimationCompletedEvent()
    {
      return AnimationPanel.RaiseAnimationCompletedEvent( ( this._switchParent != null ) ? ( UIElement )this._switchParent : ( UIElement )this );
    }

    private static RoutedEventArgs RaiseAnimationCompletedEvent( UIElement target )
    {
      if( target == null )
        return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = AnimationPanel.AnimationCompletedEvent;
      RoutedEventHelper.RaiseEvent( target, args );
      return args;
    }

    #endregion

    #region ChildEntered Event

    public static readonly RoutedEvent ChildEnteredEvent =
      EventManager.RegisterRoutedEvent( "ChildEntered", RoutingStrategy.Bubble, typeof( ChildEnteredEventHandler ), typeof( AnimationPanel ) );

    public event ChildEnteredEventHandler ChildEntered
    {
      add
      {
        this.AddHandler( AnimationPanel.ChildEnteredEvent, value );
      }
      remove
      {
        this.RemoveHandler( AnimationPanel.ChildEnteredEvent, value );
      }
    }

    protected ChildEnteredEventArgs RaiseChildEnteredEvent( UIElement child, Rect arrangeRect )
    {
      return AnimationPanel.RaiseChildEnteredEvent( this, child, arrangeRect );
    }

    internal static ChildEnteredEventArgs RaiseChildEnteredEvent( UIElement target, UIElement child, Rect arrangeRect )
    {
      if( target == null )
        return null;

      ChildEnteredEventArgs args = new ChildEnteredEventArgs( child, arrangeRect );
      args.RoutedEvent = AnimationPanel.ChildEnteredEvent;
      RoutedEventHelper.RaiseEvent( target, args );
      return args;
    }

    #endregion

    #region ChildEntering Event

    public static readonly RoutedEvent ChildEnteringEvent =
      EventManager.RegisterRoutedEvent( "ChildEntering", RoutingStrategy.Bubble, typeof( ChildEnteringEventHandler ), typeof( AnimationPanel ) );

    public event ChildEnteringEventHandler ChildEntering
    {
      add
      {
        this.AddHandler( AnimationPanel.ChildEnteringEvent, value );
      }
      remove
      {
        this.RemoveHandler( AnimationPanel.ChildEnteringEvent, value );
      }
    }

    protected ChildEnteringEventArgs RaiseChildEnteringEvent( UIElement child, Rect? EnterFrom, Rect ArrangeRect )
    {
      return AnimationPanel.RaiseChildEnteringEvent( this, child, EnterFrom, ArrangeRect );
    }

    private static ChildEnteringEventArgs RaiseChildEnteringEvent( UIElement target, UIElement child, Rect? EnterFrom, Rect ArrangeRect )
    {
      if( target == null )
        return null;

      ChildEnteringEventArgs args = new ChildEnteringEventArgs( child, EnterFrom, ArrangeRect );
      args.RoutedEvent = AnimationPanel.ChildEnteringEvent;
      RoutedEventHelper.RaiseEvent( target, args );
      return args;
    }

    #endregion

    #region ChildExited Event

    public static readonly RoutedEvent ChildExitedEvent =
      EventManager.RegisterRoutedEvent( "ChildExited", RoutingStrategy.Bubble, typeof( ChildExitedEventHandler ), typeof( AnimationPanel ) );

    public event ChildExitedEventHandler ChildExited
    {
      add
      {
        this.AddHandler( AnimationPanel.ChildExitedEvent, value );
      }
      remove
      {
        this.RemoveHandler( AnimationPanel.ChildExitedEvent, value );
      }
    }

    protected ChildExitedEventArgs RaiseChildExitedEvent( UIElement child )
    {
      return AnimationPanel.RaiseChildExitedEvent( this, child );
    }

    private static ChildExitedEventArgs RaiseChildExitedEvent( UIElement target, UIElement child )
    {
      if( target == null )
        return null;

      ChildExitedEventArgs args = new ChildExitedEventArgs( child );
      args.RoutedEvent = AnimationPanel.ChildExitedEvent;
      RoutedEventHelper.RaiseEvent( target, args );
      return args;
    }

    #endregion

    #region ChildExiting Event

    public static readonly RoutedEvent ChildExitingEvent =
      EventManager.RegisterRoutedEvent( "ChildExiting", RoutingStrategy.Bubble, typeof( ChildExitingEventHandler ), typeof( AnimationPanel ) );

    public event ChildExitingEventHandler ChildExiting
    {
      add
      {
        this.AddHandler( AnimationPanel.ChildExitingEvent, value );
      }
      remove
      {
        this.RemoveHandler( AnimationPanel.ChildExitingEvent, value );
      }
    }

    protected ChildExitingEventArgs RaiseChildExitingEvent( UIElement child, Rect? exitTo, Rect arrangeRect )
    {
      return AnimationPanel.RaiseChildExitingEvent( this, child, exitTo, arrangeRect );
    }

    private static ChildExitingEventArgs RaiseChildExitingEvent( UIElement target, UIElement child, Rect? exitTo, Rect arrangeRect )
    {
      if( target == null )
        return null;

      ChildExitingEventArgs args = new ChildExitingEventArgs( child, exitTo, arrangeRect );
      args.RoutedEvent = AnimationPanel.ChildExitingEvent;
      RoutedEventHelper.RaiseEvent( target, args );
      return args;
    }

    #endregion

    #region SwitchLayoutActivated Event

    public static readonly RoutedEvent SwitchLayoutActivatedEvent =
      EventManager.RegisterRoutedEvent( "SwitchLayoutActivated", RoutingStrategy.Direct, typeof( RoutedEventHandler ), typeof( AnimationPanel ) );

    public event RoutedEventHandler SwitchLayoutActivated
    {
      add
      {
        this.AddHandler( AnimationPanel.SwitchLayoutActivatedEvent, value );
      }
      remove
      {
        this.RemoveHandler( AnimationPanel.SwitchLayoutActivatedEvent, value );
      }
    }

    protected RoutedEventArgs RaiseSwitchLayoutActivatedEvent()
    {
      return AnimationPanel.RaiseSwitchLayoutActivatedEvent( this );
    }

    internal static RoutedEventArgs RaiseSwitchLayoutActivatedEvent( UIElement target )
    {
      if( target == null )
        return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = AnimationPanel.SwitchLayoutActivatedEvent;
      RoutedEventHelper.RaiseEvent( target, args );
      return args;
    }

    #endregion

    #region SwitchLayoutDeactivated Event

    public static readonly RoutedEvent SwitchLayoutDeactivatedEvent =
      EventManager.RegisterRoutedEvent( "SwitchLayoutDeactivated", RoutingStrategy.Direct, typeof( RoutedEventHandler ), typeof( AnimationPanel ) );

    public event RoutedEventHandler SwitchLayoutDeactivated
    {
      add
      {
        this.AddHandler( AnimationPanel.SwitchLayoutDeactivatedEvent, value );
      }
      remove
      {
        this.RemoveHandler( AnimationPanel.SwitchLayoutDeactivatedEvent, value );
      }
    }

    protected RoutedEventArgs RaiseSwitchLayoutDeactivatedEvent()
    {
      return AnimationPanel.RaiseSwitchLayoutDeactivatedEvent( this );
    }

    internal static RoutedEventArgs RaiseSwitchLayoutDeactivatedEvent( UIElement target )
    {
      if( target == null )
        return null;

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = AnimationPanel.SwitchLayoutDeactivatedEvent;
      RoutedEventHelper.RaiseEvent( target, args );
      return args;
    }

    #endregion

    public new void InvalidateArrange()
    {
      if( _switchParent == null )
      {
        base.InvalidateArrange();
      }
      else
      {
        _switchParent.InvalidateArrange();
      }
    }

    public new void InvalidateMeasure()
    {
      if( _switchParent == null )
      {
        base.InvalidateMeasure();
      }
      else
      {
        _switchParent.InvalidateMeasure();
      }
    }

    public new void InvalidateVisual()
    {
      if( _switchParent == null )
      {
        base.InvalidateVisual();
      }
      else
      {
        _switchParent.InvalidateVisual();
      }
    }

    internal void ActivateLayout()
    {
      this.HasArranged = false;
      this.IsActiveLayout = true;
      this.OnSwitchLayoutActivated();
      this.RaiseSwitchLayoutActivatedEvent();
    }

    internal void BeginChildExit( UIElement child )
    {
      ChildState state = AnimationPanel.GetChildState( child );
      if( state != null )
      {
        state.Type = AnimationType.Exit;
        state.HasExitBegun = true;

        this.ExitingChildren.Add( child );

        if( _switchParent != null )
        {
          _switchParent.AddVisualChildInternal( child );
        }
        else
        {
          this.AddVisualChild( child );
        }

        // raise the ChildExiting event only after the child has been re-added to the visual tree
        ChildExitingEventArgs ceea = AnimationPanel.RaiseChildExitingEvent( child, child, AnimationPanel.GetExitTo( child ), state.CurrentPlacement );

        // begin the exit animation, if necessary
        state.Animator = this.GetEffectiveAnimator( AnimationType.Exit );
        if( state.Animator != null )
        {
          state.TargetPlacement = ceea.ExitTo.HasValue ? ceea.ExitTo.Value : Rect.Empty;
          state.BeginTimeStamp = DateTime.Now;

          // decrement the animating count if this child is already animating because the 
          // ArrangeChild call will increment it again
          if( state.IsAnimating )
          {
            this.AnimatingChildCount--;
          }
          this.ArrangeChild( child, state.TargetPlacement );
        }
        else
        {
          // no animation, so immediately end the exit routine
          this.EndChildExit( child, state );
        }
      }
    }

    internal void BeginGrandchildAnimation( FrameworkElement grandchild, Rect currentRect, Rect placementRect )
    {
      bool isDone = true;
      object placementArgs;
      ChildState state = new ChildState( currentRect );
      AnimationPanel.SetChildState( grandchild, state );
      state.Type = AnimationType.Switch;
      state.BeginTimeStamp = DateTime.Now;
      state.TargetPlacement = placementRect;
      state.Animator = this.GetEffectiveAnimator( AnimationType.Template );
      if( state.Animator != null && !state.TargetPlacement.IsEmpty )
      {
        AnimationRate rate = this.GetEffectiveAnimationRate( AnimationType.Template );
        state.CurrentPlacement = state.Animator.GetInitialChildPlacement( grandchild, state.CurrentPlacement, state.TargetPlacement, this, ref rate, out placementArgs, out isDone );
        state.AnimationRate = rate;
        state.PlacementArgs = placementArgs;
      }
      state.IsAnimating = !isDone;
      grandchild.Arrange( state.IsAnimating ? state.CurrentPlacement : state.TargetPlacement );
      if( state.IsAnimating )
      {
        _animatingGrandchildren.Add( grandchild );
        this.AnimatingChildCount++;
      }
      else
      {
        state.CurrentPlacement = state.TargetPlacement;
      }
    }

    internal void DeactivateLayout()
    {
      this.IsActiveLayout = false;
      this.AnimatingChildCount = 0;
      this.OnSwitchLayoutDeactivated();
      this.RaiseSwitchLayoutDeactivatedEvent();
    }

    internal static UIElement FindAncestorChildOfAnimationPanel( DependencyObject element, out AnimationPanel panel )
    {
      panel = null;
      if( element == null )
        return null;

      DependencyObject parent = VisualTreeHelper.GetParent( element );
      if( parent == null )
        return null;

      if( parent is AnimationPanel || parent is SwitchPanel )
      {
        panel = ( parent is SwitchPanel )
            ? ( parent as SwitchPanel )._currentLayoutPanel
            : parent as AnimationPanel;
        return element as UIElement;
      }

      return AnimationPanel.FindAncestorChildOfAnimationPanel( parent, out panel );
    }

    internal Dictionary<string, Rect> GetNewLocationsBasedOnTargetPlacement( SwitchPresenter presenter, UIElement parent )
    {
      ChildState state = AnimationPanel.GetChildState( parent );

      // if necessary, temporarily arrange the element at its final placement
      bool rearrange = ( state.CurrentPlacement != state.TargetPlacement && state.IsAnimating );
      if( rearrange )
      {
        parent.Arrange( state.TargetPlacement );
      }

      // now create a dictionary of locations for ID'd elements
      Dictionary<string, Rect> result = new Dictionary<string, Rect>();
      foreach( KeyValuePair<string, FrameworkElement> entry in presenter._knownIDs )
      {
        Size size = entry.Value.RenderSize;
        Point[] points = { new Point(), new Point( size.Width, size.Height ) };
        ( entry.Value.TransformToAncestor( VisualTreeHelper.GetParent( entry.Value ) as Visual ) as MatrixTransform ).Matrix.Transform( points );
        result[ entry.Key ] = new Rect( points[ 0 ], points[ 1 ] );
      }

      // restore the current placement
      if( rearrange )
      {
        parent.Arrange( state.CurrentPlacement );
      }
      return result;
    }

    internal Visual GetVisualChildInternal( int index )
    {
      return this.GetVisualChild( index );
    }

    internal void OnNotifyVisualChildAddedInternal( UIElement child )
    {
      this.OnNotifyVisualChildAdded( child );
    }

    internal void OnNotifyVisualChildRemovedInternal( UIElement child )
    {
      this.OnNotifyVisualChildRemoved( child );
    }

    internal Size MeasureChildrenCore( UIElementCollection children, Size constraint )
    {
      _currentChildren = children;
      return MeasureChildrenOverride( _currentChildren, constraint );
    }

    internal Size ArrangeChildrenCore( UIElementCollection children, Size finalSize )
    {
      if( _currentChildren != children )
      {
        _currentChildren = children;
      }

      // always reset the animating children count at the beginning of an arrange
      this.AnimatingChildCount = 0;
      _animatingGrandchildren.Clear();

      Size result;
      try
      {
        // determine if this arrange represents a layout switch for a SwitchPanel
        if( !this.HasArranged && _switchParent != null )
        {
          this.IsSwitchInProgress = true;
          _switchParent.BeginLayoutSwitch();
        }

        // arrange active children
        result = this.ArrangeChildrenOverride( _currentChildren, finalSize );

        // also arrange exiting children, if necessary
        if( this.ExitingChildren.Count > 0 )
        {
          this.AnimatingChildCount += this.ExitingChildren.Count;
          this.UpdateExitingChildren();
        }

        // if this is a layout switch, make sure the switch is ended
        if( this.IsSwitchInProgress )
        {
          if( this.AnimatingChildCount == 0 )
          {
            _switchParent.EndLayoutSwitch();
          }
          else
          {
            this.EndSwitchOnAnimationCompleted = true;
          }
        }
      }
      finally
      {
        this.HasArranged = true;
        this.IsSwitchInProgress = false;
      }
      return result;
    }

    internal void OnSwitchParentVisualChildrenChanged( DependencyObject visualAdded, DependencyObject visualRemoved )
    {
      this.OnVisualChildrenChanged( visualAdded, visualRemoved );
    }

    protected sealed override Size MeasureOverride( Size constraint )
    {
      return this.MeasureChildrenCore( InternalChildren, constraint );
    }

    protected abstract Size MeasureChildrenOverride( UIElementCollection children, Size constraint );

    protected sealed override Size ArrangeOverride( Size finalSize )
    {
      return this.ArrangeChildrenCore( _currentChildren, finalSize );
    }

    protected abstract Size ArrangeChildrenOverride( UIElementCollection children, Size finalSize );

    protected void ArrangeChild( UIElement child, Rect placementRect )
    {
      // Offset in case SwitchPanel is handling scroll.
      if( placementRect.IsEmpty == false && this.PhysicalScrollOffset.Length > 0 )
      {
        placementRect.Offset( -this.PhysicalScrollOffset );
      }

      // cannot start animations unless the panel is loaded
      if( this.HasLoaded )
      {
        if( this.BeginChildAnimation( child, placementRect ) )
        {
          this.AnimatingChildCount++;
        }
      }
      else
      {
        // just arrange the child if the panel has not yet loaded
        child.Arrange( placementRect );
      }
    }

    protected new void AddVisualChild( Visual child )
    {
      if( _switchParent == null )
      {
        base.AddVisualChild( child );
      }
      else
      {
        _switchParent.AddVisualChildInternal( child );
      }
    }

    protected override Visual GetVisualChild( int index )
    {
      if( index < 0 )
      {
        throw new IndexOutOfRangeException();
      }
      if( index >= this.InternalChildren.Count )
      {
        int exitIndex = index - this.InternalChildren.Count;
        if( exitIndex < 0 || exitIndex >= this.ExitingChildren.Count )
          throw new IndexOutOfRangeException();

        return this.ExitingChildren[ exitIndex ];
      }
      return ( _switchParent == null ) ? base.GetVisualChild( index ) : _switchParent.GetVisualChildInternal( index );
    }

    protected virtual void OnNotifyVisualChildAdded( UIElement child )
    {
    }

    protected virtual void OnNotifyVisualChildRemoved( UIElement child )
    {
    }

    protected virtual void OnSwitchLayoutActivated()
    {
    }

    protected virtual void OnSwitchLayoutDeactivated()
    {
    }

    protected override void OnVisualChildrenChanged( DependencyObject visualAdded, DependencyObject visualRemoved )
    {
      if( !this.IsRemovingInternalChild )
      {
        if( visualRemoved is UIElement && visualRemoved != null )
        {
          this.IsRemovingInternalChild = true;
          try
          {
            this.BeginChildExit( visualRemoved as UIElement );
          }
          finally
          {
            this.IsRemovingInternalChild = false;
          }
        }
      }
      if( _switchParent == null )
      {
        // The OnNotifyChildAdded/Removed methods get called for all animation panels within a 
        // SwitchPanel.Layouts collection, regardless of whether they are the active layout 
        // for the SwitchPanel.  Here, we also ensure that the methods are called for standalone panels.
        if( visualAdded is UIElement )
        {
          this.OnNotifyVisualChildAdded( visualAdded as UIElement );
        }
        else if( visualRemoved is UIElement )
        {
          this.OnNotifyVisualChildRemoved( visualRemoved as UIElement );
        }
        base.OnVisualChildrenChanged( visualAdded, visualRemoved );
      }
      else
      {
        _switchParent.OnVisualChildrenChangedInternal( visualAdded, visualRemoved );
      }
    }


    protected new void RemoveVisualChild( Visual child )
    {
      if( _switchParent == null )
      {
        base.RemoveVisualChild( child );
      }
      else
      {
        _switchParent.RemoveVisualChildInternal( child );
      }
    }

    protected int FindChildFromVisual( Visual vis )
    {
      int index = -1;

      DependencyObject parent = vis;
      DependencyObject child = null;

      do
      {
        child = parent;
        parent = VisualTreeHelper.GetParent( child );
      }
      while( parent != null && parent != ChildrensParent );

      if( parent == this.ChildrensParent )
      {
        index = this.ChildrensParent.Children.IndexOf( ( UIElement )child );
      }

      return index;
    }

    private bool BeginChildAnimation( UIElement child, Rect placementRect )
    {
      // a private attached property is used to hold the information needed 
      // to calculate the location of items on subsequent frame refreshes
      bool newStateCreated;
      ChildState state = this.EnsureChildState( child, placementRect, out newStateCreated );
      if( state.HasEnterCompleted )
      {
        if( state.Type != AnimationType.Exit )
        {
          state.BeginTimeStamp = DateTime.Now;
          state.Type = IsSwitchInProgress ? AnimationType.Switch : AnimationType.Layout;
          state.TargetPlacement = placementRect;
        }
      }
      else
      {
        // if the child is in the middle of an enter animation, we
        // still need to update the placement rect
        state.BeginTimeStamp = DateTime.Now;
        state.TargetPlacement = placementRect;
      }

      if( !state.HasExitCompleted )
      {
        bool isDone = true;
        object placementArgs;
        if( state.Type != AnimationType.Enter )
        {
          state.Animator = this.GetEffectiveAnimator( state.Type );
        }
        if( state.Animator != null && !state.TargetPlacement.IsEmpty )
        {
          AnimationRate rate = this.GetEffectiveAnimationRate( state.Type );
          state.CurrentPlacement = state.Animator.GetInitialChildPlacement(
              child, state.CurrentPlacement, state.TargetPlacement, this,
              ref rate, out placementArgs, out isDone );
          state.AnimationRate = rate;
          state.PlacementArgs = placementArgs;
        }
        state.IsAnimating = !isDone;
        if( !state.IsAnimating )
        {
          state.CurrentPlacement = state.TargetPlacement;
        }
      }

      // JZ this might not be needed nice the OnRender will arrange
      if( state.IsAnimating == false )
      {
        this.UpdateTrueArrange( child, state );
      }

      return state.IsAnimating;
    }

    private void BeginChildEnter( UIElement child, ChildState state )
    {
      state.Type = AnimationType.Enter;

      // raise the ChildEntering event
      ChildEnteringEventArgs ceea = AnimationPanel.RaiseChildEnteringEvent( child,
          child, GetEnterFrom( child ), state.CurrentPlacement );

      // begin the enter animation, if necessary
      state.Animator = this.GetEffectiveAnimator( AnimationType.Enter );
      if( state.Animator != null && ceea.EnterFrom.HasValue )
      {
        state.CurrentPlacement = ceea.EnterFrom.Value;
        state.BeginTimeStamp = DateTime.Now;
      }
    }

    private void EndChildEnter( UIElement child, ChildState state )
    {
      // raise the ChildExited event
      state.HasEnterCompleted = true;
      AnimationPanel.RaiseChildEnteredEvent( child, child, state.TargetPlacement );
    }

    private void EndChildExit( UIElement child, ChildState state )
    {
      // raise the ChildExited event
      state.HasExitCompleted = true;
      AnimationPanel.RaiseChildExitedEvent( child, child );

      // remove the visual child relationship
      if( this.ExitingChildren.Contains( child ) )
      {
        this.IsRemovingInternalChild = true;
        try
        {
          if( _switchParent != null )
          {
            _switchParent.RemoveVisualChildInternal( child );
          }
          else
          {
            this.RemoveVisualChild( child );
          }
        }
        finally
        {
          this.IsRemovingInternalChild = false;
        }
        this.ExitingChildren.Remove( child );
      }

      child.ClearValue( AnimationPanel.ChildStatePropertyKey );
    }

    private ChildState EnsureChildState( UIElement child, Rect placementRect, out bool newStateCreated )
    {
      newStateCreated = false;
      ChildState state = AnimationPanel.GetChildState( child );
      if( state == null )
      {
        // if this is null, it's because this is the first time that
        // the object has been arranged
        state = new ChildState( placementRect );
        AnimationPanel.SetChildState( child, state );
        this.BeginChildEnter( child, state );
        newStateCreated = true;
      }
      return state;
    }

    internal AnimationRate GetEffectiveAnimationRate( AnimationType animationType )
    {
      AnimationRate result = ( _switchParent == null ) ? this.DefaultAnimationRate : _switchParent.DefaultAnimationRate;
      switch( animationType )
      {
        case AnimationType.Enter:
          if( this.EnterAnimationRate != AnimationRate.Default )
          {
            result = this.EnterAnimationRate;
          }
          else if( _switchParent != null && _switchParent.EnterAnimationRate != AnimationRate.Default )
          {
            result = _switchParent.EnterAnimationRate;
          }
          break;

        case AnimationType.Exit:
          if( this.ExitAnimationRate != AnimationRate.Default )
          {
            result = this.ExitAnimationRate;
          }
          else if( _switchParent != null && _switchParent.ExitAnimationRate != AnimationRate.Default )
          {
            result = _switchParent.ExitAnimationRate;
          }
          break;

        case AnimationType.Layout:
          if( this.LayoutAnimationRate != AnimationRate.Default )
          {
            result = LayoutAnimationRate;
          }
          else if( _switchParent != null && _switchParent.LayoutAnimationRate != AnimationRate.Default )
          {
            result = _switchParent.LayoutAnimationRate;
          }
          break;

        case AnimationType.Switch:
          if( this.SwitchAnimationRate != AnimationRate.Default )
          {
            result = SwitchAnimationRate;
          }
          else if( _switchParent != null && _switchParent.SwitchAnimationRate != AnimationRate.Default )
          {
            result = _switchParent.SwitchAnimationRate;
          }
          break;

        case AnimationType.Template:
          if( this.TemplateAnimationRate != AnimationRate.Default )
          {
            result = this.TemplateAnimationRate;
          }
          else if( _switchParent != null && _switchParent.TemplateAnimationRate != AnimationRate.Default )
          {
            result = _switchParent.TemplateAnimationRate;
          }
          break;
      }
      return result;
    }

    private IterativeAnimator GetEffectiveAnimator( AnimationType animationType )
    {
      IterativeAnimator result = ( _switchParent == null ) ? this.DefaultAnimator : _switchParent.DefaultAnimator;
      switch( animationType )
      {
        case AnimationType.Enter:
          if( this.EnterAnimator != IterativeAnimator.Default || ( _switchParent != null && _switchParent.EnterAnimator != IterativeAnimator.Default ) )
          {
            result = ( EnterAnimator == IterativeAnimator.Default ) ? _switchParent.EnterAnimator : EnterAnimator;
          }
          break;

        case AnimationType.Exit:
          if( this.ExitAnimator != IterativeAnimator.Default || ( _switchParent != null && _switchParent.ExitAnimator != IterativeAnimator.Default ) )
          {
            result = ( ExitAnimator == IterativeAnimator.Default ) ? _switchParent.ExitAnimator : ExitAnimator;
          }
          break;

        case AnimationType.Layout:
          if( this.LayoutAnimator != IterativeAnimator.Default || ( _switchParent != null && _switchParent.LayoutAnimator != IterativeAnimator.Default ) )
          {
            result = ( LayoutAnimator == IterativeAnimator.Default ) ? _switchParent.LayoutAnimator : LayoutAnimator;
          }
          break;

        case AnimationType.Switch:
          if( _switchParent != null && !_switchParent.AreLayoutSwitchesAnimated )
          {
            result = null;
          }
          else
          {
            if( this.SwitchAnimator != IterativeAnimator.Default || _switchParent.SwitchAnimator != IterativeAnimator.Default )
            {
              result = ( SwitchAnimator == IterativeAnimator.Default ) ? _switchParent.SwitchAnimator : SwitchAnimator;
            }
          }
          break;

        case AnimationType.Template:
          if( this.TemplateAnimator != IterativeAnimator.Default || ( _switchParent != null && _switchParent.TemplateAnimator != IterativeAnimator.Default ) )
          {
            result = ( TemplateAnimator == IterativeAnimator.Default ) ? _switchParent.TemplateAnimator : TemplateAnimator;
          }
          break;
      }
      return result;
    }

    private void OnLoaded( object sender, RoutedEventArgs e )
    {
      this.HasLoaded = true;

      // invalidate arrange to give enter animations a chance to run
      this.InvalidateArrange();
    }

    private void OnRendering( object sender, EventArgs e )
    {
      if( !this.IsActiveLayout )
        return;

      if( _currentChildren != null )
      {
        foreach( UIElement child in _currentChildren )
        {
          if( child == null )
            continue;

          ChildState state = AnimationPanel.GetChildState( child );
          if( state != null )
          {
            TimeSpan t = DateTime.Now.Subtract( state.BeginTimeStamp );
            if( state.IsAnimating )
            {
              bool isDone;
              state.CurrentPlacement = state.Animator.GetNextChildPlacement( child, t, state.CurrentPlacement,
                  state.TargetPlacement, this, state.AnimationRate, ref state.PlacementArgs, out isDone );
              state.IsAnimating = !isDone;
              this.UpdateTrueArrange( child, state );
              if( !state.IsAnimating )
              {
                this.AnimatingChildCount--;
              }
            }
          }
        }
      }

      foreach( FrameworkElement grandchild in _animatingGrandchildren )
      {
        ChildState state = AnimationPanel.GetChildState( grandchild );
        if( state != null && state.IsAnimating )
        {
          TimeSpan t = DateTime.Now.Subtract( state.BeginTimeStamp );
          bool isDone;
          state.CurrentPlacement = state.Animator.GetNextChildPlacement( grandchild, t, state.CurrentPlacement,
              state.TargetPlacement, this, state.AnimationRate, ref state.PlacementArgs, out isDone );
          state.IsAnimating = !isDone;
          Rect rect = state.IsAnimating ? state.CurrentPlacement : state.TargetPlacement;
          grandchild.Arrange( rect );
          if( !state.IsAnimating )
          {
            this.AnimatingChildCount--;
          }
        }
      }

      this.UpdateExitingChildren();

      if( this.AnimatingChildCount == 0 )
      {
        _animatingGrandchildren.Clear();
      }
    }

    private void UpdateExitingChildren()
    {
      if( this.ExitingChildren.Count > 0 )
      {
        List<UIElement> exitingChildren = new List<UIElement>( ExitingChildren );
        foreach( UIElement child in exitingChildren )
        {
          if( child == null )
            continue;

          ChildState state = AnimationPanel.GetChildState( child );
          if( state != null )
          {
            TimeSpan t = DateTime.Now.Subtract( state.BeginTimeStamp );
            if( state.IsAnimating )
            {
              bool isDone;
              state.CurrentPlacement = state.Animator.GetNextChildPlacement( child, t, state.CurrentPlacement,
                  state.TargetPlacement, this, state.AnimationRate, ref state.PlacementArgs, out isDone );
              state.IsAnimating = !isDone;
              this.UpdateTrueArrange( child, state );
              if( !state.IsAnimating )
              {
                this.AnimatingChildCount--;
              }
            }
          }
        }
      }
    }

    private void UpdateTrueArrange( UIElement child, ChildState state )
    {
      if( !state.TargetPlacement.IsEmpty )
      {
        child.Arrange( state.IsAnimating && state.Animator != null ? state.CurrentPlacement : state.TargetPlacement );
      }

      // if the child is done entering, complete the enter routine
      if( !state.IsAnimating && !state.HasEnterCompleted )
      {
        this.EndChildEnter( child, state );
      }

      // if the child is done exiting, complete the exit routine
      if( !state.IsAnimating && state.HasExitBegun )
      {
        this.EndChildExit( child, state );
      }
    }

    #region Private Fields

    private UIElementCollection _currentChildren;
    private readonly Collection<FrameworkElement> _animatingGrandchildren = new Collection<FrameworkElement>();
    private SwitchPanel _switchParent = null;
    private List<UIElement> _exitingChildren = null;
    private BitVector32 _cacheBits = new BitVector32( 1 );

    #endregion

    #region ChildState Nested Type

    private sealed class ChildState
    {
      public ChildState( Rect currentRect )
      {
        this.CurrentPlacement = currentRect;
        this.TargetPlacement = currentRect;
        this.BeginTimeStamp = DateTime.Now;
      }

      public bool HasEnterCompleted
      {
        get
        {
          return _cacheBits[ ( int )CacheBits.HasEnterCompleted ];
        }
        set
        {
          _cacheBits[ ( int )CacheBits.HasEnterCompleted ] = value;
        }
      }

      public bool HasExitBegun
      {
        get
        {
          return _cacheBits[ ( int )CacheBits.HasExitBegun ];
        }
        set
        {
          _cacheBits[ ( int )CacheBits.HasExitBegun ] = value;
        }
      }

      public bool HasExitCompleted
      {
        get
        {
          return _cacheBits[ ( int )CacheBits.HasExitCompleted ];
        }
        set
        {
          _cacheBits[ ( int )CacheBits.HasExitCompleted ] = value;
        }
      }

      public bool IsAnimating
      {
        get
        {
          return _cacheBits[ ( int )CacheBits.IsAnimating ];
        }
        set
        {
          _cacheBits[ ( int )CacheBits.IsAnimating ] = value;
        }
      }

      public AnimationType Type;
      public DateTime BeginTimeStamp;
      public IterativeAnimator Animator;
      public Rect CurrentPlacement;
      public Rect TargetPlacement;
      public AnimationRate AnimationRate;
      public object PlacementArgs;

      private BitVector32 _cacheBits = new BitVector32( 0 );
      private enum CacheBits
      {
        IsAnimating = 0x00000001,
        HasEnterCompleted = 0x00000002,
        HasExitBegun = 0x00000004,
        HasExitCompleted = 0x00000008,
      }
    }

    #endregion

    #region AnimationType Nested Type

    internal enum AnimationType
    {
      Enter,
      Exit,
      Layout,
      Switch,
      Template,
    }

    #endregion

    #region CacheBits Nested Type

    private enum CacheBits
    {
      IsActiveLayout = 0x00000001,
      IsSwitchInProgress = 0x00000002,
      EndSwitchOnAnimationCompleted = 0x00000010,
      IsRemovingInternalChild = 0x00000020,
      HasLoaded = 0x00000040,
      HasArranged = 0x00000080,
    }

    #endregion
  }
}
