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

namespace Xceed.Wpf.Toolkit.Primitives
{
  public class SelectorItem : ContentControl
  {
    #region Constructors

    static SelectorItem()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( SelectorItem ), new FrameworkPropertyMetadata( typeof( SelectorItem ) ) );
    }

    public SelectorItem()
    {
      AddHandler( Mouse.MouseDownEvent, new MouseButtonEventHandler( OnMouseDown ) );
    }

    #endregion //Constructors

    #region Properties

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register( "IsSelected", typeof( bool ), typeof( SelectorItem ), new UIPropertyMetadata( false, OnIsSelectedChanged ) );
    public bool IsSelected
    {
      get
      {
        return ( bool )GetValue( IsSelectedProperty );
      }
      set
      {
        SetValue( IsSelectedProperty, value );
      }
    }

    private static void OnIsSelectedChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      SelectorItem selectorItem = o as SelectorItem;
      if( selectorItem != null )
        selectorItem.OnIsSelectedChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnIsSelectedChanged( bool oldValue, bool newValue )
    {
      if( newValue )
        this.RaiseEvent( new RoutedEventArgs( Selector.SelectedEvent, this ) );
      else
        this.RaiseEvent( new RoutedEventArgs( Selector.UnSelectedEvent, this ) );
    }

    internal Selector ParentSelector
    {
      get
      {
        return ItemsControl.ItemsControlFromItemContainer( this ) as Selector;
      }
    }

    #endregion //Properties

    #region Events

    public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner( typeof( SelectorItem ) );
    public static readonly RoutedEvent UnselectedEvent = Selector.UnSelectedEvent.AddOwner( typeof( SelectorItem ) );

    #endregion

    #region Event Hanlders

    void OnMouseDown( object sender, MouseButtonEventArgs e )
    {
      IsSelected = !IsSelected;
    }

    #endregion //Event Hanlders

  }
}
