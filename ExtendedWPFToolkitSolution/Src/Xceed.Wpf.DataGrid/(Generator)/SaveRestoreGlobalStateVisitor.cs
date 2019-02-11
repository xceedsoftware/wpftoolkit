/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections.Generic;

namespace Xceed.Wpf.DataGrid
{
  internal class SaveRestoreGlobalStateVisitor : SaveRestoreStateVisitor
  {
    internal SaveRestoreGlobalStateVisitor( bool stopAtFirstCollapsedGroup )
    {
      m_stopAtFirstCollapsedGroup = stopAtFirstCollapsedGroup;
    }

    #region StopAtFirstCollapsedGroup Property

    internal bool StopAtFirstCollapsedGroup
    {
      get
      {
        return m_stopAtFirstCollapsedGroup;
      }
      set
      {
        m_stopAtFirstCollapsedGroup = value;
      }
    }

    private bool m_stopAtFirstCollapsedGroup;

    #endregion

    protected override void InitializeCore()
    {
      m_dataGridContextsStateDictionary = new Dictionary<WeakDataGridContextKey, SaveRestoreDataGridContextStateVisitor>();
    }

    protected override void SaveStateCore( IDataGridContextVisitable dataGridContextVisitable )
    {
      var dataGridContext = SaveRestoreStateVisitor.GetDataGridContextFromVisitable( dataGridContextVisitable );

      this.RecursiveSaveDataGridContextsState( dataGridContext );
    }

    protected override void RestoreStateCore( IDataGridContextVisitable dataGridContextVisitable )
    {
      var dataGridContext = SaveRestoreStateVisitor.GetDataGridContextFromVisitable( dataGridContextVisitable );

      this.RecursiveRestoreDataGridContextsState( dataGridContext );
    }

    private void RecursiveSaveDataGridContextsState( DataGridContext dataGridContext )
    {
      var saveRestoreDataGridContextStateVisitor = new SaveRestoreDataGridContextStateVisitor( true, int.MaxValue, m_stopAtFirstCollapsedGroup );

      saveRestoreDataGridContextStateVisitor.SaveState( dataGridContext as IDataGridContextVisitable );

      m_dataGridContextsStateDictionary.Add( new WeakDataGridContextKey( dataGridContext ), saveRestoreDataGridContextStateVisitor );

      foreach( var subDataGridContext in dataGridContext.GetChildContextsCore() )
      {
        this.RecursiveSaveDataGridContextsState( subDataGridContext );
      }
    }

    private void RecursiveRestoreDataGridContextsState( DataGridContext dataGridContext )
    {
      var weakDataGridContextKey = new WeakDataGridContextKey( dataGridContext );
      var saveRestoreDataGridContextStateVisitor = default( SaveRestoreDataGridContextStateVisitor );

      if( m_dataGridContextsStateDictionary.TryGetValue( weakDataGridContextKey, out saveRestoreDataGridContextStateVisitor ) )
      {
        try
        {
          saveRestoreDataGridContextStateVisitor.RestoreState( dataGridContext as IDataGridContextVisitable );
        }
        finally
        {
          m_dataGridContextsStateDictionary.Remove( weakDataGridContextKey );
        }
      }

      foreach( var subDataGridContext in dataGridContext.GetChildContextsCore() )
      {
        this.RecursiveRestoreDataGridContextsState( subDataGridContext );
      }
    }

    private Dictionary<WeakDataGridContextKey, SaveRestoreDataGridContextStateVisitor> m_dataGridContextsStateDictionary;
  }
}
