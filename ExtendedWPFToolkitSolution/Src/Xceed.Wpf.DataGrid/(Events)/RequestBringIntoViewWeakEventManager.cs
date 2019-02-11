/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal class RequestBringIntoViewWeakEventManager : WeakEventManager
  {
    #region CurrentManager Static Property

    private static RequestBringIntoViewWeakEventManager CurrentManager
    {
      get
      {
        var managerType = typeof( RequestBringIntoViewWeakEventManager );
        var currentManager = ( RequestBringIntoViewWeakEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new RequestBringIntoViewWeakEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    #endregion

    internal static void AddListener( FrameworkElement source, IWeakEventListener listener )
    {
      RequestBringIntoViewWeakEventManager.CurrentManager.ProtectedAddListener( source, listener );
    }

    internal static void RemoveListener( FrameworkElement source, IWeakEventListener listener )
    {
      RequestBringIntoViewWeakEventManager.CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      var fe = ( FrameworkElement )source;

      fe.RequestBringIntoView += new RequestBringIntoViewEventHandler( this.OnEventRaised );
    }

    protected override void StopListening( object source )
    {
      var fe = ( FrameworkElement )source;

      fe.RequestBringIntoView -= new RequestBringIntoViewEventHandler( this.OnEventRaised );
    }

    private void OnEventRaised( object sender, RequestBringIntoViewEventArgs e )
    {
      this.DeliverEvent( sender, e );
    }
  }
}
