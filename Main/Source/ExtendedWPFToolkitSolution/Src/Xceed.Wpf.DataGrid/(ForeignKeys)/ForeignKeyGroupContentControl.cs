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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Windows.Data;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  internal class ForeignKeyGroupContentControl : ContentControl
  {
    #region Contructors

    static ForeignKeyGroupContentControl()
    {
      ForeignKeyGroupContentControl.GroupProperty = GroupHeaderControl.GroupProperty.AddOwner( typeof( ForeignKeyGroupContentControl ), new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( ForeignKeyGroupContentControl.OnGroupChanged ),
        new CoerceValueCallback( ForeignKeyGroupContentControl.OnCoerceGroupChanged ) ) );
    }

    public ForeignKeyGroupContentControl()
    {
      // Ensure this control is not Focusable, it only displays converted value between
      // ID and ForeignKey
      this.Focusable = false;
    }

    #endregion

    #region Group Internal Property

    internal static readonly DependencyProperty GroupProperty;

    internal Group Group
    {
      get
      {
        return GroupHeaderControl.GetGroup( this );
      }
    }

    private static object OnCoerceGroupChanged( DependencyObject o, object newValue )
    {
      ForeignKeyGroupContentControl contentControl = o as ForeignKeyGroupContentControl;

      if( contentControl != null )
      {
        Group currentGroup = contentControl.Group;
        Group newGroup = newValue as Group;
        bool clearContentTemplate = false;

        if( ( currentGroup != null ) && ( newGroup != null ) )
        {
          if( ( currentGroup.Level == -1 ) || ( newGroup.Level == -1 ) )
          {
            clearContentTemplate = true;
          }
          // The Group is different, must clear the templates
          else if( currentGroup.GroupBy != newGroup.GroupBy )
          {
            clearContentTemplate = true;
          }
        }
        else
        {
          // The Group is null or different, must clear the templates
          clearContentTemplate = true;
        }

        if( clearContentTemplate )
        {
          contentControl.ClearValue( ContentControl.ContentTemplateProperty );
          contentControl.ClearValue( ContentControl.ContentTemplateSelectorProperty );
        }
      }

      return newValue;
    }

    private static void OnGroupChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ForeignKeyGroupContentControl contentControl = sender as ForeignKeyGroupContentControl;

      if( contentControl != null )
      {
        contentControl.UpdateContent( e.NewValue as Group );
      }
    }

    #endregion

    #region Private Methods

    private void UpdateContent( Group group )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return;

      if( group == null )
        return;

      string groupBy = group.GroupBy;

      if( string.IsNullOrEmpty( groupBy ) )
        throw new DataGridInternalException( "Group.GroupBy is null." );

      Column column = dataGridContext.Columns[ groupBy ] as Column;

      object newContent = group.Value;

      if( column != null )
      {
        ForeignKeyConfiguration foreignKeyConfiguration = column.ForeignKeyConfiguration;

        if( foreignKeyConfiguration != null )
        {
          // Ensure to set the Content converted according to the DataContext
          // when a converter is present. Else, the DataContext will be displayed
          if( foreignKeyConfiguration.ForeignKeyConverter != null )
          {
            newContent = foreignKeyConfiguration.ForeignKeyConverter.GetValueFromKey(
                                                                        group.Value,
                                                                        foreignKeyConfiguration );
          }
        }
      }

      // We must update the template according
      // to values present on Column
      this.UpdateContentTemplate( column, group );

      this.Content = newContent;
    }

    private void UpdateContentTemplate( Column column, Group group )
    {
      if( column == null )
        return;

      bool contentTemplateAffected = false;
      bool contentTemplateSelectorAffected = false;

      if( column.GroupValueTemplate != null )
      {
        this.ContentTemplate = column.GroupValueTemplate;
        contentTemplateAffected = true;
      }
      else if( column.GroupValueTemplateSelector != null )
      {
        this.ContentTemplateSelector = column.GroupValueTemplateSelector;
        contentTemplateSelectorAffected = true;
      }

      if( !contentTemplateAffected && !contentTemplateSelectorAffected )
      {
        ForeignKeyConfiguration foreignKeyConfiguration = column.ForeignKeyConfiguration;
        if( foreignKeyConfiguration != null )
        {
          if( column.CellContentTemplate != null )
          {
            this.ContentTemplate = column.CellContentTemplate;
            contentTemplateAffected = true;
          }
          else if( column.CellContentTemplateSelector != null )
          {
            this.ContentTemplateSelector = column.CellContentTemplateSelector;
            contentTemplateSelectorAffected = true;
          }
        }
      }

      if( !contentTemplateAffected )
      {
        this.ClearValue( ContentPresenter.ContentTemplateProperty );
      }

      if( !contentTemplateSelectorAffected )
      {
        this.ClearValue( ContentPresenter.ContentTemplateSelectorProperty );
      }
    }

    #endregion
  }
}
