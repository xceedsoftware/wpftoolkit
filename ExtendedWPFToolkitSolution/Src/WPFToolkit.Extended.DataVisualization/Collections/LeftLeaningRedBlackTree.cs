// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

// Uncomment this to enable the following debugging aids:
//   LeftLeaningRedBlackTree.HtmlFragment
//   LeftLeaningRedBlackTree.Node.HtmlFragment
//   LeftLeaningRedBlackTree.AssertInvariants
// #define DEBUGGING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Windows.Controls.DataVisualization.Collections
{
    /// <summary>
    /// Implements a left-leaning red-black tree.
    /// </summary>
    /// <remarks>
    /// Based on the research paper "Left-leaning Red-Black Trees"
    /// by Robert Sedgewick. More information available at:
    /// http://www.cs.princeton.edu/~rs/talks/LLRB/RedBlack.pdf
    /// http://www.cs.princeton.edu/~rs/talks/LLRB/08Penn.pdf
    /// </remarks>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    internal class LeftLeaningRedBlackTree<TKey, TValue>
    {
        /// <summary>
        /// Stores the key comparison function.
        /// </summary>
        private Comparison<TKey> _keyComparison;

        /// <summary>
        /// Stores the value comparison function.
        /// </summary>
        private Comparison<TValue> _valueComparison;

        /// <summary>
        /// Stores the root node of the tree.
        /// </summary>
        private Node _rootNode;

        /// <summary>
        /// Represents a node of the tree.
        /// </summary>
        /// <remarks>
        /// Using fields instead of properties drops execution time by about 40%.
        /// </remarks>
        [DebuggerDisplay("Key={Key}, Value={Value}, Siblings={Siblings}")]
        private class Node
        {
            /// <summary>
            /// Gets or sets the node's key.
            /// </summary>
            public TKey Key;

            /// <summary>
            /// Gets or sets the node's value.
            /// </summary>
            public TValue Value;

            /// <summary>
            /// Gets or sets the left node.
            /// </summary>
            public Node Left;

            /// <summary>
            /// Gets or sets the right node.
            /// </summary>
            public Node Right;

            /// <summary>
            /// Gets or sets the color of the node.
            /// </summary>
            public bool IsBlack;

            /// <summary>
            /// Gets or sets the number of "siblings" (nodes with the same key/value).
            /// </summary>
            public int Siblings;

#if DEBUGGING
        /// <summary>
        /// Gets an HTML fragment representing the node and its children.
        /// </summary>
        public string HtmlFragment
        {
            get
            {
                return
                    "<table border='1'>" +
                        "<tr>" +
                            "<td colspan='2' align='center' bgcolor='" + (IsBlack ? "gray" : "red") + "'>" + Key + ", " + Value + " [" + Siblings + "]</td>" +
                        "</tr>" +
                        "<tr>" +
                            "<td valign='top'>" + (null != Left ? Left.HtmlFragment : "[null]") + "</td>" +
                            "<td valign='top'>" + (null != Right ? Right.HtmlFragment : "[null]") + "</td>" +
                        "</tr>" +
                    "</table>";
            }
        }
#endif
        }

        /// <summary>
        /// Initializes a new instance of the LeftLeaningRedBlackTree class implementing a normal dictionary.
        /// </summary>
        /// <param name="keyComparison">The key comparison function.</param>
        public LeftLeaningRedBlackTree(Comparison<TKey> keyComparison)
        {
            if (null == keyComparison)
            {
                throw new ArgumentNullException("keyComparison");
            }
            _keyComparison = keyComparison;
        }

        /// <summary>
        /// Initializes a new instance of the LeftLeaningRedBlackTree class implementing an ordered multi-dictionary.
        /// </summary>
        /// <param name="keyComparison">The key comparison function.</param>
        /// <param name="valueComparison">The value comparison function.</param>
        public LeftLeaningRedBlackTree(Comparison<TKey> keyComparison, Comparison<TValue> valueComparison)
            : this(keyComparison)
        {
            if (null == valueComparison)
            {
                throw new ArgumentNullException("valueComparison");
            }
            _valueComparison = valueComparison;
        }

        /// <summary>
        /// Gets a value indicating whether the tree is acting as an ordered multi-dictionary.
        /// </summary>
        private bool IsMultiDictionary
        {
            get { return null != _valueComparison; }
        }

        /// <summary>
        /// Adds a key/value pair to the tree.
        /// </summary>
        /// <param name="key">Key to add.</param>
        /// <param name="value">Value to add.</param>
        public void Add(TKey key, TValue value)
        {
            _rootNode = Add(_rootNode, key, value);
            _rootNode.IsBlack = true;
#if DEBUGGING
            AssertInvariants();
#endif
        }

        /// <summary>
        /// Removes a key (and its associated value) from a normal (non-multi) dictionary.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        /// <returns>True if key present and removed.</returns>
        public bool Remove(TKey key)
        {
            if (IsMultiDictionary)
            {
                throw new InvalidOperationException("Remove is only supported when acting as a normal (non-multi) dictionary.");
            }
            return Remove(key, default(TValue));
        }

        /// <summary>
        /// Removes a key/value pair from the tree.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        /// <param name="value">Value to remove.</param>
        /// <returns>True if key/value present and removed.</returns>
        public bool Remove(TKey key, TValue value)
        {
            int initialCount = Count;
            if (null != _rootNode)
            {
                _rootNode = Remove(_rootNode, key, value);
                if (null != _rootNode)
                {
                    _rootNode.IsBlack = true;
                }
            }
#if DEBUGGING
            AssertInvariants();
#endif
            return initialCount != Count;
        }

        /// <summary>
        /// Removes all nodes in the tree.
        /// </summary>
        public void Clear()
        {
            _rootNode = null;
            Count = 0;
#if DEBUGGING
            AssertInvariants();
#endif
        }

        /// <summary>
        /// Gets a sorted list of keys in the tree.
        /// </summary>
        /// <returns>Sorted list of keys.</returns>
        public IEnumerable<TKey> GetKeys()
        {
            TKey lastKey = default(TKey);
            bool lastKeyValid = false;
            return Traverse(
                _rootNode,
                n => !lastKeyValid || !object.Equals(lastKey, n.Key),
                n =>
                {
                    lastKey = n.Key;
                    lastKeyValid = true;
                    return lastKey;
                });
        }

        /// <summary>
        /// Gets the value associated with the specified key in a normal (non-multi) dictionary.
        /// </summary>
        /// <param name="key">Specified key.</param>
        /// <returns>Value associated with the specified key.</returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetValueForKey", Justification = "Method name.")]
        public TValue GetValueForKey(TKey key)
        {
            if (IsMultiDictionary)
            {
                throw new InvalidOperationException("GetValueForKey is only supported when acting as a normal (non-multi) dictionary.");
            }
            Node node = GetNodeForKey(key);
            if (null != node)
            {
                return node.Value;
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Gets a sequence of the values associated with the specified key.
        /// </summary>
        /// <param name="key">Specified key.</param>
        /// <returns>Sequence of values.</returns>
        public IEnumerable<TValue> GetValuesForKey(TKey key)
        {
            return Traverse(GetNodeForKey(key), n => 0 == _keyComparison(n.Key, key), n => n.Value);
        }

        /// <summary>
        /// Gets a sequence of all the values in the tree.
        /// </summary>
        /// <returns>Sequence of all values.</returns>
        public IEnumerable<TValue> GetValuesForAllKeys()
        {
            return Traverse(_rootNode, n => true, n => n.Value);
        }

        /// <summary>
        /// Gets the count of key/value pairs in the tree.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the minimum key in the tree.
        /// </summary>
        public TKey MinimumKey
        {
            get { return GetExtreme(_rootNode, n => n.Left, n => n.Key); }
        }

        /// <summary>
        /// Gets the maximum key in the tree.
        /// </summary>
        public TKey MaximumKey
        {
            get { return GetExtreme(_rootNode, n => n.Right, n => n.Key); }
        }

        /// <summary>
        /// Gets the minimum key's minimum value.
        /// </summary>
        public TValue MinimumValue
        {
            get { return GetExtreme(_rootNode, n => n.Left, n => n.Value); }
        }

        /// <summary>
        /// Gets the maximum key's maximum value.
        /// </summary>
        public TValue MaximumValue
        {
            get { return GetExtreme(_rootNode, n => n.Right, n => n.Value); }
        }

        /// <summary>
        /// Returns true if the specified node is red.
        /// </summary>
        /// <param name="node">Specified node.</param>
        /// <returns>True if specified node is red.</returns>
        private static bool IsRed(Node node)
        {
            if (null == node)
            {
                // "Virtual" leaf nodes are always black
                return false;
            }
            return !node.IsBlack;
        }

        /// <summary>
        /// Adds the specified key/value pair below the specified root node.
        /// </summary>
        /// <param name="node">Specified node.</param>
        /// <param name="key">Key to add.</param>
        /// <param name="value">Value to add.</param>
        /// <returns>New root node.</returns>
        private Node Add(Node node, TKey key, TValue value)
        {
            if (null == node)
            {
                // Insert new node
                Count++;
                return new Node { Key = key, Value = value };
            }

            if (IsRed(node.Left) && IsRed(node.Right))
            {
                // Split node with two red children
                FlipColor(node);
            }

            // Find right place for new node
            int comparisonResult = KeyAndValueComparison(key, value, node.Key, node.Value);
            if (comparisonResult < 0)
            {
                node.Left = Add(node.Left, key, value);
            }
            else if (0 < comparisonResult)
            {
                node.Right = Add(node.Right, key, value);
            }
            else
            {
                if (IsMultiDictionary)
                {
                    // Store the presence of a "duplicate" node
                    node.Siblings++;
                    Count++;
                }
                else
                {
                    // Replace the value of the existing node
                    node.Value = value;
                }
            }

            if (IsRed(node.Right))
            {
                // Rotate to prevent red node on right
                node = RotateLeft(node);
            }

            if (IsRed(node.Left) && IsRed(node.Left.Left))
            {
                // Rotate to prevent consecutive red nodes
                node = RotateRight(node);
            }

            return node;
        }

        /// <summary>
        /// Removes the specified key/value pair from below the specified node.
        /// </summary>
        /// <param name="node">Specified node.</param>
        /// <param name="key">Key to remove.</param>
        /// <param name="value">Value to remove.</param>
        /// <returns>True if key/value present and removed.</returns>
        private Node Remove(Node node, TKey key, TValue value)
        {
            int comparisonResult = KeyAndValueComparison(key, value, node.Key, node.Value);
            if (comparisonResult < 0)
            {
                // * Continue search if left is present
                if (null != node.Left)
                {
                    if (!IsRed(node.Left) && !IsRed(node.Left.Left))
                    {
                        // Move a red node over
                        node = MoveRedLeft(node);
                    }

                    // Remove from left
                    node.Left = Remove(node.Left, key, value);
                }
            }
            else
            {
                if (IsRed(node.Left))
                {
                    // Flip a 3 node or unbalance a 4 node
                    node = RotateRight(node);
                }
                if ((0 == KeyAndValueComparison(key, value, node.Key, node.Value)) && (null == node.Right))
                {
                    // Remove leaf node
                    Debug.Assert(null == node.Left, "About to remove an extra node.");
                    Count--;
                    if (0 < node.Siblings)
                    {
                        // Record the removal of the "duplicate" node
                        Debug.Assert(IsMultiDictionary, "Should not have siblings if tree is not a multi-dictionary.");
                        node.Siblings--;
                        return node;
                    }
                    else
                    {
                        // Leaf node is gone
                        return null;
                    }
                }
                // * Continue search if right is present
                if (null != node.Right)
                {
                    if (!IsRed(node.Right) && !IsRed(node.Right.Left))
                    {
                        // Move a red node over
                        node = MoveRedRight(node);
                    }
                    if (0 == KeyAndValueComparison(key, value, node.Key, node.Value))
                    {
                        // Remove leaf node
                        Count--;
                        if (0 < node.Siblings)
                        {
                            // Record the removal of the "duplicate" node
                            Debug.Assert(IsMultiDictionary, "Should not have siblings if tree is not a multi-dictionary.");
                            node.Siblings--;
                        }
                        else
                        {
                            // Find the smallest node on the right, swap, and remove it
                            Node m = GetExtreme(node.Right, n => n.Left, n => n);
                            node.Key = m.Key;
                            node.Value = m.Value;
                            node.Siblings = m.Siblings;
                            node.Right = DeleteMinimum(node.Right);
                        }
                    }
                    else
                    {
                        // Remove from right
                        node.Right = Remove(node.Right, key, value);
                    }
                }
            }

            // Maintain invariants
            return FixUp(node);
        }

        /// <summary>
        /// Flip the colors of the specified node and its direct children.
        /// </summary>
        /// <param name="node">Specified node.</param>
        private static void FlipColor(Node node)
        {
            node.IsBlack = !node.IsBlack;
            node.Left.IsBlack = !node.Left.IsBlack;
            node.Right.IsBlack = !node.Right.IsBlack;
        }

        /// <summary>
        /// Rotate the specified node "left".
        /// </summary>
        /// <param name="node">Specified node.</param>
        /// <returns>New root node.</returns>
        private static Node RotateLeft(Node node)
        {
            Node x = node.Right;
            node.Right = x.Left;
            x.Left = node;
            x.IsBlack = node.IsBlack;
            node.IsBlack = false;
            return x;
        }

        /// <summary>
        /// Rotate the specified node "right".
        /// </summary>
        /// <param name="node">Specified node.</param>
        /// <returns>New root node.</returns>
        private static Node RotateRight(Node node)
        {
            Node x = node.Left;
            node.Left = x.Right;
            x.Right = node;
            x.IsBlack = node.IsBlack;
            node.IsBlack = false;
            return x;
        }

        /// <summary>
        /// Moves a red node from the right child to the left child.
        /// </summary>
        /// <param name="node">Parent node.</param>
        /// <returns>New root node.</returns>
        private static Node MoveRedLeft(Node node)
        {
            FlipColor(node);
            if (IsRed(node.Right.Left))
            {
                node.Right = RotateRight(node.Right);
                node = RotateLeft(node);
                FlipColor(node);

                // * Avoid creating right-leaning nodes
                if (IsRed(node.Right.Right))
                {
                    node.Right = RotateLeft(node.Right);
                }
            }
            return node;
        }

        /// <summary>
        /// Moves a red node from the left child to the right child.
        /// </summary>
        /// <param name="node">Parent node.</param>
        /// <returns>New root node.</returns>
        private static Node MoveRedRight(Node node)
        {
            FlipColor(node);
            if (IsRed(node.Left.Left))
            {
                node = RotateRight(node);
                FlipColor(node);
            }
            return node;
        }

        /// <summary>
        /// Deletes the minimum node under the specified node.
        /// </summary>
        /// <param name="node">Specified node.</param>
        /// <returns>New root node.</returns>
        private Node DeleteMinimum(Node node)
        {
            if (null == node.Left)
            {
                // Nothing to do
                return null;
            }

            if (!IsRed(node.Left) && !IsRed(node.Left.Left))
            {
                // Move red node left
                node = MoveRedLeft(node);
            }

            // Recursively delete
            node.Left = DeleteMinimum(node.Left);

            // Maintain invariants
            return FixUp(node);
        }

        /// <summary>
        /// Maintains invariants by adjusting the specified nodes children.
        /// </summary>
        /// <param name="node">Specified node.</param>
        /// <returns>New root node.</returns>
        private static Node FixUp(Node node)
        {
            if (IsRed(node.Right))
            {
                // Avoid right-leaning node
                node = RotateLeft(node);
            }

            if (IsRed(node.Left) && IsRed(node.Left.Left))
            {
                // Balance 4-node
                node = RotateRight(node);
            }

            if (IsRed(node.Left) && IsRed(node.Right))
            {
                // Push red up
                FlipColor(node);
            }

            // * Avoid leaving behind right-leaning nodes
            if ((null != node.Left) && IsRed(node.Left.Right) && !IsRed(node.Left.Left))
            {
                node.Left = RotateLeft(node.Left);
                if (IsRed(node.Left))
                {
                    // Balance 4-node
                    node = RotateRight(node);
                }
            }

            return node;
        }

        /// <summary>
        /// Gets the (first) node corresponding to the specified key.
        /// </summary>
        /// <param name="key">Key to search for.</param>
        /// <returns>Corresponding node or null if none found.</returns>
        private Node GetNodeForKey(TKey key)
        {
            // Initialize
            Node node = _rootNode;
            while (null != node)
            {
                // Compare keys and go left/right
                int comparisonResult = _keyComparison(key, node.Key);
                if (comparisonResult < 0)
                {
                    node = node.Left;
                }
                else if (0 < comparisonResult)
                {
                    node = node.Right;
                }
                else
                {
                    // Match; return node
                    return node;
                }
            }

            // No match found
            return null;
        }

        /// <summary>
        /// Gets an extreme (ex: minimum/maximum) value.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="node">Node to start from.</param>
        /// <param name="successor">Successor function.</param>
        /// <param name="selector">Selector function.</param>
        /// <returns>Extreme value.</returns>
        private static T GetExtreme<T>(Node node, Func<Node, Node> successor, Func<Node, T> selector)
        {
            // Initialize
            T extreme = default(T);
            Node current = node;
            while (null != current)
            {
                // Go to extreme
                extreme = selector(current);
                current = successor(current);
            }
            return extreme;
        }

        /// <summary>
        /// Traverses a subset of the sequence of nodes in order and selects the specified nodes.
        /// </summary>
        /// <typeparam name="T">Type of elements.</typeparam>
        /// <param name="node">Starting node.</param>
        /// <param name="condition">Condition method.</param>
        /// <param name="selector">Selector method.</param>
        /// <returns>Sequence of selected nodes.</returns>
        private IEnumerable<T> Traverse<T>(Node node, Func<Node, bool> condition, Func<Node, T> selector)
        {
            // Create a stack to avoid recursion
            Stack<Node> stack = new Stack<Node>();
            Node current = node;
            while (null != current)
            {
                if (null != current.Left)
                {
                    // Save current state and go left
                    stack.Push(current);
                    current = current.Left;
                }
                else
                {
                    do
                    {
                        for (int i = 0; i <= current.Siblings; i++)
                        {
                            // Select current node if relevant
                            if (condition(current))
                            {
                                yield return selector(current);
                            }
                        }
                        // Go right - or up if nothing to the right
                        current = current.Right;
                    }
                    while ((null == current) &&
                           (0 < stack.Count) &&
                           (null != (current = stack.Pop())));
                }
            }
        }

        /// <summary>
        /// Compares the specified keys (primary) and values (secondary).
        /// </summary>
        /// <param name="leftKey">The left key.</param>
        /// <param name="leftValue">The left value.</param>
        /// <param name="rightKey">The right key.</param>
        /// <param name="rightValue">The right value.</param>
        /// <returns>CompareTo-style results: -1 if left is less, 0 if equal, and 1 if greater than right.</returns>
        private int KeyAndValueComparison(TKey leftKey, TValue leftValue, TKey rightKey, TValue rightValue)
        {
            // Compare keys
            int comparisonResult = _keyComparison(leftKey, rightKey);
            if ((0 == comparisonResult) && (null != _valueComparison))
            {
                // Keys match; compare values
                comparisonResult = _valueComparison(leftValue, rightValue);
            }
            return comparisonResult;
        }

#if DEBUGGING
        /// <summary>
        /// Asserts that tree invariants are not violated.
        /// </summary>
        private void AssertInvariants()
        {
            // Root is black
            Debug.Assert((null == _rootNode) || _rootNode.IsBlack, "Root is not black");
            // Every path contains the same number of black nodes
            Dictionary<Node, Node> parents = new Dictionary<LeftLeaningRedBlackTree<TKey, TValue>.Node, LeftLeaningRedBlackTree<TKey, TValue>.Node>();
            foreach (Node node in Traverse(_rootNode, n => true, n => n))
            {
                if (null != node.Left)
                {
                    parents[node.Left] = node;
                }
                if (null != node.Right)
                {
                    parents[node.Right] = node;
                }
            }
            if (null != _rootNode)
            {
                parents[_rootNode] = null;
            }
            int treeCount = -1;
            foreach (Node node in Traverse(_rootNode, n => (null == n.Left) || (null == n.Right), n => n))
            {
                int pathCount = 0;
                Node current = node;
                while (null != current)
                {
                    if (current.IsBlack)
                    {
                        pathCount++;
                    }
                    current = parents[current];
                }
                Debug.Assert((-1 == treeCount) || (pathCount == treeCount), "Not all paths have the same number of black nodes.");
                treeCount = pathCount;
            }
            // Verify node properties...
            foreach (Node node in Traverse(_rootNode, n => true, n => n))
            {
                // Left node is less
                if (null != node.Left)
                {
                    Debug.Assert(0 > KeyAndValueComparison(node.Left.Key, node.Left.Value, node.Key, node.Value), "Left node is greater than its parent.");
                }
                // Right node is greater
                if (null != node.Right)
                {
                    Debug.Assert(0 < KeyAndValueComparison(node.Right.Key, node.Right.Value, node.Key, node.Value), "Right node is less than its parent.");
                }
                // Both children of a red node are black
                Debug.Assert(!IsRed(node) || (!IsRed(node.Left) && !IsRed(node.Right)), "Red node has a red child.");
                // Always left-leaning
                Debug.Assert(!IsRed(node.Right) || IsRed(node.Left), "Node is not left-leaning.");
                // No consecutive reds (subset of previous rule)
                //Debug.Assert(!(IsRed(node) && IsRed(node.Left)));
            }
        }

        /// <summary>
        /// Gets an HTML fragment representing the tree.
        /// </summary>
        public string HtmlDocument
        {
            get
            {
                return
                    "<html>" +
                        "<body>" +
                            (null != _rootNode ? _rootNode.HtmlFragment : "[null]") +
                        "</body>" +
                    "</html>";
            }
        }
#endif
    }
}