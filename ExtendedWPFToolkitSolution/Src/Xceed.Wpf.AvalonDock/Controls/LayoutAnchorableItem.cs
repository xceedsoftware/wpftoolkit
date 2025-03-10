/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2025 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock.Commands;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class LayoutAnchorableItem : LayoutItem
  {
    #region Members

    private LayoutAnchorable _anchorable;
    private ICommand _defaultHideCommand;
    private ICommand _defaultAutoHideCommand;
    private ICommand _defaultDockCommand;
    private ReentrantFlag _visibilityReentrantFlag = new ReentrantFlag();

    #endregion

    #region Constructors

    internal LayoutAnchorableItem()
    {
    }

    #endregion

    #region Properties

    #region HideCommand

    public static readonly DependencyProperty HideCommandProperty = DependencyProperty.Register( "HideCommand", typeof( ICommand ), typeof( LayoutAnchorableItem ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnHideCommandChanged ), new CoerceValueCallback( CoerceHideCommandValue ) ) );

    public ICommand HideCommand
    {
      get
      {
        return ( ICommand )GetValue( HideCommandProperty );
      }
      set
      {
        SetValue( HideCommandProperty, value );
      }
    }

    private static void OnHideCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutAnchorableItem )d ).OnHideCommandChanged( e );
    }

    protected virtual void OnHideCommandChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceHideCommandValue( DependencyObject d, object value )
    {
      return value;
    }

    private bool CanExecuteHideCommand( object parameter )
    {
      if( LayoutElement == null )
        return false;
      return _anchorable.CanHide;
    }

    private void ExecuteHideCommand( object parameter )
    {
      if( _anchorable != null && _anchorable.Root != null && _anchorable.Root.Manager != null )
        _anchorable.Root.Manager._ExecuteHideCommand( _anchorable );
    }

    #endregion

    #region AutoHideCommand

    public static readonly DependencyProperty AutoHideCommandProperty = DependencyProperty.Register( "AutoHideCommand", typeof( ICommand ), typeof( LayoutAnchorableItem ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnAutoHideCommandChanged ), new CoerceValueCallback( CoerceAutoHideCommandValue ) ) );

    public ICommand AutoHideCommand
    {
      get
      {
        return ( ICommand )GetValue( AutoHideCommandProperty );
      }
      set
      {
        SetValue( AutoHideCommandProperty, value );
      }
    }

    private static void OnAutoHideCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutAnchorableItem )d ).OnAutoHideCommandChanged( e );
    }

    protected virtual void OnAutoHideCommandChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceAutoHideCommandValue( DependencyObject d, object value )
    {
      return value;
    }

    private bool CanExecuteAutoHideCommand( object parameter )
    {
      if( LayoutElement == null )
        return false;

      if( LayoutElement.FindParent<LayoutAnchorableFloatingWindow>() != null )
        return false;//is floating

      return _anchorable.CanAutoHide;
    }

    private void ExecuteAutoHideCommand( object parameter )
    {
      if( _anchorable != null && _anchorable.Root != null && _anchorable.Root.Manager != null )
        _anchorable.Root.Manager._ExecuteAutoHideCommand( _anchorable );
    }

    #endregion

    #region DockCommand

    public static readonly DependencyProperty DockCommandProperty = DependencyProperty.Register( "DockCommand", typeof( ICommand ), typeof( LayoutAnchorableItem ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnDockCommandChanged ), new CoerceValueCallback( CoerceDockCommandValue ) ) );

    public ICommand DockCommand
    {
      get
      {
        return ( ICommand )GetValue( DockCommandProperty );
      }
      set
      {
        SetValue( DockCommandProperty, value );
      }
    }

    private static void OnDockCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutAnchorableItem )d ).OnDockCommandChanged( e );
    }

    protected virtual void OnDockCommandChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceDockCommandValue( DependencyObject d, object value )
    {
      return value;
    }

    private bool CanExecuteDockCommand( object parameter )
    {
      if( LayoutElement == null )
        return false;
      return LayoutElement.FindParent<LayoutAnchorableFloatingWindow>() != null;
    }

    private void ExecuteDockCommand( object parameter )
    {
      LayoutElement.Root.Manager._ExecuteDockCommand( _anchorable );
    }

    #endregion    

    #region CanHide

    public static readonly DependencyProperty CanHideProperty = DependencyProperty.Register( "CanHide", typeof( bool ), typeof( LayoutAnchorableItem ), new FrameworkPropertyMetadata( ( bool )true,
                new PropertyChangedCallback( OnCanHideChanged ) ) );

    public bool CanHide
    {
      get
      {
        return ( bool )GetValue( CanHideProperty );
      }
      set
      {
        SetValue( CanHideProperty, value );
      }
    }

    private static void OnCanHideChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutAnchorableItem )d ).OnCanHideChanged( e );
    }

    protected virtual void OnCanHideChanged( DependencyPropertyChangedEventArgs e )
    {
      if( _anchorable != null )
        _anchorable.CanHide = ( bool )e.NewValue;
    }

    #endregion

    #endregion

    #region Overrides

    internal override void Attach( LayoutContent model )
    {
      _anchorable = model as LayoutAnchorable;
      _anchorable.IsVisibleChanged += new EventHandler( _anchorable_IsVisibleChanged );

      if( _anchorable.CanClose )
      {
        _anchorable.SetCanCloseInternal( true );
      }

      base.Attach( model );
    }

    internal override void Detach()
    {
      _anchorable.IsVisibleChanged -= new EventHandler( _anchorable_IsVisibleChanged );
      _anchorable = null;
      base.Detach();
    }

    protected override bool CanExecuteDockAsDocumentCommand()
    {
      var canExecute = base.CanExecuteDockAsDocumentCommand();
      if( canExecute && ( _anchorable != null ) )
        return _anchorable.CanDockAsTabbedDocument;

      return canExecute;
    }

    protected override void Close()
    {
      if( ( _anchorable.Root != null ) && ( _anchorable.Root.Manager != null ) )
      {
        var dockingManager = _anchorable.Root.Manager;
        dockingManager._ExecuteCloseCommand( _anchorable );
      }
    }

    protected override void InitDefaultCommands()
    {
      _defaultHideCommand = new RelayCommand( ( p ) => ExecuteHideCommand( p ), ( p ) => CanExecuteHideCommand( p ) );
      _defaultAutoHideCommand = new RelayCommand( ( p ) => ExecuteAutoHideCommand( p ), ( p ) => CanExecuteAutoHideCommand( p ) );
      _defaultDockCommand = new RelayCommand( ( p ) => ExecuteDockCommand( p ), ( p ) => CanExecuteDockCommand( p ) );

      base.InitDefaultCommands();
    }

    protected override void ClearDefaultCommands()
    {
      _defaultHideCommand = null;
      _defaultAutoHideCommand = null;
      _defaultDockCommand = null;

      base.ClearDefaultCommands();
    }

    protected override void ClearDefaultBindings()
    {
      if( this.HideCommand == _defaultHideCommand )
      {
        BindingOperations.ClearBinding( this, HideCommandProperty );
        this.HideCommand = null;
      }
      if( this.AutoHideCommand == _defaultAutoHideCommand )
      {
        BindingOperations.ClearBinding( this, AutoHideCommandProperty );
        this.AutoHideCommand = null;
      }
      if( this.DockCommand == _defaultDockCommand )
      {
        BindingOperations.ClearBinding( this, DockCommandProperty );
        this.DockCommand = null;
      }

      base.ClearDefaultBindings();
    }

    protected override void SetDefaultBindings()
    {
      if( this.HideCommand == null )
      {
        this.SetCurrentValue( LayoutAnchorableItem.HideCommandProperty, _defaultHideCommand );
      }
      if( this.AutoHideCommand == null )
      {
        this.SetCurrentValue( LayoutAnchorableItem.AutoHideCommandProperty, _defaultAutoHideCommand );
      }
      if( this.DockCommand == null )
      {
        this.SetCurrentValue( LayoutAnchorableItem.DockCommandProperty, _defaultDockCommand );
      }

      this.SetCurrentValue( LayoutAnchorableItem.VisibilityProperty, _anchorable.IsVisible ? Visibility.Visible : System.Windows.Visibility.Hidden );
      base.SetDefaultBindings();
    }

    protected override void OnVisibilityChanged()
    {
      if( _anchorable != null && _anchorable.Root != null )
      {
        if( _visibilityReentrantFlag.CanEnter )
        {
          using( _visibilityReentrantFlag.Enter() )
          {
            if( Visibility == System.Windows.Visibility.Hidden )
              _anchorable.Hide( false );
            else if( Visibility == System.Windows.Visibility.Visible )
              _anchorable.Show();
          }
        }
      }

      base.OnVisibilityChanged();
    }

    #endregion

    #region Private Methods

    private void _anchorable_IsVisibleChanged( object sender, EventArgs e )
    {
      if( _anchorable != null && _anchorable.Root != null )
      {
        if( _visibilityReentrantFlag.CanEnter )
        {
          using( _visibilityReentrantFlag.Enter() )
          {
            if( _anchorable.IsVisible )
              Visibility = Visibility.Visible;
            else
              Visibility = Visibility.Hidden;
          }
        }
      }
    }







    #endregion
  }
}
