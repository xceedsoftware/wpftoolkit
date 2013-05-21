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
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  public partial class DataGridItemProperty
  {
    internal class PropertyDescriptorFromItemProperty : DataGridItemPropertyBase.PropertyDescriptorFromItemPropertyBase
    {
      public PropertyDescriptorFromItemProperty( DataGridItemProperty dataGridItemProperty )
        : base( dataGridItemProperty )
      {
        m_propertyDescriptorForValueChanged = dataGridItemProperty.PropertyDescriptor;
      }

      public override void AddValueChanged( object component, EventHandler handler )
      {
        if( m_propertyDescriptorForValueChanged != null )
        {
          if( !( component is EmptyDataItem ) )
          {
            m_propertyDescriptorForValueChanged.AddValueChanged( component, handler );
          }
        }
        else
        {
          base.AddValueChanged( component, handler );
        }
      }

      public override void RemoveValueChanged( object component, EventHandler handler )
      {
        if( m_propertyDescriptorForValueChanged != null )
        {
          if( !( component is EmptyDataItem ) )
          {
            m_propertyDescriptorForValueChanged.RemoveValueChanged( component, handler );
          }
        }
        else
        {
          base.RemoveValueChanged( component, handler );
        }
      }

      private PropertyDescriptor m_propertyDescriptorForValueChanged;
    }
  }
}
