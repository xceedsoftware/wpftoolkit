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
using System.Windows.Markup;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid.Markup
{
  [MarkupExtensionReturnType( typeof( object ) )]
  [Obsolete( "The CellContentBinding markup extension is obsolete and has been replaced by the CellContentPresenter class.", false )]
  [Browsable( false )]
  [EditorBrowsable( EditorBrowsableState.Never )]
  public class CellContentBindingExtension : MarkupExtension
  {
    public CellContentBindingExtension()
    {
      m_cellBinding.RelativeSource = new RelativeSource( RelativeSourceMode.TemplatedParent );
      m_cellBinding.Mode = BindingMode.OneWay;
      m_cellBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

      m_cellBinding.Path = new PropertyPath( Cell.ContentProperty );
    }

    #region Converter Property

    public IValueConverter Converter
    {
      get
      {
        return m_cellBinding.Converter;
      }
      set
      {
        if( this.ConverterInitialized )
          throw new InvalidOperationException( "An attempt was made to set the Converter property when it has already been initialized." );

        m_cellBinding.Converter = value;
        this.ConverterInitialized = true;
      }
    }

    #endregion Converter Property

    #region ConverterParameter Property

    public object ConverterParameter
    {
      get
      {
        return m_cellBinding.ConverterParameter;
      }
      set
      {
        if( this.ConverterParameterInitialized )
          throw new InvalidOperationException( "An attempt was made to set the ConverterParameter property when it has already been initialized." );

        m_cellBinding.ConverterParameter = value;
        this.ConverterParameterInitialized = true;
      }
    }

    #endregion ConverterParameter Property

    #region ConverterCulture Property

    public CultureInfo ConverterCulture
    {
      get
      {
        return m_cellBinding.ConverterCulture;
      }
      set
      {
        if( this.ConverterCultureInitialized )
          throw new InvalidOperationException( "An attempt was made to set the ConverterCulture property when it has already been initialized." );

        m_cellBinding.ConverterCulture = value;
        this.ConverterCultureInitialized = true;
      }
    }

    #endregion ConverterCulture Property

    #region PUBLIC METHODS

    public sealed override object ProvideValue( IServiceProvider serviceProvider )
    {
      return m_cellBinding.ProvideValue( serviceProvider );
    }

    #endregion PUBLIC METHODS

    #region PRIVATE PROPERTIES

    private bool ConverterCultureInitialized
    {
      get
      {
        return m_flags[ ( int )CellContentBindingExtensionFlags.ConverterCultureInitialized ];
      }
      set
      {
        m_flags[ ( int )CellContentBindingExtensionFlags.ConverterCultureInitialized ] = value;
      }
    }


    private bool ConverterInitialized
    {
      get
      {
        return m_flags[ ( int )CellContentBindingExtensionFlags.ConverterInitialized ];
      }
      set
      {
        m_flags[ ( int )CellContentBindingExtensionFlags.ConverterInitialized ] = value;
      }
    }

    private bool ConverterParameterInitialized
    {
      get
      {
        return m_flags[ ( int )CellContentBindingExtensionFlags.ConverterParameterInitialized ];
      }
      set
      {
        m_flags[ ( int )CellContentBindingExtensionFlags.ConverterParameterInitialized ] = value;
      }
    }

    #endregion PRIVATE PROPERTIES

    #region PRIVATE FIELDS

    private Binding m_cellBinding = new Binding();
    private BitVector32 m_flags = new BitVector32();

    #endregion PRIVATE FIELDS

    #region NESTED CLASSES

    [Flags]
    private enum CellContentBindingExtensionFlags
    {
      ConverterCultureInitialized = 1,
      ConverterInitialized = 2,
      ConverterParameterInitialized = 4
    }

    #endregion NESTED CLASSES
  }
}
