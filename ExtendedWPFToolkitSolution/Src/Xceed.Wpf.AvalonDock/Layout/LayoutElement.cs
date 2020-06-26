/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Xceed.Wpf.AvalonDock.Layout
{
  [Serializable]
  public abstract class LayoutElement : DependencyObject, ILayoutElement
  {
    #region Members

    [NonSerialized]
    private ILayoutContainer _parent = null;
    [NonSerialized]
    private ILayoutRoot _root = null;

    #endregion

    #region Constructors

    internal LayoutElement()
    {
    }

    #endregion

    #region Properties

    #region Parent

    [XmlIgnore]
    public ILayoutContainer Parent
    {
      get
      {
        return _parent;
      }
      set
      {
        if( _parent != value )
        {
          ILayoutContainer oldValue = _parent;
          ILayoutRoot oldRoot = _root;
          RaisePropertyChanging( "Parent" );
          OnParentChanging( oldValue, value );
          _parent = value;
          OnParentChanged( oldValue, value );

          _root = Root;
          if( oldRoot != _root )
            OnRootChanged( oldRoot, _root );

          RaisePropertyChanged( "Parent" );

          var root = Root as LayoutRoot;
          if( root != null )
            root.FireLayoutUpdated();
        }
      }
    }

    #endregion

    #region Root

    public ILayoutRoot Root
    {
      get
      {
        var parent = Parent;

        while( parent != null && ( !( parent is ILayoutRoot ) ) )
        {
          parent = parent.Parent;
        }

        return parent as ILayoutRoot;
      }
    }

    #endregion

    #endregion

    #region Public Methods

#if TRACE
        public virtual void ConsoleDump(int tab)
        {
          System.Diagnostics.Trace.Write( new String( ' ', tab * 4 ) );
          System.Diagnostics.Trace.WriteLine( this.ToString() );
        }
#endif

    #endregion

    #region Internal Methods

    /// <summary>
    /// Provides derived classes an opportunity to handle execute code before to the Parent property changes.
    /// </summary>
    protected virtual void OnParentChanging( ILayoutContainer oldValue, ILayoutContainer newValue )
    {
    }

    /// <summary>
    /// Provides derived classes an opportunity to handle changes to the Parent property.
    /// </summary>
    protected virtual void OnParentChanged( ILayoutContainer oldValue, ILayoutContainer newValue )
    {
    }


    protected virtual void OnRootChanged( ILayoutRoot oldRoot, ILayoutRoot newRoot )
    {
      if( oldRoot != null )
        ( ( LayoutRoot )oldRoot ).OnLayoutElementRemoved( this );
      if( newRoot != null )
        ( ( LayoutRoot )newRoot ).OnLayoutElementAdded( this );
    }

    protected virtual void RaisePropertyChanged( string propertyName )
    {
      if( PropertyChanged != null )
        PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
    }

    protected virtual void RaisePropertyChanging( string propertyName )
    {
      if( PropertyChanging != null )
        PropertyChanging( this, new System.ComponentModel.PropertyChangingEventArgs( propertyName ) );
    }

    #endregion

    #region Events

    [field: NonSerialized]
    [field: XmlIgnore]
    public event PropertyChangedEventHandler PropertyChanged;

    [field: NonSerialized]
    [field: XmlIgnore]
    public event PropertyChangingEventHandler PropertyChanging;

    #endregion
  }
}
