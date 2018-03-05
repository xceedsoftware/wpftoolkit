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
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridBindingInfo
  {
    public DataGridBindingInfo()
    {
      m_binding = new Binding();

      m_binding.Mode = BindingMode.TwoWay;
      m_binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
      m_binding.ConverterCulture = CultureInfo.CurrentCulture;

      m_binding.ValidatesOnDataErrors = true;
      m_binding.ValidatesOnExceptions = true;

      m_binding.NotifyOnTargetUpdated = true;
      m_binding.NotifyOnValidationError = true;
    }

    #region BindsDirectlyToSource Property

    public bool BindsDirectlyToSource
    {
      get
      {
        return m_binding.BindsDirectlyToSource;
      }

      set
      {
        m_binding.BindsDirectlyToSource = value;
      }
    }

    #endregion BindsDirectlyToSource Property

    #region Converter Property

    public IValueConverter Converter
    {
      get
      {
        return m_binding.Converter;
      }

      set
      {
        m_binding.Converter = value;
      }
    }

    #endregion Converter Property

    #region ConverterCulture Property

    public CultureInfo ConverterCulture
    {
      get
      {
        return m_binding.ConverterCulture;
      }

      set
      {
        m_binding.ConverterCulture = value;
      }
    }

    #endregion ConverterCulture Property

    #region ConverterParameter Property

    public object ConverterParameter
    {
      get
      {
        return m_binding.ConverterParameter;
      }

      set
      {
        m_binding.ConverterParameter = value;
      }
    }

    #endregion ConverterParameter Property

    #region ElementName Property

    public string ElementName
    {
      get
      {
        return m_binding.ElementName;
      }

      set
      {
        m_binding.ElementName = value;
      }
    }

    #endregion ElementName Property

    #region FallbackValue Property

    public object FallbackValue
    {
      get
      {
        return m_binding.FallbackValue;
      }

      set
      {
        m_binding.FallbackValue = value;
      }
    }

    #endregion FallbackValue Property

    #region IsAsync Property

    public bool IsAsync
    {
      get
      {
        return m_binding.IsAsync;
      }

      set
      {
        m_binding.IsAsync = value;
      }
    }

    #endregion IsAsync Property

    #region NotifyOnSourceUpdated Property

    public bool NotifyOnSourceUpdated
    {
      get
      {
        return m_binding.NotifyOnSourceUpdated;
      }

      set
      {
        m_binding.NotifyOnSourceUpdated = value;
      }
    }

    #endregion NotifyOnSourceUpdated Property

    #region Path Property

    public PropertyPath Path
    {
      get
      {
        return m_binding.Path;
      }

      set
      {
        m_binding.Path = value;
      }
    }

    #endregion Path Property

    #region ReadOnly Property

    public bool ReadOnly
    {
      get
      {
        return ( m_binding.Mode == BindingMode.OneWay );
      }

      set
      {
        if( value )
        {
          m_binding.Mode = BindingMode.OneWay;
        }
        else
        {
          m_binding.Mode = BindingMode.TwoWay;
        }
      }
    }

    #endregion ReadOnly Property

    #region UpdateSourceExceptionFilter Property

    public UpdateSourceExceptionFilterCallback UpdateSourceExceptionFilter
    {
      get
      {
        return m_binding.UpdateSourceExceptionFilter;
      }

      set
      {
        m_binding.UpdateSourceExceptionFilter = value;
      }
    }

    #endregion UpdateSourceExceptionFilter Property

    #region ValidationRules Property

    public Collection<ValidationRule> ValidationRules
    {
      get
      {
        return m_binding.ValidationRules;
      }
    }

    #endregion ValidationRules Property

    #region XPath Property

    public string XPath
    {
      get
      {
        return m_binding.XPath;
      }

      set
      {
        m_binding.XPath = value;
      }
    }

    #endregion Path Property

    internal Binding GetBinding()
    {
      return m_binding;
    }

    private Binding m_binding;
  }
}
