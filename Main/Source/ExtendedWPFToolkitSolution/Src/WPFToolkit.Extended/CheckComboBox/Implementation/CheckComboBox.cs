using System;
using System.Windows;
using Microsoft.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    public class CheckComboBox : Selector
    {
        private bool _surpressTextUpdateFromSelectedValueChanged;

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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override void OnSelectedValueChanged(string oldValue, string newValue)
        {
            base.OnSelectedValueChanged(oldValue, newValue);

            if (!_surpressTextUpdateFromSelectedValueChanged)
                UpdateTextFromSelectedValue();
        }

        protected override void OnSelectedItemsCollectionChanged(object item, bool remove)
        {
            _surpressTextUpdateFromSelectedValueChanged = true;
            base.OnSelectedItemsCollectionChanged(item, remove);
            UpdateDisplayText(item, remove);
            _surpressTextUpdateFromSelectedValueChanged = false;
        }

        protected override void Update(object item, bool remove)
        {
            _surpressTextUpdateFromSelectedValueChanged = true;
            base.Update(item, remove);
            UpdateDisplayText(item, remove);
            _surpressTextUpdateFromSelectedValueChanged = false;
        }

        #endregion //Base Class Overrides

        #region Methods

        private void UpdateDisplayText(object item, bool remove)
        {
            if (Text == null)
                Text = String.Empty;

            var displayText = GetItemDisplayValue(item);
            var resolvedDisplayText = GetDelimitedValue(displayText);
            string updatedText = Text;

            if (remove)
            {
                if (Text.Contains(resolvedDisplayText))
                    updatedText = Text.Replace(resolvedDisplayText, "");
            }
            else
            {
                if (!Text.Contains(resolvedDisplayText))
                    updatedText = Text + resolvedDisplayText;
            }

            UpdateText(updatedText);
        }

        private void UpdateText(string text)
        {
            if (String.IsNullOrEmpty(Text))
                Text = string.Empty;

            if (!Text.Equals(text))
                Text = text;
        }

        private void UpdateTextFromSelectedValue()
        {
            if (!String.IsNullOrEmpty(SelectedValue))
            {
                string[] values = SelectedValue.Split(new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string value in values)
                {
                    var item = ResolveItemByValue(value);
                    UpdateDisplayText(item, false);
                }
            }
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
