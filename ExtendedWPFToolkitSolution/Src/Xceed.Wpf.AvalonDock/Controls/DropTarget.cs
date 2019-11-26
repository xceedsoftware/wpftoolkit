/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2018 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Threading;

namespace Xceed.Wpf.AvalonDock.Controls
{
  internal abstract class DropTarget<T> : DropTargetBase, IDropTarget where T : FrameworkElement
  {
    #region Members

    private Rect[] _detectionRect;
    private T _targetElement;
    private DropTargetType _type;

    #endregion

    #region Constructors

    protected DropTarget( T targetElement, Rect detectionRect, DropTargetType type )
    {
      _targetElement = targetElement;
      _detectionRect = new Rect[] { detectionRect };
      _type = type;
    }

    protected DropTarget( T targetElement, IEnumerable<Rect> detectionRects, DropTargetType type )
    {
      _targetElement = targetElement;
      _detectionRect = detectionRects.ToArray();
      _type = type;
    }

    #endregion

    #region Properties

    public Rect[] DetectionRects
    {
      get
      {
        return _detectionRect;
      }
    }

    public T TargetElement
    {
      get
      {
        return _targetElement;
      }
    }    

    public DropTargetType Type
    {
      get
      {
        return _type;
      }
    }

    #endregion

    #region Overrides

    protected virtual void Drop( LayoutAnchorableFloatingWindow floatingWindow )
    {
    }

    protected virtual void Drop( LayoutDocumentFloatingWindow floatingWindow )
    {
    }

    #endregion

    #region Public Methods

    public void Drop( LayoutFloatingWindow floatingWindow )
    {
      var root = floatingWindow.Root;
      var currentActiveContent = floatingWindow.Root.ActiveContent;
      var fwAsAnchorable = floatingWindow as LayoutAnchorableFloatingWindow;

      if( fwAsAnchorable != null )
      {
        this.Drop( fwAsAnchorable );
      }
      else
      {
        var fwAsDocument = floatingWindow as LayoutDocumentFloatingWindow;
        this.Drop( fwAsDocument );
      }

      if( currentActiveContent != null )
      {
        Dispatcher.BeginInvoke( new Action( () =>
            {
              currentActiveContent.IsSelected = false;
              currentActiveContent.IsActive = false;
              currentActiveContent.IsActive = true;
            } ), DispatcherPriority.Background );
      }
    }

    public virtual bool HitTest( Point dragPoint )
    {
      return _detectionRect.Any( dr => dr.Contains( dragPoint ) );
    }

    public abstract Geometry GetPreviewPath( OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindow );

    public void DragEnter()
    {
      SetIsDraggingOver( TargetElement, true );
    }

    public void DragLeave()
    {
      SetIsDraggingOver( TargetElement, false );
    }

    #endregion
  }
}
