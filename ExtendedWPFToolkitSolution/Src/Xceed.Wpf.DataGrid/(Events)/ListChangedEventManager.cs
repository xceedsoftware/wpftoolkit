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
using System.ComponentModel;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal class ListChangedEventManager : WeakEventManager
  {
    internal static void AddListener( IBindingList source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    internal static void RemoveListener( IBindingList source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      IBindingList list = ( IBindingList )source;
      list.ListChanged += new ListChangedEventHandler( this.OnListChanged );
    }

    protected override void StopListening( object source )
    {
      IBindingList list = ( IBindingList )source;
      list.ListChanged -= new ListChangedEventHandler( this.OnListChanged );
    }

    private static ListChangedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( ListChangedEventManager );
        ListChangedEventManager currentManager = ( ListChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new ListChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnListChanged( object sender, ListChangedEventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
