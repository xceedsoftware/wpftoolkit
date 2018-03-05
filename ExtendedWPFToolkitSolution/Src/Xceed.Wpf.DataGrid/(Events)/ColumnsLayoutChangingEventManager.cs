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
  internal sealed class ColumnsLayoutChangingEventManager : WeakEventManager
  {
    internal static void AddListener( ColumnHierarchyManager source, IWeakEventListener listener )
    {
      ColumnsLayoutChangingEventManager.CurrentManager.ProtectedAddListener( source, listener );
    }

    internal static void RemoveListener( ColumnHierarchyManager source, IWeakEventListener listener )
    {
      ColumnsLayoutChangingEventManager.CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      ( ( ColumnHierarchyManager )source ).LayoutChanging += new EventHandler( this.OnEventRaised );
    }

    protected override void StopListening( object source )
    {
      ( ( ColumnHierarchyManager )source ).LayoutChanging -= new EventHandler( this.OnEventRaised );
    }

    private static ColumnsLayoutChangingEventManager CurrentManager
    {
      get
      {
        var managerType = typeof( ColumnsLayoutChangingEventManager );
        var currentManager = ( ColumnsLayoutChangingEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new ColumnsLayoutChangingEventManager();
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
