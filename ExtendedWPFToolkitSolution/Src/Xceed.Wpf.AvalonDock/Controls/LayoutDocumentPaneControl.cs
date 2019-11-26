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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class LayoutDocumentPaneControl : TabControl, ILayoutControl//, ILogicalChildrenContainer
  {
    #region Members

    private List<object> _logicalChildren = new List<object>();
    private LayoutDocumentPane _model;

    #endregion

    #region Constructors

    static LayoutDocumentPaneControl()
    {
      FocusableProperty.OverrideMetadata( typeof( LayoutDocumentPaneControl ), new FrameworkPropertyMetadata( false ) );
    }

    internal LayoutDocumentPaneControl( LayoutDocumentPane model )
    {
      if( model == null )
        throw new ArgumentNullException( "model" );

      _model = model;
      SetBinding( ItemsSourceProperty, new Binding( "Model.Children" ) { Source = this } );
      SetBinding( FlowDirectionProperty, new Binding( "Model.Root.Manager.FlowDirection" ) { Source = this } );

      this.LayoutUpdated += new EventHandler( OnLayoutUpdated );
    }

    #endregion

    #region Properties



























    #region Model

    public ILayoutElement Model
    {
      get
      {
        return _model;
      }
    }

    #endregion

    #endregion

    #region Overrides

    protected override System.Collections.IEnumerator LogicalChildren
    {
      get
      {
        return _logicalChildren.GetEnumerator();
      }
    }

    protected override void OnSelectionChanged( SelectionChangedEventArgs e )
    {
      base.OnSelectionChanged( e );

      if( _model.SelectedContent != null )
        _model.SelectedContent.IsActive = true;
    }

    protected override void OnMouseLeftButtonDown( System.Windows.Input.MouseButtonEventArgs e )
    {
      base.OnMouseLeftButtonDown( e );

      if( !e.Handled && _model.SelectedContent != null )
        _model.SelectedContent.IsActive = true;
    }

    protected override void OnMouseRightButtonDown( System.Windows.Input.MouseButtonEventArgs e )
    {
      base.OnMouseRightButtonDown( e );

      if( !e.Handled && _model.SelectedContent != null )
        _model.SelectedContent.IsActive = true;

    }


    #endregion

    #region Private Methods

    private void OnLayoutUpdated( object sender, EventArgs e )
    {
      var modelWithAtcualSize = _model as ILayoutPositionableElementWithActualSize;
      modelWithAtcualSize.ActualWidth = ActualWidth;
      modelWithAtcualSize.ActualHeight = ActualHeight;
    }

    #endregion
























  }
}
