﻿/************************************************************************

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

namespace Xceed.Wpf.DataGrid
{
  /// <summary>
  /// The priority value of an expression operator determining the order in which the
  /// operator will be parsed. A smaller value means a greater priority. The operator
  /// having an impact on the interpretation of its surrounding operators should have
  /// a greater priority than those that do not. This type should not be confused with 
  /// the FilterOperatorPrecedence enum.
  /// </summary>
  internal enum FilterTokenPriority
  {
    /// <summary>
    /// 0: And operator
    /// </summary>
    AndOperator,
    /// <summary>
    /// 1: Or operator
    /// </summary>
    OrOperator,
    /// <summary>
    /// 2: Not operator
    /// </summary>
    NotOperator,
    /// <summary>
    /// 3: RelationalOperator (=, &gt;, &lt;, ...)
    /// </summary>
    RelationalOperator,
    /// <summary>
    /// 4: Default priority
    /// </summary>
    Default
  }
}
