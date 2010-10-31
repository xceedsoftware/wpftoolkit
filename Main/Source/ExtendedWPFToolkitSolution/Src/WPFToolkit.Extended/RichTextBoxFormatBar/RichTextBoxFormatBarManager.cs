using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Windows.Controls.Core;
using System.Windows.Input;

namespace Microsoft.Windows.Controls
{
    public class RichTextBoxFormatBarManager : DependencyObject
    {
        #region Members

        private global::System.Windows.Controls.RichTextBox _richTextBox;
        private UIElementAdorner<Control> _adorner;
        private IRichTextBoxFormatBar _toolbar;

        #endregion //Members

        #region Properties

        #region FormatBar

        public static readonly DependencyProperty FormatBarProperty = DependencyProperty.RegisterAttached("FormatBar", typeof(IRichTextBoxFormatBar), typeof(RichTextBox), new PropertyMetadata(null, OnFormatBarPropertyChanged));
        public static void SetFormatBar(UIElement element, IRichTextBoxFormatBar value)
        {
            element.SetValue(FormatBarProperty, value);
        }
        public static IRichTextBoxFormatBar GetFormatBar(UIElement element)
        {
            return (IRichTextBoxFormatBar)element.GetValue(FormatBarProperty);
        }

        private static void OnFormatBarPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            global::System.Windows.Controls.RichTextBox rtb = d as global::System.Windows.Controls.RichTextBox;
            if (rtb == null)
                throw new Exception("A FormatBar can only be applied to a RichTextBox.");

            RichTextBoxFormatBarManager manager = new RichTextBoxFormatBarManager();
            manager.AttachFormatBarToRichtextBox(rtb, e.NewValue as IRichTextBoxFormatBar);
        }

        #endregion //FormatBar

        bool AdornerIsVisible
        {
            get { return _adorner.Visibility == Visibility.Visible; }
        }

        #endregion //Properties

        #region Event Handlers

        void RichTextBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextRange selectedText = new TextRange(_richTextBox.Selection.Start, _richTextBox.Selection.End);
            if (selectedText.Text.Length > 0 && !String.IsNullOrWhiteSpace(selectedText.Text))
            {
                ShowAdorner();
            }
            else
            {
                HideAdorner();
            }
        }

        void RichTextBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //if the mouse moves outside the richtextbox bounds hide the adorner
            //though this deosn't always work, especially if the user moves the mouse very quickly.
            //need to find a better solution, but this will work for now.
            Point p = e.GetPosition(_richTextBox);
            if (p.X <= 5.0 || p.X >= _richTextBox.ActualWidth - 5 || p.Y <= 3.0 || p.Y >= _richTextBox.ActualHeight - 3)
                HideAdorner();
        }

        #endregion //Event Handlers

        #region Methods

        /// <summary>
        /// Attaches a FormatBar to a RichtextBox
        /// </summary>
        /// <param name="richTextBox">The RichtextBox to attach to.</param>
        /// <param name="formatBar">The Formatbar to attach.</param>
        private void AttachFormatBarToRichtextBox(global::System.Windows.Controls.RichTextBox richTextBox, IRichTextBoxFormatBar formatBar)
        {
            _richTextBox = richTextBox;
            _richTextBox.PreviewMouseMove += RichTextBox_PreviewMouseMove;
            _richTextBox.PreviewMouseLeftButtonUp += RichTextBox_PreviewMouseLeftButtonUp;

            _adorner = new UIElementAdorner<Control>(_richTextBox);

            formatBar.RichTextBox = _richTextBox;
            _toolbar = formatBar;            
        }

        /// <summary>
        /// Shows the FormatBar
        /// </summary>
        void ShowAdorner()
        {
            VerifyAdornerLayer();

            Control adorningEditor = _toolbar as Control;
            _adorner.Child = adorningEditor;
            _adorner.Visibility = Visibility.Visible;

            PositionFormatBar(adorningEditor);
        }

        /// <summary>
        /// Positions the FormatBar so that is does not go outside the bounds of the RichTextBox or covers the selected text
        /// </summary>
        /// <param name="adorningEditor"></param>
        private void PositionFormatBar(Control adorningEditor)
        {
            Rect wordBoundary = _richTextBox.Selection.End.GetPositionAtOffset(0, LogicalDirection.Backward).GetCharacterRect(LogicalDirection.Backward);

            double left = wordBoundary.X;
            double top = (wordBoundary.Y + wordBoundary.Height) - adorningEditor.ActualHeight;

            //top boundary
            if (top < 0)
            {
                top = wordBoundary.Y + wordBoundary.Height;
            }

            //right boundary
            if (left + adorningEditor.ActualWidth > _richTextBox.ActualWidth - 20)
            {
                left = left - (adorningEditor.ActualWidth - (_richTextBox.ActualWidth - left));
                top = wordBoundary.Y + wordBoundary.Height + 5;
            }

            //bottom boundary
            if (top + adorningEditor.ActualHeight > _richTextBox.ActualHeight - 20)
            {
                top = wordBoundary.Y - (adorningEditor.ActualHeight + wordBoundary.Height);
            }

            _adorner.SetOffsets(left, top);
        }

        /// <summary>
        /// Ensures that the IRichTextFormatBar is in the adorner layer.
        /// </summary>
        /// <returns>True if the IRichTextFormatBar is in the adorner layer, else false.</returns>
        bool VerifyAdornerLayer()
        {
            if (_adorner.Parent != null)
                return true;

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(_richTextBox);
            if (layer == null)
                return false;

            layer.Add(_adorner);
            return true;
        }

        /// <summary>
        /// Hides the IRichTextFormatBar that is in the adornor layer.
        /// </summary>
        void HideAdorner()
        {
            if (AdornerIsVisible)
            {
                _adorner.Visibility = Visibility.Collapsed;
                _adorner.Child = null;
            }
        }

        #endregion //Methods
    }
}
