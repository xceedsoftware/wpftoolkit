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
using System.Windows.Controls;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit
{
  public class IconButton : Button
  {
    #region Constructors

    static IconButton()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( IconButton ), new FrameworkPropertyMetadata( typeof( IconButton ) ) );
    }

    public IconButton()
    {
    }

    #endregion //Constructors

    #region Properties

    #region Icon

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register( "Icon", typeof( Image ), typeof( IconButton ), new FrameworkPropertyMetadata( null ) );
    public Image Icon
    {
      get
      {
        return ( Image )this.GetValue( IconButton.IconProperty );
      }
      set
      {
        this.SetValue( IconButton.IconProperty, value );
      }
    }

    #endregion //Icon

    #region IconLocation

    public static readonly DependencyProperty IconLocationProperty = DependencyProperty.Register( "IconLocation", typeof( Location ),
      typeof( IconButton ), new FrameworkPropertyMetadata( Location.Left ) );
    public Location IconLocation
    {
      get
      {
        return ( Location )this.GetValue( IconButton.IconLocationProperty );
      }
      set
      {
        this.SetValue( IconButton.IconLocationProperty, value );
      }
    }

    #endregion //IconLocation

    #region MouseOverBackground

    public static readonly DependencyProperty MouseOverBackgroundProperty = DependencyProperty.Register( "MouseOverBackground", typeof( Brush ), typeof( IconButton ), new FrameworkPropertyMetadata( null ) );

    public Brush MouseOverBackground
    {
      get
      {
        return ( Brush )this.GetValue( IconButton.MouseOverBackgroundProperty );
      }
      set
      {
        this.SetValue( IconButton.MouseOverBackgroundProperty, value );
      }
    }

    #endregion //MouseOverBackground

    #region MouseOverBorderBrush

    public static readonly DependencyProperty MouseOverBorderBrushProperty = DependencyProperty.Register( "MouseOverBorderBrush", typeof( Brush ), typeof( IconButton ), new FrameworkPropertyMetadata( null ) );

    public Brush MouseOverBorderBrush
    {
      get
      {
        return ( Brush )this.GetValue( IconButton.MouseOverBorderBrushProperty );
      }
      set
      {
        this.SetValue( IconButton.MouseOverBorderBrushProperty, value );
      }
    }

    #endregion //MouseOverBorderBrush

    #region MouseOverForeground

    public static readonly DependencyProperty MouseOverForegroundProperty = DependencyProperty.Register( "MouseOverForeground", typeof( Brush ), typeof( IconButton ), new FrameworkPropertyMetadata( null ) );

    public Brush MouseOverForeground
    {
      get
      {
        return ( Brush )this.GetValue( IconButton.MouseOverForegroundProperty );
      }
      set
      {
        this.SetValue( IconButton.MouseOverForegroundProperty, value );
      }
    }

    #endregion //MouseOverForeground

    #region MousePressedBackground

    public static readonly DependencyProperty MousePressedBackgroundProperty = DependencyProperty.Register( "MousePressedBackground", typeof( Brush ), typeof( IconButton ), new FrameworkPropertyMetadata( null ) );

    public Brush MousePressedBackground
    {
      get
      {
        return ( Brush )this.GetValue( IconButton.MousePressedBackgroundProperty );
      }
      set
      {
        this.SetValue( IconButton.MousePressedBackgroundProperty, value );
      }
    }

    #endregion  //MousePressedBackground

    #region MousePressedBorderBrush

    public static readonly DependencyProperty MousePressedBorderBrushProperty = DependencyProperty.Register( "MousePressedBorderBrush", typeof( Brush ), typeof( IconButton ), new FrameworkPropertyMetadata( null ) );

    public Brush MousePressedBorderBrush
    {
      get
      {
        return ( Brush )this.GetValue( IconButton.MousePressedBorderBrushProperty );
      }
      set
      {
        this.SetValue( IconButton.MousePressedBorderBrushProperty, value );
      }
    }

    #endregion  //MousePressedBorderBrush

    #region MousePressedForeground

    public static readonly DependencyProperty MousePressedForegroundProperty = DependencyProperty.Register( "MousePressedForeground", typeof( Brush ), typeof( IconButton ), new FrameworkPropertyMetadata( null ) );

    public Brush MousePressedForeground
    {
      get
      {
        return ( Brush )this.GetValue( IconButton.MousePressedForegroundProperty );
      }
      set
      {
        this.SetValue( IconButton.MousePressedForegroundProperty, value );
      }
    }

    #endregion  //MousePressedForeground

    #endregion
  }
}
