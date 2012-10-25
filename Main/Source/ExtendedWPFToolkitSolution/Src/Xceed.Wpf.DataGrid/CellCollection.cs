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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xceed.Wpf.DataGrid
{
  public class CellCollection : Collection<Cell>
  {
    public CellCollection()
    {
    }

    internal CellCollection( IList<Cell> cells )
      : base( cells )
    {
    }

    public virtual Cell this[ ColumnBase column ]
    {
      get
      {
        int count = this.Count;

        for( int i = 0; i < count; i++ )
        {
          Cell cell = this[ i ];

          if( cell.ParentColumn == column )
            return cell;
        }

        return null;
      }
    }

    public virtual Cell this[ string fieldName ]
    {
      get
      {
        int count = this.Count;

        for( int i = 0; i < count; i++ )
        {
          Cell cell = this[ i ];

          if( string.Equals( cell.FieldName, fieldName ) )
            return cell;
        }

        return null;
      }
    }

    protected override void InsertItem( int index, Cell item )
    {
      base.InsertItem( index, item );
    }

    protected override void RemoveItem( int index )
    {
      base.RemoveItem( index );
    }

    protected override void SetItem( int index, Cell item )
    {
      base.SetItem( index, item );
    }


    protected override void ClearItems()
    {
      base.ClearItems();
    }

    internal virtual void InternalAdd( Cell cell )
    {
      this.Items.Add( cell );
    }

    internal virtual void InternalClear()
    {
      this.Items.Clear();
    }

    internal virtual void InternalInsert( int index, Cell cell )
    {
      this.Items.Insert( index, cell );
    }

    internal virtual void InternalRemove( Cell cell )
    {
      this.Items.Remove( cell );
    }

    internal virtual void InternalRemoveAt( int index )
    {
      this.Items.RemoveAt( index );
    }

    internal virtual void InternalSetCell( int index, Cell cell )
    {
      this.Items[ index ] = cell;
    }
  }
}
