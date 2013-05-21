/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows.Documents;

namespace Xceed.Wpf.Toolkit
{
  /// <summary>
  /// Formats the RichTextBox text as plain text
  /// </summary>
  public class PlainTextFormatter : ITextFormatter
  {
    public string GetText( FlowDocument document )
    {
      return new TextRange( document.ContentStart, document.ContentEnd ).Text;
    }

    public void SetText( FlowDocument document, string text )
    {
      new TextRange( document.ContentStart, document.ContentEnd ).Text = text;
    }
  }
}
