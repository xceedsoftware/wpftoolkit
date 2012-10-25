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
using Xceed.Utils.Collections;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class DistinctValuesRequestedEventArgs : EventArgs
  {
    public DistinctValuesRequestedEventArgs( string autoFilterColumnFieldName )
    {
      m_autoFilterColumnFieldName = autoFilterColumnFieldName;
    }

    public string AutoFilterColumnFieldName
    {
      get
      {
        return m_autoFilterColumnFieldName;
      }
    }

    public HashSet<object> DistinctValues
    {
      get
      {
        return m_distinctValues;
      }
    }

    private string m_autoFilterColumnFieldName;
    private HashSet<object> m_distinctValues = new HashSet<object>();
  }

  internal delegate void DistinctValuesRequestedEventHandler( object sender, DistinctValuesRequestedEventArgs e );
}
