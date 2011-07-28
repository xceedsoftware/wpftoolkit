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
            CheckedItems = new List<object>();
            AddHandler(CheckListBox.CheckedEvent, new RoutedEventHandler(CheckListBox_Checked));
            AddHandler(CheckListBox.UncheckedEvent, new RoutedEventHandler(CheckListBox_Unchecked));
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

        #region CheckedItem

        public static readonly DependencyProperty CheckedItemProperty = DependencyProperty.Register("CheckedItem", typeof(object), typeof(CheckListBox), new UIPropertyMetadata(null, OnCheckedItemChanged));
        public object CheckedItem
        {
            get { return (object)GetValue(CheckedItemProperty); }
            set { SetValue(CheckedItemProperty, value); }
        }

        private static void OnCheckedItemChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CheckListBox checkListBox = o as CheckListBox;
            if (checkListBox != null)
                checkListBox.OnCheckedItemChanged((object)e.OldValue, (object)e.NewValue);
        }

        protected virtual void OnCheckedItemChanged(object oldValue, object newValue)
        {
            
        }

        #endregion //CheckedItem

        public IList CheckedItems { get; private set; }

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
                isCheckedBinding.Mode = BindingMode.TwoWay;
                isCheckedBinding.Source = item;
                checkListBoxItem.SetBinding(CheckListBoxItem.IsCheckedProperty, isCheckedBinding);
            }
            base.PrepareContainerForItemOverride(element, item);
            _surpressSelectionChanged = false;
        }

        #endregion //Base Class Overrides

        #region Events

        public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent("CheckedEvent", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(CheckListBox));
        public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent("UncheckedEvent", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(CheckListBox));
        public static readonly RoutedEvent CheckedChangedEvent = EventManager.RegisterRoutedEvent("CheckedChanged", RoutingStrategy.Bubble, typeof(CheckListBoxCheckedChangedEventHandler), typeof(CheckListBox));
        public event CheckListBoxCheckedChangedEventHandler CheckedChanged
        {
            add { AddHandler(CheckedChangedEvent, value); }
            remove { RemoveHandler(CheckedChangedEvent, value); }
        }

        #endregion //Events

        void CheckListBox_Checked(object sender, RoutedEventArgs e)
        {
            SetCheckedItem(e.OriginalSource);
            CheckedItems.Add(CheckedItem);
            OnCheckedChanged();
        }

        void CheckListBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCheckedItem(e.OriginalSource);
            CheckedItems.Remove(CheckedItem);
            OnCheckedChanged();
        }

        private void SetCheckedItem(object source)
        {
            if (_surpressSelectionChanged)
                return;

            var selectedCheckListBoxItem = source as FrameworkElement;
            if (selectedCheckListBoxItem != null)
                CheckedItem = selectedCheckListBoxItem.DataContext;
        }

        private void OnCheckedChanged()
        {
            if (_surpressSelectionChanged)
                return;

            RaiseEvent(new CheckListBoxCheckedChangedEventArgs(CheckListBox.CheckedChangedEvent, this, CheckedItem));

            if (Command != null)
                Command.Execute(CheckedItem);
        }
    }
}
