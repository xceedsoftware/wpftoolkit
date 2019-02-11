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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;

namespace Xceed.Wpf.DataGrid
{
  public class DataCell : Cell
  {
    static DataCell()
    {
    }

    public DataCell()
    {
    }

    public DataCell( string fieldName, object content )
    {
      this.FieldName = fieldName;
      this.Content = content;
    }

    #region CanBeRecycled Property

    protected internal override bool CanBeRecycled
    {
      get
      {
        return ( m_canBeRecycled )
            && ( base.CanBeRecycled );
      }
    }

    private bool m_canBeRecycled; //= false

    #endregion

    #region OverrideColumnCellContentTemplate Property

    protected override bool OverrideColumnCellContentTemplate
    {
      get
      {
        return false;
      }
    }

    #endregion


    protected override void OnMouseEnter( MouseEventArgs e )
    {
      //If the current CellEditorDisplayConditions requires display when mouse is over the Cell 
      if( Cell.IsCellEditorDisplayConditionsSet( this, CellEditorDisplayConditions.MouseOverCell ) )
      {
        //Display the editors for the Row
        this.SetDisplayEditorMatchingCondition( CellEditorDisplayConditions.MouseOverCell );
      }

      base.OnMouseEnter( e );
    }

    protected override void OnMouseLeave( MouseEventArgs e )
    {
      //If the current CellEditorDisplayConditions requires display when mouse is over the Cell 
      if( Cell.IsCellEditorDisplayConditionsSet( this, CellEditorDisplayConditions.MouseOverCell ) )
      {
        //Display the editors for the Row
        this.RemoveDisplayEditorMatchingCondition( CellEditorDisplayConditions.MouseOverCell );
      }

      base.OnMouseLeave( e );
    }

    protected override void InitializeCore( DataGridContext dataGridContext, Row parentRow, ColumnBase parentColumn )
    {
      ColumnBase oldParentColumn = this.ParentColumn;

      base.InitializeCore( dataGridContext, parentRow, parentColumn );

      //
      // For an unknown reason, when a recycled DataCell was added back to the VisualTree (added
      // to the CellsHostPanel in its ParentRow), the binding would fail to update when the XPath
      // expression contained a namespace prefix, even if the XmlNamespaceManager property is inherited
      // and querying for the value of this property after adding the DataCell to the VTree would return
      // a valid, non-null XmlNamespaceManager.
      //
      // Forcing a local value for the XmlNamespaceManager property solves this problem, but it is
      // not the best thing to do as we are effectively bypassing the inheritance behavior for this
      // property...
      this.SetValue( Binding.XmlNamespaceManagerProperty, dataGridContext.DataGridControl.GetValue( Binding.XmlNamespaceManagerProperty ) );

      //prevent the setup of the display member binding more than once on the same column!
      if( ( !this.IsInternalyInitialized ) || ( oldParentColumn != parentColumn ) )
      {
        //call the helper function to setup the Cell's binding.
        this.SetupDisplayMemberBinding( dataGridContext );
      }
    }

    protected internal override void PrepareContainer( DataGridContext dataGridContext, object item )
    {
      base.PrepareContainer( dataGridContext, item );

      if( dataGridContext.SelectedCellsStore.Contains( DataGridVirtualizingPanel.GetItemIndex( this.ParentRow ), this.ParentColumn.VisiblePosition ) )
      {
        this.SetIsSelected( true );
      }
    }

    protected internal override void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      var newThemeKey = view.GetDefaultStyleKey( typeof( DataCell ) );
      if( object.Equals( this.DefaultStyleKey, newThemeKey ) )
        return;

      this.DefaultStyleKey = newThemeKey;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters" )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    protected internal virtual void SetupDisplayMemberBinding( DataGridContext dataGridContext )
    {
      // Bind the cell content.
      var column = this.ParentColumn as Column;

      if( column != null )
      {
        var displayMemberBinding = default( BindingBase );
        var dataItem = this.ParentRow.DataContext;

        // If the dataContext is our ParentRow, we do not create any binding
        if( dataItem != this.ParentRow )
        {
          displayMemberBinding = column.GetDisplayMemberBinding();

          if( displayMemberBinding == null )
          {
            if( dataGridContext == null )
              throw new InvalidOperationException( "An attempt was made to create a DisplayMemberBinding before the DataGridContext has been initialized." );

            if( !DesignerProperties.GetIsInDesignMode( this ) )
            {
              var propertyDescription = ItemsSourceHelper.CreateOrGetPropertyDescriptionFromColumn( dataGridContext, column, ( dataItem != null ) ? dataItem.GetType() : null );
              ItemsSourceHelper.UpdateColumnFromPropertyDescription( column, dataGridContext.DataGridControl.DefaultCellEditors, dataGridContext.AutoCreateForeignKeyConfigurations, propertyDescription );

              displayMemberBinding = column.GetDisplayMemberBinding();
            }

            column.IsBindingAutoCreated = true;
          }
        }

        if( displayMemberBinding != null )
        {
          m_canBeRecycled = DataCell.VerifyDisplayMemberBinding( displayMemberBinding );

          BindingOperations.SetBinding( this, Cell.ContentProperty, displayMemberBinding );

          var xmlElement = this.GetValue( Cell.ContentProperty ) as XmlElement;
          if( xmlElement != null )
          {


            // Convert binding to an InnerXML binding in the case we are bound on a XmlElement
            // to be able to refresh the data in the XML.

            //under any circumstances, a cell that is bound to XML cannot be recycled
            m_canBeRecycled = false;

            this.ClearDisplayMemberBinding();

            var xmlElementBinding = new Binding( "InnerXml" );
            xmlElementBinding.Source = xmlElement;
            xmlElementBinding.Mode = BindingMode.TwoWay;
            xmlElementBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;

            BindingOperations.SetBinding( this, Cell.ContentProperty, xmlElementBinding );
          }
        }
        else
        {
          this.ClearDisplayMemberBinding();
        }
      }
      else
      {
        this.ClearDisplayMemberBinding();
      }
    }

    internal virtual void ClearDisplayMemberBinding()
    {
      if( BindingOperations.IsDataBound( this, Cell.ContentProperty ) )
      {
        BindingOperations.ClearBinding( this, Cell.ContentProperty );
      }
      else
      {
        this.ClearValue( Cell.ContentProperty );
      }
    }

    internal override void ContentCommitted()
    {
      base.ContentCommitted();
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );
      DataRow parentRow = this.ParentRow as DataRow;

      if( ( dataGridContext != null ) && ( parentRow != null ) )
        parentRow.EnsurePosition( dataGridContext, this );
    }

    internal override DataTemplate GetForeignKeyDataTemplate()
    {
      var column = this.ParentColumn as Column;
      if( column == null )
        return null;

      // If a foreignKey CellContentTemplate was found by the configuration, it must be used even if a CellContentTemplate is defined
      // because the CellContentTemplate will be used by this template
      var configuration = column.ForeignKeyConfiguration;
      if( configuration == null )
        return null;

      return configuration.DefaultCellContentTemplate;
    }

    internal override DataTemplate GetCellStringFormatDataTemplate( DataTemplate contentTemplate )
    {
      // parentColumn is verified to be not null in the calling method
      var parentColumn = this.ParentColumn;

      var format = parentColumn.CellContentStringFormat;
      Debug.Assert( !string.IsNullOrEmpty( format ) );

      return StringFormatDataTemplate.Get( contentTemplate, format, parentColumn.GetCulture() );
    }

    private static bool VerifyDisplayMemberBinding( BindingBase binding )
    {
      bool retval = false;
      //a DataCell can only be recycled if the DisplayMemberBinding is of type Binding
      //and have no source, relativesource, elementname

      Binding displayMemberBinding = binding as Binding;

      if( displayMemberBinding != null )
      {
        if( ( ( displayMemberBinding.Source == null ) || ( displayMemberBinding.Source == DependencyProperty.UnsetValue ) )
          && ( ( displayMemberBinding.RelativeSource == null ) || ( displayMemberBinding.Source == DependencyProperty.UnsetValue ) )
          && ( string.IsNullOrEmpty( displayMemberBinding.ElementName ) == true ) )
        {
          retval = true;
        }
      }

      return retval;
    }
  }
}
