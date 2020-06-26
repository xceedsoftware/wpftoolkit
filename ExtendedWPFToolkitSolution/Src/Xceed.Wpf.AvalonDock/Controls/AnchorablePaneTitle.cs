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

using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class AnchorablePaneTitle : Control
  {
    #region Members

    private bool _isMouseDown = false;

    #endregion

    #region Constructors

    static AnchorablePaneTitle()
    {
      IsHitTestVisibleProperty.OverrideMetadata( typeof( AnchorablePaneTitle ), new FrameworkPropertyMetadata( true ) );
      FocusableProperty.OverrideMetadata( typeof( AnchorablePaneTitle ), new FrameworkPropertyMetadata( false ) );
      DefaultStyleKeyProperty.OverrideMetadata( typeof( AnchorablePaneTitle ), new FrameworkPropertyMetadata( typeof( AnchorablePaneTitle ) ) );
    }

    public AnchorablePaneTitle()
    {
    }

    #endregion

    #region Properties

    #region Model

    /// <summary>
    /// Model Dependency Property
    /// </summary>
    public static readonly DependencyProperty ModelProperty =  DependencyProperty.Register( "Model", typeof( LayoutAnchorable ), typeof( AnchorablePaneTitle ),
            new FrameworkPropertyMetadata( ( LayoutAnchorable )null, new PropertyChangedCallback( _OnModelChanged ) ) );

    /// <summary>
    /// Gets or sets the Model property.  This dependency property 
    /// indicates model attached to this view.
    /// </summary>
    public LayoutAnchorable Model
    {
      get
      {
        return ( LayoutAnchorable )GetValue( ModelProperty );
      }
      set
      {
        SetValue( ModelProperty, value );
      }
    }

    private static void _OnModelChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ( ( AnchorablePaneTitle )sender ).OnModelChanged( e );
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the Model property.
    /// </summary>
    protected virtual void OnModelChanged( DependencyPropertyChangedEventArgs e )
    {
      if( Model != null )
      {
        this.SetLayoutItem( Model.Root.Manager.GetLayoutItemFromModel( Model ) );
      }
      else
      {
        this.SetLayoutItem( null );
      }
    }

    #endregion

    #region LayoutItem

    /// <summary>
    /// LayoutItem Read-Only Dependency Property
    /// </summary>
    private static readonly DependencyPropertyKey LayoutItemPropertyKey  = DependencyProperty.RegisterReadOnly( "LayoutItem", typeof( LayoutItem ), typeof( AnchorablePaneTitle ),
            new FrameworkPropertyMetadata( ( LayoutItem )null ) );

    public static readonly DependencyProperty LayoutItemProperty  = LayoutItemPropertyKey.DependencyProperty;

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
      this.SetValue( LayoutItemPropertyKey, value );
    }

    #endregion

    #endregion

    #region Overrides

    protected override void OnMouseMove( System.Windows.Input.MouseEventArgs e )
    {
      if( e.LeftButton != MouseButtonState.Pressed )
      {
        _isMouseDown = false;
      }

      base.OnMouseMove( e );
    }

    protected override void OnMouseLeave( System.Windows.Input.MouseEventArgs e )
    {
      base.OnMouseLeave( e );

      if( _isMouseDown && e.LeftButton == MouseButtonState.Pressed )
      {
        var pane = this.FindVisualAncestor<LayoutAnchorablePaneControl>();
        if( pane != null )
        {
          var paneModel = pane.Model as LayoutAnchorablePane;
          var manager = paneModel.Root.Manager;

          manager.StartDraggingFloatingWindowForPane( paneModel );
        }
        else
        {
          var model = this.Model;
          if( model != null )
          {
            var root = model.Root;
            if( root != null )
            {
              var manager = root.Manager;
              if( manager != null )
              {
                manager.StartDraggingFloatingWindowForContent( model );
              }
            }
          }
        }
      }

      _isMouseDown = false;
    }

    protected override void OnMouseLeftButtonDown( System.Windows.Input.MouseButtonEventArgs e )
    {
      base.OnMouseLeftButtonDown( e );

      if( !e.Handled )
      {
        bool attachFloatingWindow = false;
        var parentFloatingWindow = Model.FindParent<LayoutAnchorableFloatingWindow>();
        if( parentFloatingWindow != null )
        {
          attachFloatingWindow = parentFloatingWindow.Descendents().OfType<LayoutAnchorablePane>().Count() == 1;
        }

        if( attachFloatingWindow )
        {
          //the pane is hosted inside a floating window that contains only an anchorable pane so drag the floating window itself
          var floatingWndControl = Model.Root.Manager.FloatingWindows.Single( fwc => fwc.Model == parentFloatingWindow );
          floatingWndControl.AttachDrag( false );
        }
        else
          _isMouseDown = true;//normal drag
      }
    }

    protected override void OnMouseLeftButtonUp( System.Windows.Input.MouseButtonEventArgs e )
    {
      _isMouseDown = false;
      base.OnMouseLeftButtonUp( e );

      if( Model != null )
        Model.IsActive = true;//FocusElementManager.SetFocusOnLastElement(Model);
    }

    #endregion

    #region Private Methods

    private void OnHide()
    {
      this.Model.Hide();
    }

    private void OnToggleAutoHide()
    {
      this.Model.ToggleAutoHide();
    }

    #endregion
  }
}
