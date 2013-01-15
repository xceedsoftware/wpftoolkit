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
using System.Collections.Generic;
using System.Text;

namespace Xceed.Wpf.Toolkit.Core
{
  public class QueryValueFromTextEventArgs : EventArgs
  {
    public QueryValueFromTextEventArgs( string text, object value )
    {
      m_text = text;
      m_value = value;
    }

    #region Text Property

    private string m_text;

    public string Text
    {
      get { return m_text; }
    }

    #endregion Text Property

    #region Value Property

    private object m_value;

    public object Value
    {
      get { return m_value; }
      set { m_value = value; }
    }

    #endregion Value Property

    #region HasParsingError Property

    private bool m_hasParsingError;

    public bool HasParsingError
    {
      get { return m_hasParsingError; }
      set { m_hasParsingError = value; }
    }

    #endregion HasParsingError Property

  }
}
