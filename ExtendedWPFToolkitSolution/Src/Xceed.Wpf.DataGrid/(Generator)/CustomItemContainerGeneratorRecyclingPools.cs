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
using System.Diagnostics;
using System.Windows;
using Xceed.Utils.Wpf;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class CustomItemContainerGeneratorRecyclingPools
  {
    #region ContainersRemoved Event

    internal event ContainersRemovedEventHandler ContainersRemoved;

    private void OnContainersRemoved()
    {
      if( ( m_containersRemoved == null ) || ( m_deferCount != 0 ) )
        return;

      var containers = m_containersRemoved;
      m_containersRemoved = null;

      if( containers.Count <= 0 )
        return;

      var handler = this.ContainersRemoved;
      if( handler == null )
        return;

      handler.Invoke( this, new ContainersRemovedEventArgs( containers ) );
    }

    #endregion

    #region RecyclingCandidateCleaned Event

    internal event RecyclingCandidatesCleanedEventHandler RecyclingCandidatesCleaned;

    private void OnRecyclingCandidatesCleaned( List<DependencyObject> recyclingCandidates )
    {
      if( recyclingCandidates.Count == 0 )
        return;

      var handler = this.RecyclingCandidatesCleaned;
      if( handler == null )
        return;

      handler.Invoke( this, new RecyclingCandidatesCleanedEventArgs( recyclingCandidates ) );
    }

    #endregion

    internal IDisposable DeferContainersRemoved()
    {
      return new DeferredDisposable( new DeferContainersRemovedState( this ) );
    }

    internal void Clear()
    {
      m_containersRemoved = m_containersRemoved ?? new List<DependencyObject>();

      this.ClearPools( m_itemContainerPools, m_containersRemoved, pool => pool.Clear() );
      this.ClearPools( m_headerFooterItemContainerPools, m_containersRemoved, pool => pool.Clear() );
      this.ClearPools( m_groupHeaderFooterItemContainerPools, m_containersRemoved, pool => pool.Clear() );

      this.OnContainersRemoved();
    }

    internal void Clear( DetailConfiguration detailConfiguration )
    {
      m_containersRemoved = m_containersRemoved ?? new List<DependencyObject>();

      var key = CustomItemContainerGeneratorRecyclingPools.GetKey( detailConfiguration );

      var itemContainerPool = default( ItemContainerRecyclingPool );
      if( m_itemContainerPools.TryGetValue( key, out itemContainerPool ) )
      {
        m_itemContainerPools.Remove( key );

        this.ClearPool( itemContainerPool, m_containersRemoved, pool => pool.Clear() );
      }

      var headerFooterItemContainerPool = default( HeaderFooterItemContainerRecyclingPool );
      if( m_headerFooterItemContainerPools.TryGetValue( key, out headerFooterItemContainerPool ) )
      {
        m_headerFooterItemContainerPools.Remove( key );

        this.ClearPool( headerFooterItemContainerPool, m_containersRemoved, pool => pool.Clear() );
      }

      var groupHeaderFooterItemContainerPool = default( GroupHeaderFooterItemContainerRecyclingPool );
      if( m_groupHeaderFooterItemContainerPools.TryGetValue( key, out groupHeaderFooterItemContainerPool ) )
      {
        m_groupHeaderFooterItemContainerPools.Remove( key );

        this.ClearPool( groupHeaderFooterItemContainerPool, m_containersRemoved, pool => pool.Clear() );
      }

      this.OnContainersRemoved();
    }

    internal void CleanRecyclingCandidates()
    {
      var recyclingCandidates = new List<DependencyObject>( 4 );

      foreach( var pool in m_itemContainerPools.Values )
      {
        foreach( var container in pool )
        {
          var dataGridItemContainer = container as IDataGridItemContainer;
          if( dataGridItemContainer != null && dataGridItemContainer.IsRecyclingCandidate )
          {
            dataGridItemContainer.IsRecyclingCandidate = false;
            dataGridItemContainer.CleanRecyclingCandidate();
            recyclingCandidates.Add( container );
          }
        }
      }

      foreach( var pool in m_headerFooterItemContainerPools.Values )
      {
        foreach( var container in pool )
        {
          var dataGridItemContainer = container as IDataGridItemContainer;
          if( dataGridItemContainer != null && dataGridItemContainer.IsRecyclingCandidate )
          {
            dataGridItemContainer.IsRecyclingCandidate = false;
            dataGridItemContainer.CleanRecyclingCandidate();
            recyclingCandidates.Add( container );
          }
        }
      }

      foreach( var pool in m_groupHeaderFooterItemContainerPools.Values )
      {
        foreach( var container in pool )
        {
          var dataGridItemContainer = container as IDataGridItemContainer;
          if( dataGridItemContainer != null && dataGridItemContainer.IsRecyclingCandidate )
          {
            dataGridItemContainer.IsRecyclingCandidate = false;
            dataGridItemContainer.CleanRecyclingCandidate();
            recyclingCandidates.Add( container );
          }
        }
      }

      this.OnRecyclingCandidatesCleaned( recyclingCandidates );
    }

    internal ItemContainerRecyclingPool GetItemContainerPool( DetailConfiguration detailConfiguration )
    {
      return this.GetPool( m_itemContainerPools, detailConfiguration, false );
    }

    internal ItemContainerRecyclingPool GetItemContainerPool( DetailConfiguration detailConfiguration, bool create )
    {
      return this.GetPool( m_itemContainerPools, detailConfiguration, create );
    }

    internal HeaderFooterItemContainerRecyclingPool GetHeaderFooterItemContainerPool( DetailConfiguration detailConfiguration )
    {
      return this.GetPool( m_headerFooterItemContainerPools, detailConfiguration, false );
    }

    internal HeaderFooterItemContainerRecyclingPool GetHeaderFooterItemContainerPool( DetailConfiguration detailConfiguration, bool create )
    {
      return this.GetPool( m_headerFooterItemContainerPools, detailConfiguration, create );
    }

    internal GroupHeaderFooterItemContainerRecyclingPool GetGroupHeaderFooterItemContainerPool( DetailConfiguration detailConfiguration )
    {
      return this.GetPool( m_groupHeaderFooterItemContainerPools, detailConfiguration, false );
    }

    internal GroupHeaderFooterItemContainerRecyclingPool GetGroupHeaderFooterItemContainerPool( DetailConfiguration detailConfiguration, bool create )
    {
      return this.GetPool( m_groupHeaderFooterItemContainerPools, detailConfiguration, create );
    }

    private T GetPool<T>( Dictionary<object, T> pools, DetailConfiguration detailConfiguration, bool create ) where T : class, new()
    {
      if( pools == null )
        throw new ArgumentNullException( "pools" );

      var key = CustomItemContainerGeneratorRecyclingPools.GetKey( detailConfiguration );
      var pool = default( T );

      if( !pools.TryGetValue( key, out pool ) )
      {
        if( create )
        {
          pool = new T();
          pools.Add( key, pool );
        }
        else
        {
          pool = null;
        }
      }

      return pool;
    }

    private void ClearPools<T>(
      Dictionary<object, T> pools,
      ICollection<DependencyObject> containers,
      Action<T> clearPool ) where T : IEnumerable<DependencyObject>
    {
      Debug.Assert( pools != null );

      foreach( var pool in pools.Values )
      {
        this.ClearPool( pool, containers, clearPool );
      }

      pools.Clear();
    }

    private void ClearPool<T>(
      T pool,
      ICollection<DependencyObject> containers,
      Action<T> clearPool ) where T : IEnumerable<DependencyObject>
    {
      Debug.Assert( pool != null );
      Debug.Assert( clearPool != null );

      foreach( var container in pool )
      {
        containers.Add( container );
      }

      clearPool.Invoke( pool );
    }

    private static object GetKey( DetailConfiguration detailConfiguration )
    {
      return detailConfiguration ?? CustomItemContainerGeneratorRecyclingPools.MasterConfiguration;
    }

    private static readonly object MasterConfiguration = new object();

    private int m_deferCount; //0
    private List<DependencyObject> m_containersRemoved; //null

    private readonly Dictionary<object, ItemContainerRecyclingPool> m_itemContainerPools = new Dictionary<object, ItemContainerRecyclingPool>();
    private readonly Dictionary<object, HeaderFooterItemContainerRecyclingPool> m_headerFooterItemContainerPools = new Dictionary<object, HeaderFooterItemContainerRecyclingPool>();
    private readonly Dictionary<object, GroupHeaderFooterItemContainerRecyclingPool> m_groupHeaderFooterItemContainerPools = new Dictionary<object, GroupHeaderFooterItemContainerRecyclingPool>();

    #region DeferContainersRemovedState Private Class

    private sealed class DeferContainersRemovedState : DeferredDisposableState
    {
      internal DeferContainersRemovedState( CustomItemContainerGeneratorRecyclingPools target )
      {
        if( target == null )
          throw new ArgumentNullException( "target" );

        m_target = target;
      }

      protected override bool IsDeferred
      {
        get
        {
          return ( m_target.m_deferCount != 0 );
        }
      }

      protected override void Increment()
      {
        m_target.m_deferCount++;
      }

      protected override void Decrement()
      {
        m_target.m_deferCount--;
      }

      protected override void OnDeferEnded( bool disposing )
      {
        m_target.OnContainersRemoved();
      }

      private readonly CustomItemContainerGeneratorRecyclingPools m_target;
    }

    #endregion
  }
}
