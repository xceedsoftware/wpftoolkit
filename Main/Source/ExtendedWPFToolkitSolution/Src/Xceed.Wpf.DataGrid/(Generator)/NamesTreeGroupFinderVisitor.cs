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
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal class NamesTreeGroupFinderVisitor : IDataGridContextVisitor
  {
    public NamesTreeGroupFinderVisitor( object[] namesTree )
    {
      if( namesTree == null )
        throw new ArgumentNullException( "namesTree" );

      if( namesTree.Length == 0 )
        throw new ArgumentException( "namesTree should not be an empty array", "namesTree" );

      m_groupNamesTreeKey = new GroupNamesTreeKey( namesTree );
    }

    #region CollectionViewGroup Group

    public CollectionViewGroup Group
    {
      get;
      private set;
    }

    #endregion

    #region IDataGridContextVisitor Members

    public void Visit( DataGridContext sourceContext, System.Windows.Data.CollectionViewGroup group, object[] namesTree, int groupLevel, bool isExpanded, bool isComputedExpanded, ref bool stopVisit )
    {
      if( this.Group == null )
      {
        GroupNamesTreeKey currentGroupKey = new GroupNamesTreeKey( namesTree );

        if( currentGroupKey.Equals( m_groupNamesTreeKey ) == true )
          this.Group = group;

      }
    }

    public void Visit( DataGridContext sourceContext, ref bool stopVisit )
    {
      throw new NotSupportedException();
    }

    public void Visit( DataGridContext sourceContext, int startSourceDataItemIndex, int endSourceDataItemIndex, ref bool stopVisit )
    {
      throw new NotSupportedException();
    }

    public void Visit( DataGridContext sourceContext, int sourceDataItemIndex, object item, ref bool stopVisit )
    {
      throw new NotSupportedException();
    }

    public void Visit( DataGridContext sourceContext, System.Windows.DataTemplate headerFooter, ref bool stopVisit )
    {
      throw new NotSupportedException();
    }

    public void Visit( DataGridContext sourceContext, GroupHeaderFooterItem groupHeaderFooter, ref bool stopVisit )
    {
      throw new NotSupportedException();
    }

    #endregion

    private GroupNamesTreeKey m_groupNamesTreeKey;
  }
}
