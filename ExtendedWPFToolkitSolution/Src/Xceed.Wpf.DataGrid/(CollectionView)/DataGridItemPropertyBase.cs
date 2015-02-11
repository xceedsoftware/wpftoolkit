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
using System.Windows.Data;
using Xceed.Wpf.DataGrid.Converters;
using Xceed.Wpf.DataGrid.FilterCriteria;

namespace Xceed.Wpf.DataGrid
{
  public abstract partial class DataGridItemPropertyBase : INotifyPropertyChanged, ICloneable
  {

    protected DataGridItemPropertyBase()
    {
      this.Browsable = true;
    }

    protected DataGridItemPropertyBase( DataGridItemPropertyBase template )
      : this()
    {
      m_name = template.m_name;
      m_dataType = template.m_dataType;
      m_title = template.m_title;
      m_synonym = template.m_synonym;
      m_flags[ DataGridItemPropertyBaseFlags.IsReadOnly ] = template.m_flags[ DataGridItemPropertyBaseFlags.IsReadOnly ];
      m_flags[ DataGridItemPropertyBaseFlags.IsOverrideReadOnlyForInsertionSet ] = template.m_flags[ DataGridItemPropertyBaseFlags.IsOverrideReadOnlyForInsertionSet ];
      m_flags[ DataGridItemPropertyBaseFlags.IsOverrideReadOnlyForInsertion ] = template.m_flags[ DataGridItemPropertyBaseFlags.IsOverrideReadOnlyForInsertion ];
      m_flags[ DataGridItemPropertyBaseFlags.IsASubRelationshipSet ] = template.m_flags[ DataGridItemPropertyBaseFlags.IsASubRelationshipSet ];
      m_flags[ DataGridItemPropertyBaseFlags.IsASubRelationship ] = template.m_flags[ DataGridItemPropertyBaseFlags.IsASubRelationship ];
      m_flags[ DataGridItemPropertyBaseFlags.IsBrowsable ] = template.m_flags[ DataGridItemPropertyBaseFlags.IsBrowsable ];
      m_flags[ DataGridItemPropertyBaseFlags.CalculateDistinctValues ] = template.m_flags[ DataGridItemPropertyBaseFlags.CalculateDistinctValues ];
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
        this.SetIsReadOnly( isReadOnly.Value );
      }

      this.SetOverrideReadOnlyForInsertion( overrideReadOnlyForInsertion );
      m_dataType = dataType;

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

        if( this.Initialized )
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
        if( this.Initialized )
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
        return m_flags[ DataGridItemPropertyBaseFlags.IsReadOnly ];
      }
      set
      {
        if( this.Initialized )
          throw new InvalidOperationException( "An attempt was made to change the IsReadOnly property of a DataGridItemProperty already added to a containing collection." );

        this.SetIsReadOnly( value );
      }
    }

    internal void SetIsReadOnly( bool isReadOnly )
    {
      m_flags[ DataGridItemPropertyBaseFlags.IsReadOnly ] = isReadOnly;
    }

    #endregion IsReadOnly Property

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
        if( this.Initialized )
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

    #endregion OverrideReadOnlyForInsertion Property

    #region IsASubRelationship Property

    internal bool IsASubRelationship
    {
      get
      {
        if( m_flags[ DataGridItemPropertyBaseFlags.IsASubRelationshipSet ] )
          return m_flags[ DataGridItemPropertyBaseFlags.IsASubRelationship ];

        if( m_dataType == null )
          return false;

        bool isASubRelationship = ItemsSourceHelper.IsASubRelationship( m_dataType );

        if( this.Initialized )
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
        if( this.Initialized )
          throw new InvalidOperationException( "An attempt was made to change the Synonym of a property already added to a containing collection." );

        this.SetSynonym( value );
      }
    }

    internal void SetSynonym( string value )
    {
      m_synonym = value;
    }

    private string m_synonym;

    #endregion Synonym Property

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
        if( this.Initialized )
          throw new InvalidOperationException( "An attempt was made to change the Converter property of a DataGridItemProperty already added to a containing collection." );

        m_converter = value;
      }
    }

    internal IValueConverter GetBindingConverter( object sourceItem )
    {
      if( !this.Initialized )
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
        if( this.Initialized )
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
        if( this.Initialized )
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
          this.OnPropertyChanged( "CalculateDistinctValues" );
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

    #region Initialized Property

    internal bool Initialized
    {
      get
      {
        return m_flags[ DataGridItemPropertyBaseFlags.IsInitialized ];
      }
      set
      {
        Debug.Assert( value );

        m_flags[ DataGridItemPropertyBaseFlags.IsInitialized ] = value;
      }
    }

    #endregion Initialized Property

    #region Browsable Property

    // That property only indicate for the DefaultProperties generated if
    // the property should take place in the real property list by default.
    internal bool Browsable
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

    #endregion Browsable Property

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

          this.OnPropertyChanged( "GroupSortStatResultPropertyName" );
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

    #region ValueChanged Event

    private void OnValueChanged( ValueChangedEventArgs e )
    {
      if( this.ValueChanged != null )
      {
        this.ValueChanged( this, e );
      }
    }

    internal event EventHandler<ValueChangedEventArgs> ValueChanged;

    #endregion ComponentValueChanged Event

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
      {
        component = unboundDataItem.DataItem;
      }

      bool isReadOnly = ( this.OverrideReadOnlyForInsertion.HasValue && this.OverrideReadOnlyForInsertion.Value ) ? false : this.IsReadOnly;

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

    protected abstract object GetValueCore( object component );

    protected virtual void SetValueCore( object component, object value )
    {
      this.OnValueChanged( new ValueChangedEventArgs( component ) );
    }

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

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged( string propertyName )
    {
      if( this.PropertyChanged != null )
        this.PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
    }

    #endregion

    private PropertyDescriptorFromItemPropertyBase m_propertyDescriptorFromItemProperty;
    private BitFlags m_flags;

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
      IsInitialized = 0x0080,
      IsBrowsable = 0x0100,
    }
  }
}
