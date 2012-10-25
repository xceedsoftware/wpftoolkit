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
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  [ContentProperty( "SelectorItems" )]
  public class LevelGroupConfigurationSelector : GroupConfigurationSelector
  {
    private LevelGroupConfigurationSelectorItemCollection m_groupConfigurationSelectorItems = new LevelGroupConfigurationSelectorItemCollection();

    public ObservableCollection<LevelGroupConfigurationSelectorItem> SelectorItems
    {
      get
      {
        return m_groupConfigurationSelectorItems;
      }
    }

    public override GroupConfiguration SelectGroupConfiguration(int groupLevel, CollectionViewGroup collectionViewGroup, System.ComponentModel.GroupDescription groupDescription)
    {
      if (m_groupConfigurationSelectorItems.Count == 0)
        return base.SelectGroupConfiguration( groupLevel, collectionViewGroup, groupDescription );

      LevelGroupConfigurationSelectorItem levelGroupConfig = m_groupConfigurationSelectorItems.GetGroupConfigurationSelectorItem( groupLevel );
      if( levelGroupConfig != null )
      {
        return levelGroupConfig.GroupConfiguration;
      }

      return base.SelectGroupConfiguration( groupLevel, collectionViewGroup, groupDescription );
    }
  }
}
