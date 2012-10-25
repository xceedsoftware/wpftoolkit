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
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class AutoFilterValuesChangedEventArgs : EventArgs
  {
    internal AutoFilterValuesChangedEventArgs(
      DataGridDetailDescription detailDescription,
      DataGridItemPropertyBase itemProperty,
      IList autoFilterValues,
      NotifyCollectionChangedEventArgs collectionChangedEvent )
    {
      if( itemProperty == null )
        throw new ArgumentNullException( "itemProperty" );

      if( autoFilterValues == null )
        throw new ArgumentNullException( "autoFilterValues" );

      if( collectionChangedEvent == null )
        throw new ArgumentNullException( "collectionChangedEvent" );

      this.DetailDescription = detailDescription;
      this.ItemProperty = itemProperty;
      this.CollectionChangedEventArgs = collectionChangedEvent;
      this.AutoFilterValues = autoFilterValues;
    }

    internal DataGridDetailDescription DetailDescription
    {
      get;
      private set;
    }

    public DataGridItemPropertyBase ItemProperty
    {
      get;
      private set;
    }

    public NotifyCollectionChangedEventArgs CollectionChangedEventArgs
    {
      get;
      private set;
    }

    public ICollection AutoFilterValues
    {
      get;
      private set;
    }
  }
}
