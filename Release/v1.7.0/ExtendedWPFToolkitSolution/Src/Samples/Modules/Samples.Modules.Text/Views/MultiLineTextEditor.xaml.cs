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


using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Controls;
using System.Collections.Generic;
namespace Samples.Modules.Text.Views
{
  public class Movie
  {
    public string Title
    {
      get;
      set;
    }

    public string Review
    {
      get;
      set;
    }

    public double Rating
    {
      get;
      set;
    }
  }

  /// <summary>
  /// Interaction logic for MultiLineTextEditor.xaml
  /// </summary>
  [RegionMemberLifetime( KeepAlive = false )]
  public partial class MultiLineTextEditorView : DemoView
  {
    public MultiLineTextEditorView()
    {
      InitializeComponent();
      _dataGrid.DataContext = InitMovieList();
    }

    private List<Movie> InitMovieList()
    {
      List<Movie> movieList = new List<Movie>();
      movieList.Add( new Movie()
      {
        Title = "Lords Of The Ring",
        Review = "A great movie with many special effects.",
        Rating = 9
      } );
      movieList.Add( new Movie()
      {
        Title = "Pirates Of The Caribbean",
        Review = "An epic pirate movie with ships, swords, explosion and a treasure.",
        Rating = 9.5
      } );
      movieList.Add( new Movie()
      {
        Title = "Batman",
        Review = "Batman returns after 8 years, stronger than ever, to deliver Gotham City from a new criminel.",
        Rating = 7.8
      } );
      movieList.Add( new Movie()
      {
        Title = "Indiana Jones",
        Review = "Harrison Ford strikes back for full-pack action movie in the jungle to uncover a mysterious Crystal skull.",
        Rating = 6.4
      } );

      return movieList;
    }
  }
}
