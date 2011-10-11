using System;
using System.Linq;

namespace Microsoft.Windows.Controls
{
    internal class Token
    {
        public string Delimiter { get; private set; }
        public object Item { get; set; }

        private string _tokenKey;
        public string TokenKey
        {
            get { return _tokenKey; }
            set { _tokenKey = String.Format("{0}{1}", value, Delimiter); }
        }

        public Token(string delimiter)
        {
            Delimiter = delimiter;
        }
    }
}
