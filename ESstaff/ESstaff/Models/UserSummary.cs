using System;
using System.Collections.Generic;
using System.Text;

namespace ESstaff.Models
{
    public class UserSummary
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string SureName { get; set; }
        public string EmailAddress { get; set; }

        public string Name
        {
            get
            {
                return $"{this.FirstName} {this.SureName}";
            }
        }
    }
}
