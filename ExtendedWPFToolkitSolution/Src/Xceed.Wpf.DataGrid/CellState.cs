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
using System.Windows.Controls;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal class CellState
  {
    public CellState()
    {
      m_contentBeforeRowEdition = DependencyProperty.UnsetValue;
      m_contentBeforeCellEdition = DependencyProperty.UnsetValue;
    }

    public CellValidationError CellValidationError
    {
      get
      {
        return m_cellValidationError;
      }
    }

    public bool IsBeingEdited
    {
      get
      {
        return m_isBeingEdited;
      }
    }

    public bool IsDirty
    {
      get
      {
        return m_isDirty;
      }
    }

    #region ContentBeforeRowEdition Read-Only Property

    public object ContentBeforeRowEdition
    {
      get { return m_contentBeforeRowEdition; }
    }

    internal void SetContentBeforeRowEdition( object value )
    {
      m_contentBeforeRowEdition = value;
    }

    private object m_contentBeforeRowEdition = DependencyProperty.UnsetValue;

    #endregion ContentBeforeRowEdition Read-Only Property

    #region ContentBeforeCellEdition Read-Only Property

    public object ContentBeforeCellEdition
    {
      get { return m_contentBeforeCellEdition; }
    }

    internal void SetContentBeforeCellEdition( object value )
    {
      m_contentBeforeCellEdition = value;
    }

    private object m_contentBeforeCellEdition = DependencyProperty.UnsetValue;

    #endregion ContentBeforeCellEdition Read-Only Property

    #region Content

    public object Content
    {
      get { return m_content; }
    }

    internal void SetContent( object value )
    {
      m_content = value;
    }

    private object m_content = DependencyProperty.UnsetValue;

    #endregion Content

    #region IsDirtyBeforeEdition Property

    public Nullable<bool> IsDirtyBeforeEdition 
    {
      get
      {
        return m_isDirtyBeforeEdition;
      }
    }

    internal void SetIsDirtyBeforeEdition( Nullable<bool> value )
    {
      m_isDirtyBeforeEdition = value;
    }

    private Nullable<bool> m_isDirtyBeforeEdition;

    #endregion IsDirtyBeforeEdition Property

    public CellState Clone()
    {
      CellState cellState = new CellState();

      cellState.m_content = m_content;
      cellState.m_contentBeforeRowEdition = m_contentBeforeRowEdition;
      cellState.m_contentBeforeCellEdition = m_contentBeforeCellEdition;
      cellState.m_isDirtyBeforeEdition = m_isDirtyBeforeEdition;

      cellState.m_cellValidationError = m_cellValidationError;
      cellState.m_isBeingEdited = m_isBeingEdited;
      cellState.m_isDirty = m_isDirty;

      return cellState;
    }

    internal void SetCellValidationError( CellValidationError value )
    {
      m_cellValidationError = value;
    }

    internal void SetIsBeingEdited( bool value )
    {
      m_isBeingEdited = value;
    }

    internal void SetIsDirty( bool value )
    {
      m_isDirty = value;
    }

    private CellValidationError m_cellValidationError; // = null
    private bool m_isBeingEdited; // = false
    private bool m_isDirty; // = false
  }
}
