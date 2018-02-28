using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using NUnit.Framework;
using System.Windows.Documents;
using System.Xml.Linq;

namespace Xceed.Wpf.Toolkit.Tests
{
    [TestFixture]
    public class HtmlFormatterTests
    {
        public HtmlFormatter CreateInstance() => new HtmlFormatter();

        public FlowDocument CreateSimpleFlowDocument()
        {
            var r = new Run("Hallo Welt! ÄÖÜ äöü ^ \" ß ' & <>");
            var b = new Bold(r);
            var p = new Paragraph(b);
            return  new FlowDocument(p);
        }

        public string CreateSimpleHtml() =>
            "<html><body><p><b>Hallo Welt! &#196;&#214;&#220; &#228;&#246;&#252; ^ &quot; &#223; &#39; &amp; &lt;&gt;</b></p><body></html>";

        [Test]
        public void GetText_ShouldReturnAHtmlText()
        {
            var sut = CreateInstance();
            var doc = CreateSimpleFlowDocument();

            var actual = sut.GetText(doc);

            const string expectedText = "Hallo Welt! &#196;&#214;&#220; &#228;&#246;&#252; ^ &quot; &#223; &#39; &amp; &lt;&gt;";
            const string expectedBoldStyle = "bold";
            const string expectedHtmlTag = "<html>";

            Assert.Multiple(delegate
            {
                Assert.That(actual, Does.Contain(expectedText));
                Assert.That(actual, Does.Contain(expectedBoldStyle));
                Assert.That(actual, Does.Contain(expectedHtmlTag));
            });
        }

        [Test]
        public void SetText_ShouldSetAHtmlTextToAFlowDocument()
        {
            var sut = CreateInstance();
            var doc = new FlowDocument();
            var text = CreateSimpleHtml();

            sut.SetText(doc, text);

            var actual = new XamlFormatter().GetText(doc);

            const string expectedText = "Hallo Welt! ÄÖÜ äöü ^ \" ß ' &amp; &lt;&gt;";
            const string expectedBoldStyle = "Bold";
            const string expectedSection = "Section";

            Assert.Multiple(() =>
            {
                Assert.That(actual, Does.Contain(expectedText));
                Assert.That(actual, Does.Contain(expectedBoldStyle));
                Assert.That(actual, Does.Contain(expectedSection));
            });
        }
    }
}