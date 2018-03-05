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
  internal class ThemeChangedEventManager : WeakEventManager
  {
    internal static void AddListener( DataGridControl source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    internal static void RemoveListener( DataGridControl source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      DataGridControl dataGridControl = ( DataGridControl )source;
      dataGridControl.ThemeChanged += new EventHandler( this.OnThemeChanged );
    }

    protected override void StopListening( object source )
    {
      DataGridControl dataGridControl = ( DataGridControl )source;
      dataGridControl.ThemeChanged -= new EventHandler( this.OnThemeChanged );
    }

    private static ThemeChangedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( ThemeChangedEventManager );
        ThemeChangedEventManager currentManager = ( ThemeChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new ThemeChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnThemeChanged( object sender, EventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
