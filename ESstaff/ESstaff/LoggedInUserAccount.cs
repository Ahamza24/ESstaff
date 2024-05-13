using ESstaff.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESstaff
{
    public class LoggedInUserAccount : DecodedAccessToken
    {
        public bool IsAdmin
        {
            get
            {
                return this.HasRole("Admin");
            }
        }

        public bool IsStaff
        {
            get
            {
                return this.HasRole("Staff");
            }
        }

        public bool IsPremium
        {
            get
            {
                return this.HasRole("Premium");
            }
        }

        public string Firstname
        {
            get
            {
                return this.FirstName;
            }
        }

        public string Surname
        {
            get
            {
                return this.SureName;
            }
        }
    }
}
