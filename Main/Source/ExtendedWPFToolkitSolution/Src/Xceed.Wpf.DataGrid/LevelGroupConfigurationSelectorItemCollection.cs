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

namespace Xceed.Wpf.DataGrid
{
  internal sealed class LevelGroupConfigurationSelectorItemCollection : ObservableCollection<LevelGroupConfigurationSelectorItem>
  {
    internal LevelGroupConfigurationSelectorItemCollection()
      : base()
    {
    }

    public LevelGroupConfigurationSelectorItem GetGroupConfigurationSelectorItem( int level )
    {
      LevelGroupConfigurationSelectorItem retval = null;

      foreach( LevelGroupConfigurationSelectorItem levelGroupConfig in this )
      {
        if( levelGroupConfig.Level == level )
        {
          retval = levelGroupConfig;
          break;
        }
      }

      return retval;
    }

    protected override void InsertItem( int index, LevelGroupConfigurationSelectorItem item )
    {
      if( this.GetGroupConfigurationSelectorItem( item.Level ) != null )
        throw new InvalidOperationException( "An attempt was made to insert a LevelGroupConfigurationSelectorItem that specifies the same level as an existing LevelGroupConfigurationSelectorItem." );

      base.InsertItem( index, item );

      item.Seal();
    }

    protected override void SetItem( int index, LevelGroupConfigurationSelectorItem item )
    {
      LevelGroupConfigurationSelectorItem oldLevelGroupConfig = this[ index ];
      LevelGroupConfigurationSelectorItem failingLevelGroupConfig = this.GetGroupConfigurationSelectorItem( item.Level );

      if( ( failingLevelGroupConfig != null ) && ( oldLevelGroupConfig != failingLevelGroupConfig ) )
        throw new InvalidOperationException( "An attempt was made to set a LevelGroupConfigurationSelectorItem that specifies the same level as an existing LevelGroupConfigurationSelectorItem." );

      base.SetItem( index, item );

      item.Seal();
    }
  }
}
