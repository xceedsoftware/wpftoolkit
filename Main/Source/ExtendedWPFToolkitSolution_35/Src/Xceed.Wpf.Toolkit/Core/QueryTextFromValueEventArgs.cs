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
using System.Collections.Generic;
using System.Text;

namespace Xceed.Wpf.Toolkit.Core
{
  public class QueryTextFromValueEventArgs : EventArgs
  {
    public QueryTextFromValueEventArgs( object value, string text )
    {
      m_value = value;
      m_text = text;
    }

    #region Value Property

    private object m_value;

    public object Value
    {
      get { return m_value; }
    }

    #endregion Value Property

    #region Text Property

    private string m_text;

    public string Text
    {
      get { return m_text; }
      set { m_text = value; }
    }

    #endregion Text Property
  }
}
