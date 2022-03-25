/***************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2021 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  *************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.PropertyGrid.Views
{
  /// <summary>
  /// Interaction logic for PropertyGridAdvancedContextMenuView.xaml
  /// </summary>
  public partial class PropertyGridAdvancedContextMenuView : DemoView
  {
    public PropertyGridAdvancedContextMenuView()
    {
      InitializeComponent();

      var selObject = new Person()
      {
        FirstName = "John",
        LastName = "Doe"
      };

      this.DataContext = selObject;
    }

    private void MenuItem_Click( object sender, RoutedEventArgs e )
    {
      MenuItem menuItem = sender as MenuItem;
      if( menuItem != null && menuItem.DataContext is PropertyItem )
      {
        Clipboard.SetData( DataFormats.Text, ( ( PropertyItem )menuItem.DataContext ).Value );
      }
    }

    private class Person
    {
      public string FirstName
      {
        get; set;
      }
      public string LastName
      {
        get; set;
      }
    }
  }

}
