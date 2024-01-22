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

/***************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2023 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ************************************************************************************/

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Window.Resources
{
  public partial class WindowModelEditor : UserControl
  {
    public WindowModelEditor()
    {
      InitializeComponent();

      ObservableCollection<ColorItem> alphaAvailableColors = new ObservableCollection<ColorItem>();
      foreach( ColorItem item in _titleShadowBrushColorPicker.AvailableColors )
      {
        System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb( ( byte )100, item.Color.Value.R, item.Color.Value.G, item.Color.Value.B );
        alphaAvailableColors.Add( new ColorItem( color, item.Name ) );
      }
      _titleShadowBrushColorPicker.AvailableColors = alphaAvailableColors;
    }

    #region IsStyleEnabled

    public static readonly DependencyProperty IsStyleEnabledProperty =
        DependencyProperty.Register( "IsStyleEnabled", typeof( bool ), typeof( WindowModelEditor ), new UIPropertyMetadata( false ) );

    public bool IsStyleEnabled
    {
      get
      {
        return ( bool )GetValue( IsStyleEnabledProperty );
      }
      set
      {
        SetValue( IsStyleEnabledProperty, value );
      }
    }

    #endregion //IsStyleEnabled
  }
}
