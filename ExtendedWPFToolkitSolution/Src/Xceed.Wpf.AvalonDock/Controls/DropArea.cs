/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public enum DropAreaType
  {
    DockingManager,
    DocumentPane,
    DocumentPaneGroup,
    AnchorablePane,
  }


  public interface IDropArea
  {
    Rect DetectionRect
    {
      get;
    }
    DropAreaType Type
    {
      get;
    }
  }

  public class DropArea<T> : IDropArea where T : FrameworkElement
  {
    #region Members

    private Rect _detectionRect;
    private DropAreaType _type;
    private T _element;

    #endregion

    #region Constructors

    internal DropArea( T areaElement, DropAreaType type )
    {
      _element = areaElement;
      _detectionRect = areaElement.GetScreenArea();
      _type = type;
    }

    #endregion

    #region Properties

    public Rect DetectionRect
    {
      get
      {
        return _detectionRect;
      }
    }   

    public DropAreaType Type
    {
      get
      {
        return _type;
      }
    }

    public T AreaElement
    {
      get
      {
        return _element;
      }
    }

    #endregion
  }
}
