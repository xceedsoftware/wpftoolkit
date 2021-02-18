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
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Collections.ObjectModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.PropertyGrid.Views
{
  /// <summary>
  /// Interaction logic for PropertyGridPropertyItemStyleView.xaml
  /// </summary>
  public partial class PropertyGridPropertyItemStyleView : DemoView
  {
    public PropertyGridPropertyItemStyleView()
    {
      InitializeComponent();

      var selObject = new Person();
      selObject.Friend = new Person();

      this.DataContext = selObject;
    }

    private void OnPreparePropertyItem( object sender, PropertyItemEventArgs e )
    {
      var propertyItem = e.PropertyItem as PropertyItem;
      // Parent of top-level properties is the PropertyGrid itself.
      bool isTopLevelProperty = 
        ( propertyItem.ParentElement is Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid );

      if( isTopLevelProperty && propertyItem.PropertyDescriptor.Name == "Friend" )
      {
        propertyItem.DisplayName = "Friend (renamed)";
      }
    }

    private class Person
    {
      public string FirstName { get; set; }
      public string LastName { get; set; }
      [ExpandableObject()]
      public Person Friend { get; set; }
    }
  }
}
