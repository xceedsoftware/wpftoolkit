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
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Xceed.Wpf.DataGrid.Converters
{
  [ValueConversion( typeof( object ), typeof( string ) )]
  public class StringFormatConverter : IValueConverter
  {
    #region FormatProvider Property

    private IFormatProvider m_formatProvider;

    public IFormatProvider FormatProvider
    {
      get
      {
        return m_formatProvider;
      }
      set
      {
        m_formatProvider = value;
      }
    }

    #endregion FormatProvider Property

    #region IValueConverter Members

    public virtual object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( ( value == null ) || ( value is DBNull ) )
        return string.Empty;

      string format = StringFormatConverter.GetFormat( parameter as string );

      if( format == null )
      {
        return value;
      }
      else
      {
        if( m_formatProvider == null )
        {
          return string.Format( culture, format, value );
        }
        else
        {
          return string.Format( m_formatProvider, format, value );
        }
      }
    }

    public virtual object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      return Binding.DoNothing;
    }

    #endregion

    private static string GetFormat( string format )
    {
      if( format == null )
        return null;

      int formatLength = format.Length;
      StringBuilder workingFormat = new StringBuilder( formatLength );
      char character;
      bool previousIsOpenBracket = false;

      for( int i = 0; i < formatLength; i++ )
      {
        character = format[ i ];

        if( character == '[' )
        {
          if( previousIsOpenBracket )
          {
            workingFormat.Append( '[' );
            previousIsOpenBracket = false;
          }
          else
          {
            previousIsOpenBracket = true;
          }
        }
        else if( character == ']' )
        {
          if( previousIsOpenBracket )
          {
            workingFormat.Append( ']' );
            previousIsOpenBracket = false;
          }
          else
          {
            workingFormat.Append( '}' );
          }
        }
        else
        {
          if( previousIsOpenBracket )
          {
            workingFormat.Append( "{0:" );
            previousIsOpenBracket = false;
          }

          workingFormat.Append( character );
        }
      }

      string resultingFormat = workingFormat.ToString();

      if( ( !resultingFormat.Contains( "{0:" ) ) && ( !resultingFormat.Contains( "{0}" ) ) )
      {
        workingFormat.Insert( 0, "{0:" );
        workingFormat.Append( '}' );
        resultingFormat = workingFormat.ToString();
      }

      return resultingFormat;
    }
  }
}
