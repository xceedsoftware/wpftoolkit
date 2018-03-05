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
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid
{
  internal class ScrollChangedWeakEventManager : WeakEventManager
  {
    internal static void AddListener( ScrollViewer source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    internal static void RemoveListener( ScrollViewer source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      ScrollViewer scrollViewer = source as ScrollViewer;

      if( scrollViewer != null )
        scrollViewer.ScrollChanged += this.ScrollViewer_ScrollChanged;
    }

    protected override void StopListening( object source )
    {
      ScrollViewer scrollViewer = source as ScrollViewer;

      if( scrollViewer != null )
        scrollViewer.ScrollChanged -= this.ScrollViewer_ScrollChanged;
    }

    private static ScrollChangedWeakEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( ScrollChangedWeakEventManager );
        ScrollChangedWeakEventManager currentManager =
          ( ScrollChangedWeakEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new ScrollChangedWeakEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void ScrollViewer_ScrollChanged( object sender, ScrollChangedEventArgs e )
    {
      this.DeliverEvent( sender, e );
    }
  }
}
