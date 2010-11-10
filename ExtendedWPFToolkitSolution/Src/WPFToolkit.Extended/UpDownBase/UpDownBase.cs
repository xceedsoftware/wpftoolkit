using System;
using System.Windows.Controls;
using Microsoft.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Microsoft.Windows.Controls
{
    public abstract class UpDownBase : InputBase
    {
        #region Members

        /// <summary>
        /// Name constant for Text template part.
        /// </summary>
        internal const string ElementTextName = "TextBox";

        /// <summary>
        /// Name constant for Spinner template part.
        /// </summary>
        internal const string ElementSpinnerName = "Spinner";

        #endregion //Members

        #region Properties

        protected TextBox TextBox { get; private set; }

        private Spinner _spinner;
        internal Spinner Spinner
        {
            get { return _spinner; }
            private set
            {
                _spinner = value;
                _spinner.Spin += OnSpinnerSpin;
            }
        }

        #endregion //Properties

        #region Constructors

        internal UpDownBase() { }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TextBox = GetTemplateChild(ElementTextName) as TextBox;
            Spinner = GetTemplateChild(ElementSpinnerName) as Spinner;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    {
                        DoIncrement();
                        e.Handled = true;
                        break;
                    }
                case Key.Down:
                    {
                        DoDecrement();
                        e.Handled = true;
                        break;
                    }
                case Key.Enter:
                    {
                        SyncTextAndValueProperties(UpDownBase.TextProperty, TextBox.Text);
                        break;
                    }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!e.Handled)
            {
                if (e.Delta < 0)
                {
                    DoDecrement();
                }
                else if (0 < e.Delta)
                {
                    DoIncrement();
                }

                e.Handled = true;
            }
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        private void OnSpinnerSpin(object sender, SpinEventArgs e)
        {
            OnSpin(e);
        }

        #endregion //Event Handlers

        #region Methods

        protected virtual void OnSpin(SpinEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (e.Direction == SpinDirection.Increase)
                DoIncrement();
            else
                DoDecrement();
        }

        /// <summary>
        /// Performs an increment if conditions allow it.
        /// </summary>
        private void DoDecrement()
        {
            if (Spinner == null || (Spinner.ValidSpinDirection & ValidSpinDirections.Decrease) == ValidSpinDirections.Decrease)
            {
                OnDecrement();
            }
        }

        /// <summary>
        /// Performs a decrement if conditions allow it.
        /// </summary>
        private void DoIncrement()
        {
            if (Spinner == null || (Spinner.ValidSpinDirection & ValidSpinDirections.Increase) == ValidSpinDirections.Increase)
            {
                OnIncrement();
            }
        }

        #region Abstract

        /// <summary>
        /// Called by OnSpin when the spin direction is SpinDirection.Increase.
        /// </summary>
        protected abstract void OnIncrement();

        /// <summary>
        /// Called by OnSpin when the spin direction is SpinDirection.Descrease.
        /// </summary>
        protected abstract void OnDecrement();

        #endregion //Abstract

        #endregion //Methods
    }
}
