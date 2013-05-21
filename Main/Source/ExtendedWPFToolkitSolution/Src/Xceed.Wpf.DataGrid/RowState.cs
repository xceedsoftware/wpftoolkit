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
using System.Collections;
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid
{
  internal class RowState
  {
    public RowState()
    {
    }

    // Restore it only if the Row is being edited
    public RowValidationError ItemValidationError
    {
      get
      {
        return m_itemValidationError;
      }
    }

    // Restore it only if the Row is being edited
    public bool IsDirty
    {
      get
      {
        return m_isDirty;
      }
    }

    public RowState Clone()
    {
      RowState rowState = new RowState();

      rowState.m_itemValidationError = m_itemValidationError;
      rowState.m_isDirty = m_isDirty;

      return rowState;
    }

    internal void SetItemValidationError( RowValidationError value )
    {
      m_itemValidationError = value;
    }

    internal void SetIsDirty( bool value )
    {
      m_isDirty = value;
    }

    private RowValidationError m_itemValidationError; // = null;
    private bool m_isDirty; // = false
  }
}
