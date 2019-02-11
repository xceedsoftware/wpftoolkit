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
  internal sealed class TableViewPage
  {
    #region Static Fields

    internal static readonly TableViewPage Empty = new TableViewPage();

    #endregion

    #region Constructors

    internal TableViewPage( TableViewFullPageInfo innerPage, TableViewFullPageInfo outerPage, double viewportHeight )
    {
      if( innerPage == null )
        throw new ArgumentNullException( "innerPage" );

      if( outerPage == null )
        throw new ArgumentNullException( "outerPage" );

      if( innerPage.Start < outerPage.Start )
        throw new ArgumentException("The inner page must be a subset of the outer page.", "innerPage");

      if( innerPage.End > outerPage.End )
        throw new ArgumentException( "The inner page must be a subset of the outer page.", "innerPage" );

      if( viewportHeight < 0d )
        throw new ArgumentException( "viewportHeight must be greater than to equal to zero.", "viewportHeight" );

      m_innerPage = innerPage;
      m_outerPage = outerPage;
      m_viewportHeight = viewportHeight;
    }

    private TableViewPage()
    {
      m_innerPage = TableViewPageInfo.Empty;
      m_outerPage = TableViewPageInfo.Empty;
    }

    #endregion

    #region InnerPage Property

    public TableViewPageInfo InnerPage
    {
      get
      {
        return m_innerPage;
      }
    }

    private readonly TableViewPageInfo m_innerPage;

    #endregion

    #region OuterPage Property

    public TableViewPageInfo OuterPage
    {
      get
      {
        return m_outerPage;
      }
    }

    private readonly TableViewPageInfo m_outerPage;

    #endregion

    #region ViewportHeight Property

    public double ViewportHeight
    {
      get
      {
        return m_viewportHeight;
      }
    }

    private readonly double m_viewportHeight; //0d

    #endregion
  }
}
