using PasswordSafe.Common;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Text.RegularExpressions;
using Windows.UI.Popups;
using SQLite;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PasswordSafe
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class SignUpPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private SQLiteAsyncConnection connection = new SQLiteAsyncConnection("User.db");

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


        public SignUpPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
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
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        public async void Signup()
        {
            bool isEmail = Regex.IsMatch(mail.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z");

            if (username.Text != string.Empty && password.Password != string.Empty && mail.Text != string.Empty && isEmail)
            {

                var result = await connection.QueryAsync<User>("Select name,email FROM Users WHERE name = ? OR email = ?", new object[] { username.Text, mail.Text });

                if (result.Count == 0)
                {

                    var User = new User()
                    {
                        name = username.Text,
                        password = Secure.Converttohex(password.Password),
                        email = mail.Text
                    };
                    await connection.InsertAsync(User);

                    MessageDialog line = new MessageDialog("Your account has been successfully created!");
                    line.Commands.Add(new UICommand("OK", (command) =>
                    {
                        this.Frame.Navigate(typeof(MainPage));

                    }));
                    await line.ShowAsync();
                }
                else
                {
                    MessageDialog line = new MessageDialog("Username or e-mail already exists");

                    line.Commands.Add(new UICommand("OK", null, 0));
                    await line.ShowAsync();

                }

            }
            else if (!isEmail && (username.Text != string.Empty || password.Password != string.Empty || mail.Text != string.Empty))
            {
                MessageDialog line = new MessageDialog("Invalid type of e-mail");
                line.Commands.Add(new UICommand("OK", null, 0));
                await line.ShowAsync();
            }
            else if (username.Text == string.Empty || password.Password == string.Empty || mail.Text == string.Empty)
            {
                var line = new MessageDialog("Please fill the empty fields.");
                line.Commands.Add(new UICommand("OK", null, 0));

                await line.ShowAsync();
            }
            username.Text = string.Empty;
            password.Password = string.Empty;
            mail.Text = string.Empty;
        }

        private void signUp_Click(object sender, RoutedEventArgs e)
        {
            Signup();
        }
    }
}
