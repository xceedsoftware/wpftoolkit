/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  [TemplatePart( Name = "PART_GroupLevelIndicatorHost", Type = typeof( Panel ) )]
  public class GroupLevelIndicatorPane : Control, IWeakEventListener
  {
    static GroupLevelIndicatorPane()
    {
      GroupLevelIndicatorPane.IsLeafProperty = GroupLevelIndicatorPane.IsLeafPropertyKey.DependencyProperty;
      GroupLevelIndicatorPane.CurrentIndicatorCountProperty = GroupLevelIndicatorPane.CurrentIndicatorCountPropertyKey.DependencyProperty;

      FocusableProperty.OverrideMetadata( typeof( GroupLevelIndicatorPane ), new FrameworkPropertyMetadata( false ) );

      DataGridControl.DataGridContextPropertyKey.OverrideMetadata( typeof( GroupLevelIndicatorPane ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnDataGridContextChanged ) ) );
      CustomItemContainerGenerator.DataItemPropertyProperty.OverrideMetadata( typeof( GroupLevelIndicatorPane ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnDataItemChanged ) ) );

      AreGroupsFlattenedBinding = new Binding();
      AreGroupsFlattenedBinding.Mode = BindingMode.OneWay;
      AreGroupsFlattenedBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

      AreGroupsFlattenedBinding.Path = new PropertyPath( "(0).(1)",
        DataGridControl.DataGridContextProperty,
        TableflowView.AreGroupsFlattenedProperty );
    }

    public GroupLevelIndicatorPane()
    {
      this.SetBinding( GroupLevelIndicatorPane.AreGroupsFlattenedProperty, GroupLevelIndicatorPane.AreGroupsFlattenedBinding );
    }

    #region AreGroupsFlattened Internal Property

    internal static readonly DependencyProperty AreGroupsFlattenedProperty = DependencyProperty.Register(
      "AreGroupsFlattened", typeof( bool ), typeof( GroupLevelIndicatorPane ),
      new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.AffectsMeasure ) );

    internal bool AreGroupsFlattened
    {
      get
      {
        return ( bool )this.GetValue( GroupLevelIndicatorPane.AreGroupsFlattenedProperty );
      }
      set
      {
        this.SetValue( GroupLevelIndicatorPane.AreGroupsFlattenedProperty, value );
      }
    }

    private static Binding AreGroupsFlattenedBinding;

    #endregion AreGroupsFlattened Internal Property

    #region GroupLevelIndicatorPaneHost Read-Only Property

    private Panel GroupLevelIndicatorPaneHost
    {
      get
      {
        //if there is no local storage for the host panel, try to retrieve and store the value
        if( m_storedGroupLevelIndicatorPaneHost == null )
        {
          m_storedGroupLevelIndicatorPaneHost = this.RetrieveGroupLevelIndicatorPaneHostPanel();
        }

        return m_storedGroupLevelIndicatorPaneHost;
      }
    }

    private Panel m_storedGroupLevelIndicatorPaneHost; //null

    #endregion GroupLevelIndicatorPaneHost Read-Only Property

    #region ShowIndicators Attached Property

    public static readonly DependencyProperty ShowIndicatorsProperty = DependencyProperty.RegisterAttached(
      "ShowIndicators", typeof( bool ), typeof( GroupLevelIndicatorPane ),
      new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure ) );

    public static bool GetShowIndicators( DependencyObject obj )
    {
      return ( bool )obj.GetValue( GroupLevelIndicatorPane.ShowIndicatorsProperty );
    }

    public static void SetShowIndicators( DependencyObject obj, bool value )
    {
      obj.SetValue( GroupLevelIndicatorPane.ShowIndicatorsProperty, value );
    }

    #endregion ShowIndicators Attached Property

    #region ShowVerticalBorder Attached Property

    public static readonly DependencyProperty ShowVerticalBorderProperty =
      DependencyProperty.RegisterAttached( "ShowVerticalBorder", typeof( bool ), typeof( GroupLevelIndicatorPane ),
      new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.AffectsMeasure ) );

    public static bool GetShowVerticalBorder( DependencyObject obj )
    {
      return ( bool )obj.GetValue( ShowVerticalBorderProperty );
    }

    public static void SetShowVerticalBorder( DependencyObject obj, bool value )
    {
      obj.SetValue( ShowVerticalBorderProperty, value );
    }

    #endregion

    #region Indented Property

    public static readonly DependencyProperty IndentedProperty =
        DependencyProperty.Register( "Indented", typeof( bool ), typeof( GroupLevelIndicatorPane ),
        new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.AffectsMeasure ) );

    public bool Indented
    {
      get
      {
        return ( bool )this.GetValue( GroupLevelIndicatorPane.IndentedProperty );
      }
      set
      {
        this.SetValue( GroupLevelIndicatorPane.IndentedProperty, value );
      }
    }

    #endregion Indented Property

    #region GroupLevel Attached Property

    public static readonly DependencyProperty GroupLevelProperty = DependencyProperty.RegisterAttached(
      "GroupLevel", typeof( int ), typeof( GroupLevelIndicatorPane ),
      new FrameworkPropertyMetadata( 0, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure ) );

    public static int GetGroupLevel( DependencyObject obj )
    {
      return ( int )obj.GetValue( GroupLevelIndicatorPane.GroupLevelProperty );
    }

    public static void SetGroupLevel( DependencyObject obj, int value )
    {
      obj.SetValue( GroupLevelIndicatorPane.GroupLevelProperty, value );
    }

    #endregion GroupLevel Attached Property

    #region IsLeaf Read-Only Property

    internal static readonly DependencyPropertyKey IsLeafPropertyKey =
      DependencyProperty.RegisterReadOnly( "IsLeaf", typeof( bool ), typeof( GroupLevelIndicatorPane ),
      new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.AffectsMeasure ) );

    public static readonly DependencyProperty IsLeafProperty;

    public bool IsLeaf
    {
      get
      {
        return ( bool )this.GetValue( GroupLevelIndicatorPane.IsLeafProperty );
      }
    }

    internal void SetIsLeaf( bool value )
    {
      this.SetValue( GroupLevelIndicatorPane.IsLeafPropertyKey, value );
    }

    #endregion IsLeaf Read-Only Property

    #region CurrentIndicatorCount Read-Only Property

    private static readonly DependencyPropertyKey CurrentIndicatorCountPropertyKey =
        DependencyProperty.RegisterReadOnly( "CurrentIndicatorCount", typeof( int ), typeof( GroupLevelIndicatorPane ), new PropertyMetadata( 0 ) );

    public static readonly DependencyProperty CurrentIndicatorCountProperty;

    public int CurrentIndicatorCount
    {
      get
      {
        return ( int )this.GetValue( GroupLevelIndicatorPane.CurrentIndicatorCountProperty );
      }
    }

    internal void SetCurrentIndicatorCount( int value )
    {
      this.SetValue( GroupLevelIndicatorPane.CurrentIndicatorCountPropertyKey, value );
    }

    #endregion CurrentIndicatorCount Read-Only Property

    internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( GroupLevelIndicatorPane ) );
    }

    protected override Size MeasureOverride( Size availableSize )
    {
      var panel = this.GroupLevelIndicatorPaneHost;
      var dataGridContext = DataGridControl.GetDataGridContext( this );

      if( ( panel == null ) || ( dataGridContext == null ) )
        return base.MeasureOverride( availableSize );

      var groupDescriptions = DataGridContext.GetGroupDescriptionsHelper( dataGridContext.Items );
      var leafGroupLevel = GroupLevelIndicatorPane.GetGroupLevel( this );

      // If Indented is true (default), we use the total groupDescriptions.Count for this DataGridContext
      var correctedGroupLevel = ( this.Indented == true ) ? groupDescriptions.Count : leafGroupLevel;

      // Ensure that the GroupLevel retrieved does not exceeds the number of group descriptions for the DataGridContext
      correctedGroupLevel = Math.Min( correctedGroupLevel, groupDescriptions.Count );

      // Then finally, if the GroupLevel is -1, then indent at maximum.
      if( correctedGroupLevel == -1 )
      {
        correctedGroupLevel = groupDescriptions.Count;
      }

      if( ( correctedGroupLevel > 0 ) && ( this.AreGroupsFlattened ) )
      {
        correctedGroupLevel = ( this.Indented ) ? 1 : 0;
      }

      var children = panel.Children;
      var childrenCount = children.Count;

      // If we need to add/remove GroupLevelIndicators from the panel
      if( correctedGroupLevel != childrenCount )
      {
        // When grouping change, we take for granted that the group deepness will change, 
        // so we initialize DataContext of the margin only in there.

        // Clear all the panel's children!
        children.Clear();

        // Create 1 group margin content presenter for each group level
        for( int i = correctedGroupLevel - 1; i >= 0; i-- )
        {
          GroupLevelIndicator groupMargin = new GroupLevelIndicator();
          groupMargin.DataContext = dataGridContext.GroupLevelDescriptions[ i ];
          children.Insert( 0, new GroupLevelIndicator() );
        }

        childrenCount = correctedGroupLevel;
        this.SetCurrentIndicatorCount( childrenCount );

        this.InvalidateGroupLevelIndicatorPaneHostPanelMeasure();
      }

      var item = dataGridContext.GetItemFromContainer( this );

      for( int i = 0; i < childrenCount; i++ )
      {
        GroupLevelIndicator groupMargin = children[ i ] as GroupLevelIndicator;

        CollectionViewGroup groupForIndicator = GroupLevelIndicatorPane.GetCollectionViewGroupHelper(
          dataGridContext, groupDescriptions, item, i );

        GroupConfiguration groupLevelConfig = GroupConfiguration.GetGroupConfiguration(
          dataGridContext, groupDescriptions, dataGridContext.GroupConfigurationSelector, i, groupForIndicator );

        if( groupLevelConfig != null )
        {
          Binding groupLevelIndicatorStyleBinding = BindingOperations.GetBinding( groupMargin, GroupLevelIndicator.StyleProperty );

          if( ( groupLevelIndicatorStyleBinding == null ) || ( groupLevelIndicatorStyleBinding.Source != groupLevelConfig ) )
          {
            groupLevelIndicatorStyleBinding = new Binding( "GroupLevelIndicatorStyle" );
            groupLevelIndicatorStyleBinding.Source = groupLevelConfig;

            // Use a Converter to manage groupLevelConfig.GroupLevelIndicatorStyle == null
            // so that an implicit syle won't be overriden by a null style.
            groupLevelIndicatorStyleBinding.Converter = new GroupLevelIndicatorConverter();

            groupLevelIndicatorStyleBinding.ConverterParameter = groupMargin;
            groupMargin.SetBinding( GroupLevelIndicator.StyleProperty, groupLevelIndicatorStyleBinding );
          }
        }
        else
        {
          groupMargin.ClearValue( GroupLevelIndicator.StyleProperty );
        }

        // If the ShowIndicators property is False or there is already leafGroupLevel GroupLevelIndicators in the panel,
        // the current newGroupMargin must be hidden.
        if( ( !GroupLevelIndicatorPane.GetShowIndicators( this ) ) || ( ( i >= leafGroupLevel ) && ( leafGroupLevel != -1 ) ) )
        {
          groupMargin.Visibility = Visibility.Hidden;
        }
        else
        {
          groupMargin.Visibility = Visibility.Visible;
        }
      }

      return base.MeasureOverride( availableSize );
    }

    private static CollectionViewGroup GetCollectionViewGroupHelper( DataGridContext dataGridContext, ObservableCollection<GroupDescription> groupDescriptions, object item, int groupLevel )
    {
      if( item == null )
        return null;

      int levelOfRecursion = groupDescriptions.Count - groupLevel - 1;

      CollectionViewGroup retval = dataGridContext.GetParentGroupFromItemCore( item, true );

      if( retval == null )
        return null;

      for( int i = 0; i < levelOfRecursion; i++ )
      {
        retval = dataGridContext.GetParentGroupFromItemCore( retval, true ) as CollectionViewGroup;

        if( retval == null )
          return null;
      }

      return retval;
    }

    private static void OnDataGridContextChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      GroupLevelIndicatorPane self = sender as GroupLevelIndicatorPane;

      if( self != null )
      {
        DataGridContext dataGridContext = e.OldValue as DataGridContext;

        //unregister to the old contexts Collection GroupDescriptions Changed event
        if( dataGridContext != null )
        {
          CollectionChangedEventManager.RemoveListener( dataGridContext.Items.GroupDescriptions, self );
        }

        dataGridContext = e.NewValue as DataGridContext;

        //register to the new contexts Collection GroupDescriptions Changed event
        if( dataGridContext != null )
        {
          CollectionChangedEventManager.AddListener( dataGridContext.Items.GroupDescriptions, self );
          self.PrepareDefaultStyleKey( dataGridContext.DataGridControl.GetView() );
        }

        self.InvalidateMeasure();
      }
    }

    private static void OnDataItemChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = ( GroupLevelIndicatorPane )sender;
      Debug.Assert( self != null );

      var oldDataItemStore = ( DataItemDataProviderBase )e.OldValue;
      var newDataItemStore = ( DataItemDataProviderBase )e.NewValue;

      if( oldDataItemStore != null )
      {
        PropertyChangedEventManager.RemoveListener( oldDataItemStore, self, "Data" );
      }

      if( newDataItemStore != null )
      {
        PropertyChangedEventManager.AddListener( newDataItemStore, self, "Data" );
      }

      if( !newDataItemStore.IsEmpty )
      {
        self.InvalidateMeasure();
      }
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      //whenever the template gets "applied" I want to invalidate the stored Panel.
      m_storedGroupLevelIndicatorPaneHost = null;
    }

    protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnPropertyChanged( e );

      if( e.Property == GroupLevelIndicatorPane.GroupLevelProperty )
      {
        this.InvalidateMeasure();
      }
    }

    private Panel RetrieveGroupLevelIndicatorPaneHostPanel()
    {
      //get the template part
      return this.GetTemplateChild( "PART_GroupLevelIndicatorHost" ) as Panel;
    }

    private void InvalidateGroupLevelIndicatorPaneHostPanelMeasure()
    {
      UIElement container = this.GroupLevelIndicatorPaneHost;

      while( ( container != null ) && ( container != this ) )
      {
        container.InvalidateMeasure();
        container = VisualTreeHelper.GetParent( container ) as UIElement;
      }
    }

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( CollectionChangedEventManager ) )
      {
        this.InvalidateMeasure();
      }
      else if( managerType == typeof( PropertyChangedEventManager ) )
      {
        var dataItemStore = sender as DataItemDataProviderBase;
        if( ( dataItemStore != null ) && !dataItemStore.IsEmpty )
        {
          this.InvalidateMeasure();
        }
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    private class GroupLevelIndicatorConverter : IValueConverter
    {
      public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
      {
        // When groupLevelConfig.GroupLevelIndicatorStyle exists, return it.
        if( value != null )
          return value;

        // When groupLevelConfig.GroupLevelIndicatorStyle is null, try to find an implicit style
        // for the GroupLevelIndicator in the resources.
        var gli = ( GroupLevelIndicator )parameter;
        var style = gli.TryFindResource( gli.GetType() );
        if( style != null )
          return style;

        return value;
      }

      public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
      {
        throw new NotSupportedException();
      }
    }
  }
}
