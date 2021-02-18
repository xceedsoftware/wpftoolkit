/***************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  *************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using System.ComponentModel;
using System.Diagnostics;
using Xceed.Wpf.Toolkit.Primitives;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Window.Views
{
  /// <summary>
  /// Interaction logic for StyleableWindowView.xaml
  /// </summary>
  public partial class StyleableWindowView : DemoView
  {
    private const string StandardMsgBoxTitle = "Standard MessageBox";
    private const string StyledMsgBoxTitle = "Toolkit for WPF styled MessageBox";
    private const string StyleableWindowTitle = "Toolkit for WPF StyleableWindow";
    private const string StandardMsgBoxMessage = "The standard system MessageBox will always have this look. No styling is possible.";
    private const string StyledMsgBoxMessage = "The Toolkit MessageBox allows you to style it in order to integrate it into your application colors and styles.";
    private const string StyleableWindowContent = "This is a StyleableWindow that has the same functionality as a normal window.";
    private const string MessageBoxStyleKey = "messageBoxStyle";
    private const string WindowControlStyleKey = "windowControlStyle";
    private const string WindowButtonStyle = "FancyButtonStyle";
    private const string StyleableWindowMessage = "StyleableWindow is a standalone window that can be styled just like ChildWindow or MessageBox. This is a feature of the \"Plus\" version.";

    public StyleableWindowView()
    {
      InitializeComponent();
      this.UpdateWindowsStyles(null,null);
    }


    private void OnStyleableWindow_Click( object sender, RoutedEventArgs e )
    {
      var msgBox = new Xceed.Wpf.Toolkit.MessageBox();
      msgBox.DataContext = this.DataContext;
      msgBox.Text = StyleableWindowMessage;
      msgBox.Caption = StyledMsgBoxTitle;
      msgBox.Style = ( _enableStyleCheckBox.IsChecked.GetValueOrDefault() ) ? ( Style )this.Resources[ MessageBoxStyleKey ] : null;
      msgBox.ShowDialog();



    }

    private void StandardMessageBoxButton_Click( object sender, RoutedEventArgs e )
    {
      System.Windows.MessageBox.Show( StandardMsgBoxMessage, StandardMsgBoxTitle );
    }

    private void StyledMessageBoxButton_Click( object sender, RoutedEventArgs e )
    {
      var msgBox = new Xceed.Wpf.Toolkit.MessageBox();
      msgBox.DataContext = this.DataContext;
      msgBox.Text = StyledMsgBoxMessage;
      msgBox.Caption = StyledMsgBoxTitle;
      if( _enableStyleCheckBox.IsChecked.GetValueOrDefault() ) 
      {
        msgBox.Style = ( Style )this.Resources[ MessageBoxStyleKey ];
      }
      msgBox.ShowDialog();
    }

    private void UpdateWindowsStyles( object sender, RoutedEventArgs e )
    {
      bool styled = _enableStyleCheckBox.IsChecked.GetValueOrDefault();
      if( styled )
      {
        _childWindow.Style = ( Style )this.Resources[ WindowControlStyleKey ];
      }
      else
      {
        _childWindow.ClearValue( ChildWindow.StyleProperty );
      }

    }
  }

  /// <summary>
  /// This model will allow to share the style values between our MessageBox, 
  /// ChildWindow and StyleableWindow controls
  /// </summary>
  public class WindowModel : DependencyObject
  {

    #region WindowBackground

    public static readonly DependencyProperty WindowBackgroundProperty =
        DependencyProperty.Register( "WindowBackground", typeof( Brush ), typeof( WindowModel ), new UIPropertyMetadata( null ) );

    public Brush WindowBackground
    {
      get { return ( Brush )GetValue( WindowBackgroundProperty ); }
      set { SetValue( WindowBackgroundProperty, value ); }
    }

    #endregion //WindowBackground

    #region WindowInactiveBackground

    public static readonly DependencyProperty WindowInactiveBackgroundProperty =
        DependencyProperty.Register( "WindowInactiveBackground", typeof( Brush ), typeof( WindowModel ), new UIPropertyMetadata( null ) );

    public Brush WindowInactiveBackground
    {
      get
      {
        return ( Brush )GetValue( WindowInactiveBackgroundProperty );
      }
      set
      {
        SetValue( WindowInactiveBackgroundProperty, value );
      }
    }

    #endregion //WindowInactiveBackground

    #region WindowBorderBrush

    public static readonly DependencyProperty WindowBorderBrushProperty =
        DependencyProperty.Register( "WindowBorderBrush", typeof( Brush ), typeof( WindowModel ) );

    public Brush WindowBorderBrush
    {
      get { return ( Brush )GetValue( WindowBorderBrushProperty ); }
      set { SetValue( WindowBorderBrushProperty, value ); }
    }

    #endregion //WindowBorderBrush

    #region TitleFontSize

    public static readonly DependencyProperty TitleFontSizeProperty =
        DependencyProperty.Register( "TitleFontSize", typeof( double ), typeof( WindowModel ) );

    public double TitleFontSize
    {
      get
      {
        return (double)GetValue( TitleFontSizeProperty );
      }
      set
      {
        SetValue( TitleFontSizeProperty, value );
      }
    }

    #endregion //TitleFontSize

    #region TitleForeground

    public static readonly DependencyProperty TitleForegroundProperty =
        DependencyProperty.Register( "TitleForeground", typeof( Brush ), typeof( WindowModel ) );

    public Brush TitleForeground
    {
      get { return ( Brush )GetValue( TitleForegroundProperty ); }
      set { SetValue( TitleForegroundProperty, value ); }
    }

    #endregion //TitleForeground

    #region TitleShadowBrush

    public static readonly DependencyProperty TitleShadowBrushProperty =
        DependencyProperty.Register( "TitleShadowBrush", typeof( Brush ), typeof( WindowModel ) );

    public Brush TitleShadowBrush
    {
      get
      {
        return ( Brush )GetValue( TitleShadowBrushProperty );
      }
      set
      {
        SetValue( TitleShadowBrushProperty, value );
      }
    }

    #endregion //TitleShadowBrush

    #region WindowBorderThickness

    public static readonly DependencyProperty WindowBorderThicknessProperty =
        DependencyProperty.Register( "WindowBorderThickness", typeof( Thickness ), typeof( WindowModel ) );

    public Thickness WindowBorderThickness
    {
      get { return ( Thickness )GetValue( WindowBorderThicknessProperty ); }
      set { SetValue( WindowBorderThicknessProperty, value ); }
    }

    #endregion //WindowBorderThickness

    #region WindowOpacity

    public static readonly DependencyProperty WindowOpacityProperty =
        DependencyProperty.Register( "WindowOpacity", typeof( double ), typeof( WindowModel ) );

    public double WindowOpacity
    {
      get { return ( double )GetValue( WindowOpacityProperty ); }
      set { SetValue( WindowOpacityProperty, value ); }
    }

    #endregion //WindowOpacity

    #region WindowStyle

    public static readonly DependencyProperty WindowStyleProperty =
        DependencyProperty.Register( "WindowStyle", typeof( WindowStyle ), typeof( WindowModel ) );

    public WindowStyle WindowStyle
    {
      get { return ( WindowStyle )GetValue( WindowStyleProperty ); }
      set { SetValue( WindowStyleProperty, value ); }
    }

    #endregion //WindowStyle

    #region ResizeMode

    public static readonly DependencyProperty ResizeModeProperty =
        DependencyProperty.Register( "ResizeMode", typeof( ResizeMode ), typeof( WindowModel ) );

    public ResizeMode ResizeMode
    {
      get { return ( ResizeMode )GetValue( ResizeModeProperty ); }
      set { SetValue( ResizeModeProperty, value ); }
    }

    #endregion //ResizeMode

    #region CloseButtonVisibility
    public static readonly DependencyProperty CloseButtonVisibilityProperty =
        DependencyProperty.Register( "CloseButtonVisibility", typeof( Visibility ), typeof( WindowModel ) );

    public Visibility CloseButtonVisibility
    {
      get { return ( Visibility )GetValue( CloseButtonVisibilityProperty ); }
      set { SetValue( CloseButtonVisibilityProperty, value ); }
    }
    #endregion //CloseButtonVisibility

    #region CloseButtonStyle
    public static readonly DependencyProperty CloseButtonStyleProperty =
        DependencyProperty.Register( "CloseButtonStyle", typeof( Style ), typeof( WindowModel ) );

    public Style CloseButtonStyle
    {
      get { return ( Style )GetValue( CloseButtonStyleProperty ); }
      set { SetValue( CloseButtonStyleProperty, value ); }
    }
    #endregion //CloseButtonStyle

    #region MinimizeButtonStyle
    public static readonly DependencyProperty MinimizeButtonStyleProperty =
        DependencyProperty.Register( "MinimizeButtonStyle", typeof( Style ), typeof( WindowModel ) );

    public Style MinimizeButtonStyle
    {
      get { return ( Style )GetValue( MinimizeButtonStyleProperty ); }
      set { SetValue( MinimizeButtonStyleProperty, value ); }
    }
    #endregion //MinimizeButtonStyle

    #region MaximizeButtonStyle
    public static readonly DependencyProperty MaximizeButtonStyleProperty =
        DependencyProperty.Register( "MaximizeButtonStyle", typeof( Style ), typeof( WindowModel ) );

    public Style MaximizeButtonStyle
    {
      get { return ( Style )GetValue( MaximizeButtonStyleProperty ); }
      set { SetValue( MaximizeButtonStyleProperty, value ); }
    }
    #endregion //MaximizeButtonStyle

    #region RestoreButtonStyle
    public static readonly DependencyProperty RestoreButtonStyleProperty =
        DependencyProperty.Register( "RestoreButtonStyle", typeof( Style ), typeof( WindowModel ) );

    public Style RestoreButtonStyle
    {
      get { return ( Style )GetValue( RestoreButtonStyleProperty ); }
      set { SetValue( RestoreButtonStyleProperty, value ); }
    }
    #endregion //RestoreButtonStyle

  }
}
