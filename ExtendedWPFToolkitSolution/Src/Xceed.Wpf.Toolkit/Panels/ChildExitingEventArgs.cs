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

namespace Xceed.Wpf.Toolkit.Panels
{
  public class ChildExitingEventArgs : RoutedEventArgs
  {
    #region Constructors

    public ChildExitingEventArgs( UIElement child, Rect? exitTo, Rect arrangeRect )
    {
      _child = child;
      _exitTo = exitTo;
      _arrangeRect = arrangeRect;
    }

    #endregion

    #region ArrangeRect Property

    public Rect ArrangeRect
    {
      get
      {
        return _arrangeRect;
      }
    }

    private readonly Rect _arrangeRect;

    #endregion

    #region Child Property

    public UIElement Child
    {
      get
      {
        return _child;
      }
    }

    private readonly UIElement _child;

    #endregion

    #region ExitTo Property

    public Rect? ExitTo
    {
      get
      {
        return _exitTo;
      }
      set
      {
        _exitTo = value;
      }
    }

    private Rect? _exitTo; //null

    #endregion
  }
}
