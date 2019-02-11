/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using Xceed.Wpf.DataGrid;

namespace Xceed.Utils.Wpf.DragDrop
{
  internal struct DropTargetInfo
  {
    public DropTargetInfo( IDropTarget target, RelativePoint? position, bool canDrop )
    {
      this.Target = target;
      this.Position = position;
      this.CanDrop = canDrop;
    }

    internal readonly IDropTarget Target;
    internal readonly RelativePoint? Position;
    internal readonly bool CanDrop;
  }
}
