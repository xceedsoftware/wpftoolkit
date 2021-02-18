/***************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ************************************************************************************/

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
using System.Collections.ObjectModel;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Window.Resources
{
  /// <summary>
  /// Interaction logic for WindowModelEditor.xaml
  /// </summary>
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
      get { return ( bool )GetValue( IsStyleEnabledProperty ); }
      set { SetValue( IsStyleEnabledProperty, value ); }
    }

    #endregion //IsStyleEnabled
  }
}
