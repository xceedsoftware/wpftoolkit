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
using System.Collections;

namespace Xceed.Utils.Data
{
  // When deriving from that class, ensure that the initial SetCapacity call is 
  // setting the value to Null ( DefaultValue )
  internal abstract class DataStore
  {
    public abstract int Compare( int xRecordIndex, int yRecordIndex );

    public abstract object GetData( int recordIndex );
    public abstract void SetData( int recordIndex, object data );
    public abstract void SetCapacity( int capacity );
  }
}
