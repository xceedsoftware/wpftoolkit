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

namespace Xceed.Wpf.DataGrid.Stats
{
  internal class StatResult
  {
    public StatResult( object value )
    {
      m_value = value;
    }

    private object m_value;

    public object Value
    {
      get
      {
        return m_value;
      }
    }
  }
}
