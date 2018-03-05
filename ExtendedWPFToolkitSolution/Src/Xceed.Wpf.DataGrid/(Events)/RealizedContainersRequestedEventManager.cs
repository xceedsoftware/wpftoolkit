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
using System.Text;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal class RealizedContainersRequestedEventManager: WeakEventManager
  {
    private RealizedContainersRequestedEventManager()
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
      ColumnCollection columns = ( ColumnCollection )source;
      columns.RealizedContainersRequested += this.OnRealizedContainersRequested;
    }

    protected override void StopListening( object source )
    {
      ColumnCollection columns = ( ColumnCollection )source;
      columns.RealizedContainersRequested -= this.OnRealizedContainersRequested;
    }

    private static RealizedContainersRequestedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( RealizedContainersRequestedEventManager );
        RealizedContainersRequestedEventManager currentManager = ( RealizedContainersRequestedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new RealizedContainersRequestedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnRealizedContainersRequested( object sender, RealizedContainersRequestedEventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
