# WatermarkPasswordBox
_Only available in the Plus Edition_

Derives from WatermarkTextBox

The WatermarkPasswordBox is a PasswordBox where you can enter a secure password and display a watermark when the password is not defined.

## Properties
|| Property || Description
| AutoMoveFocus | Gets or sets a value indicating if the focus can navigate in the appropriate flow direction (e.g., from one cell to another when a cell is being edited) when the cursor is at the beginning or end of the auto-select text box. (Inherited from Xceed.Wpf.Toolkit.AutoSelectTextBox)
| AutoSelectBehavior | Gets or sets a value indicating how the content of the auto-select text box is selected. (Inherited from Xceed.Wpf.Toolkit.AutoSelectTextBox)
| KeepWatermarkOnGotFocus | Gets or sets a value indicating if the watermark will be displayed when the focus is set on the WatermarkTextBox when the text is empty. (Inherited from Xceed.Wpf.Toolkit.WatermarkTextBox)
| Password | Gets or sets the password currently held by the WatermarkPasswordBox. Default value is System.String.Empty.  
| PasswordChar | Gets or sets the masking character for the WatermarkPasswordBox when the user enters text. Default value is a bullet character.  
| SecurePassword | Gets the password currently held by the WatermarkPasswordBox as a System.Security.SecureString.  
| Watermark | Gets or sets the object to use in place of null or missing Text. (Inherited from Xceed.Wpf.Toolkit.WatermarkTextBox)
| WatermarkTemplate | Gets or sets the DataTemplate to use to display the watermark. (Inherited from Xceed.Wpf.Toolkit.WatermarkTextBox)

## Events
|| Event || Description
| PasswordChanged | Raised when the value of the WatermarkPasswordBox.Password property changes.
---