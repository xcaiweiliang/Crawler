using Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
namespace DAL
{

    internal partial class UserTypeMap : EntityTypeConfiguration<User>
    {
        public UserTypeMap()
        {
            this.HasKey(t => t.Id);
            this.ToTable("MGR_USER");
            this.Property(t => t.Id).HasColumnName("ID");
            this.Property(t => t.PlatCode).HasColumnName("PLAT_CODE");
            this.Property(t => t.Account).HasColumnName("ACCOUNT");
            this.Property(t => t.Password).HasColumnName("PASSWORD");
            this.Property(t => t.Name).HasColumnName("NAME");
            this.Property(t => t.Enable).HasColumnName("Enable");
            this.Property(t => t.Remark).HasColumnName("Remark");
            this.Property(t => t.CreateTime).HasColumnName("CREATE_TIME");
            this.Property(t => t.LastLoginTime).HasColumnName("LAST_LOGIN_TIME");
            this.Property(t => t.TryNum).HasColumnName("TRY_NUM");
            this.Property(t => t.DisabledTime).HasColumnName("DISABLED_TIME");


            this.Ignore(t => t.IsEnable);
            this.Ignore(t => t.Roles);
        }
    }
}
