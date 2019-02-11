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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class ColumnSortCommand : ColumnCommand
  {
    protected SynchronizationContext StartSynchronizing( SortDescriptionsSyncContext context )
    {
      return new SynchronizationContext( context );
    }

    protected bool TryDeferResort( DetailConfiguration detailConfiguration, out IDisposable defer )
    {
      defer = null;

      return ( detailConfiguration != null )
          && ( this.TryDeferResort( detailConfiguration.SortDescriptions, out defer ) );
    }

    protected bool TryDeferResort( SortDescriptionCollection sortDescriptions, out IDisposable defer )
    {
      var dataGridSortDescriptions = sortDescriptions as DataGridSortDescriptionCollection;
      if( dataGridSortDescriptions != null )
      {
        defer = dataGridSortDescriptions.DeferResort();
      }
      else
      {
        defer = null;
      }

      return ( defer != null );
    }

    protected IDisposable DeferResortHelper(
      IEnumerable itemsSourceCollection,
      CollectionView collectionView )
    {
      var dataGridCollectionView = itemsSourceCollection as DataGridCollectionViewBase;
      if( dataGridCollectionView != null )
        return dataGridCollectionView.DataGridSortDescriptions.DeferResort();

      ColumnSortCommand.ThrowIfNull( collectionView, "collectionView" );

      return collectionView.DeferRefresh();
    }

    #region SynchronizationContext Protected Class

    protected sealed class SynchronizationContext : IDisposable
    {
      #region Constructor

      internal SynchronizationContext( SortDescriptionsSyncContext context )
      {
        if( context == null )
          throw new ArgumentNullException( "context" );

        m_context = context;
        m_isOwner = !context.ProcessingSortSynchronization;

        context.ProcessingSortSynchronization = true;
      }

      #endregion

      #region Own Property

      public bool Own
      {
        get
        {
          var context = m_context;

          return ( m_isOwner )
              && ( context != null )
              && ( context.ProcessingSortSynchronization );
        }
      }

      #endregion

      #region IDisposable Members

      public void Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        var context = m_context;
        if( context == null )
          return;

        m_context = null;

        if( m_isOwner )
        {
          context.ProcessingSortSynchronization = false;
        }
      }

      ~SynchronizationContext()
      {
        this.Dispose( false );
      }

      #endregion

      #region Private Fields

      private SortDescriptionsSyncContext m_context;
      private bool m_isOwner;

      #endregion
    }

    #endregion

    #region Disposer Protected Class

    protected sealed class Disposer : IDisposable
    {
      #region Constants

      private static readonly int DisposableTypeCount = Enum.GetValues( typeof( DisposableType ) ).Length;

      #endregion

      public Disposer()
      {
        m_disposable = new Stack<IDisposable>[ Disposer.DisposableTypeCount ];
      }

      public void Add( IDisposable disposable, DisposableType disposableType )
      {
        if( m_disposed )
          throw new ObjectDisposedException( "Disposer" );

        if( !Enum.IsDefined( typeof( DisposableType ), disposableType ) )
          throw new ArgumentException( "disposableType" );

        if( disposable == null )
          return;

        int index = Disposer.GetIndex( disposableType );
        var collection = m_disposable[ index ];

        if( collection == null )
        {
          collection = new Stack<IDisposable>();
          m_disposable[ index ] = collection;
        }

        collection.Push( disposable );
      }

      private static int GetIndex( DisposableType value )
      {
        int index = System.Convert.ToInt32( value );
        Debug.Assert( ( index >= 0 ) && ( index < Disposer.DisposableTypeCount ) );

        return index;
      }

      #region IDisposable Members

      public void Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        if( m_disposed )
          return;

        m_disposed = true;

        Exception exception = null;

        for( int i = m_disposable.Length - 1; i >= 0; i-- )
        {
          var disposable = m_disposable[ i ];
          if( disposable == null )
            continue;

          while( disposable.Count > 0 )
          {
            try
            {
              disposable.Pop().Dispose();
            }
            catch( Exception e )
            {
              if( exception == null )
              {
                exception = e;
              }
            }
          }

          m_disposable[ i ] = null;
        }

        if( exception != null )
          throw new DataGridInternalException( exception );
      }

      ~Disposer()
      {
        this.Dispose( false );
      }

      #endregion

      #region Private Fields

      private readonly Stack<IDisposable>[] m_disposable;
      private bool m_disposed;

      #endregion
    }

    #endregion

    #region DisposableType Protected Nested Type

    protected enum DisposableType
    {
      DeferRestoreState = 0,
      DeferResort = 1,
    }

    #endregion
  }
}
