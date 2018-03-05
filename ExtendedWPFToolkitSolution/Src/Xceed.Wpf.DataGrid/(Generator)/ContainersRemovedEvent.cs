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
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid
{
  internal delegate void ContainersRemovedEventHandler( object sender, ContainersRemovedEventArgs e );

  internal class ContainersRemovedEventArgs : EventArgs
  {
    public ContainersRemovedEventArgs( IList<DependencyObject> removedContainers )
    {
      if( removedContainers == null )
        throw new ArgumentNullException( "removedContainers" );

      m_removedContainers = removedContainers;
    }

    public IList<DependencyObject> RemovedContainers
    {
      get
      {
        return m_removedContainers;
      }
    }

    private IList<DependencyObject> m_removedContainers;
  }
}
