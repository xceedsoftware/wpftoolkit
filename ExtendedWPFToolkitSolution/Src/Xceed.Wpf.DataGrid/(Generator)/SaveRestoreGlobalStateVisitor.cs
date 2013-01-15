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
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  internal class SaveRestoreGlobalStateVisitor : SaveRestoreStateVisitor
  {
    #region CONSTRUCTORS

    public SaveRestoreGlobalStateVisitor( bool stopAtFirstCollapsedGroup )
    {
      m_stopAtFirstCollapsedGroup = stopAtFirstCollapsedGroup;
    }

    #endregion CONSTRUCTORS

    #region StopAtFirstCollapsedGroup PROPERTY

    private bool m_stopAtFirstCollapsedGroup;

    public bool StopAtFirstCollapsedGroup
    {
      get { return m_stopAtFirstCollapsedGroup; }
      set { m_stopAtFirstCollapsedGroup = value; }
    }

    #endregion StopAtFirstCollapsedGroup PROPERTY

    #region PROTECTED METHODS

    protected override void InitializeCore()
    {
      m_dataGridContextsStateDictionary = new Dictionary<WeakDataGridContextKey, SaveRestoreDataGridContextStateVisitor>();
    }

    protected override void SaveStateCore( IDataGridContextVisitable dataGridContextVisitable )
    {
      DataGridContext rootDataGridContext = SaveRestoreStateVisitor.GetDataGridContextFromVisitable( dataGridContextVisitable );

      this.RecursiveSaveDataGridContextsState( rootDataGridContext );
    }

    protected override void RestoreStateCore( IDataGridContextVisitable dataGridContextVisitable )
    {
      DataGridContext rootDataGridContext = SaveRestoreStateVisitor.GetDataGridContextFromVisitable( dataGridContextVisitable );

      this.RecursiveRestoreDataGridContextsState( rootDataGridContext );
    }

    #endregion PROTECTED METHODS

    #region PRIVATE METHODS

    private void RecursiveSaveDataGridContextsState( DataGridContext dataGridContext )
    {
      SaveRestoreDataGridContextStateVisitor saveRestoreDataGridContextStateVisitor
        = new SaveRestoreDataGridContextStateVisitor( true, int.MaxValue, m_stopAtFirstCollapsedGroup );

      saveRestoreDataGridContextStateVisitor.SaveState( dataGridContext as IDataGridContextVisitable );

      m_dataGridContextsStateDictionary.Add( new WeakDataGridContextKey( dataGridContext ), saveRestoreDataGridContextStateVisitor );

      IEnumerable<DataGridContext> subDataGridContexts = dataGridContext.GetChildContexts();

      foreach( DataGridContext subDataGridContext in subDataGridContexts )
      {
        this.RecursiveSaveDataGridContextsState( subDataGridContext );
      }
    }

    private void RecursiveRestoreDataGridContextsState( DataGridContext dataGridContext )
    {
      WeakDataGridContextKey weakDataGridContextKey = new WeakDataGridContextKey( dataGridContext );

      SaveRestoreDataGridContextStateVisitor saveRestoreDataGridContextStateVisitor;

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

      IEnumerable<DataGridContext> subDataGridContexts = dataGridContext.GetChildContexts();
      foreach( DataGridContext subDataGridContext in subDataGridContexts )
      {
        this.RecursiveRestoreDataGridContextsState( subDataGridContext );
      }
    }

    #endregion PRIVATE METHODS

    #region PRIVATE FIELDS

    Dictionary<WeakDataGridContextKey, SaveRestoreDataGridContextStateVisitor> m_dataGridContextsStateDictionary;

    #endregion PRIVATE FIELDS
  }
}
