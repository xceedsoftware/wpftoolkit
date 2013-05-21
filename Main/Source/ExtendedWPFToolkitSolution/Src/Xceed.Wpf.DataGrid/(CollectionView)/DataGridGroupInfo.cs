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
using System.Windows.Data;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridGroupInfo
  {
    internal DataGridGroupInfo( GroupDescription groupDescription, CollectionViewGroup collectionViewGroup )
    {
      if( groupDescription == null )
        throw new ArgumentNullException( "groupDescription" );

      if( collectionViewGroup == null )
        throw new ArgumentNullException( "collectionViewGroup" );

      this.GroupDescription = groupDescription;
      this.PropertyName = DataGridCollectionViewBase.GetPropertyNameFromGroupDescription( groupDescription );
      this.Value = collectionViewGroup.Name;
    }

    #region GroupDescription PROPERTY

    public GroupDescription GroupDescription
    {
      get;
      private set;
    }

    #endregion GroupDescription PROPERTY

    #region Value PROPERTY

    public object Value
    {
      get;
      private set;
    }

    #endregion Value PROPERTY

    #region PropertyName PROPERTY

    public string PropertyName
    {
      get;
      private set;
    }

    #endregion PropertyName PROPERTY
  }
}
