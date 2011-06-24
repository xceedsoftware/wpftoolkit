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
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Collections;

namespace Microsoft.Windows.Controls
{
    public class CheckListBox : MultiSelector
    {
        static CheckListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckListBox), new FrameworkPropertyMetadata(typeof(CheckListBox)));
        }

        #region Properties

        public static readonly DependencyProperty CheckedMemberPathProperty = DependencyProperty.Register("CheckedMemberPath", typeof(string), typeof(CheckListBox), new UIPropertyMetadata(null));
        public string CheckedMemberPath
        {
            get { return (string)GetValue(CheckedMemberPathProperty); }
            set { SetValue(CheckedMemberPathProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(CheckListBox), new PropertyMetadata((ICommand)null));
        [TypeConverter(typeof(CommandConverter))]
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        //public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CheckListBox), new UIPropertyMetadata(null, new PropertyChangedCallback(OnItemsSourceChanged), new CoerceValueCallback(OnCoerceItemsSource)));

        //private static object OnCoerceItemsSource(DependencyObject o, object value)
        //{
        //    CheckListBox checkListBox = o as CheckListBox;
        //    if (checkListBox != null)
        //        return checkListBox.OnCoerceItemsSource((IEnumerable)value);
        //    else
        //        return value;
        //}

        //private static void OnItemsSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        //{
        //    CheckListBox checkListBox = o as CheckListBox;
        //    if (checkListBox != null)
        //        checkListBox.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        //}

        //protected virtual IEnumerable OnCoerceItemsSource(IEnumerable value)
        //{
        //    // TODO: Keep the proposed value within the desired range.
        //    return value;
        //}

        //protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        //{
        //    // TODO: Add your property changed side-effects. Descendants can override as well.
        //}

        //public IEnumerable ItemsSource
        //{
        //    // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
        //    get
        //    {
        //        return (IEnumerable)GetValue(ItemsSourceProperty);
        //    }
        //    set
        //    {
        //        SetValue(ItemsSourceProperty, value);
        //    }
        //}
        

        #endregion //Properties

        #region Base Class Overrides

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CheckListBoxItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is CheckListBoxItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var checkListBoxItem = element as FrameworkElement;

            if (!String.IsNullOrEmpty(CheckedMemberPath))
            {
                Binding isCheckedBinding = new Binding(CheckedMemberPath);
                isCheckedBinding.Source = item;
                checkListBoxItem.SetBinding(CheckListBoxItem.IsCheckedProperty, isCheckedBinding);
            }

            base.PrepareContainerForItemOverride(element, item);
        }

        #endregion //Base Class Overrides
    }
}
