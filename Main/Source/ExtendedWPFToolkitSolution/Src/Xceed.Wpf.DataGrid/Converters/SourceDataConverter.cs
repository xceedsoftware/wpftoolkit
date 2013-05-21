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
using System.Windows;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Controls;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid.Converters
{
  public class SourceDataConverter : IValueConverter
  {
    public SourceDataConverter()
      : this( false, null )
    {
    }

    public SourceDataConverter( bool sourceSupportsDBNull )
      : this( sourceSupportsDBNull, null )
    {
    }

    internal SourceDataConverter( bool sourceSupportsDBNull, CultureInfo sourceCulture )
    {
      m_sourceSupportsDBNull = sourceSupportsDBNull;
      m_sourceCulture = sourceCulture;

      if( m_sourceCulture == null )
        m_sourceCulture = CultureInfo.InvariantCulture;
    }

    public object Convert( object sourceValue, Type targetType, object parameter, CultureInfo targetCulture )
    {
      if( ( sourceValue == DBNull.Value ) || ( sourceValue == null ) )
        return null;

      Exception exception;
      return DefaultDataConverter.TryConvert( sourceValue, targetType, CultureInfo.InvariantCulture, targetCulture, out exception );
    }

    public object ConvertBack( 
      object targetValue, 
      Type sourceType,
      object parameter, 
      CultureInfo targetCulture )
    {
      Exception exception;
      return this.TryConvertBack( targetValue, sourceType, m_sourceCulture, targetCulture, out exception );
    }

    internal object TryConvertBack( 
      object targetValue, 
      Type sourceType,
      CultureInfo sourceCulture,
      CultureInfo targetCulture,
      out Exception exception )
    {
      exception = null;

      if( ( targetValue == null ) || ( targetValue == DBNull.Value ) 
        || ( ( sourceType != typeof( string ) ) && ( string.Empty.Equals( targetValue ) ) ) )
      {
        if( m_sourceSupportsDBNull )
        {
          return DBNull.Value;
        }
        else
        {
          return null;
        }
      }

      return DefaultDataConverter.TryConvert( targetValue, sourceType, targetCulture, sourceCulture, out exception );
    }

    public bool SourceSupportsDBNull
    {
      get
      {
        return m_sourceSupportsDBNull;
      }

      set
      {
        m_sourceSupportsDBNull = value;
      }
    }

    bool m_sourceSupportsDBNull; // = false
    CultureInfo m_sourceCulture;
  }
}
