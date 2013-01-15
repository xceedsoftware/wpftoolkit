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
using System.Collections.Generic;
using System.Text;
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
      DefaultStyleKeyProperty.OverrideMetadata( typeof( HierarchicalGroupLevelIndicatorPane ), new FrameworkPropertyMetadata( new Markup.ThemeKey( typeof( Views.TableView ), typeof( HierarchicalGroupLevelIndicatorPane ) ) ) );

      FocusableProperty.OverrideMetadata( typeof( HierarchicalGroupLevelIndicatorPane ), new FrameworkPropertyMetadata( false ) );

      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( HierarchicalGroupLevelIndicatorPane ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnParentDataGridControlChanged ) ) );
    }

    #region GroupLevelIndicatorPaneHost Read-Only Property

    private Panel GroupLevelIndicatorPaneHost
    {
      get
      {
        //if there is no local storage for the host panel, try to retrieve and store the value
        if( m_storedGroupLevelIndicatorPaneHost == null )
        {
          m_storedGroupLevelIndicatorPaneHost = this.RetrieveGroupLevelIndicatorPaneHostPanel();
        }

        return m_storedGroupLevelIndicatorPaneHost;
      }
    }

    private Panel m_storedGroupLevelIndicatorPaneHost = null;

    #endregion GroupLevelIndicatorPaneHost Read-Only Property

    #region GroupLevelIndicatorPaneNeedsRefresh Private Property

    private bool GroupLevelIndicatorPaneNeedsRefresh
    {
      get
      {
        Panel panel = this.GroupLevelIndicatorPaneHost;
        if( panel == null )
          return false;

        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );
        if( dataGridContext == null )
          return false;

        //skip the "current" DataGridContext
        dataGridContext = dataGridContext.ParentDataGridContext;

        int expectedIndicatorsCount = 0;
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
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( HierarchicalGroupLevelIndicatorPane ) );
    }

    protected override Size MeasureOverride( Size availableSize )
    {
      Panel panel = this.GroupLevelIndicatorPaneHost;

      if( panel != null )
      {
        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

        if( dataGridContext != null )
        {
          if( this.GroupLevelIndicatorPaneNeedsRefresh )
          {
            //clear all the panel's children!
            panel.Children.Clear();

            DataGridContext previousContext = dataGridContext;
            DataGridContext runningDataGridContext = dataGridContext.ParentDataGridContext;

            while( runningDataGridContext != null )
            {
              //create a GroupLevelIndicator to create indentation between the GLIPs
              FrameworkElement newGroupMargin = null;
              newGroupMargin = new DetailIndicator();
              newGroupMargin.DataContext = dataGridContext;

              object bindingSource = dataGridContext.GetDefaultDetailConfigurationForContext();
              if( bindingSource == null )
                bindingSource = dataGridContext.SourceDetailConfiguration;

              //Bind the GroupLevelIndicator`s style to the running DataGridContext`s GroupLevelIndicatorStyle.
              Binding groupLevelIndicatorStyleBinding = new Binding();
              groupLevelIndicatorStyleBinding.Path = new PropertyPath( DetailConfiguration.DetailIndicatorStyleProperty );
              groupLevelIndicatorStyleBinding.Source = bindingSource;
              newGroupMargin.SetBinding( StyleProperty, groupLevelIndicatorStyleBinding );

              //insert the Spacer GroupLevelIndicator in the panel
              panel.Children.Insert( 0, newGroupMargin );

              //then create the GLIP for the running DataGridContext
              GroupLevelIndicatorPane newSubGLIP = new GroupLevelIndicatorPane();
              DataGridControl.SetDataGridContext( newSubGLIP, runningDataGridContext );
              newSubGLIP.SetIsLeaf( false );
              GroupLevelIndicatorPane.SetGroupLevel( newSubGLIP, -1 );

              //and insert it in the panel.
              panel.Children.Insert( 0, newSubGLIP );

              previousContext = runningDataGridContext;
              runningDataGridContext = runningDataGridContext.ParentDataGridContext;
            } //end of the loop to cycle through the parent contexts.
          } // end if GroupLevelIndicatorPaneNeedsRefresh
        } // end if dataGridContext != null
      } //end if panel is not null

      return base.MeasureOverride( availableSize );
    }

    private static void OnParentDataGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      HierarchicalGroupLevelIndicatorPane self = sender as HierarchicalGroupLevelIndicatorPane;
      if( self != null )
      {
        DataGridControl grid = e.OldValue as DataGridControl;

        //unsubsribe from the old DataGridControl (the GLIP was disconnected)
        if( grid != null )
        {
          DetailsChangedEventManager.RemoveListener( grid, self );
        }

        grid = e.NewValue as DataGridControl;

        //register to the parent grid control's Items Collection GroupDescriptions Changed event
        if( grid != null )
        {
          self.PrepareDefaultStyleKey( grid.GetView() );

          DetailsChangedEventManager.AddListener( grid, self );
          self.InvalidateMeasure();
        }
      }
    }

    private Panel RetrieveGroupLevelIndicatorPaneHostPanel()
    {
      //get the template part
      return this.GetTemplateChild( "PART_GroupLevelIndicatorHost" ) as Panel;
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

