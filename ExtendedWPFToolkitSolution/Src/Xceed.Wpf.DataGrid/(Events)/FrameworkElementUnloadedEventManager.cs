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
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class FrameworkElementUnloadedEventManager : WeakEventManager
  {
    internal static void AddListener( FrameworkElement source, IWeakEventListener listener )
    {
      FrameworkElementUnloadedEventManager.CurrentManager.ProtectedAddListener( source, listener );
    }

    internal static void RemoveListener( FrameworkElement source, IWeakEventListener listener )
    {
      FrameworkElementUnloadedEventManager.CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      var fe = source as FrameworkElement;
      if( fe == null )
        return;

      fe.Unloaded += new RoutedEventHandler( this.OnEventRaised );
    }

    protected override void StopListening( object source )
    {
      var fe = source as FrameworkElement;
      if( fe == null )
        return;

      var dispatcher = fe.Dispatcher;
      if( dispatcher.CheckAccess() )
      {
        this.StopListening( fe );
      }
      else
      {
        dispatcher.BeginInvoke( new Action<FrameworkElement>( this.StopListening ), DispatcherPriority.Send, fe );
      }
    }

    private static FrameworkElementUnloadedEventManager CurrentManager
    {
      get
      {
        var managerType = typeof( FrameworkElementUnloadedEventManager );
        var currentManager = ( FrameworkElementUnloadedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new FrameworkElementUnloadedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void StopListening( FrameworkElement fe )
    {
      fe.Unloaded -= new RoutedEventHandler( this.OnEventRaised );
    }

    private void OnEventRaised( object sender, RoutedEventArgs e )
    {
      this.DeliverEvent( sender, e );
    }
  }
}
