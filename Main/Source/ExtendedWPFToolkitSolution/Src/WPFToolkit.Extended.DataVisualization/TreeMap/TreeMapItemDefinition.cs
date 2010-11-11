// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Represents a class that defines various aspects of TreeMap items.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [ContentProperty("ItemTemplate")]
    public class TreeMapItemDefinition : INotifyPropertyChanged
    {
        /// <summary>
        /// A value representing the DataTemplate to instantiate in 
        /// order to create a representation of each TreeMap item.
        /// </summary>
        private DataTemplate _itemTemplate;

        /// <summary>
        /// Gets or sets a value representing the DataTemplate to instantiate in 
        /// order to create a representation of each TreeMap item.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return _itemTemplate; }
            set
            {
                if (value != _itemTemplate)
                {
                    _itemTemplate = value;
                    NotifyPropertyChanged("ItemTemplate");
                }
            }
        }

        /// <summary>
        /// A value representing a binding which can be used 
        /// to retrieve the value associated with each item, needed to calculate 
        /// relative areas of TreeMap items.        
        /// </summary>
        private Binding _valueBinding;

        /// <summary>
        /// Gets or sets a value representing a binding which can be used 
        /// to retrieve the value associated with each item, needed to calculate 
        /// relative areas of TreeMap items.
        /// </summary>
        public Binding ValueBinding
        {
            get { return _valueBinding; }
            set
            {
                if (value != _valueBinding)
                {
                    _valueBinding = value;
                    NotifyPropertyChanged("ValueBinding");
                }
            }
        }

        /// <summary>
        /// Gets or sets the Value Path used to set ValueBinding for retrieving 
        /// the value associated with each item, needed to calculate relative 
        /// areas of TreeMap items.
        /// </summary>
        public string ValuePath
        {
            get { return (null != ValueBinding) ? ValueBinding.Path.Path : null; }
            set
            {
                if (value != ValuePath)
                {
                    if (null == value)
                    {
                        ValueBinding = null;
                    }
                    else
                    {
                        ValueBinding = new Binding(value);
                    }

                    // PropertyChanged(); thru ValueBinding
                }
            }
        }

        /// <summary>
        /// The binding that indicates where to find the collection
        /// that represents the next level in the data hierarchy.
        /// </summary>
        private Binding _itemsSource;

        /// <summary>
        /// Gets or sets the binding that indicates where to find the collection
        /// that represents the next level in the data hierarchy.
        /// </summary>
        public Binding ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                if (value != _itemsSource)
                {
                    _itemsSource = value;
                    NotifyPropertyChanged("ItemsSource");
                }
            }
        }

        /// <summary>
        /// A property representing the amount of space to leave 
        /// between a parent item and its children.
        /// </summary>
        private Thickness _childItemPadding;

        /// <summary>
        /// Gets or sets a property representing the amount of space to leave 
        /// between a parent item and its children.
        /// </summary>
        public Thickness ChildItemPadding
        {
            get { return _childItemPadding; }
            set
            {
                if (value != _childItemPadding)
                {
                    _childItemPadding = value;
                    NotifyPropertyChanged("ChildItemPadding");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the TreeMapItemDefinition class.
        /// </summary>
        public TreeMapItemDefinition()
        {
            ChildItemPadding = new Thickness(0);
        }

        /// <summary>
        /// PropertyChanged event required by INotifyPropertyChanged.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Updates the TreeMap if one of properties changes.
        /// </summary>
        /// <param name="parameterName">The parameter name.</param>
        protected void NotifyPropertyChanged(string parameterName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(parameterName));
            }
        }
    }
}
