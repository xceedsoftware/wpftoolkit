/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2025 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.Core
{
  internal class UIElementAdorner<TElement> : Adorner where TElement : UIElement
  {
    #region Fields

    TElement _child = null;
    double _offsetLeft = 0;
    double _offsetTop = 0;

    #endregion // Fields

    #region Constructor

    public UIElementAdorner( UIElement adornedElement )
      : base( adornedElement )
    {
    }

    #endregion // Constructor

    #region Public Interface

    #region Child

    public TElement Child
    {
      get
      {
        return _child;
      }
      set
      {
        if( value == _child )
          return;

        if( _child != null )
        {
          base.RemoveLogicalChild( _child );
          base.RemoveVisualChild( _child );
        }

        _child = value;

        if( _child != null )
        {
          base.AddLogicalChild( _child );
          base.AddVisualChild( _child );
        }
      }
    }

    #endregion // Child

    #region GetDesiredTransform

    public override GeneralTransform GetDesiredTransform( GeneralTransform transform )
    {
      GeneralTransformGroup result = new GeneralTransformGroup();
      result.Children.Add( base.GetDesiredTransform( transform ) );
      result.Children.Add( new TranslateTransform( _offsetLeft, _offsetTop ) );
      return result;
    }

    #endregion // GetDesiredTransform

    #region OffsetLeft

    public double OffsetLeft
    {
      get
      {
        return _offsetLeft;
      }
      set
      {
        _offsetLeft = value;
        UpdateLocation();
      }
    }

    #endregion // OffsetLeft

    #region SetOffsets

    public void SetOffsets( double left, double top )
    {
      _offsetLeft = left;
      _offsetTop = top;
      this.UpdateLocation();
    }

    #endregion // SetOffsets

    #region OffsetTop

    public double OffsetTop
    {
      get
      {
        return _offsetTop;
      }
      set
      {
        _offsetTop = value;
        UpdateLocation();
      }
    }

    #endregion // OffsetTop

    #endregion // Public Interface

    #region Protected Overrides

    protected override Size MeasureOverride( Size constraint )
    {
      if( _child == null )
        return base.MeasureOverride( constraint );

      _child.Measure( constraint );
      return _child.DesiredSize;
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
      if( _child == null )
        return base.ArrangeOverride( finalSize );

      _child.Arrange( new Rect( finalSize ) );
      return finalSize;
    }

    protected override IEnumerator LogicalChildren
    {
      get
      {
        ArrayList list = new ArrayList();
        if( _child != null )
          list.Add( _child );
        return list.GetEnumerator();
      }
    }

    protected override Visual GetVisualChild( int index )
    {
      return _child;
    }

    protected override int VisualChildrenCount
    {
      get
      {
        return _child == null ? 0 : 1;
      }
    }

    #endregion // Protected Overrides

    #region Private Helpers

    void UpdateLocation()
    {
      AdornerLayer adornerLayer = base.Parent as AdornerLayer;
      if( adornerLayer != null )
        adornerLayer.Update( base.AdornedElement );
    }

    #endregion // Private Helpers
  }
}
