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
  internal class ForeignKeyConfigNotifyCollectionChangedEventManager : WeakEventManager
  {
    public static void AddListener( ForeignKeyConfiguration source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( ForeignKeyConfiguration source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      var configuration = ( ForeignKeyConfiguration )source;
      configuration.NotifiyCollectionChanged += new EventHandler( this.OnNotifiyCollectionChanged );
    }

    protected override void StopListening( object source )
    {
      var configuration = ( ForeignKeyConfiguration )source;
      configuration.NotifiyCollectionChanged -= new EventHandler( this.OnNotifiyCollectionChanged );
    }

    private static ForeignKeyConfigNotifyCollectionChangedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( ForeignKeyConfigNotifyCollectionChangedEventManager );
        var currentManager = ( ForeignKeyConfigNotifyCollectionChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new ForeignKeyConfigNotifyCollectionChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnNotifiyCollectionChanged( object sender, EventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
