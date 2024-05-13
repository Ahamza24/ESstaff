using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ESstaff.Models
{
    public class ImageFile
    {
        public int Id { get; set; }

        public byte[] Data { get; set; }

        public string Name { get; set; }
        public string ContentType { get; set; }
        public DateTime ModifiedOn { get; set; }

        [JsonIgnore]
        public string Extension { get => Path.GetExtension(this.Name); }

        public ImageFile()
        {
            this.ModifiedOn = DateTime.Now;
        }
    }
}
