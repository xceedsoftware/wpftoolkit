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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Samples.Infrastructure.Commands
{
  public class TreeViewSelected
  {
    private static readonly DependencyProperty SelectedCommandBehaviorProperty = DependencyProperty.RegisterAttached( "SelectedCommandBehavior", typeof( TreeViewCommandBehavior ), typeof( TreeViewSelected ), null );

    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached( "Command", typeof( ICommand ), typeof( TreeViewSelected ), new PropertyMetadata( OnSetCommandCallback ) );
    public static void SetCommand( TreeView menuItem, ICommand command )
    {
      menuItem.SetValue( CommandProperty, command );
    }
    public static ICommand GetCommand( TreeView menuItem )
    {
      return menuItem.GetValue( CommandProperty ) as ICommand;
    }

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached( "CommandParameter", typeof( object ), typeof( TreeViewSelected ), new PropertyMetadata( OnSetCommandParameterCallback ) );
    public static void SetCommandParameter( TreeView menuItem, object parameter )
    {
      menuItem.SetValue( CommandParameterProperty, parameter );
    }
    public static object GetCommandParameter( TreeView menuItem )
    {
      return menuItem.GetValue( CommandParameterProperty );
    }

    private static void OnSetCommandCallback( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
    {
      TreeView menuItem = dependencyObject as TreeView;
      if( menuItem != null )
      {
        TreeViewCommandBehavior behavior = GetOrCreateBehavior( menuItem );
        behavior.Command = e.NewValue as ICommand;
      }
    }

    private static void OnSetCommandParameterCallback( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
    {
      TreeView menuItem = dependencyObject as TreeView;
      if( menuItem != null )
      {
        TreeViewCommandBehavior behavior = GetOrCreateBehavior( menuItem );
        behavior.CommandParameter = e.NewValue;
      }
    }

    private static TreeViewCommandBehavior GetOrCreateBehavior( TreeView menuItem )
    {
      TreeViewCommandBehavior behavior = menuItem.GetValue( SelectedCommandBehaviorProperty ) as TreeViewCommandBehavior;
      if( behavior == null )
      {
        behavior = new TreeViewCommandBehavior( menuItem );
        menuItem.SetValue( SelectedCommandBehaviorProperty, behavior );
      }

      return behavior;
    }
  }
}
