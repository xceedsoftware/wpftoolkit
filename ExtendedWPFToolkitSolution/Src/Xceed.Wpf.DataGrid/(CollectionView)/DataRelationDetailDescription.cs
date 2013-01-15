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
using System.Data;
using System.Diagnostics;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class DataRelationDetailDescription : DataGridDetailDescription
  {
    public DataRelationDetailDescription()
      : base()
    {
    }

    public DataRelationDetailDescription( DataRelation relation )
      : this()
    {
      if( relation == null )
        throw new ArgumentNullException( "relation" );

      this.DataRelation = relation;
      m_userAssignedDataRelation = true;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    public DataRelation DataRelation
    {
      get
      {
        return m_dataRelation;
      }
      set
      {
        if( value == null )
          throw new ArgumentNullException( "DataRelation" );

        if( this.InternalIsSealed )
          throw new InvalidOperationException( "An attempt was made to set the DataRelation property after the DataRelationDetailDescription has been sealed." );

        m_dataRelation = value;
        this.RelationName = value.RelationName;
        this.Seal();
      }
    }

    protected internal override void Initialize( DataGridCollectionViewBase parentCollectionView )
    {
      base.Initialize( parentCollectionView );

      if( this.DataRelation != null )
        return;

      string relationName = this.RelationName;

      if( string.IsNullOrEmpty( relationName ) )
        throw new InvalidOperationException( "An attempt was made to initialize a DataRelationDetailDescription whose RelationName property has not been set." );

      this.DataRelation = this.FindDataRelation( parentCollectionView, relationName );
    }

    protected internal override IEnumerable GetDetailsForParentItem( DataGridCollectionViewBase parentCollectionView, object parentItem )
    {
      if( this.DataRelation == null )
        throw new InvalidOperationException( "An attempt was made to obtain the details of a DataRelationDetailDescription object whose DataRelation property has not been set." );

      this.Seal();

      System.Data.DataRow dataRow = parentItem as System.Data.DataRow;
      DataRowView dataRowView;

      if( dataRow != null )
      {
        int rawIndex = parentCollectionView.IndexOfSourceItem( dataRow );

        DataView dataView = parentCollectionView.SourceCollection as DataView;

        if( dataView == null )
          return null;

        dataRowView = dataView[ rawIndex ];
      }
      else
      {
        dataRowView = parentItem as DataRowView;
      }

      if( dataRowView == null )
        return null;

      if( !m_userAssignedDataRelation )
      {
        if( m_dataRelation.ParentTable != dataRowView.Row.Table )
        {
          m_dataRelation = this.FindDataRelation( parentCollectionView, this.RelationName );
        }
      }

      return dataRowView.CreateChildView( m_dataRelation );
    }

    private DataRelation FindDataRelation( DataGridCollectionViewBase parentCollectionView, string relationName )
    {
      DataView view = parentCollectionView.SourceCollection as DataView;

      if( view == null )
        throw new InvalidOperationException( "An attempt was made to initialize a DataRelationDetailDescription whose data source is not a DataView." );

      foreach( DataRelation relation in view.Table.ChildRelations )
      {
        if( relation.RelationName == relationName )
        {
          return relation;
        }
      }

      throw new InvalidOperationException( "An attempt was made to initialize a DataRelationDetailDescription whose data source does not contain a DataRelation corresponding to the specified name." );
    }

    private DataRelation m_dataRelation;
    private bool m_userAssignedDataRelation;
  }
}
