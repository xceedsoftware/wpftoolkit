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
using System.ComponentModel;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridSortDescriptionCollection : SortDescriptionCollection
  {
    #region CONSTRUCTORS

    public DataGridSortDescriptionCollection()
    {
    }

    #endregion CONSTRUCTORS

    #region IsResortDefered PROPERTY

    public bool IsResortDefered
    {
      get
      {
        return ( m_deferResortCount > 0 );
      }
    }

    #endregion IsResortDefered PROPERTY

    #region SyncContext PROPERTY

    public SortDescriptionsSyncContext SyncContext
    {
      get
      {
        return m_syncContext;
      }
    }

    #endregion SyncContext PROPERTY

    #region PUBLIC METHODS

    public IDisposable DeferResort()
    {
      return new DeferResortDisposable( this );
    }

    public void AddResortNotification( IDisposable notificationObject )
    {
      Debug.Assert( m_deferResortCount > 0 );

      lock( m_notificationList )
      {
        m_notificationList.Enqueue( notificationObject );
      }
    }

    #endregion PUBLIC METHODS

    #region PRIVATE FIELDS

    private SortDescriptionsSyncContext m_syncContext = new SortDescriptionsSyncContext();
    private int m_deferResortCount; // = 0
    private Queue<IDisposable> m_notificationList = new Queue<IDisposable>();

    #endregion PRIVATE FIELDS

    #region NESTED CLASSES

    private sealed class DeferResortDisposable : IDisposable
    {
      public DeferResortDisposable( DataGridSortDescriptionCollection parentCollection )
      {
        if( parentCollection == null )
          throw new ArgumentNullException( "parentCollection" );

        m_parentCollection = parentCollection;
        m_parentCollection.m_deferResortCount++;
      }

      #region IDisposable Members

      public void Dispose()
      {
        Debug.Assert( m_parentCollection.m_deferResortCount > 0 );

        lock( m_parentCollection.m_notificationList )
        {
          m_parentCollection.m_deferResortCount--;

          if( m_parentCollection.m_deferResortCount == 0 )
          {
            while( m_parentCollection.m_notificationList.Count > 0 )
            {
              IDisposable notificationObject = m_parentCollection.m_notificationList.Dequeue();

              notificationObject.Dispose();
              notificationObject = null;
            }
          }
        }

        m_parentCollection = null;
      }

      #endregion

      private DataGridSortDescriptionCollection m_parentCollection; // = null
    }

    #endregion NESTED CLASSES
  }
}
