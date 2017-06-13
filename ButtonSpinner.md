# ButtonSpinner
Derives from Xceed.Wpf.Toolkit.Spinner

The ButtonSpinner control allows you to add button spinners to any element and then respond to the Spin event to manipulate that element.  The Spin event lets the developer know which direction the buttons are spinning; SpinDirection.Increase indicates an increment, SpinDirection.Decrease indicates a decrement.

![](ButtonSpinner_buttonspinner.png)

You can wrap any element by placing it inside the content area of the ButtonSpinner control.  As an example, lets create our own simple numeric up/down control

![](ButtonSpinner_buttonspinner_numeric.png)

**XAML**
{{
        <xctk:ButtonSpinner Spin="ButtonSpinner_Spin">
            <TextBox Text="0" HorizontalContentAlignment="Right" />
        </xctk:ButtonSpinner>
}}

**Code Behind**
{{
        private void ButtonSpinner_Spin(object sender, Microsoft.Windows.Controls.SpinEventArgs e)
        {
            ButtonSpinner spinner = (ButtonSpinner)sender;
            TextBox txtBox = (TextBox)spinner.Content;

            int value = String.IsNullOrEmpty(txtBox.Text) ? 0 : Convert.ToInt32(txtBox.Text);
            if (e.Direction == Microsoft.Windows.Controls.SpinDirection.Increase)
                value++;
            else
                value--;
            txtBox.Text = value.ToString();
        }
}}
## Properties
|| Property || Description
| AllowSpin | Gets or sets a value indicating whether the spinner buttons are enabled.
| Content | Gets or sets the content of the ButtonSpinner.
| ShowButtonSpinner | Gets or sets a value indicating whether the ButtonSpinner is visible.
| ValidSpinDirection | Gets or sets the valid direction for the Spinner (None, Increase or Decrease). By default, Increase or Decrease. (Inherited from Spinner)

## Events
|| Event || Description
| Spin | Raised when spinning is initiated by the end-user. (Inherited from Spinner)

**Support this project, check out the [Plus Edition](https://xceed.com/xceed-toolkit-plus-for-wpf/).**
---