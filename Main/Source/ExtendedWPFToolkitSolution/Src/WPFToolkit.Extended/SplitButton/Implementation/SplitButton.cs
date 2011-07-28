using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls
{
    public class SplitButton : DropDownButton
    {
        #region Constructors

        static SplitButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Button = GetTemplateChild("PART_ActionButton") as Button;
        }

        #endregion //Base Class Overrides
    }
}
