/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid.Converters
{
  [ValueConversion( typeof( object ), typeof( string ) )]
  public class StatResultConverter : StringFormatConverter
  {
    #region OverflowMessage Property

    private string m_overflowMessage = "#OVER#";

    public string OverflowMessage
    {
      get
      {
        return m_overflowMessage;
      }
      set
      {
        m_overflowMessage = value;
      }
    }

    #endregion OverflowMessage Property

    #region DivisionByZeroMessage Property

    private string m_divisionByZeroMessage = "#DIV/0#";

    public string DivisionByZeroMessage
    {
      get
      {
        return m_divisionByZeroMessage;
      }
      set
      {
        m_divisionByZeroMessage = value;
      }
    }

    #endregion DivisionByZeroMessage Property

    #region InvalidValueMessage Property

    private string m_invalidValueMessage = "#VALUE#";

    public string InvalidValueMessage
    {
      get
      {
        return m_invalidValueMessage;
      }
      set
      {
        m_invalidValueMessage = value;
      }
    }

    #endregion InvalidValueMessage Property

    #region IValueConverter Members

    public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( ( value == null ) || ( value is DBNull ) )
        return null;

      if( value is DivideByZeroException )
        return m_divisionByZeroMessage;

      if( value is OverflowException )
        return m_overflowMessage;

      if( value is Stats.InvalidValueException )
        return m_invalidValueMessage;

      Stats.InvalidSourcePropertyNameException invalidSourcePropertyNameException = value as Stats.InvalidSourcePropertyNameException;

      if( invalidSourcePropertyNameException != null )
        return "#" + invalidSourcePropertyNameException.SourcePropertyName + "#";

      Exception exception = value as Exception;

      if( exception != null )
        return exception.Message;

      return base.Convert( value, targetType, parameter, culture );
    }

    #endregion
  }
}
