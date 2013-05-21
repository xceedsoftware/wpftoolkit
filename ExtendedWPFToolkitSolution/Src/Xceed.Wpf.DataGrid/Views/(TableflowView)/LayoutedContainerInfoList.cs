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

namespace Xceed.Wpf.DataGrid.Views
{
  internal class LayoutedContainerInfoList : List<LayoutedContainerInfo>
  {
    public bool ContainsContainer( UIElement container )
    {
      return ( this.IndexOfContainer( container ) > -1 );
    }

    public int IndexOfContainer( UIElement container )
    {
      int count = this.Count;

      for( int i = 0; i < count; i++ )
      {
        if( this[ i ].Container == container )
          return i;
      }

      return -1;
    }

    public bool ContainsRealizedIndex( int realizedIndex )
    {
      return ( this.IndexOfRealizedIndex( realizedIndex ) > -1 );
    }

    public int IndexOfRealizedIndex( int realizedIndex )
    {
      int count = this.Count;

      for( int i = 0; i < count; i++ )
      {
        if( this[ i ].RealizedIndex == realizedIndex )
          return i;
      }

      return -1;
    }

    public LayoutedContainerInfo this[ UIElement container ]
    {
      get
      {
        int index = this.IndexOfContainer( container );

        if( index == -1 )
          return null;

        return this[ index ];
      }
    }
  }
}
