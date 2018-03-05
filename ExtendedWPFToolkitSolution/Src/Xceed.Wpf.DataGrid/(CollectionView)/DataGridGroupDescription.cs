/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridGroupDescription : GroupDescription
  {
    public DataGridGroupDescription()
    {
    }

    public DataGridGroupDescription( string propertyName )
    {
      m_propertyGroupDescription.PropertyName = propertyName;
    }

    public string PropertyName
    {
      get
      {
        return m_propertyGroupDescription.PropertyName;
      }

      set
      {
        m_propertyGroupDescription.PropertyName = value;
        this.OnPropertyChanged( new PropertyChangedEventArgs( "PropertyName" ) );
      }
    }

    public IComparer SortComparer
    {
      get
      {
        return m_sortComparer;
      }
      set
      {
        m_sortComparer = value;
        this.OnPropertyChanged( new PropertyChangedEventArgs( "SortComparer" ) );
      }
    }

    public GroupConfiguration GroupConfiguration
    {
      get
      {
        return m_groupConfiguration;
      }
      set
      {
        m_groupConfiguration = value;
      }
    }

    public override object GroupNameFromItem( object item, int level, CultureInfo culture )
    {
      return this.GetPropertyValue( item );
    }

    public override sealed bool NamesMatch( object groupName, object itemName )
    {
      // We sealed up the NamesMatch because we will use a HashTable to find existing key
      // for performance reason.
      //
      // We do not throw, because we want our GroupDescription to be usable in other CollectionView
      return base.NamesMatch( groupName, itemName );
    }

    protected object GetPropertyValue( object item )
    {
      if( m_contextProperty != null )
        return ItemsSourceHelper.GetValueFromItemProperty( m_contextProperty, item );

      return m_propertyGroupDescription.GroupNameFromItem( item, 0, CultureInfo.InvariantCulture );
    }

    internal void SetContext( DataGridCollectionView collectionView )
    {
      if( collectionView == null )
      {
        m_contextProperty = null;
      }
      else
      {
        var propertyName = m_propertyGroupDescription.PropertyName;

        if( string.IsNullOrEmpty( propertyName ) )
        {
          m_contextProperty = null;
        }
        else
        {
          m_contextProperty = ItemsSourceHelper.GetItemPropertyFromProperty( collectionView.ItemProperties, propertyName );
        }
      }
    }

    private IComparer m_sortComparer;
    private DataGridItemPropertyBase m_contextProperty;
    private PropertyGroupDescription m_propertyGroupDescription = new PropertyGroupDescription();
    private GroupConfiguration m_groupConfiguration; // = null
  }
}
