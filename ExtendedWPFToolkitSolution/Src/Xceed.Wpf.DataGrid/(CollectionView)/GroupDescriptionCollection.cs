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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal class GroupDescriptionCollection : ObservableCollection<GroupDescription>
  {
    #region CONSTRUCTORS

    public GroupDescriptionCollection()
    {
    }

    #endregion CONSTRUCTORS

    #region PROTECTED METHODS

    protected override void InsertItem( int index, GroupDescription item )
    {
      string newGroupName;

      if( this.IsGroupDescriptionAlreadyPresent( item, out newGroupName ) == true )
        throw new DataGridInternalException( "A group with the specified name already exists in the collection: " + ( ( newGroupName != null ) ? newGroupName : "" ) );

      base.InsertItem( index, item );
    }

    protected override void SetItem( int index, GroupDescription item )
    {
      string newGroupName;

      if( this.IsGroupDescriptionAlreadyPresent( item, out newGroupName ) == true )
        throw new DataGridInternalException( "A group with the specified name already exists in the collection: " + ( ( newGroupName != null ) ? newGroupName : "" ) );

      base.SetItem( index, item );
    }

    #endregion PROTECTED METHODS

    #region PRIVATE METHODS

    private bool IsGroupDescriptionAlreadyPresent( GroupDescription newGroupDescription, out string newGroupName )
    {
      newGroupName = DataGridContext.GetColumnNameFromGroupDescription( newGroupDescription );

      // We accept null or empty group names
      if( !string.IsNullOrEmpty( newGroupName ) )
      {
        foreach( GroupDescription groupDescription in this.Items )
        {
          string groupName = DataGridContext.GetColumnNameFromGroupDescription( groupDescription );

          if( !string.IsNullOrEmpty( groupName ) )
          {
            if( newGroupName == groupName )
            {
              return true;
            }
          }
        }
      }

      return false;
    }

    #endregion PRIVATE METHODS
  }
}
