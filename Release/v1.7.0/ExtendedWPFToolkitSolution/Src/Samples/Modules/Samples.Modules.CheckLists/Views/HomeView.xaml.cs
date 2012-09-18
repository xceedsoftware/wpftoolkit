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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Controls;

namespace Samples.Modules.CheckLists.Views
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
      this.DataContext = new List<Person>()
      {
        new Person(){ID=101, FirstName="John", LastName="Smith"},
        new Person(){ID=102, FirstName="Janel", LastName="Leverling"},
        new Person(){ID=103, FirstName="Laura", LastName="Callahan"},
        new Person(){ID=104, FirstName="Robert", LastName="King"},
        new Person(){ID=105, FirstName="Margaret", LastName="Peacock"},
        new Person(){ID=106, FirstName="Andrew", LastName="Fuller"},
        new Person(){ID=107, FirstName="Anne", LastName="Dodsworth"},
        new Person(){ID=108, FirstName="Nancy", LastName="Davolio"},
        new Person(){ID=109, FirstName="Naomi", LastName="Suyama"},
      };
    }

  }

  public class Person : INotifyPropertyChanged
  {
    private bool _isSelected;
    private int _ID;
    private string _firstName;
    private string _lastName;

    public bool IsSelected
    {
      get
      {
        return _isSelected;
      }
      set
      {
        _isSelected = value;
        OnPropertyChanged( "IsSelected" );
      }
    }

    public int ID
    {
      get
      {
        return _ID;
      }
      set
      {
        _ID = value;
        OnPropertyChanged( "ID" );
      }
    }

    public string FirstName
    {
      get
      {
        return _firstName;
      }
      set
      {
        _firstName = value;
        OnPropertyChanged( "FirstName" );
      }
    }

    public string LastName
    {
      get
      {
        return _lastName;
      }
      set
      {
        _lastName = value;
        OnPropertyChanged( "LastName" );
      }
    }

    public string ModelDisplay
    {
      get
      {
        string completeName = string.Format("{0} {1}", FirstName, LastName).PadRight(20);
        return string.Format(
          "ID={0}: Name= {1}, IsSelected= {2}",
          ID,
          completeName,
          IsSelected );
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged( string propertyName )
    {
      if( PropertyChanged != null )
      {
        PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        PropertyChanged( this, new PropertyChangedEventArgs( "ModelDisplay" ) );
      }
    }
  }
}
