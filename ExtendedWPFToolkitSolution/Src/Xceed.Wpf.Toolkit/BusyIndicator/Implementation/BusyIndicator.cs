/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2023 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Xceed.Wpf.Toolkit
{
  [TemplateVisualState( Name = VisualStates.StateIdle, GroupName = VisualStates.GroupBusyStatus )]
  [TemplateVisualState( Name = VisualStates.StateBusy, GroupName = VisualStates.GroupBusyStatus )]
  [TemplateVisualState( Name = VisualStates.StateVisible, GroupName = VisualStates.GroupVisibility )]
  [TemplateVisualState( Name = VisualStates.StateHidden, GroupName = VisualStates.GroupVisibility )]
  [StyleTypedProperty( Property = "OverlayStyle", StyleTargetType = typeof( Rectangle ) )]
  [StyleTypedProperty( Property = "ProgressBarStyle", StyleTargetType = typeof( ProgressBar ) )]
  public class BusyIndicator : ContentControl
  {
    #region Private Members

    private DispatcherTimer _displayAfterTimer = new DispatcherTimer();

    #endregion //Private Members

    #region Constructors

    static BusyIndicator()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( BusyIndicator ), new FrameworkPropertyMetadata( typeof( BusyIndicator ) ) );
    }

    public BusyIndicator()
    {
      _displayAfterTimer.Tick += DisplayAfterTimerElapsed;
    }

    #endregion //Constructors

    #region Base Class Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      ChangeVisualState( false );
    }


    #endregion //Base Class Overrides

    #region Properties

    protected bool IsContentVisible
    {
      get;
      set;
    }

    #endregion //Properties

    #region Dependency Properties

    #region IsBusy

    public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
        "IsBusy",
        typeof( bool ),
        typeof( BusyIndicator ),
        new PropertyMetadata( false, new PropertyChangedCallback( OnIsBusyChanged ) ) );

    public bool IsBusy
    {
      get
      {
        return ( bool )GetValue( IsBusyProperty );
      }
      set
      {
        SetValue( IsBusyProperty, value );
      }
    }

    private static void OnIsBusyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( BusyIndicator )d ).OnIsBusyChanged( e );
    }

    protected virtual void OnIsBusyChanged( DependencyPropertyChangedEventArgs e )
    {
      if( IsBusy )
      {
        if( DisplayAfter.Equals( TimeSpan.Zero ) )
        {
          // Go visible now
          IsContentVisible = true;
        }
        else
        {
          // Set a timer to go visible
          _displayAfterTimer.Interval = DisplayAfter;
          _displayAfterTimer.Start();
        }
      }
      else
      {
        // No longer visible
        _displayAfterTimer.Stop();
        IsContentVisible = false;

        if( this.FocusAfterBusy != null )
        {
          this.FocusAfterBusy.Dispatcher.BeginInvoke( DispatcherPriority.Input, new Action( () =>
          {
            this.FocusAfterBusy.Focus();
          }
          ) );
        }
      }

      ChangeVisualState( true );
    }

    #endregion //IsBusy

    #region Busy Content

    public static readonly DependencyProperty BusyContentProperty = DependencyProperty.Register(
        "BusyContent",
        typeof( object ),
        typeof( BusyIndicator ),
        new PropertyMetadata( null ) );

    public object BusyContent
    {
      get
      {
        return ( object )GetValue( BusyContentProperty );
      }
      set
      {
        SetValue( BusyContentProperty, value );
      }
    }

    #endregion //Busy Content

    #region Busy Content Template

    public static readonly DependencyProperty BusyContentTemplateProperty = DependencyProperty.Register(
        "BusyContentTemplate",
        typeof( DataTemplate ),
        typeof( BusyIndicator ),
        new PropertyMetadata( null ) );

    public DataTemplate BusyContentTemplate
    {
      get
      {
        return ( DataTemplate )GetValue( BusyContentTemplateProperty );
      }
      set
      {
        SetValue( BusyContentTemplateProperty, value );
      }
    }

    #endregion //Busy Content Template

    #region Display After

    public static readonly DependencyProperty DisplayAfterProperty = DependencyProperty.Register(
        "DisplayAfter",
        typeof( TimeSpan ),
        typeof( BusyIndicator ),
        new PropertyMetadata( TimeSpan.FromSeconds( 0.1 ) ) );

    public TimeSpan DisplayAfter
    {
      get
      {
        return ( TimeSpan )GetValue( DisplayAfterProperty );
      }
      set
      {
        SetValue( DisplayAfterProperty, value );
      }
    }

    #endregion //Display After

    #region FocusAfterBusy

    public static readonly DependencyProperty FocusAfterBusyProperty = DependencyProperty.Register(
        "FocusAfterBusy",
        typeof( Control ),
        typeof( BusyIndicator ),
        new PropertyMetadata( null ) );

    public Control FocusAfterBusy
    {
      get
      {
        return ( Control )GetValue( FocusAfterBusyProperty );
      }
      set
      {
        SetValue( FocusAfterBusyProperty, value );
      }
    }

    #endregion //FocusAfterBusy

    #region Overlay Style

    public static readonly DependencyProperty OverlayStyleProperty = DependencyProperty.Register(
        "OverlayStyle",
        typeof( Style ),
        typeof( BusyIndicator ),
        new PropertyMetadata( null ) );

    public Style OverlayStyle
    {
      get
      {
        return ( Style )GetValue( OverlayStyleProperty );
      }
      set
      {
        SetValue( OverlayStyleProperty, value );
      }
    }
    #endregion //Overlay Style

    #region ProgressBar Style

    public static readonly DependencyProperty ProgressBarStyleProperty = DependencyProperty.Register(
        "ProgressBarStyle",
        typeof( Style ),
        typeof( BusyIndicator ),
        new PropertyMetadata( null ) );

    public Style ProgressBarStyle
    {
      get
      {
        return ( Style )GetValue( ProgressBarStyleProperty );
      }
      set
      {
        SetValue( ProgressBarStyleProperty, value );
      }
    }

    #endregion //ProgressBar Style

    #endregion //Dependency Properties

    #region Methods

    private void DisplayAfterTimerElapsed( object sender, EventArgs e )
    {
      _displayAfterTimer.Stop();
      IsContentVisible = true;
      ChangeVisualState( true );
    }

    protected virtual void ChangeVisualState( bool useTransitions )
    {
      VisualStateManager.GoToState( this, IsBusy ? VisualStates.StateBusy : VisualStates.StateIdle, useTransitions );
      VisualStateManager.GoToState( this, IsContentVisible ? VisualStates.StateVisible : VisualStates.StateHidden, useTransitions );
    }

    #endregion //Methods
  }
}
