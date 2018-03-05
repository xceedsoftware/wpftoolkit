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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  [DebuggerDisplay( "Name = {Name}" )]
  public partial class DataGridItemProperty : DataGridItemPropertyBase
  {
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
      Nullable<bool> isDisplayable,
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
        isDisplayable,
        isASubRelationship,
        foreignKeyDescription );
    }

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "This constructor is obsolete and should no longer be used.", true )]
    protected DataGridItemProperty( DataGridItemProperty template )
    {
      throw new NotSupportedException();
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
      Nullable<bool> isDisplayable,
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
        this.IsBrowsable = m_propertyDescriptor.IsBrowsable;

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

      base.Initialize( name, title, dataType, isReadOnly, overrideReadOnlyForInsertion, isDisplayable, isASubRelationship );
    }

    #region ValuePath Property

    public string ValuePath
    {
      get
      {
        return m_valuePath;
      }
      set
      {
        if( this.IsSealed )
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

    #endregion

    #region ValueXPath Property

    public string ValueXPath
    {
      get
      {
        return m_valueXPath;
      }
      set
      {
        if( this.IsSealed )
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

    #endregion

    #region PropertyDescriptor Property

    public PropertyDescriptor PropertyDescriptor
    {
      get
      {
        return m_propertyDescriptor;
      }
      set
      {
        if( this.IsSealed )
          throw new InvalidOperationException( "An attempt was made to change the PropertyDescriptor of a property already added to a containing collection." );

        this.SetPropertyDescriptor( value );
      }
    }

    internal void SetPropertyDescriptor( PropertyDescriptor propertyDescriptor )
    {
      m_propertyDescriptor = propertyDescriptor;

      if( ( m_propertyDescriptor != null ) && ( !this.IsReadOnly ) )
      {
        this.SetIsReadOnly( propertyDescriptor.IsReadOnly );
      }

      m_bindingPathValueExtractorForRead = null;
      m_bindingPathValueExtractorForWrite = null;
    }

    private PropertyDescriptor m_propertyDescriptor;

    #endregion

    #region IsAutoCreated Property

    public bool IsAutoCreated
    {
      get;
      private set;
    }

    #endregion

    #region FieldName Internal Property

    internal override string FieldName
    {
      get
      {
        var descriptor = this.PropertyDescriptor;
        if( descriptor != null )
          return descriptor.Name;

        return base.FieldName;
      }
    }

    #endregion

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
          // We have to return null if the datarow is deleted from the DataTable.  When the System.Data.DataRow has RowState == detached,
          // it can be because it has been deleted or it being inserted, nothing special found to differentiate that 2 state.  So doing a try catch.
          value = null;
        }

        CultureInfo converterCulture = this.ConverterCulture;

        if( converterCulture == null )
        {
          converterCulture = CultureInfo.InvariantCulture;
        }

        return this.GetBindingConverter( component ).Convert( value, this.DataType, this.ConverterParameter, converterCulture );
      }

      if( m_bindingPathValueExtractorForRead == null )
      {
        CultureInfo converterCulture = this.ConverterCulture;

        if( converterCulture == null )
        {
          converterCulture = CultureInfo.InvariantCulture;
        }

        PropertyPath propertyPath = null;

        if( !string.IsNullOrEmpty( this.ValuePath ) )
        {
          propertyPath = new PropertyPath( this.ValuePath, BindingPathValueExtractor.EmptyObjectArray );
        }

        m_bindingPathValueExtractorForRead = new BindingPathValueExtractor( this.ValueXPath, propertyPath, false, this.DataType,
                                                                            this.GetBindingConverter( component ), this.ConverterParameter, converterCulture );
      }

      return m_bindingPathValueExtractorForRead.GetValueFromItem( component );
    }

    protected override void SetValueCore( object component, object value )
    {
      if( m_propertyDescriptor != null )
      {
        CultureInfo converterCulture = this.ConverterCulture;

        if( converterCulture == null )
        {
          converterCulture = CultureInfo.InvariantCulture;
        }

        object convertedValue = this.GetBindingConverter( component ).ConvertBack( value, m_propertyDescriptor.PropertyType, this.ConverterParameter, converterCulture );

        m_propertyDescriptor.SetValue( component, convertedValue );
      }
      else
      {
        if( m_bindingPathValueExtractorForWrite == null )
        {
          CultureInfo converterCulture = this.ConverterCulture;

          if( converterCulture == null )
          {
            converterCulture = CultureInfo.InvariantCulture;
          }

          PropertyPath propertyPath = null;

          if( !string.IsNullOrEmpty( this.ValuePath ) )
          {
            propertyPath = new PropertyPath( this.ValuePath, BindingPathValueExtractor.EmptyObjectArray );
          }

          m_bindingPathValueExtractorForWrite = new BindingPathValueExtractor( this.ValueXPath, propertyPath, true, this.DataType,
                                                                               this.GetBindingConverter( component ), this.ConverterParameter, converterCulture );
        }

        m_bindingPathValueExtractorForWrite.SetValueToItem( component, value );
      }

      base.SetValueCore( component, value );
    }

    internal override PropertyDescriptorFromItemPropertyBase GetPropertyDescriptorForBindingCore()
    {
      return new PropertyDescriptorFromItemProperty( this );
    }

    internal override void SetUnspecifiedPropertiesValues(
      PropertyDescription description,
      Type itemType,
      bool defaultItemPropertiesCreated )
    {
      var itemIsXceedDataRow = ( itemType != null ) ? typeof( DataRow ).IsAssignableFrom( itemType ) : false;

      if( ( this.PropertyDescriptor == null ) && string.IsNullOrEmpty( this.ValuePath ) && string.IsNullOrEmpty( this.ValueXPath ) )
      {
        if( itemIsXceedDataRow )
        {
          this.SetPropertyDescriptor( new UnboundDataRowPropertyDescriptor( this.Name, this.DataType ) );
        }
        else
        {
          if( description == null )
          {
            if( this.Name == "." )
            {
              this.SetPropertyDescriptor( new SelfPropertyDescriptor( this.DataType ) );
              this.SetValuePath( "." );
              this.SetIsReadOnly( true );
              this.SetOverrideReadOnlyForInsertion( false );
            }
          }
          else
          {
            this.SetPropertyDescriptor( description.PropertyDescriptor );
            this.SetValuePath( description.Path );
            this.SetValueXPath( description.XPath );
          }
        }

        if( defaultItemPropertiesCreated && ( this.PropertyDescriptor == null ) && string.IsNullOrEmpty( this.ValuePath ) && string.IsNullOrEmpty( this.ValueXPath ) )
        {
          // I have to add this particular exception case to make sure that when the ItemProperty is "re-normalized" when the first DataGridCollectionView
          // is created for it, then the ValuePath is still null or empty (allowing it to be re-normalized)
          this.SetValuePath( this.Name );
        }
      }

      if( this.DataType == null )
      {
        //only try to affect the DataType if the DefaultPropertyDescriptions were set. (will not be the case when XAML parsing DetailDescriptions)
        if( defaultItemPropertiesCreated )
        {
          if( description == null )
            throw new InvalidOperationException( "An attempt was made to add an item (" + this.Name + ") without specifying its data type." );

          this.SetDataType( description.DataType );
        }
      }

      if( string.IsNullOrEmpty( this.Title ) )
      {
        if( !itemIsXceedDataRow && ( description != null ) && !string.IsNullOrEmpty( description.DisplayName ) )
        {
          this.Title = description.DisplayName;
        }
        else
        {
          this.Title = this.Name;
        }
      }

      if( !this.IsReadOnly )
      {
        if( description != null )
        {
          this.SetIsReadOnly( description.IsReadOnly );
        }
      }

      if( this.OverrideReadOnlyForInsertion == null )
      {
        //only try to affect the DataType if the DefaultItemProperties were set. (will not be the case when XAML parsing DetailDescriptions)
        if( defaultItemPropertiesCreated )
        {
          this.SetOverrideReadOnlyForInsertion( ( description != null ) ? description.OverrideReadOnlyForInsertion : false );
        }
      }

      if( ( !string.IsNullOrEmpty( this.ValueXPath ) ) && ( string.IsNullOrEmpty( this.ValuePath ) ) )
      {
        this.SetValuePath( "InnerText" );
      }

      var foreignKeyDescriptionIsNull = ( this.ForeignKeyDescription == null );
      var foreignKeyDescriptionItemsSourceIsNull = false;

      if( !foreignKeyDescriptionIsNull )
      {
        foreignKeyDescriptionItemsSourceIsNull = ( this.ForeignKeyDescription.ItemsSource == null );
      }

      // Update the ForeignKeyDescription if not set
      if( foreignKeyDescriptionIsNull || foreignKeyDescriptionItemsSourceIsNull )
      {
        if( ( description != null ) && ( description.ForeignKeyDescription != null ) )
        {
          if( foreignKeyDescriptionIsNull )
          {
            this.SetForeignKeyDescription( description.ForeignKeyDescription );
          }
          else
          {
            if( foreignKeyDescriptionItemsSourceIsNull )
            {
              this.ForeignKeyDescription.ItemsSource = description.ForeignKeyDescription.ItemsSource;
            }
          }
        }
      }
    }

    #region Private Fields

    private BindingPathValueExtractor m_bindingPathValueExtractorForRead;
    private BindingPathValueExtractor m_bindingPathValueExtractorForWrite;

    #endregion
  }
}
