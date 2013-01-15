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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  // We re-implement IList and IList<object> to override the implementation of the 
  // IndexOf and Contains methods to use our optimized way.
  internal class OptimizedReadOnlyObservableCollection : ReadOnlyObservableCollection<object>, IList, IList<object>
  {
    #region CONSTRUCTORS

    public OptimizedReadOnlyObservableCollection( DataGridCollectionViewGroup dataGridCollectionViewGroup )
      : base( dataGridCollectionViewGroup.ProtectedItems )
    {
      if( dataGridCollectionViewGroup == null )
        throw new ArgumentNullException( "dataGridCollectionViewGroup" );

      m_dataGridCollectionViewGroup = dataGridCollectionViewGroup;
    }

    #endregion CONSTRUCTORS

    #region "Newed" Methods

    public new int IndexOf( object item )
    {
      // The DataGridCollectionViewGroup has been optimized to use the information
      // stored on the RawItem associated with the data item instead of searching
      // the item in the list.
      return m_dataGridCollectionViewGroup.IndexOf( item );
    }

    public new bool Contains( object item )
    {
      // The DataGridCollectionViewGroup has been optimized to use the information
      // stored on the RawItem associated with the data item instead of searching
      // the item in the list.
      return m_dataGridCollectionViewGroup.Contains( item );
    }

    #endregion "Newed" Methods

    #region PRIVATE FIELDS

    private DataGridCollectionViewGroup m_dataGridCollectionViewGroup;

    #endregion PRIVATE FIELDS
  }
}
