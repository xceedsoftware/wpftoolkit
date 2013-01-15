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
using System.Collections;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridForeignKeyDescription : DependencyObject
  {
    #region Constructors

    public DataGridForeignKeyDescription()
    {
    }

    #endregion

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
      new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( DataGridForeignKeyDescription.OnItemsSourceChanged ) ) );

    public IEnumerable ItemsSource
    {
      get
      {
        return m_itemsSource;
      }
      set
      {
        this.SetValue( DataGridForeignKeyDescription.ItemsSourceProperty, value );
      }
    }

    private IEnumerable m_itemsSource; // = null;

    private static void OnItemsSourceChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridForeignKeyDescription foreignKeyDescription = sender as DataGridForeignKeyDescription;

      if( foreignKeyDescription != null )
      {
        foreignKeyDescription.m_itemsSource = e.NewValue as IEnumerable;
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
  }
}
