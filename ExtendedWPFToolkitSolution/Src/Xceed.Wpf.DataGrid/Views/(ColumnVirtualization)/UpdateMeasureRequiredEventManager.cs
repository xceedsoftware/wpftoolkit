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
using System.Linq;
using System.Text;
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
      TableViewColumnVirtualizationManager columnVirtualizationManager = source as TableViewColumnVirtualizationManager;
      if( columnVirtualizationManager != null )
      {
        columnVirtualizationManager.UpdateMeasureRequired += this.OnUpdateMeasureRequired;
        return;
      }

      throw new InvalidOperationException( "An attempt was made to use a source other than a ColumnVirtualizationManager." );
    }

    protected override void StopListening( object source )
    {
      TableViewColumnVirtualizationManager columnVirtualizationManager = source as TableViewColumnVirtualizationManager;
      if( columnVirtualizationManager != null )
      {
        columnVirtualizationManager.UpdateMeasureRequired -= this.OnUpdateMeasureRequired;
        return;
      }

      throw new InvalidOperationException( "An attempt was made to use a source other than a ColumnVirtualizationManager." );
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
