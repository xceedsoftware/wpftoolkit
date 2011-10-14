using System;
using Samples.Infrastructure.Controls;
using Microsoft.Practices.Prism.Regions;
using System.Collections.Generic;

namespace Samples.Modules.Text.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    [RegionMemberLifetime(KeepAlive = false)]
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
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string EmailAddress { get; set; }

            public string FullName
            {
                get
                {
                    return String.Format("{0}, {1}", LastName, FirstName);
                }
            }
        }
    }
}
