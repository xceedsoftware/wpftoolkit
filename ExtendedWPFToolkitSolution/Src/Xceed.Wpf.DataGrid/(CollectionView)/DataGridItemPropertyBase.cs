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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Data;
using Xceed.Wpf.DataGrid.Converters;
using Xceed.Wpf.DataGrid.Utils;

namespace Xceed.Wpf.DataGrid
{
  [DebuggerDisplay( "Name = {Name}" )]
  public abstract partial class DataGridItemPropertyBase : INotifyPropertyChanged, ICloneable
  {
    #region Static Fields

    internal static readonly string CalculateDistinctValuesPropertyName = PropertyHelper.GetPropertyName( ( DataGridItemPropertyBase i ) => i.CalculateDistinctValues );
    internal static readonly string ContainingCollectionPropertyName = PropertyHelper.GetPropertyName( ( DataGridItemPropertyBase i ) => i.ContainingCollection );
    internal static readonly string ForeignKeyDescriptionPropertyName = PropertyHelper.GetPropertyName( ( DataGridItemPropertyBase i ) => i.ForeignKeyDescription );
    internal static readonly string GroupSortStatResultPropertyNamePropertyName = PropertyHelper.GetPropertyName( ( DataGridItemPropertyBase i ) => i.GroupSortStatResultPropertyName );
    internal static readonly string IsNameSealedPropertyName = PropertyHelper.GetPropertyName( ( DataGridItemPropertyBase i ) => i.IsNameSealed );
    internal static readonly string IsSealedPropertyName = PropertyHelper.GetPropertyName( ( DataGridItemPropertyBase i ) => i.IsSealed );
    internal static readonly string ItemPropertiesInternalPropertyName = PropertyHelper.GetPropertyName( ( DataGridItemPropertyBase i ) => i.ItemPropertiesInternal );
    internal static readonly string MaxDistinctValuesPropertyName = PropertyHelper.GetPropertyName( ( DataGridItemPropertyBase i ) => i.MaxDistinctValues );
    internal static readonly string SynonymPropertyName = PropertyHelper.GetPropertyName( ( DataGridItemPropertyBase i ) => i.Synonym );

    #endregion

    protected DataGridItemPropertyBase()
    {
      this.SetIsDisplayable( true );
      this.IsBrowsable = true;
    }

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "This constructor is obsolete and should no longer be used.", true )]
    protected DataGridItemPropertyBase( DataGridItemPropertyBase template )
      : this()
    {
      throw new NotSupportedException();
    }

    protected void Initialize(
      string name,
      string title,
      Type dataType,
      Nullable<bool> isReadOnly,
      Nullable<bool> overrideReadOnlyForInsertion,
      Nullable<bool> isDisplayable,
      Nullable<bool> isASubRelationship )
    {
      if( string.IsNullOrEmpty( name ) )
        throw new ArgumentException( "name cannot be null or empty.", "name" );

      m_name = name;

      if( title == null )
      {
        m_title = name;
      }
      else
      {
        m_title = title;
      }

      if( isReadOnly.HasValue )
      {
        this.SetIsReadOnly( isReadOnly.Value );
      }

      this.SetOverrideReadOnlyForInsertion( overrideReadOnlyForInsertion );
      m_dataType = dataType;

      if( isDisplayable.HasValue )
      {
        this.SetIsDisplayable( isDisplayable.Value );
      }

      if( isASubRelationship != null )
      {
        this.SetIsASubRelationship( isASubRelationship );
      }
    }

    #region Name Property

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    public string Name
    {
      get
      {
        return m_name;
      }
      set
      {
        if( string.IsNullOrEmpty( value ) )
          throw new ArgumentException( "Name is null (Nothing in Visual Basic) or empty.", "Name" );

        if( this.IsNameSealed || this.IsSealed )
          throw new InvalidOperationException( "An attempt was made to change the name of a property already added to a containing collection." );

        m_name = value;
      }
    }

    private string m_name;

    #endregion

    #region DataType Property

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    public Type DataType
    {
      get
      {
        return m_dataType;
      }
      set
      {
        if( this.IsSealed )
          throw new InvalidOperationException( "An attempt was made to change the DataType of a property already added to a containing collection." );

        this.SetDataType( value );
      }
    }

    internal void SetDataType( Type dataType )
    {
      if( dataType == null )
        throw new ArgumentNullException( "dataType" );

      m_dataType = dataType;
    }

    private Type m_dataType;

    #endregion

    #region IsReadOnly Property

    public bool IsReadOnly
    {
      get
      {
        return m_flags[ DataGridItemPropertyBaseFlags.IsReadOnly ];
      }
      set
      {
        if( this.IsSealed )
          throw new InvalidOperationException( "An attempt was made to change the IsReadOnly property of a DataGridItemProperty already added to a containing collection." );

        this.SetIsReadOnly( value );
      }
    }

    internal void SetIsReadOnly( bool isReadOnly )
    {
      m_flags[ DataGridItemPropertyBaseFlags.IsReadOnly ] = isReadOnly;
    }

    #endregion

    #region OverrideReadOnlyForInsertion Property

    public Nullable<bool> OverrideReadOnlyForInsertion
    {
      get
      {
        if( !m_flags[ DataGridItemPropertyBaseFlags.IsOverrideReadOnlyForInsertionSet ] )
          return null;

        return m_flags[ DataGridItemPropertyBaseFlags.IsOverrideReadOnlyForInsertion ];
      }
      set
      {
        if( this.IsSealed )
          throw new InvalidOperationException( "An attempt was made to change the OverrideReadOnlyForInsertion property of a DataGridItemProperty already added to a containing collection." );

        this.SetOverrideReadOnlyForInsertion( value );
      }
    }

    internal void SetOverrideReadOnlyForInsertion( Nullable<bool> overrideReadOnlyForInsertion )
    {
      if( overrideReadOnlyForInsertion.HasValue )
      {
        m_flags[ DataGridItemPropertyBaseFlags.IsOverrideReadOnlyForInsertion ] = overrideReadOnlyForInsertion.Value;
        m_flags[ DataGridItemPropertyBaseFlags.IsOverrideReadOnlyForInsertionSet ] = true;
      }
      else
      {
        m_flags[ DataGridItemPropertyBaseFlags.IsOverrideReadOnlyForInsertionSet
               | DataGridItemPropertyBaseFlags.IsOverrideReadOnlyForInsertion ] = false;
      }
    }

    #endregion

    #region Title Property

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    public string Title
    {
      get
      {
        return m_title;
      }
      set
      {
        if( value == null )
          throw new ArgumentNullException( "Title" );

        m_title = value;
      }
    }

    private string m_title;

    #endregion

    #region Synonym Property

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    public string Synonym
    {
      get
      {
        return m_synonym;
      }
      set
      {
        if( this.IsSealed )
          throw new InvalidOperationException( "An attempt was made to change the Synonym of a property already added to a containing collection." );

        this.SetSynonym( value );
      }
    }

    internal void SetSynonym( string value )
    {
      if( value == m_synonym )
        return;

      var wasSealed = this.IsSealed;
      this.IsSealed = false;

      m_synonym = value;

      this.IsSealed = wasSealed;
      this.OnPropertyChanged( DataGridItemPropertyBase.SynonymPropertyName );
    }

    private string m_synonym;

    #endregion

    #region SortComparer Property

    public IComparer SortComparer
    {
      get
      {
        return m_sortComparer;
      }
      set
      {
        m_sortComparer = value;
      }
    }

    private IComparer m_sortComparer;

    #endregion

    #region Converter Property

    public IValueConverter Converter
    {
      get
      {
        return m_converter;
      }
      set
      {
        if( this.IsSealed )
          throw new InvalidOperationException( "An attempt was made to change the Converter property of a DataGridItemProperty already added to a containing collection." );

        m_converter = value;
      }
    }

    internal IValueConverter GetBindingConverter( object sourceItem )
    {
      if( !this.IsSealed )
        throw new InvalidOperationException( "An attempt was made to apply a binding to a DataGridItemProperty that has not be added to the ItemProperties collection." );

      if( m_bindingConverter == null )
      {
        if( m_converter != null )
        {
          m_bindingConverter = m_converter;
        }
        else
        {
          m_bindingConverter = new SourceDataConverter( ItemsSourceHelper.IsItemSupportingDBNull( sourceItem ), CultureInfo.InvariantCulture );
        }
      }

      return m_bindingConverter;
    }

    private IValueConverter m_converter;
    private IValueConverter m_bindingConverter;

    #endregion

    #region ConverterCulture Property

    public CultureInfo ConverterCulture
    {
      get
      {
        return m_converterCulture;
      }
      set
      {
        if( this.IsSealed )
          throw new InvalidOperationException( "An attempt was made to change the ConverterCulture property of a DataGridItemProperty already added to a containing collection." );

        m_converterCulture = value;
      }
    }

    private CultureInfo m_converterCulture;

    #endregion

    #region ConverterParameter Property

    public object ConverterParameter
    {
      get
      {
        return m_converterParameter;
      }
      set
      {
        if( this.IsSealed )
          throw new InvalidOperationException( "An attempt was made to change the ConverterParameter property of a DataGridItemProperty already added to a containing collection." );

        m_converterParameter = value;
      }
    }

    private object m_converterParameter;

    #endregion

    #region CalculateDistinctValues Property

    public bool CalculateDistinctValues
    {
      get
      {
        // Always activate DistinctValues if not explicitly specified
        if( !this.IsCalculateDistinctValuesInitialized )
          return true;

        return m_flags[ DataGridItemPropertyBaseFlags.CalculateDistinctValues ];
      }
      set
      {
        if( value != m_flags[ DataGridItemPropertyBaseFlags.CalculateDistinctValues ] )
        {
          m_flags[ DataGridItemPropertyBaseFlags.CalculateDistinctValues ] = value;
          this.OnPropertyChanged( DataGridItemPropertyBase.CalculateDistinctValuesPropertyName );
        }

        this.IsCalculateDistinctValuesInitialized = true;
      }
    }

    internal bool IsCalculateDistinctValuesInitialized
    {
      get
      {
        return m_flags[ DataGridItemPropertyBaseFlags.IsCalculateDistinctValuesInitialized ];
      }
      set
      {
        m_flags[ DataGridItemPropertyBaseFlags.IsCalculateDistinctValuesInitialized ] = value;
      }
    }

    #endregion

    #region MaxDistinctValues Property

    public int MaxDistinctValues
    {
      get
      {
        return m_maxDistinctValues;
      }
      set
      {
        if( m_maxDistinctValues != value )
        {
          m_maxDistinctValues = value;
          this.OnPropertyChanged( DataGridItemPropertyBase.MaxDistinctValuesPropertyName );
        }
      }
    }

    private int m_maxDistinctValues = -1; // -1 ==> no maximum

    #endregion

    #region DistinctValuesSortComparer Property

    public IComparer DistinctValuesSortComparer
    {
      get;
      set;
    }

    #endregion

    #region DistinctValuesEqualityComparer Property

    public IEqualityComparer DistinctValuesEqualityComparer
    {
      get;
      set;
    }

    #endregion

    #region ForeignKeyDescription Property

    public DataGridForeignKeyDescription ForeignKeyDescription
    {
      get
      {
        return m_foreignKeyDescription;
      }
      set
      {
        this.SetForeignKeyDescription( value );
      }
    }

    internal void SetForeignKeyDescription( DataGridForeignKeyDescription description )
    {
      if( m_foreignKeyDescription != description )
      {
        m_foreignKeyDescription = description;
        this.OnPropertyChanged( DataGridItemPropertyBase.ForeignKeyDescriptionPropertyName );
      }
    }

    private DataGridForeignKeyDescription m_foreignKeyDescription; // = null;

    #endregion

    #region GroupSortStatResultPropertyName Property

    public string GroupSortStatResultPropertyName
    {
      get
      {
        return m_groupSortStatResultPropertyName;
      }
      set
      {
        if( value == m_groupSortStatResultPropertyName )
          return;

        m_groupSortStatResultPropertyName = value;

        this.OnPropertyChanged( DataGridItemPropertyBase.GroupSortStatResultPropertyNamePropertyName );
      }
    }

    private string m_groupSortStatResultPropertyName;

    #endregion

    #region GroupSortStatResultComparer Property

    public IComparer GroupSortStatResultComparer
    {
      get;
      set;
    }

    #endregion

    #region IsDisplayable Property

    public bool IsDisplayable
    {
      get
      {
        return m_flags[ DataGridItemPropertyBaseFlags.IsDisplayable ];
      }
      set
      {
        if( this.IsSealed )
          throw new InvalidOperationException( "An attempt was made to change the IsDisplayable property of a DataGridItemProperty already added to a containing collection." );

        this.SetIsDisplayable( value );
      }
    }

    internal void SetIsDisplayable( bool value )
    {
      m_flags[ DataGridItemPropertyBaseFlags.IsDisplayable ] = value;
    }

    #endregion

    #region ItemProperties Property

    public DataGridItemPropertyCollection ItemProperties
    {
      get
      {
        if( m_itemProperties == null )
        {
          Interlocked.CompareExchange( ref m_itemProperties, new DataGridItemPropertyCollection( this ), null );
          Debug.Assert( m_itemProperties != null );

          this.OnPropertyChanged( DataGridItemPropertyBase.ItemPropertiesInternalPropertyName );
        }

        return m_itemProperties;
      }
    }

    internal DataGridItemPropertyCollection ItemPropertiesInternal
    {
      get
      {
        return m_itemProperties;
      }
    }

    private DataGridItemPropertyCollection m_itemProperties;

    #endregion

    #region FieldName Internal Property

    internal virtual string FieldName
    {
      get
      {
        return this.Name;
      }
    }

    #endregion

    #region IsNameSealed Internal Property

    internal bool IsNameSealed
    {
      get
      {
        return m_flags[ DataGridItemPropertyBaseFlags.IsNameSealed ];
      }
      set
      {
        if( value == this.IsNameSealed )
          return;

        m_flags[ DataGridItemPropertyBaseFlags.IsNameSealed ] = value;

        this.OnPropertyChanged( DataGridItemPropertyBase.IsNameSealedPropertyName );
      }
    }

    #endregion

    #region IsSealed Internal Property

    internal bool IsSealed
    {
      get
      {
        return m_flags[ DataGridItemPropertyBaseFlags.IsSealed ];
      }
      set
      {
        if( value == this.IsSealed )
          return;

        m_flags[ DataGridItemPropertyBaseFlags.IsSealed ] = value;

        this.OnPropertyChanged( DataGridItemPropertyBase.IsSealedPropertyName );
      }
    }

    #endregion

    #region IsBrowsable Internal Property

    // That property only indicates for the DefaultProperties generated if the property should take place in the real property list by default.
    internal bool IsBrowsable
    {
      get
      {
        return m_flags[ DataGridItemPropertyBaseFlags.IsBrowsable ];
      }
      set
      {
        m_flags[ DataGridItemPropertyBaseFlags.IsBrowsable ] = value;
      }
    }

    #endregion

    #region IsASubRelationship Internal Property

    internal bool IsASubRelationship
    {
      get
      {
        if( m_flags[ DataGridItemPropertyBaseFlags.IsASubRelationshipSet ] )
          return m_flags[ DataGridItemPropertyBaseFlags.IsASubRelationship ];

        if( m_dataType == null )
          return false;

        bool isASubRelationship = ItemsSourceHelper.IsASubRelationship( m_dataType );

        if( this.IsSealed )
        {
          m_flags[ DataGridItemPropertyBaseFlags.IsASubRelationship ] = isASubRelationship;
          m_flags[ DataGridItemPropertyBaseFlags.IsASubRelationshipSet ] = true;
        }

        return isASubRelationship;
      }
    }

    private void SetIsASubRelationship( Nullable<bool> isASubRelationship )
    {
      if( isASubRelationship.HasValue )
      {
        m_flags[ DataGridItemPropertyBaseFlags.IsASubRelationship ] = isASubRelationship.Value;
        m_flags[ DataGridItemPropertyBaseFlags.IsASubRelationshipSet ] = true;
      }
      else
      {
        m_flags[ DataGridItemPropertyBaseFlags.IsASubRelationshipSet
               | DataGridItemPropertyBaseFlags.IsASubRelationship ] = false;
      }
    }

    #endregion

    #region IsSortingOnForeignKeyDescription Internal Property

    internal bool IsSortingOnForeignKeyDescription
    {
      get;
      set;
    }

    #endregion

    #region ContainingCollection Internal Property

    internal DataGridItemPropertyCollection ContainingCollection
    {
      get
      {
        return m_containingCollection;
      }
      private set
      {
        if( value == m_containingCollection )
          return;

        if( ( value != null ) && ( m_containingCollection != null ) )
          throw new InvalidOperationException( "The property is already assigned to a DataGridItemPropertyCollection." );

        m_containingCollection = value;

        this.OnPropertyChanged( DataGridItemPropertyBase.ContainingCollectionPropertyName );
      }
    }

    private DataGridItemPropertyCollection m_containingCollection; //null

    #endregion

    #region ValueChanged Internal Event

    internal event EventHandler<ValueChangedEventArgs> ValueChanged;

    private void OnValueChanged( ValueChangedEventArgs e )
    {
      var handler = this.ValueChanged;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    #region DistinctValueSelector Event

    public event EventHandler<QueryDistinctValueEventArgs> QueryDistinctValue
    {
      add
      {
        m_queryDistinctValue = ( EventHandler<QueryDistinctValueEventArgs> )Delegate.Combine( m_queryDistinctValue, value );
      }
      remove
      {
        m_queryDistinctValue = ( EventHandler<QueryDistinctValueEventArgs> )Delegate.Remove( m_queryDistinctValue, value );
      }
    }

    private EventHandler<QueryDistinctValueEventArgs> m_queryDistinctValue;

    internal object GetDistinctValueFromItem( object dataSourceValue )
    {
      if( m_queryDistinctValue == null )
        return dataSourceValue;

      QueryDistinctValueEventArgs args = new QueryDistinctValueEventArgs( dataSourceValue );

      m_queryDistinctValue( this, args );

      return args.DistinctValue;
    }

    #endregion

    public object GetValue( object component )
    {
      var unboundDataItem = component as UnboundDataItem;
      if( unboundDataItem != null )
      {
        component = unboundDataItem.DataItem;
      }

      // Since EmptyDataItemSafePropertyDescriptor ensures to return null to avoid Binding exceptions when a CollectionView other
      // than the DataGridCollectionView is used, we must return null to avoid calling GetValueCore using null as component.
      if( ( component == null ) || ( component is EmptyDataItem ) )
        return null;

      return this.GetValueCore( component );
    }

    public void SetValue( object component, object value )
    {
      var unboundDataItem = component as UnboundDataItem;
      if( unboundDataItem != null )
      {
        component = unboundDataItem.DataItem;
      }

      if( component == null )
        throw new InvalidOperationException( "An attempt was made to set a value on a null data item." );

      if( component is EmptyDataItem )
        throw new InvalidOperationException( "An attempt was made to set a value on an empty data item." );

      if( this.IsReadOnly && !this.OverrideReadOnlyForInsertion.GetValueOrDefault( false ) )
        throw new InvalidOperationException( "An attempt was made to set a read-only property." );

      this.SetValueCore( component, value );
    }

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The ICloneable interface is no longer supported.", true )]
    public virtual object Clone()
    {
      throw new NotSupportedException();
    }

    protected abstract object GetValueCore( object component );

    protected virtual void SetValueCore( object component, object value )
    {
      this.OnValueChanged( new ValueChangedEventArgs( component ) );
    }

    internal PropertyDescriptorFromItemPropertyBase GetPropertyDescriptorForBinding()
    {
      if( m_propertyDescriptorFromItemProperty == null )
      {
        m_propertyDescriptorFromItemProperty = this.GetPropertyDescriptorForBindingCore();
      }

      return m_propertyDescriptorFromItemProperty;
    }

    internal virtual PropertyDescriptorFromItemPropertyBase GetPropertyDescriptorForBindingCore()
    {
      return new PropertyDescriptorFromItemPropertyBase( this );
    }

    internal virtual void SetUnspecifiedPropertiesValues( PropertyDescription description, Type itemType, bool defaultItemPropertiesCreated )
    {
    }

    internal void AttachToContainingCollection( DataGridItemPropertyCollection collection )
    {
      if( collection == null )
        throw new ArgumentNullException( "collection" );

      this.ContainingCollection = collection;
    }

    internal void DetachFromContainingCollection()
    {
      this.ContainingCollection = null;
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged( string propertyName )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    #endregion

    private PropertyDescriptorFromItemPropertyBase m_propertyDescriptorFromItemProperty;
    private BitFlags m_flags;

    #region ValueChangedEventArgs Internal Class

    internal class ValueChangedEventArgs : EventArgs
    {
      public ValueChangedEventArgs( object component )
      {
        this.Component = component;
      }

      public object Component
      {
        get;
        private set;
      }
    }

    #endregion

    #region BitFlags Private Struct

    private struct BitFlags
    {
      internal bool this[ DataGridItemPropertyBaseFlags flag ]
      {
        get
        {
          return ( ( m_data & flag ) == flag );
        }
        set
        {
          this.CheckIfIsDefined( flag );

          if( value )
          {
            m_data |= flag;
          }
          else
          {
            m_data &= ~flag;
          }
        }
      }

      [Conditional( "DEBUG" )]
      private void CheckIfIsDefined( DataGridItemPropertyBaseFlags value )
      {
        if( Enum.IsDefined( typeof( DataGridItemPropertyBaseFlags ), value ) )
          return;

        int flags = Convert.ToInt32( value );
        foreach( var flag in Enum.GetValues( typeof( DataGridItemPropertyBaseFlags ) ) )
        {
          int flagValue = Convert.ToInt32( flag );
          if( ( flags & flagValue ) == flagValue )
          {
            flags &= ~flagValue;

            if( flags == 0 )
              break;
          }
        }

        Debug.Assert( flags == 0 );
      }

      private DataGridItemPropertyBaseFlags m_data;
    }

    #endregion

    #region DataGridItemPropertyBaseFlags Private Enum

    [Flags]
    private enum DataGridItemPropertyBaseFlags : ushort
    {
      IsReadOnly = 0x0001,
      IsOverrideReadOnlyForInsertionSet = 0x0002,
      IsOverrideReadOnlyForInsertion = 0x0004,
      IsASubRelationshipSet = 0x0008,
      IsASubRelationship = 0x0010,
      CalculateDistinctValues = 0x0020,
      IsCalculateDistinctValuesInitialized = 0x0040,
      IsNameSealed = 0x0080,
      IsSealed = 0x0100,
      IsBrowsable = 0x0200,
      IsDisplayable = 0x0400,
    }

    #endregion
  }
}
