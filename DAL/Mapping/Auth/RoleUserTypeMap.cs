using Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
namespace DAL
{

    internal partial class RoleUserTypeMap : EntityTypeConfiguration<RoleUser>
    {
        public RoleUserTypeMap()
        {
            this.HasKey(t => t.Id);
            this.ToTable("MGR_ROLE_USER");
            this.Property(t => t.Id).HasColumnName("ID");
            this.Property(t => t.RoleId).HasColumnName("Role_ID");
            this.Property(t => t.UserId).HasColumnName("User_ID");
        }
    }
}
