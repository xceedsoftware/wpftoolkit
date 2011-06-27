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

namespace Microsoft.Windows.Controls
{
    public class CheckListBoxItem : ContentControl
    {
        static CheckListBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckListBoxItem), new FrameworkPropertyMetadata(typeof(CheckListBoxItem)));
        }

        public CheckListBoxItem()
        {
            AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(CheckListBoxItem_MouseDown));
        }

        #region Properties

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(CheckListBoxItem), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedChanged));
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        private static void OnIsSelectedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CheckListBoxItem checkListBoxItem = o as CheckListBoxItem;
            if (checkListBoxItem != null)
                checkListBoxItem.OnIsSelectedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsSelectedChanged(bool oldValue, bool newValue)
        {
            if (newValue)
                RaiseSelectionChangedEvent(new RoutedEventArgs(CheckListBox.SelectedEvent, this));
            else
                RaiseSelectionChangedEvent(new RoutedEventArgs(CheckListBox.UnselectedEvent, this));
        }

        #endregion //Properties

        #region Events

        public static readonly RoutedEvent SelectedEvent = CheckListBox.SelectedEvent.AddOwner(typeof(CheckListBoxItem));
        public static readonly RoutedEvent UnselectedEvent = CheckListBox.UnselectedEvent.AddOwner(typeof(CheckListBoxItem));

        #endregion

        #region Event Hanlders

        void CheckListBoxItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsSelected = !IsSelected;
        }

        #endregion //Event Hanlders

        #region Methods

        private void RaiseSelectionChangedEvent(RoutedEventArgs e)
        {
            base.RaiseEvent(e);
        }

        #endregion //Methods
    }
}
