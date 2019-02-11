/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using Xceed.Utils.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal static class FrameworkElementExtensions
  {
    #region Static Fields

    private static readonly WeakDictionary<TextProperties, double> s_fontHeight = new WeakDictionary<TextProperties, double>();

    #endregion

    internal static object CoerceMinHeight( this FrameworkElement source, Thickness padding, object value )
    {
      if( source == null )
        throw new ArgumentNullException( "source" );

      var newValue = ( double )value;
      var minHeight = FrameworkElementExtensions.GetFontHeight( source ) + padding.Top + padding.Bottom;

      if( newValue >= minHeight )
        return value;

      if( minHeight != source.MinHeight )
        return minHeight;

      return DependencyProperty.UnsetValue;
    }

    internal static double GetFontHeight( this FrameworkElement source )
    {
      if( source == null )
        throw new ArgumentNullException( "source" );

      var textProperties = new TextProperties(
                             TextOptions.GetTextFormattingMode( source ),
                             TextElement.GetFontFamily( source ),
                             TextElement.GetFontStyle( source ),
                             TextElement.GetFontWeight( source ),
                             TextElement.GetFontStretch( source ),
                             TextElement.GetFontSize( source ) );

      lock( ( ( ICollection )s_fontHeight ).SyncRoot )
      {
        double fontHeight;

        if( !s_fontHeight.TryGetValue( textProperties, out fontHeight ) )
        {
          var formatter = TextFormatter.Create( textProperties.FormattingMode );
          var typeface = new Typeface( textProperties.FontFamily, textProperties.FontStyle, textProperties.FontWeight, textProperties.FontStretch );
          var textSource = new EmptyTextSource();
          var textRunProperties = new EmptyTextRunProperties( typeface, textProperties.FontSize );
          var textParagraphProperties = new EmptyTextParagraphProperties( textRunProperties );
          var textLine = formatter.FormatLine( textSource, 0, 0d, textParagraphProperties, null );

          fontHeight = textLine.Height;
          s_fontHeight.Add( textProperties, fontHeight );
        }

        return fontHeight;
      }
    }

    #region EmptyTextSource Private Class

    private sealed class EmptyTextSource : TextSource
    {
      public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText( int textSourceCharacterIndexLimit )
      {
        return new TextSpan<CultureSpecificCharacterBufferRange>( 0, null );
      }

      public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex( int textSourceCharacterIndex )
      {
        return -1;
      }

      public override TextRun GetTextRun( int textSourceCharacterIndex )
      {
        return new TextEndOfParagraph( 1 );
      }
    }

    #endregion

    #region EmptyTextParagraphProperties Private Class

    private sealed class EmptyTextParagraphProperties : TextParagraphProperties
    {
      internal EmptyTextParagraphProperties( TextRunProperties defaultTextRunProperties )
      {
        m_defaultTextRunProperties = defaultTextRunProperties;
      }

      public override TextRunProperties DefaultTextRunProperties
      {
        get
        {
          return m_defaultTextRunProperties;
        }
      }

      public override bool FirstLineInParagraph
      {
        get
        {
          return true;
        }
      }

      public override FlowDirection FlowDirection
      {
        get
        {
          return FlowDirection.LeftToRight;
        }
      }

      public override double Indent
      {
        get
        {
          return 0d;
        }
      }

      public override double LineHeight
      {
        get
        {
          return 0d;
        }
      }

      public override TextAlignment TextAlignment
      {
        get
        {
          return TextAlignment.Left;
        }
      }

      public override TextMarkerProperties TextMarkerProperties
      {
        get
        {
          return null;
        }
      }

      public override TextWrapping TextWrapping
      {
        get
        {
          return TextWrapping.NoWrap;
        }
      }

      private readonly TextRunProperties m_defaultTextRunProperties;
    }

    #endregion

    #region EmptyTextRunProperties Private Class

    private sealed class EmptyTextRunProperties : TextRunProperties
    {
      internal EmptyTextRunProperties( Typeface typeface, double fontSize )
      {
        m_typeface = typeface;
        m_fontSize = fontSize;
      }

      public override Brush BackgroundBrush
      {
        get
        {
          return Brushes.Transparent;
        }
      }

      public override CultureInfo CultureInfo
      {
        get
        {
          return CultureInfo.InvariantCulture;
        }
      }

      public override double FontHintingEmSize
      {
        get
        {
          return m_fontSize;
        }
      }

      public override double FontRenderingEmSize
      {
        get
        {
          return m_fontSize;
        }
      }

      public override Brush ForegroundBrush
      {
        get
        {
          return Brushes.Transparent;
        }
      }

      public override TextDecorationCollection TextDecorations
      {
        get
        {
          return null;
        }
      }

      public override TextEffectCollection TextEffects
      {
        get
        {
          return null;
        }
      }

      public override Typeface Typeface
      {
        get
        {
          return m_typeface;
        }
      }

      private readonly Typeface m_typeface;
      private readonly double m_fontSize;
    }

    #endregion

    #region TextProperties Private Class

    private sealed class TextProperties : IEquatable<TextProperties>
    {
      internal TextProperties( TextFormattingMode formattingMode, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize )
      {
        this.FormattingMode = formattingMode;
        this.FontFamily = fontFamily;
        this.FontStyle = fontStyle;
        this.FontWeight = fontWeight;
        this.FontStretch = fontStretch;
        this.FontSize = fontSize;
      }

      public override int GetHashCode()
      {
        unchecked
        {
          var hashCode = this.FontSize.GetHashCode();

          if( this.FontFamily != null )
          {
            hashCode = hashCode * 13 + this.FontFamily.GetHashCode();
          }

          hashCode = hashCode * 13 + this.FontStyle.GetHashCode();
          hashCode = hashCode * 13 + this.FontWeight.GetHashCode();
          hashCode = hashCode * 13 + this.FontStretch.GetHashCode();
          hashCode = hashCode * 13 + this.FormattingMode.GetHashCode();

          return hashCode;
        }
      }

      public override bool Equals( object obj )
      {
        return this.Equals( obj as TextProperties );
      }

      public bool Equals( TextProperties obj )
      {
        if( object.ReferenceEquals( obj, null ) )
          return false;

        return object.Equals( obj.FontSize, this.FontSize )
            && object.Equals( obj.FontFamily, this.FontFamily )
            && object.Equals( obj.FontStyle, this.FontStyle )
            && object.Equals( obj.FontWeight, this.FontWeight )
            && object.Equals( obj.FontStretch, this.FontStretch )
            && object.Equals( obj.FormattingMode, this.FormattingMode );
      }

      internal readonly TextFormattingMode FormattingMode;
      internal readonly FontFamily FontFamily;
      internal readonly FontStyle FontStyle;
      internal readonly FontWeight FontWeight;
      internal readonly FontStretch FontStretch;
      internal readonly double FontSize;
    }

    #endregion
  }
}
