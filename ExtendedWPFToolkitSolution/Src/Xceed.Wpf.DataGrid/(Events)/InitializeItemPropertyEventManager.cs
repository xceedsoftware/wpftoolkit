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
  internal sealed class InitializeItemPropertyEventManager : WeakEventManager
  {
    internal static void AddListener( DataGridItemPropertyCollection source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    internal static void RemoveListener( DataGridItemPropertyCollection source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      var target = ( DataGridItemPropertyCollection )source;
      target.InitializeItemProperty += new EventHandler<InitializeItemPropertyEventArgs>( this.OnEventRaised );
    }

    protected override void StopListening( object source )
    {
      var target = ( DataGridItemPropertyCollection )source;
      target.InitializeItemProperty -= new EventHandler<InitializeItemPropertyEventArgs>( this.OnEventRaised );
    }

    private static InitializeItemPropertyEventManager CurrentManager
    {
      get
      {
        var managerType = typeof( InitializeItemPropertyEventManager );
        var currentManager = ( InitializeItemPropertyEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new InitializeItemPropertyEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnEventRaised( object sender, InitializeItemPropertyEventArgs e )
    {
      this.DeliverEvent( sender, e );
    }
  }
}
