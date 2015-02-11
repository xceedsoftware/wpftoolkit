/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridForeignKeyDescription : DependencyObject
  {
    public DataGridForeignKeyDescription()
    {
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
  }
}
