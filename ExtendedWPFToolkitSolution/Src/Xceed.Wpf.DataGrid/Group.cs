/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.DataGrid.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation;

namespace Xceed.Wpf.DataGrid
{
  public class Group : INotifyPropertyChanged
  {
    internal Group( GroupGeneratorNode node, CollectionViewGroup group, IList<GroupLevelDescription> groupLevelDescriptions, DataGridContext dataGridContext )
    {
      if( node == null )
        throw new ArgumentNullException( "node" );

      if( group == null )
        throw new ArgumentNullException( "group" );

      if( groupLevelDescriptions == null )
        throw new ArgumentNullException( "groupLevelDescriptions" );

      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      m_generatorNode = node;
      m_collectionViewGroup = group;
      m_groupDescriptionsCollection = groupLevelDescriptions;
      m_dataGridContext = dataGridContext;

      //In case no late bingding is to happen, and if we already have everything to get the groupLevelDescription immediately, let's do!
      int level = m_generatorNode.Level;
      if( m_groupDescriptionsCollection.Count > level )
      {
        m_groupLevelDescription = m_groupDescriptionsCollection[ level ];
      }
      m_lateGroupLevelDescriptionBindingPerformed = false;

      m_generatorNode.TotalLeafCountChanged += OnTotalItemCountChanged;
      m_generatorNode.IsExpandedChanged += OnIsExpandedChanged;
    }

    #region IsExpanded Property

    public bool IsExpanded
    {
      get
      {
        if( m_generatorNode != null )
          return m_generatorNode.IsExpanded;

        return false;
      }
      set
      {
        if( m_generatorNode != null )
          m_generatorNode.IsExpanded = value;
      }
    }

    internal bool IsComputedExpanded
    {
      get
      {
        if( m_generatorNode != null )
          return m_generatorNode.IsComputedExpanded;

        return false;
      }
    }

    #endregion IsExpanded Property

    #region Level Property

    public int Level
    {
      get
      {
        if( m_generatorNode != null )
          return m_generatorNode.Level;

        return -1;
      }
    }

    #endregion Level Property

    #region ItemCount Property

    public int ItemCount
    {
      get
      {
        if( m_generatorNode != null )
          return m_generatorNode.TotalLeafCount;

        return 0;
      }
    }

    #endregion ItemCount Property

    #region ParentGroups Property

    public ReadOnlyCollection<Group> ParentGroups
    {
      get
      {
        List<Group> list = new List<Group>();

        if( m_generatorNode != null )
        {
          GeneratorNodeHelper nodeHelper = new GeneratorNodeHelper( m_generatorNode, 0, 0 ); //index is not important

          while( nodeHelper.MoveToParent() )
          {
            GroupGeneratorNode parentGroup = nodeHelper.CurrentNode as GroupGeneratorNode;
            if( parentGroup != null )
            {
              list.Insert( 0, parentGroup.UIGroup );
            }
            else
            {
              break;
            }
          }
        }
        return list.AsReadOnly();
      }
    }

    #endregion ParentGroups Property

    #region SiblingGroups Property

    public ReadOnlyCollection<Group> SiblingGroups
    {
      get
      {
        if( m_generatorNode == null )
          return null;

        HeadersFootersGeneratorNode headersFootersGeneratorNode = HeadersFootersGeneratorNode.GetSameLevelFirstHeaderNode( m_generatorNode );

        if( headersFootersGeneratorNode == null )
          return null;

        int currentGeneratorContentGeneration = this.DataGridContext.CustomItemContainerGenerator.CurrentGeneratorContentGeneration;

        return headersFootersGeneratorNode.GetImmediateUIGroups( currentGeneratorContentGeneration );
      }
    }

    #endregion SiblingGroups

    #region Value Property

    public object Value
    {
      get
      {
        if( m_collectionViewGroup != null )
          return m_collectionViewGroup.Name;

        return null;
      }
    }

    #endregion Value Property

    #region ValueTemplate Property

    public DataTemplate ValueTemplate
    {
      get
      {
        this.PerformLateGroupLevelDescriptionBinding();

        if( m_groupLevelDescription != null )
        {
          return m_groupLevelDescription.ValueTemplate;
        }

        return null;
      }
    }

    #endregion ValueTemplate Property

    #region ValueTemplateSelector Property

    public DataTemplateSelector ValueTemplateSelector
    {
      get
      {
        this.PerformLateGroupLevelDescriptionBinding();

        if( m_groupLevelDescription != null )
        {
          return m_groupLevelDescription.ValueTemplateSelector;
        }

        return null;
      }
    }

    #endregion ValueTemplateSelector Property

    #region IsBottomLevel Property

    public bool IsBottomLevel
    {
      get
      {
        if( m_collectionViewGroup != null )
          return m_collectionViewGroup.IsBottomLevel;

        return true;
      }
    }

    #endregion IsBottomLevel Property

    #region Items Property [Obsoleted]

    [Obsolete( "The Items property is obsolete and has been replaced by the GroupExtensions.GetItems extensibility method.", false )]
    public ReadOnlyObservableCollection<object> Items
    {
      get
      {
        IList<object> items = this.GetItems();

        if( items is ReadOnlyObservableCollection<object> )
          return ( ReadOnlyObservableCollection<object> )items;

        return null;
      }
    }

    #endregion Items Property [Obsoleted]

    #region Title Property

    public object Title
    {
      get
      {
        this.PerformLateGroupLevelDescriptionBinding();

        if( m_groupLevelDescription != null )
        {
          return m_groupLevelDescription.Title;
        }

        return null;
      }
    }

    #endregion Title Property

    #region TitleTemplate Property

    public DataTemplate TitleTemplate
    {
      get
      {
        this.PerformLateGroupLevelDescriptionBinding();

        if( m_groupLevelDescription != null )
        {
          return m_groupLevelDescription.TitleTemplate;
        }

        return null;
      }
    }

    #endregion TitleTemplate Property

    #region TitleTemplateSelector Property

    public DataTemplateSelector TitleTemplateSelector
    {
      get
      {
        this.PerformLateGroupLevelDescriptionBinding();

        if( m_groupLevelDescription != null )
        {
          return m_groupLevelDescription.TitleTemplateSelector;
        }

        return null;
      }
    }

    #endregion TitleTemplateSelector Property

    #region StatContext Property

    public object StatContext
    {
      get
      {
        return m_collectionViewGroup as DataGridCollectionViewGroup;
      }
    }

    #endregion StatContext Property

    #region GroupConfiguration Property

    public GroupConfiguration GroupConfiguration
    {
      get
      {
        if( m_generatorNode != null )
          return m_generatorNode.GroupConfiguration;

        return null;
      }
    }

    #endregion GroupConfiguration Property

    #region GeneratorNode Property

    internal GroupGeneratorNode GeneratorNode
    {
      get
      {
        return m_generatorNode;
      }
    }

    #endregion GeneratorNode Property

    #region GroupBy Property

    internal string GroupBy
    {
      get
      {
        this.PerformLateGroupLevelDescriptionBinding();

        if( m_groupLevelDescription == null )
        {
          return string.Empty;
        }

        return m_groupLevelDescription.FieldName;
      }
    }

    #endregion GroupBy Property

    #region DataGridContext Property

    internal DataGridContext DataGridContext
    {
      get
      {
        return m_dataGridContext;
      }
    }

    #endregion DataGridContext Property

    internal DataGridGroupAutomationPeer CreateAutomationPeer()
    {
      return new DataGridGroupAutomationPeer( this );
    }

    internal void ClearGroup()
    {
      m_generatorNode.TotalLeafCountChanged -= OnTotalItemCountChanged;
      m_generatorNode.IsExpandedChanged -= OnIsExpandedChanged;

      if( m_groupLevelDescription != null )
      {
        m_groupLevelDescription.PropertyChanged -= GroupLevelDescriptionChangedHandler;
      }

      m_groupLevelDescription = null;
      m_generatorNode = null;
      m_collectionViewGroup = null;
    }

    internal CollectionViewGroup CollectionViewGroup
    {
      get
      {
        return m_collectionViewGroup;
      }
    }

    private void GroupLevelDescriptionChangedHandler( object sender, PropertyChangedEventArgs e )
    {
      // Simply relay this property changed to this Group instance.
      this.NotifyPropertyChanged( e );
    }

    private void OnTotalItemCountChanged( object sender, EventArgs e )
    {
      this.NotifyPropertyChanged( "ItemCount" );
    }

    private void OnIsExpandedChanged( object sender, EventArgs e )
    {
      this.NotifyPropertyChanged( "IsExpanded" );

      if( AutomationPeer.ListenerExists( AutomationEvents.PropertyChanged ) )
      {
        DataGridGroupAutomationPeer groupAutomationPeer = this.CreateAutomationPeer();

        ExpandCollapseState oldExpandCollapseState;
        ExpandCollapseState newExpandCollapseState;

        if( this.IsExpanded )
        {
          oldExpandCollapseState = ExpandCollapseState.Collapsed;
          newExpandCollapseState = ExpandCollapseState.Expanded;
        }
        else
        {
          oldExpandCollapseState = ExpandCollapseState.Expanded;
          newExpandCollapseState = ExpandCollapseState.Collapsed;
        }

        groupAutomationPeer.RaisePropertyChangedEvent(
          ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
          oldExpandCollapseState, newExpandCollapseState );
      }
    }

    private void PerformLateGroupLevelDescriptionBinding()
    {
      int level = this.Level;

      if( m_lateGroupLevelDescriptionBindingPerformed && this.ValidateGroupLevelDescription( level ) )
        return;

      m_lateGroupLevelDescriptionBindingPerformed = true;


      if( m_groupDescriptionsCollection.Count > level )
      {
        m_groupLevelDescription = m_groupDescriptionsCollection[ level ];
      }

      if( m_groupLevelDescription != null )
      {
        m_groupLevelDescription.PropertyChanged += new PropertyChangedEventHandler( GroupLevelDescriptionChangedHandler );

        this.NotifyPropertyChanged( "Title" );
        this.NotifyPropertyChanged( "TitleTemplate" );
        this.NotifyPropertyChanged( "TitleTemplateSelector" );
        this.NotifyPropertyChanged( "ValueTemplate" );
        this.NotifyPropertyChanged( "ValueTemplateSelector" );
      }
    }

    private bool ValidateGroupLevelDescription( int level )
    {
      //Returns false if the m_groupLevelDescription has changed since it was first initialized, and will be updated in the calling method.
      if( m_groupDescriptionsCollection.Count > level )
      {
        return ( m_groupLevelDescription == m_groupDescriptionsCollection[ level ] );
      }

      //If there is no GroupDescription corresponding to the level this group is at, then lets make sure we do not leak on the old GroupDescription
      if( m_groupLevelDescription != null )
      {
        m_groupLevelDescription.PropertyChanged -= new PropertyChangedEventHandler( GroupLevelDescriptionChangedHandler );
        m_groupLevelDescription = null;
      }

      //Return true because there is nothing else to do anyway.
      return true;
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged( String propertyName )
    {
      if( this.PropertyChanged != null )
      {
        this.PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
      }
    }

    private void NotifyPropertyChanged( PropertyChangedEventArgs e )
    {
      if( this.PropertyChanged != null )
      {
        this.PropertyChanged( this, e );
      }
    }

    #endregion INotifyPropertyChanged Members

    private GroupLevelDescription m_groupLevelDescription; // = null
    private GroupGeneratorNode m_generatorNode; // = null
    private CollectionViewGroup m_collectionViewGroup; // = null
    private IList<GroupLevelDescription> m_groupDescriptionsCollection; // = null
    private DataGridContext m_dataGridContext;
    private bool m_lateGroupLevelDescriptionBindingPerformed;
  }
}
