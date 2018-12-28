using Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
namespace DAL
{

    internal partial class RolePermissionTypeMap : EntityTypeConfiguration<RolePermission>
    {
        public RolePermissionTypeMap()
        {
            this.HasKey(t => t.Id);
            this.ToTable("MGR_Role_Permission");
            this.Property(t => t.Id).HasColumnName("ID");
            this.Property(t => t.RoleId).HasColumnName("Role_ID");
            this.Property(t => t.MenuId).HasColumnName("Menu_ID");
            this.Property(t => t.OperationId).HasColumnName("Operation_ID");
        }
    }
}
