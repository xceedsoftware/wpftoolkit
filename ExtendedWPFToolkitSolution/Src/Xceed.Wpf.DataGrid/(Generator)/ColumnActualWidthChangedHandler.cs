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
using System.Linq;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  internal class ColumnActualWidthChangedEventArgs : EventArgs
  {
    public ColumnActualWidthChangedEventArgs( ColumnBase column , double oldValue, double newValue )
    {
      m_column = column;
      m_oldValue = oldValue;
      m_newValue = newValue;
    }

    public ColumnBase Column
    {
      get
      {
        return m_column;
      }
    }

    public double NewValue
    {
      get
      {
        return m_newValue;
      }
    }

    public double OldValue
    {
      get
      {
        return m_oldValue;
      }
    }

    private ColumnBase m_column;
    private double m_oldValue;
    private double m_newValue;
  }

  internal delegate void ColumnActualWidthChangedHandler( object sender, ColumnActualWidthChangedEventArgs e );
}
