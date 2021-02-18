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

using System.Windows.Controls;
using Xceed.Wpf.Toolkit;
using System;
using System.Windows.Resources;
using System.Windows;
using System.IO;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Text.Views
{
  public enum TextFormatterEnum
  {
    PlainText,
    Rtf,
    Xaml
  };

  /// <summary>
  /// Interaction logic for RichTextboxView.xaml
  /// </summary>
  public partial class RichTextboxView : DemoView
  {
    public RichTextboxView()
    {
      InitializeComponent();
      this.UpdateFormatter();
    }

    private void OnTextFormatterChanged( object sender, SelectionChangedEventArgs args )
    {
      this.UpdateFormatter();
    }

    private void UpdateFormatter()
    {
      if( ( _textFormatter != null ) && ( _text != null ) && ( _richTextBox != null ) )
      {
        object tagValue = ( ( ComboBoxItem )_textFormatter.SelectedItem ).Tag;
        if( object.Equals( TextFormatterEnum.PlainText, tagValue ) )
        {
          _richTextBox.TextFormatter = new PlainTextFormatter();
        }
        else if( object.Equals( TextFormatterEnum.Rtf, tagValue ) )
        {
          _richTextBox.TextFormatter = new RtfFormatter();
        }
        else if( object.Equals( TextFormatterEnum.Xaml, tagValue ) )
        {
          _richTextBox.TextFormatter = new XamlFormatter();
        }
      }
    }
  }
}
