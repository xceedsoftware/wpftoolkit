/**************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  *************************************************************************************/

using System.Collections.Generic;
using System.Globalization;
using System;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Numeric.Views
{
  /// <summary>
  /// Interaction logic for NumericView.xaml
  /// </summary>
  public partial class NumericView : DemoView
  {
    public NumericView()
    {
      this.Cultures = new List<CultureInfo>() { new CultureInfo( "en-US" ),
                                                new CultureInfo("en-GB"),
                                                new CultureInfo("fr-FR"),
                                                new CultureInfo("ar-DZ"),
                                                new CultureInfo("zh-CN"),
                                                new CultureInfo("cs-CZ") };

      InitializeComponent();
    }

    public List<CultureInfo> Cultures
    {
      get;
      private set;
    }
  }


  public class FormatObject
  {
    public string Value
    {
      get;
      set;
    }

    public string DisplayValue
    {
      get;
      set;
    }
  }
}
