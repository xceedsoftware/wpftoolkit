/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Controls;

namespace Samples.Modules.Color.Views
{
  /// <summary>
  /// Interaction logic for HomeView.xaml
  /// </summary>
  [RegionMemberLifetime( KeepAlive = false )]
  public partial class HomeView : DemoView
  {
    public HomeView()
    {
      InitializeComponent();

      DataContext = new Data();

      //List<string> colors = new List<string>();
      //colors.Add("1");
      //colors.Add("2");
      //colors.Add("3");
      //colors.Add("4");
      //colors.Add("5");
      //colors.Add("6");
      //colors.Add("7");
      //colors.Add("8");
      //colors.Add("9");
      //colors.Add("10");


      List<Person> colors = new List<Person>();
      colors.Add( new Person( System.Windows.Media.Colors.Red, 0 ) );
      colors.Add( new Person( System.Windows.Media.Colors.Purple, 1 )
      {
        IsSelected = true
      } );
      colors.Add( new Person( System.Windows.Media.Colors.Coral, 2 ) );
      colors.Add( new Person( System.Windows.Media.Colors.MidnightBlue, 3 )
      {
        IsSelected = true
      } );
      colors.Add( new Person( System.Windows.Media.Colors.Green, 4 ) );
      colors.Add( new Person( System.Windows.Media.Colors.Red, 5 ) );
      colors.Add( new Person( System.Windows.Media.Colors.Purple, 6 ) );
      colors.Add( new Person( System.Windows.Media.Colors.SaddleBrown, 7 ) );
      colors.Add( new Person( System.Windows.Media.Colors.MidnightBlue, 8 ) );
      colors.Add( new Person( System.Windows.Media.Colors.Green, 9 ) );
      colors.Add( new Person( System.Windows.Media.Colors.Red, 10 ) );
      colors.Add( new Person( System.Windows.Media.Colors.Purple, 11 ) );
      colors.Add( new Person( System.Windows.Media.Colors.SaddleBrown, 12 ) );
      colors.Add( new Person( System.Windows.Media.Colors.MidnightBlue, 13 ) );
      colors.Add( new Person( System.Windows.Media.Colors.Green, 14 ) );
      colors.Add( new Person( System.Windows.Media.Colors.Red, 15 ) );
      colors.Add( new Person( System.Windows.Media.Colors.Purple, 16 ) );
      colors.Add( new Person( System.Windows.Media.Colors.SaddleBrown, 17 ) );
      colors.Add( new Person( System.Windows.Media.Colors.MidnightBlue, 18 ) );
      colors.Add( new Person( System.Windows.Media.Colors.Green, 19 ) );

      //_dataGrid.ItemsSource = colors;
      //_chk.ItemsSource = colors;
      _combo.ItemsSource = colors;
      //_combo.SelectedValue = "1,3,5,7,9,";

      _listBox.ItemsSource = colors;
      //_listBox.SelectedValue = "1,3,5,7,9,";
    }

    private void Button_Click( object sender, System.Windows.RoutedEventArgs e )
    {
      ( DataContext as Data ).SelectedValue = "1,3,5,7,9,";
    }
  }

  public class Data : INotifyPropertyChanged
  {
    private string _selectedValues;// = "1,3,5,7,9,";
    public string SelectedValue
    {
      get
      {
        return _selectedValues;
      }
      set
      {
        _selectedValues = value;
        OnPropertyChanged( "SelectedValue" );
      }
    }

    private ObservableCollection<Person> _selectedItems = new ObservableCollection<Person>();
    public ObservableCollection<Person> SelectedItems
    {
      get
      {
        return _selectedItems;
      }
      set
      {
        _selectedItems = value;
        OnPropertyChanged( "SelectedItems" );
      }
    }

    public Data()
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged( string propertyName )
    {
      if( PropertyChanged != null )
        PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
    }
  }

  public class Person : INotifyPropertyChanged
  {
    private bool _isSelected;
    public bool IsSelected
    {
      get
      {
        return _isSelected;
      }
      set
      {
        _isSelected = value;
        OnPropertyChanged( "IsSelected" );
      }
    }

    System.Windows.Media.Color _color;
    public System.Windows.Media.Color Color
    {
      get
      {
        return _color;
      }
      set
      {
        _color = value;
        OnPropertyChanged( "Color" );
      }
    }

    int _level;
    public int Level
    {
      get
      {
        return _level;
      }
      set
      {
        _level = value;
        OnPropertyChanged( "Level" );
      }
    }

    public Person()
    {
    }

    public Person( System.Windows.Media.Color color, int level )
    {
      this._color = color;
      this._level = level;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged( string propertyName )
    {
      if( PropertyChanged != null )
        PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
    }
  }
}
