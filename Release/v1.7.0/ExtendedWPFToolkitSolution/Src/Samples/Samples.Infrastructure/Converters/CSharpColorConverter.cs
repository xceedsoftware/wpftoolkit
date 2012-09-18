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

using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Samples.Infrastructure.Core.CodeFormatting;

namespace Samples.Infrastructure.Converters
{
  public class CSharpColorConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( value == null )
        return value;

      String val = ( String )value;

      CSharpFormat cSharpFormat = new CSharpFormat();
      FlowDocument doc = new FlowDocument();
      Paragraph p = new Paragraph();
      p = cSharpFormat.FormatCode( val );
      doc.Blocks.Add( p );

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
  }
}
