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

namespace Xceed.Utils.Data
{
  internal sealed class BoolDataStore : DataStore
  {
    public BoolDataStore( int initialCapacity )
    {
      this.SetCapacity( initialCapacity );
    }

    public override object GetData( int recordIndex )
    {
      switch( m_values[ recordIndex ] )
      {
        case BoolDataStoreValue.False:
          return false;

        case BoolDataStoreValue.True:
          return true;
      }

      return null;
    }

    public override void SetData( int recordIndex, object data )
    {
      if( ( data == null ) || ( data == DBNull.Value ) )
      {
        m_values[ recordIndex ] = BoolDataStoreValue.Null;
        return;
      }

      if( data is bool )
      {
        m_values[ recordIndex ] = ( ( bool )data ) ? BoolDataStoreValue.True : BoolDataStoreValue.False;
        return;
      }

      throw new ArgumentException( "The data must be of " + typeof( bool ).ToString() + " type or null (Nothing in Visual Basic).", "data" );
    }

    public override int Compare( int xRecordIndex, int yRecordIndex )
    {
      return ( m_values[ xRecordIndex ] - m_values[ yRecordIndex ] );
    }

    public override void SetCapacity( int capacity )
    {
      BoolDataStoreValue[] newValues = new BoolDataStoreValue[ capacity ];

      if( m_values != null )
        Array.Copy( m_values, 0, newValues, 0, System.Math.Min( capacity, m_values.Length ) );

      m_values = newValues;
    }

    private BoolDataStoreValue[] m_values;
  }

  internal enum BoolDataStoreValue : sbyte
  {
    Null = 0,
    False = 1,
    True = 2
  }
}
