/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

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
