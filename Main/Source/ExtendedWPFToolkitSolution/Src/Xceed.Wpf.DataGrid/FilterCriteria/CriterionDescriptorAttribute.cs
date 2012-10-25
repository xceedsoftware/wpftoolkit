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

namespace Xceed.Wpf.DataGrid.FilterCriteria
{
  [ AttributeUsage( AttributeTargets.Class, Inherited = true, AllowMultiple = false ) ]
  internal sealed class CriterionDescriptorAttribute : Attribute
  {
    public CriterionDescriptorAttribute( string pattern, FilterOperatorPrecedence operatorPrecedence, FilterTokenPriority parserPriority )
    {
      m_pattern = pattern;
      m_operatorPrecedence = operatorPrecedence;
      m_parserPriority = parserPriority;
    }

    public string Pattern
    {
      get
      {
        return m_pattern;
      }
    }

    public FilterOperatorPrecedence OperatorPrecedence
    {
      get
      {
        return m_operatorPrecedence;
      }
    }

    public FilterTokenPriority ParserPriority
    {
      get
      {
        return m_parserPriority;
      }
    }

    private readonly string m_pattern;
    private readonly FilterOperatorPrecedence m_operatorPrecedence;
    private readonly FilterTokenPriority m_parserPriority;
  }
}
