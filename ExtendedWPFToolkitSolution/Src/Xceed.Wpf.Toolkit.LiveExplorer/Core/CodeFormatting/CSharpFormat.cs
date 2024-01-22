/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2023 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

#region Copyright (C) 2001-2003 Jean-Claude Manoli [jc@manoli.net]
/*
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the author(s) be held liable for any damages arising from
 * the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *   1. The origin of this software must not be misrepresented; you must not
 *      claim that you wrote the original software. If you use this software
 *      in a product, an acknowledgment in the product documentation would be
 *      appreciated but is not required.
 * 
 *   2. Altered source versions must be plainly marked as such, and must not
 *      be misrepresented as being the original software.
 * 
 *   3. This notice may not be removed or altered from any source distribution.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Core.CodeFormatting
{
  public class CSharpFormat : CLikeFormat
  {
    protected override string Keywords
    {
      get
      {
        return "abstract as base bool break byte case catch char "
        + "checked class const continue decimal default delegate do double else "
        + "enum event explicit extern false finally fixed float for foreach goto "
        + "if implicit in int interface internal is lock long namespace new null "
        + "object operator out override partial params private protected public readonly "
        + "ref return sbyte sealed short sizeof stackalloc static string struct "
        + "switch this throw true try typeof uint ulong unchecked unsafe ushort "
        + "using value virtual void volatile where while yield";
      }
    }

    protected override string Preprocessors
    {
      get
      {
        return "#if #else #elif #endif #define #undef #warning "
            + "#error #line #region #endregion #pragma";
      }
    }
  }

  public abstract class CLikeFormat : CodeFormat
  {
    protected override string CommentRegEx
    {
      get
      {
        return @"/\*.*?\*/|//.*?(?=\r|\n)";
      }
    }

    protected override string StringRegEx
    {
      get
      {
        return @"@?""""|@?"".*?(?!\\).""|''|'.*?(?!\\).'";
      }
    }
  }

  public abstract class CodeFormat : SourceFormat
  {
    protected abstract string Keywords
    {
      get;
    }

    protected virtual string Preprocessors
    {
      get
      {
        return "";
      }
    }

    protected abstract string StringRegEx
    {
      get;
    }

    protected abstract string CommentRegEx
    {
      get;
    }

    public virtual bool CaseSensitive
    {
      get
      {
        return true;
      }
    }

    protected CodeFormat()
    {
      //generate the keyword and preprocessor regexes from the keyword lists
      Regex r;
      r = new Regex( @"\w+|-\w+|#\w+|@@\w+|#(?:\\(?:s|w)(?:\*|\+)?\w+)+|@\\w\*+" );
      string regKeyword = r.Replace( Keywords, @"(?<=^|\W)$0(?=\W)" );
      string regPreproc = r.Replace( Preprocessors, @"(?<=^|\s)$0(?=\s|$)" );
      r = new Regex( @" +" );
      regKeyword = r.Replace( regKeyword, @"|" );
      regPreproc = r.Replace( regPreproc, @"|" );

      if( regPreproc.Length == 0 )
      {
        regPreproc = "(?!.*)_{37}(?<!.*)"; //use something quite impossible...
      }

      //build a master regex with capturing groups
      StringBuilder regAll = new StringBuilder();
      regAll.Append( "(" );
      regAll.Append( CommentRegEx );
      regAll.Append( ")|(" );
      regAll.Append( StringRegEx );
      if( regPreproc.Length > 0 )
      {
        regAll.Append( ")|(" );
        regAll.Append( regPreproc );
      }
      regAll.Append( ")|(" );
      regAll.Append( regKeyword );
      regAll.Append( ")" );

      RegexOptions caseInsensitive = CaseSensitive ? 0 : RegexOptions.IgnoreCase;
      CodeRegex = new Regex( regAll.ToString(), RegexOptions.Singleline | caseInsensitive );

      CodeParagraphGlobal = new List<Run>();
    }

    protected override string MatchEval( Match match ) //protected override
    {
      if( match.Groups[ 1 ].Success ) //comment
      {
        StringReader reader = new StringReader( match.ToString() );
        StringBuilder sb = new StringBuilder();
        string line;
        bool firstLineRead = false;
        while( ( line = reader.ReadLine() ) != null )
        {
          if( firstLineRead )
            sb.Append( "\r" );

          sb.Append( line );
          firstLineRead = true;
        }

        if( !string.IsNullOrEmpty( sb.ToString() ) )
        {
          Run r = new Run( sb.ToString() );
          r.Foreground = new SolidColorBrush( Color.FromRgb( 0, 128, 0 ) );
          CodeParagraphGlobal.Add( r );
        }
        return "::::::";
      }
      else if( match.Groups[ 2 ].Success ) //string literal
      {
        Run r = new Run( match.ToString() );
        r.Foreground = new SolidColorBrush( Color.FromRgb( 0, 96, 128 ) );

        CodeParagraphGlobal.Add( r );
        return "::::::";
      }
      else if( match.Groups[ 3 ].Success ) //preprocessor keyword
      {
        Run r = new Run( match.ToString() );
        r.Foreground = new SolidColorBrush( Color.FromRgb( 204, 102, 51 ) );

        CodeParagraphGlobal.Add( r );
        return "::::::";
      }
      else if( match.Groups[ 4 ].Success ) //keyword
      {
        Run r = new Run( match.ToString() );
        r.Foreground = new SolidColorBrush( Color.FromRgb( 0, 0, 255 ) );

        CodeParagraphGlobal.Add( r );
        return "::::::";
      }
      else
      {
        return "";
      }
    }
  }

  public abstract class SourceFormat
  {
    protected SourceFormat()
    {
      _tabSpaces = 4;
      _lineNumbers = false;
      _alternate = false;
      _embedStyleSheet = false;
    }

    private byte _tabSpaces;

    public byte TabSpaces
    {
      get
      {
        return _tabSpaces;
      }
      set
      {
        _tabSpaces = value;
      }
    }

    private bool _lineNumbers;

    public bool LineNumbers
    {
      get
      {
        return _lineNumbers;
      }
      set
      {
        _lineNumbers = value;
      }
    }

    private bool _alternate;

    public bool Alternate
    {
      get
      {
        return _alternate;
      }
      set
      {
        _alternate = value;
      }
    }

    private bool _embedStyleSheet;

    public bool EmbedStyleSheet
    {
      get
      {
        return _embedStyleSheet;
      }
      set
      {
        _embedStyleSheet = value;
      }
    }

    public Paragraph FormatCode( string source )
    {
      return FormatCode( source, _lineNumbers, _alternate, _embedStyleSheet, false );
    }

    private Regex codeRegex;

    protected Regex CodeRegex
    {
      get
      {
        return codeRegex;
      }
      set
      {
        codeRegex = value;
      }
    }

    private List<Run> codeParagraphGlobal;
    protected List<Run> CodeParagraphGlobal
    {
      get
      {
        return codeParagraphGlobal;
      }
      set
      {
        codeParagraphGlobal = value;
      }
    }

    protected abstract string MatchEval( Match match ); //protected abstract

    //does the formatting job
    private Paragraph FormatCode( string source, bool lineNumbers,
        bool alternate, bool embedStyleSheet, bool subCode )
    {
      Paragraph codeParagraph = new Paragraph();
      //replace special characters
      StringBuilder sb = new StringBuilder( source );
      //color the code
      source = codeRegex.Replace( sb.ToString(), new MatchEvaluator( this.MatchEval ) );
      //codeRegex.Replace(
      string[] characters = { "::::::" };

      string[] split = source.Split( characters, new StringSplitOptions() );
      int currentChunk = 0;
      foreach( string code in split )
      {
        currentChunk++;
        Run r = new Run( code );
        codeParagraph.Inlines.Add( r );
        if( ( currentChunk - 1 ) < codeParagraphGlobal.Count )
        {
          codeParagraph.Inlines.Add( codeParagraphGlobal[ currentChunk - 1 ] );
        }
      }

      return codeParagraph;
    }

  }
}
