# FilePicker Plus
_Only available in the Plus Edition_

Derives from System.Windows.Controls.Control

Represents an editor that allows a user to pick a file from the disk.

## Properties
|| Property || Description
| BrowseButtonStyle | Gets or sets the style of the "Browse" button.
| BrowseContent | Gets or sets the content of the "Browse" button.  
| Filter | Gets or sets the filter to ues when opening the browsing window. For example, "Image Files | **.jpg;**.jpeg"
| InitialDirectory | Gets or sets the path that will be used as the intial directory when the browsing window is opened.
| IsOpen | Gets or sets a value indicating whether the browsing window is open.  
| MultiSelect | Gets or sets a value indicating whether mulitple files can be selected in the browsing window.  
| SelectedFile | Gets or sets the name of the selected file.  
| SelectedFiles | Gets or sets a collection of strings representing the names of the selected files.  
| SelectedValue | Gets the string representing the displayed selection(s) in the FilePicker.    
| Title | Gets or sets the title of the browsing window.  
| UseFullPath | Gets or sets a value indicating whether the full path of the selected file(s) should be included when return the name of the selected file(s) through the SelectedFile or SelectedFiles properties.
| Watermark | Gets or sets the watermark to display when no files are selected.  
| WatermarkTemplate | Gets or sets the DataTemplate to use for the Watermark.  

## Methods
|| Method || Description
| OnSelectedValueChanged | Raises the SelectedValueChanged event.  

## Events
|| Event || Description
| SelectedFileChanged | Raised when the selected file has been changed.
| SelectedFilesChanged | Raised when the selected files have been changed.
---