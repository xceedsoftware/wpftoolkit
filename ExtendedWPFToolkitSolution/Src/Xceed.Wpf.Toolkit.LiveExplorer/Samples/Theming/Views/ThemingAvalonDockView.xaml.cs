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
using System.IO;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Theming.Views
{
  /// <summary>
  /// Interaction logic for ThemingAvalonDockView.xaml
  /// </summary>
  public partial class ThemingAvalonDockView : DemoView
  {



    #region constructors

    public ThemingAvalonDockView()
    {
      this.Initialized += new EventHandler( AvalonDockControlsThemes_Initialized );
      InitializeComponent();
    }

    #endregion

    #region EventHandlers

    private void DefaultComboBoxItem_Selected( object sender, RoutedEventArgs e )
    {
      AvalonDockComboBoxItem comboBoxItem = sender as AvalonDockComboBoxItem;

      if( comboBoxItem != null )
      {
        this.SetOpenSourceImage( comboBoxItem );
      }
    }

    private void AvalonDockControlsThemes_Initialized( object sender, EventArgs e )
    {
      _themeCombo.SelectedIndex = 0;
    }

    private void MetroComboBoxItem_Selected( object sender, RoutedEventArgs e )
    {
      AvalonDockComboBoxItem comboBoxItem = sender as AvalonDockComboBoxItem;
      if( comboBoxItem != null )
      {
        this.SetOpenSourceImage( comboBoxItem );
      }
    }









    #endregion

    #region private Methods


     private void SetOpenSourceImage( AvalonDockComboBoxItem comboBoxItem )
    {
      if( comboBoxItem != null )
      {
        bool isPlusPanel = ( comboBoxItem.ThemeEnum == AvalonDockThemesEnum.Office2007Black 
                          || comboBoxItem.ThemeEnum == AvalonDockThemesEnum.Office2007Blue
                          || comboBoxItem.ThemeEnum == AvalonDockThemesEnum.Office2007Silver
                          || comboBoxItem.ThemeEnum == AvalonDockThemesEnum.Windows10
                          || comboBoxItem.ThemeEnum == AvalonDockThemesEnum.MetroDark
                          || comboBoxItem.ThemeEnum == AvalonDockThemesEnum.MetroLight );

        if( _openSourceScreenShot != null )
          _openSourceScreenShot.Visibility = isPlusPanel ? Visibility.Visible : Visibility.Collapsed;
        if( _openSourceTextHyperlink != null )
          _openSourceTextHyperlink.Visibility = isPlusPanel ? Visibility.Visible : Visibility.Collapsed;
        if( _dockingManager != null )
          _dockingManager.Visibility = isPlusPanel ? Visibility.Collapsed : Visibility.Visible;

        if( isPlusPanel )
        {
          BitmapImage bitmapImage = new BitmapImage();

          bitmapImage.BeginInit();
          switch( comboBoxItem.ThemeEnum )
          {
            case AvalonDockThemesEnum.Office2007Black:
              bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\AvalonDockOffice2007Black.png", UriKind.Relative );
              break;
            case AvalonDockThemesEnum.Office2007Blue:
              bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\AvalonDockOffice2007Blue.png", UriKind.Relative );
              break;
            case AvalonDockThemesEnum.Office2007Silver:
              bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\AvalonDockOffice2007Silver.png", UriKind.Relative );
              break;
            case AvalonDockThemesEnum.Windows10:
              bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\AvalonDockWindows10.png", UriKind.Relative );
              break;
            case AvalonDockThemesEnum.MetroDark:
              bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\AvalonDockMetroDark.png", UriKind.Relative );
              break;
            case AvalonDockThemesEnum.MetroLight:
              bitmapImage.UriSource = new Uri( "..\\OpenSourceImages\\AvalonDockMetroLight.png", UriKind.Relative );
              break;
            default:
              throw new InvalidDataException( "LayoutcomboBox.SelectedIndex is not valid." );
          }
          bitmapImage.EndInit();

          if( _openSourceScreenShot != null )
            _openSourceScreenShot.Source = bitmapImage;
        }
      }
    }






    #endregion
  }


  public enum AvalonDockThemesEnum
  {
    Generic,
    Aero,
    VS2010,
    Metro,
    Office2007Black,
    Office2007Blue,
    Office2007Silver,
    Windows10,
    MetroDark,
    MetroLight
  }

  public class AvalonDockComboBoxItem : ComboBoxItem
  {
    public AvalonDockThemesEnum ThemeEnum
    {
      get;
      set;
    }
  }
}
