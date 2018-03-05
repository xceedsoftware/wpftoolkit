/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Data;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  public class DataTableForeignKeyDescription : DataGridForeignKeyDescription
  {
    public DataTableForeignKeyDescription()
    {
    }

    internal DataTableForeignKeyDescription( ForeignKeyConstraint constraint )
      : this()
    {
      this.ForeignKeyConstraint = constraint;
    }

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
        return ( ForeignKeyConstraint )this.GetValue( DataTableForeignKeyDescription.ForeignKeyConstraintProperty );
      }
      set
      {
        this.SetValue( DataTableForeignKeyDescription.ForeignKeyConstraintProperty, value );
      }
    }

    private static void OnForeignKeyConstraintChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var foreignKeyDescription = sender as DataTableForeignKeyDescription;
      if( foreignKeyDescription != null )
      {
        foreignKeyDescription.UpdateValuePath();
        foreignKeyDescription.UpdateItemsSource();
      }
    }

    #endregion

    protected override void SetForeignKeyConverter()
    {
      this.ForeignKeyConverter = new DataTableForeignKeyConverter();
    }

    private void UpdateValuePath()
    {
      // Affect the ValuePath if it was not explicitly set or bound
      var valuePath = this.ReadLocalValue( ValuePathProperty );

      if( string.IsNullOrEmpty( this.ValuePath )
        && ( ( valuePath == DependencyProperty.UnsetValue )
             || ( valuePath == null ) ) )
      {
        var foreignKeyConstraint = this.ForeignKeyConstraint;

        // Affect the FieldName to the first RelatedColumn's name
        // if there is only one DataColumn in the RelatedColumns Collection
        if( ( foreignKeyConstraint != null )
          && ( foreignKeyConstraint.RelatedColumns != null )
          && ( foreignKeyConstraint.RelatedColumns.Length == 1 ) )
        {
          var foreignFieldName = foreignKeyConstraint.RelatedColumns[ 0 ].ColumnName;

          if( !string.IsNullOrEmpty( foreignFieldName ) )
          {
            this.ValuePath = foreignFieldName;
          }
        }
      }
    }

    private void UpdateItemsSource()
    {
      var foreignKeyConstraint = this.ForeignKeyConstraint;
      if( foreignKeyConstraint == null )
        return;

      var relatedTable = foreignKeyConstraint.RelatedTable;
      if( relatedTable == null )
        return;

      this.ItemsSource = relatedTable.DefaultView;
    }
  }
}
