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

using System.Windows.Controls;
using System.Windows;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class LayoutAnchorablePaneGroupControl : LayoutGridControl<ILayoutAnchorablePane>, ILayoutControl
  {
    #region Members

    private LayoutAnchorablePaneGroup _model;

    #endregion

    #region Constructors

    internal LayoutAnchorablePaneGroupControl( LayoutAnchorablePaneGroup model )
        : base( model, model.Orientation )
    {
      _model = model;
    }

    #endregion

    #region Overrides

    protected override void OnFixChildrenDockLengths()
    {
      if( _model.Orientation == Orientation.Horizontal )
      {
        for( int i = 0; i < _model.Children.Count; i++ )
        {
          var childModel = _model.Children[ i ] as ILayoutPositionableElement;
          if( !childModel.DockWidth.IsStar )
          {
            childModel.DockWidth = new GridLength( 1.0, GridUnitType.Star );
          }
        }
      }
      else
      {
        for( int i = 0; i < _model.Children.Count; i++ )
        {
          var childModel = _model.Children[ i ] as ILayoutPositionableElement;
          if( !childModel.DockHeight.IsStar )
          {
            childModel.DockHeight = new GridLength( 1.0, GridUnitType.Star );
          }
        }
      }
    }

    #endregion
  }
}
