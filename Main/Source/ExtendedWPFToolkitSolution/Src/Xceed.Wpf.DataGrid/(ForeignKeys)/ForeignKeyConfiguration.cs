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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using System.Collections.Specialized;
using System.Data;

namespace Xceed.Wpf.DataGrid
{
  public class ForeignKeyConfiguration : DependencyObject
  {
    #region Constructors

    static ForeignKeyConfiguration()
    {
      ForeignKeyConfiguration.DefaultDistinctValueItemContentTemplateProperty = ForeignKeyConfiguration.DefaultDistinctValueItemContentTemplatePropertyKey.DependencyProperty;
      ForeignKeyConfiguration.DefaultCellContentTemplateProperty = ForeignKeyConfiguration.DefaultCellContentTemplatePropertyKey.DependencyProperty;
      ForeignKeyConfiguration.DefaultGroupValueTemplateProperty = ForeignKeyConfiguration.DefaultGroupValueTemplatePropertyKey.DependencyProperty;
      ForeignKeyConfiguration.DefaultScrollTipContentTemplateProperty = ForeignKeyConfiguration.DefaultScrollTipContentTemplatePropertyKey.DependencyProperty;
      ForeignKeyConfiguration.DefaultCellEditorProperty = ForeignKeyConfiguration.DefaultCellEditorPropertyKey.DependencyProperty;
    }

    public ForeignKeyConfiguration()
    {
      this.SetDefaultDistinctValueItemContentTemplate( Column.GenericContentTemplateSelector.ForeignKeyDistinctValueItemContentTemplate );
      this.SetDefaultCellContentTemplate( Column.GenericContentTemplateSelector.ForeignKeyCellContentTemplate );
      this.SetDefaultGroupValueTemplate( Column.GenericContentTemplateSelector.ForeignKeyGroupValueTemplate );
      this.SetDefaultScrollTipContentTemplate( Column.GenericContentTemplateSelector.ForeignKeyScrollTipContentTemplate );
      this.SetDefaultCellEditor( DefaultCellEditorSelector.ForeignKeyCellEditor );
    }

    #endregion

    #region ValuePath Property

    public static readonly DependencyProperty ValuePathProperty = DependencyProperty.Register(
      "ValuePath",
      typeof( string ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( ForeignKeyConfiguration.OnSelectedValuePathChanged ) ) );

    public string ValuePath
    {
      get
      {
        return m_ValuePath;
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.ValuePathProperty, value );
      }
    }

    private string m_ValuePath; // = null;

    private static void OnSelectedValuePathChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ForeignKeyConfiguration foreignKeyConfiguration = sender as ForeignKeyConfiguration;

      if( foreignKeyConfiguration != null )
      {
        foreignKeyConfiguration.m_ValuePath = e.NewValue as string;
      }
    }

    #endregion

    #region DisplayMemberPath Property

    public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
      "DisplayMemberPath",
      typeof( string ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( ForeignKeyConfiguration.OnDisplayMemberPathChanged ) ) );

    public string DisplayMemberPath
    {
      get
      {
        return m_displayMemberPath;
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.DisplayMemberPathProperty, value );
      }
    }

    private string m_displayMemberPath; // = null;

    private static void OnDisplayMemberPathChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ForeignKeyConfiguration foreignKeyConfiguration = sender as ForeignKeyConfiguration;

      if( foreignKeyConfiguration != null )
      {
        foreignKeyConfiguration.m_displayMemberPath = e.NewValue as string;
      }
    }

    #endregion

    #region ItemContainerStyle Property

    public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(
      "ItemContainerStyle",
      typeof( Style ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( ForeignKeyConfiguration.OnItemContainerStyleChanged ) ) );

    public Style ItemContainerStyle
    {
      get
      {
        return m_itemContainerStyle;
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.ItemContainerStyleProperty, value );
      }
    }

    private Style m_itemContainerStyle; // = null;

    private static void OnItemContainerStyleChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ForeignKeyConfiguration foreignKeyConfiguration = sender as ForeignKeyConfiguration;

      if( foreignKeyConfiguration != null )
        foreignKeyConfiguration.m_itemContainerStyle = e.NewValue as Style;
    }

    #endregion

    #region ItemContainerStyleSelector Property

    public static readonly DependencyProperty ItemContainerStyleSelectorProperty = DependencyProperty.Register(
      "ItemContainerStyleSelector",
      typeof( StyleSelector ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( ForeignKeyConfiguration.OnItemContainerStyleSelectorChanged ) ) );

    public StyleSelector ItemContainerStyleSelector
    {
      get
      {
        return m_itemContainerStyleSelector;
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.ItemContainerStyleSelectorProperty, value );
      }
    }

    private StyleSelector m_itemContainerStyleSelector; // = null;

    private static void OnItemContainerStyleSelectorChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ForeignKeyConfiguration foreignKeyConfiguration = sender as ForeignKeyConfiguration;

      if( foreignKeyConfiguration != null )
      {
        foreignKeyConfiguration.m_itemContainerStyleSelector = e.NewValue as StyleSelector;
      }
    }

    #endregion

    #region IsAutoCreated Property

    internal bool IsAutoCreated
    {
      get;
      set;
    }

    #endregion

    #region ItemsSource Property

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
      "ItemsSource",
      typeof( IEnumerable ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( ForeignKeyConfiguration.OnItemsSourceChanged ) ) );

    public IEnumerable ItemsSource
    {
      get
      {
        return m_itemsSource;

      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.ItemsSourceProperty, value );
      }
    }

    private IEnumerable m_itemsSource;

    private static void OnItemsSourceChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ForeignKeyConfiguration foreignConfiguration = sender as ForeignKeyConfiguration;

      if( foreignConfiguration != null )
      {
        foreignConfiguration.m_itemsSource = e.NewValue as IEnumerable;
      }
    }

    #endregion

    #region ForeignKeyConverter Property

    public static readonly DependencyProperty ForeignKeyConverterProperty = DependencyProperty.Register(
      "ForeignKeyConverter",
      typeof( ForeignKeyConverter ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( null ) );

    public ForeignKeyConverter ForeignKeyConverter
    {
      get
      {
        return ( ForeignKeyConverter )this.GetValue( ForeignKeyConfiguration.ForeignKeyConverterProperty );
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.ForeignKeyConverterProperty, value );
      }
    }

    #endregion

    #region DefaultDistinctValueItemContentTemplate Property

    private static readonly DependencyPropertyKey DefaultDistinctValueItemContentTemplatePropertyKey = DependencyProperty.RegisterReadOnly(
      "DefaultDistinctValueItemContentTemplate",
      typeof( DataTemplate ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( null ) );

    internal static readonly DependencyProperty DefaultDistinctValueItemContentTemplateProperty;

    internal DataTemplate DefaultDistinctValueItemContentTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( ForeignKeyConfiguration.DefaultDistinctValueItemContentTemplateProperty );
      }
    }

    private void SetDefaultDistinctValueItemContentTemplate( DataTemplate value )
    {
      this.SetValue( ForeignKeyConfiguration.DefaultDistinctValueItemContentTemplatePropertyKey, value );
    }

    private void ClearDefaultDistinctValueItemContentTemplate()
    {
      this.ClearValue( ForeignKeyConfiguration.DefaultDistinctValueItemContentTemplatePropertyKey );
    }

    #endregion

    #region DefaultCellContentTemplate Internal Property

    private static readonly DependencyPropertyKey DefaultCellContentTemplatePropertyKey = DependencyProperty.RegisterReadOnly(
      "DefaultCellContentTemplate",
      typeof( DataTemplate ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( null ) );

    internal static readonly DependencyProperty DefaultCellContentTemplateProperty;

    internal DataTemplate DefaultCellContentTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( ForeignKeyConfiguration.DefaultCellContentTemplateProperty );
      }
    }

    private void SetDefaultCellContentTemplate( DataTemplate value )
    {
      this.SetValue( ForeignKeyConfiguration.DefaultCellContentTemplatePropertyKey, value );
    }

    private void ClearDefaultCellContentTemplate()
    {
      this.ClearValue( ForeignKeyConfiguration.DefaultCellContentTemplatePropertyKey );
    }

    #endregion

    #region DefaultGroupValueTemplate Internal Property

    private static readonly DependencyPropertyKey DefaultGroupValueTemplatePropertyKey = DependencyProperty.RegisterReadOnly(
      "DefaultGroupValueTemplate",
      typeof( DataTemplate ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( null ) );

    internal static readonly DependencyProperty DefaultGroupValueTemplateProperty;


    internal DataTemplate DefaultGroupValueTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( ForeignKeyConfiguration.DefaultGroupValueTemplateProperty );
      }
    }

    private void SetDefaultGroupValueTemplate( DataTemplate value )
    {
      this.SetValue( ForeignKeyConfiguration.DefaultGroupValueTemplatePropertyKey, value );
    }

    private void ClearDefaultGroupValueTemplate()
    {
      this.ClearValue( ForeignKeyConfiguration.DefaultGroupValueTemplatePropertyKey );
    }

    #endregion

    #region DefaultScrollTipContentTemplate Property

    private static readonly DependencyPropertyKey DefaultScrollTipContentTemplatePropertyKey = DependencyProperty.RegisterReadOnly(
      "DefaultScrollTipContentTemplate",
      typeof( DataTemplate ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( null ) );

    internal static readonly DependencyProperty DefaultScrollTipContentTemplateProperty;

    internal DataTemplate DefaultScrollTipContentTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( ForeignKeyConfiguration.DefaultScrollTipContentTemplateProperty );
      }
    }

    private void SetDefaultScrollTipContentTemplate( DataTemplate value )
    {
      this.SetValue( ForeignKeyConfiguration.DefaultScrollTipContentTemplatePropertyKey, value );
    }

    private void ClearDefaultScrollTipContentTemplate()
    {
      this.ClearValue( ForeignKeyConfiguration.DefaultScrollTipContentTemplatePropertyKey );
    }

    #endregion

    #region DefaultCellEditor Internal Property

    private static readonly DependencyPropertyKey DefaultCellEditorPropertyKey = DependencyProperty.RegisterReadOnly(
      "DefaultCellEditor",
      typeof( CellEditor ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( null ) );

    internal static readonly DependencyProperty DefaultCellEditorProperty;


    internal CellEditor DefaultCellEditor
    {
      get
      {
        return ( CellEditor )this.GetValue( ForeignKeyConfiguration.DefaultCellEditorProperty );
      }
    }

    private void SetDefaultCellEditor( CellEditor value )
    {
      this.SetValue( ForeignKeyConfiguration.DefaultCellEditorPropertyKey, value );
    }

    private void ClearDefaultCellEditor()
    {
      this.ClearValue( ForeignKeyConfiguration.DefaultCellEditorPropertyKey );
    }

    #endregion

    #region Static Methods

    internal static void UpdateColumnsForeignKeyConfigurations(
      Dictionary<string,ColumnBase> columns,
      IEnumerable itemsSourceCollection,
      Dictionary<string, ItemsSourceHelper.FieldDescriptor> fieldDescriptors,
      bool autoCreateForeignKeyConfigurations )
    {
      DataGridCollectionViewBase collectionViewBase =
        itemsSourceCollection as DataGridCollectionViewBase;

      if( collectionViewBase != null )
      {
        ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView(
          columns,
          collectionViewBase.ItemProperties,
          autoCreateForeignKeyConfigurations );
      }
      else
      {
        ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromFieldDescriptors(
          columns,
          fieldDescriptors,
          autoCreateForeignKeyConfigurations );
      }
    }

    // If a DataGridCollectionViewBase is used, get the ItemProperties it defines
    // to be able to retreive DataGridForeignKeyDescription for each of them
    // to get the auto-detected ForeignKey ItemsSource (if any).
    // If a DataGridCollectionViewBase is not used, the ItemsSource must be 
    // manually specified on each Column in order to correctly display/edit the Data
    internal static void UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView(
      Dictionary<string,ColumnBase> columns,
      DataGridItemPropertyCollection itemProperties,
      bool autoCreateForeignKeyConfigurations )
    {
      if( itemProperties == null )
        return;

      foreach( DataGridItemPropertyBase itemProperty in itemProperties )
      {
        DataGridForeignKeyDescription description = itemProperty.ForeignKeyDescription;

        if( description == null )
          continue;

        ColumnBase column;
        columns.TryGetValue( itemProperty.Name, out column );

        ForeignKeyConfiguration.SynchronizeForeignKeyConfigurationFromForeignKeyDescription(
          column as Column,
          description,
          autoCreateForeignKeyConfigurations );
      }
    }

    private static void UpdateColumnsForeignKeyConfigurationsFromFieldDescriptors(
      Dictionary<string, ColumnBase> columns,
      Dictionary<string, ItemsSourceHelper.FieldDescriptor> fieldDescriptors,
      bool autoCreateForeignKeyConfigurations )
    {
      if( columns == null )
        return;

      if( fieldDescriptors == null )
        return;

      foreach( ItemsSourceHelper.FieldDescriptor fieldDescriptor in fieldDescriptors.Values )
      {
        DataGridForeignKeyDescription description = fieldDescriptor.ForeignKeyDescription;

        if( description == null )
          continue;

        ColumnBase column;
        columns.TryGetValue( fieldDescriptor.Name, out column );

        ForeignKeyConfiguration.SynchronizeForeignKeyConfigurationFromForeignKeyDescription(
          column as Column,
          description,
          autoCreateForeignKeyConfigurations );
      }
    }

    internal static void SynchronizeForeignKeyConfigurationFromForeignKeyDescription(
      Column column,
      DataGridForeignKeyDescription description,
      bool autoCreateForeignKeyConfigurations )
    {
      if( ( description == null ) || ( column == null ) )
        return;

      ForeignKeyConfiguration configuration = column.ForeignKeyConfiguration;

      if( configuration == null )
      {
        if( !autoCreateForeignKeyConfigurations )
          return;

        configuration = new ForeignKeyConfiguration();
        configuration.IsAutoCreated = true;
        column.ForeignKeyConfiguration = configuration;
      }

      // ValuePath will be affected to the FieldName when the 
      // configuration is auto-created to be able to modify 
      // local source using foreign key value
      if( configuration.IsAutoCreated )
      {
        if( string.IsNullOrEmpty( configuration.ValuePath ) )
        {
          configuration.ValuePath = description.ValuePath;
        }
      }

      // Affect the ItemsSource on the configuration if it is not
      // already set
      if( ( configuration.ItemsSource == null )
        && ( description.ItemsSource != null ) )
      {
        configuration.ItemsSource = description.ItemsSource;
      }

      // Set the Converter if it was not locally set
      if( configuration.ForeignKeyConverter == null )
      {
        configuration.ForeignKeyConverter = description.ForeignKeyConverter;
      }
    }

    #endregion
  }
}
