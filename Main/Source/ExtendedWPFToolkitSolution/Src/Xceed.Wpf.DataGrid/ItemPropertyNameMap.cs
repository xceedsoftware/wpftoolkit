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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class ItemPropertyNameMap : FieldNameMap, INotifyCollectionChanged, IWeakEventListener
  {
    #region Constructors

    internal ItemPropertyNameMap( DataGridItemPropertyCollection itemProperties )
    {
      if( itemProperties == null )
        throw new ArgumentNullException( "itemProperties" );

      m_dataGridItemPropertyCollection = new WeakReference( itemProperties );

      CollectionChangedEventManager.AddListener( itemProperties, this );
    }

    #endregion

    #region ItemProperties Internal Property

    internal DataGridItemPropertyCollection ItemProperties
    {
      get
      {
        return ( DataGridItemPropertyCollection )m_dataGridItemPropertyCollection.Target;
      }
    }

    private readonly WeakReference m_dataGridItemPropertyCollection; //null

    #endregion

    protected override bool TryGetSource( string targetName, out string sourceName )
    {
      var itemProperties = this.ItemProperties;
      if( itemProperties == null )
        throw new InvalidOperationException( "Garbage collection has been performed on the collection." );

      sourceName = null;

      if( !string.IsNullOrEmpty( targetName ) )
      {
        foreach( var itemProperty in itemProperties )
        {
          if( itemProperty.Synonym == targetName )
          {
            sourceName = itemProperty.Name;
            break;
          }
        }
      }

      return ( sourceName != null );
    }

    protected override bool TryGetTarget( string sourceName, out string targetName )
    {
      var itemProperties = this.ItemProperties;
      if( itemProperties == null )
        throw new InvalidOperationException( "Garbage collection has been performed on the collection." );

      targetName = null;

      if( !string.IsNullOrEmpty( sourceName ) )
      {
        var itemProperty = itemProperties[ sourceName ];
        if( ( itemProperty != null ) && !string.IsNullOrEmpty( itemProperty.Synonym ) )
        {
          targetName = itemProperty.Synonym;
        }
      }

      return ( targetName != null );
    }

    private void OnItemPropertyCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
    {
      Debug.Assert( sender == this.ItemProperties );

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Reset:
          this.RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
          break;

        case NotifyCollectionChangedAction.Add:
          if( ( e.NewItems == null ) || ( e.NewItems.Count != 1 ) )
          {
            this.RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
          }
          else
          {
            this.RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, e.NewItems[ 0 ] ) );
          }
          break;

        case NotifyCollectionChangedAction.Remove:
          if( ( e.OldItems == null ) || ( e.OldItems.Count != 1 ) )
          {
            this.RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
          }
          else
          {
            this.RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, e.OldItems[ 0 ] ) );
          }
          break;

        case NotifyCollectionChangedAction.Replace:
          if( ( e.OldItems == null ) || ( e.NewItems == null ) || ( e.OldItems.Count != 1 ) || ( e.NewItems.Count != 1 ) )
          {
            this.RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
          }
          else
          {
            this.RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, e.NewItems[ 0 ], e.OldItems[ 0 ] ) );
          }
          break;

        default:
          break;
      }
    }

    #region INotifyCollectionChanged Members

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    private void RaiseCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      var handler = this.CollectionChanged;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        this.OnItemPropertyCollectionChanged( sender, ( NotifyCollectionChangedEventArgs )e );

        return true;
      }

      return false;
    }

    #endregion
  }
}
