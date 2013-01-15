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
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml;
using System.Data;

using Xceed.Wpf.DataGrid.Converters;
using Xceed.Wpf.DataGrid.FilterCriteria;

namespace Xceed.Wpf.DataGrid
{
  public partial class DataGridItemProperty : DataGridItemPropertyBase
  {

    #region CONSTRUCTORS

    public DataGridItemProperty()
    {
    }

    public DataGridItemProperty( string name, string valuePath, Type dataType )
      : this( name, null, valuePath, dataType )
    {
    }

    public DataGridItemProperty( string name, string valueXPath, string valuePath, Type dataType )
    {
      if( string.IsNullOrEmpty( name ) )
        throw new ArgumentException( "name cannot be null or empty.", "name" );

      if( ( string.IsNullOrEmpty( valuePath ) ) && ( string.IsNullOrEmpty( valueXPath ) ) )
        throw new ArgumentException( "valuePath or valueXPath cannot be null or empty." );

      this.Initialize(
        name,
        null,
        null,
        valueXPath,
        valuePath,
        dataType,
        false,
        null,
        null,
        null,
        null );
    }

    public DataGridItemProperty( string name, string valuePath, Type dataType, bool isReadOnly )
      : this( name, null, valuePath, dataType, isReadOnly )
    {
    }

    public DataGridItemProperty( string name, string valueXPath, string valuePath, Type dataType, bool isReadOnly )
    {
      if( string.IsNullOrEmpty( name ) )
        throw new ArgumentException( "name cannot be null or empty.", "name" );

      if( ( string.IsNullOrEmpty( valuePath ) ) && ( string.IsNullOrEmpty( valueXPath ) ) )
        throw new ArgumentException( "valuePath or valueXPath cannot be null or empty." );

      this.Initialize(
        name,
        null,
        null,
        valueXPath,
        valuePath,
        dataType,
        false,
        isReadOnly,
        null,
        null,
        null );
    }

    public DataGridItemProperty( PropertyDescriptor propertyDescriptor )
      : this( propertyDescriptor, false )
    {
    }

    public DataGridItemProperty( string name, Type dataType )
    {
      if( string.IsNullOrEmpty( name ) )
        throw new ArgumentException( "The name must not be null (Nothing in Visual Basic) or empty.", "name" );

      if( dataType == null )
        throw new ArgumentNullException( "dataType" );

      this.Initialize(
        name,
        null,
        null,
        null,
        null,
        dataType,
        false,
        null,
        null,
        null,
        null );
    }

    public DataGridItemProperty( string name, Type dataType, bool isReadOnly )
      : this( name, dataType, isReadOnly, null )
    {
    }

    public DataGridItemProperty( string name, Type dataType, bool isReadOnly, Nullable<bool> overrideReadOnlyForInsertion )
    {
      if( string.IsNullOrEmpty( name ) )
        throw new ArgumentException( "The name must not be null (Nothing in Visual Basic) or empty.", "name" );

      if( dataType == null )
        throw new ArgumentNullException( "dataType" );

      this.Initialize(
        name,
        null,
        null,
        null,
        null,
        dataType,
        false,
        isReadOnly,
        overrideReadOnlyForInsertion,
        null,
        null );
    }

    public DataGridItemProperty( string name, PropertyDescriptor propertyDescriptor )
    {
      if( string.IsNullOrEmpty( name ) )
        throw new ArgumentException( "The name must not be null (Nothing in Visual Basic) or empty.", "name" );

      if( propertyDescriptor == null )
        throw new ArgumentNullException( "propertyDescriptor" );

      this.Initialize(
        name,
        propertyDescriptor,
        null,
        null,
        null,
        null,
        false,
        null,
        null,
        null,
        null );
    }

    internal DataGridItemProperty( PropertyDescriptor propertyDescriptor, bool isAutoCreated )
    {
      if( propertyDescriptor == null )
        throw new ArgumentNullException( "propertyDescriptor" );

      this.Initialize(
        propertyDescriptor.Name,
        propertyDescriptor,
        null,
        null,
        null,
        null,
        isAutoCreated,
        null,
        null,
        null,
        null );
    }

    internal DataGridItemProperty(
      string name,
      PropertyDescriptor propertyDescriptor,
      string title,
      string valueXPath,
      string valuePath,
      Type dataType,
      bool isAutoCreated,
      Nullable<bool> isReadOnly,
      Nullable<bool> overrideReadOnlyForInsertion,
      Nullable<bool> isASubRelationship,
      DataGridForeignKeyDescription foreignKeyDescription )
    {
      if( string.IsNullOrEmpty( name ) )
        throw new ArgumentException( "name cannot be null or empty.", "name" );

      if( ( string.IsNullOrEmpty( valuePath ) ) && ( string.IsNullOrEmpty( valueXPath ) ) && ( propertyDescriptor == null ) )
        throw new ArgumentException( "The valuePath, valueXPath, or propertyDescriptor must be provided." );

      this.Initialize(
        name,
        propertyDescriptor,
        title,
        valueXPath,
        valuePath,
        dataType,
        isAutoCreated,
        isReadOnly,
        overrideReadOnlyForInsertion,
        isASubRelationship,
        foreignKeyDescription );
    }

    protected DataGridItemProperty( DataGridItemProperty template )
      : base( template )
    {
      // this.IsAutoCreated = false, after a clone, we consider the ItemProperty not AutoCreated.
      m_propertyDescriptor = template.m_propertyDescriptor;
      m_valueXPath = template.m_valueXPath;
      m_valuePath = template.m_valuePath;
    }

    private void Initialize(
      string name,
      PropertyDescriptor propertyDescriptor,
      string title,
      string valueXPath,
      string valuePath,
      Type dataType,
      bool isAutoCreated,
      Nullable<bool> isReadOnly,
      Nullable<bool> overrideReadOnlyForInsertion,
      Nullable<bool> isASubRelationship,
      DataGridForeignKeyDescription foreignKeyDescription )
    {
      this.IsAutoCreated = isAutoCreated;
      m_valueXPath = valueXPath;
      m_valuePath = valuePath;
      m_propertyDescriptor = propertyDescriptor;

      if( m_propertyDescriptor == null )
      {
        if( ( string.IsNullOrEmpty( m_valueXPath ) ) && ( m_valuePath == "." ) )
          m_propertyDescriptor = new SelfPropertyDescriptor( dataType );
      }

      if( m_propertyDescriptor != null )
      {
        this.Browsable = m_propertyDescriptor.IsBrowsable;

        if( m_propertyDescriptor.IsReadOnly )
          isReadOnly = m_propertyDescriptor.IsReadOnly;
      }

      if( title == null )
      {
        if( m_propertyDescriptor != null )
        {
          title = m_propertyDescriptor.DisplayName;
        }
      }

      if( isReadOnly == null )
      {
        if( m_propertyDescriptor != null )
        {
          isReadOnly = m_propertyDescriptor.IsReadOnly;
        }
      }

      if( dataType == null )
      {
        if( m_propertyDescriptor != null )
        {
          dataType = m_propertyDescriptor.PropertyType;
        }
      }

      this.ForeignKeyDescription = foreignKeyDescription;

      base.Initialize( name, title, dataType, isReadOnly, overrideReadOnlyForInsertion, isASubRelationship );
    }

    #endregion CONSTRUCTORS

    #region ValuePath Property

    public string ValuePath
    {
      get
      {
        return m_valuePath;
      }
      set
      {
        if( this.Initialized )
          throw new InvalidOperationException( "An attempt was made to change the ValuePath of a property already added to a containing collection." );

        this.SetValuePath( value );
      }
    }

    internal void SetValuePath( string valuePath )
    {
      m_valuePath = valuePath;
      m_bindingPathValueExtractorForRead = null;
      m_bindingPathValueExtractorForWrite = null;
    }

    private string m_valuePath;

    #endregion ValuePath Property

    #region ValueXPath Property

    public string ValueXPath
    {
      get
      {
        return m_valueXPath;
      }
      set
      {
        if( this.Initialized )
          throw new InvalidOperationException( "An attempt was made to change the ValueXPath of a property already added to a containing collection." );

        this.SetValueXPath( value );
      }
    }

    internal void SetValueXPath( string valueXPath )
    {
      m_valueXPath = valueXPath;
      m_bindingPathValueExtractorForRead = null;
      m_bindingPathValueExtractorForWrite = null;
    }

    private string m_valueXPath;

    #endregion ValueXPath Property

    #region PropertyDescriptor Property

    public PropertyDescriptor PropertyDescriptor
    {
      get
      {
        return m_propertyDescriptor;
      }
      set
      {
        if( this.Initialized )
          throw new InvalidOperationException( "An attempt was made to change the PropertyDescriptor of a property already added to a containing collection." );

        this.SetPropertyDescriptor( value );
      }
    }

    internal void SetPropertyDescriptor( PropertyDescriptor propertyDescriptor )
    {
      m_propertyDescriptor = propertyDescriptor;

      if( ( m_propertyDescriptor != null ) && ( !this.IsReadOnly ) )
        this.SetIsReadOnly( propertyDescriptor.IsReadOnly );

      m_bindingPathValueExtractorForRead = null;
      m_bindingPathValueExtractorForWrite = null;
    }

    private PropertyDescriptor m_propertyDescriptor;

    #endregion PropertyDescriptor Property

    #region IsAutoCreated

    public bool IsAutoCreated
    {
      get;
      private set;
    }

    #endregion IsAutoCreated

    #region PUBLIC METHODS

    public override object Clone()
    {
      Type type = this.GetType();

      if( type == typeof( DataGridItemProperty ) )
        return new DataGridItemProperty( this );

      return base.Clone();
    }

    #endregion PUBLIC METHODS

    #region PROTECTED METHODS

    protected override object GetValueCore( object component )
    {
      if( m_propertyDescriptor != null )
      {
        object value;

        try
        {
          value = m_propertyDescriptor.GetValue( component );
        }
        catch( DataException )
        {
          // We have to return null if the datarow is deleted from the DataTable.
          // When the System.Data.DataRow have RowState == detached, it can be because it has been deleted
          // or it being inserted, nothing special found to differentiate that 2 state.  So doing a try catch.
          value = null;
        }

        CultureInfo converterCulture = this.ConverterCulture;

        if( converterCulture == null )
          converterCulture = CultureInfo.InvariantCulture;

        return this.GetBindingConverter( component ).Convert(
          value, this.DataType,
          this.ConverterParameter, converterCulture );
      }

      if( m_bindingPathValueExtractorForRead == null )
      {
        CultureInfo converterCulture = this.ConverterCulture;

        if( converterCulture == null )
          converterCulture = CultureInfo.InvariantCulture;

        PropertyPath propertyPath = null;

        if( !string.IsNullOrEmpty( this.ValuePath ) )
          propertyPath = new PropertyPath( this.ValuePath, BindingPathValueExtractor.EmptyObjectArray );

        m_bindingPathValueExtractorForRead = new BindingPathValueExtractor(
          this.ValueXPath, propertyPath, false,
          this.DataType,
          this.GetBindingConverter( component ),
          this.ConverterParameter, converterCulture );
      }

      return m_bindingPathValueExtractorForRead.GetValueFromItem( component );
    }

    protected override void SetValueCore( object component, object value )
    {
      if( m_propertyDescriptor != null )
      {
        CultureInfo converterCulture = this.ConverterCulture;

        if( converterCulture == null )
          converterCulture = CultureInfo.InvariantCulture;

        object convertedValue = this.GetBindingConverter( component ).ConvertBack(
          value,
          m_propertyDescriptor.PropertyType,
          this.ConverterParameter,
          converterCulture );

        m_propertyDescriptor.SetValue( component, convertedValue );
      }
      else
      {
        if( m_bindingPathValueExtractorForWrite == null )
        {
          CultureInfo converterCulture = this.ConverterCulture;

          if( converterCulture == null )
            converterCulture = CultureInfo.InvariantCulture;

          PropertyPath propertyPath = null;

          if( !string.IsNullOrEmpty( this.ValuePath ) )
            propertyPath = new PropertyPath( this.ValuePath, BindingPathValueExtractor.EmptyObjectArray );

          m_bindingPathValueExtractorForWrite = new BindingPathValueExtractor(
            this.ValueXPath, propertyPath, true,
            this.DataType,
            this.GetBindingConverter( component ), this.ConverterParameter, converterCulture );
        }

        m_bindingPathValueExtractorForWrite.SetValueToItem( component, value );
      }

      base.SetValueCore( component, value );
    }

    #endregion PROTECTED METHODS

    #region INTERNAL METHODS

    internal override PropertyDescriptorFromItemPropertyBase GetPropertyDescriptorForBindingCore()
    {
      return new PropertyDescriptorFromItemProperty( this );
    }

    internal override void SetUnspecifiedPropertiesValues( DataGridItemPropertyCollection itemPropertyCollection )
    {
      // ContainingCollection.ItemType and ContainingCollection.DefaultItemProperties can be null at first when this is 
      // the ItemProperties of a DetailGrid.
      // the SetUnspecifiedPropertiesValues will be recall when both this.ItemType and this.DefaultItemProperties are affected.

      if( itemPropertyCollection == null )
        return;

      DataGridItemProperty defaultItemProperty = null;
      bool itemIsXceedDataRow = ( itemPropertyCollection.ItemType != null ) ? typeof( DataRow ).IsAssignableFrom( itemPropertyCollection.ItemType ) : false;

      if( ( string.IsNullOrEmpty( this.ValueXPath ) )
        && ( string.IsNullOrEmpty( this.ValuePath ) )
        && ( this.PropertyDescriptor == null ) )
      {
        if( itemIsXceedDataRow )
        {
          this.PropertyDescriptor = new UnboundDataRowPropertyDescriptor( this.Name, this.DataType );
        }
        else
        {
          defaultItemProperty = itemPropertyCollection.FindDefaultItemProperty( this.Name ) as DataGridItemProperty;

          if( defaultItemProperty == null )
          {
            if( this.Name == "." )
            {
              this.SetPropertyDescriptor( new SelfPropertyDescriptor( this.DataType ) );
              this.SetValuePath( "." );
              this.SetIsReadOnly( true );
              this.SetOverrideReadOnlyForInsertion( false );
            }
            else if( itemPropertyCollection.DefaultItemProperties != null )
            {
              // I have to add this particular exception case to make sure that when the ItemProperty is "re-normalized" when the first DataGridCollectionView
              // is created for it, then the ValuePath is still null or empty (allowing it to be re-normalized)
              this.SetValuePath( this.Name );
            }
          }
          else
          {
            this.SetPropertyDescriptor( defaultItemProperty.PropertyDescriptor );
            this.SetValuePath( defaultItemProperty.ValuePath );
            this.SetValueXPath( defaultItemProperty.ValueXPath );
          }
        }
      }

      if( this.DataType == null )
      {
        //only try to affect the DataType if the DefaultItemProperties were set. (will not be the case when XAML parsing DetailDescriptions)
        if( itemPropertyCollection.DefaultItemProperties != null )
        {
          if( defaultItemProperty == null )
            defaultItemProperty = itemPropertyCollection.FindDefaultItemProperty( this.Name ) as DataGridItemProperty;

          if( defaultItemProperty == null )
          {
            throw new InvalidOperationException( "An attempt was made to add an item (" + this.Name + ") without specifying its data type." );
          }

          this.SetDataType( defaultItemProperty.DataType );
        }
      }

      if( string.IsNullOrEmpty( this.Title ) )
      {
        if( itemIsXceedDataRow )
        {
          this.Title = this.Name;
        }
        else
        {
          if( defaultItemProperty == null )
            defaultItemProperty = itemPropertyCollection.FindDefaultItemProperty( this.Name ) as DataGridItemProperty;

          if( defaultItemProperty == null )
          {
            this.Title = this.Name;
          }
          else
          {
            this.Title = defaultItemProperty.Title;
          }
        }
      }

      if( !this.IsReadOnly )
      {
        if( defaultItemProperty == null )
          defaultItemProperty = itemPropertyCollection.FindDefaultItemProperty( this.Name ) as DataGridItemProperty;

        if( defaultItemProperty != null )
        {
          this.SetIsReadOnly( defaultItemProperty.IsReadOnly );
        }
      }

      if( this.OverrideReadOnlyForInsertion == null )
      {
        //only try to affect the DataType if the DefaultItemProperties were set. (will not be the case when XAML parsing DetailDescriptions)
        if( itemPropertyCollection.DefaultItemProperties != null )
        {
          if( defaultItemProperty == null )
            defaultItemProperty = itemPropertyCollection.FindDefaultItemProperty( this.Name ) as DataGridItemProperty;

          this.SetOverrideReadOnlyForInsertion( ( defaultItemProperty != null ) && ( defaultItemProperty.OverrideReadOnlyForInsertion.HasValue )
            ? defaultItemProperty.OverrideReadOnlyForInsertion.Value
            : false );
        }
      }

      if( ( !string.IsNullOrEmpty( this.ValueXPath ) ) && ( string.IsNullOrEmpty( this.ValuePath ) ) )
        this.SetValuePath( "InnerText" );

      bool foreignKeyDescriptionIsNull = ( this.ForeignKeyDescription == null );
      bool foreignKeyDescriptionItemsSourceIsNull = false;

      if( !foreignKeyDescriptionIsNull )
      {
        foreignKeyDescriptionItemsSourceIsNull = ( this.ForeignKeyDescription.ItemsSource == null );
      }

      // Update the ForeignKeyDescription if not set
      if( foreignKeyDescriptionIsNull || foreignKeyDescriptionItemsSourceIsNull )
      {
        if( defaultItemProperty == null )
          defaultItemProperty = itemPropertyCollection.FindDefaultItemProperty( this.Name ) as DataGridItemProperty;

        if( ( defaultItemProperty != null ) && ( defaultItemProperty.ForeignKeyDescription != null ) )
        {
          if( foreignKeyDescriptionIsNull )
          {
            this.SetForeignKeyDescription( defaultItemProperty.ForeignKeyDescription );
          }
          else
          {
            if( foreignKeyDescriptionItemsSourceIsNull )
            {
              this.ForeignKeyDescription.ItemsSource = defaultItemProperty.ForeignKeyDescription.ItemsSource;
            }
          }
        }
      }
    }

    #endregion INTERNAL METHODS

    #region PRIVATE FIELDS

    private BindingPathValueExtractor m_bindingPathValueExtractorForRead;
    private BindingPathValueExtractor m_bindingPathValueExtractorForWrite;

    #endregion PRIVATE FIELDS
  }
}
