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
  internal class DetailsChangedEventManager : WeakEventManager
  {
    internal static void AddListener( object source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    internal static void RemoveListener( object source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      DataGridControl dataGridControl = source as DataGridControl;
      if( dataGridControl != null )
      {
        dataGridControl.DetailsChanged += this.OnDetailsChanged;
        return;
      }
      else
      {
        CustomItemContainerGenerator generator = source as CustomItemContainerGenerator;
        if( generator != null )
        {
          generator.DetailsChanged += this.OnDetailsChanged;
          return;
        }
      }

      throw new InvalidOperationException( "An attempt was made to use a source other than a DataGridControl or CustomItemContainerGenerator." );
    }

    protected override void StopListening( object source )
    {
      DataGridControl dataGridControl = source as DataGridControl;
      if( dataGridControl != null )
      {
        dataGridControl.DetailsChanged -= this.OnDetailsChanged;
        return;
      }
      else
      {
        CustomItemContainerGenerator generator = source as CustomItemContainerGenerator;
        if( generator != null )
        {
          generator.DetailsChanged -= this.OnDetailsChanged;
          return;
        }
      }

      throw new InvalidOperationException( "An attempt was made to use a source other than a DataGridControl or CustomItemContainerGenerator." );
    }

    private static DetailsChangedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( DetailsChangedEventManager );
        DetailsChangedEventManager currentManager = ( DetailsChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new DetailsChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnDetailsChanged( object sender, EventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
