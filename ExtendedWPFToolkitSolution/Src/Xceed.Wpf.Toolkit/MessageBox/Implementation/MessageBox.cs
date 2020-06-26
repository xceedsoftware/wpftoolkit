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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using System.Text;
using System.Security.Permissions;
using System.Security;
using Xceed.Wpf.Toolkit.Primitives;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Interop;

namespace Xceed.Wpf.Toolkit
{
  [TemplateVisualState( Name = VisualStates.OK, GroupName = VisualStates.MessageBoxButtonsGroup )]
  [TemplateVisualState( Name = VisualStates.OKCancel, GroupName = VisualStates.MessageBoxButtonsGroup )]
  [TemplateVisualState( Name = VisualStates.YesNo, GroupName = VisualStates.MessageBoxButtonsGroup )]
  [TemplateVisualState( Name = VisualStates.YesNoCancel, GroupName = VisualStates.MessageBoxButtonsGroup )]
  [TemplatePart( Name = PART_CancelButton, Type = typeof( Button ) )]
  [TemplatePart( Name = PART_NoButton, Type = typeof( Button ) )]
  [TemplatePart( Name = PART_OkButton, Type = typeof( Button ) )]
  [TemplatePart( Name = PART_YesButton, Type = typeof( Button ) )]
  [TemplatePart( Name = PART_WindowControl, Type = typeof( WindowControl ) )]
  public class MessageBox : WindowControl
  {
    private const string PART_CancelButton = "PART_CancelButton";
    private const string PART_NoButton = "PART_NoButton";
    private const string PART_OkButton = "PART_OkButton";
    private const string PART_YesButton = "PART_YesButton";
    private const string PART_CloseButton = "PART_CloseButton";
    private const string PART_WindowControl = "PART_WindowControl";

    #region Private Members

    /// <summary>
    /// Tracks the MessageBoxButon value passed into the InitializeContainer method
    /// </summary>
    private MessageBoxButton _button = MessageBoxButton.OK;

    /// <summary>
    /// Tracks the MessageBoxResult to set as the default and focused button
    /// </summary>
    private MessageBoxResult _defaultResult = MessageBoxResult.None;

    /// <summary>
    /// Will contain the result when the messagebox is shown inside a WindowContainer
    /// </summary>
    private MessageBoxResult _dialogResult = MessageBoxResult.None;

    /// <summary>
    /// Tracks the owner of the MessageBox
    /// </summary>
    private Window _owner;
    private IntPtr _ownerHandle;

    private WindowControl _windowControl;

    #endregion //Private Members

    #region Constructors

    static MessageBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( MessageBox ), new FrameworkPropertyMetadata( typeof( MessageBox ) ) );
    }

    public MessageBox()
    {
      this.Visibility = Visibility.Collapsed;
      this.InitHandlers();
      this.IsVisibleChanged += this.MessageBox_IsVisibleChanged;
    }

    #endregion //Constructors

    #region Properties

    #region Protected Properties

    protected Window Container
    {
      get { return ( this.Parent as Window ); }
    }

    #endregion //Protected Properties

    #region Dependency Properties

    #region ButtonRegionBackground

    public static readonly DependencyProperty ButtonRegionBackgroundProperty = DependencyProperty.Register( "ButtonRegionBackground", typeof( Brush ), typeof( MessageBox ), new PropertyMetadata( null ) );
    public Brush ButtonRegionBackground
    {
      get
      {
        return ( Brush )GetValue( ButtonRegionBackgroundProperty );
      }
      set
      {
        SetValue( ButtonRegionBackgroundProperty, value );
      }
    }

    #endregion //ButtonRegionBackground

    #region CancelButtonContent

    public static readonly DependencyProperty CancelButtonContentProperty = DependencyProperty.Register( "CancelButtonContent", typeof( object ), typeof( MessageBox ), new UIPropertyMetadata( "Cancel" ) );
    public object CancelButtonContent
    {
      get
      {
        return ( object )GetValue( CancelButtonContentProperty );
      }
      set
      {
        SetValue( CancelButtonContentProperty, value );
      }
    }

    #endregion //CancelButtonContent

    #region CancelButtonStyle

    public static readonly DependencyProperty CancelButtonStyleProperty = DependencyProperty.Register( "CancelButtonStyle", typeof( Style ), typeof( MessageBox ), new PropertyMetadata( null ) );
    public Style CancelButtonStyle
    {
      get
      {
        return ( Style )GetValue( CancelButtonStyleProperty );
      }
      set
      {
        SetValue( CancelButtonStyleProperty, value );
      }
    }

    #endregion //CancelButtonStyle

    #region ImageSource

    public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register( "ImageSource", typeof( ImageSource ), typeof( MessageBox ), new UIPropertyMetadata( default( ImageSource ) ) );
    public ImageSource ImageSource
    {
      get
      {
        return ( ImageSource )GetValue( ImageSourceProperty );
      }
      set
      {
        SetValue( ImageSourceProperty, value );
      }
    }

    #endregion //ImageSource

    #region OkButtonContent

    public static readonly DependencyProperty OkButtonContentProperty = DependencyProperty.Register( "OkButtonContent", typeof( object ), typeof( MessageBox ), new UIPropertyMetadata( "OK" ) );
    public object OkButtonContent
    {
      get
      {
        return ( object )GetValue( OkButtonContentProperty );
      }
      set
      {
        SetValue( OkButtonContentProperty, value );
      }
    }

    #endregion //OkButtonContent

    #region OkButtonStyle

    public static readonly DependencyProperty OkButtonStyleProperty = DependencyProperty.Register( "OkButtonStyle", typeof( Style ), typeof( MessageBox ), new PropertyMetadata( null ) );
    public Style OkButtonStyle
    {
      get
      {
        return ( Style )GetValue( OkButtonStyleProperty );
      }
      set
      {
        SetValue( OkButtonStyleProperty, value );
      }
    }

    #endregion //OkButtonStyle

    #region MessageBoxResult

    /// <summary>
    /// Gets the MessageBox result, which is set when the "Closed" event is raised.
    /// </summary>
    public MessageBoxResult MessageBoxResult
    {
      get { return _dialogResult; }
    }

    #endregion //MessageBoxResult

    #region NoButtonContent

    public static readonly DependencyProperty NoButtonContentProperty = DependencyProperty.Register( "NoButtonContent", typeof( object ), typeof( MessageBox ), new UIPropertyMetadata( "No" ) );
    public object NoButtonContent
    {
      get
      {
        return ( object )GetValue( NoButtonContentProperty );
      }
      set
      {
        SetValue( NoButtonContentProperty, value );
      }
    }

    #endregion //NoButtonContent

    #region NoButtonStyle

    public static readonly DependencyProperty NoButtonStyleProperty = DependencyProperty.Register( "NoButtonStyle", typeof( Style ), typeof( MessageBox ), new PropertyMetadata( null ) );
    public Style NoButtonStyle
    {
      get
      {
        return ( Style )GetValue( NoButtonStyleProperty );
      }
      set
      {
        SetValue( NoButtonStyleProperty, value );
      }
    }

    #endregion //NoButtonStyle

    #region Text

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof( string ), typeof( MessageBox ), new UIPropertyMetadata( String.Empty ) );
    public string Text
    {
      get
      {
        return ( string )GetValue( TextProperty );
      }
      set
      {
        SetValue( TextProperty, value );
      }
    }

    #endregion //Text

    #region YesButtonContent

    public static readonly DependencyProperty YesButtonContentProperty = DependencyProperty.Register( "YesButtonContent", typeof( object ), typeof( MessageBox ), new UIPropertyMetadata( "Yes" ) );
    public object YesButtonContent
    {
      get
      {
        return ( object )GetValue( YesButtonContentProperty );
      }
      set
      {
        SetValue( YesButtonContentProperty, value );
      }
    }

    #endregion //YesButtonContent

    #region YesButtonStyle

    public static readonly DependencyProperty YesButtonStyleProperty = DependencyProperty.Register( "YesButtonStyle", typeof( Style ), typeof( MessageBox ), new PropertyMetadata( null ) );
    public Style YesButtonStyle
    {
      get
      {
        return ( Style )GetValue( YesButtonStyleProperty );
      }
      set
      {
        SetValue( YesButtonStyleProperty, value );
      }
    }

    #endregion //YesButtonStyle

    #endregion //Dependency Properties

    #endregion //Properties

    #region Base Class Overrides

    internal override bool AllowPublicIsActiveChange
    {
      get { return false; }
    }

    /// <summary>
    /// Overrides the OnApplyTemplate method.
    /// </summary>
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( _windowControl != null )
      {
        _windowControl.HeaderDragDelta -= ( o, e ) => this.OnHeaderDragDelta( e );
        _windowControl.HeaderIconDoubleClicked -= ( o, e ) => this.OnHeaderIconDoubleClicked( e );
        _windowControl.CloseButtonClicked -= ( o, e ) => this.OnCloseButtonClicked( e );
      }
      _windowControl = this.GetTemplateChild( PART_WindowControl ) as WindowControl;
      if( _windowControl != null )
      {
        _windowControl.HeaderDragDelta += ( o, e ) => this.OnHeaderDragDelta( e );
        _windowControl.HeaderIconDoubleClicked += ( o, e ) => this.OnHeaderIconDoubleClicked( e );
        _windowControl.CloseButtonClicked += ( o, e ) => this.OnCloseButtonClicked( e );
      }
      this.UpdateBlockMouseInputsPanel();

      ChangeVisualState( _button.ToString(), true );

      Button closeButton = GetMessageBoxButton( PART_CloseButton );
      if( closeButton != null )
        closeButton.IsEnabled = !object.Equals( _button, MessageBoxButton.YesNo );

      Button okButton = GetMessageBoxButton( PART_OkButton );
      if( okButton != null )
        okButton.IsCancel = object.Equals( _button, MessageBoxButton.OK );

      SetDefaultResult();
    }

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      base.OnPreviewKeyDown( e );

      // Prevent MenuItem shortcuts while MessageBox is active.
      if( Keyboard.IsKeyDown( Key.LeftAlt ) || Keyboard.IsKeyDown( Key.RightAlt ) )
      {
        e.Handled = true;
      }
    }

    protected override object OnCoerceCloseButtonVisibility( Visibility newValue )
    {
      if( newValue != Visibility.Visible )
        throw new InvalidOperationException( "Close button on MessageBox is always Visible." );
      return newValue;
    }

    protected override object OnCoerceWindowStyle( WindowStyle newValue )
    {
      if( newValue != WindowStyle.SingleBorderWindow )
        throw new InvalidOperationException( "Window style on MessageBox is not available." );
      return newValue;
    }

    internal override void UpdateBlockMouseInputsPanel()
    {
      if( _windowControl != null )
      {
        _windowControl.IsBlockMouseInputsPanelActive = this.IsBlockMouseInputsPanelActive;
      }
    }




    #endregion //Base Class Overrides

    #region Methods

    #region Public Static

    #region Show with Window as Owner


    /// <summary>
    /// Displays a message box that has a message and that returns a result.
    /// </summary>
    /// <param name="messageText">A System.String that specifies the text to display.</param>
    /// <param name="messageBoxStyle">A Style that will be applied to the MessageBox instance.</param>
    /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
    public static MessageBoxResult Show( string messageText )
    {
      return Show( messageText, string.Empty, MessageBoxButton.OK, (Style)null );
    }

    /// <summary>
    /// Displays a message box that has a message and that returns a result.
    /// </summary>
    /// <param name="owner">A System.Windows.Window that represents the owner of the MessageBox</param>
    /// <param name="messageText">A System.String that specifies the text to display.</param>
    /// <param name="messageBoxStyle">A Style that will be applied to the MessageBox instance.</param>
    /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
    public static MessageBoxResult Show( Window owner, string messageText )
    {
      return Show( owner, messageText, string.Empty, MessageBoxButton.OK, (Style) null );
    }

    /// <summary>
    /// Displays a message box that has a message and title bar caption; and that returns a result.
    /// </summary>
    /// <param name="messageText">A System.String that specifies the text to display.</param>
    /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
    /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
    public static MessageBoxResult Show( string messageText, string caption )
    {
        return Show(messageText, caption, MessageBoxButton.OK, (Style)null);
    }

    public static MessageBoxResult Show( Window owner, string messageText, string caption )
    {
        return Show(owner, messageText, caption, (Style)null);
    }

    public static MessageBoxResult Show( Window owner, string messageText, string caption, Style messageBoxStyle )
    {
      return Show( owner, messageText, caption, MessageBoxButton.OK, messageBoxStyle );
    }

    public static MessageBoxResult Show( string messageText, string caption, MessageBoxButton button )
    {
        return Show(messageText, caption, button, (Style)null);
    }

    /// <summary>
    /// Displays a message box that has a message and that returns a result.
    /// </summary>
    /// <param name="messageText">A System.String that specifies the text to display.</param>
    /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
    /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
    /// <param name="messageBoxStyle">A Style that will be applied to the MessageBox instance.</param>
    /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
    public static MessageBoxResult Show( string messageText, string caption, MessageBoxButton button, Style messageBoxStyle )
    {
      return ShowCore( null, IntPtr.Zero, messageText, caption, button, MessageBoxImage.None, MessageBoxResult.None, messageBoxStyle );
    }


    public static MessageBoxResult Show( Window owner, string messageText, string caption, MessageBoxButton button )
    {
        return Show(owner, messageText, caption, button, (Style)null);
    }

    public static MessageBoxResult Show( Window owner, string messageText, string caption, MessageBoxButton button, Style messageBoxStyle )
    {
      return ShowCore( owner, IntPtr.Zero, messageText, caption, button, MessageBoxImage.None, MessageBoxResult.None, messageBoxStyle );
    }


    public static MessageBoxResult Show( string messageText, string caption, MessageBoxButton button, MessageBoxImage icon )
    {
        return Show(messageText, caption, button, icon, (Style)null);
    }

    /// <summary>
    /// Displays a message box that has a message and that returns a result.
    /// </summary>
    /// <param name="messageText">A System.String that specifies the text to display.</param>
    /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
    /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
    /// <param name="image"> A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
    /// <param name="messageBoxStyle">A Style that will be applied to the MessageBox instance.</param>
    /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
    public static MessageBoxResult Show( string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, Style messageBoxStyle )
    {
      return ShowCore( null, IntPtr.Zero, messageText, caption, button, icon, MessageBoxResult.None, messageBoxStyle );
    }

    public static MessageBoxResult Show( Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon )
    {
        return Show(owner, messageText, caption, button, icon, (Style)null);
    }

    public static MessageBoxResult Show( Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, Style messageBoxStyle )
    {
      return ShowCore( owner, IntPtr.Zero, messageText, caption, button, icon, MessageBoxResult.None, messageBoxStyle );
    }


    public static MessageBoxResult Show( string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult )
    {
        return Show(messageText, caption, button, icon, defaultResult, (Style)null);
    }
    /// <summary>
    /// Displays a message box that has a message and that returns a result.
    /// </summary>
    /// <param name="messageText">A System.String that specifies the text to display.</param>
    /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
    /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
    /// <param name="image"> A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
    /// <param name="defaultResult">A System.Windows.MessageBoxResult value that specifies the default result of the MessageBox.</param>
    /// <param name="messageBoxStyle">A Style that will be applied to the MessageBox instance.</param>
    /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
    public static MessageBoxResult Show( string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle )
    {
      return ShowCore( null, IntPtr.Zero, messageText, caption, button, icon, defaultResult, messageBoxStyle );
    }

    public static MessageBoxResult Show( Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult )
    {
        return Show(owner, messageText, caption, button, icon, defaultResult, (Style) null);
    }


    public static MessageBoxResult Show( Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle )
    {
      return ShowCore( owner, IntPtr.Zero, messageText, caption, button, icon, defaultResult, messageBoxStyle );
    }

    #endregion //Show with Window as Owner

    #region Show with Window handle as Owner

    public static MessageBoxResult Show( IntPtr ownerWindowHandle, string messageText )
    {
      return Show( ownerWindowHandle, messageText, string.Empty, MessageBoxButton.OK, ( Style )null );
    }

    public static MessageBoxResult Show( IntPtr ownerWindowHandle, string messageText, string caption )
    {
      return Show( ownerWindowHandle, messageText, caption, ( Style )null );
    }

    public static MessageBoxResult Show( IntPtr ownerWindowHandle, string messageText, string caption, Style messageBoxStyle )
    {
      return Show( ownerWindowHandle, messageText, caption, MessageBoxButton.OK, messageBoxStyle );
    }

    public static MessageBoxResult Show( IntPtr ownerWindowHandle, string messageText, string caption, MessageBoxButton button )
    {
      return Show( ownerWindowHandle, messageText, caption, button, ( Style )null );
    }

    public static MessageBoxResult Show( IntPtr ownerWindowHandle, string messageText, string caption, MessageBoxButton button, Style messageBoxStyle )
    {
      return ShowCore( null, ownerWindowHandle, messageText, caption, button, MessageBoxImage.None, MessageBoxResult.None, messageBoxStyle );
    }

    public static MessageBoxResult Show( IntPtr ownerWindowHandle, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon )
    {
      return Show( ownerWindowHandle, messageText, caption, button, icon, ( Style )null );
    }

    public static MessageBoxResult Show( IntPtr ownerWindowHandle, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, Style messageBoxStyle )
    {
      return ShowCore( null, ownerWindowHandle, messageText, caption, button, icon, MessageBoxResult.None, messageBoxStyle );
    }

    public static MessageBoxResult Show( IntPtr ownerWindowHandle, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult )
    {
      return Show( ownerWindowHandle, messageText, caption, button, icon, defaultResult, ( Style )null );
    }

    public static MessageBoxResult Show( IntPtr ownerWindowHandle, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle )
    {
      return ShowCore( null, ownerWindowHandle, messageText, caption, button, icon, defaultResult, messageBoxStyle );
    }

    #endregion //Show with Window handle as Owner

    #endregion //Public Static

    #region Public Methods
    /// <summary>
    /// Displays this message box when embedded in a WindowContainer parent.
    /// Note that this call is not blocking and that you must register to the Closed event in order to handle the dialog result, if any.
    /// </summary>
    public void ShowMessageBox()
    {
      if( this.Container != null || this.Parent == null )
        throw new InvalidOperationException(
          "This method is not intended to be called while displaying a MessageBox outside of a WindowContainer. Use ShowDialog() instead in that case." );

      if( !( this.Parent is WindowContainer ) )
        throw new InvalidOperationException(
          "The MessageBox instance is not intended to be displayed in a container other than a WindowContainer." );

      _dialogResult = System.Windows.MessageBoxResult.None;
      this.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Displays this message box when embedded in a WindowContainer parent.
    /// Note that this call is not blocking and that you must register to the Closed event in order to handle the dialog result, if any.
    /// </summary>
    public void ShowMessageBox( string messageText )
    {
      this.ShowMessageBoxCore( messageText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None );
    }

    /// <summary>
    /// Displays this message box when embedded in a WindowContainer parent.
    /// Note that this call is not blocking and that you must register to the Closed event in order to handle the dialog result, if any.
    /// </summary>
    public void ShowMessageBox( string messageText, string caption )
    {
      this.ShowMessageBoxCore( messageText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None );
    }

    /// <summary>
    /// Displays this message box when embedded in a WindowContainer parent.
    /// Note that this call is not blocking and that you must register to the Closed event in order to handle the dialog result, if any.
    /// </summary>
    public void ShowMessageBox( string messageText, string caption, MessageBoxButton button )
    {
      this.ShowMessageBoxCore( messageText, caption, button, MessageBoxImage.None, MessageBoxResult.None );
    }

    /// <summary>
    /// Displays this message box when embedded in a WindowContainer parent.
    /// Note that this call is not blocking and that you must register to the Closed event in order to handle the dialog result, if any.
    /// </summary>
    public void ShowMessageBox( string messageText, string caption, MessageBoxButton button, MessageBoxImage icon )
    {
      this.ShowMessageBoxCore( messageText, caption, button, icon, MessageBoxResult.None );
    }

    /// <summary>
    /// Displays this message box when embedded in a WindowContainer parent.
    /// Note that this call is not blocking and that you must register to the Closed event in order to handle the dialog result, if any.
    /// </summary>
    public void ShowMessageBox( string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult )
    {
      this.ShowMessageBoxCore( messageText, caption, button, icon, defaultResult );
    }

    /// <summary>
    /// Display the MessageBox window and returns only when this MessageBox closes.
    /// </summary>
    public bool? ShowDialog()
    {
      if( this.Parent != null )
        throw new InvalidOperationException(
          "This method is not intended to be called while displaying a Message Box inside a WindowContainer. Use 'ShowMessageBox()' instead." );

      _dialogResult = System.Windows.MessageBoxResult.None;
      this.Visibility = Visibility.Visible;
      this.CreateContainer();

      return this.Container.ShowDialog();
    }

    #endregion

    #region Protected
    /// <summary>
    /// Initializes the MessageBox.
    /// </summary>
    /// <param name="owner">The Window owner.</param>
    /// <param name="ownerHandle">The Window Handle owner.</param>
    /// <param name="text">The text.</param>
    /// <param name="caption">The caption.</param>
    /// <param name="button">The button.</param>
    /// <param name="image">The image.</param>
    /// <param name="defaultResult">The MessageBox result as default.</param>
    protected void InitializeMessageBox( Window owner, IntPtr ownerHandle, string text, string caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult defaultResult )
    {
      Text = text;
      Caption = caption;
      _button = button;
      _defaultResult = defaultResult;
      _owner = owner;
      _ownerHandle = ownerHandle;
      SetImageSource( image );
    }

    /// <summary>
    /// Changes the control's visual state(s).
    /// </summary>
    /// <param name="name">name of the state</param>
    /// <param name="useTransitions">True if state transitions should be used.</param>
    protected void ChangeVisualState( string name, bool useTransitions )
    {
      VisualStateManager.GoToState( this, name, useTransitions );
    }

    #endregion //Protected





    #region Private

    private bool IsCurrentWindow( object windowtoTest )
    {
      return object.Equals( _windowControl, windowtoTest );
    }

    /// <summary>
    /// Closes the MessageBox.
    /// </summary>
    private void Close()
    {
      if( this.Container != null )
      {
        // The Window.Closed event callback will call "OnClose"
        this.Container.Close();
      }
      else
      {
        this.OnClose();
      }
    }

    /// <summary>
    /// Sets the button that represents the _defaultResult to the default button and gives it focus.
    /// </summary>
    private void SetDefaultResult()
    {
      var defaultButton = GetDefaultButtonFromDefaultResult();
      if( defaultButton != null )
      {
        defaultButton.IsDefault = true;
        defaultButton.Focus();
      }
    }

    /// <summary>
    /// Gets the default button from the _defaultResult.
    /// </summary>
    /// <returns>The default button that represents the defaultResult</returns>
    private Button GetDefaultButtonFromDefaultResult()
    {
      Button defaultButton = null;
      switch( _defaultResult )
      {
        case MessageBoxResult.Cancel:
          defaultButton = GetMessageBoxButton( PART_CancelButton );
          break;
        case MessageBoxResult.No:
          defaultButton = GetMessageBoxButton( PART_NoButton );
          break;
        case MessageBoxResult.OK:
          defaultButton = GetMessageBoxButton( PART_OkButton );
          break;
        case MessageBoxResult.Yes:
          defaultButton = GetMessageBoxButton( PART_YesButton );
          break;
        case MessageBoxResult.None:
          defaultButton = GetDefaultButton();
          break;
      }
      return defaultButton;
    }

    /// <summary>
    /// Gets the default button.
    /// </summary>
    /// <remarks>Used when the _defaultResult is set to None</remarks>
    /// <returns>The button to use as the default</returns>
    private Button GetDefaultButton()
    {
      Button defaultButton = null;
      switch( _button )
      {
        case MessageBoxButton.OK:
        case MessageBoxButton.OKCancel:
          defaultButton = GetMessageBoxButton( PART_OkButton );
          break;
        case MessageBoxButton.YesNo:
        case MessageBoxButton.YesNoCancel:
          defaultButton = GetMessageBoxButton( PART_YesButton );
          break;
      }
      return defaultButton;
    }

    /// <summary>
    /// Gets a message box button.
    /// </summary>
    /// <param name="name">The name of the button to get.</param>
    /// <returns>The button</returns>
    private Button GetMessageBoxButton( string name )
    {
      Button button = GetTemplateChild( name ) as Button;
      return button;
    }

    private void ShowMessageBoxCore( string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult )
    {
      this.InitializeMessageBox( null, IntPtr.Zero, messageText, caption, button, icon, defaultResult );
      this.ShowMessageBox();
    }

    private void InitHandlers()
    {
      AddHandler( ButtonBase.ClickEvent, new RoutedEventHandler( this.Button_Click ) );

      CommandBindings.Add( new CommandBinding( ApplicationCommands.Copy, new ExecutedRoutedEventHandler( ExecuteCopy ) ) );
    }

    /// <summary>
    /// Shows the MessageBox.
    /// </summary>
    /// <param name="messageText">The message text.</param>
    /// <param name="caption">The caption.</param>
    /// <param name="button">The button.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="defaultResult">The default result.</param>
    /// <returns></returns>
    private static MessageBoxResult ShowCore( Window owner, IntPtr ownerHandle, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle )
    {
      if( System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted )
      {
        throw new InvalidOperationException( "Static methods for MessageBoxes are not available in XBAP. Use the instance ShowMessageBox methods instead." );
      }

      if( (owner != null) && (ownerHandle != IntPtr.Zero) )
      {
        throw new NotSupportedException( "The owner of a MessageBox can't be both a Window and a WindowHandle." );
      }

      var msgBox = new MessageBox();
      msgBox.InitializeMessageBox( owner, ownerHandle, messageText, caption, button, icon, defaultResult );

      // Setting the style to null will inhibit any implicit styles      
      if( messageBoxStyle != null )
      {
        msgBox.Style = messageBoxStyle;
      }

      msgBox.ShowDialog();
      return msgBox.MessageBoxResult;
    }

    private delegate Window ComputeOwnerWindowCoreDelegate();

    /// <summary>
    /// Resolves the owner Window of the MessageBox.
    /// </summary>
    /// <returns>the owner Window</returns>
    private static Window ComputeOwnerWindow()
    {
      Window result = null;

      if( Application.Current != null )
      {
        if( Application.Current.Dispatcher.CheckAccess() )
        {
          result = ComputeOwnerWindowCore();
        }
        else
        {
          Application.Current.Dispatcher.BeginInvoke( new Action( () =>
          {
            result = ComputeOwnerWindowCore();
          }
          ) );
        }
      }     

      return result;
    }

    private static Window ComputeOwnerWindowCore()
    {
      Window owner = null;

      if( Application.Current != null )
      {
        foreach( Window w in Application.Current.Windows )
        {
          if( w.IsActive )
          {
            owner = w;
            break;
          }
        }
      }

      return owner;
    }

    /// <summary>
    /// Sets the message image source.
    /// </summary>
    /// <param name="image">The image to show.</param>
    private void SetImageSource( MessageBoxImage image )
    {
      String iconName = String.Empty;

      switch( image )
      {
        case MessageBoxImage.Error:
          {
            iconName = "Error48.png";
            break;
          }
        case MessageBoxImage.Information:
          {
            iconName = "Information48.png";
            break;
          }
        case MessageBoxImage.Question:
          {
            iconName = "Question48.png";
            break;
          }
        case MessageBoxImage.Warning:
          {
            iconName = "Warning48.png";
            break;
          }
        case MessageBoxImage.None:
        default:
          {
            return;
          }
      }

      // Use this syntax for other themes to get the icons
      this.ImageSource = new BitmapImage( new Uri( String.Format( "/Xceed.Wpf.Toolkit;component/MessageBox/Icons/{0}", iconName ), UriKind.RelativeOrAbsolute ) );
    }

    /// <summary>
    /// Creates the container which will host the MessageBox control.
    /// </summary>
    /// <returns></returns>
    private Window CreateContainer()
    {
      var newWindow = new Window();
      newWindow.AllowsTransparency = true;
      newWindow.Background = Brushes.Transparent;
      newWindow.Content = this;

      if( _ownerHandle != IntPtr.Zero )
      {
        var windowHelper = new WindowInteropHelper( newWindow ) { Owner = _ownerHandle };
        newWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
      }
      else
      {
        newWindow.Owner = _owner ?? ComputeOwnerWindow();
        if( newWindow.Owner != null )
          newWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
        else
          newWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
      }

      newWindow.ShowInTaskbar = false;
      newWindow.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
      newWindow.ResizeMode = System.Windows.ResizeMode.NoResize;
      newWindow.WindowStyle = System.Windows.WindowStyle.None;
      newWindow.Closed += new EventHandler( OnContainerClosed );
      return newWindow;
    }

    #endregion //Private

    #endregion //Methods

    #region Event Handlers

    /// <summary>
    /// Processes the move of a drag operation on the header.
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
    protected virtual void OnHeaderDragDelta( DragDeltaEventArgs e )
    {
      if( !this.IsCurrentWindow( e.OriginalSource ) )
        return;

      e.Handled = true;

      DragDeltaEventArgs args = new DragDeltaEventArgs( e.HorizontalChange, e.VerticalChange );
      args.RoutedEvent = HeaderDragDeltaEvent;
      args.Source = this;
      this.RaiseEvent( args );

      if( !args.Handled )
      {
        if( this.Container == null )
        {
          double left = 0.0;

          if( this.FlowDirection == System.Windows.FlowDirection.RightToLeft )
            left = this.Left - e.HorizontalChange;
          else
            left = this.Left + e.HorizontalChange;

          this.Left = left;
          this.Top += e.VerticalChange;
        }
        else
        {
          double left = 0.0;

          if( this.FlowDirection == System.Windows.FlowDirection.RightToLeft )
            left = Container.Left - e.HorizontalChange;
          else
            left = Container.Left + e.HorizontalChange;

          Container.Left = left;
          Container.Top += e.VerticalChange;
        }
      }
    }

    /// <summary>
    /// Processes the double-click on the header.
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Input.Primitives.MouseButtonEventArgs"/> instance containing the event data.</param>
    protected virtual void OnHeaderIconDoubleClicked( MouseButtonEventArgs e )
    {
      if( !this.IsCurrentWindow( e.OriginalSource ) )
        return;

      e.Handled = true;

      MouseButtonEventArgs args = new MouseButtonEventArgs( Mouse.PrimaryDevice, 0, MouseButton.Left );
      args.RoutedEvent = HeaderIconDoubleClickedEvent;
      args.Source = this;
      this.RaiseEvent( args );

      if( !args.Handled )
      {
        this.Close();
      }
    }

    /// <summary>
    /// Processes the close button click.
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    protected virtual void OnCloseButtonClicked( RoutedEventArgs e )
    {
      if( !this.IsCurrentWindow( e.OriginalSource ) )
        return;

      e.Handled = true;

      _dialogResult = object.Equals( _button, MessageBoxButton.OK )
                             ? MessageBoxResult.OK
                             : MessageBoxResult.Cancel;

      RoutedEventArgs args = new RoutedEventArgs( CloseButtonClickedEvent, this );
      this.RaiseEvent( args );

      if( !args.Handled )
      {
        this.Close();
      }
    }

    /// <summary>
    /// Sets the MessageBoxResult according to the button pressed and then closes the MessageBox.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void Button_Click( object sender, RoutedEventArgs e )
    {
      Button button = e.OriginalSource as Button;

      if( button == null )
        return;

      switch( button.Name )
      {
        case PART_NoButton:
          _dialogResult = MessageBoxResult.No;
          this.Close();
          break;
        case PART_YesButton:
          _dialogResult = MessageBoxResult.Yes;
          this.Close();
          break;
        case PART_CancelButton:
          _dialogResult = MessageBoxResult.Cancel;
          this.Close();
          break;
        case PART_OkButton:
          _dialogResult = MessageBoxResult.OK;
          this.Close();
          break;
      }

      e.Handled = true;
    }

    /// <summary>
    /// Callack to the Container.Closed event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnContainerClosed( object sender, EventArgs e )
    {
      this.Container.Closed -= this.OnContainerClosed;
      this.Container.Content = null;
      Debug.Assert( this.Container == null );
      this.OnClose();
    }

    private void OnClose()
    {
      this.Visibility = Visibility.Collapsed;
      this.OnClosed( EventArgs.Empty );
    }

    private void MessageBox_IsVisibleChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      if( (bool)e.NewValue )
      {
        Action action = () =>
        {
          //Focus first Focusable Child element of MessageBox to prevent Tab outside MessageBox.
          var defaultButton = this.GetDefaultButtonFromDefaultResult();
          if( defaultButton != null )
          {
            defaultButton.Focus();
          }
        };

        Dispatcher.BeginInvoke( DispatcherPriority.ApplicationIdle, action );
      }
    }

    #endregion //Event Handlers

    #region Events

    /// <summary>
    /// Occurs when the MessageBox is closed.
    /// </summary>
    public event EventHandler Closed;
    protected virtual void OnClosed( EventArgs e )
    {
      if( Closed != null )
        Closed( this, e );
    }


    #endregion

    #region COMMANDS

    private void ExecuteCopy( object sender, ExecutedRoutedEventArgs e )
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( "---------------------------" );
      sb.AppendLine();
      sb.Append( Caption );
      sb.AppendLine();
      sb.Append( "---------------------------" );
      sb.AppendLine();
      sb.Append( Text );
      sb.AppendLine();
      sb.Append( "---------------------------" );
      sb.AppendLine();
      switch( _button )
      {
        case MessageBoxButton.OK:
          sb.Append( OkButtonContent.ToString() );
          break;
        case MessageBoxButton.OKCancel:
          sb.Append( OkButtonContent + "     " + CancelButtonContent );
          break;
        case MessageBoxButton.YesNo:
          sb.Append( YesButtonContent + "     " + NoButtonContent );
          break;
        case MessageBoxButton.YesNoCancel:
          sb.Append( YesButtonContent + "     " + NoButtonContent + "     " + CancelButtonContent );
          break;
      }
      sb.AppendLine();
      sb.Append( "---------------------------" );

      try
      {
        new UIPermission( UIPermissionClipboard.AllClipboard ).Demand();
        Clipboard.SetText( sb.ToString() );
      }
      catch( SecurityException )
      {
        throw new SecurityException();
      }
    }

    #endregion COMMANDS
  }
}
