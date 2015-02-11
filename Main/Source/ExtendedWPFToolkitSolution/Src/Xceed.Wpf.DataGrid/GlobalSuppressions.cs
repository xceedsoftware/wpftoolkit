/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
  "Microsoft.Performance",
  "CA1823:AvoidUnusedPrivateFields",
  Scope = "member",
  Target = "_XceedVersionInfoCommon.Build" )]

[assembly: SuppressMessage(
  "Microsoft.Performance",
  "CA1823:AvoidUnusedPrivateFields",
  Scope = "member",
  Target = "_XceedVersionInfo.CurrentAssemblyPackUri" )]

[assembly: SuppressMessage( 
  "Microsoft.Design", 
  "CA1020:AvoidNamespacesWithFewTypes", 
  Scope = "namespace", 
  Target = "XamlGeneratedNamespace" )]

[assembly: SuppressMessage( 
  "Microsoft.Usage", 
  "CA2209:AssembliesShouldDeclareMinimumSecurity" )]

[assembly: SuppressMessage(
  "Microsoft.Design",
  "CA1020:AvoidNamespacesWithFewTypes",
  Scope = "namespace",
  Target = "Xceed.Wpf.DataGrid.ValidationRules" )]

[assembly: SuppressMessage( 
  "Microsoft.Usage", 
  "CA2233:OperationsShouldNotOverflow", 
  Scope = "member", 
  Target = "Xceed.Wpf.DataGrid.DataGridCollectionView.System.Collections.ICollection.CopyTo(System.Array,System.Int32):System.Void", 
  MessageId = "index+1" )]

[assembly: SuppressMessage( 
  "Microsoft.Design", 
  "CA1020:AvoidNamespacesWithFewTypes", 
  Scope = "namespace", 
  Target = "Xceed.Utils.Wpf.Markup" )]

[assembly: SuppressMessage( 
  "Microsoft.Design", 
  "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
  Scope = "member", 
  Target = "Xceed.Wpf.DataGrid.DataGridControl.System.Windows.Documents.IDocumentPaginatorSource.DocumentPaginator" )]

[assembly: SuppressMessage(
  "Microsoft.Design",
  "CA1043:UseIntegralOrStringArgumentForIndexers",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.CellCollection.Item[Xceed.Wpf.DataGrid.Column]" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA1801:ReviewUnusedParameters",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.DataGridCollectionView..ctor(System.Type)", MessageId = "itemType" )]

[assembly: SuppressMessage(
  "Microsoft.Performance",
  "CA1805:DoNotInitializeUnnecessarily",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.DetailConfiguration..ctor(Xceed.Wpf.DataGrid.DataGridContext)" )]

[assembly: SuppressMessage(
  "Microsoft.Performance",
  "CA1800:DoNotCastUnnecessarily",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.GroupByControl.Xceed.Utils.Wpf.DragDrop.IDropTarget.CanDropElement(System.Windows.UIElement):System.Boolean" )]

[assembly: SuppressMessage(
  "Microsoft.Performance",
  "CA1800:DoNotCastUnnecessarily",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.GroupByItem.Xceed.Utils.Wpf.DragDrop.IDropTarget.CanDropElement(System.Windows.UIElement):System.Boolean" )]

[assembly: SuppressMessage(
  "Microsoft.Design",
  "CA1011:ConsiderPassingBaseTypesAsParameters",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.Views.Theme.IsViewSupported(System.Type,System.Type):System.Boolean" )]

[assembly: SuppressMessage( 
  "Microsoft.Design", 
  "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
  Scope = "member", 
  Target = "Xceed.Wpf.DataGrid.GroupLevelIndicatorPane.System.Windows.IWeakEventListener.ReceiveWeakEvent(System.Type,System.Object,System.EventArgs):System.Boolean" )]

[assembly: SuppressMessage( 
  "Microsoft.Design", 
  "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
  Scope = "member", 
  Target = "Xceed.Wpf.DataGrid.HierarchicalGroupLevelIndicatorPane.System.Windows.IWeakEventListener.ReceiveWeakEvent(System.Type,System.Object,System.EventArgs):System.Boolean" )]

[assembly: SuppressMessage( 
  "Microsoft.Usage", 
  "CA1801:ReviewUnusedParameters", 
  Scope = "member", 
  Target = "Xceed.Wpf.DataGrid.DetailConfiguration.AddDefaultHeadersFooters(System.Collections.ObjectModel.ObservableCollection`1<System.Windows.DataTemplate>,System.Collections.ObjectModel.ObservableCollection`1<System.Windows.DataTemplate>):System.Void", MessageId = "footersCollection" )]

#region CA2214:DoNotCallOverridableMethodsInConstructors

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.Cell..ctor()" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.CellEditor..ctor()" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.Column..ctor()" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.Column..ctor(System.String,System.Object,System.Windows.Data.BindingBase)" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.ColumnManagerCell..ctor()" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.ColumnManagerRow..ctor()" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.DataCell..ctor(System.String,System.Object)" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.DataGridControl..ctor()" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.DropMarkAdorner..ctor(System.Windows.UIElement,System.Windows.Media.Pen,Xceed.Wpf.DataGrid.DropMarkOrientation)" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.Views.SynchronizedScrollViewer..ctor()" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.Views.ViewBase..ctor()" )]

[assembly: SuppressMessage( 
  "Microsoft.Usage", 
  "CA2214:DoNotCallOverridableMethodsInConstructors", 
  Scope = "member", 
  Target = "Xceed.Wpf.DataGrid.GroupHeaderControl..ctor(Xceed.Wpf.DataGrid.Group)" )]

[assembly: SuppressMessage( 
  "Microsoft.Usage", 
  "CA2214:DoNotCallOverridableMethodsInConstructors", 
  Scope = "member", 
  Target = "Xceed.Wpf.DataGrid.Views.FixedCellPanel..ctor()" )]

[assembly: SuppressMessage( 
  "Microsoft.Usage", 
  "CA2214:DoNotCallOverridableMethodsInConstructors", 
  Scope = "member", 
  Target = "Xceed.Wpf.DataGrid.Views.ScrollingCellsDecorator..ctor(Xceed.Wpf.DataGrid.Views.FixedCellPanel)" )]

[assembly: SuppressMessage( 
  "Microsoft.Usage", 
  "CA2214:DoNotCallOverridableMethodsInConstructors", 
  Scope = "member", 
  Target = "Xceed.Wpf.DataGrid.Views.DataGridScrollViewer..ctor()" )]

[assembly: SuppressMessage( 
  "Microsoft.Usage", 
  "CA2214:DoNotCallOverridableMethodsInConstructors", 
  Scope = "member", 
  Target = "Xceed.Wpf.DataGrid.GroupConfiguration..ctor()" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.DataGridContext..ctor(Xceed.Wpf.DataGrid.DataGridContext,Xceed.Wpf.DataGrid.DataGridControl,System.Object,System.Windows.Data.CollectionView,Xceed.Wpf.DataGrid.DetailConfiguration)" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.DetailConfiguration..ctor(System.Boolean)" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.DetailConfiguration..ctor(Xceed.Wpf.DataGrid.DataGridContext)" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.SaveRestoreStateVisitor..ctor()" )]

[assembly: SuppressMessage(
  "Microsoft.Usage",
  "CA2214:DoNotCallOverridableMethodsInConstructors",
  Scope = "member",
  Target = "Xceed.Wpf.DataGrid.DefaultDetailConfiguration..ctor()" )]

#endregion CA2214:DoNotCallOverridableMethodsInConstructors
