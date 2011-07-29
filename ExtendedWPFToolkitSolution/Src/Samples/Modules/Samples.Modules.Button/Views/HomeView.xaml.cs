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
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Samples.Modules.Button.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
            DataContext = new MyViewModel();
        }
    }

    public class Item
    {
        public bool IsChecked { get; set; }
        public string Name { get; set; }

        public Item()
        {
            
        }
    }
    public class MyViewModel : INotifyPropertyChanged
    {
        public ICommand MyCommand { get; private set; }

        private int _clickCount;
        public int ClickCount
        {
            get { return _clickCount; }
            set
            {
                _clickCount = value;
                OnPropertyChanged("ClickCount");
            }
        }

        private ObservableCollection<Item> _items;
        public ObservableCollection<Item> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged("Items");
            }
        }
        

        public MyViewModel()
        {
            MyCommand = new CustomCommand(Execute, CanExecute);

            Items = new ObservableCollection<Item>();
            for (int i = 0; i < 10; i++)
            {
                Items.Add(new Item() { IsChecked = i % 2 == 0, Name = String.Format("Item {0}", i) });
            }
        }

        private void Execute(object param)
        {
            ClickCount++;
            //MessageBox.Show(String.Format("Executed {0}", param));
        }

        private bool CanExecute(object param)
        {
            return Convert.ToInt32(param) != 5;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CustomCommand : ICommand
    {
        Action<object> _execute;
        Func<object, bool> _canExecute;

        public CustomCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
