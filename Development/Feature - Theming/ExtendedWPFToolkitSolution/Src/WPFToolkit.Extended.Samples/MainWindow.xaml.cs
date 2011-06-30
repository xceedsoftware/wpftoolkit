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
using Microsoft.Windows.Controls;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace WPFToolkit.Extended.Samples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new Data();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Calculator_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal?> e)
        {
            Debug.WriteLine(e.NewValue.HasValue ? e.NewValue.Value.ToString() : "NULL");
        }

        private void ColorCanvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Debug.WriteLine(e.NewValue);
        }
    }

    public class Data : System.ComponentModel.INotifyPropertyChanged
    {

        protected string _Text = "C2";
        public string Text
        {
            get { return _Text; }
            set
            {
                _Text = value;
                NotifyPropertyChanged("Text");
            }
        }

        private int? _integer = 5;
        public int? Integer
        {
            get { return _integer; }
            set
            {
                _integer = value;
                NotifyPropertyChanged("Integer");
            }
        }

        private double? _double = 5;
        public double? Double
        {
            get { return _double; }
            set
            {
                _double = value;
                NotifyPropertyChanged("Double");
            }
        }

        private decimal? _decimal = 5;
        public decimal? Decimal
        {
            get { return _decimal; }
            set
            {
                _decimal = value;
                NotifyPropertyChanged("Decimal");
            }
        }
        

        private DateTime? _dueDate;
        public DateTime? DueDate
        {
            get { return _dueDate; }
            set
            {
                _dueDate = value;
                NotifyPropertyChanged("DueDate");
            }
        }



        public Data()
        {
            //Text = "testm";
        }

        #region PropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }
}
