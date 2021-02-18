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
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Resources;
using Xceed.Wpf.Toolkit.LiveExplorer.Core;

namespace Xceed.Wpf.Toolkit.LiveExplorer
{
  public abstract class CodeBox : Xceed.Wpf.Toolkit.RichTextBox
  {
    protected CodeBox()
    {
      this.IsReadOnly = true;
      this.FontFamily = new FontFamily( "Courier New" );
      this.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
      this.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
      this.Document.PageWidth = 2500;
    }

    public string CodeSource
    {
      set
      {
        if( value == null )
          this.Text = null;

        this.Text = this.GetDataFromResource( value );
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

  public class XamlBox : CodeBox
  {
    public XamlBox() { this.TextFormatter = new Core.XamlFormatter(); }
  }

  public class CSharpBox : CodeBox
  {
    public CSharpBox() { this.TextFormatter = new Core.CSharpFormatter(); }
  }
}
