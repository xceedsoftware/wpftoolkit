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

using System;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Controls;
using Xceed.Wpf.Toolkit;

namespace Samples.Modules.Button.Views
{
  /// <summary>
  /// Interaction logic for ButtonSpinnerView.xaml
  /// </summary>
  [RegionMemberLifetime( KeepAlive = false )]
  public partial class ButtonSpinnerView : DemoView
  {
    public ButtonSpinnerView()
    {
      InitializeComponent();
    }

    private void ButtonSpinner_Spin( object sender, SpinEventArgs e )
    {
      String[] names = (String[])this.Resources[ "names" ];

      ButtonSpinner spinner = ( ButtonSpinner )sender;
      TextBox txtBox = ( TextBox )spinner.Content;

      int value = Array.IndexOf( names, txtBox.Text );
      if( e.Direction == SpinDirection.Increase )
        value++;
      else
        value--;

      if( value < 0 )
        value = names.Length - 1;
      else if( value >= names.Length )
        value = 0;

      txtBox.Text = names[ value ];
    }
  }
}
