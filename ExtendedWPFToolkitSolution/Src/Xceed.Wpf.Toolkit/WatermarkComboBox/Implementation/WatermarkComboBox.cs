/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2024 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit
{
  public class WatermarkComboBox : ComboBox
  {
    #region Properties

    #region Watermark

    public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register( "Watermark", typeof( object ), typeof( WatermarkComboBox ), new UIPropertyMetadata( null ) );
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

    public static readonly DependencyProperty WatermarkTemplateProperty = DependencyProperty.Register( "WatermarkTemplate", typeof( DataTemplate ), typeof( WatermarkComboBox ), new UIPropertyMetadata( null ) );
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

    #endregion //WatermarkBackground

    #region WatermarkBackground

    public static readonly DependencyProperty WatermarkBackgroundProperty = DependencyProperty.RegisterAttached(
        "WatermarkBackground", typeof( Brush ), typeof( WatermarkComboBox ), new PropertyMetadata( ( Brush )null ) );

    public Brush WatermarkBackground
    {
      get
      {
        return ( Brush )GetValue( WatermarkBackgroundProperty );
      }
      set
      {
        SetValue( WatermarkBackgroundProperty, value );
      }
    }

    #endregion //WatermarkBackground

    #endregion //Properties

    #region Constructors

    static WatermarkComboBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( WatermarkComboBox ), new FrameworkPropertyMetadata( typeof( WatermarkComboBox ) ) );
    }

    public WatermarkComboBox()
    {

      Core.Message.ShowMessage();
    }

    #endregion //Constructors

    #region Base Class Overrides




    #endregion
  }
}
