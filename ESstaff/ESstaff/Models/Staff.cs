using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ESstaff.Models
{
    public class Staff : LoginUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string SureName { get; set; }
        public int PhoneNumber { get; set; }

        public string Name
        {
            get
            {
                return $"{this.FirstName} {this.SureName}";
            }
        }


    }
}
