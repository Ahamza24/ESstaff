using ESstaff.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static ESstaff.Helpers.DecodedAccessToken;


namespace ESstaff
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class loginPage : ContentPage
    {
        public loginPage()
        {
            InitializeComponent();
        }

        private async void NavigateButton_OnClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new startPage());
            try
            {
                var loginUser = BindingContext as LoginUser;

                var loginResponse = await App.DataService
                    .RemoveBearerToken()
                    .DisableLoginOnUnauthorized()
                    .InsertAsync<LoginUser, Token>(loginUser, "Login");

                if (loginResponse != null)
                {
                    App.LoggedInUserAccount = Decode<LoggedInUserAccount>(loginResponse);
                }

                if (App.LoggedInUserAccount?.EmailAddress.ToLower() == loginUser.EmailAddress.ToLower())
                {
                    // login was ok, we can close this window
                    await Navigation.PopModalAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login failed", $"{ex.Message}", "Ok");
            }try
            {
                var loginUser = BindingContext as LoginUser;

                var loginResponse = await App.DataService
                    .RemoveBearerToken()
                    .DisableLoginOnUnauthorized()
                    .InsertAsync<LoginUser, Token>(loginUser, "Login");

                if (loginResponse != null)
                {
                    App.LoggedInUserAccount = Decode<LoggedInUserAccount>(loginResponse);
                }

                if (App.LoggedInUserAccount?.EmailAddress.ToLower() == loginUser.EmailAddress.ToLower())
                {
                    // login was ok, we can close this window
                    await Navigation.PopModalAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login failed", $"{ex.Message}", "Ok");
            }
        }


    }
}