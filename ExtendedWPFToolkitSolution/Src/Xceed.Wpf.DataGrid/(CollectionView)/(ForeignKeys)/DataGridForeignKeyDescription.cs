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
using System.Collections;
using System.Data;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridForeignKeyDescription : DependencyObject
  {
    public DataGridForeignKeyDescription()
    {
      this.SetForeignKeyConverter();
    }

    #region ForeignKeyConverter Property

    public static readonly DependencyProperty ForeignKeyConverterProperty = DependencyProperty.Register(
      "ForeignKeyConverter",
      typeof( ForeignKeyConverter ),
      typeof( DataGridForeignKeyDescription ),
      new FrameworkPropertyMetadata( null ) );

    public ForeignKeyConverter ForeignKeyConverter
    {
      get
      {
        return ( ForeignKeyConverter )this.GetValue( DataGridForeignKeyDescription.ForeignKeyConverterProperty );
      }
      set
      {
        this.SetValue( DataGridForeignKeyDescription.ForeignKeyConverterProperty, value );
      }
    }

    #endregion

    #region ItemsSource Property

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
      "ItemsSource",
      typeof( IEnumerable ),
      typeof( DataGridForeignKeyDescription ),
      new FrameworkPropertyMetadata( null ) );

    public IEnumerable ItemsSource
    {
      get
      {
        return ( IEnumerable )this.GetValue( DataGridForeignKeyDescription.ItemsSourceProperty );
      }
      set
      {
        this.SetValue( DataGridForeignKeyDescription.ItemsSourceProperty, value );
      }
    }

    #endregion

    #region IsAutoCreated Internal Property

    internal bool IsAutoCreated
    {
      get;
      set;
    }

    #endregion

    #region ValuePath Property

    public static readonly DependencyProperty ValuePathProperty = DependencyProperty.Register(
      "ValuePath",
      typeof( string ),
      typeof( DataGridForeignKeyDescription ),
      new FrameworkPropertyMetadata( null ) );

    public string ValuePath
    {
      get
      {
        return ( string )this.GetValue( DataGridForeignKeyDescription.ValuePathProperty );
      }
      set
      {
        this.SetValue( DataGridForeignKeyDescription.ValuePathProperty, value );
      }
    }

    #endregion

    #region DisplayMemberPath Property

    public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
      "DisplayMemberPath",
      typeof( string ),
      typeof( DataGridForeignKeyDescription ),
      new FrameworkPropertyMetadata( null ) );

    public string DisplayMemberPath
    {
      get
      {
        return ( string )this.GetValue( DataGridForeignKeyDescription.DisplayMemberPathProperty );
      }
      set
      {
        this.SetValue( DataGridForeignKeyDescription.DisplayMemberPathProperty, value );
      }
    }

    #endregion

    protected virtual void SetForeignKeyConverter()
    {
      this.ForeignKeyConverter = new DataGridForeignKeyConverter();
    }

    internal Type GetDataType()
    {
      var itemsSource = this.ItemsSource;
      if( itemsSource == null )
        return null;

      var displayMemberPath = this.DisplayMemberPath;
      var foreignKeyConverter = this.ForeignKeyConverter;

      if( string.IsNullOrWhiteSpace( displayMemberPath ) && ( foreignKeyConverter == null ) )
        return null;

      try
      {
        //Use the DisplayMemberPath or the ForeignKeyConverter to find the converted value data type, using a DataRowView or reflection.
        if( ( itemsSource is DataView ) || ( itemsSource is DataTable ) )
        {
          foreach( object item in itemsSource )
          {
            var dataRowView = item as DataRowView;
            if( dataRowView == null )
              continue;

            if( !string.IsNullOrWhiteSpace( displayMemberPath ) )
            {
              var dataColumn = dataRowView.Row.Table.Columns[ displayMemberPath ];
              if( dataColumn != null )
              {
                return dataColumn.DataType;
              }
            }
            else
            {
              var valuePath = this.ValuePath;
              if( valuePath != null )
              {
                var key = dataRowView[ valuePath ];
                var value = foreignKeyConverter.GetValueFromKey( key, this );
                if( value != null )
                {
                  return value.GetType();
                }
              }
            }
          }
        }
        else
        {
          foreach( object item in itemsSource )
          {
            if( item == null )
              continue;

            if( !string.IsNullOrWhiteSpace( displayMemberPath ) )
              return item.GetType().GetProperty( displayMemberPath ).PropertyType;

            if( item is Enum )
              return item.GetType();

            var valuePath = this.ValuePath;
            if( valuePath != null )
            {
              var key = item.GetType().GetProperty( valuePath ).GetValue( item, null );
              var value = foreignKeyConverter.GetValueFromKey( key, this );
              if( value != null )
              {
                return value.GetType();
              }
            }
          }
        }
      }
      catch
      {
        //Swallow the exception, no need to terminate the application, since the original value will be exported.
      }

      return null;
    }

    internal object GetDisplayValue( object fieldValue )
    {
      if( fieldValue == null )
        return null;

      var foreignKeyConverter = this.ForeignKeyConverter;
      if( foreignKeyConverter != null )
        return foreignKeyConverter.GetValueFromKey( fieldValue, this );

      return fieldValue;
    }
  }
}
