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
  [AttributeUsage( AttributeTargets.Field, AllowMultiple = false )] // Applies only to the fields that store the DependencyProperties
  public sealed class ViewPropertyAttribute : Attribute
  {
    #region Constructors

    public ViewPropertyAttribute( ViewPropertyMode viewPropertyMode )
      : this( viewPropertyMode, FlattenDetailBindingMode.Default )
    {
    }

    internal ViewPropertyAttribute( ViewPropertyMode viewPropertyMode, FlattenDetailBindingMode flattenDetailBindingMode )
    {
      m_viewPropertyMode = viewPropertyMode;
      m_flattenDetailBindingMode = flattenDetailBindingMode;
    }

    #endregion

    #region ViewPropertyMode Property

    public ViewPropertyMode ViewPropertyMode
    {
      get
      {
        return m_viewPropertyMode;
      }
    }

    private readonly ViewPropertyMode m_viewPropertyMode = ViewPropertyMode.None;

    #endregion

    #region FlattenDetailBindingMode Internal Property

    internal FlattenDetailBindingMode FlattenDetailBindingMode
    {
      get
      {
        return m_flattenDetailBindingMode;
      }
    }

    private readonly FlattenDetailBindingMode m_flattenDetailBindingMode = FlattenDetailBindingMode.Default;

    #endregion
  }
}
