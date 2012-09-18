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

using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Controls;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;
using System;
using System.Windows.Resources;
using System.Windows;
using System.IO;
namespace Samples.Modules.Text.Views
{
  public enum TextFormatterEnum
  {
    PlainText,
    Rtf,
    Xaml
  };

  /// <summary>
  /// Interaction logic for RichTextBox.xaml
  /// </summary>
  [RegionMemberLifetime( KeepAlive = false )]
  public partial class RichTextBoxView : DemoView
  {
    public RichTextBoxView()
    {
      InitializeComponent();
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      UpdateValues( _textFormatter );
    }

    private void OnTextFormatterChanged( object sender, SelectionChangedEventArgs args )
    {
      ComboBox comboBox = sender as ComboBox;
      UpdateValues( comboBox );
    }

    private void UpdateValues( ComboBox comboBox )
    {
      if( ( comboBox != null ) && ( _text != null ) && ( _richTextBox != null ) )
      {
        object tagValue = ( ( ComboBoxItem )comboBox.SelectedItem ).Tag;
        if( object.Equals( TextFormatterEnum.PlainText, tagValue ) )
        {
          _text.Text = GetDataFromResource( "/Samples.Modules.Text;component/Resources/PlainData.txt" );
          _richTextBox.TextFormatter = new PlainTextFormatter();
        }
        else if( object.Equals( TextFormatterEnum.Rtf, tagValue ) )
        {
          _text.Text = GetDataFromResource( "/Samples.Modules.Text;component/Resources/RtfData.txt" );
          _richTextBox.TextFormatter = new RtfFormatter();
        }
        else if( object.Equals( TextFormatterEnum.Xaml, tagValue ) )
        {
          _text.Text = GetDataFromResource( "/Samples.Modules.Text;component/Resources/XamlData.txt" );
          _richTextBox.TextFormatter = new XamlFormatter();
        }
      }
    }

    private string GetDataFromResource( string uriString )
    {
      Uri uri = new Uri( uriString, UriKind.Relative );
      StreamResourceInfo info = Application.GetResourceStream( uri );

      StreamReader reader = new StreamReader( info.Stream );
      string data = reader.ReadToEnd();
      reader.Close();

      return data;
    }
  }
}
