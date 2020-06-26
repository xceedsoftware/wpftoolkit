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

using System.Windows;

namespace Xceed.Wpf.Toolkit
{
#pragma warning disable 0618

  public class WatermarkTextBox : AutoSelectTextBox
  {
    #region Properties

    #region KeepWatermarkOnGotFocus

    public static readonly DependencyProperty KeepWatermarkOnGotFocusProperty = DependencyProperty.Register( "KeepWatermarkOnGotFocus", typeof( bool ), typeof( WatermarkTextBox ), new UIPropertyMetadata( false ) );
    public bool KeepWatermarkOnGotFocus
    {
      get
      {
        return ( bool )GetValue( KeepWatermarkOnGotFocusProperty );
      }
      set
      {
        SetValue( KeepWatermarkOnGotFocusProperty, value );
      }
    }

    #endregion //KeepWatermarkOnGotFocus

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

    public WatermarkTextBox()
    {
    }

    #endregion //Constructors

    #region Base Class Overrides




    #endregion
  }

#pragma warning restore 0618
}
