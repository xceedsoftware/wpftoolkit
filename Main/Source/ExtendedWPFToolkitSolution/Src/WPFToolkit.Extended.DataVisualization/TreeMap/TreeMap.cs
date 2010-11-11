// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Represents a control which can display hierarchical data as a set of nested rectangles. 
    /// Each item in the hierarchy is laid out in a rectangular area of a size proportional to 
    /// the value associated with the item.
    /// </summary>
    /// <remarks>
    /// You populate a TreeMap by setting its ItemsSource property to the root of the hierarchy 
    /// you would like to display. The ItemDefinition property must be set to an instance of a 
    /// TreeMapItemDefinition with appropriate bindings for Value (identifying the value to be used
    /// when calculating relative item sizes) and ItemsSource (identifying the collection of 
    /// children for each item).
    /// </remarks>
    /// <QualityBand>Preview</QualityBand>
    [TemplatePart(Name = ContainerName, Type = typeof(Canvas))]
    [ContentProperty("ItemsSource")]
    public class TreeMap : Control
    {
        /// <summary>
        /// The name of the Container template part.
        /// </summary>
        private const string ContainerName = "Container";

        #region private object InterpolatorValue
        /// <summary>
        /// Identifies the InterpolatorValue dependency property.
        /// </summary>
        private static readonly DependencyProperty InterpolatorValueProperty =
            DependencyProperty.Register(
                "InterpolatorValue",
                typeof(object),
                typeof(TreeMap),
                null);

        /// <summary>
        /// Gets or sets a generic value used as a temporary storage used as a source for TargetName/TargetProperty binding.
        /// </summary>
        private object InterpolatorValue
        {
            get { return (object)GetValue(InterpolatorValueProperty); }
            set { SetValue(InterpolatorValueProperty, value); }
        }
        #endregion

        /// <summary>
        /// Holds a helper object used to extract values using a property path.
        /// </summary>
        private BindingExtractor _helper;

        /// <summary>
        /// The roots of the pre-calculated parallel tree of TreeMapNodes.
        /// </summary>
        private IEnumerable<TreeMapNode> _nodeRoots;

        /// <summary>
        /// Cached sequence of all TreeMapNodes used by GetTreeMapNodes.
        /// </summary>
        private IEnumerable<TreeMapNode> _getTreeMapNodesCache;

        #region public TreeMapItemDefinitionSelector TreeMapItemDefinitionSelector
        /// <summary>
        /// Gets or sets the selector used to choose the item template dynamically.
        /// </summary>
        public TreeMapItemDefinitionSelector ItemDefinitionSelector
        {
            get { return (TreeMapItemDefinitionSelector)GetValue(ItemDefinitionSelectorProperty); }
            set { SetValue(ItemDefinitionSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemDefinitionSelector dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemDefinitionSelectorProperty = 
            DependencyProperty.Register(
                "ItemDefinitionSelector",
                typeof(TreeMapItemDefinitionSelector),
                typeof(TreeMap),
                new PropertyMetadata(OnItemDefinitionSelectorPropertyChanged));

        /// <summary>
        /// Called when the value of the TreeMapItemDefinitionSelectorProperty property changes.
        /// </summary>
        /// <param name="d">Reference to the TreeMap object.</param>
        /// <param name="e">Event handler arguments.</param>
        private static void OnItemDefinitionSelectorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeMap treeMap = d as TreeMap;
            if (treeMap != null)
            {
                TreeMapItemDefinitionSelector oldValue = e.OldValue as TreeMapItemDefinitionSelector;
                TreeMapItemDefinitionSelector newValue = e.NewValue as TreeMapItemDefinitionSelector;
                treeMap.OnItemDefinitionSelectorPropertyChanged(oldValue, newValue);
            }
        }

        /// <summary>
        /// Called when the value of the ItemDefinitionSelectorProperty property changes.
        /// Triggers a recalculation of the layout.
        /// </summary>
        /// <param name="oldValue">The old selector.</param>
        /// <param name="newValue">The new selector.</param>
        protected virtual void OnItemDefinitionSelectorPropertyChanged(TreeMapItemDefinitionSelector oldValue, TreeMapItemDefinitionSelector newValue)
        {
            RebuildTree();
        }

        #endregion

        #region public TreeMapItemDefinition ItemDefinition
        /// <summary>
        /// Gets or sets a value representing the template used to display each item.
        /// </summary>
        public TreeMapItemDefinition ItemDefinition
        {
            get { return (TreeMapItemDefinition)GetValue(ItemDefinitionProperty); }
            set { SetValue(ItemDefinitionProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemDefinition dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemDefinitionProperty = 
            DependencyProperty.Register(
                "ItemDefinition",
                typeof(TreeMapItemDefinition),
                typeof(TreeMap),
                new PropertyMetadata(OnItemDefinitionPropertyChanged));

        /// <summary>
        /// Called when the value of the ItemDefinitionProperty property changes.
        /// </summary>
        /// <param name="d">Reference to the TreeMap object.</param>
        /// <param name="e">Event handler arguments.</param>
        private static void OnItemDefinitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeMap treeMap = d as TreeMap;
            if (treeMap != null)
            {
                TreeMapItemDefinition oldValue = e.OldValue as TreeMapItemDefinition;
                TreeMapItemDefinition newValue = e.NewValue as TreeMapItemDefinition;

                // Unregister old TreeMapItemDefinition
                if (oldValue != null)
                {
                    oldValue.PropertyChanged -= treeMap.OnItemDefinitionPropertyChanged;
                }

                // Register new TreeMapItemDefinition
                if (newValue != null)
                {
                    newValue.PropertyChanged += treeMap.OnItemDefinitionPropertyChanged;
                }

                treeMap.OnItemDefinitionPropertyChanged(oldValue, newValue);
            }
        }

        /// <summary>
        /// This callback ensures that any change in TreeMapItemDefinition.
        /// </summary>
        /// <param name="sender">Source TreeMapItemDefinition object.</param>
        /// <param name="e">Event handler arguments (parameter name).</param>
        private void OnItemDefinitionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RebuildTree();
        }

        /// <summary>
        /// Called when the value of the ItemDefinitionProperty property changes.
        /// Triggers a recalculation of the layout.
        /// </summary>
        /// <param name="oldValue">The old item definition.</param>
        /// <param name="newValue">The new item definition.</param>
        protected virtual void OnItemDefinitionPropertyChanged(TreeMapItemDefinition oldValue, TreeMapItemDefinition newValue)
        {
            RebuildTree();
        }
        #endregion 

        #region public IEnumerable ItemsSource
        /// <summary>
        /// Gets or sets a value representing the list of hierarchies used to generate
        /// content for the TreeMap.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = 
            DependencyProperty.Register(
                "ItemsSource", 
                typeof(IEnumerable),
                typeof(TreeMap),
                new PropertyMetadata(OnItemsSourcePropertyChanged));

        /// <summary>
        /// Called when the value of the ItemsSourceProperty property changes.
        /// </summary>
        /// <param name="d">Reference to the TreeMap object.</param>
        /// <param name="e">Event handler arguments.</param>
        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeMap treeMap = d as TreeMap;
            if (treeMap != null)
            {
                IEnumerable oldValue = e.OldValue as IEnumerable;
                IEnumerable newValue = e.NewValue as IEnumerable;
                treeMap.OnItemsSourcePropertyChanged(oldValue, newValue);
            }
        }

        /// <summary>
        /// Called when the value of the ItemsSourceProperty property changes.
        /// </summary>
        /// <param name="oldValue">The old ItemsSource collection.</param>
        /// <param name="newValue">The new ItemsSource collection.</param>
        protected virtual void OnItemsSourcePropertyChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            // Remove handler for oldValue.CollectionChanged (if present)
            INotifyCollectionChanged oldValueINotifyCollectionChanged = oldValue as INotifyCollectionChanged;
            if (null != oldValueINotifyCollectionChanged)
            {
                // Detach the WeakEventListener
                if (null != _weakEventListener)
                {
                    _weakEventListener.Detach();
                    _weakEventListener = null;
                }
            }

            // Add handler for newValue.CollectionChanged (if possible)
            INotifyCollectionChanged newValueINotifyCollectionChanged = newValue as INotifyCollectionChanged;
            if (null != newValueINotifyCollectionChanged)
            {
                // Use a WeakEventListener so that the backwards reference doesn't keep this object alive
                _weakEventListener = new WeakEventListener<TreeMap, object, NotifyCollectionChangedEventArgs>(this);
                _weakEventListener.OnEventAction = (instance, source, eventArgs) => instance.ItemsSourceCollectionChanged(source, eventArgs);
                _weakEventListener.OnDetachAction = (weakEventListener) => newValueINotifyCollectionChanged.CollectionChanged -= weakEventListener.OnEvent;
                newValueINotifyCollectionChanged.CollectionChanged += _weakEventListener.OnEvent;
            }

            // Handle property change
            RebuildTree();
        }

        /// <summary>
        /// WeakEventListener used to handle INotifyCollectionChanged events.
        /// </summary>
        private WeakEventListener<TreeMap, object, NotifyCollectionChangedEventArgs> _weakEventListener;

        /// <summary>
        /// Method that handles the ObservableCollection.CollectionChanged event for the ItemsSource property.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void ItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildTree();
        }
        #endregion

        #region private Collection<Interpolator> Interpolators
        /// <summary>
        /// Gets or sets a value representing a collection of interpolators to use in TreeMap.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Setter is public to work around a limitation with the XAML editing tools.")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value", Justification = "Setter is public to work around a limitation with the XAML editing tools.")]
        public Collection<Interpolator> Interpolators
        {
            get { return (Collection<Interpolator>)GetValue(InterpolatorsProperty); }
            set { throw new NotSupportedException(Properties.Resources.TreeMap_Interpolators_SetterNotSupported); }
        }
        
        /// <summary>
        /// Identifies the Interpolators dependency property.
        /// </summary>
        public static readonly DependencyProperty InterpolatorsProperty = 
            DependencyProperty.Register(
                "Interpolators",
                typeof(Collection<Interpolator>),
                typeof(TreeMap),
                new PropertyMetadata(OnInterpolatorsPropertyChanged));

        /// <summary>
        /// Called when the value of the InterpolatorsProperty property changes.
        /// </summary>
        /// <param name="d">Reference to the TreeMap object.</param>
        /// <param name="e">Event handler arguments.</param>
        private static void OnInterpolatorsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeMap treeMap = d as TreeMap;
            if (treeMap != null)
            {
                Collection<Interpolator> oldValue = e.OldValue as Collection<Interpolator>;
                Collection<Interpolator> newValue = e.NewValue as Collection<Interpolator>;
                treeMap.OnInterpolatorsPropertyChanged(oldValue, newValue);
            }
        }

        /// <summary>
        /// Called when the value of the InterpolatorsProperty property changes.
        /// Triggers a recalculation of the layout.
        /// </summary>
        /// <param name="oldValue">The old Interpolators collection.</param>
        /// <param name="newValue">The new Interpolators collection.</param>
        protected virtual void OnInterpolatorsPropertyChanged(Collection<Interpolator> oldValue, Collection<Interpolator> newValue)
        {
            RebuildTree();
        }
        #endregion
        
        #region Template Parts
        /// <summary>
        /// The Container template part is used to hold all the items inside
        /// a TreeMap.
        /// </summary>
        private Canvas _containerElement;

        /// <summary>
        /// Gets the Container template part that is used to hold all the items inside
        /// a TreeMap.
        /// </summary>
        internal Canvas ContainerElement
        {
            get { return _containerElement; }
            private set
            {
                // Detach from the old Container element
                if (_containerElement != null)
                {
                    _containerElement.Children.Clear();
                }

                // Attach to the new Container element
                _containerElement = value;
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the TreeMap class.
        /// </summary>
        public TreeMap()
        {
            _helper = new BindingExtractor();

            DefaultStyleKey = typeof(TreeMap);

            SetValue(InterpolatorsProperty, new ObservableCollection<Interpolator>());
            (Interpolators as ObservableCollection<Interpolator>).CollectionChanged += 
                new NotifyCollectionChangedEventHandler(OnInterpolatorsCollectionChanged);
        }

        /// <summary>
        /// Invoked whenever application code or internal processes call ApplyTemplate. Gets references
        /// to the template parts required by this control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ContainerElement = GetTemplateChild(ContainerName) as Canvas;

            RebuildTree();
        }

        /// <summary>
        /// Constructs a new instance of an element used to display an item in the tree. 
        /// </summary>
        /// <remarks>
        /// By default TreeMap will use the template set in its ItemDefinition property, or the value 
        /// returned from GetTemplateForItemOverride if overridden. Override this method to build a 
        /// custom element.
        /// </remarks>
        /// <param name="data">One of the items in the ItemsSource hierarchy.</param>
        /// <param name="level">The level of the item in the hierarchy.</param>
        /// <returns>A new FrameworkElement which will be added to the TreeMap control. If this
        /// method returns null the TreeMap will create the item using the ItemDefinition property,
        /// or the value returned by TreeMapItemDefinitionSelector if specified.</returns>
        protected virtual FrameworkElement GetContainerForItemOverride(object data, int level)
        {
            return null;
        }

        /// <summary>
        /// Performs the Arrange pass of the layout.
        /// </summary>
        /// <remarks>
        /// We round rectangles to snap to nearest pixels. We do that to avoid 
        /// anti-aliasing which results in better appearance. Moreover to get
        /// correct layout we would need to use UseLayoutRounding=false which
        /// is Silverlight specific. A side effect is that areas for rectangles 
        /// in the visual tree no longer can be used to compare them as dimensions
        /// are not rounded and therefore not precise. 
        /// </remarks>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            // Sets ActualHeight & ActualWidth for the container
            finalSize = base.ArrangeOverride(finalSize);

            if (_nodeRoots != null && ContainerElement != null)
            {
                // Create a temporary pseudo-root node containing all the top-level nodes
                TreeMapNode root = new TreeMapNode()
                {
                    Area = _nodeRoots.Sum(x => x.Area),
                    Children = _nodeRoots,
                    ChildItemPadding = new Thickness(0)
                };

                // Calculate associated rectangles. We use ContainerElement, 
                // not finalSize so all elements that are above it like border 
                // (with padding and border) are taken into account
                IEnumerable<Tuple<Rect, TreeMapNode>> measuredRectangles = ComputeRectangles(
                    root,
                    new Rect(0, 0, ContainerElement.ActualWidth, ContainerElement.ActualHeight));

                // Position everything
                foreach (Tuple<Rect, TreeMapNode> rectangle in measuredRectangles)
                {
                    FrameworkElement element = rectangle.Item2.Element;
                    if (element != null)
                    {
                        double roundedTop = Math.Round(rectangle.Item1.Top);
                        double roundedLeft = Math.Round(rectangle.Item1.Left);
                        double height = Math.Round(rectangle.Item1.Height + rectangle.Item1.Top) - roundedTop;
                        double width = Math.Round(rectangle.Item1.Width + rectangle.Item1.Left) - roundedLeft;

                        // Fully specify element location/size (setting size is required on WPF)
                        Canvas.SetLeft(element, roundedLeft);
                        Canvas.SetTop(element, roundedTop);
                        element.Width = width;
                        element.Height = height;

                        element.Arrange(new Rect(roundedLeft, roundedTop, width, height));
                    }
                }
            }

            return finalSize;
        }

        /// <summary>
        /// Triggers a recalculation of the layout when items are added/removed from the Interpolators collection.
        /// </summary>
        /// <param name="sender">Reference to the Interpolators collection.</param>
        /// <param name="e">Event handler arguments.</param>
        private void OnInterpolatorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildTree();
        }

        /// <summary>
        /// Returns a sequence of TreeMapNodes in breadth-first order.
        /// </summary>
        /// <returns>Sequence of TreeMapNodes.</returns>
        private IEnumerable<TreeMapNode> GetTreeMapNodes()
        {
            if (_getTreeMapNodesCache == null)
            {
                // Create a new list
                List<TreeMapNode> allNodes = new List<TreeMapNode>();

                // Seed the queue with the roots
                Queue<TreeMapNode> nodes = new Queue<TreeMapNode>();
                foreach (TreeMapNode node in _nodeRoots ?? Enumerable.Empty<TreeMapNode>())
                {
                    nodes.Enqueue(node);
                }
                // Process the queue in breadth-first order
                while (0 < nodes.Count)
                {
                    TreeMapNode node = nodes.Dequeue();
                    allNodes.Add(node);
                    foreach (TreeMapNode child in node.Children)
                    {
                        nodes.Enqueue(child);
                    }
                }

                // Cache the list
                _getTreeMapNodesCache = allNodes;
            }

            // Return the cached sequence
            return _getTreeMapNodesCache;
        }

        /// <summary>
        /// Recursively computes TreeMap rectangles given the root node and the bounding rectangle as start.
        /// </summary>
        /// <param name="root">Root of the TreeMapNode tree.</param>
        /// <param name="boundingRectangle">Bounding rectangle which will be sub-divided.</param>
        /// <returns>A list of RectangularAreas containing a rectangle for each node in the tree.</returns>
        private IEnumerable<Tuple<Rect, TreeMapNode>> ComputeRectangles(TreeMapNode root, Rect boundingRectangle)
        {
            Queue<Tuple<Rect, TreeMapNode>> treeQueue = new Queue<Tuple<Rect, TreeMapNode>>();
            treeQueue.Enqueue(new Tuple<Rect, TreeMapNode>(boundingRectangle, root));

            // Perform a breadth-first traversal of the tree
            SquaringAlgorithm algorithm = new SquaringAlgorithm();
            while (treeQueue.Count > 0)
            {
                Tuple<Rect, TreeMapNode> currentParent = treeQueue.Dequeue();

                yield return currentParent;

                foreach (Tuple<Rect, TreeMapNode> rectangle in
                    algorithm.Split(currentParent.Item1, currentParent.Item2, currentParent.Item2.ChildItemPadding))
                {
                    treeQueue.Enqueue(rectangle);
                }
            }
        }

        /// <summary>
        /// Builds the parallel trees of TreeMapNodes with references to the original user's trees.
        /// </summary>
        /// <param name="nodes">The list of roots of the user hierarchies (whatever was passed through ItemsSource).</param>
        /// <param name="level">Level being processed at this recursive call (the root node is at level 0).</param>
        /// <returns>The list of roots of the internal trees of TreeMapNodes.</returns>
        private IEnumerable<TreeMapNode> BuildTreeMapTree(IEnumerable nodes, int level)
        {
            List<TreeMapNode> retList = new List<TreeMapNode>();

            if (nodes == null)
            {
                return retList;
            }

            foreach (object root in nodes)
            {
                // Give the template selector a chance to override the template for this item.
                TreeMapItemDefinition template = null;
                if (ItemDefinitionSelector != null)
                {
                    template = ItemDefinitionSelector.SelectItemDefinition(this, root, level);
                }

                // Use the default otherwise
                if (template == null)
                {
                    template = ItemDefinition;
                }

                if (template == null)
                {
                    throw new ArgumentException(
                        Properties.Resources.TreeMap_BuildTreeMapTree_TemplateNotSet);
                }

                // Silently create 0 elements if ValueBinding is set to null 
                // in the template
                if (template.ValueBinding != null)
                {
                    IEnumerable objectChildren = (template.ItemsSource != null) ?
                        _helper.RetrieveProperty(root, template.ItemsSource) as IEnumerable :
                        null;
                    IEnumerable<TreeMapNode> children = (objectChildren != null) ?
                        BuildTreeMapTree(objectChildren, level + 1) :
                        children = Enumerable.Empty<TreeMapNode>();

                    // Subscribe to CollectionChanged for the collection
                    WeakEventListener<TreeMap, object, NotifyCollectionChangedEventArgs> weakEventListener = null;
                    INotifyCollectionChanged objectChildrenINotifyCollectionChanged = objectChildren as INotifyCollectionChanged;
                    if (objectChildrenINotifyCollectionChanged != null)
                    {
                        // Use a WeakEventListener so that the backwards reference doesn't keep this object alive
                        weakEventListener = new WeakEventListener<TreeMap, object, NotifyCollectionChangedEventArgs>(this);
                        weakEventListener.OnEventAction = (instance, source, eventArgs) => instance.ItemsSourceCollectionChanged(source, eventArgs);
                        weakEventListener.OnDetachAction = (wel) => objectChildrenINotifyCollectionChanged.CollectionChanged -= wel.OnEvent;
                        objectChildrenINotifyCollectionChanged.CollectionChanged += weakEventListener.OnEvent;
                    }

                    // Auto-aggregate children area values
                    double area;
                    if (children.Any())
                    {
                        area = children.Sum(x => x.Area);
                    }
                    else
                    {
                        IConvertible value = _helper.RetrieveProperty(root, template.ValueBinding) as IConvertible;
                        if (value == null)
                        {
                            // Provide a default value so there's something to display
                            value = 1.0;
                        }

                        area = value.ToDouble(CultureInfo.InvariantCulture);
                    }

                    // Do not include elements with negative or 0 size in the
                    // VisualTransition tree. We skip interpolation for such
                    // elements as well
                    if (area > 0)
                    {
                        // Calculate ranges for all interpolators, only consider leaf 
                        // nodes in the LeafNodesOnly mode, or all nodes in the AllNodes 
                        // mode.
                        foreach (Interpolator interpolator in Interpolators)
                        {
                            if (interpolator.InterpolationMode == InterpolationMode.AllNodes || !children.Any())
                            {
                                interpolator.IncludeInRange(root);
                            }
                        }

                        retList.Add(new TreeMapNode()
                                        {
                                            DataContext = root,
                                            Level = level,
                                            Area = area,
                                            ItemDefinition = template,
                                            ChildItemPadding = template.ChildItemPadding,
                                            Children = children,
                                            WeakEventListener = weakEventListener,
                                        });
                    }
                }
            }
            
            return retList;
        }

        /// <summary>
        /// Extracts all children from the user's trees (ItemsSource) into a flat list, and 
        /// creates UI elements for them.
        /// </summary>
        private void CreateChildren()
        {
            // Breadth-first traversal so elements closer to the root will be added first,
            // so that leaf elements will show on top of them.
            foreach (TreeMapNode current in GetTreeMapNodes())
            {
                // Create the UI element and keep a reference to it in our tree
                FrameworkElement element = GetContainerForItemOverride(current.DataContext, current.Level);
                if (element == null && current.ItemDefinition.ItemTemplate != null)
                {
                    element = current.ItemDefinition.ItemTemplate.LoadContent() as FrameworkElement;
                }

                // If an element was created
                if (element != null)
                {
                    current.Element = element;

                    // Apply interpolators to element
                    foreach (Interpolator interpolator in Interpolators)
                    {
                        // Apply interpolators only for leaf nodes in the 
                        // LeafNodesOnly mode, or for all nodes in the AllNodes 
                        // mode.
                        if (interpolator.InterpolationMode == InterpolationMode.AllNodes || !current.Children.Any())
                        {
                            DependencyObject target = element.FindName(interpolator.TargetName) as DependencyObject;
                            if (target != null)
                            {
                                SetBinding(
                                    InterpolatorValueProperty,
                                    new Binding(interpolator.TargetProperty) { Source = target, Mode = BindingMode.TwoWay });

                                if (interpolator.DataRangeBinding == null)
                                {
                                    throw new ArgumentException(
                                        Properties.Resources.TreeMap_CreateChildren_InterpolatorBindingNotSet);
                                }

                                // Extract the current value to interpolate
                                IConvertible value =
                                    _helper.RetrieveProperty(current.DataContext, interpolator.DataRangeBinding) as
                                    IConvertible;
                                if (value == null)
                                {
                                    throw new ArgumentException(
                                        Properties.Resources.Interpolator_IncludeInRange_DataRangeBindingNotIConvertible);
                                }

                                // This will update the TargetProperty of the TargetName object
                                InterpolatorValue = interpolator.Interpolate(value.ToDouble(CultureInfo.InvariantCulture));
                            }
                        }
                    }

                    // Add new child to the panel
                    element.DataContext = current.DataContext;
                    ContainerElement.Children.Add(element);
                }
            }
        }

        /// <summary>
        /// Called internally whenever a property of TreeMap is changed and the internal 
        /// structures need to be rebuilt in order to recalculate the layout.
        /// </summary>
        private void RebuildTree()
        {
            if (ContainerElement != null)
            {
                // Unhook from CollectionChanged
                foreach (TreeMapNode treeMapNode in GetTreeMapNodes().Where(n => n.WeakEventListener != null))
                {
                    treeMapNode.WeakEventListener.Detach();
                }

                // Reset all interpolators
                foreach (Interpolator interpolator in Interpolators)
                {
                    interpolator.ActualDataMinimum = double.PositiveInfinity;
                    interpolator.ActualDataMaximum = double.NegativeInfinity;
                    interpolator.DataContext = this.DataContext;
                }

                // Build the parallel tree of TreeMapNodes needed by the algorithm
                _nodeRoots = BuildTreeMapTree(ItemsSource, 0);

                // Clear cache
                _getTreeMapNodesCache = null;

                // Populate the TreeMap panel with a flat list of all children
                // in the hierarchy passed in.
                ContainerElement.Children.Clear();
                CreateChildren();

                // Refresh UI
                InvalidateArrange();
            }
        }
    }
}
