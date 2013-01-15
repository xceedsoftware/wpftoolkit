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
using System.ComponentModel;

namespace Xceed.Utils.Collections
{
  internal class ListChangedEventManager : WeakEventManager
  {
    private ListChangedEventManager()
    {
    }

    public static void AddListener( IBindingList source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( IBindingList source, IWeakEventListener listener )
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
