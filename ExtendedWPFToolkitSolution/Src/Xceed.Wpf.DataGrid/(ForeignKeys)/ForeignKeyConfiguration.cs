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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  public class ForeignKeyConfiguration : DependencyObject, IWeakEventListener
  {
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
      this.SetDefaultCellContentTemplate( GenericContentTemplateSelector.ForeignKeyCellContentTemplate );
      this.SetDefaultGroupValueTemplate( GenericContentTemplateSelector.ForeignKeyGroupValueTemplate );
      this.SetDefaultScrollTipContentTemplate( GenericContentTemplateSelector.ForeignKeyScrollTipContentTemplate );
      this.SetDefaultCellEditor( DefaultCellEditorSelector.ForeignKeyCellEditor );
    }

    #region ValuePath Property

    public static readonly DependencyProperty ValuePathProperty = DependencyProperty.Register(
      "ValuePath",
      typeof( string ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( null ) );

    public string ValuePath
    {
      get
      {
        return ( string )this.GetValue( ForeignKeyConfiguration.ValuePathProperty );
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.ValuePathProperty, value );
      }
    }

    #endregion

    #region DisplayMemberPath Property

    public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
      "DisplayMemberPath",
      typeof( string ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( null ) );

    public string DisplayMemberPath
    {
      get
      {
        return ( string )this.GetValue( ForeignKeyConfiguration.DisplayMemberPathProperty );
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.DisplayMemberPathProperty, value );
      }
    }

    #endregion

    #region ItemContainerStyle Property

    public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(
      "ItemContainerStyle",
      typeof( Style ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( null ) );

    public Style ItemContainerStyle
    {
      get
      {
        return ( Style )this.GetValue( ForeignKeyConfiguration.ItemContainerStyleProperty );
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.ItemContainerStyleProperty, value );
      }
    }

    #endregion

    #region ItemContainerStyleSelector Property

    public static readonly DependencyProperty ItemContainerStyleSelectorProperty = DependencyProperty.Register(
      "ItemContainerStyleSelector",
      typeof( StyleSelector ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( null ) );

    public StyleSelector ItemContainerStyleSelector
    {
      get
      {
        return ( StyleSelector )this.GetValue( ForeignKeyConfiguration.ItemContainerStyleSelectorProperty );
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.ItemContainerStyleSelectorProperty, value );
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
        return ( IEnumerable )this.GetValue( ForeignKeyConfiguration.ItemsSourceProperty );
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.ItemsSourceProperty, value );
      }
    }

    private static void OnItemsSourceChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var foreignConfiguration = sender as ForeignKeyConfiguration;
      if( foreignConfiguration != null )
      {
        foreignConfiguration.OnItemsSourceChanged( ( IEnumerable )e.OldValue, ( IEnumerable )e.NewValue );
      }
    }

    private void OnItemsSourceChanged( IEnumerable oldItemsSource, IEnumerable newItemsSource )
    {
      //Unsubscribe from the old list changed event, if any.
      if( oldItemsSource != null )
      {
        var oldNotifyCollectionChanged = oldItemsSource as INotifyCollectionChanged;
        if( oldNotifyCollectionChanged != null )
        {
          CollectionChangedEventManager.RemoveListener( oldNotifyCollectionChanged, this );
        }
        else
        {
          var oldBindingList = oldItemsSource as IBindingList;
          if( oldBindingList != null && oldBindingList.SupportsChangeNotification )
          {
            ListChangedEventManager.RemoveListener( oldBindingList, this );
          }
        }
      }

      //Subscribe from to the new list changed event, if any.
      if( newItemsSource != null )
      {
        var newNotifyCollectionChanged = newItemsSource as INotifyCollectionChanged;
        if( newNotifyCollectionChanged != null )
        {
          CollectionChangedEventManager.AddListener( newNotifyCollectionChanged, this );
        }
        else
        {
          var newBindingList = newItemsSource as IBindingList;
          if( newBindingList != null && newBindingList.SupportsChangeNotification )
          {
            ListChangedEventManager.AddListener( newBindingList, this );
          }
        }
      }
    }

    private void OnNotifiyCollectionChanged()
    {
      var handler = this.NotifiyCollectionChanged;
      if( handler == null )
        return;

      handler.Invoke( this, EventArgs.Empty );
    }

    internal event EventHandler NotifiyCollectionChanged;

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

    #region ForeignKeyValueConverter Property

    public static readonly DependencyProperty ForeignKeyValueConverterProperty = DependencyProperty.Register(
      "ForeignKeyValueConverter",
      typeof( IValueConverter ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( null ) );

    public IValueConverter ForeignKeyValueConverter
    {
      get
      {
        return ( IValueConverter )this.GetValue( ForeignKeyConfiguration.ForeignKeyValueConverterProperty );
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.ForeignKeyValueConverterProperty, value );
      }
    }

    #endregion

    #region ForeignKeyValueConverterParameter Property

    public static readonly DependencyProperty ForeignKeyValueConverterParameterProperty = DependencyProperty.Register(
      "ForeignKeyValueConverterParameter",
      typeof( object ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( null ) );

    public object ForeignKeyValueConverterParameter
    {
      get
      {
        return ( object )this.GetValue( ForeignKeyConfiguration.ForeignKeyValueConverterParameterProperty );
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.ForeignKeyValueConverterParameterProperty, value );
      }
    }

    #endregion

    #region ForeignKeyValueConverterCulture Property

    public static readonly DependencyProperty ForeignKeyValueConverterCultureProperty = DependencyProperty.Register(
      "ForeignKeyValueConverterCulture",
      typeof( CultureInfo ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( null ) );

    public CultureInfo ForeignKeyValueConverterCulture
    {
      get
      {
        return ( CultureInfo )this.GetValue( ForeignKeyConfiguration.ForeignKeyValueConverterCultureProperty );
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.ForeignKeyValueConverterCultureProperty, value );
      }
    }

    #endregion

    #region UseDefaultFilterCriterion Property

    public static readonly DependencyProperty UseDefaultFilterCriterionProperty = DependencyProperty.Register(
      "UseDefaultFilterCriterion",
      typeof( bool ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( true ) );

    public bool UseDefaultFilterCriterion
    {
      get
      {
        return ( bool )this.GetValue( ForeignKeyConfiguration.UseDefaultFilterCriterionProperty );
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.UseDefaultFilterCriterionProperty, value );
      }
    }

    #endregion

    #region UseDisplayedValueWhenExporting Property

    public static readonly DependencyProperty UseDisplayedValueWhenExportingProperty = DependencyProperty.Register(
      "UseDisplayedValueWhenExporting",
      typeof( bool ),
      typeof( ForeignKeyConfiguration ),
      new FrameworkPropertyMetadata( true ) );

    public bool UseDisplayedValueWhenExporting
    {
      get
      {
        return ( bool )this.GetValue( ForeignKeyConfiguration.UseDisplayedValueWhenExportingProperty );
      }
      set
      {
        this.SetValue( ForeignKeyConfiguration.UseDisplayedValueWhenExportingProperty, value );
      }
    }

    #endregion

    #region ValuePathDataType Property

    internal Type ValuePathDataType
    {
      get;
      set;
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

    internal object GetDisplayMemberValue( object fieldValue )
    {
      try
      {
        var valuePath = this.ValuePath;
        var displayMemberPath = this.DisplayMemberPath;

        if( ( valuePath == null ) || ( displayMemberPath == null ) )
          return fieldValue;

        var itemsSource = this.ItemsSource;

        //Convert the value from the ValuePath value to the DisplayMemberPath value, using a DataRowView or reflection.
        if( ( itemsSource is DataView ) || ( itemsSource is DataTable ) )
        {
          foreach( object item in itemsSource )
          {
            var dataRowView = item as DataRowView;
            if( dataRowView != null )
            {
              var value = dataRowView[ valuePath ];

              if( fieldValue.Equals( value ) )
                return dataRowView[ displayMemberPath ];
            }
          }
        }
        else
        {
          foreach( object item in itemsSource )
          {
            var value = item.GetType().GetProperty( valuePath ).GetValue( item, null );

            if( fieldValue.Equals( value ) )
              return item.GetType().GetProperty( displayMemberPath ).GetValue( item, null );
          }
        }
      }
      catch
      {
        //Swallow the exception, no need to terminate the application, since the original value will be exported.
      }

      return fieldValue;
    }

    internal static void UpdateColumnsForeignKeyConfigurations(
      ObservableColumnCollection columns,
      IEnumerable itemsSourceCollection,
      PropertyDescriptionRouteDictionary propertyDescriptions,
      bool autoCreateForeignKeyConfigurations )
    {
      var collectionViewBase = itemsSourceCollection as DataGridCollectionViewBase;
      if( collectionViewBase != null )
      {
        ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView( columns, collectionViewBase.ItemProperties, autoCreateForeignKeyConfigurations );
      }
      else
      {
        ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromPropertyDescriptions( columns, propertyDescriptions, autoCreateForeignKeyConfigurations );
      }
    }

    // If a DataGridCollectionViewBase is used, get the ItemProperties it defines to be able to retreive DataGridForeignKeyDescription for each of them
    // to get the auto-detected ForeignKey ItemsSource (if any).
    // If a DataGridCollectionViewBase is not used, the ItemsSource must be manually specified on each Column in order to correctly display/edit the Data
    internal static void UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView(
      ObservableColumnCollection columns,
      DataGridItemPropertyCollection itemProperties,
      bool autoCreateForeignKeyConfigurations )
    {
      if( itemProperties == null )
        return;

      foreach( var itemProperty in itemProperties )
      {
        var description = itemProperty.ForeignKeyDescription;
        if( description != null )
        {
          var columnName = PropertyRouteParser.Parse( itemProperty );
          var column = ( columnName != null ) ? columns[ columnName ] as Column : null;

          if( column != null )
          {
            ForeignKeyConfiguration.SynchronizeForeignKeyConfigurationFromForeignKeyDescription( column, description, autoCreateForeignKeyConfigurations );
          }
        }

        if( itemProperty.ItemPropertiesInternal != null )
        {
          ForeignKeyConfiguration.UpdateColumnsForeignKeyConfigurationsFromDataGridCollectionView(
            columns,
            itemProperty.ItemPropertiesInternal,
            autoCreateForeignKeyConfigurations );
        }
      }
    }

    private static void UpdateColumnsForeignKeyConfigurationsFromPropertyDescriptions(
      ObservableColumnCollection columns,
      PropertyDescriptionRouteDictionary propertyDescriptions,
      bool autoCreateForeignKeyConfigurations )
    {
      if( ( columns == null ) || ( propertyDescriptions == null ) )
        return;

      foreach( var column in columns )
      {
        var targetColumn = column as Column;
        if( targetColumn == null )
          continue;

        var key = PropertyRouteParser.Parse( targetColumn.FieldName );
        if( key == null )
          continue;

        var propertyDescription = propertyDescriptions[ key ];
        if( propertyDescription == null )
          continue;

        var foreignKeyDescription = propertyDescription.Current.ForeignKeyDescription;
        if( foreignKeyDescription == null )
          continue;

        ForeignKeyConfiguration.SynchronizeForeignKeyConfigurationFromForeignKeyDescription( targetColumn, foreignKeyDescription, autoCreateForeignKeyConfigurations );
      }
    }

    internal static void SynchronizeForeignKeyConfigurationFromForeignKeyDescription(
      Column column,
      DataGridForeignKeyDescription description,
      bool autoCreateForeignKeyConfigurations )
    {
      if( ( description == null ) || ( column == null ) )
        return;

      var configuration = column.ForeignKeyConfiguration;
      if( configuration == null )
      {
        if( !autoCreateForeignKeyConfigurations )
          return;

        configuration = new ForeignKeyConfiguration();
        configuration.IsAutoCreated = true;
        column.ForeignKeyConfiguration = configuration;
      }

      // ValuePath will be affected to the FieldName when the configuration is auto-created to be able to modify local source using foreign key value
      if( configuration.IsAutoCreated )
      {
        if( string.IsNullOrEmpty( configuration.ValuePath ) )
        {
          configuration.ValuePath = description.ValuePath;
        }

        if( string.IsNullOrEmpty( configuration.DisplayMemberPath ) )
        {
          configuration.DisplayMemberPath = description.DisplayMemberPath;
        }
      }

      // Affect the ItemsSource on the configuration if it is not already set
      if( ( configuration.ItemsSource == null ) && ( description.ItemsSource != null ) )
      {
        configuration.ItemsSource = description.ItemsSource;
      }

      // Set the Converter if it was not locally set
      if( configuration.ForeignKeyConverter == null )
      {
        configuration.ForeignKeyConverter = description.ForeignKeyConverter;
      }
    }

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( ( managerType == typeof( CollectionChangedEventManager ) ) || ( managerType == typeof( ListChangedEventManager ) ) )
      {
        this.OnNotifiyCollectionChanged();
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion
  }
}
