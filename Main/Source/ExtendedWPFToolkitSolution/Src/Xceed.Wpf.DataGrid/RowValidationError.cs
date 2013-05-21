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
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid
{
  public class RowValidationError
  {
    public RowValidationError(
      ValidationRule ruleInError,
      Row rowInError,
      object errorContent,
      Exception exception )
    {
      m_ruleInError = ruleInError;
      m_rowInError = rowInError;
      m_errorContent = errorContent;
      m_exception = exception;
    }

    public ValidationRule RuleInError
    {
      get
      {
        return m_ruleInError;
      }
    }

    public Row RowInError
    {
      get
      {
        return m_rowInError;
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

    private ValidationRule m_ruleInError;
    private Row m_rowInError;
    private object m_errorContent;
    private Exception m_exception;
  }
}
