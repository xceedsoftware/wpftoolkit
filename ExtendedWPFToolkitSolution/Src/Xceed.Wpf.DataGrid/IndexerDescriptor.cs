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
using System.Linq;
using System.Reflection;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class IndexerDescriptor : PropertyDescriptor
  {
    #region Static Fields

    private static readonly object NotInitialized = new object();
    private static readonly object NotSet = new object();

    #endregion

    // The name must be set to Item[] for binding to get notified when the data item implements INotifyPropertyChanged.
    private IndexerDescriptor( string indexerName, string parametersList, object[] parameterValues, PropertyInfo propertyInfo, Attribute[] attributes )
        : base( string.Format( "{0}[]", indexerName ), attributes )
    {
      m_displayName = string.Format( "{0}[{1}]", indexerName, parametersList );
      m_indexerName = indexerName;
      m_indexerParameters = parametersList;
      m_indexerParameterValues = parameterValues;
      m_propertyInfo = propertyInfo;
    }

    #region ComponentType Property

    public override Type ComponentType
    {
      get
      {
        return m_propertyInfo.DeclaringType;
      }
    }

    #endregion

    #region PropertyType Property

    public override Type PropertyType
    {
      get
      {
        return m_propertyInfo.PropertyType;
      }
    }

    #endregion

    #region IsReadOnly Property

    public sealed override bool IsReadOnly
    {
      get
      {
        if( !m_propertyInfo.CanWrite )
          return true;

        var attribute = ( ReadOnlyAttribute )this.Attributes[ typeof( ReadOnlyAttribute ) ];

        return ( attribute != null )
            && ( attribute.IsReadOnly );
      }
    }

    #endregion

    #region DisplayName Property

    public override string DisplayName
    {
      get
      {
        return m_displayName;
      }
    }

    private readonly string m_displayName;

    #endregion

    #region IndexerName Internal Property

    internal string IndexerName
    {
      get
      {
        return m_indexerName;
      }
    }

    private readonly string m_indexerName;

    #endregion

    #region IndexerParameters Internal Property

    internal string IndexerParameters
    {
      get
      {
        return m_indexerParameters;
      }
    }

    private readonly string m_indexerParameters;

    #endregion

    #region DefaultValue Private Property

    private object DefaultValue
    {
      get
      {
        if( m_defaultValue == IndexerDescriptor.NotInitialized )
        {
          var attribute = ( DefaultValueAttribute )this.Attributes[ typeof( DefaultValueAttribute ) ];
          if( attribute != null )
          {
            var propertyType = this.PropertyType;
            var value = attribute.Value;

            if( ( value != null ) && ( propertyType.IsEnum ) && ( Enum.GetUnderlyingType( propertyType ) == value.GetType() ) )
            {
              value = Enum.ToObject( propertyType, value );
            }

            m_defaultValue = value;
          }
          else
          {
            m_defaultValue = IndexerDescriptor.NotSet;
          }
        }

        return m_defaultValue;
      }
    }

    private object m_defaultValue = IndexerDescriptor.NotInitialized;

    #endregion

    #region AmbientValue Private Property

    private object AmbientValue
    {
      get
      {
        if( m_ambientValue == IndexerDescriptor.NotInitialized )
        {
          var attribute = ( AmbientValueAttribute )this.Attributes[ typeof( AmbientValueAttribute ) ];
          if( attribute != null )
          {
            m_ambientValue = attribute.Value;
          }
          else
          {
            m_ambientValue = IndexerDescriptor.NotSet;
          }
        }

        return m_ambientValue;
      }
    }

    private object m_ambientValue = IndexerDescriptor.NotInitialized;

    #endregion

    public override bool ShouldSerializeValue( object component )
    {
      if( this.IsReadOnly )
        return this.Attributes.Contains( DesignerSerializationVisibilityAttribute.Content );

      if( this.DefaultValue != IndexerDescriptor.NotSet )
        return !object.Equals( this.GetValue( component ), this.DefaultValue );

      return true;
    }

    public override object GetValue( object component )
    {
      if( component == null )
        return null;

      return m_propertyInfo.GetValue( component, m_indexerParameterValues );
    }

    public override void SetValue( object component, object value )
    {
      if( ( component == null ) || this.IsReadOnly )
        return;

      m_propertyInfo.SetValue( component, value, m_indexerParameterValues );

      this.OnValueChanged( component, EventArgs.Empty );
    }

    public override bool CanResetValue( object component )
    {
      if( this.IsReadOnly )
        return false;

      if( this.DefaultValue != IndexerDescriptor.NotSet )
        return !object.Equals( this.GetValue( component ), this.DefaultValue );

      return ( this.AmbientValue != IndexerDescriptor.NotSet )
          && ( this.ShouldSerializeValue( component ) );
    }

    public override void ResetValue( object component )
    {
      if( ( component == null ) || this.IsReadOnly )
        return;

      if( this.DefaultValue != IndexerDescriptor.NotSet )
      {
        this.SetValue( component, this.DefaultValue );
      }
      else if( this.AmbientValue != IndexerDescriptor.NotSet )
      {
        this.SetValue( component, this.AmbientValue );
      }
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override bool Equals( object obj )
    {
      var descriptor = obj as IndexerDescriptor;
      if( object.ReferenceEquals( descriptor, null ) )
        return false;

      return ( base.Equals( descriptor ) )
          && ( descriptor.ComponentType == this.ComponentType )
          && ( descriptor.IsReadOnly == this.IsReadOnly )
          && ( descriptor.SupportsChangeEvents == this.SupportsChangeEvents )
          && ( descriptor.IndexerName == this.IndexerName )
          && ( descriptor.IndexerParameters == this.IndexerParameters );
    }

    internal static IndexerDescriptor Create( PropertyInfo propertyInfo, object[] parameterNames, object[] parameterValues )
    {
      if( propertyInfo == null )
        throw new ArgumentNullException( "propertyInfo" );

      if( parameterNames == null )
        throw new ArgumentNullException( "parameterNames" );

      if( parameterNames.Length <= 0 )
        throw new ArgumentException( "The indexer must contain at least one index.", "parameterNames" );

      if( parameterValues == null )
        throw new ArgumentNullException( "parameterValues" );

      if( parameterValues.Length <= 0 )
        throw new ArgumentException( "The indexer must contain at least one index.", "parameterValues" );

      var parameters = propertyInfo.GetIndexParameters();
      if( ( parameters == null ) || ( parameters.Length <= 0 ) )
        throw new ArgumentException( "The target property is not a valid indexer.", "propertyInfo" );

      var attributes = ( from entry in propertyInfo.GetCustomAttributes( true )
                         let attr = ( entry as Attribute )
                         where ( attr != null )
                         select attr ).ToArray();
      var parametersList = IndexerParametersParser.Parse( parameterNames );
      var indexerName = propertyInfo.Name;

      return new IndexerDescriptor( indexerName, parametersList, parameterValues, propertyInfo, attributes );
    }

    private readonly PropertyInfo m_propertyInfo;
    private readonly object[] m_indexerParameterValues;
  }
}
