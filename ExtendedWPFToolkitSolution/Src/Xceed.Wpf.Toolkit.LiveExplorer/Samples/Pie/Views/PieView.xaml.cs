/***************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  **************************************************************************************/

using System.Windows.Data;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Pie.Views
{
  /// <summary>
  /// Interaction logic for PieView.xaml
  /// </summary>
  public partial class PieView : DemoView
  {
    public PieView()
    {
      InitializeComponent();
    }

    private void ComboBox_SelectionChanged( object sender, System.Windows.Controls.SelectionChangedEventArgs e )
    {
      ComboBox modeCombo = ( ComboBox )sender;
      PieMode newMode = ( PieMode )modeCombo.SelectedItem;
      this.endAngleSlider.IsEnabled = ( newMode != PieMode.Slice );
      this.sliceSlider.IsEnabled = ( newMode != PieMode.EndAngle );
    }
  }
}
