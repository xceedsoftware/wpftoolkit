/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridItemPropertyDictionary : Dictionary<DataGridItemPropertyBase, object>
  {
    internal DataGridItemPropertyDictionary()
    {
    }

    public object this[ string name ]
    {
      get
      {
        foreach( object item in this.Keys )
        {
          DataGridItemPropertyBase dataGridItemProperty = item as DataGridItemPropertyBase;
          Debug.Assert( dataGridItemProperty != null );

          if( dataGridItemProperty.Name.Equals( name ) )
            return this[ dataGridItemProperty ];
        }

        return null;
      }
    }
  }
}
