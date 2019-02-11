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
  internal sealed class CurrentItemChangedEventManager : WeakEventManager
  {
    internal static void AddListener( DataGridContext source, IWeakEventListener listener )
    {
      CurrentItemChangedEventManager.CurrentManager.ProtectedAddListener( source, listener );
    }

    internal static void RemoveListener( DataGridContext source, IWeakEventListener listener )
    {
      CurrentItemChangedEventManager.CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      ( ( DataGridContext )source ).CurrentItemChanged += new EventHandler( this.OnEventRaised );
    }

    protected override void StopListening( object source )
    {
      ( ( DataGridContext )source ).CurrentItemChanged -= new EventHandler( this.OnEventRaised );
    }

    private static CurrentItemChangedEventManager CurrentManager
    {
      get
      {
        var managerType = typeof( CurrentItemChangedEventManager );
        var currentManager = ( CurrentItemChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new CurrentItemChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnEventRaised( object sender, EventArgs e )
    {
      this.DeliverEvent( sender, e );
    }
  }
}
