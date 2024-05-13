using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Xamarin.Forms;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ESstaff.Models
{
    public class Brands : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }

        public string BrandName { get; set; }

        public int EthicalScore { get; set; }

        public string Link1 { get; set; }

        public string Link2 { get; set; }

       



        private string productReference; // backing field
        public string ProductReference // property
        {
            get
            {
                return this.productReference;
            }
            set
            {
                this.productReference = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProductReference)));
            }
        }

        



        public int? ImageFileId { get; set; }

        private ImageFile imageFile;
        public ImageFile ImageFile
        {
            get
            {
                return this.imageFile;
            }
            set
            {
                this.imageFile = value;
                if (this.ImageFile != null)
                {
                    this.ImageFileId = this.ImageFile.Id;
                }
                else
                {
                    this.ImageFileId = null;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageFile)));
            }
        }


        public string ModifiedByUserId { get; set; }

    }
}
