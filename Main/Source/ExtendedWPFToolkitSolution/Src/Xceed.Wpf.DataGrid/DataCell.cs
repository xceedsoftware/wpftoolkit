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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xceed.Wpf.DataGrid.Views;
using Xceed.Wpf.DataGrid.Markup;

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
        return ( m_canBeRecycled && base.CanBeRecycled );
      }
    }

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

    protected override void InitializeCore( DataGridContext dataGridContext, Row parentRow, ColumnBase parentColumn )
    {
      ColumnBase oldParentColumn = this.ParentColumn;

      base.InitializeCore( dataGridContext, parentRow, parentColumn );

      // This is a fix for Case 106982 (could not bind to XML datasources that contained namespaces).
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
      else
      {

        BindingExpression binding = BindingOperations.GetBindingExpression( this, DataCell.ContentProperty );

        if( binding != null )
        {
          binding.UpdateTarget();
        }
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
      object currentThemeKey = view.GetDefaultStyleKey( typeof( DataCell ) );

      if( currentThemeKey.Equals( this.DefaultStyleKey ) == false )
      {
        this.DefaultStyleKey = currentThemeKey;
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters" )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    protected internal virtual void SetupDisplayMemberBinding( DataGridContext dataGridContext )
    {
      // Bind the cell content.
      Column column = this.ParentColumn as Column;

      if( column != null )
      {
        BindingBase displayMemberBinding = null;
        object dataContext = this.ParentRow.DataContext;

        // If the dataContext is our ParentRow, we do not create any binding
        if( dataContext != this.ParentRow )
        {

          // Disable warning for DisplayMemberBinding when internaly used
#pragma warning disable 618

          displayMemberBinding = column.DisplayMemberBinding;

#pragma warning restore 618


          if( displayMemberBinding == null )
          {
            if( ( dataGridContext == null ) || ( dataGridContext.ItemsSourceFieldDescriptors == null ) )
              throw new InvalidOperationException( "An attempt was made to create a DisplayMemberBinding before the DataGridContext has been initialized." );

            if( !DesignerProperties.GetIsInDesignMode( this ) )
            {
              bool isDataGridUnboundItemProperty;

              displayMemberBinding = ItemsSourceHelper.AutoCreateDisplayMemberBinding(
                column, dataGridContext, dataContext, out isDataGridUnboundItemProperty );

              column.IsBoundToDataGridUnboundItemProperty = isDataGridUnboundItemProperty;
            }

            // Disable warning for DisplayMemberBinding when internaly used
#pragma warning disable 618
            column.DisplayMemberBinding = displayMemberBinding;
#pragma warning restore 618

            //mark the Column's Binding as AutoCreated.
            column.IsBindingAutoCreated = true;
          }
        }

        if( displayMemberBinding != null )
        {
          m_canBeRecycled = DataCell.VerifyDisplayMemberBinding( displayMemberBinding );

          BindingOperations.SetBinding( this, Cell.ContentProperty, displayMemberBinding );

          XmlElement xmlElement =
            this.GetValue( Cell.ContentProperty ) as XmlElement;

          if( xmlElement != null )
          {


            // Convert binding to an InnerXML binding in the case we are bound on a XmlElement
            // to be able to refresh the data in the XML.

            //under any circumstances, a cell that is bound to XML cannot be recycled
            m_canBeRecycled = false;

            BindingOperations.ClearBinding( this, Cell.ContentProperty );
            this.ClearValue( Cell.ContentProperty );

            Binding xmlElementBinding = new Binding( "InnerXml" );
            xmlElementBinding.Source = xmlElement;
            xmlElementBinding.Mode = BindingMode.TwoWay;
            xmlElementBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;

            BindingOperations.SetBinding( this, Cell.ContentProperty, xmlElementBinding );
          }
        }
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

    private bool m_canBeRecycled; // = false
  }
}
