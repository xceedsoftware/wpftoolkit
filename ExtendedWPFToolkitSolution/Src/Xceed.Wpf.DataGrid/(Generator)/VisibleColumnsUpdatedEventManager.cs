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
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal class VisibleColumnsUpdatedEventManager : WeakEventManager
  {
    private VisibleColumnsUpdatedEventManager()
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
      columns.VisibleColumnsUpdated += new EventHandler( this.OnVisibleColumnsUpdated );
    }

    protected override void StopListening( object source )
    {
      ColumnCollection columns = ( ColumnCollection )source;
      columns.VisibleColumnsUpdated -= new EventHandler( this.OnVisibleColumnsUpdated );
    }

    private static VisibleColumnsUpdatedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( VisibleColumnsUpdatedEventManager );
        VisibleColumnsUpdatedEventManager currentManager = ( VisibleColumnsUpdatedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new VisibleColumnsUpdatedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnVisibleColumnsUpdated( object sender, EventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
