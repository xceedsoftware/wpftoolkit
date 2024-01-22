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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.AvalonDock.Commands;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public abstract class LayoutItem : FrameworkElement
  {
    #region Members

    private ICommand _defaultCloseCommand;
    private ICommand _defaultFloatCommand;
    private ICommand _defaultDockAsDocumentCommand;
    private ICommand _defaultCloseAllButThisCommand;
    private ICommand _defaultCloseAllCommand;
    private ICommand _defaultActivateCommand;
    private ICommand _defaultNewVerticalTabGroupCommand;
    private ICommand _defaultNewHorizontalTabGroupCommand;
    private ICommand _defaultMoveToNextTabGroupCommand;
    private ICommand _defaultMoveToPreviousTabGroupCommand;
    internal ContentPresenter _view = null;
    private ReentrantFlag _isSelectedReentrantFlag = new ReentrantFlag();
    private ReentrantFlag _isActiveReentrantFlag = new ReentrantFlag();

    #endregion

    #region Constructors

    static LayoutItem()
    {
      ToolTipProperty.OverrideMetadata( typeof( LayoutItem ), new FrameworkPropertyMetadata( null, ( s, e ) => OnToolTipChanged( s, e ) ) );
      VisibilityProperty.OverrideMetadata( typeof( LayoutItem ), new FrameworkPropertyMetadata( Visibility.Visible, ( s, e ) => OnVisibilityChanged( s, e ) ) );
    }


    internal LayoutItem()
    {
    }

    #endregion

    #region Properties

    #region LayoutElement

    public LayoutContent LayoutElement
    {
      get;
      private set;
    }

    #endregion

    #region Model

    public object Model
    {
      get;
      private set;
    }

    #endregion

    #region View

    public ContentPresenter View
    {
      get
      {
        if( _view == null )
        {
          _view = new ContentPresenter();

          _view.SetBinding( ContentPresenter.ContentProperty, new Binding( "Content" ) { Source = LayoutElement } );
          if( ( LayoutElement != null ) && ( LayoutElement.Root != null ) )
          {
            _view.SetBinding( ContentPresenter.ContentTemplateProperty, new Binding( "LayoutItemTemplate" ) { Source = LayoutElement.Root.Manager } );
            _view.SetBinding( ContentPresenter.ContentTemplateSelectorProperty, new Binding( "LayoutItemTemplateSelector" ) { Source = LayoutElement.Root.Manager } );
            if( LayoutElement.Root.Manager != null )
            {
              LayoutElement.Root.Manager.InternalAddLogicalChild( _view );
            }
          }
        }

        return _view;
      }
    }

    #endregion

    #region Title

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register( "Title", typeof( string ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( ( string )null, new PropertyChangedCallback( OnTitleChanged ) ) );

    public string Title
    {
      get
      {
        return ( string )GetValue( TitleProperty );
      }
      set
      {
        SetValue( TitleProperty, value );
      }
    }

    private static void OnTitleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnTitleChanged( e );
    }

    protected virtual void OnTitleChanged( DependencyPropertyChangedEventArgs e )
    {
      if( LayoutElement != null )
        LayoutElement.Title = ( string )e.NewValue;
    }

    #endregion

    #region IconSource

    public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register( "IconSource", typeof( ImageSource ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( ( ImageSource )null, new PropertyChangedCallback( OnIconSourceChanged ) ) );

    public ImageSource IconSource
    {
      get
      {
        return ( ImageSource )GetValue( IconSourceProperty );
      }
      set
      {
        SetValue( IconSourceProperty, value );
      }
    }

    private static void OnIconSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnIconSourceChanged( e );
    }

    protected virtual void OnIconSourceChanged( DependencyPropertyChangedEventArgs e )
    {
      if( LayoutElement != null )
        LayoutElement.IconSource = IconSource;
    }

    #endregion

    #region ContentId

    public static readonly DependencyProperty ContentIdProperty = DependencyProperty.Register( "ContentId", typeof( string ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( ( string )null, new PropertyChangedCallback( OnContentIdChanged ) ) );

    public string ContentId
    {
      get
      {
        return ( string )GetValue( ContentIdProperty );
      }
      set
      {
        SetValue( ContentIdProperty, value );
      }
    }

    private static void OnContentIdChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnContentIdChanged( e );
    }

    protected virtual void OnContentIdChanged( DependencyPropertyChangedEventArgs e )
    {
      if( LayoutElement != null )
        LayoutElement.ContentId = ( string )e.NewValue;
    }

    #endregion

    #region IsSelected

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register( "IsSelected", typeof( bool ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( ( bool )false, new PropertyChangedCallback( OnIsSelectedChanged ) ) );

    public bool IsSelected
    {
      get
      {
        return ( bool )GetValue( IsSelectedProperty );
      }
      set
      {
        SetValue( IsSelectedProperty, value );
      }
    }

    private static void OnIsSelectedChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnIsSelectedChanged( e );
    }

    protected virtual void OnIsSelectedChanged( DependencyPropertyChangedEventArgs e )
    {
      if( _isSelectedReentrantFlag.CanEnter )
      {
        using( _isSelectedReentrantFlag.Enter() )
        {
          if( LayoutElement != null )
            LayoutElement.IsSelected = ( bool )e.NewValue;
        }
      }
    }

    #endregion

    #region IsActive  

    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register( "IsActive", typeof( bool ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( ( bool )false, new PropertyChangedCallback( OnIsActiveChanged ) ) );

    public bool IsActive
    {
      get
      {
        return ( bool )GetValue( IsActiveProperty );
      }
      set
      {
        SetValue( IsActiveProperty, value );
      }
    }

    private static void OnIsActiveChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnIsActiveChanged( e );
    }

    protected virtual void OnIsActiveChanged( DependencyPropertyChangedEventArgs e )
    {
      if( _isActiveReentrantFlag.CanEnter )
      {
        using( _isActiveReentrantFlag.Enter() )
        {
          if( LayoutElement != null )
            LayoutElement.IsActive = ( bool )e.NewValue;
        }
      }
    }

    #endregion















    #region CanClose

    public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register( "CanClose", typeof( bool ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( ( bool )true, new PropertyChangedCallback( OnCanCloseChanged ) ) );

    public bool CanClose
    {
      get
      {
        return ( bool )GetValue( CanCloseProperty );
      }
      set
      {
        SetValue( CanCloseProperty, value );
      }
    }

    private static void OnCanCloseChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnCanCloseChanged( e );
    }

    protected virtual void OnCanCloseChanged( DependencyPropertyChangedEventArgs e )
    {
      if( LayoutElement != null )
        LayoutElement.CanClose = ( bool )e.NewValue;
    }

    #endregion

    #region CanFloat

    public static readonly DependencyProperty CanFloatProperty = DependencyProperty.Register( "CanFloat", typeof( bool ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( ( bool )true, new PropertyChangedCallback( OnCanFloatChanged ) ) );

    public bool CanFloat
    {
      get
      {
        return ( bool )GetValue( CanFloatProperty );
      }
      set
      {
        SetValue( CanFloatProperty, value );
      }
    }

    private static void OnCanFloatChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnCanFloatChanged( e );
    }

    protected virtual void OnCanFloatChanged( DependencyPropertyChangedEventArgs e )
    {
      if( LayoutElement != null )
        LayoutElement.CanFloat = ( bool )e.NewValue;
    }

    #endregion

    #region CloseCommand

    public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register( "CloseCommand", typeof( ICommand ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnCloseCommandChanged ), new CoerceValueCallback( CoerceCloseCommandValue ) ) );

    public ICommand CloseCommand
    {
      get
      {
        return ( ICommand )GetValue( CloseCommandProperty );
      }
      set
      {
        SetValue( CloseCommandProperty, value );
      }
    }

    private static void OnCloseCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnCloseCommandChanged( e );
    }

    protected virtual void OnCloseCommandChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceCloseCommandValue( DependencyObject d, object value )
    {
      return value;
    }

    private bool CanExecuteCloseCommand( object parameter )
    {
      return LayoutElement != null && LayoutElement.CanClose;
    }

    private void ExecuteCloseCommand( object parameter )
    {
      Close();
    }

    protected abstract void Close();


    #endregion

    #region FloatCommand
    public static readonly DependencyProperty FloatCommandProperty = DependencyProperty.Register( "FloatCommand", typeof( ICommand ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnFloatCommandChanged ), new CoerceValueCallback( CoerceFloatCommandValue ) ) );

    public ICommand FloatCommand
    {
      get
      {
        return ( ICommand )GetValue( FloatCommandProperty );
      }
      set
      {
        SetValue( FloatCommandProperty, value );
      }
    }

    private static void OnFloatCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnFloatCommandChanged( e );
    }

    protected virtual void OnFloatCommandChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceFloatCommandValue( DependencyObject d, object value )
    {
      return value;
    }

    private bool CanExecuteFloatCommand( object anchorable )
    {
      return LayoutElement != null && LayoutElement.CanFloat && LayoutElement.FindParent<LayoutFloatingWindow>() == null;
    }

    private void ExecuteFloatCommand( object parameter )
    {
      LayoutElement.Root.Manager._ExecuteFloatCommand( LayoutElement );
    }

    protected virtual void Float()
    {

    }

    #endregion








    #region DockAsDocumentCommand

    public static readonly DependencyProperty DockAsDocumentCommandProperty = DependencyProperty.Register( "DockAsDocumentCommand", typeof( ICommand ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnDockAsDocumentCommandChanged ), new CoerceValueCallback( CoerceDockAsDocumentCommandValue ) ) );

    public ICommand DockAsDocumentCommand
    {
      get
      {
        return ( ICommand )GetValue( DockAsDocumentCommandProperty );
      }
      set
      {
        SetValue( DockAsDocumentCommandProperty, value );
      }
    }

    private static void OnDockAsDocumentCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnDockAsDocumentCommandChanged( e );
    }

    protected virtual void OnDockAsDocumentCommandChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceDockAsDocumentCommandValue( DependencyObject d, object value )
    {
      return value;
    }

    protected virtual bool CanExecuteDockAsDocumentCommand()
    {
      return ( LayoutElement != null && LayoutElement.FindParent<LayoutDocumentPane>() == null );
    }

    private bool CanExecuteDockAsDocumentCommand( object parameter )
    {
      return this.CanExecuteDockAsDocumentCommand();
    }

    private void ExecuteDockAsDocumentCommand( object parameter )
    {
      LayoutElement.Root.Manager._ExecuteDockAsDocumentCommand( LayoutElement );
    }

    #endregion

    #region CloseAllButThisCommand

    public static readonly DependencyProperty CloseAllButThisCommandProperty = DependencyProperty.Register( "CloseAllButThisCommand", typeof( ICommand ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnCloseAllButThisCommandChanged ), new CoerceValueCallback( CoerceCloseAllButThisCommandValue ) ) );

    public ICommand CloseAllButThisCommand
    {
      get
      {
        return ( ICommand )GetValue( CloseAllButThisCommandProperty );
      }
      set
      {
        SetValue( CloseAllButThisCommandProperty, value );
      }
    }

    private static void OnCloseAllButThisCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnCloseAllButThisCommandChanged( e );
    }

    protected virtual void OnCloseAllButThisCommandChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceCloseAllButThisCommandValue( DependencyObject d, object value )
    {
      return value;
    }

    private bool CanExecuteCloseAllButThisCommand( object parameter )
    {
      if( LayoutElement == null )
        return false;
      var root = LayoutElement.Root;
      if( root == null )
        return false;

      return LayoutElement.Root.Manager.Layout.
          Descendents().OfType<LayoutContent>().Where( d => d != LayoutElement && ( d.Parent is LayoutDocumentPane || d.Parent is LayoutDocumentFloatingWindow ) ).Any();
    }

    private void ExecuteCloseAllButThisCommand( object parameter )
    {
      LayoutElement.Root.Manager._ExecuteCloseAllButThisCommand( LayoutElement );
    }

    #endregion

    #region CloseAllCommand

    public static readonly DependencyProperty CloseAllCommandProperty = DependencyProperty.Register( "CloseAllCommand", typeof( ICommand ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnCloseAllCommandChanged ), new CoerceValueCallback( CoerceCloseAllCommandValue ) ) );

    public ICommand CloseAllCommand
    {
      get
      {
        return ( ICommand )GetValue( CloseAllCommandProperty );
      }
      set
      {
        SetValue( CloseAllCommandProperty, value );
      }
    }

    private static void OnCloseAllCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnCloseAllCommandChanged( e );
    }

    protected virtual void OnCloseAllCommandChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceCloseAllCommandValue( DependencyObject d, object value )
    {
      return value;
    }

    private bool CanExecuteCloseAllCommand( object parameter )
    {
      if( LayoutElement == null )
        return false;
      var root = LayoutElement.Root;
      if( root == null )
        return false;

      return LayoutElement.Root.Manager.Layout.
          Descendents().OfType<LayoutContent>().Where( d => ( d.Parent is LayoutDocumentPane || d.Parent is LayoutDocumentFloatingWindow ) ).Any();
    }

    private void ExecuteCloseAllCommand( object parameter )
    {
      LayoutElement.Root.Manager._ExecuteCloseAllCommand( LayoutElement );
    }

    #endregion

    #region ActivateCommand

    public static readonly DependencyProperty ActivateCommandProperty = DependencyProperty.Register( "ActivateCommand", typeof( ICommand ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnActivateCommandChanged ), new CoerceValueCallback( CoerceActivateCommandValue ) ) );

    public ICommand ActivateCommand
    {
      get
      {
        return ( ICommand )GetValue( ActivateCommandProperty );
      }
      set
      {
        SetValue( ActivateCommandProperty, value );
      }
    }

    private static void OnActivateCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnActivateCommandChanged( e );
    }

    protected virtual void OnActivateCommandChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private static object CoerceActivateCommandValue( DependencyObject d, object value )
    {
      return value;
    }

    private bool CanExecuteActivateCommand( object parameter )
    {
      return LayoutElement != null;
    }

    private void ExecuteActivateCommand( object parameter )
    {
      LayoutElement.Root.Manager._ExecuteContentActivateCommand( LayoutElement );
    }

    #endregion

    #region NewVerticalTabGroupCommand

    public static readonly DependencyProperty NewVerticalTabGroupCommandProperty = DependencyProperty.Register( "NewVerticalTabGroupCommand", typeof( ICommand ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( ( ICommand )null, new PropertyChangedCallback( OnNewVerticalTabGroupCommandChanged ) ) );

    public ICommand NewVerticalTabGroupCommand
    {
      get
      {
        return ( ICommand )GetValue( NewVerticalTabGroupCommandProperty );
      }
      set
      {
        SetValue( NewVerticalTabGroupCommandProperty, value );
      }
    }

    private static void OnNewVerticalTabGroupCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnNewVerticalTabGroupCommandChanged( e );
    }

    protected virtual void OnNewVerticalTabGroupCommandChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private bool CanExecuteNewVerticalTabGroupCommand( object parameter )
    {
      if( LayoutElement == null )
        return false;
      var layoutDocument = LayoutElement as LayoutDocument;
      if( ( layoutDocument != null ) && !layoutDocument.CanMove )
        return false;
      var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
      var parentDocumentPane = LayoutElement.Parent as LayoutDocumentPane;
      return ( ( parentDocumentGroup == null ||
          parentDocumentGroup.ChildrenCount == 1 ||
          parentDocumentGroup.Root.Manager.AllowMixedOrientation ||
          parentDocumentGroup.Orientation == System.Windows.Controls.Orientation.Horizontal ) &&
          parentDocumentPane != null &&
          parentDocumentPane.ChildrenCount > 1 );
    }

    private void ExecuteNewVerticalTabGroupCommand( object parameter )
    {
      var layoutElement = LayoutElement;
      var parentDocumentGroup = layoutElement.FindParent<LayoutDocumentPaneGroup>();
      var parentDocumentPane = layoutElement.Parent as LayoutDocumentPane;

      if( parentDocumentGroup == null )
      {
        var grandParent = parentDocumentPane.Parent as ILayoutContainer;
        parentDocumentGroup = new LayoutDocumentPaneGroup() { Orientation = System.Windows.Controls.Orientation.Horizontal };
        grandParent.ReplaceChild( parentDocumentPane, parentDocumentGroup );
        parentDocumentGroup.Children.Add( parentDocumentPane );
      }
      parentDocumentGroup.Orientation = System.Windows.Controls.Orientation.Horizontal;
      int indexOfParentPane = parentDocumentGroup.IndexOfChild( parentDocumentPane );
      parentDocumentGroup.InsertChildAt( indexOfParentPane + 1, new LayoutDocumentPane( layoutElement ) );
      layoutElement.IsActive = true;
      layoutElement.Root.CollectGarbage();
    }
    #endregion

    #region NewHorizontalTabGroupCommand

    public static readonly DependencyProperty NewHorizontalTabGroupCommandProperty = DependencyProperty.Register( "NewHorizontalTabGroupCommand", typeof( ICommand ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( ( ICommand )null, new PropertyChangedCallback( OnNewHorizontalTabGroupCommandChanged ) ) );

    public ICommand NewHorizontalTabGroupCommand
    {
      get
      {
        return ( ICommand )GetValue( NewHorizontalTabGroupCommandProperty );
      }
      set
      {
        SetValue( NewHorizontalTabGroupCommandProperty, value );
      }
    }

    private static void OnNewHorizontalTabGroupCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnNewHorizontalTabGroupCommandChanged( e );
    }

    protected virtual void OnNewHorizontalTabGroupCommandChanged( DependencyPropertyChangedEventArgs e )
    {
    }


    private bool CanExecuteNewHorizontalTabGroupCommand( object parameter )
    {
      if( LayoutElement == null )
        return false;
      var layoutDocument = LayoutElement as LayoutDocument;
      if( ( layoutDocument != null ) && !layoutDocument.CanMove )
        return false;
      var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
      var parentDocumentPane = LayoutElement.Parent as LayoutDocumentPane;
      return ( ( parentDocumentGroup == null ||
          parentDocumentGroup.ChildrenCount == 1 ||
          parentDocumentGroup.Root.Manager.AllowMixedOrientation ||
          parentDocumentGroup.Orientation == System.Windows.Controls.Orientation.Vertical ) &&
          parentDocumentPane != null &&
          parentDocumentPane.ChildrenCount > 1 );
    }

    private void ExecuteNewHorizontalTabGroupCommand( object parameter )
    {
      var layoutElement = LayoutElement;
      var parentDocumentGroup = layoutElement.FindParent<LayoutDocumentPaneGroup>();
      var parentDocumentPane = layoutElement.Parent as LayoutDocumentPane;

      if( parentDocumentGroup == null )
      {
        var grandParent = parentDocumentPane.Parent as ILayoutContainer;
        parentDocumentGroup = new LayoutDocumentPaneGroup() { Orientation = System.Windows.Controls.Orientation.Vertical };
        grandParent.ReplaceChild( parentDocumentPane, parentDocumentGroup );
        parentDocumentGroup.Children.Add( parentDocumentPane );
      }
      parentDocumentGroup.Orientation = System.Windows.Controls.Orientation.Vertical;
      int indexOfParentPane = parentDocumentGroup.IndexOfChild( parentDocumentPane );
      parentDocumentGroup.InsertChildAt( indexOfParentPane + 1, new LayoutDocumentPane( layoutElement ) );
      layoutElement.IsActive = true;
      layoutElement.Root.CollectGarbage();
    }
    #endregion

    #region MoveToNextTabGroupCommand

    public static readonly DependencyProperty MoveToNextTabGroupCommandProperty = DependencyProperty.Register( "MoveToNextTabGroupCommand", typeof( ICommand ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( ( ICommand )null, new PropertyChangedCallback( OnMoveToNextTabGroupCommandChanged ) ) );

    public ICommand MoveToNextTabGroupCommand
    {
      get
      {
        return ( ICommand )GetValue( MoveToNextTabGroupCommandProperty );
      }
      set
      {
        SetValue( MoveToNextTabGroupCommandProperty, value );
      }
    }

    private static void OnMoveToNextTabGroupCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnMoveToNextTabGroupCommandChanged( e );
    }

    protected virtual void OnMoveToNextTabGroupCommandChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private bool CanExecuteMoveToNextTabGroupCommand( object parameter )
    {
      if( LayoutElement == null )
        return false;

      var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
      var parentDocumentPane = LayoutElement.Parent as LayoutDocumentPane;
      return ( parentDocumentGroup != null &&
          parentDocumentPane != null &&
          parentDocumentGroup.ChildrenCount > 1 &&
          parentDocumentGroup.IndexOfChild( parentDocumentPane ) < parentDocumentGroup.ChildrenCount - 1 &&
          parentDocumentGroup.Children[ parentDocumentGroup.IndexOfChild( parentDocumentPane ) + 1 ] is LayoutDocumentPane );
    }

    private void ExecuteMoveToNextTabGroupCommand( object parameter )
    {
      var layoutElement = LayoutElement;
      var parentDocumentGroup = layoutElement.FindParent<LayoutDocumentPaneGroup>();
      var parentDocumentPane = layoutElement.Parent as LayoutDocumentPane;
      int indexOfParentPane = parentDocumentGroup.IndexOfChild( parentDocumentPane );
      var nextDocumentPane = parentDocumentGroup.Children[ indexOfParentPane + 1 ] as LayoutDocumentPane;
      nextDocumentPane.InsertChildAt( 0, layoutElement );
      layoutElement.IsActive = true;
      layoutElement.Root.CollectGarbage();
    }

    #endregion

    #region MoveToPreviousTabGroupCommand

    public static readonly DependencyProperty MoveToPreviousTabGroupCommandProperty = DependencyProperty.Register( "MoveToPreviousTabGroupCommand", typeof( ICommand ), typeof( LayoutItem ),
            new FrameworkPropertyMetadata( ( ICommand )null, new PropertyChangedCallback( OnMoveToPreviousTabGroupCommandChanged ) ) );

    public ICommand MoveToPreviousTabGroupCommand
    {
      get
      {
        return ( ICommand )GetValue( MoveToPreviousTabGroupCommandProperty );
      }
      set
      {
        SetValue( MoveToPreviousTabGroupCommandProperty, value );
      }
    }

    private static void OnMoveToPreviousTabGroupCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )d ).OnMoveToPreviousTabGroupCommandChanged( e );
    }

    protected virtual void OnMoveToPreviousTabGroupCommandChanged( DependencyPropertyChangedEventArgs e )
    {
    }

    private bool CanExecuteMoveToPreviousTabGroupCommand( object parameter )
    {
      if( LayoutElement == null )
        return false;
      var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
      var parentDocumentPane = LayoutElement.Parent as LayoutDocumentPane;
      return ( parentDocumentGroup != null &&
          parentDocumentPane != null &&
          parentDocumentGroup.ChildrenCount > 1 &&
          parentDocumentGroup.IndexOfChild( parentDocumentPane ) > 0 &&
          parentDocumentGroup.Children[ parentDocumentGroup.IndexOfChild( parentDocumentPane ) - 1 ] is LayoutDocumentPane );
    }

    private void ExecuteMoveToPreviousTabGroupCommand( object parameter )
    {
      var layoutElement = LayoutElement;
      var parentDocumentGroup = layoutElement.FindParent<LayoutDocumentPaneGroup>();
      var parentDocumentPane = layoutElement.Parent as LayoutDocumentPane;
      int indexOfParentPane = parentDocumentGroup.IndexOfChild( parentDocumentPane );
      var nextDocumentPane = parentDocumentGroup.Children[ indexOfParentPane - 1 ] as LayoutDocumentPane;
      nextDocumentPane.InsertChildAt( 0, layoutElement );
      layoutElement.IsActive = true;
      layoutElement.Root.CollectGarbage();
    }
    #endregion

    #endregion

    #region Internal Methods

    protected virtual void InitDefaultCommands()
    {
      _defaultCloseCommand = new RelayCommand( ( p ) => ExecuteCloseCommand( p ), ( p ) => CanExecuteCloseCommand( p ) );
      _defaultFloatCommand = new RelayCommand( ( p ) => ExecuteFloatCommand( p ), ( p ) => CanExecuteFloatCommand( p ) );
      _defaultDockAsDocumentCommand = new RelayCommand( ( p ) => ExecuteDockAsDocumentCommand( p ), ( p ) => CanExecuteDockAsDocumentCommand( p ) );
      _defaultCloseAllButThisCommand = new RelayCommand( ( p ) => ExecuteCloseAllButThisCommand( p ), ( p ) => CanExecuteCloseAllButThisCommand( p ) );
      _defaultCloseAllCommand = new RelayCommand( ( p ) => ExecuteCloseAllCommand( p ), ( p ) => CanExecuteCloseAllCommand( p ) );
      _defaultActivateCommand = new RelayCommand( ( p ) => ExecuteActivateCommand( p ), ( p ) => CanExecuteActivateCommand( p ) );
      _defaultNewVerticalTabGroupCommand = new RelayCommand( ( p ) => ExecuteNewVerticalTabGroupCommand( p ), ( p ) => CanExecuteNewVerticalTabGroupCommand( p ) );
      _defaultNewHorizontalTabGroupCommand = new RelayCommand( ( p ) => ExecuteNewHorizontalTabGroupCommand( p ), ( p ) => CanExecuteNewHorizontalTabGroupCommand( p ) );
      _defaultMoveToNextTabGroupCommand = new RelayCommand( ( p ) => ExecuteMoveToNextTabGroupCommand( p ), ( p ) => CanExecuteMoveToNextTabGroupCommand( p ) );
      _defaultMoveToPreviousTabGroupCommand = new RelayCommand( ( p ) => ExecuteMoveToPreviousTabGroupCommand( p ), ( p ) => CanExecuteMoveToPreviousTabGroupCommand( p ) );
    }

    protected virtual void ClearDefaultCommands()
    {
      _defaultCloseCommand = null;
      _defaultFloatCommand = null;
      _defaultDockAsDocumentCommand = null;
      _defaultCloseAllButThisCommand = null;
      _defaultCloseAllCommand = null;
      _defaultActivateCommand = null;
      _defaultNewVerticalTabGroupCommand = null;
      _defaultNewHorizontalTabGroupCommand = null;
      _defaultMoveToNextTabGroupCommand = null;
      _defaultMoveToPreviousTabGroupCommand = null;
    }

    protected virtual void ClearDefaultBindings()
    {
      if( this.CloseCommand == _defaultCloseCommand )
      {
        BindingOperations.ClearBinding( this, CloseCommandProperty );
        this.CloseCommand = null;
      }
      if( this.FloatCommand == _defaultFloatCommand )
      {
        BindingOperations.ClearBinding( this, FloatCommandProperty );
        this.FloatCommand = null;
      }
      if( this.DockAsDocumentCommand == _defaultDockAsDocumentCommand )
      {
        BindingOperations.ClearBinding( this, DockAsDocumentCommandProperty );
        this.DockAsDocumentCommand = null;
      }
      if( this.CloseAllButThisCommand == _defaultCloseAllButThisCommand )
      {
        BindingOperations.ClearBinding( this, CloseAllButThisCommandProperty );
        this.CloseAllButThisCommand = null;
      }
      if( this.CloseAllCommand == _defaultCloseAllCommand )
      {
        BindingOperations.ClearBinding( this, CloseAllCommandProperty );
        this.CloseAllCommand = null;
      }
      if( this.ActivateCommand == _defaultActivateCommand )
      {
        BindingOperations.ClearBinding( this, ActivateCommandProperty );
        this.ActivateCommand = null;
      }
      if( this.NewVerticalTabGroupCommand == _defaultNewVerticalTabGroupCommand )
      {
        BindingOperations.ClearBinding( this, NewVerticalTabGroupCommandProperty );
        this.NewVerticalTabGroupCommand = null;
      }
      if( this.NewHorizontalTabGroupCommand == _defaultNewHorizontalTabGroupCommand )
      {
        BindingOperations.ClearBinding( this, NewHorizontalTabGroupCommandProperty );
        this.NewHorizontalTabGroupCommand = null;
      }
      if( this.MoveToNextTabGroupCommand == _defaultMoveToNextTabGroupCommand )
      {
        BindingOperations.ClearBinding( this, MoveToNextTabGroupCommandProperty );
        this.MoveToNextTabGroupCommand = null;
      }
      if( this.MoveToPreviousTabGroupCommand == _defaultMoveToPreviousTabGroupCommand )
      {
        BindingOperations.ClearBinding( this, MoveToPreviousTabGroupCommandProperty );
        this.MoveToPreviousTabGroupCommand = null;
      }
    }

    protected virtual void SetDefaultBindings()
    {
      if( this.CloseCommand == null )
      {
        this.SetCurrentValue( LayoutItem.CloseCommandProperty, _defaultCloseCommand );
      }
      if( this.FloatCommand == null )
      {
        this.SetCurrentValue( LayoutItem.FloatCommandProperty, _defaultFloatCommand );
      }
      if( this.DockAsDocumentCommand == null )
      {
        this.SetCurrentValue( LayoutItem.DockAsDocumentCommandProperty, _defaultDockAsDocumentCommand );
      }
      if( this.CloseAllButThisCommand == null )
      {
        this.SetCurrentValue( LayoutItem.CloseAllButThisCommandProperty, _defaultCloseAllButThisCommand );
      }
      if( this.CloseAllCommand == null )
      {
        this.SetCurrentValue( LayoutItem.CloseAllCommandProperty, _defaultCloseAllCommand );
      }
      if( this.ActivateCommand == null )
      {
        this.SetCurrentValue( LayoutItem.ActivateCommandProperty, _defaultActivateCommand );
      }
      if( this.NewVerticalTabGroupCommand == null )
      {
        this.SetCurrentValue( LayoutItem.NewVerticalTabGroupCommandProperty, _defaultNewVerticalTabGroupCommand );
      }
      if( this.NewHorizontalTabGroupCommand == null )
      {
        this.SetCurrentValue( LayoutItem.NewHorizontalTabGroupCommandProperty, _defaultNewHorizontalTabGroupCommand );
      }
      if( this.MoveToNextTabGroupCommand == null )
      {
        this.SetCurrentValue( LayoutItem.MoveToNextTabGroupCommandProperty, _defaultMoveToNextTabGroupCommand );
      }
      if( this.MoveToPreviousTabGroupCommand == null )
      {
        this.SetCurrentValue( LayoutItem.MoveToPreviousTabGroupCommandProperty, _defaultMoveToPreviousTabGroupCommand );
      }

      this.SetCurrentValue( LayoutItem.IsSelectedProperty, LayoutElement.IsSelected );
      this.SetCurrentValue( LayoutItem.IsActiveProperty, LayoutElement.IsActive );
      this.SetCurrentValue( LayoutItem.CanCloseProperty, LayoutElement.CanClose );
    }

    protected virtual void OnVisibilityChanged()
    {
      if( LayoutElement != null && Visibility == System.Windows.Visibility.Collapsed )
        LayoutElement.Close();
    }

    internal virtual void Attach( LayoutContent model )
    {
      this.LayoutElement = model;
      this.Model = model.Content;

      this.InitDefaultCommands();

      this.LayoutElement.IsSelectedChanged += new EventHandler( LayoutElement_IsSelectedChanged );
      this.LayoutElement.IsActiveChanged += new EventHandler( LayoutElement_IsActiveChanged );

      this.DataContext = this;
    }

    internal virtual void Detach()
    {
      this.ClearDefaultCommands();

      this.LayoutElement.IsSelectedChanged -= new EventHandler( LayoutElement_IsSelectedChanged );
      this.LayoutElement.IsActiveChanged -= new EventHandler( LayoutElement_IsActiveChanged );

      this.LayoutElement = null;
      this.Model = null;

      this.DataContext = null;
    }

    internal void _ClearDefaultBindings()
    {
      ClearDefaultBindings();
    }

    internal void _SetDefaultBindings()
    {
      SetDefaultBindings();
    }

    internal bool IsViewExists()
    {
      return ( _view != null );
    }

    #endregion

    #region Private Methods

    private void LayoutElement_IsActiveChanged( object sender, EventArgs e )
    {
      if( _isActiveReentrantFlag.CanEnter )
      {
        using( _isActiveReentrantFlag.Enter() )
        {
          IsActive = LayoutElement.IsActive;
        }
      }
    }

    private void LayoutElement_IsSelectedChanged( object sender, EventArgs e )
    {
      if( _isSelectedReentrantFlag.CanEnter )
      {
        using( _isSelectedReentrantFlag.Enter() )
        {
          IsSelected = LayoutElement.IsSelected;
        }
      }
    }

    private static void OnToolTipChanged( DependencyObject s, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )s ).OnToolTipChanged();
    }

    private void OnToolTipChanged()
    {
      if( LayoutElement != null )
        LayoutElement.ToolTip = ToolTip;
    }

    private static void OnVisibilityChanged( DependencyObject s, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutItem )s ).OnVisibilityChanged();
    }

    #endregion
  }
}
