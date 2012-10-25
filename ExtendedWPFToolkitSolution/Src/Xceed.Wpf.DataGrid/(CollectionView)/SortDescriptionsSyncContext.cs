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

namespace Xceed.Wpf.DataGrid
{
  internal class SortDescriptionsSyncContext
  {
    //public bool IsInitializing
    //{
    //  get
    //  {
    //    return m_isInitializing;
    //  }
    //  set
    //  {
    //    m_isInitializing = value;
    //  }
    //}

    public bool ProcessingSortSynchronization
    {
      get
      {
        return m_processingSortSynchronization;
      }
      set
      {
        m_processingSortSynchronization = value;
      }
    }

    //public bool SynchronizeSortDelayed
    //{
    //  get
    //  {
    //    return m_synchronizeSortDelayed;
    //  }
    //  set
    //  {
    //    m_synchronizeSortDelayed = value;
    //  }
    //}

    private bool m_processingSortSynchronization;
    //private bool m_synchronizeSortDelayed;
    //private bool m_isInitializing;

  }
}
