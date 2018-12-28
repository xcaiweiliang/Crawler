using Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
namespace DAL
{
    internal partial class RoleTypeMap : EntityTypeConfiguration<Role>
    {
        public RoleTypeMap()
        {
            this.HasKey(t => t.Id);
            this.ToTable("MGR_ROLE");
            this.Property(t => t.Id).HasColumnName("ID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Remark).HasColumnName("Remark");
        }
    }
}