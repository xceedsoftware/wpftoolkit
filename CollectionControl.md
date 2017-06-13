# CollectionControl
Derives from Control

Provides a user interface that can edit types of collections.

![](CollectionControl_collectioneditor.jpg)

## Properties
|| Property || Description
| IsReadOnly | Gets or sets if the data in the CollectionControl can be modified.
| Items | Gets or sets the collection used to generate the content of the CollectionControl.
| ItemsSource | Gets or sets a list used to generate the content of the CollectionControl.
| ItemsSourceType | Gets or sets the type of ItemsSource.
| NewItemTypes | Gets or sets a list of custom item types that appear in the Add ListBox.
| PropertyGrid | Gets the PropertyGrid associated with the CollectionControl. Users will be able to be notifyed on a PropertyGrid.PropertyValueChanged event.
| SelectedItem | Gets or sets the currently selected item.

## Events
|| Event || Description
| ItemAdded | Raised when the "Add" button is pressed to add an item to the ListBox and the adding is done.
| ItemAdding | Raised when the "Add" button is pressed to add an item to the ListBox and the adding is starting.
| ItemDeleted | Raised when the "X" button is pressed to remove an item from the ListBox and the removing is done.
| ItemDeleting | Raised when the "X" button is pressed to remove an item from the ListBox and the removing is starting.

**Support this project, check out the [Plus Edition](https://xceed.com/xceed-toolkit-plus-for-wpf/).**
---