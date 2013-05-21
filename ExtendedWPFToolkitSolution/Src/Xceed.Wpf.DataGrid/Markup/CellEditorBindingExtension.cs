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
using System.Globalization;
using System.Text;
using System.Windows.Markup;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;

using Xceed.Wpf.DataGrid.Converters;
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid.Markup
{
  [MarkupExtensionReturnType( typeof( object ) )]
  public class CellEditorBindingExtension : MarkupExtension
  {
    public CellEditorBindingExtension()
    {
      m_cellBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );
      m_cellBinding.Mode = BindingMode.TwoWay;
      m_cellBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      m_cellBinding.Converter = new DefaultDataConverter();

      m_cellBinding.Path = new PropertyPath( 
        "(0).(1)", 
        Cell.ParentCellProperty, Cell.ContentProperty );
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

    #region NotifyOnSourceUpdated Property

    public bool NotifyOnSourceUpdated
    {
      get
      {
        return m_cellBinding.NotifyOnSourceUpdated;
      }
      set
      {
        if( this.NotifyOnSourceUpdatedInitialized )
          throw new InvalidOperationException( "An attempt was made to set the NotifyOnSourceUpdated property when it has already been initialized." );

        m_cellBinding.NotifyOnSourceUpdated = value;
        this.NotifyOnSourceUpdatedInitialized = true;
      }
    }

    #endregion NotifyOnSourceUpdated Property

    #region NotifyOnTargetUpdated Property

    public bool NotifyOnTargetUpdated
    {
      get
      {
        return m_cellBinding.NotifyOnTargetUpdated;
      }
      set
      {
        if( this.NotifyOnTargetUpdatedInitialized )
          throw new InvalidOperationException( "An attempt was made to set the NotifyOnTargetUpdated property when it has already been initialized." );

        m_cellBinding.NotifyOnTargetUpdated = value;
        this.NotifyOnTargetUpdatedInitialized = true;
      }
    }

    #endregion NotifyOnTargetUpdated Property

    #region NotifyOnValidationError Property

    public bool NotifyOnValidationError
    {
      get
      {
        return m_cellBinding.NotifyOnValidationError;
      }
      set
      {
        if( this.NotifyOnValidationErrorInitialized )
          throw new InvalidOperationException( "An attempt was made to set the NotifyOnValidationError property when it has already been initialized." );

        m_cellBinding.NotifyOnValidationError = value;
        this.NotifyOnValidationErrorInitialized = true;
      }
    }

    #endregion NotifyOnValidationError Property

    #region PUBLIC METHODS

    public sealed override object ProvideValue( IServiceProvider serviceProvider )
    {
      return m_cellBinding.ProvideValue( serviceProvider );
    }

    #endregion PUBLIC METHODS

    #region PRIVATE PROPERTIES

    private bool ConverterInitialized
    {
      get
      {
        return m_flags[ ( int )CellEditorBindingExtensionFlags.ConverterInitialized ];
      }
      set
      {
        m_flags[ ( int )CellEditorBindingExtensionFlags.ConverterInitialized ] = value;
      }
    }

    private bool ConverterParameterInitialized
    {
      get
      {
        return m_flags[ ( int )CellEditorBindingExtensionFlags.ConverterParameterInitialized ];
      }
      set
      {
        m_flags[ ( int )CellEditorBindingExtensionFlags.ConverterParameterInitialized ] = value;
      }
    }

    private bool ConverterCultureInitialized
    {
      get
      {
        return m_flags[ ( int )CellEditorBindingExtensionFlags.ConverterCultureInitialized ];
      }
      set
      {
        m_flags[ ( int )CellEditorBindingExtensionFlags.ConverterCultureInitialized ] = value;
      }
    }

    private bool NotifyOnSourceUpdatedInitialized
    {
      get
      {
        return m_flags[ ( int )CellEditorBindingExtensionFlags.NotifyOnSourceUpdatedInitialized ];
      }
      set
      {
        m_flags[ ( int )CellEditorBindingExtensionFlags.NotifyOnSourceUpdatedInitialized ] = value;
      }
    }

    private bool NotifyOnTargetUpdatedInitialized
    {
      get
      {
        return m_flags[ ( int )CellEditorBindingExtensionFlags.NotifyOnTargetUpdatedInitialized ];
      }
      set
      {
        m_flags[ ( int )CellEditorBindingExtensionFlags.NotifyOnTargetUpdatedInitialized ] = value;
      }
    }

    private bool NotifyOnValidationErrorInitialized
    {
      get
      {
        return m_flags[ ( int )CellEditorBindingExtensionFlags.NotifyOnValidationErrorInitialized ];
      }
      set
      {
        m_flags[ ( int )CellEditorBindingExtensionFlags.NotifyOnValidationErrorInitialized ] = value;
      }
    }

    #endregion PRIVATE PROPERTIES

    #region PRIVATE FIELDS

    private Binding m_cellBinding = new Binding();
    private BitVector32 m_flags = new BitVector32();

    #endregion PRIVATE FIELDS

    #region NESTED CLASSES

    [Flags]
    private enum CellEditorBindingExtensionFlags
    {
      ConverterInitialized = 1,
      ConverterParameterInitialized = 2,
      ConverterCultureInitialized = 4,
      NotifyOnSourceUpdatedInitialized = 8,
      NotifyOnTargetUpdatedInitialized = 16,
      NotifyOnValidationErrorInitialized = 32
    }

    #endregion NESTED CLASSES
  }
}
