using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls
{
    public class WizardPage : ContentControl
    {
        #region Properties

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(WizardPage));
        public string Caption
        {
            get { return (string)base.GetValue(CaptionProperty); }
            set { base.SetValue(CaptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(WizardPage));
        public string Description
        {
            get { return (string)base.GetValue(DescriptionProperty); }
            set { base.SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty NextPageProperty = DependencyProperty.Register("NextPage", typeof(WizardPage), typeof(WizardPage), new UIPropertyMetadata(null));
        public WizardPage NextPage
        {
            get { return (WizardPage)GetValue(NextPageProperty); }
            set { SetValue(NextPageProperty, value); }
        }

        public static readonly DependencyProperty PreviousPageProperty = DependencyProperty.Register("PreviousPage", typeof(WizardPage), typeof(WizardPage), new UIPropertyMetadata(null));
        public WizardPage PreviousPage
        {
            get { return (WizardPage)GetValue(PreviousPageProperty); }
            set { SetValue(PreviousPageProperty, value); }
        }        

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(WizardPage));
        public string Title
        {
            get { return (string)base.GetValue(TitleProperty); }
            set { base.SetValue(TitleProperty, value); }
        }

        #endregion //Properties

        #region Constructors

        static WizardPage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WizardPage), new FrameworkPropertyMetadata(typeof(WizardPage)));
        }

        #endregion //Constructors
    }
}
