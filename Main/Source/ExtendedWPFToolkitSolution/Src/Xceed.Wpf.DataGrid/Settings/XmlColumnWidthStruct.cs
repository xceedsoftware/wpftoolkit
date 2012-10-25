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
using System.Linq;
using System.Text;

namespace Xceed.Wpf.DataGrid.Settings
{
  public struct XmlColumnWidth
  {
    #region CONSTRUCTORS

    public XmlColumnWidth( ColumnWidth columnWidth )
    {
      m_value = columnWidth.Value;
      m_unitType = columnWidth.UnitType;
    }

    #endregion CONSTRUCTORS

    #region Value Property

    public double Value
    {
      get
      {
        return m_value;
      }
      set
      {
        m_value = value;
      }
    }

    #endregion Value Property

    #region UnitType Property

    public ColumnWidthUnitType UnitType
    {
      get
      {
        return m_unitType;
      }
      set
      {
        m_unitType = value;
      }
    }

    #endregion UnitType Property

    #region PUBLIC METHODS

    public ColumnWidth ConvertToColumnWidth()
    {
      return new ColumnWidth( this.Value, this.UnitType );
    }

    #endregion PUBLIC METHODS

    #region PRIVATE FIELDS

    private double m_value;
    private ColumnWidthUnitType m_unitType;

    #endregion PRIVATE FIELDS
  }
}
