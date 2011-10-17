using System;
using System.Linq;
using System.Windows;
using Microsoft.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    public class CheckComboBox : Selector
    {
        #region Constructors

        static CheckComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckComboBox), new FrameworkPropertyMetadata(typeof(CheckComboBox)));
        }

        public CheckComboBox()
        {

        }

        #endregion //Constructors

        #region Properties

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CheckComboBox), new UIPropertyMetadata(null));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #region IsDropDownOpen

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(CheckComboBox), new UIPropertyMetadata(false, OnIsDropDownOpenChanged));
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        private static void OnIsDropDownOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CheckComboBox comboBox = o as CheckComboBox;
            if (comboBox != null)
                comboBox.OnIsDropDownOpenChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsDropDownOpenChanged(bool oldValue, bool newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //IsDropDownOpen

        #endregion //Properties

        #region Base Class Overrides

        protected override void OnSelectedValueChanged(string oldValue, string newValue)
        {
            base.OnSelectedValueChanged(oldValue, newValue);
            UpdateText();
        }

        #endregion //Base Class Overrides

        #region Methods

        private void UpdateText()
        {
            string newValue = String.Join(Delimiter, SelectedItems.Cast<object>().Select(x => GetItemDisplayValue(x)));

            if (String.IsNullOrEmpty(Text) || !Text.Equals(newValue))
                Text = newValue;
        }

        protected object GetItemDisplayValue(object item)
        {
            if (!String.IsNullOrEmpty(DisplayMemberPath))
            {
                var property = item.GetType().GetProperty(DisplayMemberPath);
                if (property != null)
                    return property.GetValue(item, null);
            }

            return item;
        }

        #endregion //Methods
    }
}
