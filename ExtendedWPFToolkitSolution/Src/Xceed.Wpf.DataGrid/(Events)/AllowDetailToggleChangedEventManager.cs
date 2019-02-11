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
  internal class AllowDetailToggleChangedEventManager : WeakEventManager
  {
    internal static void AddListener( object source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    internal static void RemoveListener( object source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      DataGridControl dataGridControl = source as DataGridControl;
      if( dataGridControl != null )
      {
        dataGridControl.AllowDetailToggleChanged += new EventHandler( this.OnAllowDetailToggleChanged );
        return;
      }

      DetailConfiguration detailConfiguration = source as DetailConfiguration;
      if( detailConfiguration != null )
      {
        detailConfiguration.AllowDetailToggleChanged += new EventHandler( this.OnAllowDetailToggleChanged );
        return;
      }

      throw new ArgumentException( "The specified source must be a DataGridControl or a DetailConfiguration.", "source" );
    }

    protected override void StopListening( object source )
    {
      DataGridControl dataGridControl = source as DataGridControl;
      if( dataGridControl != null )
      {
        dataGridControl.AllowDetailToggleChanged -= new EventHandler( this.OnAllowDetailToggleChanged );
        return;
      }

      DetailConfiguration detailConfiguration = source as DetailConfiguration;
      if( detailConfiguration != null )
      {
        detailConfiguration.AllowDetailToggleChanged -= new EventHandler( this.OnAllowDetailToggleChanged );
        return;
      }

      throw new ArgumentException( "The specified source must be a DataGridControl or a DetailConfiguration.", "source" );
    }

    private static AllowDetailToggleChangedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( AllowDetailToggleChangedEventManager );
        AllowDetailToggleChangedEventManager currentManager = ( AllowDetailToggleChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new AllowDetailToggleChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnAllowDetailToggleChanged( object sender, EventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
