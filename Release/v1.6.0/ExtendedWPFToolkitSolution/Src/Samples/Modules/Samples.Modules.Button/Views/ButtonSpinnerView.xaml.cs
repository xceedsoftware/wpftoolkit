/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

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
using Microsoft.Windows.Controls;
using Samples.Infrastructure.Controls;

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

    private void ButtonSpinner_Spin( object sender, Microsoft.Windows.Controls.SpinEventArgs e )
    {
      ButtonSpinner spinner = ( ButtonSpinner )sender;
      TextBox txtBox = ( TextBox )spinner.Content;

      try
      {
        int value = String.IsNullOrEmpty( txtBox.Text ) ? 0 : Convert.ToInt32( txtBox.Text );
        if( e.Direction == Microsoft.Windows.Controls.SpinDirection.Increase )
          value++;
        else
          value--;
        txtBox.Text = value.ToString();
      }
      catch
      {
        txtBox.Text = "0";
      }
    }
  }
}
