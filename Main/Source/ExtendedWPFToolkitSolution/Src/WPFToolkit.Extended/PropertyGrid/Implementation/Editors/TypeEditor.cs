using System;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
    public abstract class TypeEditor : ITypeEditor
    {
        #region Properties

        protected FrameworkElement Editor { get; set; }
        protected DependencyProperty ValueProperty { get; set; }

        #endregion //Properties

        #region Constructors

        public TypeEditor()
        {
            Initialize();
        }

        #endregion //Constructors

        #region ITypeEditor Members

        public virtual void Attach(PropertyItem propertyItem)
        {
            ResolveBinding(propertyItem);
        }

        public virtual FrameworkElement ResolveEditor()
        {
            return Editor;
        }

        #endregion //ITypeEditor Members

        #region Methods

        protected abstract void Initialize();

        protected virtual void ResolveBinding(PropertyItem propertyItem)
        {
            var _binding = new Binding("Value");
            _binding.Source = propertyItem;
            _binding.ValidatesOnExceptions = true;
            _binding.ValidatesOnDataErrors = true;
            _binding.Mode = propertyItem.IsWriteable ? BindingMode.TwoWay : BindingMode.OneWay;
            _binding.Converter = CreateConverter();
            BindingOperations.SetBinding(Editor, ValueProperty, _binding);
        }

        protected virtual IValueConverter CreateConverter()
        {
            return null;
        }

        #endregion //Methods
    }
}
