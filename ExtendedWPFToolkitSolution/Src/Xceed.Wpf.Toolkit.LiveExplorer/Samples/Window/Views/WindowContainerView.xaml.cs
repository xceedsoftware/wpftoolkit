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
using Xceed.Wpf.Toolkit;
using System.Collections.ObjectModel;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Window.Views
{
  public enum GenderData
  {
    Male = 0, 
    Female
  }

  /// <summary>
  /// Interaction logic for WindowContainerView.xaml
  /// </summary>
  public partial class WindowContainerView : DemoView
  {
    public WindowContainerView()
    {
      InitializeComponent();

      ObservableCollection<ColorItem> alphaAvailableColors = new ObservableCollection<ColorItem>();
      foreach( ColorItem item in _modalBackgroundColorPicker.AvailableColors )
      {
        System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb( ( byte )100, item.Color.Value.R, item.Color.Value.G, item.Color.Value.B );
        alphaAvailableColors.Add( new ColorItem( color, item.Name ) );
      }
      _modalBackgroundColorPicker.AvailableColors = alphaAvailableColors;
    }

    #region Event Handler

    private void OnChildWindowCheckBoxClick( object sender, EventArgs e )
    {
      _childWindow.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
    }

    private void OnModalChildWindowCheckBoxClick( object sender, EventArgs e )
    {
      _modalChildWindow.WindowState = Xceed.Wpf.Toolkit.WindowState.Open;
    }

    private void OnMessageBoxCheckBoxChecked( object sender, EventArgs e )
    {
      _messageBox.ShowMessageBox();
    }

    private void OnMessageBoxCheckBoxUnchecked( object sender, EventArgs e )
    {
      _messageBox.Visibility = Visibility.Collapsed;
      _messageBoxCheckBox.IsChecked = false;
    }

    private void OnMessageBoxClose( object sender, EventArgs e )
    {
      _messageBoxCheckBox.IsChecked = false;
    }

    #endregion
  }
}
