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
