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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  public class ScrollTip : ContentControl, IWeakEventListener
  {
    #region CONSTRUCTORS

    static ScrollTip()
    {
      // This DefaultStyleKey will only be used in design-time.
      DefaultStyleKeyProperty.OverrideMetadata( typeof( ScrollTip ), new FrameworkPropertyMetadata( new Markup.ThemeKey( typeof( Views.TableView ), typeof( ScrollTip ) ) ) );

      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( ScrollTip ), new FrameworkPropertyMetadata( new PropertyChangedCallback( ScrollTip.ParentGridControlPropertyChangedCallback ) ) );
      DataGridControl.DataGridContextPropertyKey.OverrideMetadata( typeof( ScrollTip ), new FrameworkPropertyMetadata( new PropertyChangedCallback( ScrollTip.OnDataGridContextPropertyChanged ) ) );
    }

    #endregion

    #region PUBLIC METHODS

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      this.ScrollTipContentTemplateNeedsRefresh = true;
      this.RefreshDefaultScrollTipContentTemplate();
    }

    #endregion

    #region PROTECTED METHODS

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( ScrollTip ) );
    }

    #endregion

    #region ScrollTipContentTemplateNeedsRefresh Private Property

    private bool ScrollTipContentTemplateNeedsRefresh
    {
      get
      {
        return m_flags[ ( int )ScrollTipFlags.ScrollTipContentTemplateNeedsRefresh ];
      }
      set
      {
        m_flags[ ( int )ScrollTipFlags.ScrollTipContentTemplateNeedsRefresh ] = value;
      }
    }

    #endregion

    #region UsingDefaultScrollTipContentTemplate Private Property

    private bool UsingDefaultScrollTipContentTemplate
    {
      get
      {
        return m_flags[ ( int )ScrollTipFlags.UsingDefaultScrollTipContentTemplate ];
      }
      set
      {
        m_flags[ ( int )ScrollTipFlags.UsingDefaultScrollTipContentTemplate ] = value;
      }
    }

    #endregion

    #region IsInParentGridChanged Private Property

    private bool IsInParentGridChanged
    {
      get
      {
        return m_flags[ ( int )ScrollTipFlags.IsInParentGridChanged ];
      }
      set
      {
        m_flags[ ( int )ScrollTipFlags.IsInParentGridChanged ] = value;
      }
    }

    #endregion

    #region IsPixelScrolling Private Property

    private bool IsPixelScrolling
    {
      get
      {
        return m_flags[ ( int )ScrollTipFlags.IsPixelScrolling ];
      }
      set
      {
        m_flags[ ( int )ScrollTipFlags.IsPixelScrolling ] = value;
      }
    }

    #endregion

    #region ShouldDisplayScrollTip Private Property

    private bool ShouldDisplayScrollTip
    {
      get
      {
        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );
        if( dataGridContext != null )
        {
          if( dataGridContext.DataGridControl != null )
          {
            UIViewBase uiViewBase = dataGridContext.DataGridControl.GetView() as UIViewBase;
            if( ( uiViewBase != null ) && ( uiViewBase.ShowScrollTip ) )
              return true;
          }
        }

        return false;
      }
    }

    #endregion

    #region DefaultScrollTipContentTemplateBinding Private Property

    private Binding DefaultScrollTipContentTemplateBinding
    {
      get
      {
        if( m_defaultScrollTipContentTemplateBinding == null )
        {
          m_defaultScrollTipContentTemplateBinding = new Binding();
          m_defaultScrollTipContentTemplateBinding.Path = new PropertyPath( "(0).ScrollTipContentTemplate", DataGridControl.DataGridContextProperty );
          m_defaultScrollTipContentTemplateBinding.Source = this;
        }

        return m_defaultScrollTipContentTemplateBinding;
      }
    }

    private Binding m_defaultScrollTipContentTemplateBinding; // = null;

    #endregion

    #region DefaultScrollTipContentTemplateSelectorBinding Private Property

    private Binding DefaultScrollTipContentTemplateSelectorBinding
    {
      get
      {
        if( m_defaultScrollTipContentTemplateSelectorBinding == null )
        {
          m_defaultScrollTipContentTemplateSelectorBinding = new Binding();
          m_defaultScrollTipContentTemplateSelectorBinding.Path = new PropertyPath( "(0).ScrollTipContentTemplateSelector", DataGridControl.DataGridContextProperty );
          m_defaultScrollTipContentTemplateSelectorBinding.Source = this;
        }

        return m_defaultScrollTipContentTemplateSelectorBinding;
      }
    }


    private Binding m_defaultScrollTipContentTemplateSelectorBinding; // = null;

    #endregion

    #region DefaultCellContentTemplateBinding Private Property

    private Binding DefaultCellContentTemplateBinding
    {
      get
      {
        if( m_defaultCellContentTemplateBinding == null )
        {
          m_defaultCellContentTemplateBinding = new Binding();
          m_defaultCellContentTemplateBinding.Path = new PropertyPath( "(0).Columns.MainColumn.CellContentTemplate", DataGridControl.DataGridContextProperty );
          m_defaultCellContentTemplateBinding.Source = this;
        }

        return m_defaultCellContentTemplateBinding;
      }
    }

    private Binding m_defaultCellContentTemplateBinding;

    #endregion

    #region DefaultCellContentTemplateSelectorBinding Private Property

    private Binding DefaultCellContentTemplateSelectorBinding
    {
      get
      {
        if( m_defaultCellContentTemplateSelectorBinding == null )
        {
          m_defaultCellContentTemplateSelectorBinding = new Binding();
          m_defaultCellContentTemplateSelectorBinding.Path = new PropertyPath( "(0).Columns.MainColumn.CellContentTemplateSelector", DataGridControl.DataGridContextProperty );
          m_defaultCellContentTemplateSelectorBinding.Source = this;
        }

        return m_defaultCellContentTemplateSelectorBinding;
      }
    }

    private Binding m_defaultCellContentTemplateSelectorBinding;

    #endregion

    #region PRIVATE METHODS

    private static void OnDataGridContextPropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ScrollTip scrollTip = ( ScrollTip )sender;

      DataGridContext oldItemContext = e.OldValue as DataGridContext;
      DataGridContext newItemContext = e.NewValue as DataGridContext;

      if( scrollTip.m_mainColumn != null )
      {
        PropertyChangedEventManager.RemoveListener( scrollTip.m_mainColumn, scrollTip, Column.DisplayMemberBindingPropertyName );
      }

      scrollTip.m_mainColumn = null;

      if( oldItemContext != null )
      {
        PropertyChangedEventManager.RemoveListener( oldItemContext.Columns, scrollTip, ColumnCollection.MainColumnPropertyName );
      }

      scrollTip.ClearValue( ScrollTip.ContentProperty );

      if( newItemContext != null )
      {
        scrollTip.m_mainColumn = newItemContext.Columns.MainColumn;

        if( scrollTip.m_mainColumn != null )
        {
          PropertyChangedEventManager.AddListener( scrollTip.m_mainColumn, scrollTip, Column.DisplayMemberBindingPropertyName );
        }

        PropertyChangedEventManager.AddListener( newItemContext.Columns, scrollTip, ColumnCollection.MainColumnPropertyName );
      }

      if( !scrollTip.IsInParentGridChanged )
      {
        if( !scrollTip.ApplyTemplate() )
          scrollTip.RefreshDefaultScrollTipContentTemplate();
      }
    }

    private static void ParentGridControlPropertyChangedCallback( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl oldParentGridControl = e.OldValue as DataGridControl;
      DataGridControl parentGridControl = e.NewValue as DataGridControl;
      ScrollTip scrollTip = sender as ScrollTip;

      scrollTip.IsInParentGridChanged = true;
      try
      {
        if( oldParentGridControl != null )
        {
          if( scrollTip.UsingDefaultScrollTipContentTemplate )
          {
            // Unload DefaultTemplate
            Xceed.Wpf.DataGrid.Views.UIViewBase uiViewBase = oldParentGridControl.GetView() as Xceed.Wpf.DataGrid.Views.UIViewBase;

            if( uiViewBase != null )
            {
              uiViewBase.ClearValue( UIViewBase.ScrollTipContentTemplateProperty );
              scrollTip.UsingDefaultScrollTipContentTemplate = false;
            }
          }

          if( scrollTip.m_mainColumn != null )
          {
            PropertyChangedEventManager.RemoveListener( scrollTip.m_mainColumn, scrollTip, "DisplayMemberBinding" );
          }

          scrollTip.m_mainColumn = null;

          scrollTip.UnregisterListeners( oldParentGridControl );
        }

        if( parentGridControl == null )
          return;

        scrollTip.PrepareDefaultStyleKey( parentGridControl.GetView() );

        // Assert Template is applied in order to be notified for ScrollBars events
        DataGridControl.SetDataGridContext( scrollTip, parentGridControl.DataGridContext );

        if( !scrollTip.ApplyTemplate() )
          scrollTip.RefreshDefaultScrollTipContentTemplate();

        scrollTip.RegisterListeners( parentGridControl );
      }
      finally
      {
        scrollTip.IsInParentGridChanged = false;
      }
    }

    private void RegisterListeners( DataGridControl parentGridControl )
    {
      if( parentGridControl.ScrollViewer != null )
      {
        parentGridControl.ScrollViewer.ScrollChanged += new ScrollChangedEventHandler( this.OnScrollViewerScrollChanged );
        m_scrollViewerTemplateHelper = new ScrollViewerTemplateHelper( parentGridControl.ScrollViewer, this.DragScrollBegin, this.DragScrollEnd );
        m_scrollViewerTemplateHelper.RefreshTemplate();
      }
    }

    private void UnregisterListeners( DataGridControl parentGridControl )
    {
      if( parentGridControl.ScrollViewer != null )
      {
        parentGridControl.ScrollViewer.ScrollChanged -= new ScrollChangedEventHandler( this.OnScrollViewerScrollChanged );
      }

      if( m_scrollViewerTemplateHelper != null )
      {
        m_scrollViewerTemplateHelper.Dispose();
        m_scrollViewerTemplateHelper = null;
      }
    }

    private void RefreshDefaultScrollTipContentTemplate()
    {
      DataGridContext itemContext = DataGridControl.GetDataGridContext( this );

      if( itemContext == null )
        return;

      DataGridControl parentGridControl = itemContext.DataGridControl;

      if( ( parentGridControl == null ) || ( parentGridControl.ScrollViewer == null ) )
        return;

      ColumnCollection columnCollection = itemContext.Columns;

      if( columnCollection == null )
        return;

      Column mainColumn = columnCollection.MainColumn as Column;

      if( mainColumn == null )
        return;

      Xceed.Wpf.DataGrid.Views.UIViewBase uiViewBase = parentGridControl.GetView() as Xceed.Wpf.DataGrid.Views.UIViewBase;

      if( uiViewBase == null )
        return;

      // The ScrollTip.ContentTemplate will now be set. This is to avoid
      // a null ContentTemplate when the ColumnsCollection update its
      // MainColumn after the template is applied
      this.ScrollTipContentTemplateNeedsRefresh = false;

      ForeignKeyConfiguration configuration = mainColumn.ForeignKeyConfiguration;

      // Do not create default template only when none was created before and a template already exists
      if( ( !this.UsingDefaultScrollTipContentTemplate ) && ( uiViewBase.ScrollTipContentTemplate != null ) )
      {
        if( configuration != null )
        {
          this.ContentTemplate = configuration.DefaultScrollTipContentTemplate;
        }
        else
        {
          // Clear the value in case we previously affected it 
          this.ClearValue( ScrollTip.ContentTemplateProperty );

          // Set the default Binding values
          this.SetBinding( ScrollTip.ContentTemplateProperty, this.DefaultScrollTipContentTemplateBinding );
          this.SetBinding( ScrollTip.ContentTemplateSelectorProperty, this.DefaultScrollTipContentTemplateSelectorBinding );
        }
      }
      else
      {
        // A default ContentTemplate template is created using MainColumn as displayed data
        this.UsingDefaultScrollTipContentTemplate = true;

        // If a configuration was found, the default ContentTemplate will
        // be used to convert Content to Foreign value and
        // it will use the ScrollTipContentTemplate defined on UIViewBase
        // if any
        if( configuration == null )
        {
          // Disable warning for DisplayMemberBinding when internaly used
#pragma warning disable 618

          BindingBase displayMemberBinding = mainColumn.DisplayMemberBinding;

#pragma warning restore 618

          FrameworkElementFactory contentPresenter = new FrameworkElementFactory( typeof( ContentPresenter ) );
          contentPresenter.SetValue( ContentPresenter.NameProperty, "defaultScrollTipDataTemplateContentPresenter" );
          contentPresenter.SetBinding( ContentPresenter.ContentProperty, displayMemberBinding );

          contentPresenter.SetBinding( ContentPresenter.ContentTemplateProperty, this.DefaultCellContentTemplateBinding );
          contentPresenter.SetBinding( ContentPresenter.ContentTemplateSelectorProperty, this.DefaultCellContentTemplateSelectorBinding );

          DataTemplate template = new DataTemplate();
          template.VisualTree = contentPresenter;
          template.Seal();

          this.ContentTemplate = template;
        }
        else
        {
          this.SetBinding( ContentPresenter.ContentTemplateProperty, this.DefaultCellContentTemplateBinding );
          this.SetBinding( ContentPresenter.ContentTemplateSelectorProperty, this.DefaultCellContentTemplateSelectorBinding );
          this.ContentTemplate = configuration.DefaultScrollTipContentTemplate;
        }
      }
    }

    private void DragScrollBegin( Orientation orientation )
    {
      if( !this.ShouldDisplayScrollTip )
        return;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return;

      if( dataGridContext.DataGridControl == null )
        return;

      // Update items scrolling orientation and pixel scrolling
      m_itemsScrollingOrientation = ScrollViewerHelper.GetItemScrollingOrientation( dataGridContext.DataGridControl );

      if( m_itemsScrollingOrientation != orientation )
        return;

      this.Visibility = Visibility.Visible;

      this.IsPixelScrolling = ScrollViewerHelper.IsPixelScrolling( dataGridContext.DataGridControl, dataGridContext.DataGridControl.ItemsHost, dataGridContext.DataGridControl.ScrollViewer );

      this.RefreshScrollTipContent( m_scrollViewerTemplateHelper.GetScrollBar( orientation ) );
    }

    private void DragScrollEnd( Orientation orientation )
    {
      this.Visibility = Visibility.Collapsed;
    }

    private void OnScrollViewerScrollChanged( object sender, ScrollChangedEventArgs e )
    {
      if( this.Visibility != Visibility.Visible )
        return;

      if( !this.ShouldDisplayScrollTip )
        return;

      // Determine if we are scrolling horizontally or vertically
      ScrollBar scrollBar = null;

      if( e.VerticalChange == 0 )
      {
        scrollBar = m_scrollViewerTemplateHelper.HorizontalScrollBar;
      }
      else
      {
        scrollBar = m_scrollViewerTemplateHelper.VerticalScrollBar;
      }

      if( scrollBar == null )
        return;

      this.RefreshScrollTipContent( scrollBar );
    }

    private void RefreshScrollTipContent( ScrollBar scrollBar )
    {
      const int MaxItemToVisit = 100;

      var dataGridContext = DataGridControl.GetDataGridContext( this );
      if( dataGridContext == null )
        return;

      var parentGridControl = dataGridContext.DataGridControl;
      if( parentGridControl == null )
        return;

      if( scrollBar.Orientation != m_itemsScrollingOrientation )
        return;

      var maxOffset = 0d;

      if( scrollBar.Orientation == Orientation.Vertical )
      {
        maxOffset = parentGridControl.ScrollViewer.ExtentHeight;
      }
      else
      {
        maxOffset = parentGridControl.ScrollViewer.ExtentWidth;
      }

      var offset = scrollBar.Track.Value;

      var itemMaxOffset = 0;
      var itemOffset = 0;
      var itemHiddenRatio = 0d;

      if( this.IsPixelScrolling )
      {
        // Calculate the offset ratio to get the item from the Generator
        int itemsCount = parentGridControl.CustomItemContainerGenerator.ItemCount;
        itemMaxOffset = itemsCount;

        if( ( maxOffset > 0 ) && ( itemsCount > 0 ) )
        {
          var position = offset / ( maxOffset / itemsCount );
          itemOffset = ( int )position;
          itemHiddenRatio = position - itemOffset;
        }
        else
        {
          itemOffset = 0;
        }
      }
      else
      {
        itemMaxOffset = ( int )maxOffset;
        itemOffset = ( int )offset;
        itemHiddenRatio = offset - itemOffset;
      }

      object newValue;
      DataGridContext itemContext;

      // If data is grouped, we do not want to keep the next data item if a HeaderFooterItem
      // is the current item returned. So we increment the scroll up to the next item in order
      // to get the real item that will be visible when the user will release the mouse
      var visitor = new ItemContextVisitor( parentGridControl.ItemsHost as TableflowViewItemsHost, itemHiddenRatio );
      var endOffset = Math.Min( itemOffset + MaxItemToVisit, itemMaxOffset );
      bool visitWasStopped;
      ( ( IDataGridContextVisitable )parentGridControl.DataGridContext ).AcceptVisitor( itemOffset, endOffset, visitor, DataGridContextVisitorType.Items, out visitWasStopped );

      if( visitor.VisitSuccessful )
      {
        itemContext = visitor.ParentDataGridContext;
        newValue = visitor.Item;
      }
      else
      {
        //Set the ItemContext as the ScrollTip's DataGridContext, this is to ensure the
        //ScrollTip will not change the ScrollContentTemplate ( by invalidation of the binding).
        itemContext = DataGridControl.GetDataGridContext( this );
        newValue = null; //show nothing in the ScrollTip.
      }

      // The TippedItemDataGridContext PropertChanged callback will take care of refreshing the ScrollTip CellContentTemplate.
      if( itemContext != DataGridControl.GetDataGridContext( this ) )
      {
        DataGridControl.SetDataGridContext( this, itemContext );
      }

      this.Content = newValue;
    }

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      var dataGridContext = DataGridControl.GetDataGridContext( this );
      if( dataGridContext == null )
        return true;

      if( managerType == typeof( PropertyChangedEventManager ) )
      {
        if( dataGridContext.Columns == sender )
        {
          if( m_mainColumn != null )
          {
            PropertyChangedEventManager.RemoveListener( m_mainColumn, this, Column.DisplayMemberBindingPropertyName );
          }

          m_mainColumn = dataGridContext.Columns.MainColumn;

          if( m_mainColumn != null )
          {
            PropertyChangedEventManager.AddListener( m_mainColumn, this, Column.DisplayMemberBindingPropertyName );
          }

          // If a defaut template was created for the previous MainColumn
          // create another one for the new MainColumn
          if( this.UsingDefaultScrollTipContentTemplate || this.ScrollTipContentTemplateNeedsRefresh )
          {
            this.RefreshDefaultScrollTipContentTemplate();
          }
        }
        else if( sender == m_mainColumn )
        {
          // If a defaut template was created for the previous MainColumn
          // create another one
          if( this.UsingDefaultScrollTipContentTemplate || this.ScrollTipContentTemplateNeedsRefresh )
          {
            this.RefreshDefaultScrollTipContentTemplate();
          }
        }
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    #region PRIVATE FIELDS

    private ColumnBase m_mainColumn; // = null;
    private Orientation m_itemsScrollingOrientation = Orientation.Vertical;
    private ScrollViewerTemplateHelper m_scrollViewerTemplateHelper;

    private BitVector32 m_flags = new BitVector32();

    #endregion

    #region ScrollTipFlags Private Enum

    [Flags]
    private enum ScrollTipFlags
    {
      UsingDefaultScrollTipContentTemplate = 1,
      IsInParentGridChanged = 2,
      IsPixelScrolling = 4,
      ScrollTipContentTemplateNeedsRefresh = 8,
    }

    #endregion
  }
}
