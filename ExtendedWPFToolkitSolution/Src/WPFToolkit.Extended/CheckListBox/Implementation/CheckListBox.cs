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
        private bool _surpressSelectionChanged;

        static CheckListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckListBox), new FrameworkPropertyMetadata(typeof(CheckListBox)));
        }

        public CheckListBox()
        {

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

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (!_surpressSelectionChanged)
                base.OnSelectionChanged(e);
        }

        #endregion //Base Class Overrides
    }
}
