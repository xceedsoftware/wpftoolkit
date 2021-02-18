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

using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.LiveExplorer.Core.CodeFormatting;
using Xceed.Wpf.Toolkit;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Core
{
  /// <summary>
  /// Formats the RichTextBox text as colored Xaml
  /// </summary>
  public class XamlFormatter : ITextFormatter
  {
    public readonly static XamlFormatter Instance = new XamlFormatter();

    public string GetText( FlowDocument document )
    {
      return new TextRange( document.ContentStart, document.ContentEnd ).Text;
    }

    public void SetText( FlowDocument document, string text )
    {
      document.Blocks.Clear();
      document.PageWidth = 2500;
      XamlFormatter.ColorizeXAML( text, document );
    }

    #region SyntaxColoring

    #region ColorizeXAML

    public static FlowDocument ColorizeXAML( string xamlText, FlowDocument targetDoc )
    {
      XmlTokenizer tokenizer = new XmlTokenizer();
      XmlTokenizerMode mode = XmlTokenizerMode.OutsideElement;

      List<XmlToken> tokens = tokenizer.Tokenize( xamlText, ref mode );
      List<string> tokenTexts = new List<string>( tokens.Count );
      List<Color> colors = new List<Color>( tokens.Count );
      int position = 0;
      foreach( XmlToken token in tokens )
      {
        string tokenText = xamlText.Substring( position, token.Length );
        tokenTexts.Add( tokenText );
        Color color = ColorForToken( token, tokenText );
        colors.Add( color );
        position += token.Length;
      }

      Paragraph p = new Paragraph();

      // Loop through tokens
      for( int i = 0; i < tokenTexts.Count; i++ )
      {
        Run r = new Run( tokenTexts[ i ] );
        r.Foreground = new SolidColorBrush( colors[ i ] );
        p.Inlines.Add( r );
      }

      targetDoc.Blocks.Add( p );

      return targetDoc;
    }

    #endregion //ColorizeXAML

    static Color ColorForToken( XmlToken token, string tokenText )
    {
      Color color = Color.FromRgb( 0, 0, 0 );
      switch( token.Kind )
      {
        case XmlTokenKind.Open:
        case XmlTokenKind.OpenClose:
        case XmlTokenKind.Close:
        case XmlTokenKind.SelfClose:
        case XmlTokenKind.CommentBegin:
        case XmlTokenKind.CommentEnd:
        case XmlTokenKind.CDataBegin:
        case XmlTokenKind.CDataEnd:
        case XmlTokenKind.Equals:
        case XmlTokenKind.OpenProcessingInstruction:
        case XmlTokenKind.CloseProcessingInstruction:
        case XmlTokenKind.AttributeValue:
          color = Color.FromRgb( 0, 0, 255 );
          // color = "blue";
          break;
        case XmlTokenKind.ElementName:
          color = Color.FromRgb( 163, 21, 21 );
          // color = "brown";
          break;
        case XmlTokenKind.TextContent:
          // color = "black";
          break;
        case XmlTokenKind.AttributeName:
        case XmlTokenKind.Entity:
          color = Color.FromRgb( 255, 0, 0 );
          // color = "red";
          break;
        case XmlTokenKind.CommentText:
          color = Color.FromRgb( 0, 128, 0 );
          // color = "green";
          break;
      }
      if( token.Kind == XmlTokenKind.ElementWhitespace
          || ( token.Kind == XmlTokenKind.TextContent && tokenText.Trim() == "" ) )
      {
        // color = null;
      }
      return color;
    }
    #endregion SyntaxColoring
  }
}
