using System;
using System.Collections.Generic;

namespace Microsoft.Windows.Controls.PropertyGrid.Attributes
{
    public interface IItemsSource
    {
        ItemCollection GetValues();
    }

    public class Item
    {
        public string DisplayName { get; set; }
        public object Value { get; set; }        
    }

    public class ItemCollection : List<Item>
    {
        public void Add(object value)
        {
            Item item = new Item();
            item.DisplayName = value.ToString();
            item.Value = value;
            base.Add(item);
        }

        public void Add(object value, string displayName)
        {
            Item newItem = new Item();
            newItem.DisplayName = displayName;
            newItem.Value = value;
            base.Add(newItem);
        }

    }
}
