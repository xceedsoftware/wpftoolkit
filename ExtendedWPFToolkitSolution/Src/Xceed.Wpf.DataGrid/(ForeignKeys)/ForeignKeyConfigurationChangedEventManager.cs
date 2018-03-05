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
  internal class ForeignKeyConfigurationChangedEventManager : WeakEventManager
  {
    private ForeignKeyConfigurationChangedEventManager()
    {
    }

    public static void AddListener( Column source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( Column source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      Column column = ( Column )source;
      column.ForeignKeyConfigurationChanged += new EventHandler( this.OnForeignKeyConfigurationChangedChanged );
    }

    protected override void StopListening( object source )
    {
      Column column = ( Column )source;
      column.ForeignKeyConfigurationChanged -= new EventHandler( this.OnForeignKeyConfigurationChangedChanged );
    }

    private static ForeignKeyConfigurationChangedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( ForeignKeyConfigurationChangedEventManager );
        ForeignKeyConfigurationChangedEventManager currentManager = ( ForeignKeyConfigurationChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new ForeignKeyConfigurationChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnForeignKeyConfigurationChangedChanged( object sender, EventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
