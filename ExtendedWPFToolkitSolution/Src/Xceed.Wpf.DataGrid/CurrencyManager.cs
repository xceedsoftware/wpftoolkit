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
using System.Windows.Data;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  internal class CurrencyManager
  {
    #region CONSTRUCTORS

    public CurrencyManager( DataGridContext dataGridContext, CollectionView collectionView )
    {
      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      if( collectionView == null )
        throw new ArgumentNullException( "collectionView" );

      m_dataGridContext = dataGridContext;
      m_collectionView = collectionView;

      System.Diagnostics.Debug.Assert( m_dataGridContext.CurrentItem == m_collectionView.CurrentItem );

      this.RegisterListeners();
    }

    #endregion CONSTRUCTORS

    #region IsCurrentChanging Property

    public bool IsCurrentChanging
    {
      get;
      private set;
    }

    #endregion IsCurrentChanging Property

    #region PUBLIC METHODS

    public void CleanCurrencyManager()
    {
      this.UnregisterListeners();

      m_dataGridContext = null;
      m_collectionView = null;
    }

    #endregion PUBLIC METHODS

    #region EVENT HANDLERS

    private void OnDataGridContextCurrentItemChanged( object sender, EventArgs e )
    {
      this.ChangeCollectionViewCurrentItem();
    }

    private void OnCollectionViewCurrentChanged( object sender, EventArgs e )
    {
      this.ChangeDataGridContextCurrentItem();
    }

    #endregion EVENT HANDLERS

    #region PRIVATE PROPERTIES

    private bool IsSetCurrentInProgress
    {
      get
      {
        return m_dataGridContext.DataGridControl.IsSetCurrentInProgress;
      }
    }

    private bool ShouldSynchronizeCurrentItem
    {
      get
      {
        return m_dataGridContext.DataGridControl.ShouldSynchronizeCurrentItem;
      }
    }

    private bool ShouldSynchronizeSelectionWithCurrent
    {
      get
      {
        return m_dataGridContext.DataGridControl.SynchronizeSelectionWithCurrent;
      }
    }

    #endregion

    #region PRIVATE METHODS

    private void ChangeCollectionViewCurrentItem()
    {
      if( ( this.IsCurrentChanging ) || ( !this.ShouldSynchronizeCurrentItem ) )
        return;

      this.IsCurrentChanging = true;

      try
      {
        // Synchronize the CurrentItem of the CollecitonView 
        // with the one of the DataGridContext.
        m_collectionView.MoveCurrentToPosition( m_dataGridContext.CurrentItemIndex );
      }
      finally
      {
        this.IsCurrentChanging = false;
      }
    }

    private void ChangeDataGridContextCurrentItem()
    {
      // Prevent when a SetCurrent is in progress
      if( this.IsSetCurrentInProgress )
        return;

      if( ( this.IsCurrentChanging ) || ( !this.ShouldSynchronizeCurrentItem ) )
        return;

      this.IsCurrentChanging = true;

      try
      {
        // Synchronize the CurrentItem of the DataGridContext
        // with the one of the CollecitonView.
        m_dataGridContext.SetCurrent(
          m_collectionView.CurrentItem, null, m_collectionView.CurrentPosition,
          m_dataGridContext.CurrentColumn, false, false, this.ShouldSynchronizeSelectionWithCurrent );

        if( m_collectionView.CurrentItem == null )
          this.ChangeCurrentDataGridContext();
      }
      catch( DataGridException )
      {
        // When we deleted an item in edition that contain invalid data, we don't want a throw to go all the way up to the end user.
        // We try to abort the edition in that case.

        if( ( m_dataGridContext.IsCurrent ) && ( m_dataGridContext.DataGridControl.IsBeingEdited ) && ( !m_dataGridContext.Items.Contains( m_dataGridContext.CurrentItem ) ) )
        {
          m_dataGridContext.CancelEdit();

          m_dataGridContext.SetCurrent(
            m_collectionView.CurrentItem, null, m_collectionView.CurrentPosition,
            m_dataGridContext.CurrentColumn, false, false, this.ShouldSynchronizeSelectionWithCurrent );

          if( m_collectionView.CurrentItem == null )
            this.ChangeCurrentDataGridContext();
        }
      }
      finally
      {
        this.IsCurrentChanging = false;
      }
    }

    private void ChangeCurrentDataGridContext()
    {
      if( ( m_dataGridContext.ParentDataGridContext == null )
        || ( m_dataGridContext.ParentItem == null ) )
      {
        return;
      }

      List<DataGridContext> childContexts = this.GetParentItemChildContexts();
      int currentContextIndex = childContexts.IndexOf( m_dataGridContext );

      if( childContexts.Count > 1 )
      {
        if( currentContextIndex < ( childContexts.Count - 1 ) )
        {
          // We are not the last context, we'll search for a
          // non-empty context after us.
          for( int i = currentContextIndex + 1; i < childContexts.Count; i++ )
          {
            DataGridContext childContext = childContexts[ i ];

            if( childContext.Items.Count == 0 )
              continue;

            childContext.SetCurrentItemCore( childContext.Items.GetItemAt( 0 ), false, this.ShouldSynchronizeSelectionWithCurrent );
            return;
          }
        }

        // No context have been found. We'll search for a 
        // non-empty context before us.
        for( int i = currentContextIndex - 1; i >= 0; i-- )
        {
          DataGridContext childContext = childContexts[ i ];

          int count = childContext.Items.Count;

          if( count == 0 )
            continue;

          childContext.SetCurrentItemCore( childContext.Items.GetItemAt( count - 1 ), false, this.ShouldSynchronizeSelectionWithCurrent );
          return;
        }
      }

      // No context after or before us have been found, we will set the CurrentItem to our ParentItem.
      m_dataGridContext.ParentDataGridContext.SetCurrentItemCore( m_dataGridContext.ParentItem, false, this.ShouldSynchronizeSelectionWithCurrent );
    }

    private List<DataGridContext> GetParentItemChildContexts()
    {
      List<DataGridContext> childContexts = new List<DataGridContext>();

      foreach( DetailConfiguration detailConfig in m_dataGridContext.ParentDataGridContext.DetailConfigurations )
      {
        childContexts.Add( m_dataGridContext.ParentDataGridContext.GetChildContext( m_dataGridContext.ParentItem, detailConfig ) );
      }

      return childContexts;
    }

    private void RegisterListeners()
    {
      // We are not checking this.ShouldSynchronizeCurrentItem since it might return the wrong value if the grid
      // is still in the process of being instantiated.
      m_dataGridContext.CurrentItemChanged += new EventHandler( this.OnDataGridContextCurrentItemChanged );
      m_collectionView.CurrentChanged += new EventHandler( this.OnCollectionViewCurrentChanged );
    }

    private void UnregisterListeners()
    {
      // We are not checking this.ShouldSynchronizeCurrentItem since it might have returned the wrong value 
      // when RegisterListeners was called if the grid was still in the process of being instantiated.
      m_dataGridContext.CurrentItemChanged -= new EventHandler( this.OnDataGridContextCurrentItemChanged );
      m_collectionView.CurrentChanged -= new EventHandler( this.OnCollectionViewCurrentChanged );
    }

    #endregion PRIVATE METHODS

    #region PRIVATE FIELDS

    private DataGridContext m_dataGridContext;
    private CollectionView m_collectionView;

    #endregion PRIVATE FIELDS
  }
}
