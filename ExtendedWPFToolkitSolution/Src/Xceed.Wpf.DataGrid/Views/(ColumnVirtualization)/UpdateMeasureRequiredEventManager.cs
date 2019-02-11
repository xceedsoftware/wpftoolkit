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
  internal class UpdateMeasureRequiredEventManager : WeakEventManager
  {
    private UpdateMeasureRequiredEventManager()
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

      columnVirtualizationManager.UpdateMeasureRequired += this.OnUpdateMeasureRequired;
    }

    protected override void StopListening( object source )
    {
      var columnVirtualizationManager = source as TableViewColumnVirtualizationManagerBase;
      if( columnVirtualizationManager == null )
        throw new InvalidOperationException( "An attempt was made to use a source other than a ColumnVirtualizationManager." );

      columnVirtualizationManager.UpdateMeasureRequired -= this.OnUpdateMeasureRequired;
    }

    private static UpdateMeasureRequiredEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( UpdateMeasureRequiredEventManager );
        UpdateMeasureRequiredEventManager currentManager = ( UpdateMeasureRequiredEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new UpdateMeasureRequiredEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnUpdateMeasureRequired( object sender, EventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
