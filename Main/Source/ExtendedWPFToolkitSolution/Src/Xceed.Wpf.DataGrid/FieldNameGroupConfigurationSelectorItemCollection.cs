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

namespace Xceed.Wpf.DataGrid
{
  internal sealed class FieldNameGroupConfigurationSelectorItemCollection : ObservableCollection<FieldNameGroupConfigurationSelectorItem>
  {
    internal FieldNameGroupConfigurationSelectorItemCollection()
      :base()
    {
    }

    public FieldNameGroupConfigurationSelectorItem GetGroupConfigurationSelectorItem( string fieldName )
    {
      FieldNameGroupConfigurationSelectorItem retval = null;

      foreach( FieldNameGroupConfigurationSelectorItem fieldNameGroupConfig in this )
      {
        if( fieldNameGroupConfig.FieldName == fieldName )
        {
          retval = fieldNameGroupConfig;
          break;
        }
      }

      return retval;
    }

    protected override void InsertItem( int index, FieldNameGroupConfigurationSelectorItem item )
    {
      if( this.GetGroupConfigurationSelectorItem( item.FieldName ) != null )
        throw new InvalidOperationException( "An attempt was made to insert a FieldNameGroupConfigurationSelectorItem that has the same field name as an existing FieldNameGroupConfigurationSelectorItem." );

      base.InsertItem( index, item );

      item.Seal();
    }

    protected override void SetItem( int index, FieldNameGroupConfigurationSelectorItem item )
    {
      FieldNameGroupConfigurationSelectorItem oldGroupConfig = this[ index ];
      FieldNameGroupConfigurationSelectorItem failingGroupConfig = this.GetGroupConfigurationSelectorItem( item.FieldName );

      if( ( failingGroupConfig != null ) && ( oldGroupConfig != failingGroupConfig ) )
        throw new InvalidOperationException( "An attempt was made to set a FieldNameGroupConfigurationSelectorItem that has the same field name as an existing FieldNameGroupConfigurationSelectorItem." );

      base.SetItem( index, item );

      item.Seal();
    }
  }
}
