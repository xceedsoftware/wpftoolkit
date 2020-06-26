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
using System.Windows.Markup;

namespace Xceed.Wpf.AvalonDock.Layout
{
  [ContentProperty( "Children" )]
  [Serializable]
  public class LayoutAnchorSide : LayoutGroup<LayoutAnchorGroup>
  {
    #region Constructors

    public LayoutAnchorSide()
    {
    }

    #endregion

    #region Properties

    #region Side

    private AnchorSide _side;
    public AnchorSide Side
    {
      get
      {
        return _side;
      }
      private set
      {
        if( _side != value )
        {
          RaisePropertyChanging( "Side" );
          _side = value;
          RaisePropertyChanged( "Side" );
        }
      }
    }

    #endregion

    #endregion

    #region Overrides

    protected override bool GetVisibility()
    {
      return Children.Count > 0;
    }


    protected override void OnParentChanged( ILayoutContainer oldValue, ILayoutContainer newValue )
    {
      base.OnParentChanged( oldValue, newValue );

      UpdateSide();
    }

    #endregion

    #region Private Methods

    private void UpdateSide()
    {
      if( Root.LeftSide == this )
        Side = AnchorSide.Left;
      else if( Root.TopSide == this )
        Side = AnchorSide.Top;
      else if( Root.RightSide == this )
        Side = AnchorSide.Right;
      else if( Root.BottomSide == this )
        Side = AnchorSide.Bottom;
    }

    #endregion
  }
}
