# MultiCalendar
_Only available in the Plus Edition_

Derives from Control

The MultiCalendar combines X Microsoft Calendars that can be positioned in a grid with rows and columns. The selected dates, blackout dates and highlighted dates can be chosen from any calendars. The Background/Foreground of any CalendarDayButton can be easily customized based on its state(selected/highlighted/blackout/mouseOver/focused/today) and the background of any Calendar can be customized with an image or a brush.

## Properties
|| Property || Description
| BlackoutDates | Gets or sets the collection of dates that are marked as not selectable through all the calendars.
| CalendarButtonBlackoutDataTemplate | Gets or sets the dataTemplate to be displayed over each blackout dates.
| CalendarButtonCurrentDateBorderBrush | Gets or sets the border brush of the current date in the MultiCalendar control.
| CalendarButtonHighlightedBackground | Gets or sets the background brush of each highlighted dates in the MultiCalendar control.
| CalendarButtonHighlightedForeground | Gets or sets the foreground of each highlighted dates in the MultiCalendar control.
| CalendarButtonMouseOverBackground | Gets or sets the background brush of the date under the mouse in the MultiCalendar control.
| CalendarButtonMouseOverForeground | Gets or sets the foreground of the date under the mouse in the MultiCalendar control.
| CalendarButtonSelectedBackground | Gets or sets the background brush of each selected dates in the MultiCalendar control.
| CalendarButtonSelectedForeground | Gets or sets the foreground of each selected dates in the MultiCalendar control.
| CalendarButtonTodayBackground | Gets or sets the background brush of the today date in the MultiCalendar control.
| CalendarButtonTodayForeground | Gets or sets the foreground of the today date in the MultiCalendar control.
| CalendarDayTitleDataTemplate | Gets or sets the DataTemplate to use for the days of the week displayed in each calendar of the MultiCalendar control.
| CalendarHeaderButtonStyle | Gets or sets the style for the header of each calendar in the MultiCalendar control.
| CalendarsBackground | Gets or sets the background brush of each calendar in the MultiCalendar control. This background will be used when a calendar has no monthlyBackground associated.
| CalendarsBorderBrush | Gets or sets the border brush of each calendar in the MultiCalendar control.
| CalendarsBorderThickness | Gets or sets the border thickness of each calendar in the MultiCalendar control.
| CalendarsCollection | Gets the list of Calendars used in the MultiCalendar control.
| CalendarStyle | Gets or sets the style of the Calendars in the MultiCalendar control.
| ColumnCount | Gets or sets the number of columns to use to display the calendars of the MultiCalendar control. The value is adjusted automatically when the property MonthCount changes. However, as soon as a custom value is assigned, this custom value will remain and no longer be adjusted automatically. If the combination of the properties ColumnCount and RowCount is not big enough for the property MonthCount, the MonthCount will be modified to fit based on the ColumnCount/RowCount provided.
| DisplayDateEnd | Gets or sets the last day available in the MultiCalendar control. If the combination of the properties DisplayDateStart and DisplayDateEnd is not big enough for the property MonthCount, the MonthCount will be modified to fit based on the DisplayDateStart/DisplayDateEnd provided.
| DisplayDateStart | Gets or sets the first day available in the MultiCalendar control. If the combination of the properties DisplayDateStart and DisplayDateEnd is not big enough for the property MonthCount, the MonthCount will be modified to fit based on the DisplayDateStart/DisplayDateEnd provided.
| FirstDisplayedDate | Gets the first date that is currently displayed in the MultiCalendar control.
| HideInactiveDates | Gets or sets if the inactive dates will be displayed at beginning and end of each calendar in the MultiCalendar control. Default is true.
| HighlightedDates | Gets or sets the collection of dates that are marked as highlighted through all the calendars.
| MonthCount | Gets or sets the number of calendars to display in the MultiCalendar control. Default is 2. If values are not provided for propreties ColumnCount and RowCount, they will be filled with values based on MonthCount. If values are provided for properties ColumnCount and RowCount, and the MonthCount doesn't fit in the ColumnCount/RowCount because too big, the MonthCount will be modified to fit based on the ColumnCount/RowCount provided.
| MonthlyBackgrounds | Gets or sets the collection of monthly background data used to display the background of specific calendars in the MultiCalendar control.
| NextButtonStyle | Gets or sets the style to use for the button changing to next month in the MultiCalendar control.
| Orientation | Gets or sets the orientation of the calendars (Rows/Columns) in the MultiCalendar control. Default is Horizontal.
| PreviousButtonStyle | Gets or sets the style to use for the button changing to previous month in the MultiCalendar control.
| RowCount | Gets or sets the number of rows to use to display the calendars of the MultiCalendar control. The value is adjusted automatically when the property MonthCount changes. However, as soon as a custom value is assigned, this custom value will remain and no longer be adjusted automatically. If the combination of the properties ColumnCount and RowCount is not big enough for the property MonthCount, the MonthCount will be modified to fit based on the ColumnCount/RowCount provided.
| SelectedDate | Gets or sets the selected date in the MultiCalendar control.
| SelectedDates | Gets or sets the collection of dates that are marked as selected through all the calendars.
| SelectionMode | Gets or sets the selection mode in the MultiCalendar control. Default is SingleRange.

## Events
|| Event || Description
| BlackoutDatesChanged | Raised when the collection of blackout dates has a collectionChanged.
| HighlightedDatesChanged | Raised when the collection of highlighted dates has a collectionChanged.
| SelectedDateChanged | Raised when the selected date is changed.
| SelectedDatesChanged | Raised when the collection of selected dates has a collectionChanged.
---