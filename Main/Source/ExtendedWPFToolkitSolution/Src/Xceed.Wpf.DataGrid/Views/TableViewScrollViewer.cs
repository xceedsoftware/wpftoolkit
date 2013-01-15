/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Utils.Wpf;
using System.Windows.Data;
using Xceed.Wpf.DataGrid.Converters;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid.Views
{
  [TemplatePart( Name = "PART_RowSelectorPane", Type = typeof( RowSelectorPane ) )]
  public class TableViewScrollViewer : DataGridScrollViewer
  {
    static TableViewScrollViewer()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( TableViewScrollViewer ), new FrameworkPropertyMetadata( typeof( TableViewScrollViewer ) ) );
      StoredFixedTransformProperty = StoredFixedTransformPropertyKey.DependencyProperty;
    }

    public TableViewScrollViewer()
    {
      ExecutedRoutedEventHandler executedRoutedEventHandler = new ExecutedRoutedEventHandler( this.OnScrollCommand );
      CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = new CanExecuteRoutedEventHandler( this.OnQueryScrollCommand );

      this.CommandBindings.Add(
        new CommandBinding( ScrollBar.PageLeftCommand,
        executedRoutedEventHandler,
        canExecuteRoutedEventHandler ) );

      this.CommandBindings.Add(
        new CommandBinding( ScrollBar.PageRightCommand,
        executedRoutedEventHandler,
        canExecuteRoutedEventHandler ) );
    }

    #region RowSelectorPaneWidth Property

    public static readonly DependencyProperty RowSelectorPaneWidthProperty =
        DependencyProperty.Register(
          "RowSelectorPaneWidth",
          typeof( double ),
          typeof( TableViewScrollViewer ),
          new FrameworkPropertyMetadata( 20d ) );

    public double RowSelectorPaneWidth
    {
      get
      {
        return ( double )this.GetValue( RowSelectorPaneWidthProperty );
      }
      set
      {
        this.SetValue( RowSelectorPaneWidthProperty, value );
      }
    }

    #endregion RowSelectorPaneWidth Property

    #region ShowRowSelectorPane Property

    public static readonly DependencyProperty ShowRowSelectorPaneProperty =
        DependencyProperty.Register(
          "ShowRowSelectorPane",
          typeof( bool ),
          typeof( TableViewScrollViewer ),
          new FrameworkPropertyMetadata( true ) );

    public bool ShowRowSelectorPane
    {
      get
      {
        return ( bool )this.GetValue( ShowRowSelectorPaneProperty );
      }
      set
      {
        this.SetValue( ShowRowSelectorPaneProperty, value );
      }
    }

    #endregion ShowRowSelectorPane Property

    #region StoredFixedTransform Attached Property

    internal static readonly DependencyPropertyKey StoredFixedTransformPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly( "StoredFixedTransform",
        typeof( Transform ),
        typeof( TableViewScrollViewer ),
        new UIPropertyMetadata( null ) );

    internal static readonly DependencyProperty StoredFixedTransformProperty;

    internal static Transform GetStoredFixedTransform( ScrollViewer obj )
    {
      Transform actualValue = ( Transform )obj.GetValue( TableViewScrollViewer.StoredFixedTransformProperty );

      if( actualValue == null )
      {
        actualValue = TableViewScrollViewer.CreateFixedPanelTransform( obj );
        TableViewScrollViewer.SetStoredFixedTransform( obj, actualValue );
      }

      return actualValue;
    }

    internal static void SetStoredFixedTransform( ScrollViewer obj, Transform value )
    {
      obj.SetValue( TableViewScrollViewer.StoredFixedTransformPropertyKey, value );
    }

    #endregion StoredFixedTransform Attached Property

    #region RowSelectorPane Property

    internal RowSelectorPane RowSelectorPane
    {
      get
      {
        return m_rowSelectorPane;
      }
    }

    #endregion

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      m_rowSelectorPane = this.Template.FindName( "PART_RowSelectorPane", this ) as RowSelectorPane;
    }

    internal static void SetFixedTranslateTransform( UIElement element, bool canScrollHorizontally )
    {
      if( element == null )
        throw new ArgumentNullException( "element" );

      ScrollViewer parentScrollViewer = TableViewScrollViewer.GetParentScrollViewer( element ) as ScrollViewer;

      if( parentScrollViewer != null )
      {
        Transform fixedTransform = TableViewScrollViewer.GetStoredFixedTransform( parentScrollViewer );

        Debug.Assert( fixedTransform != null );

        if( canScrollHorizontally )
        {
          if( element.RenderTransform == fixedTransform )
          {
            element.ClearValue( UIElement.RenderTransformProperty );
          }
        }
        else
        {
          element.RenderTransform = fixedTransform;
        }
      }
    }

    internal static ScrollViewer GetParentScrollViewer( DependencyObject obj )
    {
      DependencyObject parent = TreeHelper.GetParent( obj );
      ScrollViewer parentScrollViewer = parent as ScrollViewer;

      while( ( parent != null ) && ( parentScrollViewer == null ) )
      {
        parent = TreeHelper.GetParent( parent );
        parentScrollViewer = parent as ScrollViewer;
      }

      return parentScrollViewer;
    }

    internal static TableViewScrollViewer GetParentTableViewScrollViewer( DependencyObject obj )
    {
      DependencyObject parent = TreeHelper.GetParent( obj );
      TableViewScrollViewer parentScrollViewer = parent as TableViewScrollViewer;

      while( ( parent != null ) && ( parentScrollViewer == null ) )
      {
        parent = TreeHelper.GetParent( parent );
        parentScrollViewer = parent as TableViewScrollViewer;
      }

      return parentScrollViewer;
    }

    internal static Transform CreateFixedPanelTransform( ScrollViewer parentScrollViewer )
    {
      TranslateTransform newTransform = new TranslateTransform();

      Binding horizontalOffsetBinding = new Binding();
      horizontalOffsetBinding.Source = parentScrollViewer;
      horizontalOffsetBinding.Path = new PropertyPath( ScrollViewer.HorizontalOffsetProperty );

      BindingOperations.SetBinding( newTransform, TranslateTransform.XProperty, horizontalOffsetBinding );

      return newTransform;
    }

    private void OnQueryScrollCommand( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = true;
    }

    private void OnScrollCommand( object sender, ExecutedRoutedEventArgs e )
    {
      double scrollingViewport = this.ViewportWidth;
      FixedCellPanel cellPanel = this.LocateFixedCellPanel( this );

      if( cellPanel != null )
        scrollingViewport -= cellPanel.GetFixedWidth();

      if( e.Command == ScrollBar.PageLeftCommand )
      {
        double newOffset = Math.Max( this.HorizontalOffset - scrollingViewport, 0d );
        this.ScrollToHorizontalOffset( newOffset );
        e.Handled = true;
      }
      else if( e.Command == ScrollBar.PageRightCommand )
      {
        double newOffset = Math.Min( this.HorizontalOffset + scrollingViewport, this.ScrollableWidth );
        this.ScrollToHorizontalOffset( newOffset );
        e.Handled = true;
      }
      else
      {
        System.Diagnostics.Debug.Assert( false );
      }
    }

    private FixedCellPanel LocateFixedCellPanel( DependencyObject obj )
    {
      DependencyObject child;
      FixedCellPanel foundPanel;

      for( int i = VisualTreeHelper.GetChildrenCount( obj ) - 1; i >= 0; i-- )
      {
        child = VisualTreeHelper.GetChild( obj, i );

        foundPanel = child as FixedCellPanel;

        if( foundPanel != null )
          return foundPanel;

        foundPanel = this.LocateFixedCellPanel( child );

        if( foundPanel != null )
        {
          // The FixedCellPanel found can be one of the recycled container in the visual tree
          DataGridContext dataGridContext = DataGridControl.GetDataGridContext( foundPanel );

          if( dataGridContext != null )
          {
            if( dataGridContext.GetItemFromContainer( foundPanel ) != null )
              return foundPanel;
          }
        }
      }

      return null;
    }


    RowSelectorPane m_rowSelectorPane;
  }
}
