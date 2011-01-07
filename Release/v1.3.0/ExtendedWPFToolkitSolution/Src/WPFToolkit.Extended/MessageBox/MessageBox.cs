using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls
{
    [TemplateVisualState(Name = VisualStates.OK, GroupName = VisualStates.MessageBoxButtonsGroup)]
    [TemplateVisualState(Name = VisualStates.OKCancel, GroupName = VisualStates.MessageBoxButtonsGroup)]
    [TemplateVisualState(Name = VisualStates.YesNo, GroupName = VisualStates.MessageBoxButtonsGroup)]
    [TemplateVisualState(Name = VisualStates.YesNoCancel, GroupName = VisualStates.MessageBoxButtonsGroup)]
    public class MessageBox : Control
    {
        #region Private Members

        /// <summary>
        /// Tracks the MessageBoxButon value passed into the InitializeContainer method
        /// </summary>
        private MessageBoxButton _button = MessageBoxButton.OK;

        #endregion //Private Members

        #region Constructors

        static MessageBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MessageBox), new FrameworkPropertyMetadata(typeof(MessageBox)));
        }

        internal MessageBox()
        { /*user cannot create instance */ }

        #endregion //Constructors

        #region Properties

        #region Protected Properties

        /// <summary>
        /// A System.Windows.MessageBoxResult value that specifies which message box button was clicked by the user.
        /// </summary>
        protected MessageBoxResult MessageBoxResult = MessageBoxResult.None;

        protected Window Container { get; private set; }
        protected Thumb DragWidget { get; private set; }
        protected Button CloseButton { get; private set; }

        protected Button OkButton { get; private set; }
        protected Button CancelButton { get; private set; }
        protected Button YesButton { get; private set; }
        protected Button NoButton { get; private set; }

        protected Button OkButton1 { get; private set; }
        protected Button CancelButton1 { get; private set; }
        protected Button YesButton1 { get; private set; }
        protected Button NoButton1 { get; private set; }

        #endregion //Protected Properties

        #region Dependency Properties

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(MessageBox), new UIPropertyMetadata(String.Empty));
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public static readonly DependencyProperty CaptionForegroundProperty = DependencyProperty.Register("CaptionForeground", typeof(Brush), typeof(MessageBox), new UIPropertyMetadata(null));
        public Brush CaptionForeground
        {
            get { return (Brush)GetValue(CaptionForegroundProperty); }
            set { SetValue(CaptionForegroundProperty, value); }
        }

        public static readonly DependencyProperty CloseButtonStyleProperty = DependencyProperty.Register("CloseButtonStyle", typeof(Style), typeof(MessageBox), new PropertyMetadata(null));
        public Style CloseButtonStyle
        {
            get { return (Style)GetValue(CloseButtonStyleProperty); }
            set { SetValue(CloseButtonStyleProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(MessageBox), new UIPropertyMetadata(default(ImageSource)));
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MessageBox), new UIPropertyMetadata(String.Empty));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty WindowBackgroundProperty = DependencyProperty.Register("WindowBackground", typeof(Brush), typeof(MessageBox), new PropertyMetadata(null));
        public Brush WindowBackground
        {
            get { return (Brush)GetValue(WindowBackgroundProperty); }
            set { SetValue(WindowBackgroundProperty, value); }
        }

        public static readonly DependencyProperty WindowBorderBrushProperty = DependencyProperty.Register("WindowBorderBrush", typeof(Brush), typeof(MessageBox), new PropertyMetadata(null));
        public Brush WindowBorderBrush
        {
            get { return (Brush)GetValue(WindowBorderBrushProperty); }
            set { SetValue(WindowBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty WindowOpacityProperty = DependencyProperty.Register("WindowOpacity", typeof(double), typeof(MessageBox), new PropertyMetadata(null));
        public double WindowOpacity
        {
            get { return (double)GetValue(WindowOpacityProperty); }
            set { SetValue(WindowOpacityProperty, value); }
        }

        #endregion //Dependency Properties

        #endregion //Properties

        #region Base Class Overrides

        /// <summary>
        /// Overrides the OnApplyTemplate method.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            DragWidget = (Thumb)GetTemplateChild("PART_DragWidget");
            if (DragWidget != null)
                DragWidget.DragDelta += (o, e) => ProcessMove(e);

            CloseButton = (Button)GetTemplateChild("PART_CloseButton");
            if (CloseButton != null)
                CloseButton.Click += (o, e) => Close();

            NoButton = (Button)GetTemplateChild("PART_NoButton");
            if (NoButton != null)
                NoButton.Click += (o, e) => Button_Click(o, e);

            NoButton1 = (Button)GetTemplateChild("PART_NoButton1");
            if (NoButton1 != null)
                NoButton1.Click += (o, e) => Button_Click(o, e);

            YesButton = (Button)GetTemplateChild("PART_YesButton");
            if (YesButton != null)
                YesButton.Click += (o, e) => Button_Click(o, e);

            YesButton1 = (Button)GetTemplateChild("PART_YesButton1");
            if (YesButton1 != null)
                YesButton1.Click += (o, e) => Button_Click(o, e);

            CancelButton = (Button)GetTemplateChild("PART_CancelButton");
            if (CancelButton != null)
                CancelButton.Click += (o, e) => Button_Click(o, e);

            CancelButton1 = (Button)GetTemplateChild("PART_CancelButton1");
            if (CancelButton1 != null)
                CancelButton1.Click += (o, e) => Button_Click(o, e);

            OkButton = (Button)GetTemplateChild("PART_OkButton");
            if (OkButton != null)
                OkButton.Click += (o, e) => Button_Click(o, e);

            OkButton1 = (Button)GetTemplateChild("PART_OkButton1");
            if (OkButton1 != null)
                OkButton1.Click += (o, e) => Button_Click(o, e);

            ChangeVisualState(_button.ToString(), true);
        }

        #endregion //Base Class Overrides

        #region Methods

        #region Public Static

        /// <summary>
        /// Displays a message box that has a message and that returns a result.
        /// </summary>
        /// <param name="messageText">A System.String that specifies the text to display.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(string messageText)
        {
            return Show(messageText, string.Empty, MessageBoxButton.OK);
        }

        /// <summary>
        /// Displays a message box that has a message and title bar caption; and that returns a result.
        /// </summary>
        /// <param name="messageText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(string messageText, string caption)
        {
            return Show(messageText, caption, MessageBoxButton.OK);
        }

        /// <summary>
        /// Displays a message box that has a message and that returns a result.
        /// </summary>
        /// <param name="messageText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button)
        {
            return ShowCore(messageText, caption, button, MessageBoxImage.None);
        }

        /// <summary>
        /// Displays a message box that has a message and that returns a result.
        /// </summary>
        /// <param name="messageText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="image"> A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return ShowCore(messageText, caption, button, icon);
        }

        #endregion //Public Static

        #region Private Static

        private static MessageBoxResult ShowCore(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            MessageBox msgBox = new MessageBox();
            msgBox.InitializeMessageBox(messageText, caption, button, icon);
            msgBox.Show();
            return msgBox.MessageBoxResult;
        }

        /// <summary>
        /// Resolves the owner Window of the MessageBox.
        /// </summary>
        /// <returns></returns>
        private static FrameworkElement ResolveOwner()
        {
            FrameworkElement owner = null;
            if (Application.Current != null)
            {
                foreach (Window w in Application.Current.Windows)
                {
                    if (w.IsActive)
                    {
                        owner = w;
                        break;
                    }
                }
            }
            return owner;
        }

        #endregion //Private Static

        #region Protected

        /// <summary>
        /// Shows the MessageBox
        /// </summary>
        protected void Show()
        {
            Container.ShowDialog();
        }

        /// <summary>
        /// Initializes the MessageBox.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="button">The button.</param>
        /// <param name="image">The image.</param>
        protected void InitializeMessageBox(string text, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            Text = text;
            Caption = caption;
            _button = button;
            SetImageSource(image);
            Container = CreateContainer();
        }

        /// <summary>
        /// Changes the control's visual state(s).
        /// </summary>
        /// <param name="name">name of the state</param>
        /// <param name="useTransitions">True if state transitions should be used.</param>
        protected void ChangeVisualState(string name, bool useTransitions)
        {
            VisualStateManager.GoToState(this, name, useTransitions);
        }

        #endregion //Protected

        #region Private

        /// <summary>
        /// Sets the message image source.
        /// </summary>
        /// <param name="image">The image to show.</param>
        private void SetImageSource(MessageBoxImage image)
        {
            String iconName = String.Empty;

            switch (image)
            {
                case MessageBoxImage.Error:
                    {
                        iconName = "Error48.png";
                        break;
                    }
                case MessageBoxImage.Information:
                    {
                        iconName = "Information48.png";
                        break;
                    }
                case MessageBoxImage.Question:
                    {
                        iconName = "Question48.png";
                        break;
                    }
                case MessageBoxImage.Warning:
                    {
                        iconName = "Warning48.png";
                        break;
                    }
                case MessageBoxImage.None:
                default:
                    {
                        return;
                    }
            }

            ImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString(String.Format("pack://application:,,,/WPFToolkit.Extended;component/MessageBox/Icons/{0}", iconName));
        }

        /// <summary>
        /// Creates the container which will host the MessageBox control.
        /// </summary>
        /// <returns></returns>
        private Window CreateContainer()
        {
            var newWindow = new Window();
            newWindow.AllowsTransparency = true;
            newWindow.Background = Brushes.Transparent;
            newWindow.Content = this;

            var owner = ResolveOwner();
            if (owner != null)
                newWindow.Owner = Window.GetWindow(owner);

            newWindow.ShowInTaskbar = false;
            newWindow.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
            newWindow.ResizeMode = System.Windows.ResizeMode.NoResize;
            newWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            newWindow.WindowStyle = System.Windows.WindowStyle.None;
            return newWindow;
        }

        #endregion //Private

        #endregion //Methods

        #region Event Handlers

        /// <summary>
        /// Processes the move of a drag operation on the header.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
        private void ProcessMove(DragDeltaEventArgs e)
        {
            Container.Left = Container.Left + e.HorizontalChange;
            Container.Top = Container.Top + e.VerticalChange;
        }

        /// <summary>
        /// Sets the MessageBoxResult according to the button pressed and then closes the MessageBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.Source as Button;
            switch (button.Name)
            {
                case "PART_NoButton":
                case "PART_NoButton1":
                    MessageBoxResult = MessageBoxResult.No;
                    break;
                case "PART_YesButton":
                case "PART_YesButton1":
                    MessageBoxResult = MessageBoxResult.Yes;
                    break;
                case "PART_CancelButton":
                case "PART_CancelButton1":
                    MessageBoxResult = MessageBoxResult.Cancel;
                    break;
                case "PART_OkButton":
                case "PART_OkButton1":
                    MessageBoxResult = MessageBoxResult.OK;
                    break;
            }

            Close();
        }

        /// <summary>
        /// Closes the MessageBox.
        /// </summary>
        private void Close()
        {
            Container.Close();
        }

        #endregion //Event Handlers
    }
}
