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
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public class LayoutAnchorSideControl : Control, ILayoutControl
  {
    #region Members

    private LayoutAnchorSide _model = null;
    private ObservableCollection<LayoutAnchorGroupControl> _childViews = new ObservableCollection<LayoutAnchorGroupControl>();


    #endregion

    #region Constructors

    static LayoutAnchorSideControl()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( LayoutAnchorSideControl ), new FrameworkPropertyMetadata( typeof( LayoutAnchorSideControl ) ) );
    }

    internal LayoutAnchorSideControl( LayoutAnchorSide model )
    {
      if( model == null )
        throw new ArgumentNullException( "model" );


      _model = model;

      CreateChildrenViews();

      _model.Children.CollectionChanged += ( s, e ) => OnModelChildrenCollectionChanged( e );

      UpdateSide();
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

    #region Children

    public ObservableCollection<LayoutAnchorGroupControl> Children
    {
      get
      {
        return _childViews;
      }
    }

    #endregion

    #region IsLeftSide

    /// <summary>
    /// IsLeftSide Read-Only Dependency Property
    /// </summary>
    private static readonly DependencyPropertyKey IsLeftSidePropertyKey = DependencyProperty.RegisterReadOnly( "IsLeftSide", typeof( bool ), typeof( LayoutAnchorSideControl ),
            new FrameworkPropertyMetadata( ( bool )false ) );

    public static readonly DependencyProperty IsLeftSideProperty = IsLeftSidePropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the IsLeftSide property.  This dependency property 
    /// indicates this control is anchored to left side.
    /// </summary>
    public bool IsLeftSide
    {
      get
      {
        return ( bool )GetValue( IsLeftSideProperty );
      }
    }

    /// <summary>
    /// Provides a secure method for setting the IsLeftSide property.  
    /// This dependency property indicates this control is anchored to left side.
    /// </summary>
    /// <param name="value">The new value for the property.</param>
    protected void SetIsLeftSide( bool value )
    {
      SetValue( IsLeftSidePropertyKey, value );
    }

    #endregion

    #region IsTopSide

    /// <summary>
    /// IsTopSide Read-Only Dependency Property
    /// </summary>
    private static readonly DependencyPropertyKey IsTopSidePropertyKey = DependencyProperty.RegisterReadOnly( "IsTopSide", typeof( bool ), typeof( LayoutAnchorSideControl ),
            new FrameworkPropertyMetadata( ( bool )false ) );

    public static readonly DependencyProperty IsTopSideProperty = IsTopSidePropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the IsTopSide property.  This dependency property 
    /// indicates this control is anchored to top side.
    /// </summary>
    public bool IsTopSide
    {
      get
      {
        return ( bool )GetValue( IsTopSideProperty );
      }
    }

    /// <summary>
    /// Provides a secure method for setting the IsTopSide property.  
    /// This dependency property indicates this control is anchored to top side.
    /// </summary>
    /// <param name="value">The new value for the property.</param>
    protected void SetIsTopSide( bool value )
    {
      SetValue( IsTopSidePropertyKey, value );
    }

    #endregion

    #region IsRightSide

    /// <summary>
    /// IsRightSide Read-Only Dependency Property
    /// </summary>
    private static readonly DependencyPropertyKey IsRightSidePropertyKey = DependencyProperty.RegisterReadOnly( "IsRightSide", typeof( bool ), typeof( LayoutAnchorSideControl ),
            new FrameworkPropertyMetadata( ( bool )false ) );

    public static readonly DependencyProperty IsRightSideProperty = IsRightSidePropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the IsRightSide property.  This dependency property 
    /// indicates this control is anchored to right side.
    /// </summary>
    public bool IsRightSide
    {
      get
      {
        return ( bool )GetValue( IsRightSideProperty );
      }
    }

    /// <summary>
    /// Provides a secure method for setting the IsRightSide property.  
    /// This dependency property indicates this control is anchored to right side.
    /// </summary>
    /// <param name="value">The new value for the property.</param>
    protected void SetIsRightSide( bool value )
    {
      SetValue( IsRightSidePropertyKey, value );
    }

    #endregion

    #region IsBottomSide

    /// <summary>
    /// IsBottomSide Read-Only Dependency Property
    /// </summary>
    private static readonly DependencyPropertyKey IsBottomSidePropertyKey = DependencyProperty.RegisterReadOnly( "IsBottomSide", typeof( bool ), typeof( LayoutAnchorSideControl ),
            new FrameworkPropertyMetadata( ( bool )false ) );

    public static readonly DependencyProperty IsBottomSideProperty = IsBottomSidePropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the IsBottomSide property.  This dependency property 
    /// indicates if this panel is anchored to bottom side.
    /// </summary>
    public bool IsBottomSide
    {
      get
      {
        return ( bool )GetValue( IsBottomSideProperty );
      }
    }

    /// <summary>
    /// Provides a secure method for setting the IsBottomSide property.  
    /// This dependency property indicates if this panel is anchored to bottom side.
    /// </summary>
    /// <param name="value">The new value for the property.</param>
    protected void SetIsBottomSide( bool value )
    {
      SetValue( IsBottomSidePropertyKey, value );
    }

    #endregion

    #endregion

    #region Overrides


    #endregion

    #region Private Methods

    private void CreateChildrenViews()
    {
      var manager = _model.Root.Manager;
      foreach( var childModel in _model.Children )
      {
        _childViews.Add( manager.CreateUIElementForModel( childModel ) as LayoutAnchorGroupControl );
      }
    }

    private void OnModelChildrenCollectionChanged( System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
    {
      if( e.OldItems != null &&
          ( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
          e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace ) )
      {
        foreach( var childModel in e.OldItems )
          _childViews.Remove( _childViews.First( cv => cv.Model == childModel ) );
      }

      if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset )
        _childViews.Clear();

      if( e.NewItems != null &&
          ( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
          e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace ) )
      {
        var manager = _model.Root.Manager;
        int insertIndex = e.NewStartingIndex;
        foreach( LayoutAnchorGroup childModel in e.NewItems )
        {
          _childViews.Insert( insertIndex++, manager.CreateUIElementForModel( childModel ) as LayoutAnchorGroupControl );
        }
      }
    }

    private void UpdateSide()
    {
      switch( _model.Side )
      {
        case AnchorSide.Left:
          SetIsLeftSide( true );
          break;
        case AnchorSide.Top:
          SetIsTopSide( true );
          break;
        case AnchorSide.Right:
          SetIsRightSide( true );
          break;
        case AnchorSide.Bottom:
          SetIsBottomSide( true );
          break;
      }
    }

    #endregion
  }
}
