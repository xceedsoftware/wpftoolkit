using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace Xceed.Wpf.Toolkit.Core.Html
{
    public class HtmlEncodedTextWriter : XmlTextWriter
    {
        public HtmlEncodedTextWriter(TextWriter w) : base(w) { }

        #region Overrides of XmlTextWriter

        /// <inheritdoc />
        public override void WriteString(string text)
        {
            text = WebUtility.HtmlEncode(text);
            WriteRaw(text);
        }

        #endregion
    }
}
