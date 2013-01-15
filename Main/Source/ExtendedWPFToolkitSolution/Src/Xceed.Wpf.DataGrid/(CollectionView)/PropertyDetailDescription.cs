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
using System.ComponentModel;
using System.Collections;

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

    protected internal override void Initialize( DataGridCollectionViewBase parentCollectionView )
    {
      base.Initialize( parentCollectionView );

      if( this.PropertyDescriptor != null )
        return;

      string relationName = this.RelationName;

      if( String.IsNullOrEmpty( relationName ) == true )
        throw new InvalidOperationException( "An attempt was made to initialize a PropertyDetailDescription whose Name property has not been set." );

      foreach( DataGridDetailDescription detailDescription in parentCollectionView.DetailDescriptions.DefaultDetailDescriptions )
      {
        PropertyDetailDescription propertyDetailDescription = detailDescription as PropertyDetailDescription;
        if( propertyDetailDescription != null )
        {
          if( propertyDetailDescription.RelationName == relationName )
          {
            this.PropertyDescriptor = propertyDetailDescription.PropertyDescriptor;
            return;
          }
        }
      }

      // The DetailDescription for RelationName was not found in DefaultDetailDescription
      // and may not have the PropertyDetailDescriptionAttribute
      if( this.PropertyDescriptor == null )
      {
        PropertyDescriptor relationDescriptor = null;

        IEnumerable enumeration = parentCollectionView as IEnumerable;

        if( enumeration != null )
        {
          // Try to get it from the first item in the DataGridCollectionView
          object firstItem = ItemsSourceHelper.GetFirstItemByEnumerable( enumeration );

          if( firstItem != null )
          {
            relationDescriptor = this.GetPropertyDescriptorFromFirstItem( firstItem );

            if( relationDescriptor != null )
            {
              this.PropertyDescriptor = relationDescriptor;
              return;
            }
          }
        }

        // If the list is empty, check if the SourceCollection is ITypedList
        ITypedList iTypedList = parentCollectionView.SourceCollection as ITypedList;

        if( iTypedList != null )
        {
          relationDescriptor = this.GetPropertyDescriptorFromITypedList( iTypedList );

          if( relationDescriptor != null )
          {
            this.PropertyDescriptor = relationDescriptor;
            return;
          }
        }
      }

      throw new InvalidOperationException( "An attempt was made to initialize a PropertyDetailDescription whose data source does not contain a property that corresponds to the specified relation name." );
    }

    private PropertyDescriptor GetPropertyDescriptorFromITypedList( ITypedList iTypedList )
    {
      if( string.IsNullOrEmpty( this.RelationName ) == true )
        return null;

      Type objectType = null;
      ICustomTypeDescriptor customTypeDescriptor = null;
      PropertyDescriptorCollection properties = null;
      PropertyDescriptor relationDescriptor = null;
      TypeDescriptionProvider typeDescriptionProvider = null;

      if( iTypedList != null )
      {
        properties = iTypedList.GetItemProperties( null );

        if( ( properties != null ) && ( properties.Count > 0 ) )
        {
          relationDescriptor = properties[ this.RelationName ];
        }
        else
        {
          string listName = iTypedList.GetListName( null );

          if( string.IsNullOrEmpty( listName ) == false )
          {
            objectType = Type.GetType( listName, false, false );

            if( objectType != null )
            {
              typeDescriptionProvider = TypeDescriptor.GetProvider( objectType );
              if( typeDescriptionProvider != null )
              {
                customTypeDescriptor = typeDescriptionProvider.GetTypeDescriptor( objectType );

                if( customTypeDescriptor != null )
                {
                  properties = customTypeDescriptor.GetProperties();

                  if( ( properties != null ) && ( properties.Count > 0 ) )
                    relationDescriptor = properties[ this.RelationName ];
                }
              }
            }
          }
        }
      }

      return relationDescriptor;
    }

    private PropertyDescriptor GetPropertyDescriptorFromFirstItem( object firstItem )
    {
      if( string.IsNullOrEmpty( this.RelationName ) == true )
        return null;

      Type objectType = null;
      ICustomTypeDescriptor customTypeDescriptor = null;
      PropertyDescriptorCollection properties = null;
      PropertyDescriptor relationDescriptor = null;
      TypeDescriptionProvider typeDescriptionProvider = null;

      objectType = firstItem.GetType();

      if( objectType != null )
      {
        typeDescriptionProvider = TypeDescriptor.GetProvider( objectType );
        if( typeDescriptionProvider != null )
        {
          customTypeDescriptor = typeDescriptionProvider.GetTypeDescriptor( objectType, firstItem );

          if( customTypeDescriptor != null )
          {
            properties = customTypeDescriptor.GetProperties();

            if( ( properties != null ) && ( properties.Count > 0 ) )
              relationDescriptor = properties[ this.RelationName ];
          }
        }
      }

      return relationDescriptor;
    }

    protected internal override IEnumerable GetDetailsForParentItem( DataGridCollectionViewBase parentCollectionView, object parentItem )
    {
      if( this.PropertyDescriptor == null )
        throw new InvalidOperationException( "An attempt was made to obtain details of a PropertyDetailDescription object whose PropertyDescriptor property has not been set." );

      this.Seal();

      object value = this.PropertyDescriptor.GetValue( parentItem );

      if( value == null )
        return null;

      IEnumerable enumeration = value as IEnumerable;

      if( enumeration == null )
      {
        IListSource listSource = value as IListSource;
        if( listSource != null )
        {
          enumeration = listSource.GetList();
        }
      }

      return enumeration;
    }

    private PropertyDescriptor m_propertyDescriptor;
  }
}
