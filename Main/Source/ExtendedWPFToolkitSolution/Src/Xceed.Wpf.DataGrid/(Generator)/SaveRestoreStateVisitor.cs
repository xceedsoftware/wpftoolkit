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
using System.Text;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  abstract internal class SaveRestoreStateVisitor : IDataGridContextVisitor
  {
    #region STATIC MEMBERS

    public static DataGridContext GetDataGridContextFromVisitable( IDataGridContextVisitable dataGridContextVisitable )
    {
      DataGridContext dataGridContext = dataGridContextVisitable as DataGridContext;

      if( dataGridContext == null )
      {
        DataGridControl dataGridControl = dataGridContextVisitable as DataGridControl;

        if( dataGridControl != null )
          dataGridContext = dataGridControl.DataGridContext;
      }

      if( dataGridContext == null )
        throw new InvalidOperationException( "A dataGridContext could not be extracted from the IDataGridContextVisitable object." );

      return dataGridContext;
    }

    #endregion STATIC MEMBERS

    #region CONSTRUCTORS

    public SaveRestoreStateVisitor()
    {
      this.Initialize();
    }

    #endregion CONSTRUCTORS

    #region SaveRestoreStateVisitorStatus

    private SaveRestoreStateVisitorStatus m_status;

    public SaveRestoreStateVisitorStatus SaveRestoreStateVisitorStatus
    {
      get { return m_status; }
    }

    #endregion SaveRestoreStateVisitorStatus

    #region PUBLIC METHODS

    public void Initialize()
    {
      this.InitializeCore();
      m_status = SaveRestoreStateVisitorStatus.Ready;
    }

    public void SaveState( IDataGridContextVisitable dataGridContextVisitable )
    {
      if( m_status != SaveRestoreStateVisitorStatus.Ready )
        throw new InvalidOperationException( "An attempt was made to save the state of a visitor that is not ready." );

      m_status = SaveRestoreStateVisitorStatus.Saving;
      try
      {
        this.SaveStateCore( dataGridContextVisitable );
        m_status = SaveRestoreStateVisitorStatus.RestorePending;
      }
      catch
      {
        m_status = SaveRestoreStateVisitorStatus.Error;
        throw;
      }
    }

    public void RestoreState( IDataGridContextVisitable dataGridContextVisitable )
    {
      if( m_status != SaveRestoreStateVisitorStatus.RestorePending )
        throw new InvalidOperationException( "An attempt was made to restore the state of a visitor that does not have a pending restorable state." );

      m_status = SaveRestoreStateVisitorStatus.Restoring;
      try
      {
        this.RestoreStateCore( dataGridContextVisitable );
        m_status = SaveRestoreStateVisitorStatus.Restored;
      }
      catch
      {
        m_status = SaveRestoreStateVisitorStatus.Error;
        throw;
      }
    }

    #endregion PUBLIC METHODS

    #region PROTECTED METHODS

    protected abstract void SaveStateCore( IDataGridContextVisitable dataGridContextVisitable );

    protected abstract void RestoreStateCore( IDataGridContextVisitable dataGridContextVisitable );

    protected abstract void InitializeCore();

    protected virtual void SavingVisit( DataGridContext sourceContext )
    {
      throw new NotImplementedException();
    }

    protected virtual void RestoringVisit( DataGridContext sourceContext )
    {
      throw new NotImplementedException();
    }

    protected virtual void SavingVisit( DataGridContext sourceContext, object item )
    {
      throw new NotImplementedException();
    }

    protected virtual void RestoringVisit( DataGridContext sourceContext, object item )
    {
      throw new NotImplementedException();
    }

    protected virtual void SavingVisit( DataGridContext sourceContext, 
      CollectionViewGroup group, 
      object[] namesTree, 
      int groupLevel, 
      bool isExpanded, 
      bool isComputedExpanded )
    {
      throw new NotImplementedException();
    }

    protected virtual void RestoringVisit( DataGridContext sourceContext,
      CollectionViewGroup group,
      object[] namesTree,
      int groupLevel,
      bool isExpanded,
      bool isComputedExpanded )
    {
      throw new NotImplementedException();
    }

    protected virtual void SavingVisit( DataGridContext sourceContext, System.Windows.DataTemplate headerFooter )
    {
      throw new NotImplementedException();
    }

    protected virtual void RestoringVisit( DataGridContext sourceContext, System.Windows.DataTemplate headerFooter )
    {
      throw new NotImplementedException();
    }

    protected virtual void SavingVisit( DataGridContext sourceContext, GroupHeaderFooterItem groupHeaderFooter )
    {
      throw new NotImplementedException();
    }

    protected virtual void RestoringVisit( DataGridContext sourceContext, GroupHeaderFooterItem groupHeaderFooter )
    {
      throw new NotImplementedException();
    }

    #endregion PROTECTED METHODS

    #region IDataGridContextVisitor Members

    void IDataGridContextVisitor.Visit( DataGridContext sourceContext, ref bool stopVisit )
    {
      switch( m_status )
      {
        case SaveRestoreStateVisitorStatus.Saving:
          {
            this.SavingVisit( sourceContext );
            break;
          }

        case SaveRestoreStateVisitorStatus.Restoring:
          {
            this.RestoringVisit( sourceContext );
            break;
          }

        default:
          {
            throw new InvalidOperationException( "An attempt was made to visit using a method other than the SaveRestoreStateVisitor's Save and Restore methods." );
          }
      }
    }

    void IDataGridContextVisitor.Visit( DataGridContext sourceContext, int startSourceDataItemIndex, int endSourceDataItemIndex, ref bool stopVisit )
    {
      throw new NotSupportedException( "The SaveRestoreStateVisitor is not handling of data items block." );
    }

    void IDataGridContextVisitor.Visit( DataGridContext sourceContext, int sourceDataItemIndex, object item, ref bool stopVisit )
    {
      switch( m_status )
      {
        case SaveRestoreStateVisitorStatus.Saving:
          {
            this.SavingVisit( sourceContext, item );
            break;
          }

        case SaveRestoreStateVisitorStatus.Restoring:
          {
            this.RestoringVisit( sourceContext, item );
            break;
          }

        default:
          {
            throw new InvalidOperationException( "An attempt was made to visit using a method other than the SaveRestoreStateVisitor's Save and Restore methods." );
          }
      }
    }

    void IDataGridContextVisitor.Visit( DataGridContext sourceContext, System.Windows.Data.CollectionViewGroup group, object[] namesTree, int groupLevel, bool isExpanded, bool isComputedExpanded, ref bool stopVisit )
    {
      switch( m_status )
      {
        case SaveRestoreStateVisitorStatus.Saving:
          {
            this.SavingVisit( sourceContext, group, namesTree, groupLevel, isExpanded, isComputedExpanded );
            break;
          }

        case SaveRestoreStateVisitorStatus.Restoring:
          {
            this.RestoringVisit( sourceContext, group, namesTree, groupLevel, isExpanded, isComputedExpanded );
            break;
          }

        default:
          {
            throw new InvalidOperationException( "An attempt was made to visit using a method other than the SaveRestoreStateVisitor's Save and Restore methods." );
          }
      }
    }

    void IDataGridContextVisitor.Visit( DataGridContext sourceContext, System.Windows.DataTemplate headerFooter, ref bool stopVisit )
    {
      switch( m_status )
      {
        case SaveRestoreStateVisitorStatus.Saving:
          {
            this.SavingVisit( sourceContext, headerFooter );
            break;
          }

        case SaveRestoreStateVisitorStatus.Restoring:
          {
            this.RestoringVisit( sourceContext, headerFooter );
            break;
          }

        default:
          {
            throw new InvalidOperationException( "An attempt was made to visit using a method other than the SaveRestoreStateVisitor's Save and Restore methods." );
          }
      }
    }

    void IDataGridContextVisitor.Visit( DataGridContext sourceContext, GroupHeaderFooterItem groupHeaderFooter, ref bool stopVisit )
    {
      switch( m_status )
      {
        case SaveRestoreStateVisitorStatus.Saving:
          {
            this.SavingVisit( sourceContext, groupHeaderFooter );
            break;
          }

        case SaveRestoreStateVisitorStatus.Restoring:
          {
            this.RestoringVisit( sourceContext, groupHeaderFooter );
            break;
          }

        default:
          {
            throw new InvalidOperationException( "An attempt was made to visit using a method other than the SaveRestoreStateVisitor's Save and Restore methods." );
          }
      }
    }

    #endregion
  }

  internal enum SaveRestoreStateVisitorStatus
  {
    Ready = 0,
    Saving = 1,
    RestorePending = 2,
    Restoring = 3,
    Restored = 4,
    Error = 5
  }

}
