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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class DropDownControlArea : UserControl
  {
    #region Constructors

    //static DropDownControlArea()
    //{
    //    //IsHitTestVisibleProperty.OverrideMetadata(typeof(DropDownControlArea), new FrameworkPropertyMetadata(true));
    //}

    public DropDownControlArea()
    {
    }

    #endregion

    #region Properties

    #region DropDownContextMenu

    /// <summary>
    /// DropDownContextMenu Dependency Property
    /// </summary>
    public static readonly DependencyProperty DropDownContextMenuProperty = DependencyProperty.Register( "DropDownContextMenu", typeof( ContextMenu ), typeof( DropDownControlArea ),
            new FrameworkPropertyMetadata( ( ContextMenu )null ) );

    /// <summary>
    /// Gets or sets the DropDownContextMenu property.  This dependency property 
    /// indicates context menu to show when a right click is detected over the area occpied by the control.
    /// </summary>
    public ContextMenu DropDownContextMenu
    {
      get
      {
        return ( ContextMenu )GetValue( DropDownContextMenuProperty );
      }
      set
      {
        SetValue( DropDownContextMenuProperty, value );
      }
    }

    #endregion

    #region DropDownContextMenuDataContext

    /// <summary>
    /// DropDownContextMenuDataContext Dependency Property
    /// </summary>
    public static readonly DependencyProperty DropDownContextMenuDataContextProperty = DependencyProperty.Register( "DropDownContextMenuDataContext", typeof( object ), typeof( DropDownControlArea ),
            new FrameworkPropertyMetadata( ( object )null ) );

    /// <summary>
    /// Gets or sets the DropDownContextMenuDataContext property.  This dependency property 
    /// indicates data context to attach when context menu is shown.
    /// </summary>
    public object DropDownContextMenuDataContext
    {
      get
      {
        return ( object )GetValue( DropDownContextMenuDataContextProperty );
      }
      set
      {
        SetValue( DropDownContextMenuDataContextProperty, value );
      }
    }

    #endregion

    #endregion

    #region Overrides

    protected override void OnMouseRightButtonDown( System.Windows.Input.MouseButtonEventArgs e )
    {
      base.OnMouseRightButtonDown( e );
    }

    protected override void OnPreviewMouseRightButtonUp( System.Windows.Input.MouseButtonEventArgs e )
    {
      base.OnPreviewMouseRightButtonUp( e );

      if( !e.Handled )
      {
        if( DropDownContextMenu != null )
        {
          DropDownContextMenu.PlacementTarget = null;
          DropDownContextMenu.Placement = PlacementMode.MousePoint;
          DropDownContextMenu.HorizontalOffset = 0d;
          DropDownContextMenu.VerticalOffset = 0d;
          DropDownContextMenu.DataContext = DropDownContextMenuDataContext;
          DropDownContextMenu.IsOpen = true;
          // e.Handled = true;
        }
      }
    }

    //protected override System.Windows.Media.HitTestResult HitTestCore(System.Windows.Media.PointHitTestParameters hitTestParameters)
    //{
    //    var hitResult = base.HitTestCore(hitTestParameters);
    //    if (hitResult == null)
    //        return new PointHitTestResult(this, hitTestParameters.HitPoint);

    //    return hitResult;
    //}

    #endregion
  }
}
