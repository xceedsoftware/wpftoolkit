using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Windows.Controls
{
    public class WizardPage : ContentControl
    {
        #region Properties

        public static readonly DependencyProperty BackButtonVisibilityProperty = DependencyProperty.Register("BackButtonVisibility", typeof(WizardPageButtonVisibility), typeof(WizardPage), new UIPropertyMetadata(WizardPageButtonVisibility.Inherit));
        public WizardPageButtonVisibility BackButtonVisibility
        {
            get { return (WizardPageButtonVisibility)GetValue(BackButtonVisibilityProperty); }
            set { SetValue(BackButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty CancelButtonVisibilityProperty = DependencyProperty.Register("CancelButtonVisibility", typeof(WizardPageButtonVisibility), typeof(WizardPage), new UIPropertyMetadata(WizardPageButtonVisibility.Inherit));
        public WizardPageButtonVisibility CancelButtonVisibility
        {
            get { return (WizardPageButtonVisibility)GetValue(CancelButtonVisibilityProperty); }
            set { SetValue(CancelButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(WizardPage));
        public string Description
        {
            get { return (string)base.GetValue(DescriptionProperty); }
            set { base.SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty FinishButtonVisibilityProperty = DependencyProperty.Register("FinishButtonVisibility", typeof(WizardPageButtonVisibility), typeof(WizardPage), new UIPropertyMetadata(WizardPageButtonVisibility.Inherit));
        public WizardPageButtonVisibility FinishButtonVisibility
        {
            get { return (WizardPageButtonVisibility)GetValue(FinishButtonVisibilityProperty); }
            set { SetValue(FinishButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty HelpButtonVisibilityProperty = DependencyProperty.Register("HelpButtonVisibility", typeof(WizardPageButtonVisibility), typeof(WizardPage), new UIPropertyMetadata(WizardPageButtonVisibility.Inherit));
        public WizardPageButtonVisibility HelpButtonVisibility
        {
            get { return (WizardPageButtonVisibility)GetValue(HelpButtonVisibilityProperty); }
            set { SetValue(HelpButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty NextButtonVisibilityProperty = DependencyProperty.Register("NextButtonVisibility", typeof(WizardPageButtonVisibility), typeof(WizardPage), new UIPropertyMetadata(WizardPageButtonVisibility.Inherit));
        public WizardPageButtonVisibility NextButtonVisibility
        {
            get { return (WizardPageButtonVisibility)GetValue(NextButtonVisibilityProperty); }
            set { SetValue(NextButtonVisibilityProperty, value); }
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
