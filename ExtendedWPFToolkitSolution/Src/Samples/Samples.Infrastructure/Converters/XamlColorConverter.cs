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
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Samples.Infrastructure.Core.CodeFormatting;

namespace Samples.Infrastructure.Converters
{
  public class XamlColorConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( value == null )
        return value;

      string xaml = value as string;

      FlowDocument doc = new FlowDocument();
      ColorizeXAML( xaml, doc );

      RichTextBox rtb = new RichTextBox();
      rtb.IsReadOnly = true;
      rtb.Document = doc;
      rtb.Document.PageWidth = 2500.0;
      rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
      rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
      rtb.FontFamily = new FontFamily( "Courier New" );
      return rtb;
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      throw new NotImplementedException();
    }

    #region SyntaxColoring

    #region ColorizeXAML

    public void ColorizeXAML( string xamlText, FlowDocument targetDoc )
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
