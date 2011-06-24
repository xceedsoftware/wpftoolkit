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

        #region Properties

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(CheckListBoxItem), new UIPropertyMetadata(false, OnIsSelectedChanged));
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
                OnSelected(new RoutedEventArgs(Selector.SelectedEvent, this));
            else
                OnUnselected(new RoutedEventArgs(Selector.UnselectedEvent, this));
        }

        #endregion //Properties

        #region Events

        public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(CheckListBoxItem));
        public event RoutedEventHandler Selected
        {
            add { base.AddHandler(SelectedEvent, value); }
            remove { base.RemoveHandler(SelectedEvent, value); }
        }

        public static readonly RoutedEvent UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(CheckListBoxItem));
        public event RoutedEventHandler Unselected
        {
            add { base.AddHandler(UnselectedEvent, value); }
            remove { base.RemoveHandler(UnselectedEvent, value); }
        }

        #endregion

        #region Methods

        protected virtual void OnSelected(RoutedEventArgs e)
        {
            this.OnIsSelectedChanged(true, e);
        }

        protected virtual void OnUnselected(RoutedEventArgs e)
        {
            this.OnIsSelectedChanged(false, e);
        }

        private void OnIsSelectedChanged(bool newValue, RoutedEventArgs e)
        {
            base.RaiseEvent(e);
        }

        #endregion //Methods
    }
}
