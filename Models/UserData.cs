//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LoginRegistrationApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserData
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string Lastname { get; set; }
        public string EmailID { get; set; }
        public Nullable<System.DateTime> DataOfBirth { get; set; }
        public string Password { get; set; }
        public bool IsEmailVerified { get; set; }
        public System.Guid Activationcode { get; set; }
    }
}
