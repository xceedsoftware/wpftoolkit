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
