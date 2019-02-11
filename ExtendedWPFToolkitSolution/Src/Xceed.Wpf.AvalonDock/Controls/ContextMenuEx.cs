/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows.Controls;
using System.Windows.Data;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class ContextMenuEx : ContextMenu
  {
    #region Constructors

    static ContextMenuEx()
    {
    }

    public ContextMenuEx()
    {
    }

    #endregion

    #region Overrides

    protected override System.Windows.DependencyObject GetContainerForItemOverride()
    {
      return new MenuItemEx();
    }

    protected override void OnOpened( System.Windows.RoutedEventArgs e )
    {
      BindingOperations.GetBindingExpression( this, ItemsSourceProperty ).UpdateTarget();

      base.OnOpened( e );
    }

    #endregion
  }
}
