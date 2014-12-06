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
    
    public partial class City
    {
        public City()
        {
            this.HumanBasicFile = new HashSet<HumanBasicFile>();
            this.Managers = new HashSet<Managers>();
            this.SShop = new HashSet<SShop>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public int Code { get; set; }
        public Nullable<int> OfficeId { get; set; }
    
        public virtual Office Office { get; set; }
        public virtual ICollection<HumanBasicFile> HumanBasicFile { get; set; }
        public virtual ICollection<Managers> Managers { get; set; }
        public virtual ICollection<SShop> SShop { get; set; }
    }
}