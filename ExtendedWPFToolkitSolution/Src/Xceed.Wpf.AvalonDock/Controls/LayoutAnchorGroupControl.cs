/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Linq;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class LayoutAnchorGroupControl : Control, ILayoutControl
  {
    #region Members

    private ObservableCollection<LayoutAnchorControl> _childViews = new ObservableCollection<LayoutAnchorControl>();
    private LayoutAnchorGroup _model;

    #endregion

    #region Constructors

    static LayoutAnchorGroupControl()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( LayoutAnchorGroupControl ), new FrameworkPropertyMetadata( typeof( LayoutAnchorGroupControl ) ) );
    }

    internal LayoutAnchorGroupControl( LayoutAnchorGroup model )
    {
      _model = model;
      CreateChildrenViews();

      _model.Children.CollectionChanged += ( s, e ) => OnModelChildrenCollectionChanged( e );
    }

    #endregion

    #region Properties

    public ObservableCollection<LayoutAnchorControl> Children
    {
      get
      {
        return _childViews;
      }
    }

    public ILayoutElement Model
    {
      get
      {
        return _model;
      }
    }

    #endregion

    #region Private Methods

    private void CreateChildrenViews()
    {
      var manager = _model.Root.Manager;
      foreach( var childModel in _model.Children )
      {
        _childViews.Add( new LayoutAnchorControl( childModel ) { Template = manager.AnchorTemplate } );
      }
    }

    private void OnModelChildrenCollectionChanged( System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
    {
      if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
          e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace )
      {
        if( e.OldItems != null )
        {
          {
            foreach( var childModel in e.OldItems )
              _childViews.Remove( _childViews.First( cv => cv.Model == childModel ) );
          }
        }
      }

      if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset )
        _childViews.Clear();

      if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
          e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace )
      {
        if( e.NewItems != null )
        {
          var manager = _model.Root.Manager;
          int insertIndex = e.NewStartingIndex;
          foreach( LayoutAnchorable childModel in e.NewItems )
          {
            _childViews.Insert( insertIndex++, new LayoutAnchorControl( childModel ) { Template = manager.AnchorTemplate } );
          }
        }
      }
    }

    #endregion
  }
}
