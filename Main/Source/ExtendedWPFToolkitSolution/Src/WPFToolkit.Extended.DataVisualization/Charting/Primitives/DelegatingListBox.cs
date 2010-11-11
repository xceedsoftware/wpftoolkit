// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization.Charting.Primitives
{
    /// <summary>
    /// Subclasses ListBox to provide an easy way for a consumer of
    /// ListBox to hook into the four standard ListBox *Container*
    /// overrides.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public class DelegatingListBox : ListBox
    {
        /// <summary>
        /// Gets or sets a function to call when the
        /// IsItemItsOwnContainerOverride method executes.
        /// </summary>
        public Func<object, bool> IsItemItsOwnContainer { get; set; }

        /// <summary>
        /// Gets or sets a function to call when the
        /// GetContainerForItem method executes.
        /// </summary>
        public Func<DependencyObject> GetContainerForItem { get; set; }

        /// <summary>
        /// Gets or sets an action to call when the
        /// PrepareContainerForItem method executes.
        /// </summary>
        public Action<DependencyObject, object> PrepareContainerForItem { get; set; }

        /// <summary>
        /// Gets or sets an action to call when the
        /// ClearContainerForItem method executes.
        /// </summary>
        public Action<DependencyObject, object> ClearContainerForItem { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Initializes static members of the DelegatingListBox class.
        /// </summary>
        static DelegatingListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DelegatingListBox), new FrameworkPropertyMetadata(typeof(DelegatingListBox)));
        }
#endif

        /// <summary>
        /// Initializes a new instance of the DelegatingListBox class.
        /// </summary>
        public DelegatingListBox()
        {
#if SILVERLIGHT
            DefaultStyleKey = typeof(DelegatingListBox);
#endif
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the item is (or is eligible to be) its own container; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (null != IsItemItsOwnContainer) ?
                IsItemItsOwnContainer(item) :
                base.IsItemItsOwnContainerOverride(item);
        }

        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>The element that is used to display the given item.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return (null != GetContainerForItem) ?
                GetContainerForItem() :
                base.GetContainerForItemOverride();
        }

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">The element used to display the specified item.</param>
        /// <param name="item">The item to display.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (null != PrepareContainerForItem)
            {
                PrepareContainerForItem(element, item);
            }
        }

        /// <summary>
        /// Undoes the effects of the PrepareContainerForItemOverride method.
        /// </summary>
        /// <param name="element">The container element.</param>
        /// <param name="item">The item to display.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);
            if (null != ClearContainerForItem)
            {
                ClearContainerForItem(element, item);
            }
        }
    }
}
