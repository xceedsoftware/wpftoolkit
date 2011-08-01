using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Samples.Modules.PropertyGrid.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
            _listBox.Items.Add(new Data() { Name = "Item One" });
            _listBox.Items.Add(new Data() { Name = "Item Two" });
        }
    }

    public class Data
    {
        private List<Person> _pages = new List<Person>();
        public List<Person> Pages
        {
            get { return _pages; }
            set
            {
                _pages = value;
            }
        }

        private List<int> _valueTypes = new List<int>() { 1, 2, 3 };
        public List<int> ValueTypes
        {
            get { return _valueTypes; }
            set
            {
                _valueTypes = value;
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        private bool? _isLate;
        public bool? IsLate
        {
            get { return _isLate; }
            set
            {
                _isLate = value;
            }
        }

        private DateTime? _datOfBirth;
        public DateTime? DatOfBirth
        {
            get { return _datOfBirth; }
            set
            {
                _datOfBirth = value;
            }
        }

        private Color _color;
        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
            }
        }
        

        public Data()
        {
            Pages.Add(new Person() { FirstName = "One" });
            Pages.Add(new Person() { FirstName = "Two" });
        }
    }

    public class Person
    {
        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
            }
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
            }
        }

        public Person()
        {

        }
    }
}
