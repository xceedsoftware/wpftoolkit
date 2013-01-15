using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;

namespace Microsoft.Windows.Controls.Formatting
{
    internal class FormatToolbarManager
    {
        RichTextBox _richTextBox;
        readonly UIElementAdorner<Control> _adorner;
        FormatToolbar toolbar;

        bool AdornerIsVisible
        {
            get { return _adorner.Visibility == Visibility.Visible; }
        }

        public FormatToolbarManager(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
            _adorner = new UIElementAdorner<Control>(richTextBox);
            toolbar = new FormatToolbar(richTextBox);
            AttachToRichtextBox();
        }

        private void AttachToRichtextBox()
        {
            _richTextBox.Selection.Changed += Selection_Changed;
        }

        void Selection_Changed(object sender, EventArgs e)
        {
            TextRange selectedText = new TextRange(_richTextBox.Selection.Start, _richTextBox.Selection.End);
            if (selectedText.Text.Length > 0)
            {
                VerifyAdorner();
            }
            else
            {
                HideAdorner();
            }
        }

        //TODO: refactor
        void VerifyAdorner()
        {
            VerifyAdornerLayer();

            Control adorningEditor = toolbar;
            _adorner.Child = adorningEditor;
            _adorner.Visibility = Visibility.Visible;

            Rect wordBoundary = _richTextBox.Selection.End.GetPositionAtOffset(0, LogicalDirection.Backward).GetCharacterRect(LogicalDirection.Backward);

            double left = wordBoundary.X;
            double top = (wordBoundary.Y + wordBoundary.Height) - toolbar.ActualHeight;

            //top boundary
            if (top < 0)
            {
                top = wordBoundary.Y + wordBoundary.Height;
            }

            //right boundary
            if (left + toolbar.ActualWidth > _richTextBox.ActualWidth - 20)
            {
                left = left - toolbar.ActualWidth;
                top =  wordBoundary.Y + wordBoundary.Height + 5;
            }

            //bottom boundary
            if (top + toolbar.ActualHeight > _richTextBox.ActualHeight - 20)
            {
                top = wordBoundary.Y - (toolbar.ActualHeight + wordBoundary.Height);
            }

            _adorner.SetOffsets(left, top);
        }

        /// <summary>
        /// Ensures that the adorner is in the adorner layer.
        /// </summary>
        /// <returns>True if the adorner is in the adorner layer, else false.</returns>
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

        void HideAdorner()
        {
            if (this.AdornerIsVisible)
            {
                _adorner.Visibility = Visibility.Collapsed;
                _adorner.Child = null;
            }
        }
    }
}
