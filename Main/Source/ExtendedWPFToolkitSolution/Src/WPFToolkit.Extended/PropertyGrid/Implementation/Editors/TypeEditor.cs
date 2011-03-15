using System;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public abstract class TypeEditor<T> : ITypeEditor
        where T : FrameworkElement, new()
    {
        #region Properties

        protected T Editor { get; set; }
        protected DependencyProperty ValueProperty { get; set; }

        #endregion //Properties

        #region Constructors

        public TypeEditor()
        {
            Editor = new T();
            SetValueDependencyProperty();
            SetControlProperties();
        }

        #endregion //Constructors

        #region ITypeEditor Members

        public virtual void Attach(PropertyItem propertyItem)
        {
            ResolveValueBinding(propertyItem);
        }

        public virtual FrameworkElement ResolveEditor()
        {
            return Editor;
        }

        #endregion //ITypeEditor Members

        #region Methods

        protected virtual IValueConverter CreateValueConverter()
        {
            return null;
        }

        protected virtual void ResolveValueBinding(PropertyItem propertyItem)
        {
            var _binding = new Binding("Value");
            _binding.Source = propertyItem;
            _binding.ValidatesOnExceptions = true;
            _binding.ValidatesOnDataErrors = true;
            _binding.Mode = propertyItem.IsWriteable ? BindingMode.TwoWay : BindingMode.OneWay;
            _binding.Converter = CreateValueConverter();
            BindingOperations.SetBinding(Editor, ValueProperty, _binding);
        }

        protected virtual void SetControlProperties()
        {
            //TODO: implement in derived class
        }

        protected abstract void SetValueDependencyProperty();


        #endregion //Methods
    }
}
