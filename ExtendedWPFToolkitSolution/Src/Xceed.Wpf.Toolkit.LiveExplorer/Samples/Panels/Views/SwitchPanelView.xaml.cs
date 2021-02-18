/**************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ************************************************************************************/

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System;
using System.Windows;
using Xceed.Wpf.Toolkit.Panels;
using System.Collections.Generic;
using Xceed.Wpf.Toolkit;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Panels.Views
{
  /// <summary>
  /// Interaction logic for SwitchPanelView.xaml
  /// </summary>
  public partial class SwitchPanelView : DemoView
  {
    #region Members


    #endregion

    public SwitchPanelView()
    {
      InitializeComponent();
    }

    #region Event Handlers

    private void OnLayoutComboSelectionChanged( object sender, RoutedEventArgs e )
    {
      ComboBox comboBox = sender as ComboBox;
      bool isPlusPanel = (comboBox.SelectedIndex >= 2);

      if( _openSourceScreenShot != null )
        _openSourceScreenShot.Visibility = isPlusPanel ? Visibility.Visible : Visibility.Collapsed;
      if( _openSourceScreenShotDesc != null )
        _openSourceScreenShotDesc.Visibility = isPlusPanel ? Visibility.Visible : Visibility.Collapsed;
      if( _openSourceTextHyperlink != null )
        _openSourceTextHyperlink.Visibility = isPlusPanel ? Visibility.Visible : Visibility.Collapsed;
      if( _switchPanel != null )
        _switchPanel.Visibility = isPlusPanel ? Visibility.Collapsed : Visibility.Visible;

      if( isPlusPanel )
      {
        BitmapImage bitmapImage = new BitmapImage();
        string desc;

        bitmapImage.BeginInit();
        switch( comboBox.SelectedIndex )
        {
          case 2: 
            bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\Canvas.png", UriKind.Relative );
            desc = this.Resources[ "canvasPanelDescription" ] as string;
            break;
          case 3: 
            bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\Carousel.png", UriKind.Relative );
            desc = this.Resources[ "carouselDescription" ] as string;
            break;
          case 4: 
            bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\DockPanel.png", UriKind.Relative );
            desc = this.Resources[ "dockPanelDescription" ] as string;
            break;
          case 5: 
            bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\Grid.png", UriKind.Relative );
            desc = this.Resources[ "gridDescription" ] as string;
            break;
          case 6: 
            bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\StackPanel.png", UriKind.Relative );
            desc = this.Resources[ "stackPanelDescription" ] as string;
            break;
          case 7: 
            bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\StackedStackPanel.png", UriKind.Relative );
            desc = this.Resources[ "stackedStackPanelDescription" ] as string;
            break;
          case 8: 
            bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\AutoStretchStackPanel.png", UriKind.Relative );
            desc = this.Resources[ "autoStretchStackPanelDescription" ] as string;
              break;
          case 9: 
            bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\RelativeCanvas.png", UriKind.Relative );
            desc = this.Resources[ "relativeCanvasDescription" ] as string;
              break;
          case 10: 
            bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\RadialCanvas.png", UriKind.Relative );
            desc = this.Resources[ "radialCanvasDescription" ] as string;
            break;
          case 11: 
            bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\CameraPanel.png", UriKind.Relative );
            desc = this.Resources[ "cameraPanelDescription" ] as string;
            break;
          case 12: 
            bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\PerspectivePanel.png", UriKind.Relative );
            desc = this.Resources[ "perspectivePanelDescription" ] as string;
            break;
          case 13: 
            bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\AnimatedTimelinePanel.png", UriKind.Relative );
            desc = this.Resources[ "animatedTimelinePanelDescription" ] as string;
            break;
          default: throw new InvalidDataException( "LayoutcomboBox.SelectedIndex is not valid." );
        }
        bitmapImage.EndInit();

        if( _openSourceScreenShot != null )
          _openSourceScreenShot.Source = bitmapImage;
        if( _openSourceScreenShotDesc != null )
          _openSourceScreenShotDesc.Text = desc;
      }
    }

    private void OnSwitchPanelLayoutChanged( object sender, RoutedEventArgs e )
    {
    }





















    #endregion 

    #region Methods (Private)





























    #endregion

  }


  public class Coordinates
  {
    public double X
    {
      get;
      set;
    }
    public double Y
    {
      get;
      set;
    }

    public override string ToString()
    {
      return "(" + this.X + ", " + this.Y + ")";
    }
  }
}
