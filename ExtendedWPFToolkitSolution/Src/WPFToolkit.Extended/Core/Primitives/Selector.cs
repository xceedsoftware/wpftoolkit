using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(CheckListBox), new PropertyMetadata((ICommand)null));
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

        //Make Read Only
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

            //if we are controling the selections from the SelectedValue we can only update the IsSelected property of the items if the containers have been generated
            if (ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
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
            bool isSelected = false;
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
            else
            {
                var value = item;

                //let's check if we can find a value on the item using the SelectedValuePath property
                if (!String.IsNullOrEmpty(ValueMemberPath))
                {
                    var property = item.GetType().GetProperty(ValueMemberPath);
                    if (property != null)
                    {
                        value = property.GetValue(item, null);
                    }
                }

                //now check to see if the SelectedValue string contains our value.  If it does then set Selector.IsSelected to true
                if (!String.IsNullOrEmpty(SelectedValue) && SelectedValue.Contains(GetDelimitedValue(value)))
                {
                    isSelected = true;
                }
                else if (SelectedItems != null)
                {
                    //if we get here we could find the value in the SelectedValue property, so lets search the SelectedItems for a match
                    foreach (object selectedItem in SelectedItems)
                    {
                        //a match was found so select it and get the hell out of here
                        if (value.Equals(GetItemValue(selectedItem)))
                        {
                            isSelected = true;
                            break;
                        }
                    }
                }

                selectorItem.SetValue(SelectorItem.IsSelectedProperty, isSelected);
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
                Command.Execute(SelectedItem);
        }

        protected virtual void Update(object item, bool remove)
        {
            UpdateSelectedItem(item);
            UpdateSelectedItems(item, remove);
            UpdateSelectedValue(item, remove);
        }

        private void UpdateSelectedItem(object item)
        {
            SelectedItem = item;
        }

        private void UpdateSelectedItems(object item, bool remove)
        {
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

        private void UpdateSelectedValue(object item, bool remove)
        {
            //make sure we have a selected value, or we will get an exception
            if (SelectedValue == null)
                UpdateSelectedValue(String.Empty);

            var value = GetItemValue(item);
            var resolvedValue = GetDelimitedValue(value);
            string updateValue = SelectedValue;

            if (remove)
            {
                if (SelectedValue.Contains(resolvedValue))
                    updateValue = SelectedValue.Replace(resolvedValue, "");
            }
            else
            {
                if (!SelectedValue.Contains(resolvedValue))
                    updateValue = SelectedValue + resolvedValue;
            }

            //if the SelectedValue is the same as the updated value then just ignore it
            if (!SelectedValue.Equals(updateValue))
                UpdateSelectedValue(updateValue);
        }

        private void UpdateSelectedValue(string value)
        {
            //get out of here if we don't want to set the SelectedValue
            if (_ignoreSetSelectedValue)
                return;

            _surpressSelectedValueChanged = true;

            SelectedValue = value;

            _surpressSelectedValueChanged = false;
        }

        private void UpdateSelectedItemsFromSelectedValue()
        {
            _surpressSelectionChanged = true;

            //first we have to unselect everything
            UnselectAll();

            if (!String.IsNullOrEmpty(SelectedValue))
            {
                string[] values = SelectedValue.Split(new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string value in values)
                {
                    var item = ResolveItemByValue(value);
                    var selectorItem = ItemContainerGenerator.ContainerFromItem(item) as SelectorItem;
                    if (selectorItem != null)
                    {
                        if (!selectorItem.IsSelected)
                            selectorItem.IsSelected = true;
                    }
                }
            }

            _surpressSelectionChanged = false;
        }

        private void UnselectAll()
        {
            _ignoreSetSelectedValue = true;

            foreach (object item in ItemsSource)
            {
                var selectorItem = ItemContainerGenerator.ContainerFromItem(item) as SelectorItem;
                if (selectorItem != null)
                {
                    if (selectorItem.IsSelected)
                        selectorItem.IsSelected = false;
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