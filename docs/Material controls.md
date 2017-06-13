# Material Design Controls
_Only available in the Plus Edition_

The toolkit's material controls (see list below) were designed to follow the color palettes and animations defined by the [Material Design Specifications](http://www.google.com/design/spec/material-design/introduction.html#).

Each control exposes a **MaterialAccent** property, which represents the material color palette that will be applied to the control. Each color palette defines complimentary background and foreground colors in addition to any other required brushes, such as the selection and border brushes. Setting the **MaterialAccentBrush** or **MaterialForeground** properties will override the values defined by the color palette assigned to the **MaterialAccent** property.

## Included Controls

* [MaterialButton](MaterialButton)
* [MaterialCheckBox](MaterialCheckBox)
* [MaterialComboBox](MaterialComboBox) and [MaterialComboBoxItem](MaterialComboBoxItem)
* [MaterialDropDown](MaterialDropDown)
* [MaterialFrame](MaterialFrame)
* [MaterialListBox](MaterialListBox) and [MaterialListBoxItem](MaterialListBoxItem)
* [MaterialProgressBar](MaterialProgressBar)
* [MaterialProgressBarCircular](MaterialProgressBarCircular)
* [MaterialRadioButton](MaterialRadioButton)
* [MaterialSlider](MaterialSlider)
* [MaterialSwitch](MaterialSwitch)
* [MaterialTabControl](MaterialTabControl) and [MaterialTabItem](MaterialTabItem)
* [MaterialTextField](MaterialTextField)
* [MaterialToast](MaterialToast)
* [MaterialToolTip](MaterialToolTip)

{{
  <StackPanel>
      <!-- Using one of the predefined material palettes -->
      <xctk:MaterialButton Content="DeepPurple material palette"
                           MaterialAccent="DeepPurple"
                           Width="150"
                           Height="25" 
                           Margin="5" />
      <!-- Using a custom brush -->
      <xctk:MaterialButton Content="Custom colors"
                           MaterialAccentBrush="Aqua"
                           MaterialForeground="White"
                           Width="150"
                           Height="25"
                           Margin="5" />
  </StackPanel>
}}

## Material Design Color Palette

![](Material controls_material_palette.png)
---