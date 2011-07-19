using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace Microsoft.Windows.Controls
{
    public class SplitButton : ContentControl, ICommandSource
    {
        #region Members
        
        Button _actionButton;

        #endregion //Members

        #region Constructors

        static SplitButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
        }

        public SplitButton()
        {
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
        }

        #endregion //Constructors

        #region Properties

        #region DropDownContent

        public static readonly DependencyProperty DropDownContentProperty = DependencyProperty.Register("DropDownContent", typeof(object), typeof(SplitButton), new UIPropertyMetadata(null, OnDropDownContentChanged));
        public object DropDownContent
        {
            get { return (object)GetValue(DropDownContentProperty); }
            set { SetValue(DropDownContentProperty, value); }
        }

        private static void OnDropDownContentChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SplitButton splitButton = o as SplitButton;
            if (splitButton != null)
                splitButton.OnDropDownContentChanged((object)e.OldValue, (object)e.NewValue);
        }

        protected virtual void OnDropDownContentChanged(object oldValue, object newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //DropDownContent

        #region IsOpen

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(SplitButton), new UIPropertyMetadata(false, OnIsOpenChanged));
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        private static void OnIsOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            SplitButton splitButton = o as SplitButton;
            if (splitButton != null)
                splitButton.OnIsOpenChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsOpenChanged(bool oldValue, bool newValue)
        {
            if (newValue)
                RaiseRoutedEvent(SplitButton.OpenedEvent);
            else
                RaiseRoutedEvent(SplitButton.ClosedEvent);
        }

        #endregion //IsOpen

        #endregion //Properties

        #region Events

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitButton));
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent("Opened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitButton));
        public event RoutedEventHandler Opened
        {
            add { AddHandler(OpenedEvent, value); }
            remove { RemoveHandler(OpenedEvent, value); }
        }

        public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitButton));
        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        #endregion //Events

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _actionButton = (Button)GetTemplateChild("PART_ActionButton");
            _actionButton.Click += ActionButton_Click;
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            OnClick();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    {
                        CloseDropDown();
                        break;
                    }
            }
        }

        private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
        {
            CloseDropDown();
        }

        void CanExecuteChanged(object sender, EventArgs e)
        {
            if (Command != null)
            {
                RoutedCommand command = Command as RoutedCommand;

                // If a RoutedCommand.
                if (command != null)
                    IsEnabled = command.CanExecute(CommandParameter, CommandTarget) ? true : false;
                // If a not RoutedCommand.
                else
                    IsEnabled = Command.CanExecute(CommandParameter) ? true : false;
            }
        }

        #endregion //Event Handlers

        #region Methods

        #region Protected

        protected virtual void OnClick()
        {
            RaiseRoutedEvent(SplitButton.ClickEvent);
            RaiseCommand();
        }
        #endregion //Protected

        #region Private

        /// <summary>
        /// Raises routed events.
        /// </summary>
        private void RaiseRoutedEvent(RoutedEvent routedEvent)
        {
            RoutedEventArgs args = new RoutedEventArgs(routedEvent, this);
            RaiseEvent(args);
        }

        /// <summary>
        /// Raises the command's Execute event.
        /// </summary>
        private void RaiseCommand()
        {
            if (Command != null)
            {
                RoutedCommand routedCommand = Command as RoutedCommand;

                if (routedCommand == null)
                    ((ICommand)Command).Execute(CommandParameter);
                else
                    routedCommand.Execute(CommandParameter, CommandTarget);
            }
        }

        /// <summary>
        /// Unhooks a command from the Command property.
        /// </summary>
        /// <param name="oldCommand">The old command.</param>
        /// <param name="newCommand">The new command.</param>
        private void UnhookCommand(ICommand oldCommand, ICommand newCommand)
        {
            EventHandler handler = CanExecuteChanged;
            oldCommand.CanExecuteChanged -= handler;
        }

        /// <summary>
        /// Hooks up a command to the CanExecuteChnaged event handler.
        /// </summary>
        /// <param name="oldCommand">The old command.</param>
        /// <param name="newCommand">The new command.</param>
        private void HookUpCommand(ICommand oldCommand, ICommand newCommand)
        {
            EventHandler handler = new EventHandler(CanExecuteChanged);
            canExecuteChangedHandler = handler;
            if (newCommand != null)
                newCommand.CanExecuteChanged += canExecuteChangedHandler;
        }

        /// <summary>
        /// Closes the drop down.
        /// </summary>
        private void CloseDropDown()
        {
            if (IsOpen)
                IsOpen = false;
            ReleaseMouseCapture();
        }

        #endregion //Private

        #endregion //Methods

        #region ICommandSource Members

        // Keeps a copy of the CanExecuteChnaged handler so it doesn't get garbage collected.
        private EventHandler canExecuteChangedHandler;

        #region Command

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(SplitButton), new PropertyMetadata((ICommand)null, OnCommandChanged));
        [TypeConverter(typeof(CommandConverter))]
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SplitButton splitButton = d as SplitButton;
            if (splitButton != null)
                splitButton.OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
        }

        protected virtual void OnCommandChanged(ICommand oldValue, ICommand newValue)
        {
            // If old command is not null, then we need to remove the handlers.
            if (oldValue != null)
                UnhookCommand(oldValue, newValue);

            HookUpCommand(oldValue, newValue);
        }

        #endregion //Command

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(SplitButton), new PropertyMetadata(null));
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(SplitButton), new PropertyMetadata(null));
        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        #endregion
    }
}
