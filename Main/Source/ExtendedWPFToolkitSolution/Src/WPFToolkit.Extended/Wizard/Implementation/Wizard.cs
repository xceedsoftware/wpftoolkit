using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Data;

namespace Microsoft.Windows.Controls
{
    public class Wizard : ItemsControl
    {
        #region Properties

        public static readonly DependencyProperty BackButtonContentProperty = DependencyProperty.Register("BackButtonContent", typeof(object), typeof(Wizard), new UIPropertyMetadata("Back"));
        public object BackButtonContent
        {
            get { return (object)GetValue(BackButtonContentProperty); }
            set { SetValue(BackButtonContentProperty, value); }
        }

        public static readonly DependencyProperty BackButtonVisibilityProperty = DependencyProperty.Register("BackButtonVisibility", typeof(Visibility), typeof(Wizard), new UIPropertyMetadata(Visibility.Visible));
        public Visibility BackButtonVisibility
        {
            get { return (Visibility)GetValue(BackButtonVisibilityProperty); }
            set { SetValue(BackButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty CancelButtonContentProperty = DependencyProperty.Register("CancelButtonContent", typeof(object), typeof(Wizard), new UIPropertyMetadata("Cancel"));
        public object CancelButtonContent
        {
            get { return (object)GetValue(CancelButtonContentProperty); }
            set { SetValue(CancelButtonContentProperty, value); }
        }

        public static readonly DependencyProperty CancelButtonVisibilityProperty = DependencyProperty.Register("CancelButtonVisibility", typeof(Visibility), typeof(Wizard), new UIPropertyMetadata(Visibility.Visible));
        public Visibility CancelButtonVisibility
        {
            get { return (Visibility)GetValue(CancelButtonVisibilityProperty); }
            set { SetValue(CancelButtonVisibilityProperty, value); }
        }

        #region CurrentPage

        public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register("CurrentPage", typeof(WizardPage), typeof(Wizard), new UIPropertyMetadata(null, OnCurrentPageChanged));
        public WizardPage CurrentPage
        {
            get { return (WizardPage)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        private static void OnCurrentPageChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Wizard wizard = o as Wizard;
            if (wizard != null)
                wizard.OnCurrentPageChanged((WizardPage)e.OldValue, (WizardPage)e.NewValue);
        }

        protected virtual void OnCurrentPageChanged(WizardPage oldValue, WizardPage newValue)
        {
            RaiseRoutedEvent(Wizard.PageChangedEvent);
        }

        #endregion //CurrentPage

        public static readonly DependencyProperty FinishButtonContentProperty = DependencyProperty.Register("FinishButtonContent", typeof(object), typeof(Wizard), new UIPropertyMetadata("Finish"));
        public object FinishButtonContent
        {
            get { return (object)GetValue(FinishButtonContentProperty); }
            set { SetValue(FinishButtonContentProperty, value); }
        }

        public static readonly DependencyProperty FinishButtonVisibilityProperty = DependencyProperty.Register("FinishButtonVisibility", typeof(Visibility), typeof(Wizard), new UIPropertyMetadata(Visibility.Visible));
        public Visibility FinishButtonVisibility
        {
            get { return (Visibility)GetValue(FinishButtonVisibilityProperty); }
            set { SetValue(FinishButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty HelpButtonContentProperty = DependencyProperty.Register("HelpButtonContent", typeof(object), typeof(Wizard), new UIPropertyMetadata("Help"));
        public object HelpButtonContent
        {
            get { return (object)GetValue(HelpButtonContentProperty); }
            set { SetValue(HelpButtonContentProperty, value); }
        }

        public static readonly DependencyProperty HelpButtonVisibilityProperty = DependencyProperty.Register("HelpButtonVisibility", typeof(Visibility), typeof(Wizard), new UIPropertyMetadata(Visibility.Visible));
        public Visibility HelpButtonVisibility
        {
            get { return (Visibility)GetValue(HelpButtonVisibilityProperty); }
            set { SetValue(HelpButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty NextButtonContentProperty = DependencyProperty.Register("NextButtonContent", typeof(object), typeof(Wizard), new UIPropertyMetadata("Next"));
        public object NextButtonContent
        {
            get { return (object)GetValue(NextButtonContentProperty); }
            set { SetValue(NextButtonContentProperty, value); }
        }

        public static readonly DependencyProperty NextButtonVisibilityProperty = DependencyProperty.Register("NextButtonVisibility", typeof(Visibility), typeof(Wizard), new UIPropertyMetadata(Visibility.Visible));
        public Visibility NextButtonVisibility
        {
            get { return (Visibility)GetValue(NextButtonVisibilityProperty); }
            set { SetValue(NextButtonVisibilityProperty, value); }
        }

        #endregion //Properties

        #region Constructors

        static Wizard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Wizard), new FrameworkPropertyMetadata(typeof(Wizard)));
        }

        public Wizard()
        {
            CommandBindings.Add(new CommandBinding(WizardCommands.Cancel, CancelWizard, CanCancelWizard));
            CommandBindings.Add(new CommandBinding(WizardCommands.Finish, FinishWizard, CanFinishWizard));
            CommandBindings.Add(new CommandBinding(WizardCommands.Help, RequestHelp, CanRequestHelp));
            CommandBindings.Add(new CommandBinding(WizardCommands.NextPage, SelectNextPage, CanSelectNextPage));
            CommandBindings.Add(new CommandBinding(WizardCommands.PreviousPage, SelectPreviousPage, CanSelectPreviousPage));
        }

        #endregion //Constructors

        #region Base Class Overrides

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new WizardPage();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is WizardPage);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (Items.Count > 0 && CurrentPage == null)
                CurrentPage = Items[0] as WizardPage;
        }

        #endregion //Base Class Overrides

        #region Commands

        private void CancelWizard(object sender, ExecutedRoutedEventArgs e)
        {
            RaiseRoutedEvent(Wizard.CancelEvent);
        }

        private void CanCancelWizard(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void FinishWizard(object sender, ExecutedRoutedEventArgs e)
        {
            RaiseRoutedEvent(Wizard.FinishEvent);
        }

        private void CanFinishWizard(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void RequestHelp(object sender, ExecutedRoutedEventArgs e)
        {
            RaiseRoutedEvent(Wizard.HelpEvent);
        }

        private void CanRequestHelp(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void SelectNextPage(object sender, ExecutedRoutedEventArgs e)
        {
            WizardPage nextPage = null;

            if (CurrentPage != null)
            {
                //check next page
                if (CurrentPage.NextPage != null)
                    nextPage = CurrentPage.NextPage;
                else
                {
                    //no next page defined use index
                    var currentIndex = Items.IndexOf(CurrentPage);
                    var nextPageIndex = currentIndex + 1;
                    if (nextPageIndex < Items.Count)
                        nextPage = Items[nextPageIndex] as WizardPage;
                }
            }

            CurrentPage = nextPage;
        }

        private void CanSelectNextPage(object sender, CanExecuteRoutedEventArgs e)
        {
            if (CurrentPage != null)
            {
                if (CurrentPage.NextPage != null)
                    e.CanExecute = true;
                else
                {
                    var currentIndex = Items.IndexOf(CurrentPage);
                    var nextPageIndex = currentIndex + 1;
                    if (nextPageIndex < Items.Count)
                        e.CanExecute = true;
                }
            }
        }

        private void SelectPreviousPage(object sender, ExecutedRoutedEventArgs e)
        {
            WizardPage previousPage = null;

            if (CurrentPage != null)
            {
                //check previous page
                if (CurrentPage.PreviousPage != null)
                    previousPage = CurrentPage.PreviousPage;
                else
                {
                    //no previous page defined so use index
                    var currentIndex = Items.IndexOf(CurrentPage);
                    var previousPageIndex = currentIndex - 1;
                    if (previousPageIndex > 0 && previousPageIndex < Items.Count)
                        previousPage = Items[previousPageIndex] as WizardPage;
                }
            }

            CurrentPage = previousPage;
        }

        private void CanSelectPreviousPage(object sender, CanExecuteRoutedEventArgs e)
        {
            if (CurrentPage != null)
            {
                if (CurrentPage.PreviousPage != null)
                    e.CanExecute = true;
                else
                {
                    var currentIndex = Items.IndexOf(CurrentPage);
                    var previousPageIndex = currentIndex - 1;
                    if (previousPageIndex > 0 && previousPageIndex < Items.Count)
                        e.CanExecute = true;
                }
            }
        }

        #endregion //Commands

        #region Events

        public static readonly RoutedEvent CancelEvent = EventManager.RegisterRoutedEvent("Cancel", RoutingStrategy.Bubble, typeof(EventHandler), typeof(Wizard));
        public event RoutedEventHandler Cancel
        {
            add { AddHandler(CancelEvent, value); }
            remove { RemoveHandler(CancelEvent, value); }
        }

        public static readonly RoutedEvent PageChangedEvent = EventManager.RegisterRoutedEvent("PageChanged", RoutingStrategy.Bubble, typeof(EventHandler), typeof(Wizard));
        public event RoutedEventHandler PageChanged
        {
            add { AddHandler(PageChangedEvent, value); }
            remove { RemoveHandler(PageChangedEvent, value); }
        }

        public static readonly RoutedEvent FinishEvent = EventManager.RegisterRoutedEvent("Finish", RoutingStrategy.Bubble, typeof(EventHandler), typeof(Wizard));
        public event RoutedEventHandler Finish
        {
            add { AddHandler(FinishEvent, value); }
            remove { RemoveHandler(FinishEvent, value); }
        }

        public static readonly RoutedEvent HelpEvent = EventManager.RegisterRoutedEvent("Help", RoutingStrategy.Bubble, typeof(EventHandler), typeof(Wizard));
        public event RoutedEventHandler Help
        {
            add { AddHandler(HelpEvent, value); }
            remove { RemoveHandler(HelpEvent, value); }
        }

        #endregion //Events

        #region Methods

        private void RaiseRoutedEvent(RoutedEvent routedEvent)
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(routedEvent, this);
            base.RaiseEvent(newEventArgs);
        }

        #endregion //Methods
    }
}