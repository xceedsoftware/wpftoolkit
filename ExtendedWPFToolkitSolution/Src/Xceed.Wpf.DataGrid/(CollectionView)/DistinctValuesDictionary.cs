/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Diagnostics;
using Xceed.Utils.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class DistinctValuesDictionary : ReadOnlyDictionary<string, ReadOnlyObservableHashList>
  {
    public DistinctValuesDictionary( DataGridCollectionViewBase dataGridCollectionViewBase )
    {
      if( dataGridCollectionViewBase == null )
        throw new DataGridInternalException( "dataGridCollectionView is null." );

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
      }

      Debug.Assert( value != null );

      return value;
    }

    private DataGridCollectionViewBase m_dataGridCollectionViewBase;
  }
}
