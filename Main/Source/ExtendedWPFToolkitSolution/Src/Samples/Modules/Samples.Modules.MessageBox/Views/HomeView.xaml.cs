/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Controls;
using System.Windows;

namespace Samples.Modules.MessageBox.Views
{
  /// <summary>
  /// Interaction logic for HomeView.xaml
  /// </summary>
  [RegionMemberLifetime( KeepAlive = false )]
  public partial class HomeView : DemoView
  {
    private string _standardTitle = "Standard MessageBox";
    private string _styledTitle = "Extended WPF Toolkit MessageBox";
    private string _standardMessage = "The standard system MessageBox will always have this look. No styling is possible.";
    private string _styledMessage = "The Toolkit MessageBox allows you to style it in order to integrate it into your application colors and styles.";

    public HomeView()
    {
      InitializeComponent();
    }

    private void StandardMessageBoxButton_Click( object sender, System.Windows.RoutedEventArgs e )
    {
      System.Windows.MessageBox.Show( _standardMessage, _standardTitle );
    }

    private void StyledMessageBoxButton_Click( object sender, System.Windows.RoutedEventArgs e )
    {
      System.Windows.Style style = ( System.Windows.Style )this.Resources[ "messageBoxStyle" ];
      Xceed.Wpf.Toolkit.MessageBox.Show( null, _styledMessage, _styledTitle, style );
    }
  }
}
