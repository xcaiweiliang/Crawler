using Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
namespace DAL
{
    internal partial class MenuTypeMap : EntityTypeConfiguration<Menu>
    {
        public MenuTypeMap()
        {
            this.HasKey(t => t.Id);
            this.ToTable("MGR_MENU");
            this.Property(t => t.Id).HasColumnName("ID");
            this.Property(t => t.ParentId).HasColumnName("Parent_ID");
            this.Property(t => t.Code).HasColumnName("Code");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Url).HasColumnName("URL");
            this.Property(t => t.Sort).HasColumnName("Sort");
            this.Property(t => t.Remark).HasColumnName("Remark");
            this.Ignore(t => t.IsShow);
        }
    }
}
