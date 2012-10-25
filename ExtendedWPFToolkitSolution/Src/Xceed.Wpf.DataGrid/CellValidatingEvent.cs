/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Globalization;

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
