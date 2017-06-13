# CheckListBox
Derives from Xceed.Wpf.Toolkit.Primitives.Selector

The CheckListBox control is a ListBox in which each item is represented with a CheckBox. The CheckBox.IsSelected can be data bound using the SelectedMemberPath property. The CheckListBox also provides a Command property which will execute everytime an item is checked/unchecked. The CommandParameter is the recently checked/unchecked item.

NOTE: As of v1.6.0, this control derives from Selector.

![](CheckListBox_checklistbox.jpg)

{{
         <xctk:CheckListBox x:Name="_listBox" 
                            Height="250"
                            DisplayMemberPath="Color"
                            ValueMemberPath="Level" 
                            SelectedMemberPath="IsSelected"
                            SelectedValue="{Binding SelectedValue}"
                            SelectedItemsOverride="{Binding SelectedItems}" />
}}

## Properties
|| Property || Description
| Command | Gets or sets the command to execute when an item is checked/unchecked. (Inherited from Selector)
| Delimiter | Gets or sets the string used to separate the concatenated string representations of the checked items. (Inherited from Selector)
| SelectedItem | Gets or sets the last checked item. (Inherited from Selector)
| SelectedItems | Gets the collection of checked items. (Inherited from Selector)
| SelectedItemsOverride | Gets or sets the list of **SelectedItems**. (Inherited from Selector)
| SelectedMemberPath | Gets or sets a path to a value on the source object used to determine whether an item is selected. (Inherited from Selector)
| SelectedValue | Gets or sets a string containing the selected items separated by the value of Delimiter (ex., "Item1, Item2, Item3"). (Inherited from Selector)
| ValueMemberPath | Gets or sets a path to a value on the source object representing the value to use. (Inherited from Selector)

## Events
|| Event || Description
| ItemSelectionChanged | Raised when an item's selection is changed. (Inherited from Selector)

**Support this project, check out the [Plus Edition](https://xceed.com/xceed-toolkit-plus-for-wpf/).**
---