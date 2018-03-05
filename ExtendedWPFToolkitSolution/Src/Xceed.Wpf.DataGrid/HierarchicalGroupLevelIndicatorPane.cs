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

namespace Xceed.Wpf.DataGrid
{
  public class HierarchicalGroupLevelIndicatorPane : Control, IWeakEventListener
  {
    static HierarchicalGroupLevelIndicatorPane()
    {
      // This DefaultStyleKey will only be used in design-time.
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( HierarchicalGroupLevelIndicatorPane ), new FrameworkPropertyMetadata( new Markup.ThemeKey( typeof( Views.TableView ), typeof( HierarchicalGroupLevelIndicatorPane ) ) ) );

      UIElement.FocusableProperty.OverrideMetadata( typeof( HierarchicalGroupLevelIndicatorPane ), new FrameworkPropertyMetadata( false ) );

      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( HierarchicalGroupLevelIndicatorPane ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnParentDataGridControlChanged ) ) );
      DataGridControl.DataGridContextPropertyKey.OverrideMetadata( typeof( HierarchicalGroupLevelIndicatorPane ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnDataGridContextChanged ) ) );
    }

    #region GroupLevelIndicatorPaneHost Private Property

    private Panel GroupLevelIndicatorPaneHost
    {
      get
      {
        //if there is no local storage for the host panel, try to retrieve and store the value
        if( m_storedGroupLevelIndicatorPaneHost == null )
        {
          m_storedGroupLevelIndicatorPaneHost = this.GetTemplateChild( "PART_GroupLevelIndicatorHost" ) as Panel;
        }

        return m_storedGroupLevelIndicatorPaneHost;
      }
    }

    private Panel m_storedGroupLevelIndicatorPaneHost = null;

    #endregion

    #region GroupLevelIndicatorPaneNeedsRefresh Private Property

    private bool GroupLevelIndicatorPaneNeedsRefresh
    {
      get
      {
        var panel = this.GroupLevelIndicatorPaneHost;
        if( panel == null )
          return false;

        var dataGridContext = DataGridControl.GetDataGridContext( this );
        if( dataGridContext == null )
          return false;

        //skip the "current" DataGridContext
        dataGridContext = dataGridContext.ParentDataGridContext;

        var expectedIndicatorsCount = 0;
        for( ; dataGridContext != null; dataGridContext = dataGridContext.ParentDataGridContext )
        {
          //a group indicator and a detail indicator should exist per level
          expectedIndicatorsCount += 2;
        }

        return ( panel.Children.Count != expectedIndicatorsCount );
      }
    }

    #endregion

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      //whenever the template gets "applied" I want to invalidate the stored Panel.
      m_storedGroupLevelIndicatorPaneHost = null;
    }

    internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      var newThemeKey = view.GetDefaultStyleKey( typeof( HierarchicalGroupLevelIndicatorPane ) );
      if( object.Equals( this.DefaultStyleKey, newThemeKey ) )
        return;

      this.DefaultStyleKey = newThemeKey;
    }

    protected override Size MeasureOverride( Size availableSize )
    {
      var panel = this.GroupLevelIndicatorPaneHost;
      if( panel == null )
        return base.MeasureOverride( availableSize );

      var dataGridContext = DataGridControl.GetDataGridContext( this );
      if( ( dataGridContext == null ) || !this.GroupLevelIndicatorPaneNeedsRefresh )
        return base.MeasureOverride( availableSize );

      //clear all the panel's children!
      panel.Children.Clear();

      var previousContext = dataGridContext;
      var runningDataGridContext = dataGridContext.ParentDataGridContext;

      while( runningDataGridContext != null )
      {
        //create a GroupLevelIndicator to create indentation between the GLIPs
        FrameworkElement newGroupMargin = new DetailIndicator();
        newGroupMargin.DataContext = dataGridContext;

        object bindingSource = dataGridContext.GetDefaultDetailConfigurationForContext();
        if( bindingSource == null )
        {
          bindingSource = dataGridContext.SourceDetailConfiguration;
        }

        //Bind the GroupLevelIndicator`s style to the running DataGridContext`s GroupLevelIndicatorStyle.
        var groupLevelIndicatorStyleBinding = new Binding();
        groupLevelIndicatorStyleBinding.Path = new PropertyPath( DetailConfiguration.DetailIndicatorStyleProperty );
        groupLevelIndicatorStyleBinding.Source = bindingSource;
        newGroupMargin.SetBinding( StyleProperty, groupLevelIndicatorStyleBinding );

        //insert the Spacer GroupLevelIndicator in the panel
        panel.Children.Insert( 0, newGroupMargin );

        if( !runningDataGridContext.AreDetailsFlatten )
        {
          //then create the GLIP for the running DataGridContext
          var newSubGLIP = new GroupLevelIndicatorPane();
          DataGridControl.SetDataGridContext( newSubGLIP, runningDataGridContext );
          newSubGLIP.SetIsLeaf( false );
          GroupLevelIndicatorPane.SetGroupLevel( newSubGLIP, -1 );

          //and insert it in the panel.
          panel.Children.Insert( 0, newSubGLIP );
        }

        previousContext = runningDataGridContext;
        runningDataGridContext = runningDataGridContext.ParentDataGridContext;
      }

      return base.MeasureOverride( availableSize );
    }

    private static void OnParentDataGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as HierarchicalGroupLevelIndicatorPane;
      if( self == null )
        return;

      var dataGridControl = e.OldValue as DataGridControl;
      if( dataGridControl != null )
      {
        DetailsChangedEventManager.RemoveListener( dataGridControl, self );
      }

      dataGridControl = e.NewValue as DataGridControl;

      //register to the parent grid control's Items Collection GroupDescriptions Changed event
      if( dataGridControl != null )
      {
        self.PrepareDefaultStyleKey( dataGridControl.GetView() );

        DetailsChangedEventManager.AddListener( dataGridControl, self );
        self.InvalidateMeasure();
      }
    }

    private static void OnDataGridContextChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as HierarchicalGroupLevelIndicatorPane;
      if( self == null )
        return;

      var panel = self.GroupLevelIndicatorPaneHost;
      if( panel != null )
      {
        panel.Children.Clear();
      }

      self.InvalidateMeasure();
    }

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( DetailsChangedEventManager ) )
      {
        this.InvalidateMeasure();
        return true;
      }

      return false;
    }

    #endregion
  }
}

