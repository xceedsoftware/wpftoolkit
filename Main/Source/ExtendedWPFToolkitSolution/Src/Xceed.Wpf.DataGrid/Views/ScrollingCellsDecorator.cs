/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Xceed.Wpf.DataGrid.Views
{
  internal class ScrollingCellsDecorator : Decorator
  {
    #region Constructors

    static ScrollingCellsDecorator()
    {
      KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(
        typeof( ScrollingCellsDecorator ),
        new FrameworkPropertyMetadata( KeyboardNavigationMode.Local ) );

      // Binding used to affect the AnimatedSplitterTranslationBinding only
      // when performing an animated Column reordering
      AreColumnsBeingReorderedBinding = new Binding();
      AreColumnsBeingReorderedBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );
      AreColumnsBeingReorderedBinding.Path = new PropertyPath( "(0).(1)",
        DataGridControl.DataGridContextProperty,
        TableflowView.AreColumnsBeingReorderedProperty );

      AreColumnsBeingReorderedBinding.Mode = BindingMode.OneWay;
    }

    public ScrollingCellsDecorator()
    {
      this.Focusable = false;

      BindingOperations.SetBinding(
         this,
         ScrollingCellsDecorator.AreColumnsBeingReorderedProperty,
         ScrollingCellsDecorator.AreColumnsBeingReorderedBinding );
    }

    #endregion

    #region SplitterOffset Public Property

    public static readonly DependencyProperty SplitterOffsetProperty = DependencyProperty.Register(
      "SplitterOffset",
      typeof( double ),
      typeof( ScrollingCellsDecorator ),
      new FrameworkPropertyMetadata( 0d, FrameworkPropertyMetadataOptions.AffectsArrange ) );

    public double SplitterOffset
    {
      get
      {
        return ( double )this.GetValue( ScrollingCellsDecorator.SplitterOffsetProperty );
      }
      set
      {
        this.SetValue( ScrollingCellsDecorator.SplitterOffsetProperty, value );
      }
    }

    #endregion

    #region ParentScrollViewer Private Property

    private ScrollViewer ParentScrollViewer
    {
      get
      {
        if( m_scrollViewer == null )
        {
          m_scrollViewer = TableViewScrollViewer.GetParentScrollViewer( this );
        }

        return m_scrollViewer;
      }
    }

    #endregion

    #region AreColumnsBeingReordered Property

    internal static readonly DependencyProperty AreColumnsBeingReorderedProperty = DependencyProperty.Register(
      "AreColumnsBeingReordered",
      typeof( bool ),
      typeof( ScrollingCellsDecorator ),
      new FrameworkPropertyMetadata(
        ( bool )false,
        new PropertyChangedCallback( ScrollingCellsDecorator.OnAreColumnsBeingReorderedChanged ) ) );

    internal bool AreColumnsBeingReordered
    {
      get
      {
        return ( bool )this.GetValue( ScrollingCellsDecorator.AreColumnsBeingReorderedProperty );
      }
      set
      {
        this.SetValue( ScrollingCellsDecorator.AreColumnsBeingReorderedProperty, value );
      }
    }

    private static void OnAreColumnsBeingReorderedChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ScrollingCellsDecorator decorator = sender as ScrollingCellsDecorator;

      if( decorator == null )
        return;


      if( ( bool )e.NewValue )
      {
        decorator.SetSplitterOffsetBinding();
      }
      else
      {
        decorator.ClearSplitterOffsetBinding();
      }
    }

    #endregion

    #region Protected Methods

    protected override Size ArrangeOverride( Size arrangeSize )
    {
      // We need to take ScrollingCellsDecoratorClipOffset into consideration
      // in case an animated column reordering is in progress
      double clipOffset = this.SplitterOffset;

      //Width of Rect cannot be less then 0.
      double widthOffset = Math.Max( 0, arrangeSize.Width - clipOffset );

      // Try to get the RectangleGeometry from the Clip
      // to avoid recreating one per call
      RectangleGeometry clip = this.Clip as RectangleGeometry;

      if( clip == null )
      {
        clip = new RectangleGeometry();
        this.Clip = clip;
      }

      clip.Rect = new Rect( clipOffset, -0.5d, widthOffset, arrangeSize.Height + 1d );

      return base.ArrangeOverride( arrangeSize );
    }

    #endregion

    #region Private Methods

    private void SetSplitterOffsetBinding()
    {
      if( m_splitterOffsetBinding == null )
      {
        // Binding used to know if there was a splitter translation to enlarge
        // or reduce the Clip region of the Decorator
        m_splitterOffsetBinding = new Binding();
        m_splitterOffsetBinding.Source = this;
        m_splitterOffsetBinding.Path = new PropertyPath( "(0).(1).X",
          DataGridControl.DataGridContextProperty,
          TableflowView.FixedColumnSplitterTranslationProperty );

        m_splitterOffsetBinding.Mode = BindingMode.OneWay;
      }

      BindingOperations.SetBinding(
          this,
          ScrollingCellsDecorator.SplitterOffsetProperty,
          m_splitterOffsetBinding );
    }

    private void ClearSplitterOffsetBinding()
    {
      BindingOperations.ClearBinding( this,
        ScrollingCellsDecorator.SplitterOffsetProperty );
    }


    #endregion

    #region Private Fields

    private ScrollViewer m_scrollViewer; // = null;
    private Binding m_splitterOffsetBinding; // = null;
    private static Binding AreColumnsBeingReorderedBinding; // = null;

    #endregion
  }
}
