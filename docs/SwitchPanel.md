# SwitchPanel
Derives from PanelBase

SwitchPanel allows you to switch between panels with the same children on the fly with animation support. Includes [RandomPanel](RandomPanel) and [WrapPanel](WrapPanel) layouts as examples.

## Usage

**XAML**

{{<xctk:SwitchPanel x:Name="_switchPanel" Grid.Row="1" ActiveLayoutIndex="{Binding ElementName=layoutCombo, Path=SelectedIndex}" ActiveLayoutChanged="OnSwitchPanelLayoutChanged">
            <xctk:SwitchPanel.Layouts>
               <xctk:WrapPanel x:Name="_wrapPanel" ItemWidth="100" ItemHeight="100"/>
               <xctk:RandomPanel x:Name="_randomPanel" />
            </xctk:SwitchPanel.Layouts>
            <TextBlock x:Name="_item1" Text="Item #1" Style="{StaticResource panelElement}"/>
            <TextBlock x:Name="_item2" Text="Item #2" Style="{StaticResource panelElement}"/>
            <TextBlock x:Name="_item3" Text="Item #3" Style="{StaticResource panelElement}"/>
            <TextBlock x:Name="_item4" Text="Item #4" Style="{StaticResource panelElement}"/>
            <TextBlock x:Name="_item5" Text="Item #5" Style="{StaticResource panelElement}"/>
            <TextBlock x:Name="_item6" Text="Item #6" Style="{StaticResource panelElement}"/>
            <TextBlock x:Name="_item7" Text="Item #7" Style="{StaticResource panelElement}"/>
            <TextBlock x:Name="_item8" Text="Item #8" Style="{StaticResource panelElement}"/>
         </xctk:SwitchPanel>
}}

## Properties
|| Property || Description
| ActiveLayout | Gets the ActiveLayout property. This dependency property indicates which animation panel is currently controlling layout for the SwitchPanel.  
| ActiveLayoutIndex | Gets or sets the ActiveLayoutIndex property. This dependency property indicates the index of the current SwitchablePanel within the Layouts collection.  
| ActiveSwitchTemplate | Gets the ActiveSwitchTemplate property. This dependency property indicates which switch template should be used by SwitchPresenter descendants.  
| AreLayoutSwitchesAnimated | Gets or sets the AreLayoutSwitchesAnimated property. This dependency property indicates whether transitions between panels are animated.  
| CanHorizontallyScroll | Gets or sets a value indicating whether the children can scroll horizontally.
| CanVerticallyScroll | Gets or sets a value indicating whether the children can scroll vertically. 
| DefaultAnimationRate | Gets or sets the DefaultAnimationRate property. This dependency property indicates the duration or speed at which other animations will occur for panels within the layouts collection that set their respective AnimationRate properties to AnimationRate.Default. This property can be used to set a single animation rate to be used for EnterAnimationRate, ExitAnimationRate, LayoutAnimationRate, SwitchAnimationRate, and TemplateAnimationRate.  
| DefaultAnimator | Gets or sets the DefaultAnimator property. This dependency property indicates the default animator that will be used by panels within the Layouts collection that do not explicitly specify their own DefaultAnimator value.  
| EnterAnimationRate | Gets or sets the EnterAnimationRate property. This dependency property indicates the default animation rate that will be used by panels within the Layouts collection that do not explicitly specify their own EnterAnimationRate value.  
| EnterAnimator | Gets or sets the EnterAnimator property. This dependency property indicates the default animator that will be used by panels within the Layouts collection that do not explicitly specify their own EnterAnimator value.  
| ExitAnimationRate | Gets or sets the ExitAnimationRate property. This dependency property indicates the default animation rate that will be used by panels within the Layouts collection that do not explicitly specify their own ExitAnimationRate value.  
| ExitAnimator | Gets or sets the ExitAnimator property. This dependency property indicates the default animator that will be used by panels within the Layouts collection that do not explicitly specify their own ExitAnimator value.  
| ExtentHeight | Gets the extent height.
| ExtentWidth | Gets the extent width.	 
| HorizontalOffset | Gets the horizontal offsett. 
| LayoutAnimationRate | Gets or sets the LayoutAnimationRate property. This dependency property indicates the default animation rate that will be used by panels within the Layouts collection that do not explicitly specify their own LayoutAnimationRate value.  
| LayoutAnimator | Gets or sets the LayoutAnimator property. This dependency property indicates the default layout animator that will be used by panels within the Layouts collection that do not explicitly specify their own LayoutAnimator value.  
| Layouts | Gets the Layouts property. This dependency property contains a collection of SwitchablePanel objects that represent the different layouts available within the SwitchPanel.  
| ScrollOwner | Gets or sets the scroll owner.
| SwitchAnimationRate | Gets or sets the SwitchAnimationRate property. This dependency property indicates the default animation rate that will be used by panels within the Layouts collection that do not explicitly specify their own SwitchAnimationRate value.  
| SwitchAnimator | Gets or sets the SwitchAnimator property. This dependency property indicates the default switch animator that will be used by panels within the Layouts collection that do not explicitly specify their own SwitchAnimator value.  
| SwitchTemplate | Gets or sets the SwitchTemplate property. This dependency property indicates the switch template that should be used by any SwitchPresenter descendants.  
| TemplateAnimationRate | Gets or sets the TemplateAnimationRate property. This dependency property indicates the default animation rate that will be used by panels within the Layouts collection that do not explicitly specify their own TemplateAnimationRate value.  
| TemplateAnimator | Gets or sets the TemplateAnimator property. This dependency property indicates the default switch animator that will be used by panels within the Layouts collection that do not explicitly specify their own TemplateAnimator value.  
| VerticalOffset | Gets the vertical offset. 
| ViewportHeight | Gets the viewport's height.
| ViewportWidth | Gets the viewport's width.

**Support this project, check out the [Plus Edition](https://xceed.com/xceed-toolkit-plus-for-wpf/).**
---