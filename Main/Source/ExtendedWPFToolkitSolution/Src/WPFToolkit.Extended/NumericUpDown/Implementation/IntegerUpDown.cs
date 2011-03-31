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
using Microsoft.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    public class IntegerUpDown : UpDownBase<int?>
    {
        #region Properties

        #region DefaultValue

        //can possibly be in base class
        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(int), typeof(IntegerUpDown), new UIPropertyMetadata(default(int)));
        public int DefaultValue
        {
            get { return (int)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        #endregion //DefaultValue

        #region Increment

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(int), typeof(IntegerUpDown), new PropertyMetadata(1));
        public int Increment
        {
            get { return (int)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        #endregion

        #endregion //Properties

        #region Constructors

        static IntegerUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IntegerUpDown), new FrameworkPropertyMetadata(typeof(IntegerUpDown)));
        }

        #endregion //Constructors

        #region Base Class Overrides

        protected override void OnIncrement()
        {
            if (Value.HasValue)
                Value += Increment;
            else
                Value = DefaultValue;
        }

        protected override void OnDecrement()
        {
            if (Value.HasValue)
                Value -= Increment;
            else
                Value = DefaultValue;
        }

        protected override int? ConvertTextToValue(string text)
        {
            int? result = null;

            if (String.IsNullOrEmpty(text))
                return result;

            try
            {
                result = Int16.Parse(text, System.Globalization.NumberStyles.Any);
            }
            catch
            {
                Text = ConvertValueToText(Value);
                return Value;
            }

            return result;
        }

        protected override string ConvertValueToText(object value)
        {
            if (value == null)
                return string.Empty;

            return value.ToString();
        }

        #endregion //Base Class Overrides
    }
}
