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
using System.Globalization;
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid
{
  public class CellValidatingEventArgs : EventArgs
  {
    public CellValidatingEventArgs(
      object value,
      CultureInfo culture,
      CellValidationContext context )
    {
      m_value = value;
      m_culture = culture;
      m_context = context;
    }

    public object Value
    {
      get
      {
        return m_value;
      }
    }

    public CultureInfo Culture
    {
      get
      {
        return m_culture;
      }
    }

    public CellValidationContext Context
    {
      get
      {
        return m_context;
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    public ValidationResult Result
    {
      get
      {
        return m_validationResult;
      }

      set
      {
        if( value == null )
          throw new ArgumentNullException( "Result" );

        m_validationResult = value;
      }
    }

    object m_value;
    CultureInfo m_culture;
    CellValidationContext m_context;
    ValidationResult m_validationResult = ValidationResult.ValidResult;
  }
}
