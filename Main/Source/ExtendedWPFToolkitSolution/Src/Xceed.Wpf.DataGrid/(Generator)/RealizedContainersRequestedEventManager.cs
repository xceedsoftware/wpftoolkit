﻿/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal class RealizedContainersRequestedEventManager: WeakEventManager
  {
    private RealizedContainersRequestedEventManager()
    {
    }

    public static void AddListener( ColumnCollection source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( ColumnCollection source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      ColumnCollection columns = ( ColumnCollection )source;
      columns.RealizedContainersRequested += this.OnRealizedContainersRequested;
    }

    protected override void StopListening( object source )
    {
      ColumnCollection columns = ( ColumnCollection )source;
      columns.RealizedContainersRequested -= this.OnRealizedContainersRequested;
    }

    private static RealizedContainersRequestedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( RealizedContainersRequestedEventManager );
        RealizedContainersRequestedEventManager currentManager = ( RealizedContainersRequestedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new RealizedContainersRequestedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnRealizedContainersRequested( object sender, RealizedContainersRequestedEventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
