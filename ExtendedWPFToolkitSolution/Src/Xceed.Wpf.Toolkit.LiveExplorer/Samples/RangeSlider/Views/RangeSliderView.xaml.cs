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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.RangeSlider.Views
{
  /// <summary>
  /// Interaction logic for RangeSliderView.xaml
  /// </summary>
  public partial class RangeSliderView : DemoView
  {
    public RangeSliderView()
    {
      InitializeComponent();
    }

    private void RangeStyleComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
    {
      if( e.AddedItems.Count > 0 && e.AddedItems[ 0 ] is ComboBoxItem)
      {
        ComboBoxItem item = e.AddedItems[ 0 ] as ComboBoxItem;
        //A style different from null is chosen
        if( item.Tag != null )
        {
          //LowerRangeBackground is Transparent, force a color
          if( object.Equals( sender, lowerRangeStyleComboBox ) && ((SolidColorBrush)_rangeSlider.LowerRangeBackground).Color.Equals( Colors.Transparent) )
          {
            _rangeSlider.LowerRangeBackground = new SolidColorBrush( Colors.Green );
          }
          //RangeBackground is Transparent, force a color
          else if( object.Equals( sender, rangeStyleComboBox ) && ( ( SolidColorBrush )_rangeSlider.RangeBackground ).Color.Equals( Colors.Transparent ) )
          {
            _rangeSlider.RangeBackground = new SolidColorBrush( Colors.Blue );
          }
          //HigherRangeBackground is Transparent, force a color
          else if( object.Equals( sender, higherRangeStyleComboBox ) && ( ( SolidColorBrush )_rangeSlider.HigherRangeBackground ).Color.Equals( Colors.Transparent ) )
          {
            _rangeSlider.HigherRangeBackground = new SolidColorBrush( Colors.Green );
          }
        }
      }
    }
  }
}
