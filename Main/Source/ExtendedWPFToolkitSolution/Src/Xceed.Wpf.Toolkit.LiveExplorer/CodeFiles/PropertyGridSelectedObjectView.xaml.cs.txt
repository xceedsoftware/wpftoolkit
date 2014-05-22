/***************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2014 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  *************************************************************************************/

using System;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.PropertyGrid.Views
{
  /// <summary>
  /// Interaction logic for PropertyGridSelectedObjectView.xaml
  /// </summary>
  public partial class PropertyGridSelectedObjectView : DemoView
  {
    public PropertyGridSelectedObjectView()
    {
      InitializeComponent();
    }

    private void OnControlGetFocus(object sender, System.Windows.RoutedEventArgs e)
    {
      _propertyGrid.SelectedObject = e.Source;
    }
  }
}
