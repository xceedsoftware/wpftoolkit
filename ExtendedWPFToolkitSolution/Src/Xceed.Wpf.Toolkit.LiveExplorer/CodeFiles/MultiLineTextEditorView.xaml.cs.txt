/***************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  *************************************************************************************/

using System.Collections.Generic;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Text.Views
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
        Title = "Lord Of The Rings",
        Review = "A great movie with many special effects.",
        Rating = 9
      } );
      movieList.Add( new Movie()
      {
        Title = "Pirates Of The Caribbean",
        Review = "An epic pirate movie with ships, swords, explosions, and a treasure.",
        Rating = 9.5
      } );
      movieList.Add( new Movie()
      {
        Title = "Batman",
        Review = "Batman returns after 8 years, stronger than ever, to deliver Gotham City from a new criminal.",
        Rating = 7.8
      } );
      movieList.Add( new Movie()
      {
        Title = "Indiana Jones",
        Review = "Harrison Ford strikes back for an action-packed movie in the jungle to find a mysterious Crystal skull.",
        Rating = 6.4
      } );

      return movieList;
    }
  }
}
