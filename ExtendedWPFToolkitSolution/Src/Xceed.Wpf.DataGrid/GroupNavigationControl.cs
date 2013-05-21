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
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using Xceed.Wpf.DataGrid.Views;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  [TemplatePart( Name = "PART_Button", Type = typeof( Button ) )]
  [TemplatePart( Name = "PART_ToggleButton", Type = typeof( ToggleButton ) )]
  [TemplatePart( Name = "PART_Popup", Type = typeof( Popup ) )]
  [StyleTypedProperty( Property = "ItemContainerStyle", StyleTargetType = typeof( GroupNavigationControlItem ) )]
  public class GroupNavigationControl : ItemsControl
  {
    #region CONSTRUCTORS

    static GroupNavigationControl()
    {
      GroupNavigationControl.GroupProperty = GroupHeaderControl.GroupProperty.AddOwner( typeof( GroupNavigationControl ) );

      DefaultStyleKeyProperty.OverrideMetadata(
        typeof( GroupNavigationControl ),
        new FrameworkPropertyMetadata( new Markup.ThemeKey( typeof( TableView ), typeof( GroupNavigationControl ) ) ) );

      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata(
        typeof( GroupNavigationControl ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( GroupNavigationControl.OnParentGridControlChanged ) ) );

      EventManager.RegisterClassHandler(
        typeof( GroupNavigationControl ),
        Mouse.LostMouseCaptureEvent,
        new MouseEventHandler( GroupNavigationControl.OnLostMouseCapture ) );

      EventManager.RegisterClassHandler(
        typeof( GroupNavigationControl ),
        Mouse.PreviewMouseDownEvent,
        new MouseButtonEventHandler( GroupNavigationControl.OnPreviewMouseButtonDown ) );

      EventManager.RegisterClassHandler(
        typeof( GroupNavigationControl ),
        Mouse.MouseDownEvent,
        new MouseButtonEventHandler( GroupNavigationControl.OnMouseButtonDown ), true );

      EventManager.RegisterClassHandler(
        typeof( GroupNavigationControl ),
        Mouse.MouseWheelEvent,
        new MouseWheelEventHandler( GroupNavigationControl.OnMouseWheel ), true );
    }

    public GroupNavigationControl()
    {
    }

    #endregion CONSTRUCTORS

    #region Group Property

    public static readonly DependencyProperty GroupProperty;

    public Group Group
    {
      get
      {
        return GroupHeaderControl.GetGroup( this );
      }
      set
      {
        GroupHeaderControl.SetGroup( this, value );
      }
    }

    #endregion Group Property

    #region MainItemTemplate Property

    public static readonly DependencyProperty MainItemTemplateProperty = DependencyProperty.Register(
      "MainItemTemplate",
      typeof( DataTemplate ),
      typeof( GroupNavigationControl ) );

    public DataTemplate MainItemTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( GroupNavigationControl.MainItemTemplateProperty );
      }
      set
      {
        this.SetValue( GroupNavigationControl.MainItemTemplateProperty, value );
      }
    }

    #endregion MainItemTemplate Property

    #region MainItemTemplateSelector Property

    public static readonly DependencyProperty MainItemTemplateSelectorProperty = DependencyProperty.Register(
      "MainItemTemplateSelector",
      typeof( DataTemplateSelector ),
      typeof( GroupNavigationControl ) );

    public DataTemplateSelector MainItemTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )this.GetValue( GroupNavigationControl.MainItemTemplateSelectorProperty );
      }
      set
      {
        this.SetValue( GroupNavigationControl.MainItemTemplateSelectorProperty, value );
      }
    }

    #endregion MainItemTemplateSelector Property

    #region MaxDropDownHeight Property

    public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register(
      "MaxDropDownHeight",
      typeof( double ),
      typeof( GroupNavigationControl ),
      new FrameworkPropertyMetadata( SystemParameters.PrimaryScreenHeight / 3.0 ) );

    public double MaxDropDownHeight
    {
      get
      {
        return ( double )this.GetValue( GroupNavigationControl.MaxDropDownHeightProperty );
      }
      set
      {
        this.SetValue( GroupNavigationControl.MaxDropDownHeightProperty, value );
      }
    }

    #endregion MaxDropDownHeight Property

    #region IsDropDownOpen Property

    [Browsable( false )]
    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
      "IsDropDownOpen",
      typeof( bool ),
      typeof( GroupNavigationControl ),
      new FrameworkPropertyMetadata( new PropertyChangedCallback( GroupNavigationControl.OnIsDropDownOpenChanged ) ) );

    [Browsable( false )]
    public bool IsDropDownOpen
    {
      get
      {
        return ( bool )this.GetValue( GroupNavigationControl.IsDropDownOpenProperty );
      }
      set
      {
        this.SetValue( GroupNavigationControl.IsDropDownOpenProperty, value );
      }
    }

    private static void OnIsDropDownOpenChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      GroupNavigationControl groupNavigationControl = ( GroupNavigationControl )sender;

      if( ( bool )e.NewValue )
      {
        // To avoid lag while generating the control, we do 
        // a late binding of the items.
        if( groupNavigationControl.ItemsSource == null )
        {
          Group group = groupNavigationControl.DataContext as Group;
          if( group != null )
          {
            groupNavigationControl.ItemsSource = group.SiblingGroups;
          }
        }

        Mouse.Capture( groupNavigationControl, CaptureMode.SubTree );
      }
      else
      {
        groupNavigationControl.ItemsSource = null;

        if( groupNavigationControl.IsKeyboardFocusWithin )
        {
          groupNavigationControl.Focus();
        }

        if( Mouse.Captured == groupNavigationControl )
        {
          Mouse.Capture( null );
        }
      }
    }

    #endregion IsDropDownOpen Property

    #region PartButton Private Property

    private Button PartButton
    {
      get;
      set;
    }

    #endregion PartButton Private Property

    #region PartToggleButton Private Property

    private ToggleButton PartToggleButton
    {
      get;
      set;
    }

    #endregion PartToggleButton Private Property

    #region PartPopup Private Property

    private Popup PartPopup
    {
      get;
      set;
    }

    #endregion PartPopup Private Property

    #region EVENT HANDLERS

    private static void OnLostMouseCapture( object sender, MouseEventArgs e )
    {
      GroupNavigationControl groupNavigationControl = ( GroupNavigationControl )sender;

      DependencyObject originalSource = e.OriginalSource as DependencyObject;

      if( Mouse.Captured != groupNavigationControl )
      {
        if( e.OriginalSource == groupNavigationControl )
        {
          if( ( Mouse.Captured == null )
            && ( Xceed.Utils.Wpf.TreeHelper.IsDescendantOf( originalSource, groupNavigationControl ) ) )
          {
            groupNavigationControl.Close();
          }
        }
        else if( ( originalSource != null ) && ( Xceed.Utils.Wpf.TreeHelper.IsDescendantOf( originalSource, groupNavigationControl ) ) )
        {
          if( ( groupNavigationControl.IsDropDownOpen ) && ( Mouse.Captured == null ) )
          {
            Mouse.Capture( groupNavigationControl, CaptureMode.SubTree );
            e.Handled = true;
          }
        }
        else
        {
          groupNavigationControl.Close();
        }
      }
    }

    private static void OnPreviewMouseButtonDown( object sender, MouseButtonEventArgs e )
    {
      GroupNavigationControl groupNavigationControl = ( GroupNavigationControl )sender;
      Visual originalSource = e.OriginalSource as Visual;

      if( ( originalSource != null )
        && ( groupNavigationControl.PartButton != null )
        && ( groupNavigationControl.PartButton.IsAncestorOf( originalSource ) ) )
      {
        if( groupNavigationControl.IsDropDownOpen )
        {
          groupNavigationControl.Close();
        }
      }
    }

    private static void OnMouseButtonDown( object sender, MouseButtonEventArgs e )
    {
      GroupNavigationControl groupNavigationControl = ( GroupNavigationControl )sender;

      if( !groupNavigationControl.IsKeyboardFocusWithin )
      {
        groupNavigationControl.Focus();
      }

      e.Handled = true;

      if( ( Mouse.Captured == groupNavigationControl ) && ( e.OriginalSource == groupNavigationControl ) )
      {
        groupNavigationControl.Close();
      }
    }

    private static void OnMouseWheel( object sender, MouseWheelEventArgs e )
    {
      GroupNavigationControl groupNavigationControl = ( GroupNavigationControl )sender;

      if( groupNavigationControl.IsDropDownOpen )
      {
        // This is to avoid scrolling any other list if the drop down is open. 
        // If not set, the grid could scroll while leaving the drop down in view. 
        // The dorp down will then be left alone while the GroupNavigationControl 
        // might not be in view anymore.
        e.Handled = true;
      }
    }

    private static void OnParentGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      GroupNavigationControl groupNavigationControl = sender as GroupNavigationControl;

      if( groupNavigationControl == null )
        return;

      DataGridControl parentGridControl = e.NewValue as DataGridControl;

      if( parentGridControl == null )
        return;

      groupNavigationControl.PrepareDefaultStyleKey( parentGridControl.GetView() );
    }

    #endregion EVENT HANDLERS

    #region OVERRIDES

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      this.PartButton = this.GetTemplateChild( "PART_Button" ) as Button;
      this.PartToggleButton = this.GetTemplateChild( "PART_ToggleButton" ) as ToggleButton;
      this.PartPopup = this.GetTemplateChild( "PART_Popup" ) as Popup;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
      return new GroupNavigationControlItem( this );
    }

    protected override void PrepareContainerForItemOverride( DependencyObject element, object item )
    {
      base.PrepareContainerForItemOverride( element, item );

      element.SetValue( GroupHeaderControl.GroupProperty, item );
    }

    protected override void ClearContainerForItemOverride( DependencyObject element, object item )
    {
      element.ClearValue( GroupHeaderControl.GroupProperty );

      base.ClearContainerForItemOverride( element, item );
    }

    protected override void OnIsKeyboardFocusWithinChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnIsKeyboardFocusWithinChanged( e );

      if( ( this.IsDropDownOpen )
        && ( !base.IsKeyboardFocusWithin ) )
      {
        this.Close();
      }
    }

    #endregion OVERRIDES

    #region INTERNAL METHODS

    internal void NotifyGroupNavigationControlItemMouseDown( GroupNavigationControlItem groupNavigationControlItem )
    {
    }

    internal void NotifyGroupNavigationControlItemMouseUp( GroupNavigationControlItem groupNavigationControlItem )
    {
      Group group = groupNavigationControlItem.DataContext as Group;
      if( group != null )
      {
        GroupNavigationButton.NavigateToGroup.Execute( group, this );
      }

      this.Close();
    }

    #endregion INTERNAL METHODS

    #region PROTECTED METHODS

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( GroupNavigationControl ) );
    }

    #endregion PROTECTED METHODS

    #region PRIVATE METHODS

    private void Close()
    {
      if( this.IsDropDownOpen )
      {
        base.ClearValue( GroupNavigationControl.IsDropDownOpenProperty );

        if( this.IsDropDownOpen )
          this.IsDropDownOpen = false;
      }
    }

    #endregion PRIVATE METHODS
  }
}
