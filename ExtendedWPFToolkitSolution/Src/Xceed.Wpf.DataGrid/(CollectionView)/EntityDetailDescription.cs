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
using System.Collections;
using System.ComponentModel;
using System.Data.Objects.DataClasses;
using System.Reflection;

namespace Xceed.Wpf.DataGrid
{
  internal class EntityDetailDescription : DataGridDetailDescription
  {
    public EntityDetailDescription()
    {
    }

    public EntityDetailDescription( string propertyName )
      : this()
    {
      if( string.IsNullOrEmpty( propertyName ) )
        throw new ArgumentException( "The specified property name is null or empty", "propertyName" );

      this.RelationName = propertyName;
    }

    #region QueryDetails Event

    public event EventHandler<QueryEntityDetailsEventArgs> QueryDetails;

    protected virtual void OnQueryDetails( QueryEntityDetailsEventArgs e )
    {
      var handler = this.QueryDetails;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

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
  }
}
