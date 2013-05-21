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
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xceed.Wpf.DataGrid
{
  [ContentProperty( "SelectorItems" )]
  public class FieldNameGroupConfigurationSelector : GroupConfigurationSelector
  {
    private FieldNameGroupConfigurationSelectorItemCollection m_groupConfigurationSelectorItems = new FieldNameGroupConfigurationSelectorItemCollection();

      public ObservableCollection<FieldNameGroupConfigurationSelectorItem> SelectorItems
    {
      get
      {
        return m_groupConfigurationSelectorItems;
      }
    }

    public override GroupConfiguration SelectGroupConfiguration( int groupLevel, CollectionViewGroup collectionViewGroup, System.ComponentModel.GroupDescription groupDescription )
    {
      if( m_groupConfigurationSelectorItems.Count == 0 )
        return base.SelectGroupConfiguration( groupLevel, collectionViewGroup, groupDescription );

      string fieldName = string.Empty;

      DataGridGroupDescription dataGridGroupDescription = groupDescription as DataGridGroupDescription;
      if( dataGridGroupDescription != null )
      {
        fieldName = dataGridGroupDescription.PropertyName;
      }
      else
      {
        PropertyGroupDescription propertyGroupDescription = groupDescription as PropertyGroupDescription;
        if( propertyGroupDescription != null )
        {
          fieldName = propertyGroupDescription.PropertyName;
        }
      }

      if( String.IsNullOrEmpty( fieldName ) == true )
        return base.SelectGroupConfiguration( groupLevel, collectionViewGroup, groupDescription );

      FieldNameGroupConfigurationSelectorItem groupConfig = m_groupConfigurationSelectorItems.GetGroupConfigurationSelectorItem( fieldName );
      if( groupConfig != null )
      {
        return groupConfig.GroupConfiguration;
      }

      return base.SelectGroupConfiguration( groupLevel, collectionViewGroup, groupDescription );
    }
  }
}
