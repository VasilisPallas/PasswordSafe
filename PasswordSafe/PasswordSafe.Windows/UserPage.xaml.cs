using PasswordSafe.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SQLite;
using Windows.UI.Popups;
using System.Diagnostics;
using System.Collections.Generic;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PasswordSafe
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class UserPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private SQLiteAsyncConnection connection = new SQLiteAsyncConnection("User.db");
        private string username;
        private bool menuIsOpen = false;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public UserPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            Loaded += UserPage_Loaded;
        }

        private void UserPage_Loaded(object sender, RoutedEventArgs e)
        {
            fillCombobox();
        }


        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="Common.NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="Common.SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="Common.NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);

            pageTitle.Text = "User: " + e.Parameter as string; // textBlock.Text = e.Parameter as string
            username = e.Parameter as string;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void addNewPassword_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddValuesPage), username);
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!menuIsOpen)
            {
                menugrid.Visibility = Visibility.Visible;
                menuIsOpen = true;
            }
            else
            {
                menugrid.Visibility = Visibility.Collapsed;
                menuIsOpen = false;
            }
        }

        public async void fillCombobox()
        {
            var results = await connection.QueryAsync<User>("Select name FROM Passwords");
            passwordsNameCombobox.Items.Add(" ");
            foreach (var result in results)
            {
                passwordsNameCombobox.Items.Add(UppercaseFirst(result.name));
            }

        }

        static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        private async void passwordsNameCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string content = passwordsNameCombobox.SelectedItem.ToString();
            Debug.WriteLine(content);
            if (infoGrid.Visibility == Visibility.Collapsed)
                infoGrid.Visibility = Visibility.Visible;

            if (content.Equals(" "))
                infoGrid.Visibility = Visibility.Collapsed;
            else
            {
                var result = await connection.QueryAsync<User>("Select name,email,password FROM Passwords WHERE name = ? ", new object[] { content.ToLower() });
                foreach (var r in result)
                {
                    nameTextBlock.Text = r.name;
                    emailTextBlock.Text = r.email;
                    passwordTextBlock.Text = r.password != string.Empty ? Secure.Converttostring(r.password) : string.Empty;
                }
            }
        }

        private void changebutton_Click(object sender, RoutedEventArgs e)
        {
            changebutton.Visibility = Visibility.Collapsed;
            emailTextBlock.Visibility = Visibility.Collapsed;
            passwordTextBlock.Visibility = Visibility.Collapsed;

            savebutton.Visibility = Visibility.Visible;
            emailTextBox.Visibility = Visibility.Visible;
            passwordTextBox.Visibility = Visibility.Visible;

            emailTextBox.Text = emailTextBlock.Text;
            passwordTextBox.Text = passwordTextBlock.Text;
        }

        private void savebutton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog line = new MessageDialog("Are you sure that you want to save your changes?");
            line.Commands.Add(new UICommand("OK", (command) =>
            {
                var result = connection.QueryAsync<Password>("Update Passwords SET email = ?, password = ? WHERE name = ?", new object[] { emailTextBox.Text, Secure.Converttohex(passwordTextBox.Text), nameTextBlock.Text });

            }));
            line.Commands.Add(new UICommand("Discard changes", null, 0));
            line.ShowAsync();

            emailTextBlock.Text = emailTextBox.Text;
            passwordTextBlock.Text = passwordTextBox.Text;

            changebutton.Visibility = Visibility.Visible;
            emailTextBlock.Visibility = Visibility.Visible;
            passwordTextBlock.Visibility = Visibility.Visible;

            savebutton.Visibility = Visibility.Collapsed;
            emailTextBox.Visibility = Visibility.Collapsed;
            passwordTextBox.Visibility = Visibility.Collapsed;
        }

        private void deletebutton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog line = new MessageDialog("Are you sure that you want to delete that password?");
            line.Commands.Add(new UICommand("Yes", (command) =>
            {
                var result1 = connection.QueryAsync<User>("Delete FROM Passwords WHERE name = ?", new object[] { nameTextBlock.Text });
                passwordsNameCombobox.SelectionChanged -= passwordsNameCombobox_SelectionChanged;
                passwordsNameCombobox.Items.Clear();
                fillCombobox();
                passwordsNameCombobox.SelectionChanged += passwordsNameCombobox_SelectionChanged;
                infoGrid.Visibility = Visibility.Collapsed;
            }));
            line.Commands.Add(new UICommand("Cancel", null, 0));
            line.ShowAsync();

        }

        private void changeUserData_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ChangeUserDataPage), username);
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            MessageDialog line = new MessageDialog("Are you sure that you want to log out?");
            line.Commands.Add(new UICommand("Yes", (command) =>
            {
                this.Frame.Navigate(typeof(MainPage));
            }));
            line.Commands.Add(new UICommand("No", null, 0));
            line.ShowAsync();

        }
    }
}
