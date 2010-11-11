// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace System.Windows.Controls.DataVisualization
{
    /// <summary>
    /// This class contains general purpose functions to manipulate the generic
    /// IEnumerable type.
    /// </summary>
    internal static class EnumerableFunctions
    {
        /// <summary>
        /// Attempts to cast IEnumerable to a list in order to retrieve a count 
        /// in order one.  It attempts to cast fail the sequence is enumerated.
        /// </summary>
        /// <param name="that">The sequence.</param>
        /// <returns>The number of elements in the sequence.</returns>
        public static int FastCount(this IEnumerable that)
        {
            IList list = that as IList;
            if (list != null)
            {
                return list.Count;
            }
            return that.CastWrapper<object>().Count();
        }

        /// <summary>
        /// Returns the minimum value in the stream based on the result of a
        /// project function.
        /// </summary>
        /// <typeparam name="T">The stream type.</typeparam>
        /// <param name="that">The stream.</param>
        /// <param name="projectionFunction">The function that transforms the
        /// item.</param>
        /// <returns>The minimum value or null.</returns>
        public static T MinOrNull<T>(this IEnumerable<T> that, Func<T, IComparable> projectionFunction)
            where T : class
        {
            IComparable result = null;
            T minimum = default(T);
            if (!that.Any())
            {
                return minimum;
            }

            minimum = that.First();
            result = projectionFunction(minimum);
            foreach (T item in that.Skip(1))
            {
                IComparable currentResult = projectionFunction(item);
                if (result.CompareTo(currentResult) > 0)
                {
                    result = currentResult;
                    minimum = item;
                }
            }

            return minimum;
        }

        /// <summary>
        /// Returns the sum of all values in the sequence or the default value.
        /// </summary>
        /// <param name="that">The stream.</param>
        /// <returns>The sum of all values or the default value.</returns>
        public static double SumOrDefault(this IEnumerable<double> that)
        {
            if (!that.Any())
            {
                return 0.0;
            }
            else
            {
                return that.Sum();
            }
        }

        /// <summary>
        /// Returns the maximum value in the stream based on the result of a
        /// project function.
        /// </summary>
        /// <typeparam name="T">The stream type.</typeparam>
        /// <param name="that">The stream.</param>
        /// <param name="projectionFunction">The function that transforms the
        /// item.</param>
        /// <returns>The maximum value or null.</returns>
        public static T MaxOrNull<T>(this IEnumerable<T> that, Func<T, IComparable> projectionFunction)
            where T : class
        {
            IComparable result = null;
            T maximum = default(T);
            if (!that.Any())
            {
                return maximum;
            }

            maximum = that.First();
            result = projectionFunction(maximum);
            foreach (T item in that.Skip(1))
            {
                IComparable currentResult = projectionFunction(item);
                if (result.CompareTo(currentResult) < 0)
                {
                    result = currentResult;
                    maximum = item;
                }
            }

            return maximum;
        }

        /// <summary>
        /// Accepts two sequences and applies a function to the corresponding 
        /// values in the two sequences.
        /// </summary>
        /// <typeparam name="T0">The type of the first sequence.</typeparam>
        /// <typeparam name="T1">The type of the second sequence.</typeparam>
        /// <typeparam name="R">The return type of the function.</typeparam>
        /// <param name="enumerable0">The first sequence.</param>
        /// <param name="enumerable1">The second sequence.</param>
        /// <param name="func">The function to apply to the corresponding values
        /// from the two sequences.</param>
        /// <returns>A sequence of transformed values from both sequences.</returns>
        public static IEnumerable<R> Zip<T0, T1, R>(IEnumerable<T0> enumerable0, IEnumerable<T1> enumerable1, Func<T0, T1, R> func)
        {
            IEnumerator<T0> enumerator0 = enumerable0.GetEnumerator();
            IEnumerator<T1> enumerator1 = enumerable1.GetEnumerator();
            while (enumerator0.MoveNext() && enumerator1.MoveNext())
            {
                yield return func(enumerator0.Current, enumerator1.Current);
            }
        }

        /// <summary>
        /// Creates a sequence of values by accepting an initial value, an 
        /// iteration function, and apply the iteration function recursively.
        /// </summary>
        /// <typeparam name="T">The type of the sequence.</typeparam>
        /// <param name="value">The initial value.</param>
        /// <param name="nextFunction">The function to apply to the value.
        /// </param>
        /// <returns>A sequence of the iterated values.</returns>
        public static IEnumerable<T> Iterate<T>(T value, Func<T, T> nextFunction)
        {
            yield return value;
            while (true)
            {
                value = nextFunction(value);
                yield return value;
            }
        }

        /// <summary>
        /// Returns the index of an item in a sequence.
        /// </summary>
        /// <param name="that">The sequence.</param>
        /// <param name="value">The item to search for.</param>
        /// <returns>The index of the item or -1 if not found.</returns>
        public static int IndexOf(this IEnumerable that, object value)
        {
            int index = 0;
            foreach (object item in that)
            {
                if (object.ReferenceEquals(value, item) || value.Equals(item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        /// <summary>
        /// Executes an action for each item and a sequence, passing in the 
        /// index of that item to the action procedure.
        /// </summary>
        /// <typeparam name="T">The type of the sequence.</typeparam>
        /// <param name="that">The sequence.</param>
        /// <param name="action">A function that accepts a sequence item and its
        /// index in the sequence.</param>
        public static void ForEachWithIndex<T>(this IEnumerable<T> that, Action<T, int> action)
        {
            int index = 0;
            foreach (T item in that)
            {
                action(item, index);
                index++;
            }
        }

        /// <summary>
        /// Returns the maximum value or null if sequence is empty.
        /// </summary>
        /// <typeparam name="T">The type of the sequence.</typeparam>
        /// <param name="that">The sequence to retrieve the maximum value from.
        /// </param>
        /// <returns>The maximum value or null.</returns>
        public static T? MaxOrNullable<T>(this IEnumerable<T> that)
            where T : struct, IComparable
        {
            if (!that.Any())
            {
                return null;
            }
            return that.Max();
        }

        /// <summary>
        /// Returns the minimum value or null if sequence is empty.
        /// </summary>
        /// <typeparam name="T">The type of the sequence.</typeparam>
        /// <param name="that">The sequence to retrieve the minimum value from.
        /// </param>
        /// <returns>The minimum value or null.</returns>
        public static T? MinOrNullable<T>(this IEnumerable<T> that)
            where T : struct, IComparable
        {
            if (!that.Any())
            {
                return null;
            }
            return that.Min();
        }

        /// <summary>
        /// Attempts to retrieve an element at an index by testing whether a 
        /// sequence is randomly accessible.  If not, performance degrades to a 
        /// linear search.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="that">The sequence.</param>
        /// <param name="index">The index of the element in the sequence.</param>
        /// <returns>The element at the given index.</returns>
        public static T FastElementAt<T>(this IEnumerable that, int index)
        {
            {
                IList<T> list = that as IList<T>;
                if (list != null)
                {
                    return list[index];
                }
            }
            {
                IList list = that as IList;
                if (list != null)
                {
                    return (T) list[index];
                }
            }
            return that.CastWrapper<T>().ElementAt(index);
        }

        /// <summary>
        /// Applies an accumulator function over a sequence and returns each intermediate result.
        /// </summary>
        /// <typeparam name="T">Type of elements in source sequence.</typeparam>
        /// <typeparam name="S">Type of elements in result sequence.</typeparam>
        /// <param name="that">Sequence to scan.</param>
        /// <param name="seed">Initial accumulator value.</param>
        /// <param name="accumulator">Function used to generate the result sequence.</param>
        /// <returns>Sequence of intermediate results.</returns>
        public static IEnumerable<S> Scan<T, S>(this IEnumerable<T> that, S seed, Func<S, T, S> accumulator)
        {
            S value = seed;
            yield return seed;
            foreach (T t in that)
            {
                value = accumulator(value, t);
                yield return value;
            }
            yield break;
        }

        /// <summary>
        /// Converts the elements of an System.Collections.IEnumerable to the specified type.
        /// </summary>
        /// <remarks>
        /// A wrapper for the Enumerable.Cast(T) method that works around a limitation on some platforms.
        /// </remarks>
        /// <typeparam name="TResult">The type to convert the elements of source to.</typeparam>
        /// <param name="source">The System.Collections.IEnumerable that contains the elements to be converted.</param>
        /// <returns>
        /// An System.Collections.Generic.IEnumerable(T) that contains each element of the source sequence converted to the specified type.
        /// </returns>
        public static IEnumerable<TResult> CastWrapper<TResult>(this IEnumerable source)
        {
#if SILVERLIGHT
            // Certain flavors of this platform have a bug which causes Cast<T> to raise an exception incorrectly.
            // Work around that by using the more general OfType<T> method instead for no loss of functionality.
            return source.OfType<TResult>();
#else
            // No issues on this platform - call directly through to Cast<T>
            return source.Cast<TResult>();
#endif
        }
    }
}
