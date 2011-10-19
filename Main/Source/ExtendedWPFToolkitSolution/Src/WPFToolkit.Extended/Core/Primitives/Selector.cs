using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Microsoft.Windows.Controls.Primitives
{
    public class Selector : ItemsControl //should probably make this control an ICommandSource
    {
        #region Members

        private bool _ignoreSetSelectedValue;
        private bool _surpressSelectionChanged;
        private bool _surpressSelectedValueChanged;

        #endregion //Members

        #region Constructors

        public Selector()
        {
            SelectedItems = new ObservableCollection<object>();
            AddHandler(Selector.SelectedEvent, new RoutedEventHandler(Selector_ItemSelected));
            AddHandler(Selector.UnSelectedEvent, new RoutedEventHandler(Selector_ItemUnselected));
        }

        #endregion //Constructors

        #region Properties

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(Selector), new PropertyMetadata((ICommand)null));
        [TypeConverter(typeof(CommandConverter))]
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty DelimiterProperty = DependencyProperty.Register("Delimiter", typeof(string), typeof(Selector), new UIPropertyMetadata(","));
        public string Delimiter
        {
            get { return (string)GetValue(DelimiterProperty); }
            set { SetValue(DelimiterProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(Selector), new UIPropertyMetadata(null));
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        //Since you cannot data bind to ReadOnly DependencyProperty, I am leaving this a public get/set DP.  This will allow you to data bind to the SelectedItems from a ViewModel, but it is
        //intended to be ReadOnly.  So you MUST set the binding Mode=OneWayToSource.  Otherwise it will not behave as expected.
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", typeof(IList), typeof(Selector), new UIPropertyMetadata(null));
        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty SelectedMemberPathProperty = DependencyProperty.Register("SelectedMemberPath", typeof(string), typeof(Selector), new UIPropertyMetadata(null));
        public string SelectedMemberPath
        {
            get { return (string)GetValue(SelectedMemberPathProperty); }
            set { SetValue(SelectedMemberPathProperty, value); }
        }

        #region SelectedValue

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(string), typeof(Selector), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));
        public string SelectedValue
        {
            get { return (string)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        private static void OnSelectedValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Selector selector = o as Selector;
            if (selector != null)
                selector.OnSelectedValueChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnSelectedValueChanged(string oldValue, string newValue)
        {
            if (_surpressSelectedValueChanged)
                return;

            UpdateSelectedItemsFromSelectedValue();
        }

        #endregion //SelectedValue

        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register("ValueMemberPath", typeof(string), typeof(Selector), new UIPropertyMetadata(null));
        public string ValueMemberPath
        {
            get { return (string)GetValue(ValueMemberPathProperty); }
            set { SetValue(ValueMemberPathProperty, value); }
        }

        #endregion //Properties

        #region Base Class Overrides

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is SelectorItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new SelectorItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            _surpressSelectionChanged = true;
            var selectorItem = element as FrameworkElement;

            //first try resolving SelectorItem.IsSelected by data binding to the SelectedMemeberPath property
            if (!String.IsNullOrEmpty(SelectedMemberPath))
            {
                Binding selectedBinding = new Binding(SelectedMemberPath)
                {
                    Mode = BindingMode.TwoWay,
                    Source = item
                };
                selectorItem.SetBinding(SelectorItem.IsSelectedProperty, selectedBinding);
            }

            //now let's search the SelectedItems for the current item.  If it's there then mark it as selected
            if (SelectedItems != null)
            {
                foreach (object selectedItem in SelectedItems)
                {
                    //a match was found so select it and get the hell out of here
                    if (item.Equals(selectedItem))
                    {
                        selectorItem.SetValue(SelectorItem.IsSelectedProperty, true);
                        break;
                    }
                }
            }

            base.PrepareContainerForItemOverride(element, item);
            _surpressSelectionChanged = false;
        }

        #endregion //Base Class Overrides

        #region Events

        public static readonly RoutedEvent SelectedEvent = EventManager.RegisterRoutedEvent("SelectedEvent", RoutingStrategy.Bubble, typeof(SelectedItemChangedEventHandler), typeof(Selector));
        public static readonly RoutedEvent UnSelectedEvent = EventManager.RegisterRoutedEvent("UnSelectedEvent", RoutingStrategy.Bubble, typeof(SelectedItemChangedEventHandler), typeof(Selector));

        public static readonly RoutedEvent SelectedItemChangedEvent = EventManager.RegisterRoutedEvent("SelectedItemChanged", RoutingStrategy.Bubble, typeof(SelectedItemChangedEventHandler), typeof(Selector));
        public event SelectedItemChangedEventHandler SelectedItemChanged
        {
            add { AddHandler(SelectedItemChangedEvent, value); }
            remove { RemoveHandler(SelectedItemChangedEvent, value); }
        }

        #endregion //Events

        #region Event Handlers

        protected virtual void Selector_ItemSelected(object sender, RoutedEventArgs e)
        {
            OnItemSelected(e.OriginalSource, false);
        }

        protected virtual void Selector_ItemUnselected(object sender, RoutedEventArgs e)
        {
            OnItemSelected(e.OriginalSource, true);
        }

        #endregion //Event Handlers

        #region Methods

        protected object GetItemValue(object item)
        {
            if (!String.IsNullOrEmpty(ValueMemberPath))
            {
                var property = item.GetType().GetProperty(ValueMemberPath);
                if (property != null)
                    return property.GetValue(item, null);
            }

            return item;
        }

        protected static object GetDataContextItem(object item)
        {
            var element = item as FrameworkElement;

            if (element != null)
                return element.DataContext;
            else
                return null;
        }

        protected string GetDelimitedValue(object value)
        {
            return String.Format("{0}{1}", value, Delimiter);
        }

        protected virtual void OnItemSelected(object source, bool remove)
        {
            var item = GetDataContextItem(source);
            Update(item, remove);
            RaiseSelectedItemChangedEvent(item, !remove); //inverse the remove paramter to correctly reflect the IsSelected state
        }

        protected virtual void RaiseSelectedItemChangedEvent(object item, bool isSelected)
        {
            if (_surpressSelectionChanged)
                return;

            RaiseEvent(new SelectedItemChangedEventArgs(Selector.SelectedItemChangedEvent, this, item, isSelected));

            if (Command != null)
                Command.Execute(item);
        }

        protected virtual void Update(object item, bool remove)
        {
            UpdateSelectedItem(item);
            UpdateSelectedItems(item, remove);
            UpdateSelectedValue();
        }

        private void UpdateSelectedItem(object item)
        {
            SelectedItem = item;
        }

        private void UpdateSelectedItems(object item, bool remove)
        {
            if (SelectedItems == null)
                SelectedItems = new ObservableCollection<object>();

            if (remove)
            {
                if (SelectedItems.Contains(item))
                    SelectedItems.Remove(item);
            }
            else
            {
                if (!SelectedItems.Contains(item))
                    SelectedItems.Add(item);
            }
        }

        private void UpdateSelectedValue()
        {
            //get out of here if we don't want to set the SelectedValue
            if (_ignoreSetSelectedValue)
                return;

            _surpressSelectedValueChanged = true;

#if VS2008
            string newValue = String.Join(Delimiter, SelectedItems.Cast<object>().Select(x => GetItemValue(x).ToString()).ToArray());
#else
            string newValue = String.Join(Delimiter, SelectedItems.Cast<object>().Select(x => GetItemValue(x)));
#endif
            if (String.IsNullOrEmpty(SelectedValue) || !SelectedValue.Equals(newValue))
                SelectedValue = newValue;

            _surpressSelectedValueChanged = false;
        }

        private void UpdateSelectedItemsFromSelectedValue()
        {
            _surpressSelectionChanged = true;

            //first we have to unselect everything
            ClearSelectedItems();

            if (!String.IsNullOrEmpty(SelectedValue))
            {
                string[] values = SelectedValue.Split(new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string value in values)
                {
                    var item = ResolveItemByValue(value);

                    if (item != null)
                    {
                        SelectedItems.Add(item);

                        //now try to select it in the list
                        var selectorItem = ItemContainerGenerator.ContainerFromItem(item) as SelectorItem;
                        if (selectorItem != null)
                        {
                            if (!selectorItem.IsSelected)
                                selectorItem.IsSelected = true;
                        }
                    }
                }
            }

            _surpressSelectionChanged = false;
        }

        private void ClearSelectedItems()
        {
            if (SelectedItems != null)
                SelectedItems.Clear();
            else
                SelectedItems = new ObservableCollection<object>();

            UnselectAllInternal();
        }

        private void UnselectAllInternal()
        {
            _ignoreSetSelectedValue = true;

            if (ItemsSource != null)
            {
                foreach (object item in ItemsSource)
                {
                    var selectorItem = ItemContainerGenerator.ContainerFromItem(item) as SelectorItem;
                    if (selectorItem != null)
                    {
                        if (selectorItem.IsSelected)
                            selectorItem.IsSelected = false;
                    }
                }
            }

            _ignoreSetSelectedValue = false;
        }

        protected object ResolveItemByValue(string value)
        {
            if (!String.IsNullOrEmpty(ValueMemberPath))
            {
                if (ItemsSource != null)
                {
                    foreach (object item in ItemsSource)
                    {
                        var property = item.GetType().GetProperty(ValueMemberPath);
                        if (property != null)
                        {
                            var propertyValue = property.GetValue(item, null);
                            if (value.Equals(propertyValue.ToString(), StringComparison.InvariantCultureIgnoreCase))
                                return item;
                        }
                    }
                }
            }

            return value;
        }

        #endregion //Methods
    }

    public delegate void SelectedItemChangedEventHandler(object sender, SelectedItemChangedEventArgs e);
    public class SelectedItemChangedEventArgs : RoutedEventArgs
    {
        public bool IsSelected { get; private set; }
        public object Item { get; private set; }

        public SelectedItemChangedEventArgs(RoutedEvent routedEvent, object source, object item, bool isSelected)
            : base(routedEvent, source)
        {
            Item = item;
            IsSelected = isSelected;
        }
    }
}