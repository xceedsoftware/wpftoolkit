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
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid
{
  internal class ItemPropertiesChangedEventManager : WeakEventManager
  {
    private ItemPropertiesChangedEventManager()
    {
    }

    public static void AddListener( DataGridItemPropertyCollection source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( DataGridItemPropertyCollection source, IWeakEventListener listener )
    {
      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      DataGridItemPropertyCollection itemProperties = source as DataGridItemPropertyCollection;
      itemProperties.CollectionChanged += new NotifyCollectionChangedEventHandler( ItemProperties_CollectionChanged );
    }

    protected override void StopListening( object source )
    {
      DataGridItemPropertyCollection itemProperties = source as DataGridItemPropertyCollection;
      itemProperties.CollectionChanged -= new NotifyCollectionChangedEventHandler( ItemProperties_CollectionChanged );
    }

    private static ItemPropertiesChangedEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof( ItemPropertiesChangedEventManager );
        ItemPropertiesChangedEventManager currentManager = ( ItemPropertiesChangedEventManager )WeakEventManager.GetCurrentManager( managerType );

        if( currentManager == null )
        {
          currentManager = new ItemPropertiesChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    private void ItemProperties_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      this.DeliverEvent( sender, e );
    }
  }
}
