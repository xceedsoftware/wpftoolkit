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

using System.Windows.Documents;
using Xceed.Wpf.Toolkit.LiveExplorer.Core.CodeFormatting;
using Xceed.Wpf.Toolkit;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Core
{
  /// <summary>
  /// Formats the RichTextBox text as colored C#
  /// </summary>
  public class CSharpFormatter : ITextFormatter
  {
    public readonly static CSharpFormatter Instance = new CSharpFormatter();

    public string GetText( FlowDocument document )
    {
      return new TextRange( document.ContentStart, document.ContentEnd ).Text;
    }

    public void SetText( FlowDocument document, string text )
    {
      document.Blocks.Clear();
      document.PageWidth = 2500;

      CSharpFormat cSharpFormat = new CSharpFormat();
      Paragraph p = new Paragraph();
      p = cSharpFormat.FormatCode( text );
      document.Blocks.Add( p );
    }
  }
}
