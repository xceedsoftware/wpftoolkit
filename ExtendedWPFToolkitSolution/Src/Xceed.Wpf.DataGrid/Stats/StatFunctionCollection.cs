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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Xceed.Wpf.DataGrid.Stats
{
  internal sealed class StatFunctionCollection : ObservableCollection<StatFunction>
  {
    #region Constructor

    internal StatFunctionCollection()
      : base()
    {
    }

    #endregion

    #region [] Property

    public StatFunction this[ string resultPropertyName ]
    {
      get
      {
        if( resultPropertyName == null )
          return null;

        StatFunction value;
        if( m_lookup.TryGetValue( resultPropertyName, out value ) )
          return value;

        return null;
      }
    }

    #endregion

    public bool Contains( string resultPropertyName )
    {
      if( resultPropertyName == null )
        return false;

      return m_lookup.ContainsKey( resultPropertyName );
    }

    protected override void InsertItem( int index, StatFunction item )
    {
      this.PrepareStatFunction( item );
      base.InsertItem( index, item );
    }

    protected override void SetItem( int index, StatFunction item )
    {
      this.PrepareStatFunction( item );
      base.SetItem( index, item );
    }

    protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Add:
          this.AddStatFunctions( e.NewItems.Cast<StatFunction>() );
          break;

        case NotifyCollectionChangedAction.Remove:
          this.RemoveStatFunctions( e.OldItems.Cast<StatFunction>() );
          break;

        case NotifyCollectionChangedAction.Replace:
          this.RemoveStatFunctions( e.OldItems.Cast<StatFunction>() );
          this.AddStatFunctions( e.NewItems.Cast<StatFunction>() );
          break;

        case NotifyCollectionChangedAction.Reset:
          this.ResetStatFunctions();
          break;
      }

      base.OnCollectionChanged( e );
    }

    private void PrepareStatFunction( StatFunction item )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      var key = item.ResultPropertyName;
      if( key == null )
        throw new ArgumentException( "TODODOC: The StatFunction must have a ResultPropertyName", "item" );

      if( this.Contains( key ) )
        throw new ArgumentException( "A StatFunction with the same ResultPropertyName already exists in the StatFunctionCollection." );

      item.Validate();
      item.Seal();
    }

    private void AddStatFunctions( IEnumerable<StatFunction> items )
    {
      foreach( StatFunction item in items )
      {
        m_lookup.Add( item.ResultPropertyName, item );
      }
    }

    private void RemoveStatFunctions( IEnumerable<StatFunction> items )
    {
      foreach( StatFunction item in items )
      {
        m_lookup.Remove( item.ResultPropertyName );
      }
    }

    private void ResetStatFunctions()
    {
      m_lookup.Clear();

      this.AddStatFunctions( this );
    }

    #region Private Fields

    private readonly Dictionary<string, StatFunction> m_lookup = new Dictionary<string, StatFunction>();

    #endregion
  }
}
