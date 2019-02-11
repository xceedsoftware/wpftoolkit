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

namespace Xceed.Wpf.DataGrid
{
  internal class PropertyDetailDescription : DataGridDetailDescription
  {
    public PropertyDetailDescription()
      : base()
    {
    }

    public PropertyDetailDescription( PropertyDescriptor relation )
      : this()
    {
      if( relation == null )
        throw new ArgumentNullException( "relation" );

      this.PropertyDescriptor = relation;
    }

    #region PropertyDescriptor Property

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    public PropertyDescriptor PropertyDescriptor
    {
      get
      {
        return m_propertyDescriptor;
      }
      set
      {
        if( value == null )
          throw new ArgumentNullException( "PropertyDescriptor" );

        if( this.InternalIsSealed == true )
          throw new InvalidOperationException( "An attempt was made to set the PropertyDescriptor property after the PropertyDetailDescription has been sealed." );

        m_propertyDescriptor = value;
        this.RelationName = value.Name;

        this.Seal();
      }
    }

    private PropertyDescriptor m_propertyDescriptor;

    #endregion

    protected internal override void Initialize( DataGridCollectionViewBase parentCollectionView )
    {
      base.Initialize( parentCollectionView );

      if( this.PropertyDescriptor != null )
        return;

      string relationName = this.RelationName;

      if( String.IsNullOrEmpty( relationName ) == true )
        throw new InvalidOperationException( "An attempt was made to initialize a PropertyDetailDescription whose Name property has not been set." );

      var enumeration = parentCollectionView as IEnumerable;
      if( enumeration != null )
      {
        // Try to get it from the first item in the DataGridCollectionView
        var firstItem = ItemsSourceHelper.GetFirstItemByEnumerable( enumeration );
        if( firstItem != null )
        {
          var propertyDescriptor = this.GetPropertyDescriptorFromFirstItem( firstItem );
          if( propertyDescriptor != null )
          {
            this.PropertyDescriptor = propertyDescriptor;
            return;
          }
        }
      }

      // If the list is empty, check if the SourceCollection is ITypedList
      var typedList = parentCollectionView.SourceCollection as ITypedList;
      if( typedList != null )
      {
        var propertyDescriptor = this.GetPropertyDescriptorFromITypedList( typedList );
        if( propertyDescriptor != null )
        {
          this.PropertyDescriptor = propertyDescriptor;
          return;
        }
      }

      throw new InvalidOperationException( "An attempt was made to initialize a PropertyDetailDescription whose data source does not contain a property that corresponds to the specified relation name." );
    }

    private PropertyDescriptor GetPropertyDescriptorFromITypedList( ITypedList typedList )
    {
      if( string.IsNullOrEmpty( this.RelationName ) || ( typedList == null ) )
        return null;

      var properties = typedList.GetItemProperties( null );
      if( ( properties != null ) && ( properties.Count > 0 ) )
        return properties[ this.RelationName ];

      var listName = typedList.GetListName( null );
      if( string.IsNullOrEmpty( listName ) )
        return null;

      var itemType = Type.GetType( listName, false, false );
      if( itemType == null )
        return null;

      var descriptionProvider = TypeDescriptor.GetProvider( itemType );
      if( descriptionProvider == null )
        return null;

      var descriptor = descriptionProvider.GetTypeDescriptor( itemType );
      if( descriptor == null )
        return null;

      properties = descriptor.GetProperties();
      if( ( properties != null ) && ( properties.Count > 0 ) )
        return properties[ this.RelationName ];

      return null;
    }

    private PropertyDescriptor GetPropertyDescriptorFromFirstItem( object firstItem )
    {
      if( string.IsNullOrEmpty( this.RelationName ) || ( firstItem == null ) )
        return null;

      var descriptor = ItemsSourceHelper.GetCustomTypeDescriptorFromItem( firstItem, firstItem.GetType() );
      if( descriptor == null )
        return null;

      var properties = descriptor.GetProperties();
      if( ( properties != null ) && ( properties.Count > 0 ) )
        return properties[ this.RelationName ];

      return null;
    }

    protected internal override IEnumerable GetDetailsForParentItem( DataGridCollectionViewBase parentCollectionView, object parentItem )
    {
      if( this.PropertyDescriptor == null )
        throw new InvalidOperationException( "An attempt was made to obtain details of a PropertyDetailDescription object whose PropertyDescriptor property has not been set." );

      this.Seal();

      var value = this.PropertyDescriptor.GetValue( parentItem );
      if( value == null )
        return null;

      var enumeration = value as IEnumerable;
      if( enumeration == null )
      {
        var listSource = value as IListSource;
        if( listSource != null )
        {
          enumeration = listSource.GetList();
        }
      }

      return enumeration;
    }
  }
}
