/***************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  *************************************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.CheckLists.Views
{
  /// <summary>
  /// Interaction logic for CheckListsView.xaml
  /// </summary>
  public partial class CheckListsView : DemoView
  {
    public CheckListsView()
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
