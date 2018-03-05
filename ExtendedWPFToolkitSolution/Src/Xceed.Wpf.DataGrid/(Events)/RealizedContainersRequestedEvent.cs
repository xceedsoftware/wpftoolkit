/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  internal class RealizedContainersRequestedEventArgs: EventArgs
  {
    public List<object> RealizedContainers
    {
      get
      {
        return m_realizedContainers;
      }
    }

    private List<object> m_realizedContainers = new List<object>();
  }

  internal delegate void RealizedContainersRequestedEventHandler( object sender, RealizedContainersRequestedEventArgs e );
}
