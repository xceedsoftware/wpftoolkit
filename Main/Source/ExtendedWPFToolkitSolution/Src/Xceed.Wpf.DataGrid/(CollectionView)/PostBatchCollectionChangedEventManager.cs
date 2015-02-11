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
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class PostBatchCollectionChangedEventManager : WeakEventManager
  {
    #region Constructor

    private PostBatchCollectionChangedEventManager()
    {
    }

    #endregion

    #region CurrentManager Static Property

    private static PostBatchCollectionChangedEventManager CurrentManager
    {
      get
      {
        var managerType = typeof( PostBatchCollectionChangedEventManager );
        var currentManager = ( PostBatchCollectionChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new PostBatchCollectionChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    #endregion

    public static void AddListener( DataGridCollectionViewBase source, IWeakEventListener listener )
    {
      PostBatchCollectionChangedEventManager.CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( DataGridCollectionViewBase source, IWeakEventListener listener )
    {
      PostBatchCollectionChangedEventManager.CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      ( ( DataGridCollectionViewBase )source ).PostBatchCollectionChanged += new EventHandler( this.OnEventRaised );
    }

    protected override void StopListening( object source )
    {
      ( ( DataGridCollectionViewBase )source ).PostBatchCollectionChanged -= new EventHandler( this.OnEventRaised );
    }

    private void OnEventRaised( object sender, EventArgs e )
    {
      this.DeliverEvent( sender, e );
    }
  }
}
