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

using System.Collections;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.Core
{
  /// <summary>
  /// An adorner that can display one and only one UIElement.  
  /// That element can be a panel, which contains multiple other elements.
  /// The element is added to the adorner's visual and logical trees, enabling it to 
  /// particpate in dependency property value inheritance, amongst other things.
  /// </summary>
  internal class UIElementAdorner<TElement> : Adorner where TElement : UIElement
  {
    #region Fields

    TElement _child = null;
    double _offsetLeft = 0;
    double _offsetTop = 0;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor. 
    /// </summary>
    /// <param name="adornedElement">The element to which the adorner will be bound.</param>
    public UIElementAdorner( UIElement adornedElement )
      : base( adornedElement )
    {
    }

    #endregion // Constructor

    #region Public Interface

    #region Child

    /// <summary>
    /// Gets/sets the child element hosted in the adorner.
    /// </summary>
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

    /// <summary>
    /// Override.
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public override GeneralTransform GetDesiredTransform( GeneralTransform transform )
    {
      GeneralTransformGroup result = new GeneralTransformGroup();
      result.Children.Add( base.GetDesiredTransform( transform ) );
      result.Children.Add( new TranslateTransform( _offsetLeft, _offsetTop ) );
      return result;
    }

    #endregion // GetDesiredTransform

    #region OffsetLeft

    /// <summary>
    /// Gets/sets the horizontal offset of the adorner.
    /// </summary>
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

    /// <summary>
    /// Updates the location of the adorner in one atomic operation.
    /// </summary>
    public void SetOffsets( double left, double top )
    {
      _offsetLeft = left;
      _offsetTop = top;
      this.UpdateLocation();
    }

    #endregion // SetOffsets

    #region OffsetTop

    /// <summary>
    /// Gets/sets the vertical offset of the adorner.
    /// </summary>
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

    /// <summary>
    /// Override.
    /// </summary>
    /// <param name="constraint"></param>
    /// <returns></returns>
    protected override Size MeasureOverride( Size constraint )
    {
      if( _child == null )
        return base.MeasureOverride( constraint );

      _child.Measure( constraint );
      return _child.DesiredSize;
    }

    /// <summary>
    /// Override.
    /// </summary>
    /// <param name="finalSize"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride( Size finalSize )
    {
      if( _child == null )
        return base.ArrangeOverride( finalSize );

      _child.Arrange( new Rect( finalSize ) );
      return finalSize;
    }

    /// <summary>
    /// Override.
    /// </summary>
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

    /// <summary>
    /// Override.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    protected override Visual GetVisualChild( int index )
    {
      return _child;
    }

    /// <summary>
    /// Override.
    /// </summary>
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
