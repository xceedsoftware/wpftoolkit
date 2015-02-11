/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Xceed.Wpf.DataGrid.Views
{
  public class TableflowView : TableView
  {
    #region Constructors

    static TableflowView()
    {
      FrameworkContentElement.DefaultStyleKeyProperty.OverrideMetadata(
        typeof( TableflowView ),
        new FrameworkPropertyMetadata( TableflowView.GetDefaultStyleKey( typeof( TableflowView ), null ) ) );

      TableView.IsAlternatingRowStyleEnabledProperty.OverrideMetadata(
        typeof( TableflowView ),
        new FrameworkPropertyMetadata( true ) );

      TableView.AllowRowResizeProperty.OverrideMetadata(
        typeof( TableflowView ),
        new FrameworkPropertyMetadata( false ) );

      TableflowView.AreGroupsFlattenedProperty.OverrideMetadata(
        typeof( TableflowView ),
        new FrameworkPropertyMetadata( true ) );

      TableflowView.IsDeferredLoadingEnabledProperty.OverrideMetadata(
        typeof( TableflowView ),
        new FrameworkPropertyMetadata( true ) );
    }

    public TableflowView()
      : base()
    {
      // No need to preserve container size since it is
      // forced via ContainerHeight property
      this.PreserveContainerSize = false;
    }

    #endregion

    #region RowFadeInAnimationDuration Attached Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty RowFadeInAnimationDurationProperty = DependencyProperty.RegisterAttached(
      "RowFadeInAnimationDuration",
      typeof( double ),
      typeof( TableflowView ),
      new FrameworkPropertyMetadata( 300d ) );

    public double RowFadeInAnimationDuration
    {
      get
      {
        return TableflowView.GetRowFadeInAnimationDuration( this );
      }
      set
      {
        TableflowView.SetRowFadeInAnimationDuration( this, value );
      }
    }

    public static double GetRowFadeInAnimationDuration( DependencyObject obj )
    {
      return ( double )obj.GetValue( TableflowView.RowFadeInAnimationDurationProperty );
    }

    public static void SetRowFadeInAnimationDuration( DependencyObject obj, double value )
    {
      obj.SetValue( TableflowView.RowFadeInAnimationDurationProperty, value );
    }

    #endregion RowFadeInAnimationDuration Property

    #region IsAnimatedColumnReorderingEnabled Property

    public static readonly DependencyProperty IsAnimatedColumnReorderingEnabledProperty = DependencyProperty.Register(
      "IsAnimatedColumnReorderingEnabled",
      typeof( bool ),
      typeof( TableflowView ),
      new FrameworkPropertyMetadata( true ) );

    public bool IsAnimatedColumnReorderingEnabled
    {
      get
      {
        return ( bool )this.GetValue( TableflowView.IsAnimatedColumnReorderingEnabledProperty );
      }
      set
      {
        this.SetValue( TableflowView.IsAnimatedColumnReorderingEnabledProperty, value );
      }
    }

    #endregion RowFadeInAnimationDuration Attached Property

    #region ColumnReorderingDragSourceManager Attached Property

    [ViewProperty( ViewPropertyMode.RoutedNoFallback )]
    internal static readonly DependencyProperty ColumnReorderingDragSourceManagerProperty = DependencyProperty.RegisterAttached(
      "ColumnReorderingDragSourceManager",
      typeof( ColumnReorderingDragSourceManager ),
      typeof( TableflowView ),
      new FrameworkPropertyMetadata( null ) );

    internal static ColumnReorderingDragSourceManager GetColumnReorderingDragSourceManager( DependencyObject obj )
    {
      return ( ColumnReorderingDragSourceManager )obj.GetValue( TableflowView.ColumnReorderingDragSourceManagerProperty );
    }

    internal static void SetColumnReorderingDragSourceManager( DependencyObject obj, ColumnReorderingDragSourceManager value )
    {
      obj.SetValue( TableflowView.ColumnReorderingDragSourceManagerProperty, value );
    }

    internal static void ClearColumnReorderingDragSourceManager( DependencyObject obj )
    {
      obj.ClearValue( TableflowView.ColumnReorderingDragSourceManagerProperty );
    }

    #endregion

    #region AreColumnsBeingReordered Attached Property

    [ViewProperty( ViewPropertyMode.RoutedNoFallback, FlattenDetailBindingMode.MasterOneWay )]
    public static readonly DependencyProperty AreColumnsBeingReorderedProperty = DependencyProperty.RegisterAttached(
      "AreColumnsBeingReordered",
      typeof( bool ),
      typeof( TableflowView ),
      new FrameworkPropertyMetadata( ( bool )false ) );


    public static bool GetAreColumnsBeingReordered( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableflowView.AreColumnsBeingReorderedProperty );
    }

    [EditorBrowsable( EditorBrowsableState.Never )]
    public static void SetAreColumnsBeingReordered( DependencyObject obj, bool value )
    {
      obj.SetValue( TableflowView.AreColumnsBeingReorderedProperty, value );
    }

    #endregion

    #region IsBeingDraggedAnimated Attached Property

    internal static readonly DependencyProperty IsBeingDraggedAnimatedProperty = DependencyProperty.RegisterAttached(
      "IsBeingDraggedAnimated",
      typeof( bool ),
      typeof( TableflowView ),
      new FrameworkPropertyMetadata( ( bool )false ) );

    internal static bool GetIsBeingDraggedAnimated( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableflowView.IsBeingDraggedAnimatedProperty );
    }

    internal static void SetIsBeingDraggedAnimated( DependencyObject obj, bool value )
    {
      obj.SetValue( TableflowView.IsBeingDraggedAnimatedProperty, value );
    }

    internal static void ClearIsBeingDraggedAnimated( DependencyObject obj )
    {
      obj.ClearValue( TableflowView.IsBeingDraggedAnimatedProperty );
    }

    #endregion

    #region ContainerHeight Attached Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty ContainerHeightProperty = DependencyProperty.RegisterAttached(
      "ContainerHeight",
      typeof( double ),
      typeof( TableflowView ),
      new FrameworkPropertyMetadata( 26d ) );

    public double ContainerHeight
    {
      get
      {
        return TableflowView.GetContainerHeight( this );
      }
      set
      {
        TableflowView.SetContainerHeight( this, value );
      }
    }

    public static double GetContainerHeight( DependencyObject obj )
    {
      return ( double )obj.GetValue( TableflowView.ContainerHeightProperty );
    }

    public static void SetContainerHeight( DependencyObject obj, double value )
    {
      obj.SetValue( TableflowView.ContainerHeightProperty, value );
    }

    #endregion ContainerHeight Attached Property

    #region FixedColumnSplitterTranslation Attached Property

    [ViewProperty( ViewPropertyMode.RoutedNoFallback, FlattenDetailBindingMode.MasterOneWay )]
    internal static readonly DependencyProperty FixedColumnSplitterTranslationProperty = DependencyProperty.RegisterAttached(
      "FixedColumnSplitterTranslation",
      typeof( TranslateTransform ),
      typeof( TableflowView ),
      new FrameworkPropertyMetadata( null ) );

    internal static TranslateTransform GetFixedColumnSplitterTranslation( DependencyObject obj )
    {
      return ( TranslateTransform )obj.GetValue( TableflowView.FixedColumnSplitterTranslationProperty );
    }

    internal static void SetFixedColumnSplitterTranslation( DependencyObject obj, TranslateTransform value )
    {
      obj.SetValue( TableflowView.FixedColumnSplitterTranslationProperty, value );
    }

    #endregion

    #region ScrollingAnimationDuration Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty ScrollingAnimationDurationProperty = DependencyProperty.RegisterAttached(
      "ScrollingAnimationDuration",
      typeof( double ),
      typeof( TableflowView ),
      new FrameworkPropertyMetadata( 750d ) );

    public double ScrollingAnimationDuration
    {
      get
      {
        return TableflowView.GetScrollingAnimationDuration( this );
      }
      set
      {
        TableflowView.SetScrollingAnimationDuration( this, value );
      }
    }

    public static double GetScrollingAnimationDuration( DependencyObject obj )
    {
      return ( double )obj.GetValue( TableflowView.ScrollingAnimationDurationProperty );
    }

    public static void SetScrollingAnimationDuration( DependencyObject obj, double value )
    {
      obj.SetValue( TableflowView.ScrollingAnimationDurationProperty, value );
    }

    #endregion ScrollingAnimationDuration Property

    #region AreHeadersSticky Attached Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty AreHeadersStickyProperty = DependencyProperty.RegisterAttached(
      "AreHeadersSticky",
      typeof( bool ),
      typeof( TableflowView ),
      new FrameworkPropertyMetadata( true ) );

    public bool AreHeadersSticky
    {
      get
      {
        return TableflowView.GetAreHeadersSticky( this );
      }
      set
      {
        TableflowView.SetAreHeadersSticky( this, value );
      }
    }

    public static bool GetAreHeadersSticky( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableflowView.AreHeadersStickyProperty );
    }

    public static void SetAreHeadersSticky( DependencyObject obj, bool value )
    {
      obj.SetValue( TableflowView.AreHeadersStickyProperty, value );
    }

    #endregion AreHeadersSticky Attached Property

    #region AreFootersSticky Attached Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty AreFootersStickyProperty = DependencyProperty.RegisterAttached(
      "AreFootersSticky",
      typeof( bool ),
      typeof( TableflowView ),
      new FrameworkPropertyMetadata( true ) );

    public bool AreFootersSticky
    {
      get
      {
        return TableflowView.GetAreFootersSticky( this );
      }
      set
      {
        TableflowView.SetAreFootersSticky( this, value );
      }
    }

    public static bool GetAreFootersSticky( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableflowView.AreFootersStickyProperty );
    }

    public static void SetAreFootersSticky( DependencyObject obj, bool value )
    {
      obj.SetValue( TableflowView.AreFootersStickyProperty, value );
    }

    #endregion AreFootersSticky Attached Property

    #region AreGroupHeadersSticky Attached Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty AreGroupHeadersStickyProperty = DependencyProperty.RegisterAttached(
      "AreGroupHeadersSticky",
      typeof( bool ),
      typeof( TableflowView ),
      new FrameworkPropertyMetadata( true ) );

    public bool AreGroupHeadersSticky
    {
      get
      {
        return TableflowView.GetAreGroupHeadersSticky( this );
      }
      set
      {
        TableflowView.SetAreGroupHeadersSticky( this, value );
      }
    }

    public static bool GetAreGroupHeadersSticky( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableflowView.AreGroupHeadersStickyProperty );
    }

    public static void SetAreGroupHeadersSticky( DependencyObject obj, bool value )
    {
      obj.SetValue( TableflowView.AreGroupHeadersStickyProperty, value );
    }

    #endregion AreGroupHeadersSticky Attached Property

    #region AreGroupFootersSticky Attached Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty AreGroupFootersStickyProperty = DependencyProperty.RegisterAttached(
      "AreGroupFootersSticky",
      typeof( bool ),
      typeof( TableflowView ),
      new FrameworkPropertyMetadata( true ) );

    public bool AreGroupFootersSticky
    {
      get
      {
        return TableflowView.GetAreGroupFootersSticky( this );
      }
      set
      {
        TableflowView.SetAreGroupFootersSticky( this, value );
      }
    }

    public static bool GetAreGroupFootersSticky( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableflowView.AreGroupFootersStickyProperty );
    }

    public static void SetAreGroupFootersSticky( DependencyObject obj, bool value )
    {
      obj.SetValue( TableflowView.AreGroupFootersStickyProperty, value );
    }

    #endregion AreGroupFootersSticky Attached Property

    #region AreParentRowsSticky Attached Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty AreParentRowsStickyProperty = DependencyProperty.RegisterAttached(
      "AreParentRowsSticky",
      typeof( bool ),
      typeof( TableflowView ),
      new FrameworkPropertyMetadata( true ) );

    public bool AreParentRowsSticky
    {
      get
      {
        return TableflowView.GetAreParentRowsSticky( this );
      }
      set
      {
        TableflowView.SetAreParentRowsSticky( this, value );
      }
    }

    public static bool GetAreParentRowsSticky( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableflowView.AreParentRowsStickyProperty );
    }

    public static void SetAreParentRowsSticky( DependencyObject obj, bool value )
    {
      obj.SetValue( TableflowView.AreParentRowsStickyProperty, value );
    }

    #endregion AreParentRowsSticky Attached Property

    #region AreGroupsFlattened Attached Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty AreGroupsFlattenedProperty = DependencyProperty.RegisterAttached(
      "AreGroupsFlattened",
      typeof( bool ),
      typeof( TableflowView ) );

    public bool AreGroupsFlattened
    {
      get
      {
        return TableflowView.GetAreGroupsFlattened( this );
      }
      set
      {
        TableflowView.SetAreGroupsFlattened( this, value );
      }
    }

    public static bool GetAreGroupsFlattened( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableflowView.AreGroupsFlattenedProperty );
    }

    public static void SetAreGroupsFlattened( DependencyObject obj, bool value )
    {
      obj.SetValue( TableflowView.AreGroupsFlattenedProperty, value );
    }

    #endregion AreGroupsFlattened Attached Property

    #region IsDeferredLoadingEnabled Attached Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty IsDeferredLoadingEnabledProperty = DependencyProperty.RegisterAttached(
      "IsDeferredLoadingEnabled",
      typeof( bool ),
      typeof( TableflowView ) );

    public bool IsDeferredLoadingEnabled
    {
      get
      {
        return TableflowView.GetIsDeferredLoadingEnabled( this );
      }
      set
      {
        TableflowView.SetIsDeferredLoadingEnabled( this, value );
      }
    }

    public static bool GetIsDeferredLoadingEnabled( DependencyObject obj )
    {
      return ( bool )obj.GetValue( TableflowView.IsDeferredLoadingEnabledProperty );
    }

    public static void SetIsDeferredLoadingEnabled( DependencyObject obj, bool value )
    {
      obj.SetValue( TableflowView.IsDeferredLoadingEnabledProperty, value );
    }

    #endregion IsDeferredLoadingEnabled Attached Property
  }
}
