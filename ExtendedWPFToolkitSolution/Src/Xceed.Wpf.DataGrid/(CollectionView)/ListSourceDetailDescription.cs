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
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  internal class ListSourceDetailDescription : DataGridDetailDescription
  {
    public ListSourceDetailDescription()
      : base()
    {
      this.RelationName = "Children";
    }

    protected internal override IEnumerable GetDetailsForParentItem( DataGridCollectionViewBase parentCollectionView, object parentItem )
    {
      IListSource listSource = parentItem as IListSource;

      if( listSource == null )
        return null;

      this.Seal();

      return listSource.GetList();

    }
  }
}
