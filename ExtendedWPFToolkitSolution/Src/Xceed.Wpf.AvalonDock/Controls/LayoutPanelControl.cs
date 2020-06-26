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
using System.Windows.Controls;
using System.Windows;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class LayoutPanelControl : LayoutGridControl<ILayoutPanelElement>, ILayoutControl
  {
    #region Members

    private LayoutPanel _model;

    #endregion

    #region Constructors

    internal LayoutPanelControl( LayoutPanel model )
        : base( model, model.Orientation )
    {
      _model = model;

    }

    #endregion

    #region Overrides

    protected override void OnFixChildrenDockLengths()
    {
      if( ActualWidth == 0.0 ||
          ActualHeight == 0.0 )
        return;

      var modelAsPositionableElement = _model as ILayoutPositionableElementWithActualSize;
      #region Setup DockWidth/Height for children
      if( _model.Orientation == Orientation.Horizontal )
      {
        if( _model.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>() )
        {
          for( int i = 0; i < _model.Children.Count; i++ )
          {
            var childContainerModel = _model.Children[ i ] as ILayoutContainer;
            var childPositionableModel = _model.Children[ i ] as ILayoutPositionableElement;

            if( childContainerModel != null &&
                ( childContainerModel.IsOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>() ||
                 childContainerModel.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>() ) )
            {
             // childPositionableModel.DockWidth = new GridLength( 1.0, GridUnitType.Star );
            }
            else if( (childPositionableModel != null) && childPositionableModel.DockWidth.IsStar )
            {
              var childPositionableModelWidthActualSize = childPositionableModel as ILayoutPositionableElementWithActualSize;
              if( childPositionableModelWidthActualSize.ActualWidth == 0d )
                continue;

              var widthToSet = Math.Max( childPositionableModelWidthActualSize.ActualWidth, childPositionableModel.DockMinWidth );

              widthToSet = Math.Min( widthToSet, ActualWidth / 2.0 );
              widthToSet = Math.Max( widthToSet, childPositionableModel.DockMinWidth );

              childPositionableModel.DockWidth = new GridLength( double.IsNaN( widthToSet ) ? ActualWidth / 2.0 : widthToSet, GridUnitType.Pixel );
            }
          }
        }
        //else
        //{
        //  for( int i = 0; i < _model.Children.Count; i++ )
        //  {
        //    var childPositionableModel = _model.Children[ i ] as ILayoutPositionableElement;
        //    if( !childPositionableModel.DockWidth.IsStar )
        //    {
        //      childPositionableModel.DockWidth = new GridLength( 1.0, GridUnitType.Star );
        //    }
        //  }
        //}
      }
      else
      {
        if( _model.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>() )
        {
          for( int i = 0; i < _model.Children.Count; i++ )
          {
            var childContainerModel = _model.Children[ i ] as ILayoutContainer;
            var childPositionableModel = _model.Children[ i ] as ILayoutPositionableElement;

            if( childContainerModel != null &&
                ( childContainerModel.IsOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>() ||
                 childContainerModel.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>() ) )
            {
              //childPositionableModel.DockHeight = new GridLength( 1.0, GridUnitType.Star );
            }
            else if( (childPositionableModel != null) && childPositionableModel.DockHeight.IsStar )
            {
              var childPositionableModelWidthActualSize = childPositionableModel as ILayoutPositionableElementWithActualSize;
              if( childPositionableModelWidthActualSize.ActualHeight == 0d )
                continue;

              var heightToSet = Math.Max( childPositionableModelWidthActualSize.ActualHeight, childPositionableModel.DockMinHeight );
              heightToSet = Math.Min( heightToSet, ActualHeight / 2.0 );
              heightToSet = Math.Max( heightToSet, childPositionableModel.DockMinHeight );

              childPositionableModel.DockHeight = new GridLength( double.IsNaN( heightToSet ) ? ActualHeight / 2.0 : heightToSet, GridUnitType.Pixel );
            }
          }
        }
        //else
        //{
        //  for( int i = 0; i < _model.Children.Count; i++ )
        //  {
        //    var childPositionableModel = _model.Children[ i ] as ILayoutPositionableElement;
        //    if( !childPositionableModel.DockHeight.IsStar )
        //    {
        //      childPositionableModel.DockHeight = new GridLength( 1.0, GridUnitType.Star );
        //    }
        //  }
        //}
      }
      #endregion
    }

    #endregion
  }
}
