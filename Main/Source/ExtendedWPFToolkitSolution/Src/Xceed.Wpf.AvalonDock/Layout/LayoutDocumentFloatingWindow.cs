/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Diagnostics;

namespace Xceed.Wpf.AvalonDock.Layout
{
    [ContentProperty("RootDocument")]
    [Serializable]
    public class LayoutDocumentFloatingWindow : LayoutFloatingWindow
    {
        public LayoutDocumentFloatingWindow()
        {

        }

        #region RootDocument

        private LayoutDocument _rootDocument = null;
        public LayoutDocument RootDocument
        {
            get { return _rootDocument; }
            set
            {
                if (_rootDocument != value)
                {
                    RaisePropertyChanging("RootDocument");
                    _rootDocument = value;
                    if (_rootDocument != null)
                        _rootDocument.Parent = this;
                    RaisePropertyChanged("RootDocument");

                    if (RootDocumentChanged != null)
                        RootDocumentChanged(this, EventArgs.Empty);
                }
            }
        }


        public event EventHandler RootDocumentChanged;

        #endregion

        public override IEnumerable<ILayoutElement> Children
        {
            get
            {
                if (RootDocument == null)
                    yield break;

                yield return RootDocument;
            }
        }

        public override void RemoveChild(ILayoutElement element)
        {
            Debug.Assert(element == RootDocument && element != null);
            RootDocument = null;
        }

        public override void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
        {
            Debug.Assert(oldElement == RootDocument && oldElement != null);
            RootDocument = newElement as LayoutDocument;
        }

        public override int ChildrenCount
        {
            get { return RootDocument != null ? 1 : 0; }
        }

        public override bool IsValid
        {
            get { return RootDocument != null; }
        }


#if TRACE
        public override void ConsoleDump(int tab)
        {
          System.Diagnostics.Trace.Write( new string( ' ', tab * 4 ) );
          System.Diagnostics.Trace.WriteLine( "FloatingDocumentWindow()" );

          RootDocument.ConsoleDump(tab + 1);
        }
#endif
    }

}
