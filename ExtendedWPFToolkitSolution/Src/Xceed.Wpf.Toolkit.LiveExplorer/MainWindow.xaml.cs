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
using System.Globalization;
using System.Reflection;
using Xceed.Wpf.Toolkit.LiveExplorer;
using Xceed.Wpf.Toolkit.LiveExplorer.Core;
using System.Diagnostics;

namespace Xceed.Wpf.Toolkit.LiveExplorer
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private const string toolkitAssembly =
       "Xceed.Wpf.Toolkit.LiveExplorer"
      ;

    public MainWindow()
    {
      InitializeComponent();
       this.Title = "Toolkit Plus for WPF - LiveExplorer";

      this.Loaded += new RoutedEventHandler( this.MainWindow_Loaded );

      var version = Assembly.GetExecutingAssembly().GetName().Version;
      VersionTextBlock.Text = "Version: " + version.Major + "." + version.Minor;
    }


#region Properties

#region View

    public static readonly DependencyProperty ViewProperty = DependencyProperty.Register( "View", typeof( DemoView ), typeof( MainWindow ), new UIPropertyMetadata( null, OnViewChanged ) );
    public DemoView View
    {
      get
      {
        return ( DemoView )GetValue( ViewProperty );
      }
      set
      {
        SetValue( ViewProperty, value );
      }
    }

    private static void OnViewChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      MainWindow window = o as MainWindow;
      if( window != null )
        window.OnViewChanged( ( DemoView )e.OldValue, ( DemoView )e.NewValue );
    }

    protected virtual void OnViewChanged( DemoView oldValue, DemoView newValue )
    {
      this.InitView();
    }

#endregion //View

#endregion //Properties

#region Event Handler

    void MainWindow_Loaded( object sender, RoutedEventArgs e )
    {
      this.InitView();
    }

    private void OnTreeViewSelectionChanged( object sender, RoutedPropertyChangedEventArgs<Object> e )
    {
      this.UpdateSelectedView( e.NewValue as LiveExplorerTreeViewItem );
    }

    private void UpdateSelectedView( LiveExplorerTreeViewItem treeViewItem )
    {
      if( treeViewItem != null )
      {
        treeViewItem.IsExpanded = true;
        Type type = treeViewItem.SampleType;
        if( type != null )
        {
          string name = type.FullName;

          Assembly assembly = Assembly.Load( toolkitAssembly );
          Type sampleType = assembly.GetType( name );

          this.View = ( DemoView )Activator.CreateInstance( sampleType );
        }
      }
    }

    private void Hyperlink_RequestNavigate( object sender, System.Windows.Navigation.RequestNavigateEventArgs e )
    {
      Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) );
      e.Handled = true;
    }

    private void Image_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
    {
      //When the user clicks the Xceed logo on the top left.
      //
    }

#endregion //EventHandler

#region Methods

    private void InitView()
    {
      if( ( _flowDocumentDesc != null ) && ( this.View != null) )
      {
        _flowDocumentDesc.Blocks.Clear();
        if( this.View.Description != null )
        {
          _flowDocumentDesc.Blocks.Add( this.View.Description );
        }
      }
      if( _contentScrollViewer != null )
      {
        _contentScrollViewer.ScrollToHome();
      }
    }

#endregion

  }
}
