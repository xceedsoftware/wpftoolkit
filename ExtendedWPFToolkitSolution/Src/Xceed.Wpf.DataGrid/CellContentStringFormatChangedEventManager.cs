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
  internal class CellContentStringFormatChangedEventManager : WeakEventManager
  {
    private CellContentStringFormatChangedEventManager()
    {
    }

    public static void AddListener( ColumnBase source, IWeakEventListener listener )
    {
      CellContentStringFormatChangedEventManager.CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( ColumnBase source, IWeakEventListener listener )
    {
      CellContentStringFormatChangedEventManager.CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      var column = ( ColumnBase )source;

      column.CellContentStringFormatChanged += new EventHandler( this.OnCellContentStringFormatChanged );
    }

    protected override void StopListening( object source )
    {
      var column = ( ColumnBase )source;

      column.CellContentStringFormatChanged -= new EventHandler( this.OnCellContentStringFormatChanged );
    }

    private static CellContentStringFormatChangedEventManager CurrentManager
    {
      get
      {
        var managerType = typeof( CellContentStringFormatChangedEventManager );
        var currentManager = ( CellContentStringFormatChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new CellContentStringFormatChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnCellContentStringFormatChanged( object sender, EventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
