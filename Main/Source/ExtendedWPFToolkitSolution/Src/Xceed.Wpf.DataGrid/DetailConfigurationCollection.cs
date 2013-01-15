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
using System.Windows;
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid
{
  internal class DetailConfigurationCollection : ObservableCollection<DetailConfiguration>, IWeakEventListener
  {
    #region CONSTRUCTORS

    internal DetailConfigurationCollection( DataGridControl dataGridControl, DetailConfiguration parentDetailConfiguration )
      : base()
    {
      this.DataGridControl = dataGridControl;
      this.ParentDetailConfiguration = parentDetailConfiguration;
    }

    #endregion CONSTRUCTORS

    public DetailConfiguration this[ string relationName]
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

        foreach( DetailConfiguration detail in this.Items )
        {
          if( detail.Columns != null )
          {
            detail.Columns.DataGridControl = value;
          }

          if( detail.DetailConfigurations != null )
          {
            detail.DetailConfigurations.DataGridControl = value;
          }
        }
      }
    }

    private DataGridControl m_dataGridControl;

    #endregion DataGridControl Property

    #region ParentDetailConfiguration Property

    internal DetailConfiguration ParentDetailConfiguration
    {
      get;
      set;
    }

    #endregion ParentDetailConfiguration Property
  }
}
