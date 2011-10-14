using System;
using Samples.Infrastructure.Controls;
using Microsoft.Practices.Prism.Regions;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Samples.Modules.Color.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    [RegionMemberLifetime(KeepAlive = false)]
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
            colors.Add(new Person(System.Windows.Media.Colors.Red, 0));
            colors.Add(new Person(System.Windows.Media.Colors.Purple, 1));
            colors.Add(new Person(System.Windows.Media.Colors.Coral, 2));
            colors.Add(new Person(System.Windows.Media.Colors.MidnightBlue, 3));
            colors.Add(new Person(System.Windows.Media.Colors.Green, 4));
            colors.Add(new Person(System.Windows.Media.Colors.Red, 5));
            colors.Add(new Person(System.Windows.Media.Colors.Purple, 6));
            colors.Add(new Person(System.Windows.Media.Colors.SaddleBrown, 7));
            colors.Add(new Person(System.Windows.Media.Colors.MidnightBlue, 8));
            colors.Add(new Person(System.Windows.Media.Colors.Green, 9));
            colors.Add(new Person(System.Windows.Media.Colors.Red, 10));
            colors.Add(new Person(System.Windows.Media.Colors.Purple, 11));
            colors.Add(new Person(System.Windows.Media.Colors.SaddleBrown, 12));
            colors.Add(new Person(System.Windows.Media.Colors.MidnightBlue, 13));
            colors.Add(new Person(System.Windows.Media.Colors.Green, 14));
            colors.Add(new Person(System.Windows.Media.Colors.Red, 15));
            colors.Add(new Person(System.Windows.Media.Colors.Purple, 16));
            colors.Add(new Person(System.Windows.Media.Colors.SaddleBrown, 17));
            colors.Add(new Person(System.Windows.Media.Colors.MidnightBlue, 18));
            colors.Add(new Person(System.Windows.Media.Colors.Green, 19));

            //_dataGrid.ItemsSource = colors;
            //_chk.ItemsSource = colors;
            _combo.ItemsSource = colors;
            //_combo.SelectedValue = "1,3,5,7,9,";

            _listBox.ItemsSource = colors;
            //_listBox.DelimitedValue = "1,3,5,7,9,";
        }
    }

    public class Data
    {
        private string _selectedValues;// = "1,3,5,7,9,";
        public string SelectedValues
        {
            get
            {
                return _selectedValues;
            }
            set
            {
                _selectedValues = value;
            }
        }

        private ObservableCollection<Person> _selectedItems = new ObservableCollection<Person>()
        { 
            new Person(System.Windows.Media.Colors.Red, 0),
            new Person(System.Windows.Media.Colors.Coral, 2)            
        };

        public ObservableCollection<Person> SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                _selectedItems = value;
            }
        }        
    }

    public class Person : INotifyPropertyChanged
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        System.Windows.Media.Color _color;
        public System.Windows.Media.Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("Color");
            }
        }

        int _level;
        public int Level
        {
            get { return _level; }
            set { _level = value; OnPropertyChanged("Level"); }
        }

        public Person()
        {
        }

        public Person(System.Windows.Media.Color color, int level)
        {
            this._color = color;
            this._level = level;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
