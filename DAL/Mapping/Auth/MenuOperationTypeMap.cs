using Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
namespace DAL
{


    internal partial class MenuOperationTypeMap : EntityTypeConfiguration<MenuOperation>
    {
        public MenuOperationTypeMap()
        {
            this.HasKey(t => t.Id);
            this.ToTable("MGR_Menu_Operation");
            this.Property(t => t.Id).HasColumnName("ID");
            this.Property(t => t.MenuId).HasColumnName("Menu_ID");
            this.Property(t => t.OperationId).HasColumnName("Operation_ID");
        }
    }
}
