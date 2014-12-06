//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Management.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Managers
    {
        public Managers()
        {
            this.HumanBasicFile = new HashSet<HumanBasicFile>();
            this.LoginLog = new HashSet<LoginLog>();
            this.WorkContent = new HashSet<WorkContent>();
        }
    
        public System.Guid Id { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public int Authority { get; set; }
        public string Remark { get; set; }
        public string Name { get; set; }
        public Nullable<bool> Sex { get; set; }
        public string Boss { get; set; }
        public string IDcard { get; set; }
        public string BankCard { get; set; }
        public Nullable<int> Bank { get; set; }
        public string School { get; set; }
        public string Telephone { get; set; }
        public Nullable<int> City { get; set; }
        public string Major { get; set; }
        public Nullable<System.DateTime> Graduate { get; set; }
        public string Academic { get; set; }
        public Nullable<decimal> height { get; set; }
        public Nullable<decimal> weight { get; set; }
        public string BWH { get; set; }
        public string Speciality { get; set; }
        public string Photo { get; set; }
        public string StudentCardPhoto { get; set; }
        public string CreatedManId { get; set; }
        public string EditManId { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string Email { get; set; }
        public bool IsDraft { get; set; }
        public Nullable<bool> IsDelete { get; set; }
    
        public virtual Authority Authority1 { get; set; }
        public virtual Bank Bank1 { get; set; }
        public virtual City City1 { get; set; }
        public virtual ICollection<HumanBasicFile> HumanBasicFile { get; set; }
        public virtual ICollection<LoginLog> LoginLog { get; set; }
        public virtual ICollection<WorkContent> WorkContent { get; set; }
    }
}