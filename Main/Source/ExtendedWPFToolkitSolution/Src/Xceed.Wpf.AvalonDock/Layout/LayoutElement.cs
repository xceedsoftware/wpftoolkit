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
using System.Windows;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Xceed.Wpf.AvalonDock.Layout
{
    [Serializable]
    public abstract class LayoutElement : DependencyObject, ILayoutElement
    {
        internal LayoutElement()
        { }

        #region Parent

        [NonSerialized]
        private ILayoutContainer _parent = null;
        [NonSerialized]
        private ILayoutRoot _root = null;
        [XmlIgnore]
        public ILayoutContainer Parent
        {
            get { return _parent; }
            set
            {
                if (_parent != value)
                {
                    ILayoutContainer oldValue = _parent;
                    ILayoutRoot oldRoot = _root;
                    RaisePropertyChanging("Parent");
                    OnParentChanging(oldValue, value);
                    _parent = value;
                    OnParentChanged(oldValue, value);

                    _root = Root;
                    if (oldRoot != _root)
                        OnRootChanged(oldRoot, _root);

                    RaisePropertyChanged("Parent");

                    var root = Root as LayoutRoot;
                    if (root != null)
                        root.FireLayoutUpdated();
                }
            }
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle execute code before to the Parent property changes.
        /// </summary>
        protected virtual void OnParentChanging(ILayoutContainer oldValue, ILayoutContainer newValue)
        {
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Parent property.
        /// </summary>
        protected virtual void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
        {

        }


        protected virtual void OnRootChanged(ILayoutRoot oldRoot, ILayoutRoot newRoot)
        {
            if (oldRoot != null)
                ((LayoutRoot)oldRoot).OnLayoutElementRemoved(this);
            if (newRoot != null)
                ((LayoutRoot)newRoot).OnLayoutElementAdded(this);        
        }


        #endregion

        [field: NonSerialized]
        [field: XmlIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        [field: NonSerialized]
        [field: XmlIgnore]
        public event PropertyChangingEventHandler PropertyChanging;

        protected virtual void RaisePropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
                PropertyChanging(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
        }

        public ILayoutRoot Root
        {
            get
            {
                var parent = Parent;

                while (parent != null && (!(parent is ILayoutRoot)))
                {
                    parent = parent.Parent;
                }

                return parent as ILayoutRoot;
            }
        }


#if TRACE
        public virtual void ConsoleDump(int tab)
        {
          System.Diagnostics.Trace.Write( new String( ' ', tab * 4 ) );
          System.Diagnostics.Trace.WriteLine( this.ToString() );
        }
#endif
    }
}
