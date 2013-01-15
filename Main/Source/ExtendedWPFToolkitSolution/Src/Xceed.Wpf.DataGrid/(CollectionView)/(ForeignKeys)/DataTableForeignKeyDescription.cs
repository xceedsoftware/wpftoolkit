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
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Windows;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  public class DataTableForeignKeyDescription : DataGridForeignKeyDescription
  {
    #region CONSTRUCTORS

    public DataTableForeignKeyDescription()
    {
      this.ForeignKeyConverter = new DataTableForeignKeyConverter();
    }

    internal DataTableForeignKeyDescription( ForeignKeyConstraint constraint )
      : this()
    {
      this.ForeignKeyConstraint = constraint;
    }

    #endregion

    #region ForeignKeyConstraint Property

    public static readonly DependencyProperty ForeignKeyConstraintProperty = DependencyProperty.Register(
      "ForeignKeyConstraint",
      typeof( ForeignKeyConstraint ),
      typeof( DataTableForeignKeyDescription ),
      new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( DataTableForeignKeyDescription.OnForeignKeyConstraintChanged ) ) );

    public ForeignKeyConstraint ForeignKeyConstraint
    {
      get
      {
        return m_foreignKeyConstraint;
      }
      set
      {
        this.SetValue( DataTableForeignKeyDescription.ForeignKeyConstraintProperty, value );
      }
    }

    private static void OnForeignKeyConstraintChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataTableForeignKeyDescription foreignKeyDescription = sender as DataTableForeignKeyDescription;

      if( foreignKeyDescription != null )
      {
        foreignKeyDescription.m_foreignKeyConstraint = e.NewValue as ForeignKeyConstraint;
        foreignKeyDescription.UpdateValuePath();
        foreignKeyDescription.UpdateItemsSource();
      }
    }

    private ForeignKeyConstraint m_foreignKeyConstraint;

    #endregion

    #region PRIVATE METHODS

    private void UpdateValuePath()
    {
      // Affect the ValuePath if it was not explicitly set or bound
      object valuePath = this.ReadLocalValue( ValuePathProperty );

      if( string.IsNullOrEmpty( this.ValuePath )
        && ( ( valuePath == DependencyProperty.UnsetValue )
             || ( valuePath == null ) ) )
      {
        // Affect the FieldName to the first RelatedColumn's name
        // if there is only one DataColumn in the RelatedColumns Collection
        if( ( m_foreignKeyConstraint != null )
          && ( m_foreignKeyConstraint.RelatedColumns != null )
          && ( m_foreignKeyConstraint.RelatedColumns.Length == 1 ) )
        {
          string foreignFieldName = m_foreignKeyConstraint.RelatedColumns[ 0 ].ColumnName;

          if( !string.IsNullOrEmpty( foreignFieldName ) )
          {
            this.ValuePath = foreignFieldName;
          }
        }
      }
    }

    private void UpdateItemsSource()
    {
      if( ( m_foreignKeyConstraint != null )
        && ( m_foreignKeyConstraint.RelatedTable != null ) )
      {
        this.ItemsSource = m_foreignKeyConstraint.RelatedTable.DefaultView;
      }
    }

    #endregion
  }
}
