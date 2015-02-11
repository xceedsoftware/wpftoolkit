/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class ColumnAddGroupCommand : ColumnGroupCommand
  {
    #region GroupDescriptions Protected Property

    protected abstract ObservableCollection<GroupDescription> GroupDescriptions
    {
      get;
    }

    #endregion

    public bool CanExecute( ColumnBase column )
    {
      return this.CanExecute( column, -1 );
    }

    public bool CanExecute( ColumnBase column, int index )
    {
      return ( column != null )
          && ( this.CanExecuteCore( column, index ) );
    }

    public void Execute( ColumnBase column )
    {
      this.Execute( column, -1 );
    }

    public void Execute( ColumnBase column, int index )
    {
      if( !this.CanExecute( column, index ) )
        return;

      this.ExecuteCore( column, index );
    }

    protected virtual string GetColumnName( ColumnBase column )
    {
      return column.FieldName;
    }

    protected virtual GroupDescription GetGroupDescription( ColumnBase column )
    {
      return column.GroupDescription;
    }

    protected virtual GroupConfiguration GetGroupConfiguration( ColumnBase column )
    {
      return column.GroupConfiguration;
    }

    protected virtual bool CanExecuteCore( ColumnBase column, int index )
    {
      return ( this.GroupDescriptions != null )
          && ( !string.IsNullOrEmpty( this.GetColumnName( column ) ) );
    }

    protected virtual void ExecuteCore( ColumnBase column, int index )
    {
      var groupDescriptions = this.GroupDescriptions;
      if( groupDescriptions == null )
        return;

      GroupDescription groupDescription = this.GetGroupDescription( column );
      if( groupDescription == null )
      {
        groupDescription = new DataGridGroupDescription( this.GetColumnName( column ) );
      }

      var dataGridGroupDescription = groupDescription as DataGridGroupDescription;
      if( ( dataGridGroupDescription != null ) && ( dataGridGroupDescription.GroupConfiguration == null ) )
      {
        dataGridGroupDescription.GroupConfiguration = this.GetGroupConfiguration( column );
      }

      if( index < 0 )
      {
        groupDescriptions.Add( groupDescription );
      }
      else
      {
        groupDescriptions.Insert( index, groupDescription );
      }
    }

    protected sealed override bool CanExecuteImpl( object parameter )
    {
      return this.CanExecute( parameter as ColumnBase );
    }

    protected sealed override void ExecuteImpl( object parameter )
    {
      this.Execute( parameter as ColumnBase );
    }
  }
}
