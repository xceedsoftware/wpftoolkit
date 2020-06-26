/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

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
