/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows.Data;
using Xceed.Wpf.DataGrid.Utils;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class DataItemDataProviderBase : DataSourceProvider
  {
    internal static readonly string DataPropertyName = PropertyHelper.GetPropertyName( ( DataItemDataProviderBase s ) => s.Data );

    #region IsEmpty Property

    public abstract bool IsEmpty
    {
      get;
    }

    #endregion

    public abstract void SetDataItem( object dataItem );
    public abstract void ClearDataItem();
  }
}
