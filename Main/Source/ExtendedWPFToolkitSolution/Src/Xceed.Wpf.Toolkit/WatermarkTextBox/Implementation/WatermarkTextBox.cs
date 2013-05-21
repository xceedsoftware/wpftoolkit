/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xceed.Wpf.Toolkit
{
  public class WatermarkTextBox : TextBox
  {
    #region Properties

    #region SelectAllOnGotFocus

    public static readonly DependencyProperty SelectAllOnGotFocusProperty = DependencyProperty.Register( "SelectAllOnGotFocus", typeof( bool ), typeof( WatermarkTextBox ), new PropertyMetadata( false ) );
    public bool SelectAllOnGotFocus
    {
      get
      {
        return ( bool )GetValue( SelectAllOnGotFocusProperty );
      }
      set
      {
        SetValue( SelectAllOnGotFocusProperty, value );
      }
    }

    #endregion //SelectAllOnGotFocus

    #region Watermark

    public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register( "Watermark", typeof( object ), typeof( WatermarkTextBox ), new UIPropertyMetadata( null ) );
    public object Watermark
    {
      get
      {
        return ( object )GetValue( WatermarkProperty );
      }
      set
      {
        SetValue( WatermarkProperty, value );
      }
    }

    #endregion //Watermark

    #region WatermarkTemplate

    public static readonly DependencyProperty WatermarkTemplateProperty = DependencyProperty.Register( "WatermarkTemplate", typeof( DataTemplate ), typeof( WatermarkTextBox ), new UIPropertyMetadata( null ) );
    public DataTemplate WatermarkTemplate
    {
      get
      {
        return ( DataTemplate )GetValue( WatermarkTemplateProperty );
      }
      set
      {
        SetValue( WatermarkTemplateProperty, value );
      }
    }

    #endregion //WatermarkTemplate

    #endregion //Properties

    #region Constructors

    static WatermarkTextBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( WatermarkTextBox ), new FrameworkPropertyMetadata( typeof( WatermarkTextBox ) ) );
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override void OnGotFocus( RoutedEventArgs e )
    {
      base.OnGotFocus( e );

       if( SelectAllOnGotFocus )
        SelectAll();
    }

    protected override void OnPreviewMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      if( !IsKeyboardFocused && SelectAllOnGotFocus )
      {
        e.Handled = true;
        Focus();
      }

      base.OnPreviewMouseLeftButtonDown( e );
    }

    #endregion //Base Class Overrides
  }
}
