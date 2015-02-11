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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class ItemPropertyGroupSortStatNameChangedEventManager : WeakEventManager
  {
    private ItemPropertyGroupSortStatNameChangedEventManager()
    {
    }

    #region CurrentManager Private Property

    private static ItemPropertyGroupSortStatNameChangedEventManager CurrentManager
    {
      get
      {
        var managerType = typeof( ItemPropertyGroupSortStatNameChangedEventManager );
        var currentManager = WeakEventManager.GetCurrentManager( managerType ) as ItemPropertyGroupSortStatNameChangedEventManager;

        if( currentManager == null )
        {
          currentManager = new ItemPropertyGroupSortStatNameChangedEventManager();
          WeakEventManager.SetCurrentManager( managerType, currentManager );
        }

        return currentManager;
      }
    }

    #endregion

    public static void AddListener( DataGridItemPropertyCollection source, IWeakEventListener listener )
    {
      if( source == null )
        throw new ArgumentNullException( "source" );

      if( listener == null )
        throw new ArgumentNullException( "listener" );

      CurrentManager.ProtectedAddListener( source, listener );
    }

    public static void RemoveListener( DataGridItemPropertyCollection source, IWeakEventListener listener )
    {
      if( source == null )
        throw new ArgumentNullException( "source" );

      if( listener == null )
        throw new ArgumentNullException( "listener" );

      CurrentManager.ProtectedRemoveListener( source, listener );
    }

    protected override void StartListening( object source )
    {
      var target = source as DataGridItemPropertyCollection;
      //target.ItemPropertyGroupSortStatNameChanged += new EventHandler( this.DeliverEvent );
      target.ItemPropertyGroupSortStatNameChanged += target_ItemPropertyGroupSortStatNameChanged;
    }

    void target_ItemPropertyGroupSortStatNameChanged( object sender, EventArgs e )
    {
      this.DeliverEvent( sender, e );
    }

    protected override void StopListening( object source )
    {
      var target = source as DataGridItemPropertyCollection;
      target.ItemPropertyGroupSortStatNameChanged -= new EventHandler( this.DeliverEvent );
    }
  }
}
