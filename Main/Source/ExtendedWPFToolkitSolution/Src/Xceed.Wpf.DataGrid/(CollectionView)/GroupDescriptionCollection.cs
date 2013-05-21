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
