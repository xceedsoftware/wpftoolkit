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
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace Xceed.Utils.Data
{
  internal sealed class ValueTypeDataStore<T> : DataStore
    where T : struct
  {
    public ValueTypeDataStore( int initialCapacity )
    {
      this.SetCapacity( initialCapacity );
    }

    public override int Compare( int xRecordIndex, int yRecordIndex )
    {
      if( m_valuesIsNull[ xRecordIndex ] == ValueNull )
      {
        if( m_valuesIsNull[ yRecordIndex ] == ValueNull )
        {
          return 0;
        }
        else
        {
          return -1;
        }
      }
      else
      {
        if( m_valuesIsNull[ yRecordIndex ] == ValueNull )
        {
          return 1;
        }
      }

      return Comparer<T>.Default.Compare( m_values[ xRecordIndex ], m_values[ yRecordIndex ] );
    }

    public override object GetData( int recordIndex )
    {
      if( m_valuesIsNull[ recordIndex ] == ValueNull )
        return null;

      return m_values[ recordIndex ];
    }

    public override void SetData( int recordIndex, object data )
    {
      if( ( data == null ) || ( data == DBNull.Value ) )
      {
        m_valuesIsNull[ recordIndex ] = ValueNull;
        return;
      }

      if( data is T )
      {
        m_valuesIsNull[ recordIndex ] = ValueNotNull;
        m_values[ recordIndex ] = ( T )data;
        return;
      }

      throw new ArgumentException( "The data must be of " + typeof( T ).ToString() + " type or null (Nothing in Visual Basic).", "data" );
    }

    public override void SetCapacity( int capacity )
    {
      T[] newValues = new T[ capacity ];
      byte[] newValuesIsNull = new byte[ capacity ];

      if( m_values != null )
      {
        int copyCount = System.Math.Min( capacity, m_values.Length );
        Array.Copy( m_values, 0, newValues, 0, copyCount );
        Debug.Assert( ( m_valuesIsNull != null ) && ( m_valuesIsNull.Length == m_values.Length ) );
        Array.Copy( m_valuesIsNull, 0, newValuesIsNull, 0, copyCount );
      }

      m_values = newValues;
      m_valuesIsNull = newValuesIsNull;
    }

    private const byte ValueNull = 0;
    private const byte ValueNotNull = 1;

    private T[] m_values;
    private byte[] m_valuesIsNull;
  }
}
