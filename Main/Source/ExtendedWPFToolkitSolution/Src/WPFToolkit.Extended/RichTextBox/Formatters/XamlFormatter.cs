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
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace Microsoft.Windows.Controls
{
  /// <summary>
  /// Formats the RichTextBox text as Xaml
  /// </summary>
  public class XamlFormatter : ITextFormatter
  {
    public string GetText( System.Windows.Documents.FlowDocument document )
    {
      TextRange tr = new TextRange( document.ContentStart, document.ContentEnd );
      using( MemoryStream ms = new MemoryStream() )
      {
        tr.Save( ms, DataFormats.Xaml );
        return ASCIIEncoding.Default.GetString( ms.ToArray() );
      }
    }

    public void SetText( System.Windows.Documents.FlowDocument document, string text )
    {
      try
      {
        //if the text is null/empty clear the contents of the RTB. If you were to pass a null/empty string
        //to the TextRange.Load method an exception would occur.
        if( String.IsNullOrEmpty( text ) )
        {
          document.Blocks.Clear();
        }
        else
        {
          TextRange tr = new TextRange( document.ContentStart, document.ContentEnd );
          using( MemoryStream ms = new MemoryStream( Encoding.ASCII.GetBytes( text ) ) )
          {
            tr.Load( ms, DataFormats.Xaml );
          }
        }
      }
      catch
      {
        throw new InvalidDataException( "Data provided is not in the correct Xaml format." );
      }
    }
  }
}
