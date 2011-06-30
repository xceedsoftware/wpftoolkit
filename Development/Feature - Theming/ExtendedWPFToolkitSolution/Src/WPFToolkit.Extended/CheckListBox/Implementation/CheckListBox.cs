using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections;

namespace Microsoft.Windows.Controls
{
    public class CheckListBox : ItemsControl
    {
        private bool _surpressSelectionChanged;

        #region Constructors

        static CheckListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckListBox), new FrameworkPropertyMetadata(typeof(CheckListBox)));
        }

        public CheckListBox()
        {
            SelectedItems = new List<object>();
            AddHandler(CheckListBox.SelectedEvent, new RoutedEventHandler(CheckListBox_Selected));
            AddHandler(CheckListBox.UnselectedEvent, new RoutedEventHandler(CheckListBox_Unselected));
        }

        #endregion //Constructors

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

        #region SelectedItem

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(CheckListBox), new UIPropertyMetadata(null, OnSelectedItemChanged));
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        private static void OnSelectedItemChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CheckListBox checkListBox = o as CheckListBox;
            if (checkListBox != null)
                checkListBox.OnSelectedItemChanged((object)e.OldValue, (object)e.NewValue);
        }

        protected virtual void OnSelectedItemChanged(object oldValue, object newValue)
        {
            OnSelectionChanged();
        }

        #endregion //SelectedItem

        public IList SelectedItems { get; private set; }

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
            _surpressSelectionChanged = true;
            var checkListBoxItem = element as FrameworkElement;
            if (!String.IsNullOrEmpty(CheckedMemberPath))
            {
                Binding isCheckedBinding = new Binding(CheckedMemberPath);
                isCheckedBinding.Source = item;
                checkListBoxItem.SetBinding(CheckListBoxItem.IsSelectedProperty, isCheckedBinding);
            }
            base.PrepareContainerForItemOverride(element, item);
            _surpressSelectionChanged = false;
        }

        #endregion //Base Class Overrides

        #region Events

        public static readonly RoutedEvent SelectedEvent = EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(CheckListBox));
        public static readonly RoutedEvent UnselectedEvent = EventManager.RegisterRoutedEvent("Unselected", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(CheckListBox));
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(CheckListBoxSelectionChangedEventHandler), typeof(CheckListBox));
        public event CheckListBoxSelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        #endregion //Events

        void CheckListBox_Selected(object sender, RoutedEventArgs e)
        {
            SetSelectedItem(e.OriginalSource);
            SelectedItems.Add(SelectedItem);
        }

        void CheckListBox_Unselected(object sender, RoutedEventArgs e)
        {
            SetSelectedItem(e.OriginalSource);
            SelectedItems.Remove(SelectedItem);
        }

        private void SetSelectedItem(object source)
        {
            if (_surpressSelectionChanged)
                return;

            var selectedCheckListBoxItem = source as FrameworkElement;
            if (selectedCheckListBoxItem != null)
                SelectedItem = selectedCheckListBoxItem.DataContext;
        }

        private void OnSelectionChanged()
        {
            if (_surpressSelectionChanged)
                return;

            RaiseEvent(new CheckListBoxSelectionChangedEventArgs(CheckListBox.SelectionChangedEvent, this, SelectedItem));

            if (Command != null)
                Command.Execute(SelectedItem);
        }
    }
}
