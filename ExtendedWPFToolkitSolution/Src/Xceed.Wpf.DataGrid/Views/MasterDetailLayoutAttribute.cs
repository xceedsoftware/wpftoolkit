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

namespace Xceed.Wpf.DataGrid.Views
{
  [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )] // Applies only to the class of a view
  internal sealed class MasterDetailLayoutAttribute : Attribute
  {
    public MasterDetailLayoutAttribute( MasterDetailLayoutMode masterDetailLayoutMode )
    {
      if( !Enum.IsDefined( typeof( MasterDetailLayoutMode ), masterDetailLayoutMode ) )
        throw new ArgumentException( "The value is not a valid enum value.", "masterDetailLayoutMode" );

      m_masterDetailLayoutMode = masterDetailLayoutMode;
    }

    public MasterDetailLayoutMode MasterDetailLayoutMode
    {
      get
      {
        return m_masterDetailLayoutMode;
      }
    }

    private MasterDetailLayoutMode m_masterDetailLayoutMode = MasterDetailLayoutMode.Default;
  }
}
