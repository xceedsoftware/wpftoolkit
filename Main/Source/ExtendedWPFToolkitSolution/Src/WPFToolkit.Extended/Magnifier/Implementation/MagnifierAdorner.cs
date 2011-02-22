using System;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Windows.Controls
{
    public class MagnifierAdorner : Adorner
    {
        #region Members

        private Magnifier _magnifier;
        private Point _currentMousePosition;

        #endregion

        #region Constructors

        public MagnifierAdorner(UIElement element, Magnifier magnifier)
            : base(element)
        {
            InputManager.Current.PostProcessInput += Current_PostProcessInput;
            _magnifier = magnifier;            
            UpdateViewBox();
            AddVisualChild(_magnifier);         
        }

        #endregion

        #region Private/Internal methods

        private void Current_PostProcessInput(object sender, ProcessInputEventArgs e)
        {
            Point pt = Mouse.GetPosition(this);

            if (_currentMousePosition == pt)
                return;

            _currentMousePosition = pt;
            UpdateViewBox();
            InvalidateArrange();
        }

        internal void UpdateViewBox()
        {
            var viewBoxLocation = CalculateViewBoxLocation();
            _magnifier.ViewBox = new Rect(viewBoxLocation, _magnifier.ViewBox.Size);
        }

        private Point CalculateViewBoxLocation()
        {
            double offsetX = 0, offsetY = 0;

            Point adorner = Mouse.GetPosition(this);
            Point element = Mouse.GetPosition(AdornedElement);

            offsetX = element.X - adorner.X;
            offsetY = element.Y - adorner.Y;

            double left = _currentMousePosition.X - ((_magnifier.ViewBox.Width / 2) + offsetX);
            double top = _currentMousePosition.Y - ((_magnifier.ViewBox.Height / 2) + offsetY);
            return new Point(left, top);
        }

        #endregion

        #region Overrides

        protected override Visual GetVisualChild(int index)
        {
            return _magnifier;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _magnifier.Measure(constraint);
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = _currentMousePosition.X - (_magnifier.DefaultSize.Width / 2);
            double y = _currentMousePosition.Y - (_magnifier.DefaultSize.Height / 2);
            _magnifier.Arrange(new Rect(x, y, _magnifier.DefaultSize.Width, _magnifier.DefaultSize.Height));
            return base.ArrangeOverride(finalSize);
        }

        #endregion
    }
}
