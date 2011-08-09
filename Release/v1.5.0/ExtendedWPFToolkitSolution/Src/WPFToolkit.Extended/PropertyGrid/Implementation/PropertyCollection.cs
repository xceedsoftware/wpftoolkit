using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace Microsoft.Windows.Controls.PropertyGrid
{
    public class PropertyCollection : ObservableCollection<PropertyItem>
    {
        public PropertyCollection()
        {
            
        }

        public PropertyCollection(List<PropertyItem> list)
            : base(list)
        {
            
        }
        public PropertyCollection(IEnumerable<PropertyItem> collection)
            : base(collection)
        {
                        
        }

        private ICollectionView GetDefaultView()
        {
            return CollectionViewSource.GetDefaultView(this);
        }

        public void GroupBy(string name)
        {
            GetDefaultView().GroupDescriptions.Add(new PropertyGroupDescription(name));
        }

        public void SortBy(string name, ListSortDirection sortDirection)
        {
            GetDefaultView().SortDescriptions.Add(new SortDescription(name, sortDirection));
        }

        public void Filter(string text)
        {
            GetDefaultView().Filter = (item) => 
            {
                var property = item as PropertyItem;
                return property.DisplayName.ToLower().StartsWith(text.ToLower());
            };
        }
    }
}
