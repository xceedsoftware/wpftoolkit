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

namespace Xceed.Wpf.DataGrid
{
  public class GroupNameCountPair
  {
    public GroupNameCountPair( object name, int itemCount )
    {
      if( name == null )
        throw new ArgumentNullException( "name" );

      if( itemCount < 0 )
        throw new ArgumentOutOfRangeException( "itemCount", "The specified item count must be greater than or equal to zero." );

      this.Name = name;
      this.ItemCount = itemCount;
    }

    public object Name { get; private set; }
    public int ItemCount { get; private set; }
  }
}
