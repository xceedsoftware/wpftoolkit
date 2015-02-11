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
  internal class VisibilityChangingEventManager : WeakEventManager
  {
    private VisibilityChangingEventManager()
    {
    }

    public static void AddListener( ColumnCollection source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( ColumnCollection source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      var columnCollection = ( ColumnCollection )source;
      columnCollection.ColumnVisibilityChanging += new EventHandler( this.OnColumnVisibilityChanging );
    }

    protected override void StopListening( object source )
    {
      var columnCollection = ( ColumnCollection )source;
      columnCollection.ColumnVisibilityChanging -= new EventHandler( this.OnColumnVisibilityChanging );
    }

    private static VisibilityChangingEventManager CurrentManager
    {
      get
      {
        var managerType = typeof( VisibilityChangingEventManager );
        var currentManager = ( VisibilityChangingEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new VisibilityChangingEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnColumnVisibilityChanging( object sender, EventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
