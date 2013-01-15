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
using System.Threading;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal class FrameworkElementUnloadedEventManager : WeakEventManager
  {
    private FrameworkElementUnloadedEventManager()
    {
    }

    public static void AddListener( FrameworkElement source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( FrameworkElement source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      FrameworkElement frameworkElement = source as FrameworkElement;

      if( frameworkElement != null )
        frameworkElement.Unloaded += this.FrameworkElement_Unloaded;
    }

    protected override void StopListening( object source )
    {
      FrameworkElement frameworkElement = source as FrameworkElement;

      if( frameworkElement != null )
      {
        if( frameworkElement.Dispatcher.Thread == Thread.CurrentThread )
        {
          frameworkElement.Unloaded -= this.FrameworkElement_Unloaded;
        }
        else
        {
          System.Windows.Threading.Dispatcher dispatcher = frameworkElement.Dispatcher;
          Debug.Assert( ( dispatcher != null ) && ( !dispatcher.HasShutdownStarted ) && ( !dispatcher.HasShutdownFinished ) );

          if( dispatcher != null )
          {
            dispatcher.Invoke( new Action( 
              delegate
              {
                frameworkElement.Unloaded -= this.FrameworkElement_Unloaded;
              } ), null );
          }
        }
      }
    }

    private static FrameworkElementUnloadedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( FrameworkElementUnloadedEventManager );

        FrameworkElementUnloadedEventManager currentManager =
          ( FrameworkElementUnloadedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new FrameworkElementUnloadedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void FrameworkElement_Unloaded( object sender, RoutedEventArgs e )
    {
      this.DeliverEvent( sender, e );
    }
  }
}
