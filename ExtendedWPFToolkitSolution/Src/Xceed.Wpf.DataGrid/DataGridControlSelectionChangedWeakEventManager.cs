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

namespace Xceed.Wpf.DataGrid
{
  internal class DataGridControlSelectionChangedWeakEventManager : WeakEventManager
  {
    private DataGridControlSelectionChangedWeakEventManager()
    {
    }

    public static void AddListener( DataGridControl source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( DataGridControl source, IWeakEventListener listener )
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

    private static DataGridControlSelectionChangedWeakEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( DataGridControlSelectionChangedWeakEventManager );
        DataGridControlSelectionChangedWeakEventManager currentManager =
          ( DataGridControlSelectionChangedWeakEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new DataGridControlSelectionChangedWeakEventManager();
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
