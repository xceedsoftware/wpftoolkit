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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal class DetailConfigurationCollection : ObservableCollection<DetailConfiguration>, IWeakEventListener
  {
    internal DetailConfigurationCollection( DataGridControl dataGridControl, DetailConfiguration parentDetailConfiguration )
      : base()
    {
      this.DataGridControl = dataGridControl;
      this.ParentDetailConfiguration = parentDetailConfiguration;
    }

    #region DataGridControl Property

    internal DataGridControl DataGridControl
    {
      get
      {
        return m_dataGridControl;
      }
      set
      {
        if( m_dataGridControl == value )
          return;

        m_dataGridControl = value;

        this.OnPropertyChanged( new PropertyChangedEventArgs( "DataGridControl" ) );
      }
    }

    private DataGridControl m_dataGridControl;

    #endregion

    #region ParentDetailConfiguration Property

    internal DetailConfiguration ParentDetailConfiguration
    {
      get;
      set;
    }

    #endregion

    public DetailConfiguration this[ string relationName ]
    {
      get
      {
        foreach( DetailConfiguration config in this )
        {
          if( string.Equals( config.RelationName, relationName ) == true )
          {
            return config;
          }
        }

        return null;
      }
    }

    protected override void InsertItem( int index, DetailConfiguration item )
    {
      if( item != null )
      {
        item.AttachToContainingCollection( this );

        DetailVisibilityChangedEventManager.AddListener( item, this );
      }

      base.InsertItem( index, item );
    }

    protected override void RemoveItem( int index )
    {
      DetailConfiguration detailConfig = this[ index ];

      detailConfig.DetachFromContainingCollection();

      DetailVisibilityChangedEventManager.RemoveListener( detailConfig, this );

      base.RemoveItem( index );
    }

    protected override void ClearItems()
    {
      foreach( DetailConfiguration detailConfig in this )
      {
        detailConfig.DetachFromContainingCollection();

        DetailVisibilityChangedEventManager.RemoveListener( detailConfig, this );
      }

      base.ClearItems();
    }

    protected override void SetItem( int index, DetailConfiguration item )
    {
      DetailConfiguration detailConfig = this[ index ];

      if( ( detailConfig != null ) && ( detailConfig != item ) )
      {
        detailConfig.DetachFromContainingCollection();
      }

      if( ( item != null ) && ( item != detailConfig ) )
      {
        item.AttachToContainingCollection( this );
      }

      base.SetItem( index, item );
    }

    #region IWeakEventListener Members

    internal event EventHandler DetailVisibilityChanged;

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( DetailVisibilityChangedEventManager ) )
      {
        if( this.DetailVisibilityChanged != null )
        {
          this.DetailVisibilityChanged( this, EventArgs.Empty );
        }

        return true;
      }
      return false;
    }

    #endregion
  }
}
