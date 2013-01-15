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
using Xceed.Wpf.DataGrid.ValidationRules;
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid
{
  public class CellValidationError
  {
    public CellValidationError( 
      CellValidationRule ruleInError, 
      Cell cellInError, 
      object errorContent, 
      Exception exception )
    {
      m_ruleInError = ruleInError;
      m_cellInError = cellInError;
      m_errorContent = errorContent;
      m_exception = exception;
    }

    public CellValidationRule RuleInError
    {
      get
      {
        return m_ruleInError;
      }
    }

    public Cell CellInError
    {
      get
      {
        return m_cellInError;
      }
    }

    public object ErrorContent
    {
      get
      {
        return m_errorContent;
      }
      set
      {
        m_errorContent = value;
      }
    }

    public Exception Exception
    {
      get
      {
        return m_exception;
      }
    }

    private CellValidationRule m_ruleInError;
    private Cell m_cellInError;
    private object m_errorContent;
    private Exception m_exception;
  }
}
