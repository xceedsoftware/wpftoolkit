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

namespace Xceed.Wpf.DataGrid
{
  internal class DetailVisibilityChangedEventManager : WeakEventManager
  {
    private DetailVisibilityChangedEventManager()
    {
    }

    public static void AddListener( object source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( object source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      DetailConfiguration detailConfig = source as DetailConfiguration;
      if( detailConfig != null )
      {
        detailConfig.VisibilityChanged += new EventHandler( this.OnVisibilityChanged );
        return;
      }

      DetailConfigurationCollection detailConfigCollection = source as DetailConfigurationCollection;
      if( detailConfigCollection != null )
      {
        detailConfigCollection.DetailVisibilityChanged += new EventHandler( this.OnVisibilityChanged );
      }

    }

    protected override void StopListening( object source )
    {
      DetailConfiguration detailConfig = source as DetailConfiguration;
      if( detailConfig != null )
      {
        detailConfig.VisibilityChanged -= new EventHandler( this.OnVisibilityChanged );
        return;
      }

      DetailConfigurationCollection detailConfigCollection = source as DetailConfigurationCollection;
      if( detailConfigCollection != null )
      {
        detailConfigCollection.DetailVisibilityChanged -= new EventHandler( this.OnVisibilityChanged );
      }
    }

    private static DetailVisibilityChangedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( DetailVisibilityChangedEventManager );
        DetailVisibilityChangedEventManager currentManager = ( DetailVisibilityChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new DetailVisibilityChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void OnVisibilityChanged( object sender, EventArgs e )
    {
      this.DeliverEvent( sender, e );
    }
  }
}
