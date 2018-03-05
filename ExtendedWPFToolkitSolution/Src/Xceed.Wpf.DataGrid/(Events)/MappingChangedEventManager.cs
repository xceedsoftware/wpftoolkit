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
  internal sealed class MappingChangedEventManager : WeakEventManager
  {
    internal static void AddListener( DataGridItemPropertyMap source, IWeakEventListener listener )
    {
      MappingChangedEventManager.CurrentManager.ProtectedAddListener( source, listener );
    }

    internal static void RemoveListener( DataGridItemPropertyMap source, IWeakEventListener listener )
    {
      MappingChangedEventManager.CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      ( ( DataGridItemPropertyMap )source ).MappingChanged += new EventHandler( this.OnEventRaised );
    }

    protected override void StopListening( object source )
    {
      ( ( DataGridItemPropertyMap )source ).MappingChanged -= new EventHandler( this.OnEventRaised );
    }

    private static MappingChangedEventManager CurrentManager
    {
      get
      {
        var managerType = typeof( MappingChangedEventManager );
        var currentManager = ( MappingChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new MappingChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnEventRaised( object sender, EventArgs e )
    {
      this.DeliverEvent( sender, e );
    }
  }
}
