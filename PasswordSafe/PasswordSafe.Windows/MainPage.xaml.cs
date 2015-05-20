using PasswordSafe.Common;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SQLite;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.Networking.Connectivity;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PasswordSafe
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Page
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


        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            CreateDatabase();
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

        public async Task<bool> DoesDbExist(string DatabaseName)
        {
            bool dbexist = true;
            try
            {
                StorageFile storageFile = await ApplicationData.Current.LocalFolder.GetFileAsync(DatabaseName);

            }
            catch
            {
                dbexist = false;
            }

            return dbexist;
        }

        public async void CreateDatabase()
        {
            SQLiteAsyncConnection connection = new SQLiteAsyncConnection("User.db");
            await connection.CreateTableAsync<User>();
            await connection.CreateTableAsync<Password>();
        }

        public async void Loggin()
        {
            if (username.Text != string.Empty && password.Password != string.Empty)
            {

                var result = await connection.QueryAsync<User>("Select name,password FROM Users WHERE name = ? AND password = ?", new object[] { username.Text, Secure.Converttohex(password.Password) });

                if (result.Count == 1)
                {
                    string param = username.Text;//<-- dinei sthn userpage to onoma tou xrhsth
                    this.Frame.Navigate(typeof(UserPage), username.Text);
                }
                else
                {
                    MessageDialog msg = new MessageDialog("Invalid username or password");
                    msg.Commands.Add(new UICommand("OK", null, 0));
                    await msg.ShowAsync();
                }
            }

            username.Text = string.Empty;
            password.Password = string.Empty;
        }

        private void signIn_Click(object sender, RoutedEventArgs e)
        {
            Loggin();
        }

        private void singUp_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SignUpPage));
        }

        private void deleteAccount_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DeleteAccountPage));
        }




        private void ForgottenPassword_Click(object sender, RoutedEventArgs e)
        {
                this.Frame.Navigate(typeof(ForgottenPasswordPage));
        }
    }
}
