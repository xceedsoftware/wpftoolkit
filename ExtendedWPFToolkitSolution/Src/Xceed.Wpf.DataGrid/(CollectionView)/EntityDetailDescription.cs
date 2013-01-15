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
using System.Collections;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Reflection;
using System.Windows;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  internal class EntityDetailDescription : DataGridDetailDescription
  {
    #region CONSTRUCTORS

    public EntityDetailDescription()
      : base()
    {
    }

    public EntityDetailDescription( string propertyName )
      : this()
    {
      if( string.IsNullOrEmpty( propertyName ) )
        throw new ArgumentException( "The specified property name is null or empty", "propertyName" );

      this.RelationName = propertyName;
    }

    #endregion CONSTRUCTORS

    #region QueryDetails Event

    public event EventHandler<QueryEntityDetailsEventArgs> QueryDetails;

    protected virtual void OnQueryDetails( QueryEntityDetailsEventArgs e )
    {
      if( this.QueryDetails != null )
        this.QueryDetails( this, e );
    }

    #endregion QueryDetails Event

    #region DataGridDetailDescription Overrides

    protected internal override IEnumerable GetDetailsForParentItem( DataGridCollectionViewBase parentCollectionView, object parentItem )
    {
      EntityObject entityObject = parentItem as EntityObject;

      if( entityObject == null )
        return null;

      // Even if EntityObject is not in a loadable state, we must still return the IList
      // so that the ItemProperties can be extracted based on the elements type.
      bool entityObjectLoadable = ItemsSourceHelper.IsEntityObjectLoadable( entityObject );

      // We let the user take charge of handling the details.
      QueryEntityDetailsEventArgs args = new QueryEntityDetailsEventArgs( entityObject );

      if( entityObjectLoadable )
        this.OnQueryDetails( args );

      // The parentItem must implement IEntityWithRelationships
      Type parentItemType = parentItem.GetType();
      if( typeof( IEntityWithRelationships ).IsAssignableFrom( parentItemType ) )
      {
        // Since the relationship was based on the the property 
        // name, we must find that property using reflection.
        PropertyInfo propertyInfo = parentItemType.GetProperty( this.RelationName );

        if( propertyInfo != null )
        {
          RelatedEnd relatedEnd = propertyInfo.GetValue( parentItem, null ) as RelatedEnd;

          if( relatedEnd != null )
          {
            // Make sure that the details are loaded 
            // except if the user already handled it.
            if( !relatedEnd.IsLoaded 
              && !args.Handled 
              && entityObjectLoadable )
            {
              relatedEnd.Load();
            }

            IListSource listSource = relatedEnd as IListSource;

            // Returns an IList to have proper change notification events.
            if( listSource != null )
              return listSource.GetList();
          }
        }
      }

      return null;
    }

    #endregion DataGridDetailDescription Overrides
  }
}
