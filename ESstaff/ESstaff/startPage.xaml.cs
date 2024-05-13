using ESstaff.Helpers;
using ESstaff.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ESstaff
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class startPage : ContentPage
    {
        private ICommand refreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    this.IsRefreshing = true;

                    await this.RefreshBrandList();

                    this.IsRefreshing = false;
                });
            }
        }

        private ICommand showUsersCommand;
        public ICommand ShowUsersCommand
        {
            get
            {
                if (showUsersCommand == null)
                {
                    showUsersCommand = new Command(execute: () =>
                    {
                        Navigation.PushAsync(new registerPage());
                    },
                    canExecute: () =>
                    {
                        return App.LoggedInUserAccount?.IsAdmin ?? false;
                    });
                }
                return showUsersCommand;
            }
        }

        private bool isRefreshing = false;
        public bool IsRefreshing
        {
            get
            {
                return this.isRefreshing;
            }
            set
            {
                this.isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public string SearchTerm { get; set; }

        public startPage()
        {
            InitializeComponent();
            this.BindingContext = this;
        }

        private WebAuthenticatorResult webAuthenticatorResult;
        public WebAuthenticatorResult WebAuthenticatorResult
        {
            get
            {
                return this.webAuthenticatorResult;
            }
            set
            {
                this.webAuthenticatorResult = value;
                OnPropertyChanged(nameof(WebAuthenticatorResult));
            }
        }

        private async Task RefreshBrandList()
        {

            Expression<Func<Brands, bool>> searchLambda = x => x.BrandName.Contains("SearchTerm");

            // Convert the lambda to a string and apply a string replace operation to replace the "SearchTerm" string with the actual value
            var stringLambda = searchLambda.ToString().Replace("SearchTerm", $"{SearchTerm}");

            // Parse the "hacked" string to reform the Lambda expression again using ParseLambda
            searchLambda = DynamicExpressionParser.ParseLambda<Brands, bool>(new ParsingConfig(), true, stringLambda, this.SearchTerm);

            try
            {
                // Pass the prepared lambda to the DataService call as normal 
                listView.ItemsSource = await
                    App.DataService
                       .AddBearerToken(App.LoggedInUserAccount?.AccessToken)
                       .EnableLoginOnUnauthorized()
                       .GetAllAsync<Brands>(searchLambda, "GetBrandsWithRelatedData");
            }
            catch (Exception e)
            {
                await DisplayAlert("Error", e.Message, "Ok");
            }

        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            ((Command)ShowUsersCommand).ChangeCanExecute();

            App.DataService
                .RemoveLoginFlowAction()
                .SetLoginFlowAction(async () =>
                {
                    await this.Login();
                })
                .EnableLoginOnUnauthorized();

            listView.BeginRefresh();
        }
        private void SearchTerm_Completed(object sender, EventArgs e)
        {
            this.RefreshCommand.Execute(null);
        }

        void OnLogoutClicked(object sender, EventArgs e)
        {
            App.DataService.RemoveBearerToken();
            App.LoggedInUserAccount = null;
            this.RefreshCommand.Execute(null);
        }




        async void addPage(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new addPage());
        }

        private async void rnw1(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new registerPage());
        }

        
    }
}