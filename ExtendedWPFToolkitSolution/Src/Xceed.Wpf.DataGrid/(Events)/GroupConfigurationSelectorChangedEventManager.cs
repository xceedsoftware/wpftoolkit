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
  internal class GroupConfigurationSelectorChangedEventManager : WeakEventManager
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
      DataGridContext context = source as DataGridContext;

      if (context != null)
      {
        context.GroupConfigurationSelectorChanged += new EventHandler(this.OnGroupConfigurationSelectorChanged);
        return;
      }

      DataGridControl dataGridControl = source as DataGridControl;
      if (dataGridControl != null)
      {
        dataGridControl.GroupConfigurationSelectorChanged += new EventHandler(this.OnGroupConfigurationSelectorChanged);
        return;
      }

      DetailConfiguration detailConfig = source as DetailConfiguration;
      if (detailConfig != null)
      {
        detailConfig.GroupConfigurationSelectorChanged += new EventHandler(this.OnGroupConfigurationSelectorChanged);
        return;
      }

      throw new InvalidOperationException("An attempt was made to register an item other than a DataGridContext, DataGridControl, or DetailConfiguration to the GroupConfigurationSelectorChangedEventManager.");
    }

    protected override void StopListening(object source)
    {
      DataGridContext context = source as DataGridContext;

      if (context != null)
      {
        context.GroupConfigurationSelectorChanged -= new EventHandler(this.OnGroupConfigurationSelectorChanged);
        return;
      }

      DataGridControl dataGridControl = source as DataGridControl;
      if (dataGridControl != null)
      {
        dataGridControl.GroupConfigurationSelectorChanged -= new EventHandler(this.OnGroupConfigurationSelectorChanged);
        return;
      }

      DetailConfiguration detailConfig = source as DetailConfiguration;
      if (detailConfig != null)
      {
        detailConfig.GroupConfigurationSelectorChanged -= new EventHandler(this.OnGroupConfigurationSelectorChanged);
        return;
      }

      throw new InvalidOperationException( "An attempt was made to unregister an item other than a DataGridContext, DataGridControl, or DetailConfiguration from the GroupConfigurationSelectorChangedEventManager." );
    }

    private static GroupConfigurationSelectorChangedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( GroupConfigurationSelectorChangedEventManager );
        GroupConfigurationSelectorChangedEventManager currentManager = ( GroupConfigurationSelectorChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new GroupConfigurationSelectorChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnGroupConfigurationSelectorChanged( object sender, EventArgs args )
    {
      this.DeliverEvent( sender, args );
    }
  }
}
