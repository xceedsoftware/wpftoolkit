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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Security;

namespace Xceed.Wpf.DataGrid.Export
{
  [SecurityCritical]
  [Serializable]
  internal class XceedDataObject : IDataObject
  {
    public XceedDataObject()
    {
      m_formatToValue = new Dictionary<string, object>();
    }

    #region PRIVATE FIELDS

    private Dictionary<string, object> m_formatToValue; // = null;

    #endregion

    #region IDataObject Members

    object IDataObject.GetData( string format, bool autoConvert )
    {
      return ( ( IDataObject )this ).GetData( format );
    }

    object IDataObject.GetData( Type format )
    {
      throw new NotSupportedException();
    }

    object IDataObject.GetData( string format )
    {
      object value = null;

      m_formatToValue.TryGetValue( format, out value );

      return value;
    }

    bool IDataObject.GetDataPresent( string format, bool autoConvert )
    {
      return ( ( IDataObject )this ).GetDataPresent( format );
    }

    bool IDataObject.GetDataPresent( Type format )
    {
      throw new NotSupportedException();
    }

    bool IDataObject.GetDataPresent( string format )
    {
      return m_formatToValue.ContainsKey( format );
    }

    string[] IDataObject.GetFormats( bool autoConvert )
    {
      return ( ( IDataObject )this ).GetFormats();
    }

    string[] IDataObject.GetFormats()
    {
      return m_formatToValue.Keys.ToArray<string>();
    }

    void IDataObject.SetData( string format, object data, bool autoConvert )
    {
      ( ( IDataObject )this ).SetData( format, data );
    }

    void IDataObject.SetData( Type format, object data )
    {
      throw new NotSupportedException();
    }

    void IDataObject.SetData( string format, object data )
    {
      if( m_formatToValue.ContainsKey( format ) )
      {
        m_formatToValue[ format ] = data;
      }
      else
      {
        m_formatToValue.Add( format, data );
      }
    }

    void IDataObject.SetData( object data )
    {
      throw new NotSupportedException();
    }

    #endregion
  }
}
