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
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Commands;

namespace Samples.Infrastructure.Commands
{
  public class TreeViewCommandBehavior : CommandBehaviorBase<TreeView>
  {
    public TreeViewCommandBehavior( TreeView treeView )
      : base( treeView )
    {
      treeView.SelectedItemChanged += SelectedItemChanged;
    }

    void SelectedItemChanged( object sender, RoutedPropertyChangedEventArgs<object> e )
    {
      //This treeview command is specfic to the navigation region, 
      //so I always want to pass the tag of the selected treeviewitem
      //because it will conatin the Type of view to navigate to.
      var type = ( e.NewValue as FrameworkElement ).Tag as Type;
      CommandParameter = type != null ? type.FullName : null;
      ExecuteCommand();
    }
  }
}
