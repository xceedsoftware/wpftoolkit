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
  internal class DataGridControlSelectionChangedEventManager : WeakEventManager
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
      DataGridControl dataGridControl = source as DataGridControl;

      if( dataGridControl != null )
        dataGridControl.SelectionChanged += this.DataGridControl_SelectionChanged;
    }

    protected override void StopListening( object source )
    {
      DataGridControl dataGridControl = source as DataGridControl;

      if( dataGridControl != null )
        dataGridControl.SelectionChanged -= this.DataGridControl_SelectionChanged;
    }

    private static DataGridControlSelectionChangedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( DataGridControlSelectionChangedEventManager );
        DataGridControlSelectionChangedEventManager currentManager =
          ( DataGridControlSelectionChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new DataGridControlSelectionChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void DataGridControl_SelectionChanged( object sender, DataGridSelectionChangedEventArgs e )
    {
      this.DeliverEvent( sender, e );
    }
  }
}
