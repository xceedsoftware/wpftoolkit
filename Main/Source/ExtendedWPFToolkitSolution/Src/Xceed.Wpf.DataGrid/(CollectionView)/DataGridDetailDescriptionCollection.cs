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
using System.Text;
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
        int index = this.IndexOf( relationName );

        if( index == -1 )
          return null;

        return this.Items[ index ];
      }
    }

    public int IndexOf( string relationName )
    {
      IList<DataGridDetailDescription> items = this.Items;
      int count = items.Count;

      for( int i = 0; i < count; i++ )
      {
        if( string.Equals( items[ i ].RelationName, relationName ) )
          return i;
      }

      return -1;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly" )]
    internal ICollection<DataGridDetailDescription> DefaultDetailDescriptions
    {
      get
      {
        return m_defaultDetailDescriptions;
      }
      set
      {
        if( m_defaultDetailDescriptions == null )
          m_defaultDetailDescriptions = value;
      }
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

    private DataGridDetailDescription FindDefaultDetailDescription( string name )
    {
      if( m_defaultDetailDescriptions != null )
      {
        foreach( DataGridDetailDescription detailDescription in m_defaultDetailDescriptions )
        {
          if( string.Equals( detailDescription.RelationName, name ) )
            return detailDescription;
        }
      }

      return null;
    }

    private ICollection<DataGridDetailDescription> m_defaultDetailDescriptions;

  }
}
