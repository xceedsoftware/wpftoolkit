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
using System.Xml;
using System.Xml.XPath;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Xceed.Utils;
using Xceed.Wpf.DataGrid.Converters;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Data;
using System.Collections.Specialized;
using System.Reflection;
using Xceed.Wpf.DataGrid.ValidationRules;
using System.Data.Objects.DataClasses;

namespace Xceed.Wpf.DataGrid
{
  internal class ItemsSourceHelper
  {
    #region PUBLIC STATIC METHODS

    public static bool IsSourceSupportingChangeNotification( object obj )
    {
      if( obj == null )
        throw new ArgumentNullException( "obj" );

      if( obj is INotifyCollectionChanged )
        return true;

      IBindingList bindingList = obj as IBindingList;

      return ( ( bindingList != null )
            && ( bindingList.SupportsChangeNotification ) );
    }

    public static bool IsSourceSupportingDBNull( IEnumerable toCheck )
    {
      if( toCheck is DataGridCollectionView )
      {
        // false because all the DBNull conversion should have been done in the DataGridCollectionView.ItemProperties converter.
        return false;
      }

      if( toCheck is DataView )
        return true;

      CollectionView cv = toCheck as CollectionView;

      if( cv != null )
        return ItemsSourceHelper.IsSourceSupportingDBNull( cv.SourceCollection );

      return false;
    }

    public static bool IsItemSupportingDBNull( object toCheck )
    {
      return ( toCheck is System.Data.DataRow ) || ( toCheck is System.Data.DataRowView );
    }

    public static bool IsDataView( IEnumerable toCheck )
    {
      if( toCheck is DataGridCollectionView )
        return false;

      if( toCheck is DataView )
        return true;

      CollectionView cv = toCheck as CollectionView;

      if( ( cv != null ) && ( cv.SourceCollection is DataView ) )
        return true;

      return false;
    }

    [Obsolete( "You should use 'DataGridContext.ItemsSourceCollection as DataGridCollectionViewBase'" )]
    public static DataGridCollectionViewBase TryGetDataGridCollectionViewBase( IEnumerable itemsSource )
    {
      // This method is intended to return null if not using a DataGridCollectionViewBase.
      DataGridCollectionViewBase dataGridCollectionViewBase = itemsSource as DataGridCollectionViewBase;

      // This is to ensure that this is not the master Generator ( and this CollectionView is the ItemsCollection )
      if( dataGridCollectionViewBase == null )
      {
        CollectionView collectionView = itemsSource as CollectionView;

        dataGridCollectionViewBase = ( collectionView == null ) ?
          null : collectionView.SourceCollection as DataGridCollectionViewBase;
      }

      return dataGridCollectionViewBase;
    }

    public static IList TryGetIList( IEnumerable itemsSourceCollection )
    {

      CollectionView collectionView = itemsSourceCollection as ItemCollection;

      if( collectionView != null )
      {
        IList list = collectionView.SourceCollection as IList;

        if( list != null )
          return list;
      }

      return itemsSourceCollection as IList;
    }

    public static bool IsValueType( Type type )
    {
      bool isValueType = type.IsValueType;

      return ( ( ( isValueType ) && ( type.IsPrimitive ) ) ||
        ( ( isValueType ) &&
        ( ( type == typeof( decimal ) ) ||
        ( type == typeof( DateTime ) ) ||
        ( type == typeof( TimeSpan ) ) ||
        ( type == typeof( Guid ) ) ) ) ||
        ( type == typeof( string ) ) );
    }

    public static bool IsEntityObjectLoadable( EntityObject entityObject )
    {
      return
        ( ( entityObject.EntityState & EntityState.Added ) != EntityState.Added ) &&
        ( ( entityObject.EntityState & EntityState.Detached ) != EntityState.Detached );
    }

    public static bool IsEntityFramework( Type type )
    {
      // The EntityFramework assembly is not loaded.  We are running on the client framework.
      if( EntityObjectType == null )
        return false;

      return EntityObjectType.IsAssignableFrom( type );
    }

    public static bool IsEntityFramework( object o )
    {
      if( o == null )
        return false;

      return ItemsSourceHelper.IsEntityFramework( o.GetType() );
    }

    public static object GetFirstItemByEnumerable( IEnumerable enumerable )
    {
      if( enumerable == null )
        return null;

      object current = null;
      IList list = enumerable as IList;

      if( list != null )
      {
        return ( ( list.Count > 0 ) ? list[ 0 ] : null );
      }

      try
      {
        IEnumerator enumerator = enumerable.GetEnumerator();


        if( enumerator.MoveNext() )
          current = enumerator.Current;

      }
      catch( NotSupportedException )
      {
        current = null;
      }

      return current;
    }

    public static object AddNewDataItem(
      IEnumerable itemsSourceCollection,
      DataGridControl dataGridControl,
      out int itemIndex )
    {
      DataGridCollectionViewBase dataGridCollectionViewBase = itemsSourceCollection as DataGridCollectionViewBase;

      if( dataGridCollectionViewBase != null )
      {
        if( !dataGridCollectionViewBase.CanAddNew )
          throw new InvalidOperationException( "An attempt was made to add a new data item to a source that does not support insertion." );

        itemIndex = dataGridCollectionViewBase.Count;
        return dataGridCollectionViewBase.AddNew();
      }

      if( ( dataGridControl != null ) && ( dataGridControl.ItemsSource == null ) )
      {
        //unbound
#pragma warning disable 618
        AddingNewDataItemEventArgs eventArgs = new AddingNewDataItemEventArgs();
        dataGridControl.OnAddingNewDataItem( eventArgs );
        object newItem = eventArgs.DataItem;
#pragma warning restore 618

        if( newItem == null )
          throw new InvalidOperationException( "The AddingNewDataItem event did not return a new data item because the grid is not bound to a data source." );

        itemIndex = dataGridControl.Items.Add( newItem );
        return newItem;
      }

      DataView dataView = itemsSourceCollection as DataView;

      if( dataView == null )
      {
        CollectionView collectionView = itemsSourceCollection as CollectionView;

        dataView = ( collectionView == null ) ?
          null : collectionView.SourceCollection as DataView;
      }

      if( dataView != null )
      {
        itemIndex = dataView.Count;
        return dataView.AddNew();
      }

      IBindingList bindingList = itemsSourceCollection as IBindingList;

      if( bindingList == null )
      {
        CollectionView collectionView = itemsSourceCollection as CollectionView;

        bindingList = ( collectionView == null ) ?
          null : collectionView.SourceCollection as IBindingList;
      }

      if( ( bindingList != null ) && ( bindingList.AllowNew ) )
      {
        itemIndex = bindingList.Count;
        return bindingList.AddNew();
      }

      Type itemType = ItemsSourceHelper.GetItemTypeFromEnumeration( itemsSourceCollection );

      if( itemType == null )
        throw new InvalidOperationException( "An attempt was made to use a source whose item type cannot be determined." );

      try
      {
        itemIndex = -1;
        return Activator.CreateInstance( itemType );
      }
      catch( MissingMethodException exception )
      {
        throw new InvalidOperationException( "An attempt was made to use a source whose item type does not have a default constructor.", exception );
      }
      catch( Exception exception )
      {
        throw new InvalidOperationException( "An unsuccessful attempt was made to create an instance of the source's item type using the item's default constructor.", exception );
      }
    }

    public static void CancelNewDataItem(
      IEnumerable itemsSourceCollection,
      DataGridControl dataGridControl,
      object newItem,
      int newItemIndex )
    {
      DataGridCollectionViewBase dataGridCollectionViewBase = itemsSourceCollection as DataGridCollectionViewBase;

      if( dataGridCollectionViewBase != null )
      {
        if( dataGridCollectionViewBase.CurrentAddItem == newItem )
        {
          // The DataGridCollectionViewBAse's CancelNew will take care of calling
          // the item's CancelEdit if it must do so.
          dataGridCollectionViewBase.CancelNew();
        }

        return;
      }

      if( ( dataGridControl != null ) && ( dataGridControl.ItemsSource == null ) )
      {
        //unbound
        return;
      }

      ICancelAddNew cancelAddNew = itemsSourceCollection as ICancelAddNew;

      if( cancelAddNew == null )
      {
        CollectionView collectionView = itemsSourceCollection as CollectionView;

        cancelAddNew = ( collectionView == null ) ?
          null : collectionView.SourceCollection as ICancelAddNew;
      }

      if( cancelAddNew != null )
      {
        cancelAddNew.CancelNew( newItemIndex );
        return;
      }

      IEditableObject editableObject = ItemsSourceHelper.GetEditableObject( newItem );

      // editableObject can be an xceed datarow when directly inserted as Items in the DataGridControl.
      if( ( editableObject != null ) && ( !( editableObject is Xceed.Wpf.DataGrid.DataRow ) ) )
      {
        editableObject.CancelEdit();
        return;
      }

      if( newItemIndex != -1 )
      {
        IList list = itemsSourceCollection as IList;

        if( list == null )
        {
          CollectionView collectionView = itemsSourceCollection as CollectionView;

          list = ( collectionView == null ) ?
            null : collectionView.SourceCollection as IList;
        }

        if( ( list != null ) && ( !list.IsFixedSize ) )
        {
          list.RemoveAt( newItemIndex );
        }
      }
    }

    public static void EndNewDataItem(
      IEnumerable itemsSourceCollection,
      DataGridControl dataGridControl,
      object newItem,
      ref int newItemIndex )
    {
      DataGridCollectionViewBase dataGridCollectionViewBase = itemsSourceCollection as DataGridCollectionViewBase;

      if( dataGridCollectionViewBase != null )
      {
        if( dataGridCollectionViewBase.CurrentAddItem == newItem )
        {
          // The DataGridCollectionViewBase's EndNew will take care of calling
          // the item's EndEdit if it must do so.
          dataGridCollectionViewBase.CommitNew();
        }

        return;
      }

      if( ( dataGridControl != null ) && ( dataGridControl.ItemsSource == null ) )
      {
        //unbound
        return;
      }

      ICancelAddNew cancelAddNew = itemsSourceCollection as ICancelAddNew;

      if( cancelAddNew == null )
      {
        CollectionView collectionView = itemsSourceCollection as CollectionView;

        cancelAddNew = ( collectionView == null ) ?
          null : collectionView.SourceCollection as ICancelAddNew;
      }

      if( cancelAddNew != null )
      {
        cancelAddNew.EndNew( newItemIndex );
        return;
      }

      IEditableObject editableObject = ItemsSourceHelper.GetEditableObject( newItem );

      // editableObject can be a datarow when directly inserted as Items in the DataGridControl.
      if( ( editableObject != null ) && ( !( editableObject is Xceed.Wpf.DataGrid.DataRow ) ) )
      {
        editableObject.EndEdit();
        return;
      }

      IBindingList bindingList = itemsSourceCollection as IBindingList;

      if( bindingList == null )
      {
        CollectionView collectionView = itemsSourceCollection as CollectionView;

        bindingList = ( collectionView == null ) ?
          null : collectionView.SourceCollection as IBindingList;
      }

      if( ( bindingList != null ) && ( bindingList.AllowNew ) )
      {
        // In that case the item is already added into the list by IBindingList.AddNew.
        return;
      }

      IList list = itemsSourceCollection as IList;

      if( list == null )
      {
        CollectionView collectionView = itemsSourceCollection as CollectionView;

        list = ( collectionView == null ) ?
          null : collectionView.SourceCollection as IList;
      }

      if( ( list == null ) || ( list.IsFixedSize ) )
        throw new InvalidOperationException( "An attempt was made to insert an item into a source that does not implement the IList interface or that has a fixed size." );

      newItemIndex = list.Count;
      list.Add( newItem );
    }

    public static Dictionary<string, FieldDescriptor> GetFieldDescriptors(
      PropertyDescriptorCollection properties,
      bool supportDBNull )
    {
      int fieldCount = properties.Count;

      Dictionary<string, FieldDescriptor> fieldDescriptors =
        new Dictionary<string, FieldDescriptor>( fieldCount );

      ItemsSourceHelper.ExtractFieldDescriptors( string.Empty, properties, supportDBNull, fieldDescriptors );

      return fieldDescriptors;
    }

    public static void ExtractFieldDescriptors(
      string namePrefix,
      PropertyDescriptorCollection properties,
      bool supportDBNull,
      Dictionary<string, FieldDescriptor> fieldDescriptors )
    {
      int propertyCount = properties.Count;

      for( int i = 0; i < propertyCount; i++ )
      {
        PropertyDescriptor propertyDescriptor = properties[ i ];
        string name = propertyDescriptor.Name;

        if( ( fieldDescriptors.ContainsKey( name ) ) && ( !string.IsNullOrEmpty( namePrefix ) ) )
          name = namePrefix + "." + name;

        Type type = propertyDescriptor.PropertyType;

        DataGridForeignKeyDescription foreignKeyDescription = null;

        // Try to retreive the ForeignKeyDescription if the field is an Enum
        if( ( type != null ) && ( type.IsEnum ) )
        {
          foreignKeyDescription = ItemsSourceHelper.GetDataGridForeignKeyDescriptionForEnum( type );
        }

        fieldDescriptors[ name ] = new FieldDescriptor( name,
                                                        propertyDescriptor.DisplayName,
                                                        type,
                                                        propertyDescriptor,
                                                        null,
                                                        null,
                                                        propertyDescriptor.IsReadOnly,
                                                        false,
                                                        supportDBNull,
                                                        propertyDescriptor.IsBrowsable,
                                                        ItemsSourceHelper.IsASubRelationship( propertyDescriptor.PropertyType ),
                                                        false,
                                                        foreignKeyDescription );
      }
    }

    public static void GenerateColumnsFromItemsSourceFields(
      ColumnCollection columns,
      IDictionary<Type, CellEditor> defaultCellEditors,
      Dictionary<string, FieldDescriptor> fields,
      bool autoCreateForeignKeyConfigurations )
    {
      DataGridControl dataGridControl = columns.DataGridControl;

      using( columns.DeferColumnAdditionMessages() )
      {
        foreach( FieldDescriptor field in fields.Values )
        {
          string fieldName = field.Name;
          ColumnBase column = columns[ fieldName ];
          Column dataColumn = column as Column;
          Type dataType = field.DataType;

          if( column == null )
          {
            dataColumn = ItemsSourceHelper.CreateColumnFromItemsSourceField(
              dataGridControl, defaultCellEditors, field, autoCreateForeignKeyConfigurations );

            if( dataColumn != null )
            {
              columns.Add( dataColumn );
              ItemsSourceHelper.ApplySettingsRepositoryToColumn( dataColumn );
            }
          }
          else if( dataColumn != null )
          {
            if( field.ReadOnly )
            {
              if( dataColumn.ReadLocalValue( Column.ReadOnlyProperty ) == DependencyProperty.UnsetValue )
              {
                dataColumn.ReadOnly = field.ReadOnly;
              }
            }

            if( field.OverrideReadOnlyForInsertion )
            {
              if( dataColumn.ReadLocalValue( ColumnBase.OverrideReadOnlyForInsertionProperty ) == DependencyProperty.UnsetValue )
              {
                dataColumn.OverrideReadOnlyForInsertion = field.OverrideReadOnlyForInsertion;
              }
            }

            if( dataColumn.ReadLocalValue( Column.TitleProperty ) == DependencyProperty.UnsetValue )
            {
              dataColumn.Title = field.DisplayName;
            }

            if( dataColumn.ReadLocalValue( Column.CellEditorProperty ) == DependencyProperty.UnsetValue )
            {
              CellEditor cellEditor = null;

              if( defaultCellEditors != null )
              {
                defaultCellEditors.TryGetValue( dataType, out cellEditor );
              }

              if( cellEditor == null )
              {
                object descriptionItemsSource = null;
                object configurationItemsSource = null;
                ForeignKeyConfiguration configuration = dataColumn.ForeignKeyConfiguration;

                if( field.ForeignKeyDescription != null )
                {
                  descriptionItemsSource = field.ForeignKeyDescription.ItemsSource;
                }

                if( configuration != null )
                {
                  configurationItemsSource = configuration.ItemsSource;

                  if( configurationItemsSource == null )
                  {
                    configurationItemsSource = dataColumn.ReadLocalValue( Column.ForeignKeyConfigurationProperty );
                  }
                }

                // A foreign key ItemsSource is set and we can auto-create configuration
                // OR
                // if the foreign key ItemsSource was found in the ForeignKeyConfiguration
                //
                // use the Default ForeignKey CellEditor.
                if( ( ( descriptionItemsSource != null ) && ( autoCreateForeignKeyConfigurations ) )
                    || ( configurationItemsSource != null ) )
                {
                  cellEditor = DefaultCellEditorSelector.ForeignKeyCellEditor;
                }
              }

              if( cellEditor == null )
              {
                cellEditor = DefaultCellEditorSelector.SelectCellEditor( dataType );
              }

              dataColumn.CellEditor = cellEditor;
            }

            if( ( field.ForeignKeyDescription != null )
                && ( field.ForeignKeyDescription.ItemsSource != null )
                && ( autoCreateForeignKeyConfigurations ) )
            {
              // Update the ForeignKeyConfiguration from the ForeignKeyDescription
              // found on the FieldDescriptor
              ForeignKeyConfiguration.SynchronizeForeignKeyConfigurationFromForeignKeyDescription(
                dataColumn,
                field.ForeignKeyDescription,
                autoCreateForeignKeyConfigurations );
            }

            // Disable warning for DisplayMemberBinding when internaly used
#pragma warning disable 618

            if( dataColumn.DisplayMemberBinding == null )
            {
              dataColumn.DisplayMemberBinding = ItemsSourceHelper.CreateDefaultBinding(
                false, field.Name, field,
                false, ( dataColumn.ReadOnly && !dataColumn.OverrideReadOnlyForInsertion ), dataType );

              //mark the Column's Binding as AutoCreated.
              dataColumn.IsBindingAutoCreated = true;
              dataColumn.IsBoundToDataGridUnboundItemProperty = field.IsDataGridUnboundItemProperty;
            }

#pragma warning restore 618
          }
        }
      } //end using
    }

    public static Column CreateColumnFromItemsSourceField(
      DataGridControl dataGridControl,
      IDictionary<Type, CellEditor> defaultCellEditors,
      FieldDescriptor field,
      bool autoCreateForeignKeyConfigurations )
    {
      if( ( field.IsASubRelationship ) || ( !field.Browsable ) )
        return null;

      string fieldName = field.Name;
      Type dataType = field.DataType;

      Column dataColumn = new Column();
      dataColumn.IsAutoCreated = true;
      dataColumn.FieldName = fieldName;

      bool readOnly = field.ReadOnly;
      bool overrideReadOnlyForInsertion = field.OverrideReadOnlyForInsertion;

      // We only set ReadOnly when the value is true in order for the inheritence chain to work.  
      // Otherwise, the column value is always used instead of the row or grid value.
      if( readOnly )
        dataColumn.ReadOnly = readOnly;

      dataColumn.OverrideReadOnlyForInsertion = overrideReadOnlyForInsertion;
      dataColumn.Title = field.DisplayName;

      // Disable warning for DisplayMemberBinding when internaly used
#pragma warning disable 618

      dataColumn.DisplayMemberBinding = ItemsSourceHelper.CreateDefaultBinding(
        false, field.Name, field,
        false, ( readOnly && !overrideReadOnlyForInsertion ), dataType );

#pragma warning restore 618

      //mark the Column's Binding as AutoCreated.
      dataColumn.IsBindingAutoCreated = true;
      dataColumn.IsBoundToDataGridUnboundItemProperty = field.IsDataGridUnboundItemProperty;

      CellEditor cellEditor = null;

      if( defaultCellEditors != null )
      {
        defaultCellEditors.TryGetValue( dataType, out cellEditor );
      }

      if( ( field.ForeignKeyDescription != null )
        && ( field.ForeignKeyDescription.ItemsSource != null )
        && ( autoCreateForeignKeyConfigurations ) )
      {
        // We will only use the default foreign key CellEditor
        // if:
        // - a ForeignKey ItemsSource was detected
        // - the grid allows the auto-creation of the ForeignKeyConfigurations
        // else, use the default CellEditor
        if( cellEditor == null )
        {
          cellEditor = DefaultCellEditorSelector.ForeignKeyCellEditor;
        }

        // Update the ForeignKeyConfiguration from the ForeignKeyDescription
        // found on the FieldDescriptor
        ForeignKeyConfiguration.SynchronizeForeignKeyConfigurationFromForeignKeyDescription(
          dataColumn,
          field.ForeignKeyDescription,
          autoCreateForeignKeyConfigurations );
      }

      if( cellEditor == null )
      {
        cellEditor = DefaultCellEditorSelector.SelectCellEditor( dataType );
      }

      dataColumn.CellEditor = cellEditor;
      return dataColumn;
    }

    private static void ApplySettingsRepositoryToColumn( ColumnBase column )
    {
    }

    public static System.Windows.Data.Binding CreateDefaultBinding(
      bool dataItemIsDataRow,
      string fieldName,
      FieldDescriptor sourceField,
      bool supportDBNull,
      bool readOnly,
      Type dataType )
    {
      DataGridBindingInfo bindingInfo = new DataGridBindingInfo();

      PropertyDescriptor propertyDescriptor = null;
      string bindingPath = null;
      string bindingXPath = null;

      if( sourceField != null )
      {
        dataType = sourceField.DataType;
        supportDBNull = sourceField.SupportDBNull;
        readOnly |= ( sourceField.ReadOnly && !sourceField.OverrideReadOnlyForInsertion );
        propertyDescriptor = sourceField.PropertyDescriptor;
        bindingXPath = sourceField.BindingXPath;
        bindingPath = sourceField.BindingPath;
      }

      if( dataItemIsDataRow )
      {
        bindingInfo.Path = new PropertyPath( "Cells[" + fieldName + "].Content" );
      }
      else
      {
        // We always use Path and XPath to be able to write back to a binding on self (".").
        // Because the "propertyDescriptor" received is readonly, we must use the normal binding path to pass over that.
        if( !string.IsNullOrEmpty( bindingXPath ) )
        {
          bindingInfo.XPath = bindingXPath;

          if( !string.IsNullOrEmpty( bindingPath ) )
            bindingInfo.Path = new PropertyPath( bindingPath, ItemsSourceHelper.EmptyObjectArray );
        }
        else
        {
          bindingInfo.Path = ItemsSourceHelper.CreatePropertyPath( fieldName, bindingPath, propertyDescriptor );
        }
      }

      bindingInfo.ReadOnly = readOnly;
      bindingInfo.Converter = new SourceDataConverter( supportDBNull );

      bindingInfo.ValidationRules.Add( new SourceDataConverterValidationRule( supportDBNull, dataType ) );

      return bindingInfo.GetBinding();
    }

    public static PropertyPath CreatePropertyPath( string fieldName, string bindingPath, PropertyDescriptor propertyDescriptor )
    {
      PropertyPath propertyPath = null;

      if( !string.IsNullOrEmpty( bindingPath ) )
      {
        // Using user defined column binding info.
        propertyPath = new PropertyPath( bindingPath, ItemsSourceHelper.EmptyObjectArray );
      }
      else
      {
        if( propertyDescriptor != null )
        {
          if( propertyDescriptor is DataGridItemPropertyBase.PropertyDescriptorFromItemPropertyBase )
          {
            // Using a DataGridItemPropertyBase.
            propertyPath = new PropertyPath( "(0)", propertyDescriptor );
          }
          else
          {
            // Not using our CollectionView.
            propertyPath = new PropertyPath( "(0).(1)", EmptyDataItemSafePropertyDescriptor.Singleton, propertyDescriptor );
          }
        }
        else
        {
          propertyPath = new PropertyPath( fieldName, ItemsSourceHelper.EmptyObjectArray );
        }
      }

      return propertyPath;
    }

    public static Dictionary<string, FieldDescriptor> GetFields( IEnumerable itemsSource, Type itemType )
    {
      DataView dataView = itemsSource as DataView;

      if( dataView != null )
        return ItemsSourceHelper.GetFieldsForDataView( dataView );

      DataGridCollectionViewBase dataGridCollectionViewBase = itemsSource as DataGridCollectionViewBase;

      if( dataGridCollectionViewBase != null )
        return ItemsSourceHelper.GetFieldsForDataGridCollectionViewBase( dataGridCollectionViewBase, dataGridCollectionViewBase.ItemProperties );

      bool supportsDBNull = ItemsSourceHelper.IsSourceSupportingDBNull( itemsSource );
      ITypedList typedList = itemsSource as ITypedList;

      if( typedList != null )
        return ItemsSourceHelper.GetFieldDescriptors( typedList.GetItemProperties( null ), supportsDBNull );

      if( itemType == null )
        itemType = ItemsSourceHelper.GetItemTypeFromEnumeration( itemsSource );

      return ItemsSourceHelper.GetFieldsForItemType( itemsSource, itemType, supportsDBNull );
    }

    private static Dictionary<string, FieldDescriptor> GetFieldsForItemType( IEnumerable itemsSource, Type itemType, bool supportsDBNull )
    {
      if( itemType == null )
        return new Dictionary<string, FieldDescriptor>();

      if( typeof( XmlNode ).IsAssignableFrom( itemType ) )
        return new Dictionary<string, FieldDescriptor>();

      if( itemType.IsArray )
        return ItemsSourceHelper.GetFieldsForJaggedArray( itemType, itemsSource );

      if( itemType.IsInterface )
        return ItemsSourceHelper.GetFieldsForInterface( itemType );

      if( ItemsSourceHelper.IsEntityFramework( itemType ) )
        return ItemsSourceHelper.GetFieldsForEntityFramework( itemType );

      TypeDescriptionProvider typeDescriptionProvider = TypeDescriptor.GetProvider( itemType );
      object firstItem = ItemsSourceHelper.GetFirstItemByEnumerable( itemsSource );

      ICustomTypeDescriptor customTypeDescriptor = firstItem as ICustomTypeDescriptor;

      if( customTypeDescriptor == null )
        customTypeDescriptor = typeDescriptionProvider.GetTypeDescriptor( itemType, firstItem );

      if( ItemsSourceHelper.IsValueType( itemType ) )
      {
        Dictionary<string, FieldDescriptor> fieldDescriptors = new Dictionary<string, FieldDescriptor>();

        fieldDescriptors.Add( ".", new FieldDescriptor( ".",
                                                        string.Empty,
                                                        itemType,
                                                        null,
                                                        null,
                                                        ".",
                                                        true,
                                                        false,
                                                        supportsDBNull,
                                                        true,
                                                        ItemsSourceHelper.IsASubRelationship( itemType ),
                                                        false,
                                                        null ) );

        return fieldDescriptors;
      }
      else if( customTypeDescriptor != null )
      {
        string className = customTypeDescriptor.GetClassName();

        Type customType = ( className == null ) ? null : Type.GetType( className );

        if( ( customType != null ) && ( ItemsSourceHelper.IsValueType( customType ) ) )
        {
          Dictionary<string, FieldDescriptor> fieldDescriptors = new Dictionary<string, FieldDescriptor>();

          DataGridForeignKeyDescription foreignKeyDescription = null;

          // Try to retreive the ForeignKeyDescription if the field is an Enum
          if( ( itemType != null ) && ( itemType.IsEnum ) )
          {
            foreignKeyDescription = ItemsSourceHelper.GetDataGridForeignKeyDescriptionForEnum( itemType );
          }

          fieldDescriptors.Add( ".", new FieldDescriptor( ".",
                                                          string.Empty,
                                                          itemType,
                                                          null,
                                                          null,
                                                          ".",
                                                          true,
                                                          false,
                                                          supportsDBNull,
                                                          true,
                                                          ItemsSourceHelper.IsASubRelationship( itemType ),
                                                          false,
                                                          foreignKeyDescription ) );

          return fieldDescriptors;
        }
      }

      if( customTypeDescriptor != null )
        return ItemsSourceHelper.GetFieldDescriptors( customTypeDescriptor.GetProperties(), supportsDBNull );

      return ItemsSourceHelper.GetFieldDescriptors( TypeDescriptor.GetProperties( itemType ), supportsDBNull );
    }

    public static Type GetItemTypeFromEnumeration( IEnumerable source )
    {
      if( source == null )
        return null;

      Type listType = source.GetType();

      if( typeof( Array ).IsAssignableFrom( listType ) )
        return listType.GetElementType();

      Type itemType = ItemsSourceHelper.GetTypedListIndexerType( listType );

      if( itemType != null )
        return itemType;

      itemType = ItemsSourceHelper.GetTypedEnumerationItemType( listType );

      if( itemType != null )
        return itemType;

      object item = ItemsSourceHelper.GetFirstItemByEnumerable( source );

      if( item != null )
        return item.GetType();

      return typeof( object );
    }

    public static IEditableObject GetEditableObject( object item )
    {
      System.Data.DataRow dataRow = item as System.Data.DataRow;

      if( dataRow != null )
        return new DataRowEditableWrapper( dataRow );

      return item as IEditableObject;
    }

    public static bool IsASubRelationship( Type dataType )
    {
      if( ( !dataType.IsValueType ) && ( dataType != typeof( string ) ) )
      {
        if( typeof( IEnumerable ).IsAssignableFrom( dataType ) )
        {
          if( typeof( byte[] ).IsAssignableFrom( dataType ) )
            return false;

          return true;
        }

        if( typeof( IListSource ).IsAssignableFrom( dataType ) )
          return true;
      }

      return false;
    }

    public static object TryGetDataRowFromDataItem( object dataItem )
    {
      DataRowView dataRowView = dataItem as DataRowView;

      if( dataRowView != null )
      {
        return dataRowView.Row;
      }

      return dataItem;
    }

    public static System.Data.DataView TryGetDataViewFromDataGridContext( DataGridContext context )
    {
      if( context == null )
        return null;

      System.Data.DataView dataView = context.ItemsSourceCollection as System.Data.DataView;

      // Maybe the context is a detail
      if( dataView == null )
      {
        DataGridCollectionViewBase collectionViewBase =
          context.ItemsSourceCollection as DataGridCollectionViewBase;

        if( collectionViewBase != null )
        {
          dataView = collectionViewBase.SourceCollection as System.Data.DataView;
        }
      }

      return dataView;
    }

    public static Type GetColumnDataType( System.Data.DataColumn column )
    {
      Type columnType;

      if( column.AllowDBNull )
      {
        if( column.DataType == typeof( Boolean ) )
          columnType = typeof( Nullable<Boolean> );
        else if( column.DataType == typeof( Boolean ) )
          columnType = typeof( Nullable<Boolean> );
        else if( column.DataType == typeof( Byte ) )
          columnType = typeof( Nullable<Byte> );
        else if( column.DataType == typeof( Char ) )
          columnType = typeof( Nullable<Char> );
        else if( column.DataType == typeof( DateTime ) )
          columnType = typeof( Nullable<DateTime> );
        else if( column.DataType == typeof( Decimal ) )
          columnType = typeof( Nullable<Decimal> );
        else if( column.DataType == typeof( Double ) )
          columnType = typeof( Nullable<Double> );
        else if( column.DataType == typeof( Int16 ) )
          columnType = typeof( Nullable<Int16> );
        else if( column.DataType == typeof( Int32 ) )
          columnType = typeof( Nullable<Int32> );
        else if( column.DataType == typeof( Int64 ) )
          columnType = typeof( Nullable<Int64> );
        else if( column.DataType == typeof( SByte ) )
          columnType = typeof( Nullable<SByte> );
        else if( column.DataType == typeof( Single ) )
          columnType = typeof( Nullable<Single> );
        else if( column.DataType == typeof( TimeSpan ) )
          columnType = typeof( Nullable<TimeSpan> );
        else if( column.DataType == typeof( UInt16 ) )
          columnType = typeof( Nullable<UInt16> );
        else if( column.DataType == typeof( UInt32 ) )
          columnType = typeof( Nullable<UInt32> );
        else if( column.DataType == typeof( UInt64 ) )
          columnType = typeof( Nullable<UInt64> );
        else
          columnType = column.DataType;
      }
      else
        columnType = column.DataType;
      return columnType;
    }

    public static void CleanUpColumns( ColumnCollection columns, bool deleteAutoCreatedColumn )
    {
      ColumnBase[] tempColumns = new ColumnBase[ columns.Count ];
      columns.CopyTo( tempColumns, 0 );

      foreach( ColumnBase column in tempColumns )
      {
        Column dataColumn = column as Column;

        if( dataColumn == null )
          continue;

        if( ( deleteAutoCreatedColumn ) && ( dataColumn.IsAutoCreated ) )
        {
          columns.Remove( column );
        }
        else if( dataColumn.IsBindingAutoCreated )
        {
          // Disable warning for DisplayMemberBinding when internaly used
#pragma warning disable 618

          //clear the display member binding.
          dataColumn.DisplayMemberBinding = null;

#pragma warning restore 618

          dataColumn.IsBindingAutoCreated = false;
          dataColumn.IsBoundToDataGridUnboundItemProperty = false;
        }
      }
    }

    public static FieldDescriptor CreateFieldFromDataGridItemProperty( DataGridItemPropertyBase itemProperty )
    {
      DataGridItemPropertyBase.PropertyDescriptorFromItemPropertyBase propertyDescriptor =
        itemProperty.GetPropertyDescriptorForBinding();

      string name = propertyDescriptor.Name;
      Type type = propertyDescriptor.PropertyType;

      // If an ItemProperty is in the public ItemProperties of the collection view,
      // we always want to consider that property is not a detail and browsable.
      return new FieldDescriptor( name,
                                  propertyDescriptor.DisplayName,
                                  propertyDescriptor.PropertyType,
                                  propertyDescriptor,
                                  null,
                                  null,
                                  itemProperty.IsReadOnly,
                                  itemProperty.OverrideReadOnlyForInsertion.GetValueOrDefault( false ),
                                  false,
                                  true,
                                  false,
                                  itemProperty is DataGridUnboundItemProperty,
                                  itemProperty.ForeignKeyDescription );
    }

    public static void UpdateColumnsOnItemsPropertiesChanged( 
      DataGridControl dataGridControl, 
      ColumnCollection columns,
      bool autoCreateForeignKeyConfigurations,
      NotifyCollectionChangedEventArgs e,
      DataGridItemPropertyCollection itemProperties )
    {
      if( dataGridControl == null )
        return;

      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Add:
          {
            foreach( DataGridItemPropertyBase itemProperty in e.NewItems )
            {
              string name = itemProperty.Name;

              if( columns[ name ] == null )
              {
                Column column = ItemsSourceHelper.CreateColumnFromItemsSourceField(
                  dataGridControl, dataGridControl.DefaultCellEditors,
                  ItemsSourceHelper.CreateFieldFromDataGridItemProperty( itemProperty ),
                  autoCreateForeignKeyConfigurations );

                if( column != null )
                {
                  columns.Add( column );
                  ItemsSourceHelper.ApplySettingsRepositoryToColumn( column );
                }
              }
            }
          }

          break;

        case NotifyCollectionChangedAction.Remove:
          {
            foreach( DataGridItemPropertyBase itemProperty in e.OldItems )
            {
              string name = itemProperty.Name;
              Column column = columns[ name ] as Column;

              if( ( column != null ) && ( column.IsAutoCreated ) )
              {
                columns.Remove( column );
              }
            }

            break;
          }

        case NotifyCollectionChangedAction.Replace:
          {
            foreach( DataGridItemPropertyBase itemProperty in e.OldItems )
            {
              string name = itemProperty.Name;
              Column column = columns[ name ] as Column;

              if( ( column != null ) && ( column.IsAutoCreated ) )
              {
                columns.Remove( column );
              }
            }

            foreach( DataGridItemPropertyBase itemProperty in e.NewItems )
            {
              string name = itemProperty.Name;

              if( columns[ name ] == null )
              {
                Column column = ItemsSourceHelper.CreateColumnFromItemsSourceField(
                  dataGridControl, dataGridControl.DefaultCellEditors,
                  ItemsSourceHelper.CreateFieldFromDataGridItemProperty( itemProperty ),
                  autoCreateForeignKeyConfigurations );

                if( column != null )
                {
                  columns.Add( column );
                  ItemsSourceHelper.ApplySettingsRepositoryToColumn( column );
                }
              }
            }

            break;
          }

        case NotifyCollectionChangedAction.Reset:
          {
            for( int i = columns.Count - 1; i >= 0; i-- )
            {
              Column dataColumn = columns[ i ] as Column;

              if( ( dataColumn != null ) && ( dataColumn.IsAutoCreated ) )
              {
                columns.Remove( dataColumn );
              }
            }

            foreach( DataGridItemPropertyBase itemProperty in itemProperties )
            {
              string name = itemProperty.Name;

              if( columns[ name ] == null )
              {
                Column column = ItemsSourceHelper.CreateColumnFromItemsSourceField(
                  dataGridControl, dataGridControl.DefaultCellEditors,
                  ItemsSourceHelper.CreateFieldFromDataGridItemProperty( itemProperty ),
                  autoCreateForeignKeyConfigurations );

                if( column != null )
                {
                  columns.Add( column );
                  ItemsSourceHelper.ApplySettingsRepositoryToColumn( column );
                }
              }
            }

            break;
          }

        //case NotifyCollectionChangedAction.Move:
        default:
          break;

      }
    }

    #endregion PUBLIC STATIC METHODS

    #region PRIVATE STATIC METHODS

    private static Type GetTypedListIndexerType( Type listType )
    {
      if( ( !typeof( IList ).IsAssignableFrom( listType ) && !typeof( ITypedList ).IsAssignableFrom( listType ) ) && !typeof( IListSource ).IsAssignableFrom( listType ) )
        return null;

      PropertyInfo info = null;
      PropertyInfo[] properties = listType.GetProperties( BindingFlags.Public | BindingFlags.Instance );

      for( int i = 0; i < properties.Length; i++ )
      {
        if( ( properties[ i ].GetIndexParameters().Length > 0 ) && ( properties[ i ].PropertyType != typeof( object ) ) )
        {
          info = properties[ i ];

          if( info.Name == "Item" )
            return info.PropertyType;
        }
      }

      if( info != null )
        return info.PropertyType;

      return null;
    }

    private static Type GetTypedEnumerationItemType( Type listType )
    {
      foreach( Type interfaceType in listType.GetInterfaces() )
      {
        if( ( interfaceType.IsGenericType ) && ( interfaceType.GetGenericTypeDefinition() == typeof( IEnumerable<> ) ) )
          return interfaceType.GetGenericArguments()[ 0 ];
      }

      return null;
    }

    private static Dictionary<string, FieldDescriptor> GetFieldsForDataView( DataView dataView )
    {
      PropertyDescriptorCollection itemProperties = ( ( ITypedList )dataView ).GetItemProperties( null );
      int fieldCount = itemProperties.Count;
      Dictionary<string, FieldDescriptor> fieldDescriptors = new Dictionary<string, FieldDescriptor>( fieldCount );

      Dictionary<string, ForeignKeyConstraint> foreignKeyConstraints =
        ItemsSourceHelper.GetForeignKeyConstraints( dataView.Table.Constraints );

      DataColumnCollection columns = dataView.Table.Columns;

      for( int i = 0; i < fieldCount; i++ )
      {
        PropertyDescriptor propertyDescriptor = itemProperties[ i ];

        string name = propertyDescriptor.Name;
        DataColumn column = columns[ name ];

        string displayName =
          ( column == null ) ?
          propertyDescriptor.DisplayName : column.Caption;

        bool allowBDNull =
          ( column == null ) ?
          false : column.AllowDBNull;

        ForeignKeyConstraint foreignKeyConstraint = null;
        foreignKeyConstraints.TryGetValue( name, out foreignKeyConstraint );

        DataTableForeignKeyDescription foreignKeyDescription =
          ItemsSourceHelper.GetDataGridForeignKeyDescriptionForForeignKeyConstraint( foreignKeyConstraint );

        fieldDescriptors[ name ] = new FieldDescriptor( name,
                                                        displayName,
                                                        propertyDescriptor.PropertyType,
                                                        propertyDescriptor,
                                                        null,
                                                        name,
                                                        propertyDescriptor.IsReadOnly,
                                                        false,
                                                        allowBDNull,
                                                        propertyDescriptor.IsBrowsable,
                                                        ( column == null ) && ItemsSourceHelper.IsASubRelationship( propertyDescriptor.PropertyType ),
                                                        false,
                                                        foreignKeyDescription );

        Debug.Assert( ( column != null ) || ( ItemsSourceHelper.IsASubRelationship( propertyDescriptor.PropertyType ) ),
          "If we don't have a column that corresponds to the property of a DataRowView, it is safe to assume that we have a sub relation." );
      }

      return fieldDescriptors;
    }

    private static Dictionary<string, FieldDescriptor> GetFieldsForDataTable( DataTable dataTable )
    {
      DataColumnCollection columns = dataTable.Columns;
      int fieldCount = columns.Count;

      Dictionary<string, FieldDescriptor> fieldDescriptors = new Dictionary<string, FieldDescriptor>( fieldCount );
      Dictionary<string, ForeignKeyConstraint> foreignKeyConstraints =
       ItemsSourceHelper.GetForeignKeyConstraints( dataTable.Constraints );

      for( int i = 0; i < fieldCount; i++ )
      {
        DataColumn column = columns[ i ];

        string name = column.Caption;

        ForeignKeyConstraint foreignKeyConstraint = null;
        foreignKeyConstraints.TryGetValue( name, out foreignKeyConstraint );

        DataTableForeignKeyDescription foreignKeyDescription =
          ItemsSourceHelper.GetDataGridForeignKeyDescriptionForForeignKeyConstraint( foreignKeyConstraint );

        fieldDescriptors[ name ] = new FieldDescriptor( name,
                                                        name,
                                                        column.DataType,
                                                        null,
                                                        null,
                                                        "[" + name + "]",
                                                        column.ReadOnly,
                                                        false,
                                                        column.AllowDBNull,
                                                        true,
                                                        false,
                                                        false,
                                                        foreignKeyDescription );
      }

      return fieldDescriptors;
    }

    private static Dictionary<string, FieldDescriptor> GetFieldsForDataGridCollectionViewBase(
      DataGridCollectionViewBase dataGridCollectionViewBase,
      DataGridItemPropertyCollection itemProperties )
    {
      int fieldCount = itemProperties.Count;

      Dictionary<string, FieldDescriptor> fieldDescriptors =
        new Dictionary<string, FieldDescriptor>( fieldCount );

      for( int i = 0; i < fieldCount; i++ )
      {
        DataGridItemPropertyBase itemProperty = itemProperties[ i ];
        fieldDescriptors[ itemProperty.Name ] = ItemsSourceHelper.CreateFieldFromDataGridItemProperty( itemProperty );
      }

      return fieldDescriptors;
    }

    private static Dictionary<string, FieldDescriptor> GetFieldsForInterface( Type itemType )
    {
      Dictionary<string, FieldDescriptor> fieldDescriptors = new Dictionary<string, FieldDescriptor>();
      ItemsSourceHelper.ExtractFieldDescriptors( string.Empty, TypeDescriptor.GetProperties( itemType ), false, fieldDescriptors );

      foreach( Type interfaceType in itemType.GetInterfaces() )
      {
        ItemsSourceHelper.ExtractFieldDescriptors( interfaceType.FullName, TypeDescriptor.GetProperties( interfaceType ), false, fieldDescriptors );
      }

      return fieldDescriptors;
    }

    private static Dictionary<string, FieldDescriptor> GetFieldsForJaggedArray(
      Type itemType,
      IEnumerable jaggedArray )
    {
      int fieldCount = 0;
      IEnumerator enumerator = jaggedArray.GetEnumerator();
      enumerator.MoveNext();

      try
      {
        Array arrayItem = enumerator.Current as Array;
        fieldCount = arrayItem.GetLength( 0 );
      }
      catch
      {
      }

      Dictionary<string, FieldDescriptor> fieldDescriptors =
        new Dictionary<string, FieldDescriptor>( fieldCount );

      Type fieldType = itemType.GetElementType();

      for( int i = 0; i < fieldCount; i++ )
      {
        string name = ".[" + i.ToString( CultureInfo.InvariantCulture ) + "]";

        DataGridForeignKeyDescription foreignKeyDescription = null;

        // Try to retreive the ForeignKeyDescription if the field is an Enum
        if( ( fieldType != null ) && ( fieldType.IsEnum ) )
        {
          foreignKeyDescription = ItemsSourceHelper.GetDataGridForeignKeyDescriptionForEnum( fieldType );
        }

        fieldDescriptors[ name ] = new FieldDescriptor( name,
                                                        name,
                                                        fieldType,
                                                        new JaggedArrayPropertyDescriptor( i, fieldType ),
                                                        null,
                                                        null,
                                                        false,
                                                        false,
                                                        false,
                                                        true,
                                                        ItemsSourceHelper.IsASubRelationship( fieldType ),
                                                        false,
                                                        foreignKeyDescription );
      }

      return fieldDescriptors;
    }

    private static Dictionary<string, FieldDescriptor> GetFieldsForEntityFramework( Type itemType )
    {
      PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties( itemType );
      Dictionary<string, FieldDescriptor> fieldDescriptors = new Dictionary<string, FieldDescriptor>( propertyDescriptors.Count );

      foreach( PropertyDescriptor propertyDescriptor in propertyDescriptors )
      {
        bool isEntityKey = false;
        bool allowNull = false;

        EdmScalarPropertyAttribute attribute = propertyDescriptor.Attributes[ typeof( EdmScalarPropertyAttribute ) ] as EdmScalarPropertyAttribute;

        if( attribute != null )
        {
          isEntityKey = attribute.EntityKeyProperty;
          allowNull = attribute.IsNullable;
        }

        string name = propertyDescriptor.Name;

        fieldDescriptors[ name ] = new FieldDescriptor(
          name,
          propertyDescriptor.DisplayName,
          propertyDescriptor.PropertyType,
          propertyDescriptor,
          null,
          name,
          ( propertyDescriptor.IsReadOnly || isEntityKey ), // A column must be read-only if it's an EntityKey (except for the InsertionRow)
          ( !propertyDescriptor.IsReadOnly && isEntityKey ),
          allowNull,
          propertyDescriptor.IsBrowsable,
          ItemsSourceHelper.IsASubRelationship( propertyDescriptor.PropertyType ),
          false,
          null );
      }

      return fieldDescriptors;
    }

    private static Dictionary<string, ForeignKeyConstraint> GetForeignKeyConstraints(
      ConstraintCollection constraints )
    {
      Dictionary<string, ForeignKeyConstraint> foreignKeyConstraints =
        new Dictionary<string, ForeignKeyConstraint>();

      // Detect every ForeignKeyConstraints
      foreach( Constraint constraint in constraints )
      {
        ForeignKeyConstraint foreignKeyConstraint = constraint as ForeignKeyConstraint;

        // Not a ForeignKeyConstraint
        if( foreignKeyConstraint == null )
          continue;

        // We only support auto-detection when the ForeignKey is composed of 
        // a single column
        if( ( foreignKeyConstraint.Columns != null )
          && ( foreignKeyConstraint.Columns.Length == 1 ) )
        {
          foreignKeyConstraints.Add( foreignKeyConstraint.Columns[ 0 ].ColumnName, foreignKeyConstraint );
        }
      }

      return foreignKeyConstraints;
    }

    internal static DataGridForeignKeyDescription GetDataGridForeignKeyDescriptionForEnum( Type enumType )
    {
      DataGridForeignKeyDescription foreignKeyDescription = null;

      if( ( enumType != null ) && ( enumType.IsEnum ) )
      {
        foreignKeyDescription = new DataGridForeignKeyDescription();

        // Using "." as default value path will revert to Self when used as 
        // SelectedValuePath when bound to a DataGridForeignKeyDictionary or
        // ComboBox (default editor)
        foreignKeyDescription.ValuePath = ".";
        foreignKeyDescription.ItemsSource = Enum.GetValues( enumType );
        foreignKeyDescription.IsAutoCreated = true;
      }

      return foreignKeyDescription;
    }

    private static DataTableForeignKeyDescription GetDataGridForeignKeyDescriptionForForeignKeyConstraint(
      ForeignKeyConstraint foreignKeyConstraint )
    {
      DataTableForeignKeyDescription foreignKeyDescription = null;

      if( foreignKeyConstraint != null )
      {
        if( ( foreignKeyConstraint.Columns != null )
          && ( foreignKeyConstraint.Columns.Length == 1 ) )
        {
          foreignKeyDescription = new DataTableForeignKeyDescription();
          ( ( DataTableForeignKeyDescription )foreignKeyDescription ).ForeignKeyConstraint = foreignKeyConstraint;
          foreignKeyDescription.IsAutoCreated = true;
        }
      }

      return foreignKeyDescription;
    }

    #endregion PRIVATE STATIC METHODS

    #region PRIVATE FIELDS

    private static readonly object[] EmptyObjectArray = new object[ 0 ];

    private static Type EntityObjectType = Type.GetType(
      "System.Data.Objects.DataClasses.EntityObject, System.Data.Entity, Version=" + _XceedVersionInfo.FrameworkVersion + ", Culture=neutral, PublicKeyToken=b77a5c561934e089",
      false, false );

    #endregion PRIVATE FIELDS

    #region FieldDescriptor NESTED CLASSES

    internal class FieldDescriptor
    {
      #region CONSTRUCTORS

      public FieldDescriptor(
        string name,
        string displayName,
        Type dataType,
        PropertyDescriptor propertyDescriptor,
        string bindingXPath,
        string bindingPath,
        bool readOnly,
        bool overrideReadOnlyForInsertion,
        bool supportDBNull,
        bool browsable,
        bool isASubRelationship,
        bool isDataGridUnboundItemProperty,
        DataGridForeignKeyDescription foreignKeyDescription )
      {
        if( name == null )
          throw new ArgumentNullException( "name" );

        if( displayName == null )
          throw new ArgumentNullException( "displayName" );

        if( dataType == null )
          throw new ArgumentNullException( "dataType" );

        this.ReadOnly = readOnly;
        this.OverrideReadOnlyForInsertion = overrideReadOnlyForInsertion;
        m_name = name;
        m_displayName = displayName;
        m_dataType = dataType;
        m_propertyDescriptor = propertyDescriptor;
        m_bindingPath = bindingPath;
        m_bindingXPath = bindingXPath;
        this.SupportDBNull = supportDBNull;
        this.Browsable = browsable;
        this.IsASubRelationship = isASubRelationship;
        this.IsDataGridUnboundItemProperty = isDataGridUnboundItemProperty;
        this.ForeignKeyDescription = foreignKeyDescription;
      }

      #endregion CONSTRUCTORS

      #region Name PROPERTY

      public string Name
      {
        get
        {
          return m_name;
        }
      }

      private string m_name; // = null

      #endregion Name PROPERTY

      #region DisplayName PROPERTY

      public string DisplayName
      {
        get
        {
          return m_displayName;
        }
      }

      private string m_displayName; // = null

      #endregion DisplayName PROPERTY

      #region BindingPath PROPERTY

      public string BindingPath
      {
        get
        {
          return m_bindingPath;
        }
      }

      private string m_bindingPath; // = null

      #endregion BindingPath PROPERTY

      #region BindingXPath PROPERTY

      public string BindingXPath
      {
        get
        {
          return m_bindingXPath;
        }
      }

      private string m_bindingXPath; // = null

      #endregion BindingXPath PROPERTY

      #region DataType PROPERTY

      public Type DataType
      {
        get
        {
          return m_dataType;
        }
      }

      private Type m_dataType; // = null

      #endregion DataType PROPERTY

      #region PropertyDescriptor PROPERTY

      public PropertyDescriptor PropertyDescriptor
      {
        get
        {
          return m_propertyDescriptor;
        }
      }

      private PropertyDescriptor m_propertyDescriptor; // = null

      #endregion PropertyDescriptor PROPERTY

      #region ReadOnly PROPERTY

      public bool ReadOnly
      {
        get
        {
          return m_flags[ ( int )FieldDescriptorFlags.ReadOnly ];
        }
        private set
        {
          m_flags[ ( int )FieldDescriptorFlags.ReadOnly ] = value;
        }
      }

      #endregion ReadOnly PROPERTY

      #region OverrideReadOnlyForInsertion Property

      public bool OverrideReadOnlyForInsertion
      {
        get
        {
          return m_flags[ ( int )FieldDescriptorFlags.OverrideReadOnlyForInsertion ];
        }
        private set
        {
          m_flags[ ( int )FieldDescriptorFlags.OverrideReadOnlyForInsertion ] = value;
        }
      }

      #endregion OverrideReadOnlyForInsertion Property

      #region SupportDBNull PROPERTY

      public bool SupportDBNull
      {
        get
        {
          return m_flags[ ( int )FieldDescriptorFlags.SupportDBNull ];
        }
        private set
        {
          m_flags[ ( int )FieldDescriptorFlags.SupportDBNull ] = value;
        }
      }

      #endregion SupportDBNull PROPERTY

      #region Browsable Property

      public bool Browsable
      {
        get
        {
          return m_flags[ ( int )FieldDescriptorFlags.Browsable ];
        }
        private set
        {
          m_flags[ ( int )FieldDescriptorFlags.Browsable ] = value;
        }
      }

      #endregion Browsable Property

      #region IsASubRelationship Property

      public bool IsASubRelationship
      {
        get
        {
          return m_flags[ ( int )FieldDescriptorFlags.IsASubRelationship ];
        }
        private set
        {
          m_flags[ ( int )FieldDescriptorFlags.IsASubRelationship ] = value;
        }
      }

      #endregion IsASubRelationship Property

      #region IsDataGridUnboundItemProperty Property

      public bool IsDataGridUnboundItemProperty
      {
        get
        {
          return m_flags[ ( int )FieldDescriptorFlags.IsDataGridUnboundItemProperty ];
        }
        private set
        {
          m_flags[ ( int )FieldDescriptorFlags.IsDataGridUnboundItemProperty ] = value;
        }
      }

      #endregion IsDataGridUnboundItemProperty Property

      #region ForeignKeyDescription

      public DataGridForeignKeyDescription ForeignKeyDescription
      {
        get;
        private set;
      }

      #endregion

      #region Private Fields

      private BitVector32 m_flags = new BitVector32();

      #endregion

      #region FieldDescriptor Flags

      [Flags]
      private enum FieldDescriptorFlags
      {
        ReadOnly = 1,
        SupportDBNull = 2,
        Browsable = 4,
        IsASubRelationship = 8,
        IsForeignKey = 16,
        OverrideReadOnlyForInsertion = 32,
        IsDataGridUnboundItemProperty = 64
      }

      #endregion
    }

    #endregion NESTED CLASSES

    internal static System.Windows.Data.Binding AutoCreateDisplayMemberBinding( Column column, DataGridContext dataGridContext, object dataItem, out bool isDataGridUnboundItemProperty )
    {
      System.Windows.Data.Binding displayMemberBinding = null;

      if( column == null )
        throw new ArgumentNullException( "column" );

      string name = column.FieldName;

      // Don't create the default binding if FieldName is null and we're in design-time.
      if( !string.IsNullOrEmpty( name ) )
      {
        ItemsSourceHelper.FieldDescriptor fieldDescriptor;
        dataGridContext.ItemsSourceFieldDescriptors.TryGetValue( name, out fieldDescriptor );

        isDataGridUnboundItemProperty = ( fieldDescriptor == null ) ? false :
          fieldDescriptor.IsDataGridUnboundItemProperty;

        displayMemberBinding = ItemsSourceHelper.CreateDefaultBinding(
          dataItem is DataRow, name, fieldDescriptor,
          false, ( column.ReadOnly && !column.OverrideReadOnlyForInsertion ), typeof( object ) );
      }
      else
      {
        isDataGridUnboundItemProperty = false;
      }

      return displayMemberBinding;
    }
  }
}
