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
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class SelectionChangedInternalEventManager : WeakEventManager
  {
    internal static void AddListener( DataGridContext source, IWeakEventListener listener )
    {
      SelectionChangedInternalEventManager.CurrentManager.ProtectedAddListener( source, listener );
    }

    internal static void RemoveListener( DataGridContext source, IWeakEventListener listener )
    {
      SelectionChangedInternalEventManager.CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      ( ( DataGridContext )source ).SelectionChangedInternal += new EventHandler<SelectionChangedInternalEventArgs>( this.OnEventRaised );
    }

    protected override void StopListening( object source )
    {
      ( ( DataGridContext )source ).SelectionChangedInternal -= new EventHandler<SelectionChangedInternalEventArgs>( this.OnEventRaised );
    }

    private static SelectionChangedInternalEventManager CurrentManager
    {
      get
      {
        var managerType = typeof( SelectionChangedInternalEventManager );
        var currentManager = ( SelectionChangedInternalEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new SelectionChangedInternalEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnEventRaised( object sender, SelectionChangedInternalEventArgs e )
    {
      this.DeliverEvent( sender, e );
    }
  }
}
