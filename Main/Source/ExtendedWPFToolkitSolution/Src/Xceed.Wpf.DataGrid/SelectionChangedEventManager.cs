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
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid
{
  internal class SelectionChangedEventManager : WeakEventManager
  {
    private SelectionChangedEventManager()
    {
    }

    public static void AddListener( Selector source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( Selector source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      Selector selector = ( Selector )source;

      if( selector != null )
        selector.SelectionChanged += new SelectionChangedEventHandler( this.Selector_SelectionChanged );
    }

    protected override void StopListening( object source )
    {
      Selector selector = ( Selector )source;

      if( selector != null )
        selector.SelectionChanged -= new SelectionChangedEventHandler( this.Selector_SelectionChanged );
    }

    private static SelectionChangedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( SelectionChangedEventManager );
        SelectionChangedEventManager currentManager = ( SelectionChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new SelectionChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void Selector_SelectionChanged( object sender, SelectionChangedEventArgs e )
    {
      this.DeliverEvent( sender, e );
    }
  }
}
