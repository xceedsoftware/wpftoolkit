// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// Represents a storyboard queue that plays storyboards in sequence.
    /// </summary>
    internal class StoryboardQueue
    {
        /// <summary>
        /// A queue of the storyboards.
        /// </summary>
        private Queue<Storyboard> _storyBoards = new Queue<Storyboard>();

        /// <summary>
        /// Accepts a new storyboard to play in sequence.
        /// </summary>
        /// <param name="storyBoard">The storyboard to play.</param>
        /// <param name="completedAction">An action to execute when the 
        /// storyboard completes.</param>
        public void Enqueue(Storyboard storyBoard, EventHandler completedAction)
        {
            storyBoard.Completed +=
                (sender, args) =>
                {
                    if (completedAction != null)
                    {
                        completedAction(sender, args);
                    }

                    _storyBoards.Dequeue();
                    Dequeue();
                };

            _storyBoards.Enqueue(storyBoard);

            if (_storyBoards.Count == 1)
            {
                Dequeue();
            }
        }

        /// <summary>
        /// Removes the next storyboard in the queue and plays it.
        /// </summary>
        private void Dequeue()
        {
            if (_storyBoards.Count > 0)
            {
                Storyboard storyboard = _storyBoards.Peek();
                storyboard.Begin();
            }
        }
    }
}