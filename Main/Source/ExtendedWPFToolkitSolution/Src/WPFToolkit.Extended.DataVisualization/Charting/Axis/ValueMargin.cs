// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls.DataVisualization.Charting
{
    /// <summary>
    /// A margin specified for a given value.
    /// </summary>
    public struct ValueMargin
    {
        /// <summary>
        /// Gets the value that the margin is associated with.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Gets the low margin for a value.
        /// </summary>
        public double LowMargin { get; private set; }

        /// <summary>
        /// Gets the high margin for a value.
        /// </summary>
        public double HighMargin { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ValueMargin class.
        /// </summary>
        /// <param name="value">The value the margin is associated with.</param>
        /// <param name="lowMargin">The lower margin.</param>
        /// <param name="highMargin">The higher margin.</param> 
        public ValueMargin(object value, double lowMargin, double highMargin) : this()
        {
            Value = value;
            LowMargin = lowMargin;
            HighMargin = highMargin;
        }

        /// <summary>
        /// Determines whether two value margins are equal.
        /// </summary>
        /// <param name="obj">The value margin to compare with this one.</param>
        /// <returns>A value indicating whether the two value margins are equal.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is ValueMargin)
            {
                ValueMargin valueMargin = (ValueMargin)obj;
                return this.Value.Equals(valueMargin.Value) && this.LowMargin.Equals(valueMargin.LowMargin) && this.HighMargin.Equals(valueMargin.HighMargin);
            }
            return false;
        }

        /// <summary>
        /// Determines whether two unit value objects are equal.
        /// </summary>
        /// <param name="left">The left value margin.</param>
        /// <param name="right">The right value margin.</param>
        /// <returns>A value indicating  whether two value margins objects are 
        /// equal.</returns>
        public static bool operator ==(ValueMargin left, ValueMargin right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two value margin objects are not equal.
        /// </summary>
        /// <param name="left">The left value margin.</param>
        /// <param name="right">The right value margin.</param>
        /// <returns>A value indicating whether two value margin objects are not
        /// equal.</returns>
        public static bool operator !=(ValueMargin left, ValueMargin right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns the hash code of the value margin object.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode() ^ this.LowMargin.GetHashCode() ^ this.HighMargin.GetHashCode();
        }
    }
}