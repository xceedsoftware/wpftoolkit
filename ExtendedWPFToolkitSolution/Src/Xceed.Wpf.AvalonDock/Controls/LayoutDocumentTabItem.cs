/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class LayoutDocumentTabItem : Control
  {
    #region Members

    private List<Rect> _otherTabsScreenArea = null;
    private List<TabItem> _otherTabs = null;
    private Rect _parentDocumentTabPanelScreenArea;
    private DocumentPaneTabPanel _parentDocumentTabPanel;
    private bool _isMouseDown = false;
    private Point _mouseDownPoint;

    #endregion

    #region Contructors

    static LayoutDocumentTabItem()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( LayoutDocumentTabItem ), new FrameworkPropertyMetadata( typeof( LayoutDocumentTabItem ) ) );
    }

    public LayoutDocumentTabItem()
    {
    }

    #endregion

    #region Properties

    #region Model

    /// <summary>
    /// Model Dependency Property
    /// </summary>
    public static readonly DependencyProperty ModelProperty =  DependencyProperty.Register( "Model", typeof( LayoutContent ), typeof( LayoutDocumentTabItem ),
            new FrameworkPropertyMetadata( ( LayoutContent )null, new PropertyChangedCallback( OnModelChanged ) ) );

    /// <summary>
    /// Gets or sets the Model property.  This dependency property 
    /// indicates the layout content model attached to the tab item.
    /// </summary>
    public LayoutContent Model
    {
      get
      {
        return ( LayoutContent )GetValue( ModelProperty );
      }
      set
      {
        SetValue( ModelProperty, value );
      }
    }

    /// <summary>
    /// Handles changes to the Model property.
    /// </summary>
    private static void OnModelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( LayoutDocumentTabItem )d ).OnModelChanged( e );
    }


    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the Model property.
    /// </summary>
    protected virtual void OnModelChanged( DependencyPropertyChangedEventArgs e )
    {
      if( Model != null )
        SetLayoutItem( Model.Root.Manager.GetLayoutItemFromModel( Model ) );
      else
        SetLayoutItem( null );
      //UpdateLogicalParent();
    }

    #endregion

    #region LayoutItem

    /// <summary>
    /// LayoutItem Read-Only Dependency Property
    /// </summary>
    private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly( "LayoutItem", typeof( LayoutItem ), typeof( LayoutDocumentTabItem ),
            new FrameworkPropertyMetadata( ( LayoutItem )null ) );

    public static readonly DependencyProperty LayoutItemProperty = LayoutItemPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the LayoutItem property.  This dependency property 
    /// indicates the LayoutItem attached to this tag item.
    /// </summary>
    public LayoutItem LayoutItem
    {
      get
      {
        return ( LayoutItem )GetValue( LayoutItemProperty );
      }
    }

    /// <summary>
    /// Provides a secure method for setting the LayoutItem property.  
    /// This dependency property indicates the LayoutItem attached to this tag item.
    /// </summary>
    /// <param name="value">The new value for the property.</param>
    protected void SetLayoutItem( LayoutItem value )
    {
      SetValue( LayoutItemPropertyKey, value );
    }

    #endregion

    #endregion

    #region Overrides

    protected override void OnMouseLeftButtonDown( System.Windows.Input.MouseButtonEventArgs e )
    {
      base.OnMouseLeftButtonDown( e );

      Model.IsActive = true;

      var layoutDocument = Model as LayoutDocument;
      if( ( layoutDocument != null ) && !layoutDocument.CanMove )
        return;

      if( e.ClickCount == 1 )
      {
        _mouseDownPoint = e.GetPosition( this );
        _isMouseDown = true;
      }
    }

    protected override void OnMouseMove( System.Windows.Input.MouseEventArgs e )
    {
      base.OnMouseMove( e );

      if( _isMouseDown )
      {
        Point ptMouseMove = e.GetPosition( this );

        if( Math.Abs( ptMouseMove.X - _mouseDownPoint.X ) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs( ptMouseMove.Y - _mouseDownPoint.Y ) > SystemParameters.MinimumVerticalDragDistance )
        {
          this.UpdateDragDetails();
          this.CaptureMouse();
          _isMouseDown = false;
        }
      }

      if( this.IsMouseCaptured )
      {
        var mousePosInScreenCoord = this.PointToScreenDPI( e.GetPosition( this ) );
        if( !_parentDocumentTabPanelScreenArea.Contains( mousePosInScreenCoord ) )
        {
          this.StartDraggingFloatingWindowForContent();
        }
        else
        {
          int indexOfTabItemWithMouseOver = _otherTabsScreenArea.FindIndex( r => r.Contains( mousePosInScreenCoord ) );
          if( indexOfTabItemWithMouseOver >= 0 )
          {
            var targetModel = _otherTabs[ indexOfTabItemWithMouseOver ].Content as LayoutContent;
            var container = this.Model.Parent as ILayoutContainer;
            var containerPane = this.Model.Parent as ILayoutPane;

            if( ( containerPane is LayoutDocumentPane ) && !( ( LayoutDocumentPane )containerPane ).CanRepositionItems )
              return;
            if( ( containerPane.Parent != null ) && ( containerPane.Parent is LayoutDocumentPaneGroup ) && !( ( LayoutDocumentPaneGroup )containerPane.Parent ).CanRepositionItems )
              return;

            var childrenList = container.Children.ToList();
            containerPane.MoveChild( childrenList.IndexOf( Model ), childrenList.IndexOf( targetModel ) );
            this.Model.IsActive = true;
            _parentDocumentTabPanel.UpdateLayout();
            this.UpdateDragDetails();
          }
        }
      }
    }

    protected override void OnMouseLeftButtonUp( System.Windows.Input.MouseButtonEventArgs e )
    {
      if( IsMouseCaptured )
        ReleaseMouseCapture();
      _isMouseDown = false;

      base.OnMouseLeftButtonUp( e );
    }

    protected override void OnMouseLeave( System.Windows.Input.MouseEventArgs e )
    {
      base.OnMouseLeave( e );
      _isMouseDown = false;
    }

    protected override void OnMouseEnter( MouseEventArgs e )
    {
      base.OnMouseEnter( e );
      _isMouseDown = false;
    }

    protected override void OnMouseDown( MouseButtonEventArgs e )
    {
      if( e.ChangedButton == MouseButton.Middle )
      {
        if( LayoutItem.CloseCommand.CanExecute( null ) )
          LayoutItem.CloseCommand.Execute( null );
      }

      base.OnMouseDown( e );
    }

    #endregion

    #region Private Methods

    private void UpdateDragDetails()
    {
      _parentDocumentTabPanel = this.FindLogicalAncestor<DocumentPaneTabPanel>();
      _parentDocumentTabPanelScreenArea = _parentDocumentTabPanel.GetScreenArea();
      _otherTabs = _parentDocumentTabPanel.Children.Cast<TabItem>().Where( ch =>
           ch.Visibility != System.Windows.Visibility.Collapsed ).ToList();
      Rect currentTabScreenArea = this.FindLogicalAncestor<TabItem>().GetScreenArea();
      _otherTabsScreenArea = _otherTabs.Select( ti =>
      {
        var screenArea = ti.GetScreenArea();
        return new Rect( screenArea.Left, screenArea.Top, currentTabScreenArea.Width, screenArea.Height );
      } ).ToList();
    }

    private void StartDraggingFloatingWindowForContent()
    {
      this.ReleaseMouseCapture();

      if( this.Model is LayoutAnchorable )
      {
        ( ( LayoutAnchorable )this.Model ).ResetCanCloseInternal();
      }
      var manager = this.Model.Root.Manager;
      manager.StartDraggingFloatingWindowForContent( this.Model );
    }

    #endregion
  }
}
