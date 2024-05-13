using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ESstaff.Models;
using System.Windows.Input;
using Xamarin.Essentials;
using System.IO;
using ESstaff.Helpers;

namespace ESstaff
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class addPage : ContentPage
    {
        private Brands brands;
        public Brands Brands
        {
            get
            {
                return this.brands;
            }
            set
            {
                this.brands = value;
                OnPropertyChanged(nameof(Brands));
            }
        }

        private string lastModifiedByUser;
        public string LastModifiedByUser
        {
            get
            {
                return this.lastModifiedByUser;
            }
            set
            {
                this.lastModifiedByUser = value;
                OnPropertyChanged(nameof(LastModifiedByUser));
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            App.DataService
                .SetLoginFlowAction(async () =>
                {
                    await this.Login();
                })
                .EnableLoginOnUnauthorized();

            if (!string.IsNullOrEmpty(brands.ModifiedByUserId))
            {
                var userSummary = await App.DataService
                    .AddBearerToken(App.LoggedInUserAccount?.AccessToken)
                    .DisableLoginOnUnauthorized()
                    .DisableThrowExceptionOnHttpClientError()
                    .GetAsync<Staff, string>(brands.ModifiedByUserId, "Summary");

                App.DataService.EnableThrowExceptionOnHttpClientError();

                this.LastModifiedByUser = userSummary?.Name;
            }

            this.BindingContext = this;
        }
        async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            bool success = false;
            try
            {
                // handle adding (or replacing) of image
                if (Brands.ImageFileId == 0 && Brands.ImageFile != null)
                {
                    //ImageFile object is not null and id is 0 which means it's a new image, so we need to add it
                    var image = await App.DataService.InsertAsync(Brands.ImageFile);
                    if (image == null)
                    {
                        throw new Exception("Image was not returned, something went wrong with data service!");
                    }
                    // we nullify the ImageFile object and only keep the inserted Image's Id 
                    // as otherwise the ImageFile child object gets serialised and sent with 
                    // the Movie object resuling in a much larger than necessary payload to the server
                    Brands.ImageFile = null;
                    Brands.ImageFileId = image.Id;
                }

                // update the ModifiedBy property since this user has now modified the movie entry
                brands.ModifiedByUserId = App.LoggedInUserAccount.Id;

                if (Brands.Id == 0)
                {
                    var response = await App.DataService
                        .AddBearerToken(App.LoggedInUserAccount?.AccessToken)
                        .EnableLoginOnUnauthorized()
                        .InsertAsync(Brands);

                    success = !(response == null || response?.Id == 0);
                }
                else
                {
                    success = await App.DataService
                        .AddBearerToken(App.LoggedInUserAccount?.AccessToken)
                        .EnableLoginOnUnauthorized()
                        .UpdateAsync(Brands, Brands.Id);
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

        async void OnTakePictureButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                await LoadPhotoAsync(photo);
                Console.WriteLine($"CapturePhotoAsync COMPLETED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
            }
        }

        async void OnChoosePictureButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync();
                await LoadPhotoAsync(photo);
                Console.WriteLine($"PickPhotoAsync COMPLETED:");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PickPhotoAsync THREW: {ex.Message}");
            }
        }

        async Task LoadPhotoAsync(FileResult photo)
        {
            // cancelled
            if (photo == null)
            {
                return;
            }
            // save the file to the local database
            using (var ms = new MemoryStream())
            {
                using (var inputStream = await photo.OpenReadAsync())
                {
                    await inputStream.CopyToAsync(ms);
                    var image = new ImageFile()
                    {
                        Data = ms.ToArray(),
                        ContentType = photo.ContentType,
                        Name = photo.FileName
                    };
                    Brands.ImageFile = image;
                }
            }
        }

        async void OnScanBarcodeClicked(object sender, EventArgs e)
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();

            var result = await scanner.Scan();

            if (result != null)
            {
                Brands.ProductReference = result.Text;
            }
        }

    }
}