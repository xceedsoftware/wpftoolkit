/*************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ************************************************************************************/

using System.Windows;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using System.IO;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.AvalonDock.Views
{
  /// <summary>
  /// Interaction logic for AvalonDockView.xaml
  /// </summary>
  public partial class AvalonDockView : DemoView
  {
    public AvalonDockView()
    {
      InitializeComponent();
    }

    private void SaveButton_Click( object sender, RoutedEventArgs e )
    {
      using( var writer = new StreamWriter( "AvalonDockSavedFile.txt" ) )
      {
        var layoutSerializer = new XmlLayoutSerializer( _dockingManager );
        layoutSerializer.Serialize( writer );
      }
    }

    private void LoadButton_Click( object sender, RoutedEventArgs e )
    {
      using( var reader = new StreamReader( "AvalonDockSavedFile.txt" ) )
      {
        var layoutSerializer = new XmlLayoutSerializer( _dockingManager );
        layoutSerializer.Deserialize( reader );
      }
    }
  }
}
