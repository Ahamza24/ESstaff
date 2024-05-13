using ESstaff.Helpers;
using ESstaff.Models;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ESstaff
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class registerPage : ContentPage
    {
        public string UserId { get; set; }

        public registerPage()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            App.DataService
                .RemoveLoginFlowAction()
                .SetLoginFlowAction(async () =>
                {
                    await this.Login();
                })
                .EnableLoginOnUnauthorized();

            EditableUser user;

            if (string.IsNullOrEmpty(this.UserId))
            {
                this.Title = "Register New User";
                user = new EditableUser();
            }
            else
            {
                this.Title = "Modify Existing User";
                user = await App.DataService
                       .AddBearerToken(App.LoggedInUserAccount?.AccessToken)
                       .EnableLoginOnUnauthorized()
                       .GetAsync<EditableUser, string>(this.UserId, "Modify");
            }

            this.BindingContext = user;

            if (user.Id == App.LoggedInUserAccount.Id)
            {
                // A user should not be able to remove themselves as an Admin                
                AdminCheckbox.IsEnabled = false;
            }
        }

        async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            var user = this.BindingContext as EditableUser;

            if (!string.IsNullOrEmpty(user.Password)
                && !string.IsNullOrEmpty(user.PasswordConfirmation)
                && (user.Password != user.PasswordConfirmation))
            {
                await DisplayAlert("Error", "Passwords do not match!", "Ok");
                return;
            }

            if ((string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.PasswordConfirmation))
                && string.IsNullOrEmpty(user.Id))
            {
                await DisplayAlert("Error", "Passwords must be provided when creating a new account!", "Ok");
                return;
            }

            bool success = false;

            try
            {
                if (user != null && !string.IsNullOrEmpty(user.Id))
                {
                    success = await App.DataService
                        .AddBearerToken(App.LoggedInUserAccount?.AccessToken)
                        .EnableLoginOnUnauthorized()
                        .UpdateAsync<EditableUser, string>(user, UserId, "Modify");
                }
                else
                {
                    var response = await App.DataService
                        .AddBearerToken(App.LoggedInUserAccount?.AccessToken)
                        .EnableLoginOnUnauthorized()
                        .InsertAsync<EditableUser>(user, "Register");
                    success = !(response == null || response?.Id == string.Empty);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }

            if (success)
            {
                await Navigation.PopAsync();
            }
        }
    }
}