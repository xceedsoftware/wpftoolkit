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
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DataItemTypeDescriptionProvider : TypeDescriptionProvider
  {
    #region Constructor

    internal DataItemTypeDescriptionProvider( TypeDescriptionProvider parent )
      : base( parent )
    {
    }

    #endregion

    public override ICustomTypeDescriptor GetTypeDescriptor( Type objectType, object instance )
    {
      var descriptor = base.GetTypeDescriptor( objectType, instance );

      return DataItemTypeDescriptionProvider.GetTypeDescriptor( objectType, descriptor );
    }

    internal static ICustomTypeDescriptor GetTypeDescriptor( Type objectType, ICustomTypeDescriptor descriptor )
    {
      if( descriptor == null )
        return null;

      return new DataItemTypeDescriptor( descriptor, objectType );
    }
  }
}
