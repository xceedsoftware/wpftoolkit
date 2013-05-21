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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Diagnostics;
using System.Globalization;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class BindingPathValueExtractor : FrameworkElement
  {
    #region CONSTRUCTORS

    public BindingPathValueExtractor(
      string xPath,
      PropertyPath propertyPath,
      bool onlyForWrite,
      Type dataType,
      IValueConverter converter,
      object converterParameter,
      CultureInfo converterCulture )
    {
      m_onlyForWrite = onlyForWrite;
      m_targetType = dataType;
      m_contextDataProvider = new ContextDataProvider();
      m_binding = new Binding();
      m_binding.XPath = xPath;
      m_binding.Path = propertyPath;

      if( converter == null )
        throw new DataGridInternalException( "A Converter must be used" );

      if( onlyForWrite )
      {
        m_binding.Mode = BindingMode.OneWayToSource;
        m_binding.Converter = new ConvertBackInhibitorPassthroughConverter( converter );
        m_binding.ConverterParameter = converterParameter;
        m_binding.ConverterCulture = converterCulture;
      }
      else
      {
        m_binding.Mode = BindingMode.OneWay;
        m_converter = converter;
        m_converterParameter = converterParameter;
        m_converterCulture = converterCulture;
      }

      m_binding.Source = m_contextDataProvider;

      this.SetBinding(
        BindingPathValueExtractor.ValueProperty,
        m_binding );
    }

    #endregion CONSTRUCTORS

    #region ValueProperty

    private static object UninitializedValueKey = new object();

    private static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
      "Value", typeof( object ), typeof( BindingPathValueExtractor ), new PropertyMetadata( BindingPathValueExtractor.UninitializedValueKey ) );

    private object Value
    {
      get
      {
        if( m_onlyForWrite )
          throw new InvalidOperationException( "An attempt was made to read in a write-only binding." );

        object value = this.GetValue( BindingPathValueExtractor.ValueProperty );

        if( value == BindingPathValueExtractor.UninitializedValueKey )
          value = null;

        value = m_converter.Convert( value, m_targetType, m_converterParameter, m_converterCulture );

        if( ( value == DependencyProperty.UnsetValue ) || ( value == Binding.DoNothing ) )
          return null;

        return value;
      }
      set
      {
        if( !m_onlyForWrite )
          throw new InvalidOperationException( "An attempt was made to write in a read-only binding." );

        this.SetValue( BindingPathValueExtractor.ValueProperty, value );
      }
    }

    #endregion ValueProperty

    #region PUBLIC METHODS

    public object GetValueFromItem( object item )
    {
      m_contextDataProvider.SetNewDataContext( item );
      return this.Value;

      // To gain some performance we do not reset the context
      //m_contextDataProvider.SetNewDataContext( null );
    }

    public void SetValueToItem( object item, object value )
    {
      if( m_binding.Mode == BindingMode.OneWay )
        throw new InvalidOperationException( "An attempt was made to set a value for a read-only destination path." );

      m_contextDataProvider.SetNewDataContext( item );
      this.Value = value;

      // To gain some performance we do not reset the context
      //m_contextDataProvider.SetNewDataContext( null );
    }

    #endregion PUBLIC METHODS

    #region PRIVATE FIELDS

    internal static readonly object[] EmptyObjectArray = new object[ 0 ];

    private ContextDataProvider m_contextDataProvider;
    private Binding m_binding;
    private IValueConverter m_converter;
    private object m_converterParameter;
    private CultureInfo m_converterCulture;
    private bool m_onlyForWrite;
    private Type m_targetType;

    #endregion PRIVATE FIELDS

    #region PRIVATE ConvertBackInhibitorConverter CLASS

    private class ConvertBackInhibitorPassthroughConverter : IValueConverter
    {
      public ConvertBackInhibitorPassthroughConverter( IValueConverter converter )
      {
        m_converter = converter;
      }

      #region IValueConverter Members

      public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
      {
        if( m_converter != null )
          return m_converter.Convert( value, targetType, parameter, culture );

        return value;
      }

      public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
      {
        if( value == BindingPathValueExtractor.UninitializedValueKey )
          return Binding.DoNothing;

        if( m_converter != null )
          return m_converter.ConvertBack( value, targetType, parameter, culture );

        return value;
      }

      #endregion

      #region PRIVATE FIELDS

      private IValueConverter m_converter;

      #endregion PRIVATE FIELDS
    }

    #endregion PRIVATE ConvertBackInhibitorConverter CLASS

    #region PRIVATE ContextDataProvider CLASS

    private class ContextDataProvider : DataSourceProvider
    {
      public ContextDataProvider()
      {
      }

      public void SetNewDataContext( object dataContext )
      {
        m_dataContext = dataContext;
        this.Refresh();
      }

      protected override void BeginQuery()
      {
        this.OnQueryFinished( m_dataContext );
      }

      private object m_dataContext;
    }

    #endregion PRIVATE ContextDataProvider CLASS
  }
}
