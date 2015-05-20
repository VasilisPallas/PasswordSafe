using PasswordSafe.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Limilabs.Mail;
using Limilabs.Mail.Headers;
using Limilabs.Client.SMTP;
using SQLite;
using Windows.Networking.Connectivity;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PasswordSafe
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ForgottenPasswordPage : Page
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


        public ForgottenPasswordPage()
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

        public async void RecoverPass()
        {

            if (username.Text != string.Empty || mail.Text != string.Empty)
            {
                string encryptedPassword = null;
                var result = await connection.QueryAsync<User>("Select password FROM Users WHERE name = ? AND email = ?", new object[] { username.Text, mail.Text });
                if (result.Count == 1)
                {

                    //var d = connection.Table<User>().Where(usr => usr.name == username.Text && usr.email == mail.Text);

                    var queryResult = result;

                    foreach (var sd in result)
                    {
                        ///apothikeusi password pou anazhthsame wste na steiloume me mail
                        // pass = sd.password.ToString();
                        encryptedPassword = sd.password.ToString();
                    }


                    Send(username.Text, mail.Text, encryptedPassword);
                    MessageDialog line = new MessageDialog("Your password has been sucessfully sent to your email.");
                    line.Commands.Add(new UICommand("OK", (command) =>
                    {
                        this.Frame.Navigate(typeof(MainPage));

                    }));
                    await line.ShowAsync();
                }
                else
                {
                    MessageDialog msg = new MessageDialog("Invalid Username or e-mail");
                    msg.Commands.Add(new UICommand("OK", null, 0));

                    await msg.ShowAsync();
                }

            }
            else if (username.Text == string.Empty || mail.Text == string.Empty)
            {
                var line = new MessageDialog("Please fill the empty fields.");
                line.Commands.Add(new UICommand("OK", null, 0));
                await line.ShowAsync();
            }
            username.Text = string.Empty;
            mail.Text = string.Empty;
        }


        public async void Send(string name, string mail, string password)
        {
            string pass = Secure.Converttostring(password);

            MailBuilder myMail = new MailBuilder();
            string subject = "Password Recover";
            myMail.Html = "Hello " + name + ". Your password is " + pass;
            myMail.Subject = subject;
            myMail.To.Add(new MailBox(mail));
            myMail.From.Add(new MailBox("vspallas@gmail.com"));
            IMail email = myMail.Create();

            try
            {
                using (Smtp smtp = new Smtp())
                {

                    await smtp.Connect("smtp.gmail.com", 587);
                    await smtp.UseBestLoginAsync("vspallas@gmail.com", "todaywasagoodday");
                    await smtp.SendMessageAsync(email);
                    await smtp.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                MessageDialog msg = new MessageDialog(ex.Message);
                await msg.ShowAsync();
            }

            //no network connection 

        }
        private static bool IsInternet()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }
        

        private async void SendPassword_Click(object sender, RoutedEventArgs e)
        {
            bool internet;
            internet = IsInternet();
            if (!internet)
            {
                MessageDialog line = new MessageDialog("You don't have internet access.");

                line.Commands.Add(new UICommand("OK", null, 0));
                await line.ShowAsync();
            }
            else
            {
                RecoverPass();
            }
            
        }
    }
}
