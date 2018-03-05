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
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class UpdateColumnSortCommand : ColumnSortCommand
  {
    #region SortDescriptionsSyncContext Protected Property

    protected abstract SortDescriptionsSyncContext SortDescriptionsSyncContext
    {
      get;
    }

    #endregion

    public bool CanExecute()
    {
      return this.CanExecuteCore();
    }

    public void Execute()
    {
      if( !this.CanExecute() )
        return;

      this.ExecuteCore();
    }

    protected abstract bool CanExecuteCore();
    protected abstract void ExecuteCore();

    protected override bool CanExecuteImpl( object parameter )
    {
      return this.CanExecuteCore();
    }

    protected override void ExecuteImpl( object parameter )
    {
      this.Execute();
    }

    protected void SynchronizeColumnSort(
      SynchronizationContext synchronizationContext,
      SortDescriptionCollection sortDescriptions,
      ColumnCollection columns )
    {
      ColumnSortCommand.ThrowIfNull( synchronizationContext, "synchronizationContext" );
      ColumnSortCommand.ThrowIfNull( sortDescriptions, "sortDescriptions" );
      ColumnSortCommand.ThrowIfNull( columns, "columns" );

      if( !synchronizationContext.Own || !columns.Any() )
        return;

      this.SetResortCallback( sortDescriptions, columns );

      int count = sortDescriptions.Count;
      Dictionary<string, ColumnSortInfo> sortOrder = new Dictionary<string, ColumnSortInfo>( count );

      for( int i = 0; i < count; i++ )
      {
        var sortDescription = sortDescriptions[ i ];
        string propertyName = sortDescription.PropertyName;

        if( sortOrder.ContainsKey( propertyName ) )
          continue;

        sortOrder.Add( propertyName, new ColumnSortInfo( i, sortDescription.Direction ) );
      }

      foreach( var column in columns )
      {
        ColumnSortInfo entry;

        if( sortOrder.TryGetValue( column.FieldName, out entry ) )
        {
          column.SetSortIndex( entry.Index );
          column.SetSortDirection( entry.Direction );
        }
        else
        {
          column.SetSortIndex( -1 );
          column.SetSortDirection( SortDirection.None );
        }
      }
    }

    private void SetResortCallback( SortDescriptionCollection sortDescriptions, ColumnCollection columns )
    {
      var collection = sortDescriptions as DataGridSortDescriptionCollection;
      if( ( m_resortCallback != null ) || ( collection == null ) || !collection.IsResortDefered )
        return;

      m_resortCallback = new ResortCallback( this, sortDescriptions, columns );
      collection.AddResortNotification( m_resortCallback );
    }

    private void OnResortCallback( SortDescriptionCollection sortDescriptions, ColumnCollection columns )
    {
      using( var synchronizationContext = this.StartSynchronizing( this.SortDescriptionsSyncContext ) )
      {
        this.SynchronizeColumnSort( synchronizationContext, sortDescriptions, columns );
      }
    }

    #region Private Fields

    private IDisposable m_resortCallback; //null

    #endregion

    #region ColumnSortInfo Private Nested Type

    private struct ColumnSortInfo
    {
      public ColumnSortInfo( int index, ListSortDirection direction )
      {
        this.Index = index;
        this.Direction = ColumnSortInfo.Convert( direction );
      }

      private static SortDirection Convert( ListSortDirection value )
      {
        if( value == ListSortDirection.Descending )
          return SortDirection.Descending;

        return SortDirection.Ascending;
      }

      public readonly int Index;
      public readonly SortDirection Direction;
    }

    #endregion

    #region ResortCallback Private Class

    private sealed class ResortCallback : IDisposable
    {
      internal ResortCallback(
        UpdateColumnSortCommand owner,
        SortDescriptionCollection sortDescriptions,
        ColumnCollection columns )
      {
        if( owner == null )
          throw new ArgumentNullException( "owner" );

        m_owner = new WeakReference( owner );
        m_sortDescriptions = sortDescriptions;
        m_columns = columns;
      }

      void IDisposable.Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        if( !disposing )
          return;

        var ownerRef = Interlocked.Exchange( ref m_owner, null );
        if( ownerRef == null )
          return;

        var owner = ( UpdateColumnSortCommand )ownerRef.Target;
        if( owner != null )
        {
          owner.OnResortCallback( m_sortDescriptions, m_columns );
        }

        m_sortDescriptions = null;
        m_columns = null;
      }

      private WeakReference m_owner;
      private SortDescriptionCollection m_sortDescriptions;
      private ColumnCollection m_columns;
    }

    #endregion
  }
}
