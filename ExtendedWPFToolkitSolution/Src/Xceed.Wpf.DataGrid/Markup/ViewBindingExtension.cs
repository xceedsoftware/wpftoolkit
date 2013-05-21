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
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows;
using System.Globalization;
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid.Markup
{
  [MarkupExtensionReturnType( typeof( Binding ) )]
  public class ViewBindingExtension : MarkupExtension
  {
    public ViewBindingExtension() : base()
    {
    }

    public ViewBindingExtension( string path ) : base()
    {
      if( path == null )
        throw new ArgumentNullException( "path" );

      m_path = path;
    }

    #region Path Property

    private string m_path; // = null;

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    public string Path
    {
      get
      {
        return m_path;
      }
      set
      {
        if( value == null )
          throw new ArgumentNullException( "Path" );

        if( m_path != null )
          throw new InvalidOperationException( "An attempt was made to set the Path property when it has already been initialized.s" );

        m_path = value;
      }
    }

    #endregion Path Property

    #region Mode Property

    private BindingMode m_bindingMode = BindingMode.OneWay;

    public BindingMode Mode
    {
      get
      {
        return m_bindingMode;
      }
      set
      {
        m_bindingMode = value;
      }
    }

    #endregion Mode Property

    #region Converter Property

    private IValueConverter m_converter; // = null;

    public IValueConverter Converter
    {
      get
      {
        return m_converter;
      }
      set
      {
        if( this.ConverterInitialized )
          throw new InvalidOperationException( "An attempt was made to set the Converter property when it has already been initialized." );

        m_converter = value;
        this.ConverterInitialized = true;
      }
    }

    #endregion Converter Property

    #region ConverterParameter Property

    private object m_converterParameter; // = null;

    public object ConverterParameter
    {
      get
      {
        return m_converterParameter;
      }
      set
      {
        if( this.ConverterParameterInitialized )
          throw new InvalidOperationException( "An attempt was made to set the ConverterParameter property when it has already been initialized." );

        m_converterParameter = value;
        this.ConverterParameterInitialized = true;
      }
    }

    #endregion ConverterParameter Property

    #region ConverterCulture Property

    private CultureInfo m_converterCulture; // = null;

    public CultureInfo ConverterCulture
    {
      get
      {
        return m_converterCulture;
      }
      set
      {
        if( this.ConverterCultureInitialized )
          throw new InvalidOperationException( "An attempt was made to set the ConverterCulture property when it has already been initialized." );

        m_converterCulture = value;
        this.ConverterCultureInitialized = true;
      }
    }

    #endregion ConverterCulture Property

    #region PUBLIC METHODS

    public override object ProvideValue( IServiceProvider serviceProvider )
    {
      return this.CreateBinding();
    }

    #endregion PUBLIC METHODS

    #region PRIVATE METHODS

    private Binding CreateBinding()
    {
      if( m_path == null )
        throw new InvalidOperationException( "An attempt was made to create a binding without a Path." );

      Binding binding;

      binding = new Binding();
      binding.Path = new PropertyPath( "(0)." + m_path, DataGridControl.DataGridContextProperty );
      binding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

      binding.Mode = m_bindingMode;
      binding.Converter = m_converter;
      binding.ConverterParameter = m_converterParameter;
      binding.ConverterCulture = m_converterCulture;

      return binding;
    }

    #endregion PRIVATE METHODS

    #region PRIVATE PROPERTIES

    private bool ConverterCultureInitialized
    {
      get
      {
        return m_flags[ ( int )ViewBindingExtensionFlags.ConverterCultureInitialized ];
      }
      set
      {
        m_flags[ ( int )ViewBindingExtensionFlags.ConverterCultureInitialized ] = value;
      }
    }


    private bool ConverterInitialized
    {
      get
      {
        return m_flags[ ( int )ViewBindingExtensionFlags.ConverterInitialized ];
      }
      set
      {
        m_flags[ ( int )ViewBindingExtensionFlags.ConverterInitialized ] = value;
      }
    }

    private bool ConverterParameterInitialized
    {
      get
      {
        return m_flags[ ( int )ViewBindingExtensionFlags.ConverterParameterInitialized ];
      }
      set
      {
        m_flags[ ( int )ViewBindingExtensionFlags.ConverterParameterInitialized ] = value;
      }
    }

    #endregion PRIVATE PROPERTIES

    #region PRIVATE FIELDS

    private BitVector32 m_flags = new BitVector32();

    #endregion PRIVATE FIELDS

    #region NESTED CLASSES

    [Flags]
    private enum ViewBindingExtensionFlags
    {
      ConverterCultureInitialized = 1,
      ConverterInitialized = 2,
      ConverterParameterInitialized = 4
    }

    #endregion NESTED CLASSES
  }
}
