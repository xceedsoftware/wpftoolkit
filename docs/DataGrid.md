# DataGrid

The datagrid included in Extended WPF Toolkit provides a stunning, shaded appearance and capabilities such as inertial smooth scrolling and animated full-column reordering—which mimics the physics of real-life movement. Add to that the datagrid’s zero-lag data virtualization, and you have the fastest WPF datagrid around—in performance and feel. It also easily handles millions of rows and thousands of columns, and integrates quickly into any WPF app.

This datagrid was the first datagrid for WPF. It was released in 2007 and has been consistently updated since then. There have been 68 major and minor as of April 2016. It is used in many major business applications, and is also used by portions of Visual Studio.

The datagrid control is contained in a separate assembly, Xceed.Wpf.DataGrid, which must be added to your project and then referenced where necessary.

Note: You can find complete documentation of the datagrid API [here](https://xceed.com/wp-content/documentation/xceed-toolkit-plus-for-wpf/webframe.html#Datagrid%20control.html). You can also find detailed descriptions of how the various classes work together in the [Xceed DataGrid for WPF documentation](https://xceed.com/wp-content/documentation/xceed-datagrid-for-wpf/webframe.html), but please bear in mind that that product's documentation covers features that may not be available in this Toolkit version.

See the [Advanced DataGrid](Advanced-DataGrid) page for a list of differences between the Toolkit's datagrid and the advanced edition, Xceed DataGrid for WPF.

![](DataGrid_grid.jpg)

## Usage

**XAML**
{{
<sample:DemoView x:Class="Samples.Modules.DataGrid.Views.HomeView"
                 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"  
                 xmlns:sample="clr-namespace:Samples.Infrastructure.Controls;assembly=Samples.Infrastructure"
                 xmlns:xctk="http://schemas.xceed.com/wpf/xaml/toolkit"
                 xmlns:xcdg="http://schemas.xceed.com/wpf/xaml/datagrid"
                 xmlns:compModel="clr-namespace:System.ComponentModel;assembly=WindowsBase"
                 xmlns:local="clr-namespace:Samples.Modules.DataGrid"
                 Title="DataGrid" 
                 x:Name="_demo">
    <sample:DemoView.Description>
        Extended WPF Toolkit DataGrid control sample.
    </sample:DemoView.Description>
    <Grid>
        <Grid.Resources>
            <xcdg:DataGridCollectionViewSource x:Key="cvsOrders"
                                            Source="{Binding ElementName=_demo, Path=Orders}">
                <xcdg:DataGridCollectionViewSource.GroupDescriptions>
                    <PropertyGroupDescription PropertyName="ShipCountry" />
                    <PropertyGroupDescription PropertyName="ShipCity" />
                </xcdg:DataGridCollectionViewSource.GroupDescriptions>
            </xcdg:DataGridCollectionViewSource>
        </Grid.Resources>

        <xcdg:DataGridControl x:Name="_dataGrid" 
                            MaxHeight="400"
                            ItemsSource="{Binding Source={StaticResource cvsOrders} }" >
            <xcdg:DataGridControl.View>
                <xcdg:TableflowView FixedColumnCount="2" />
            </xcdg:DataGridControl.View>

            <xcdg:DataGridControl.Columns>
                <!--Preconfigure the OrderID Column of the grid with CellValidationRule. -->
                <xcdg:Column FieldName="OrderID"
                         IsMainColumn="True">
                    <xcdg:Column.CellValidationRules>
                        <local:UniqueIDCellValidationRule />
                    </xcdg:Column.CellValidationRules>
                </xcdg:Column>
            </xcdg:DataGridControl.Columns>
        </xcdg:DataGridControl>
    </Grid>
</sample:DemoView>
}}

**Code behind**
{{
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Controls;
using Xceed.Wpf.DataGrid.Samples.SampleData;
using System.Data;
using Xceed.Wpf.DataGrid;

namespace Samples.Modules.DataGrid.Views
{
  /// <summary>
  /// Interaction logic for HomeView.xaml
  /// </summary>
  [RegionMemberLifetime( KeepAlive = false )](RegionMemberLifetime(-KeepAlive-=-false-))
  public partial class HomeView : DemoView
  {
    public HomeView()
    {
      this.Orders = DataProvider.GetNorthwindDataSet().Tables[ "Orders" ](-_Orders_-);
      InitializeComponent();
    }

    public DataTable Orders
    {
      get;
      private set;
    }

  }
}
}}

**Support this project, check out the [Plus Edition](https://xceed.com/xceed-toolkit-plus-for-wpf/).**
---