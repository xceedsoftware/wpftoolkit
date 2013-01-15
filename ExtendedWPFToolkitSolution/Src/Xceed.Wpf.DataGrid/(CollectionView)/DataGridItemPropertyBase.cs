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
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using Xceed.Wpf.DataGrid.Converters;
using System.Windows.Data;
using Xceed.Wpf.DataGrid.FilterCriteria;

namespace Xceed.Wpf.DataGrid
{
  public abstract partial class DataGridItemPropertyBase : INotifyPropertyChanged, ICloneable
  {

    #region CONSTRUCTORS

    protected DataGridItemPropertyBase()
    {
    }

    protected DataGridItemPropertyBase( DataGridItemPropertyBase template )
    {
      m_name = template.m_name;
      m_dataType = template.m_dataType;
      m_title = template.m_title;
      m_readOnly = template.m_readOnly;
      m_overrideReadOnlyForInsertion = template.m_overrideReadOnlyForInsertion;
      m_isASubRelationship = template.m_isASubRelationship;
      m_browsable = template.m_browsable;
      m_calculateDistinctValues = template.m_calculateDistinctValues;
      m_converter = template.m_converter;
      m_converterCulture = template.m_converterCulture;
      m_converterParameter = template.m_converterParameter;
      this.FilterCriterion = template.m_filterCriterion;
      m_foreignKeyDescription = template.m_foreignKeyDescription;
      m_maxDistinctValues = template.m_maxDistinctValues;
      m_sortComparer = template.m_sortComparer;
      this.DistinctValuesEqualityComparer = template.DistinctValuesEqualityComparer;
      this.DistinctValuesSortComparer = template.DistinctValuesSortComparer;

      // FilterCriterionChanged is not cloned since only used after the clone occurs
      this.PropertyChanged += template.PropertyChanged;
      this.QueryDistinctValue += template.m_queryDistinctValue;
    }

    protected void Initialize(
      string name,
      string title,
      Type dataType,
      Nullable<bool> isReadOnly,
      Nullable<bool> overrideReadOnlyForInsertion,
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
        m_readOnly = isReadOnly.Value;
      }

      m_overrideReadOnlyForInsertion = overrideReadOnlyForInsertion;
      m_dataType = dataType;

      if( isASubRelationship != null )
      {
        m_isASubRelationship = isASubRelationship;
      }
    }

    #endregion CONSTRUCTORS

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

        if( m_initialized )
          throw new InvalidOperationException( "An attempt was made to change the name of a property already added to a containing collection." );

        m_name = value;
      }
    }

    private string m_name;

    #endregion Name Property

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
        if( m_initialized )
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

    #endregion DataType Property

    #region IsReadOnly Property

    public bool IsReadOnly
    {
      get
      {
        return m_readOnly;
      }
      set
      {
        if( m_initialized )
          throw new InvalidOperationException( "An attempt was made to change the IsReadOnly property of a DataGridItemProperty already added to a containing collection." );

        this.SetIsReadOnly( value );
      }
    }

    internal void SetIsReadOnly( bool isReadOnly )
    {
      m_readOnly = isReadOnly;
    }

    private bool m_readOnly;

    #endregion IsReadOnly Property

    #region OverrideReadOnlyForInsertion Property

    public Nullable<bool> OverrideReadOnlyForInsertion
    {
      get
      {
        return m_overrideReadOnlyForInsertion;
      }
      set
      {
        if( m_initialized )
          throw new InvalidOperationException( "An attempt was made to change the OverrideReadOnlyForInsertion property of a DataGridItemProperty already added to a containing collection." );

        this.SetOverrideReadOnlyForInsertion( value );
      }
    }

    internal void SetOverrideReadOnlyForInsertion( Nullable<bool> overrideReadOnlyForInsertion )
    {
      m_overrideReadOnlyForInsertion = overrideReadOnlyForInsertion;
    }

    private Nullable<bool> m_overrideReadOnlyForInsertion;

    #endregion OverrideReadOnlyForInsertion Property

    #region IsASubRelationship Property

    internal bool IsASubRelationship
    {
      get
      {
        if( m_isASubRelationship == null )
        {
          if( m_dataType == null )
            return false;

          bool isASubRelationship = ItemsSourceHelper.IsASubRelationship( m_dataType );

          if( m_initialized )
            m_isASubRelationship = isASubRelationship;

          return isASubRelationship;
        }

        return m_isASubRelationship.Value;
      }
    }

    private Nullable<bool> m_isASubRelationship;

    #endregion IsASubRelationship Property

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

    #endregion Title Property

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

    #endregion SortComparer Property

    #region Converter Property

    public IValueConverter Converter
    {
      get
      {
        return m_converter;
      }
      set
      {
        if( m_initialized )
          throw new InvalidOperationException( "An attempt was made to change the Converter property of a DataGridItemProperty already added to a containing collection." );

        m_converter = value;
      }
    }

    internal IValueConverter GetBindingConverter( object sourceItem )
    {
      if( !m_initialized )
        throw new InvalidOperationException( "An attempt was made to apply a binding to a DataGridItemProperty that has not be added to the ItemProperties collection." );

      if( m_bindingConverter == null )
      {
        if( m_converter != null )
        {
          m_bindingConverter = m_converter;
        }
        else
        {
          m_bindingConverter = new SourceDataConverter(
            ItemsSourceHelper.IsItemSupportingDBNull( sourceItem ),
            CultureInfo.InvariantCulture );
        }
      }

      return m_bindingConverter;
    }

    private IValueConverter m_converter;
    private IValueConverter m_bindingConverter;

    #endregion Converter Property

    #region ConverterCulture Property

    public CultureInfo ConverterCulture
    {
      get
      {
        return m_converterCulture;
      }
      set
      {
        if( m_initialized )
          throw new InvalidOperationException( "An attempt was made to change the ConverterCulture property of a DataGridItemProperty already added to a containing collection." );

        m_converterCulture = value;
      }
    }

    private CultureInfo m_converterCulture;

    #endregion ConverterCulture Property

    #region ConverterParameter Property

    public object ConverterParameter
    {
      get
      {
        return m_converterParameter;
      }
      set
      {
        if( m_initialized )
          throw new InvalidOperationException( "An attempt was made to change the ConverterParameter property of a DataGridItemProperty already added to a containing collection." );

        m_converterParameter = value;
      }
    }

    private object m_converterParameter;

    #endregion ConverterParameter Property

    #region FilterCriterion Property

    public FilterCriterion FilterCriterion
    {
      get
      {
        return m_filterCriterion;
      }

      set
      {
        if( value != m_filterCriterion )
        {
          if( m_filterCriterion != null )
            m_filterCriterion.PropertyChanged -= new PropertyChangedEventHandler( FilterCriterion_PropertyChanged );

          m_filterCriterion = value;

          this.RaiseFilterCriterionChanged();

          if( m_filterCriterion != null )
            m_filterCriterion.PropertyChanged += new PropertyChangedEventHandler( FilterCriterion_PropertyChanged );

          this.OnPropertyChanged( "FilterCriterion" );
        }
      }
    }

    private void FilterCriterion_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      this.RaiseFilterCriterionChanged();
    }

    private void RaiseFilterCriterionChanged()
    {
      if( this.FilterCriterionChanged != null )
        this.FilterCriterionChanged( this, EventArgs.Empty );
    }


    // Triggered if the instance of FilterCriterion changes, if a property of the 
    // FilterCriterion changes or if a property of one of the child FilterCriterion
    // changes.
    internal event EventHandler FilterCriterionChanged;

    private FilterCriterion m_filterCriterion; // = null;

    #endregion FilterCriterion Property

    #region ValueChanged Event

    private void OnValueChanged( ValueChangedEventArgs e )
    {
      if( this.ValueChanged != null )
        this.ValueChanged( this, e );
    }

    internal event EventHandler<ValueChangedEventArgs> ValueChanged;

    #endregion ComponentValueChanged Event

    #region CalculateDistinctValues Property

    public bool CalculateDistinctValues
    {
      get
      {
        // Always activate DistinctValues if not explicitly specified
        if( !m_isCalculateDistinctValuesInitialized )
          return true;

        return m_calculateDistinctValues;
      }
      set
      {
        if( m_calculateDistinctValues != value )
        {
          m_calculateDistinctValues = value;
          this.OnPropertyChanged( "CalculateDistinctValues" );
        }

        m_isCalculateDistinctValuesInitialized = true;
      }
    }

    internal bool IsCalculateDistinctValuesInitialized
    {
      get
      {
        return m_isCalculateDistinctValuesInitialized;
      }
      set
      {
        m_isCalculateDistinctValuesInitialized = value;
      }
    }

    private bool m_calculateDistinctValues; // = null; 
    private bool m_isCalculateDistinctValuesInitialized;

    #endregion CalculateDistinctValues Property

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
          this.OnPropertyChanged( "MaxDistinctValues" );
        }
      }
    }

    private int m_maxDistinctValues = -1; // -1 ==> no maximum

    #endregion MaxDistinctValues Property

    #region DistinctValuesSortComparer

    public IComparer DistinctValuesSortComparer
    {
      get;
      set;
    }

    #endregion

    #region DistinctValuesEqualityComparer

    public IEqualityComparer DistinctValuesEqualityComparer
    {
      get;
      set;
    }

    #endregion

    #region Initialized Property

    internal bool Initialized
    {
      get
      {
        return m_initialized;
      }
      set
      {
        Debug.Assert( value );

        m_initialized = value;
      }
    }

    private bool m_initialized;

    #endregion Initialized Property

    #region Browsable Property

    // That property only indicate for the DefaultProperties generated if
    // the property should take place in the real property list by default.
    internal bool Browsable
    {
      get
      {
        return m_browsable;
      }
      set
      {
        m_browsable = value;
      }
    }

    private bool m_browsable = true;

    #endregion Browsable Property

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
        this.OnPropertyChanged( "ForeignKeyDescription" );
      }
    }

    private DataGridForeignKeyDescription m_foreignKeyDescription; // = null;

    #endregion ForeignKeyDescription Property

    #region GroupSortStatResultPropertyName Property

    public string GroupSortStatResultPropertyName
    {
      get
      {
        return m_groupSortStatResultPropertyName;
      }
      set
      {
        if( m_groupSortStatResultPropertyName != value )
        {
          m_groupSortStatResultPropertyName = value;
        }
      }
    }

    private string m_groupSortStatResultPropertyName;

    #endregion GroupSortStatResultPropertyName Property

    #region GroupSortStatResultComparer Property

    public IComparer GroupSortStatResultComparer
    {
      get;
      set;
    }

    #endregion GroupSortStatResultComparer Property

    #region PUBLIC METHODS

    public object GetValue( object component )
    {
      // Since EmptyDataItemSafePropertyDescriptor ensure
      // to return null to avoid Binding exceptions when a 
      // CollectionView other than the DataGridCollectionView
      // is used, we must return null to avoid calling 
      // GetValueCore using null as component
      if( ( component == null )
          || ( component is EmptyDataItem ) )
        return null;

      UnboundDataItem unboundDataItem = component as UnboundDataItem;

      if( unboundDataItem != null )
        component = unboundDataItem.DataItem;

      return this.GetValueCore( component );
    }

    public void SetValue( object component, object value )
    {
      if( component is EmptyDataItem )
        throw new InvalidOperationException( "An attempt was made to set a value on an empty data item." );

      UnboundDataItem unboundDataItem = component as UnboundDataItem;

      if( unboundDataItem != null )
        component = unboundDataItem.DataItem;

      bool isReadOnly = ( this.OverrideReadOnlyForInsertion.HasValue && this.OverrideReadOnlyForInsertion.Value )
        ? false
        : this.IsReadOnly;

      if( isReadOnly )
        throw new InvalidOperationException( "An attempt was made to set a read-only property." );

      this.SetValueCore( component, value );
    }

    public virtual object Clone()
    {
      try
      {
        return Activator.CreateInstance( this.GetType(), this );
      }
      catch( Exception exception )
      {
        throw new NotImplementedException( "An attempt was made to Clone an instance of type " + this.GetType().ToString() + " that does not override the Clone() method.", exception );
      }
    }

#if DEBUG
    public override string ToString()
    {
      if( this.Name != null )
      {
        return this.GetType() + ": " + this.Name;
      }
      else
      {
        return this.GetType().ToString();
      }
    }
#endif

    #endregion PUBLIC METHODS

    #region PROTECTED METHODS

    protected abstract object GetValueCore( object component );

    protected virtual void SetValueCore( object component, object value )
    {
      this.OnValueChanged( new ValueChangedEventArgs( component ) );
    }

    #endregion PROTECTED METHODS

    #region INTERNAL METHODS

    internal PropertyDescriptorFromItemPropertyBase GetPropertyDescriptorForBinding()
    {
      if( m_propertyDescriptorFromItemProperty == null )
        m_propertyDescriptorFromItemProperty = this.GetPropertyDescriptorForBindingCore();

      return m_propertyDescriptorFromItemProperty;
    }

    internal virtual PropertyDescriptorFromItemPropertyBase GetPropertyDescriptorForBindingCore()
    {
      return new PropertyDescriptorFromItemPropertyBase( this );
    }

    internal virtual void SetUnspecifiedPropertiesValues( DataGridItemPropertyCollection itemPropertyCollection )
    {
    }

    #endregion INTERNAL METHODS

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged( string propertyName )
    {
      if( this.PropertyChanged != null )
        this.PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
    }

    #endregion

    #region PRIVATE FIELDS

    private PropertyDescriptorFromItemPropertyBase m_propertyDescriptorFromItemProperty;

    #endregion PRIVATE FIELDS

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
  }
}
