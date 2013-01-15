/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using System.Text;
using System.Security.Permissions;
using System.Security;

namespace Xceed.Wpf.Toolkit
{
  [TemplateVisualState( Name = VisualStates.OK, GroupName = VisualStates.MessageBoxButtonsGroup )]
  [TemplateVisualState( Name = VisualStates.OKCancel, GroupName = VisualStates.MessageBoxButtonsGroup )]
  [TemplateVisualState( Name = VisualStates.YesNo, GroupName = VisualStates.MessageBoxButtonsGroup )]
  [TemplateVisualState( Name = VisualStates.YesNoCancel, GroupName = VisualStates.MessageBoxButtonsGroup )]
  [TemplatePart( Name = PART_DragWidget, Type = typeof( Thumb ) )]
  [TemplatePart( Name = PART_CancelButton, Type = typeof( Button ) )]
  [TemplatePart( Name = PART_NoButton, Type = typeof( Button ) )]
  [TemplatePart( Name = PART_OkButton, Type = typeof( Button ) )]
  [TemplatePart( Name = PART_YesButton, Type = typeof( Button ) )]
  public class MessageBox : Control
  {
    private const string PART_DragWidget = "PART_DragWidget";
    private const string PART_CancelButton = "PART_CancelButton";
    private const string PART_NoButton = "PART_NoButton";
    private const string PART_OkButton = "PART_OkButton";
    private const string PART_YesButton = "PART_YesButton";
    private const string PART_CloseButton = "PART_CloseButton";

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
    /// Tracks the owner of the MessageBox
    /// </summary>
    private Window _owner;

    #endregion //Private Members

    #region Constructors

    static MessageBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( MessageBox ), new FrameworkPropertyMetadata( typeof( MessageBox ) ) );
    }

    internal MessageBox()
    {
      /*user cannot create instance */
      AddHandler( ButtonBase.ClickEvent, new RoutedEventHandler( Button_Click ) );

      CommandBindings.Add( new CommandBinding( ApplicationCommands.Copy, new ExecutedRoutedEventHandler( ExecuteCopy ) ) );
    }

    #endregion //Constructors

    #region Properties

    #region Protected Properties

    /// <summary>
    /// A System.Windows.MessageBoxResult value that specifies which message box button was clicked by the user.
    /// </summary>
    protected MessageBoxResult MessageBoxResult = MessageBoxResult.None;

    protected Window Container
    {
      get;
      private set;
    }
    protected Thumb DragWidget
    {
      get;
      private set;
    }

    #endregion //Protected Properties

    #region Dependency Properties

    public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register( "Caption", typeof( string ), typeof( MessageBox ), new UIPropertyMetadata( String.Empty ) );
    public string Caption
    {
      get
      {
        return ( string )GetValue( CaptionProperty );
      }
      set
      {
        SetValue( CaptionProperty, value );
      }
    }

    public static readonly DependencyProperty CaptionForegroundProperty = DependencyProperty.Register( "CaptionForeground", typeof( Brush ), typeof( MessageBox ), new UIPropertyMetadata( null ) );
    public Brush CaptionForeground
    {
      get
      {
        return ( Brush )GetValue( CaptionForegroundProperty );
      }
      set
      {
        SetValue( CaptionForegroundProperty, value );
      }
    }

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

    public static readonly DependencyProperty CloseButtonStyleProperty = DependencyProperty.Register( "CloseButtonStyle", typeof( Style ), typeof( MessageBox ), new PropertyMetadata( null ) );
    public Style CloseButtonStyle
    {
      get
      {
        return ( Style )GetValue( CloseButtonStyleProperty );
      }
      set
      {
        SetValue( CloseButtonStyleProperty, value );
      }
    }

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

    public static readonly DependencyProperty WindowBackgroundProperty = DependencyProperty.Register( "WindowBackground", typeof( Brush ), typeof( MessageBox ), new PropertyMetadata( null ) );
    public Brush WindowBackground
    {
      get
      {
        return ( Brush )GetValue( WindowBackgroundProperty );
      }
      set
      {
        SetValue( WindowBackgroundProperty, value );
      }
    }

    public static readonly DependencyProperty WindowBorderBrushProperty = DependencyProperty.Register( "WindowBorderBrush", typeof( Brush ), typeof( MessageBox ), new PropertyMetadata( null ) );
    public Brush WindowBorderBrush
    {
      get
      {
        return ( Brush )GetValue( WindowBorderBrushProperty );
      }
      set
      {
        SetValue( WindowBorderBrushProperty, value );
      }
    }

    public static readonly DependencyProperty WindowOpacityProperty = DependencyProperty.Register( "WindowOpacity", typeof( double ), typeof( MessageBox ), new PropertyMetadata( null ) );
    public double WindowOpacity
    {
      get
      {
        return ( double )GetValue( WindowOpacityProperty );
      }
      set
      {
        SetValue( WindowOpacityProperty, value );
      }
    }

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

    #endregion //Dependency Properties

    #endregion //Properties

    #region Base Class Overrides

    /// <summary>
    /// Overrides the OnApplyTemplate method.
    /// </summary>
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( DragWidget != null )
        DragWidget.DragDelta -= ( o, e ) => ProcessMove( e );

      DragWidget = GetTemplateChild( PART_DragWidget ) as Thumb;

      if( DragWidget != null )
        DragWidget.DragDelta += ( o, e ) => ProcessMove( e );

      ChangeVisualState( _button.ToString(), true );

      Button closeButton = GetMessageBoxButton( PART_CloseButton );
      if( closeButton != null )
        closeButton.IsEnabled = !object.Equals( _button, MessageBoxButton.YesNo );

      Button okButton = GetMessageBoxButton( PART_OkButton );
      if( okButton != null )
        okButton.IsCancel = object.Equals( _button, MessageBoxButton.OK );

      SetDefaultResult();
    }

    #endregion //Base Class Overrides

    #region Methods

    #region Public Static


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
      return ShowCore( null, messageText, caption, button, MessageBoxImage.None, MessageBoxResult.None, messageBoxStyle );
    }


    public static MessageBoxResult Show( Window owner, string messageText, string caption, MessageBoxButton button )
    {
        return Show(owner, messageText, caption, button, (Style)null);
    }

    public static MessageBoxResult Show( Window owner, string messageText, string caption, MessageBoxButton button, Style messageBoxStyle )
    {
      return ShowCore( owner, messageText, caption, button, MessageBoxImage.None, MessageBoxResult.None, messageBoxStyle );
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
      return ShowCore( null, messageText, caption, button, icon, MessageBoxResult.None, messageBoxStyle );
    }

    public static MessageBoxResult Show( Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon )
    {
        return Show(owner, messageText, caption, button, icon, (Style)null);
    }

    public static MessageBoxResult Show( Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, Style messageBoxStyle )
    {
      return ShowCore( owner, messageText, caption, button, icon, MessageBoxResult.None, messageBoxStyle );
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
      return ShowCore( null, messageText, caption, button, icon, defaultResult, messageBoxStyle );
    }

    public static MessageBoxResult Show( Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult )
    {
        return Show(owner, messageText, caption, button, icon, defaultResult, (Style) null);
    }


    public static MessageBoxResult Show( Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle )
    {
      return ShowCore( owner, messageText, caption, button, icon, defaultResult, messageBoxStyle );
    }

    #endregion //Public Static

    #region Protected

    /// <summary>
    /// Shows the container which contains the MessageBox.
    /// </summary>
    protected void Show()
    {
      Container.ShowDialog();
    }

    /// <summary>
    /// Initializes the MessageBox.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="caption">The caption.</param>
    /// <param name="button">The button.</param>
    /// <param name="image">The image.</param>
    protected void InitializeMessageBox( Window owner, string text, string caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult defaultResult )
    {
      Text = text;
      Caption = caption;
      _button = button;
      _defaultResult = defaultResult;
      _owner = owner;
      SetImageSource( image );
      Container = CreateContainer();
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

    /// <summary>
    /// Shows the MessageBox.
    /// </summary>
    /// <param name="messageText">The message text.</param>
    /// <param name="caption">The caption.</param>
    /// <param name="button">The button.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="defaultResult">The default result.</param>
    /// <returns></returns>
    private static MessageBoxResult ShowCore( Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle )
    {
      MessageBox msgBox = new MessageBox();
      msgBox.InitializeMessageBox( owner, messageText, caption, button, icon, defaultResult );

      // Setting the style to null will inhibit any implicit styles      
      if( messageBoxStyle != null )
      {
        msgBox.Style = messageBoxStyle;
      }

      msgBox.Show();
      return msgBox.MessageBoxResult;
    }

    /// <summary>
    /// Resolves the owner Window of the MessageBox.
    /// </summary>
    /// <returns>the owner Window</returns>
    private static Window ComputeOwnerWindow()
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

      ImageSource = ( ImageSource )new ImageSourceConverter().ConvertFromString( String.Format( "pack://application:,,,/Xceed.Wpf.Toolkit;component/MessageBox/Icons/{0}", iconName ) );
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
      newWindow.Owner = _owner ?? ComputeOwnerWindow();

      if( newWindow.Owner != null )
        newWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
      else
        newWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

      newWindow.ShowInTaskbar = false;
      newWindow.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
      newWindow.ResizeMode = System.Windows.ResizeMode.NoResize;
      newWindow.WindowStyle = System.Windows.WindowStyle.None;
      return newWindow;
    }

    #endregion //Private

    #endregion //Methods

    #region Event Handlers

    /// <summary>
    /// Processes the move of a drag operation on the header.
    /// </summary>
    /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
    private void ProcessMove( DragDeltaEventArgs e )
    {
      double left = 0.0;

      if( FlowDirection == System.Windows.FlowDirection.RightToLeft )
        left = Container.Left - e.HorizontalChange;
      else
        left = Container.Left + e.HorizontalChange;

      Container.Left = left;
      Container.Top = Container.Top + e.VerticalChange;
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
          MessageBoxResult = MessageBoxResult.No;
          break;
        case PART_YesButton:
          MessageBoxResult = MessageBoxResult.Yes;
          break;
        case PART_CloseButton:
          MessageBoxResult = object.Equals( _button, MessageBoxButton.OK )
                              ? MessageBoxResult.OK
                              : MessageBoxResult.Cancel;
          break;
        case PART_CancelButton:
          MessageBoxResult = MessageBoxResult.Cancel;
          break;
        case PART_OkButton:
          MessageBoxResult = MessageBoxResult.OK;
          break;
      }

      e.Handled = true;

      Close();
    }

    /// <summary>
    /// Closes the MessageBox.
    /// </summary>
    private void Close()
    {
      Container.Close();
    }

    #endregion //Event Handlers

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
