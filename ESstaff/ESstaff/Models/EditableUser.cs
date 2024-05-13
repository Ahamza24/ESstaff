using System;
using System.Collections.Generic;
using System.Text;

namespace ESstaff.Models
{
    public class EditableUser
    {
        public string Id { get; set; }
        public bool Admin { get; set; }
        public bool Worker { get; set; }
        public bool Premium { get; set; }
        public string FirstName { get; set; }
        public string SureName { get; set; }
        public string EmailAddress { get; set; }

        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }

        public string Name
        {
            get
            {
                return $"{this.FirstName} {this.SureName}";
            }
        }
    }
}
