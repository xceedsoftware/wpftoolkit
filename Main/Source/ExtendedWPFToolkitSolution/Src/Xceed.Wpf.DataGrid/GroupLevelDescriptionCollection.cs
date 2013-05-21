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
using System.Collections.ObjectModel;

namespace Xceed.Wpf.DataGrid
{
  public class GroupLevelDescriptionCollection : ReadOnlyObservableCollection<GroupLevelDescription>
  {
    public GroupLevelDescriptionCollection()
      : base( new ObservableCollection<GroupLevelDescription>() )
    {
    }

    internal void Add( GroupLevelDescription info )
    {
      this.Items.Add( info );
    }

    internal void Insert( int index, GroupLevelDescription info )
    {
      this.Items.Insert( index, info );
    }

    internal bool Remove( GroupLevelDescription info )
    {
      return this.Items.Remove( info );
    }

    internal void RemoveAt( int index )
    {
      this.Items.RemoveAt( index );
    }

    internal void Clear()
    {
      this.Items.Clear();
    }
  }
}
