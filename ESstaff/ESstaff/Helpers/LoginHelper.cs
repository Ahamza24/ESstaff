using ESstaff.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ESstaff.Helpers
{
    public static class LoginHelper
    {
        public static async Task Login(this NavigableElement element)
        {
            await element.Navigation.PushModalAsync(new loginPage()
            {
                BindingContext = new LoginUser()
                {
                    // here you can pre-fill the login dialog details, useful for testing to prevent endless re-inputting each time!
                    EmailAddress = "admin@webapimovie.bolton.ac.uk",
                    Password = "Pa$$w0rd!"
                }
            });
        }
    }
}
