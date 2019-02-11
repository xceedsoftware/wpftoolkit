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

namespace Xceed.Wpf.DataGrid.Views
{
  internal class VirtualizingCellCollectionUpdateRequiredEventManager : WeakEventManager
  {
    private VirtualizingCellCollectionUpdateRequiredEventManager()
    {
    }

    public static void AddListener( object source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( object source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      var columnVirtualizationManager = source as TableViewColumnVirtualizationManagerBase;
      if( columnVirtualizationManager == null )
        throw new InvalidOperationException( "An attempt was made to use a source other than a ColumnVirtualizationManager." );

      columnVirtualizationManager.VirtualizingCellCollectionUpdateRequired += this.OnVirtualizingCellCollectionUpdateRequired;
    }

    protected override void StopListening( object source )
    {
      var columnVirtualizationManager = source as TableViewColumnVirtualizationManagerBase;
      if( columnVirtualizationManager == null )
        throw new InvalidOperationException( "An attempt was made to use a source other than a ColumnVirtualizationManager." );

      columnVirtualizationManager.VirtualizingCellCollectionUpdateRequired -= this.OnVirtualizingCellCollectionUpdateRequired;
    }

    private static VirtualizingCellCollectionUpdateRequiredEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( VirtualizingCellCollectionUpdateRequiredEventManager );
        VirtualizingCellCollectionUpdateRequiredEventManager currentManager = ( VirtualizingCellCollectionUpdateRequiredEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new VirtualizingCellCollectionUpdateRequiredEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnVirtualizingCellCollectionUpdateRequired( object sender, EventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
