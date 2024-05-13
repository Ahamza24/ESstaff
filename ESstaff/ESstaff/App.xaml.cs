using ESstaff.Models;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinMovieListApp.Data;

namespace ESstaff
{
    public partial class App : Application
    {
        static DataService dataService;

        public static DataService DataService
        {
            get
            {
                if (dataService == null)
                {
                    dataService =
                        new DataService("https://192.168.137.1:5001")
                            .AddEntityModelEndpoint<Brands>("api/Brands")
                            .AddEntityModelEndpoint<ImageFile>("api/ImageFiles")
                            .AddEntityModelEndpoint<Staff>("api/Staff")
                            .AddEntityModelEndpoint<EditableUser>("api/Account")
                            .AddEntityModelEndpoint<LoginUser>("api/Account")
                            .AddEntityModelEndpoint<UserSummary>("api/Account"); ;
                           
                }
                return dataService;
            }
        }

        static LoggedInUserAccount loggedInUserAccount;
        public static LoggedInUserAccount LoggedInUserAccount
        {
            get
            {
                return loggedInUserAccount;
            }
            set
            {
                loggedInUserAccount = value;
            }
        }

        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new welcomePage());
        }
    }
}
