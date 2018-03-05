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
using System.Collections.ObjectModel;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridDetailDescriptionCollection : ObservableCollection<DataGridDetailDescription>
  {
    public DataGridDetailDescriptionCollection()
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1002:DoNotExposeGenericLists" )]
    public DataGridDetailDescriptionCollection( List<DataGridDetailDescription> detailDescriptions )
      : base( detailDescriptions )
    {
    }

    public DataGridDetailDescription this[ string relationName ]
    {
      get
      {
        var index = this.IndexOf( relationName );
        if( index < 0 )
          return null;

        return this.Items[ index ];
      }
    }

    public int IndexOf( string relationName )
    {
      var items = this.Items;
      var count = items.Count;

      for( int i = 0; i < count; i++ )
      {
        if( string.Equals( items[ i ].RelationName, relationName ) )
          return i;
      }

      return -1;
    }

    protected override void SetItem( int index, DataGridDetailDescription item )
    {
      if( string.IsNullOrEmpty( item.RelationName ) )
        throw new ArgumentException( "The RelationName property of the specified DataGridDetailDescription cannot be null or empty.", "item" );

      base.SetItem( index, item );
    }

    protected override void InsertItem( int index, DataGridDetailDescription item )
    {
      if( string.IsNullOrEmpty( item.RelationName ) )
        throw new ArgumentException( "The RelationName property of the specified DataGridDetailDescription cannot be null or empty.", "item" );

      base.InsertItem( index, item );
    }
  }
}
