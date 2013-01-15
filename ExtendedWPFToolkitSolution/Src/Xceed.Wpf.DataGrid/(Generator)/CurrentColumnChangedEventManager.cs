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

namespace Xceed.Wpf.DataGrid
{
  internal class CurrentColumnChangedEventManager: WeakEventManager
  {
    private CurrentColumnChangedEventManager()
    {
    }

    public static void AddListener( DetailConfiguration source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( DetailConfiguration source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      DetailConfiguration detailConfig = ( DetailConfiguration )source;
      detailConfig.CurrentColumnChanged += new EventHandler( this.OnCurrentColumnChanged );
    }

    protected override void StopListening( object source )
    {
      DetailConfiguration detailConfig = ( DetailConfiguration )source;
      detailConfig.CurrentColumnChanged -= new EventHandler( this.OnCurrentColumnChanged );
    }

    private static CurrentColumnChangedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( CurrentColumnChangedEventManager );
        CurrentColumnChangedEventManager currentManager = ( CurrentColumnChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new CurrentColumnChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnCurrentColumnChanged( object sender, EventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
