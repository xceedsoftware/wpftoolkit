# WindowContainer
Derives from Canvas

Starting with version 2.0, ChildWindow and MessageBox are derived from the WindowControl class and no longer manage their parent’s background or their position based on their parent’s size.

A WindowContainer should now be used to contain these controls. It provides an area where multiple WindowControl-derived controls (ChildWindow or MessageBox controls) can be displayed. This is particularly interesting in an XBAP application where windows can't be popped up. In this case the WindowContainer can be sized to fit the application and the window-like control can be moved around in the WindowContainer.

The WindowContainer derives from Canvas and positions its children according to its size. Many actions performed on its children are managed by the WindowContainer (movement and positioning, resizing, visibility, modal, and mouse click). This will restrict the WindowControl movements and resizing to the WindowContainer’s size.

When no Width and Height are specified in the WindowContainer, its DesiredSize will be the size of its biggest child.

When a child of the WindowContainer is modal (modal ChildWindow or MessageBox) and visible, the background of the WindowContainer can be colored via the ModalBackgroundBrush property.

It can be useful to set the WindowContainer over an application (with the same width and height) and to use a semi-transparent ModalBackgroundBrush property. When a modal window is shown, the application controls will still be visible through the WindowContainer.

In the WindowContainer, the modal windows will always be in front, preventing the use of other windows from the WindowContainer or controls from the application.

{{
<xctk:WindowContainer>
  <xctk:ChildWindow WindowBackground="Blue"
                    Left="75"
                    Top="50"
                    Width="275"
                    Height="125"
                    WindowState="Open">
    <TextBlock Text="This is a Child Window" Padding="10"/>
  </xctk:ChildWindow>

  <xctk:ChildWindow WindowBackground="Green"
                    Left="175"
                    Top="125"
                    Width="275"
                    Height="125"
                    WindowState="Open">
    <TextBlock Text="This is another Child Window" Padding="10"/>
  </xctk:ChildWindow>

  <xctk:MessageBox Caption="MessageBox"
                   Text="This is a MessageBox"/>
</xctk:WindowContainer>
}}
Note: You can find complete documentation of the API [here](http://doc.xceedsoft.com/products/XceedWpfToolkit/). See the 'Live Explorer' application with source code that demonstrates the features of this class and others [here](http://wpftoolkit.com/try-it/).

## Properties
|| Property || Description
| ModalBackgroundBrush | Gets or sets a Background Color for the WindowContainer when using a modal window.

**Support this project, check out the [Plus Edition](https://xceed.com/xceed-toolkit-plus-for-wpf/).**
---