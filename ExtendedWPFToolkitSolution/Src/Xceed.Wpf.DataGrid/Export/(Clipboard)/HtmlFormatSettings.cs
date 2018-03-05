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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xceed.Wpf.DataGrid.Export
{
  public class HtmlFormatSettings : FormatSettingsBase
  {
    public HtmlFormatSettings()
    {
      this.ExporterStartDelimiter = "<TABLE>";
      this.ExporterEndDelimiter = "</TABLE>";

      this.HeaderFieldStartDelimiter = "<TH>";
      this.HeaderFieldEndDelimiter = "</TH>";
      this.HeaderDataStartDelimiter = "<TR>";
      this.HeaderDataEndDelimiter = "</TR>";

      this.FieldStartDelimiter = "<TD>";
      this.FieldEndDelimiter = "</TD>";
      this.DataStartDelimiter = "<TR>";
      this.DataEndDelimiter = "</TR>";
    }

    #region PUBLIC PROPERTIES

    public string ExporterStartDelimiter
    {
      get;
      set;
    }

    public string ExporterEndDelimiter
    {
      get;
      set;
    }

    public string HeaderFieldStartDelimiter
    {
      get;
      set;
    }

    public string HeaderFieldEndDelimiter
    {
      get;
      set;
    }

    public string HeaderDataStartDelimiter
    {
      get;
      set;
    }

    public string HeaderDataEndDelimiter
    {
      get;
      set;
    }

    public string FieldStartDelimiter
    {
      get;
      set;
    }

    public string FieldEndDelimiter
    {
      get;
      set;
    }

    public string DataStartDelimiter
    {
      get;
      set;
    }

    public string DataEndDelimiter
    {
      get;
      set;
    }

    #endregion
  }
}
