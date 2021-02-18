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

using System.Windows.Controls;
using System.Windows.Data;
using System;
using System.Windows;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Zoombox.Views
{
  /// <summary>
  /// Interaction logic for ZoomboxView.xaml
  /// </summary>
  public partial class ZoomboxView : DemoView
  {
    public ZoomboxView()
    {
      InitializeComponent();
    }

    private void AdjustAnimationDuration( object sender, RoutedPropertyChangedEventArgs<double> e )
    {
      Slider slider = sender as Slider;
      if( slider == null )
        return;

      zoombox.AnimationDuration = System.TimeSpan.FromMilliseconds( slider.Value );
    }

    private void CoerceAnimationRatios( object sender, RoutedPropertyChangedEventArgs<double> e )
    {
      Slider slider = sender as Slider;
      if( slider == null )
        return;

      Slider otherRatio = ( sender == this.AccelerationSlider ) ? this.DecelerationSlider : this.AccelerationSlider;

      if( slider.Value + otherRatio.Value > 1 )
      {
        otherRatio.Value = 1 - slider.Value;
      }
    }
  }
}
