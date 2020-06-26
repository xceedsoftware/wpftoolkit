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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit
{
  public class MagnifierAdorner : Adorner
  {
    #region Members

    private Magnifier _magnifier;
    private Point _currentMousePosition;
    private double _currentZoomFactor;

    #endregion

    #region Constructors

    public MagnifierAdorner( UIElement element, Magnifier magnifier )
      : base( element )
    {
      _magnifier = magnifier;
      _currentZoomFactor = _magnifier.ZoomFactor;
      UpdateViewBox();
      AddVisualChild( _magnifier );

      Loaded += ( s, e ) => InputManager.Current.PostProcessInput += OnProcessInput;
      Unloaded += ( s, e ) => InputManager.Current.PostProcessInput -= OnProcessInput;
    }


    #endregion

    #region Private/Internal methods

    private void OnProcessInput( object sender, ProcessInputEventArgs e )
    {
      Point pt = Mouse.GetPosition( this );

      if( (_currentMousePosition == pt) && (_magnifier.ZoomFactor == _currentZoomFactor) )
        return;

      if( _magnifier.IsFrozen )
        return;

      _currentMousePosition = pt;
      _currentZoomFactor = _magnifier.ZoomFactor;
      UpdateViewBox();
      InvalidateArrange();
    }

    internal void UpdateViewBox()
    {
      var viewBoxLocation = CalculateViewBoxLocation();
      _magnifier.ViewBox = new Rect( viewBoxLocation, _magnifier.ViewBox.Size );
    }

    private Point CalculateViewBoxLocation()
    {
      double offsetX = 0, offsetY = 0;

      Point adorner = Mouse.GetPosition( this );
      Point element = Mouse.GetPosition( AdornedElement );

      offsetX = element.X - adorner.X;
      offsetY = element.Y - adorner.Y;

      //An element will use the offset from its parent (StackPanel, Grid, etc.) to be rendered.
      //When this element is put in a VisualBrush, the element will draw with that offset applied. 
      //To fix this: we add that parent offset to Magnifier location.
      Vector parentOffsetVector = VisualTreeHelper.GetOffset( _magnifier.Target );
      Point parentOffset = new Point( parentOffsetVector.X, parentOffsetVector.Y );

      double left = _currentMousePosition.X - ( ( _magnifier.ViewBox.Width / 2 ) + offsetX ) + parentOffset.X;
      double top = _currentMousePosition.Y - ( ( _magnifier.ViewBox.Height / 2 ) + offsetY ) + parentOffset.Y;
      return new Point( left, top );
    }

    #endregion

    #region Overrides

    protected override Visual GetVisualChild( int index )
    {
      return _magnifier;
    }

    protected override int VisualChildrenCount
    {
      get
      {
        return 1;
      }
    }

    protected override Size MeasureOverride( Size constraint )
    {
      _magnifier.Measure( constraint );
      return base.MeasureOverride( constraint );
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
      double x = _currentMousePosition.X - ( _magnifier.Width / 2 );
      double y = _currentMousePosition.Y - ( _magnifier.Height / 2 );
      _magnifier.Arrange( new Rect( x, y, _magnifier.Width, _magnifier.Height ) );
      return base.ArrangeOverride( finalSize );
    }

    #endregion
  }
}
