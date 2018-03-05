/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;
using System.Windows.Controls.Primitives;

namespace Xceed.Wpf.DataGrid
{
  public interface ICustomItemContainerGenerator : IItemContainerGenerator
  {
    int ItemCount
    {
      get;
    }

    bool IsRecyclingEnabled
    {
      get;
      set;
    }

    DependencyObject ContainerFromIndex( int index );
    DependencyObject GetRealizedContainerForIndex( int index );
    int GetRealizedIndexForContainer( DependencyObject container );

    void SetCurrentIndex( int newCurrentIndex );
    int GetCurrentIndex();

    void RestoreFocus( DependencyObject container );
  }
}
