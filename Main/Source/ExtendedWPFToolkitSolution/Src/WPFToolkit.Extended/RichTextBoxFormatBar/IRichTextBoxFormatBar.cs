using System;

namespace Microsoft.Windows.Controls
{
    public interface IRichTextBoxFormatBar
    {
        /// <summary>
        /// Represents the RichTextBox that will be the target for all text manipulations in the format bar.
        /// </summary>
        global::System.Windows.Controls.RichTextBox Target { get; set; }
    }
}
