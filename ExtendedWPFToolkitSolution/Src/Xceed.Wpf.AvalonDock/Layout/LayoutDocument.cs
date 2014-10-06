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
using System.Globalization;

namespace Xceed.Wpf.AvalonDock.Layout
{
    [Serializable]
    public class LayoutDocument : LayoutContent
    {
        public bool IsVisible
        {
            get { return true; }
        }

        #region Description

        private string _description = null;
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    RaisePropertyChanged("Description");
                }
            }
        }

        #endregion

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);

            if (!string.IsNullOrWhiteSpace(Description))
                writer.WriteAttributeString("Description", Description);

        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.MoveToAttribute("Description"))
                Description = reader.Value;

            base.ReadXml(reader);
        }

        public override void Close()
        {
          if( ( this.Root != null ) && ( this.Root.Manager != null ) )
          {
            var dockingManager = this.Root.Manager;
            dockingManager._ExecuteCloseCommand( this );
          }
          else
          {
            this.CloseDocument();
          }
        }

        internal bool CloseDocument()
        {
          if( this.TestCanClose() )
          {
            this.CloseInternal();
            return true;
          }

          return false;
        }

#if TRACE
        public override void ConsoleDump(int tab)
        {
          System.Diagnostics.Trace.Write( new string( ' ', tab * 4 ) );
          System.Diagnostics.Trace.WriteLine( "Document()" );
        }
#endif


        protected override void InternalDock()
        {
            var root = Root as LayoutRoot;
            LayoutDocumentPane documentPane = null;
            if (root.LastFocusedDocument != null &&
                root.LastFocusedDocument != this)
            {
                documentPane = root.LastFocusedDocument.Parent as LayoutDocumentPane;
            }

            if (documentPane == null)
            {
                documentPane = root.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
            }


            bool added = false;
            if (root.Manager.LayoutUpdateStrategy != null)
            {
                added = root.Manager.LayoutUpdateStrategy.BeforeInsertDocument(root, this, documentPane);
            }

            if (!added)
            {
                if (documentPane == null)
                    throw new InvalidOperationException("Layout must contains at least one LayoutDocumentPane in order to host documents");

                documentPane.Children.Add(this);
                added = true;
            }

            if (root.Manager.LayoutUpdateStrategy != null)
            {
                root.Manager.LayoutUpdateStrategy.AfterInsertDocument(root, this);
            }


            base.InternalDock();
        }
    }
}
