/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Controls;

namespace Samples.Modules.Text.Views
{
  /// <summary>
  /// Interaction logic for HomeView.xaml
  /// </summary>
  [RegionMemberLifetime( KeepAlive = false )]
  public partial class HomeView : DemoView
  {
    public HomeView()
    {
      InitializeComponent();

      _textBox.Text = "1;2;"; //is of object ids
      _textBox.ItemsSource = new List<Email>()
        {
            new Email() { Id = 1, FirstName = "John", LastName = "Doe", EmailAddress = "john@test.com" },
            new Email() { Id = 2, FirstName = "Jane", LastName = "Doe", EmailAddress = "jane@test.com" },
        };
    }

    public class Email
    {
      public int Id
      {
        get;
        set;
      }
      public string FirstName
      {
        get;
        set;
      }
      public string LastName
      {
        get;
        set;
      }
      public string EmailAddress
      {
        get;
        set;
      }

      public string FullName
      {
        get
        {
          return String.Format( "{0}, {1}", LastName, FirstName );
        }
      }
    }
  }
}
