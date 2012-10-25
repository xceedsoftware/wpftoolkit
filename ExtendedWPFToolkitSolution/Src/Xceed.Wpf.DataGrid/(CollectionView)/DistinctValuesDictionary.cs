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
using System.Text;
using Xceed.Utils.Collections;
using System.Diagnostics;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class DistinctValuesDictionary : ReadOnlyDictionary<string, ReadOnlyObservableHashList>
  {
    public DistinctValuesDictionary( DataGridCollectionViewBase dataGridCollectionViewBase )
    {
      if( dataGridCollectionViewBase == null )
        throw new DataGridInternalException();

      m_dataGridCollectionViewBase = dataGridCollectionViewBase;
    }

    public override ReadOnlyObservableHashList this[ string key ]
    {
      get
      {
        return this.AssertValueIsCreated( key );
      }
      set
      {
        base[ key ] = value;
      }
    }

    public override bool TryGetValue( string key, out ReadOnlyObservableHashList value )
    {
      // We force the creation of the key in the Dictionary
      value = this.AssertValueIsCreated( key );

      return true;
    }

    internal bool InternalTryGetValue( string key, out ReadOnlyObservableHashList value )
    {
      return base.TryGetValue( key, out value );
    }

    private ReadOnlyObservableHashList AssertValueIsCreated( string key )
    {
      ReadOnlyObservableHashList value = null;

      if( !base.TryGetValue( key, out value ) )
      {
        value = new ReadOnlyObservableHashList();
        this.InternalAdd( key, value );
        m_dataGridCollectionViewBase.ForceRefreshDistinctValuesForFieldName( key, value.InnerObservableHashList );
      }

      Debug.Assert( value != null );

      return value;
    }

    private DataGridCollectionViewBase m_dataGridCollectionViewBase;
  }
}
