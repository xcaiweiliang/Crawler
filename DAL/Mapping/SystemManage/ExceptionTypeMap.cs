using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
namespace DAL
{
    internal partial class ExceptionTypeMap : EntityTypeConfiguration<Model.ExceptionLog>
    {
        public ExceptionTypeMap()
        {
            this.HasKey(t => t.Id);
            this.ToTable("MGR_Exception_Log");
            this.Property(t => t.Id).HasColumnName("ID");
            this.Property(t => t.TypeCode).HasColumnName("Type_Code");
            this.Property(t => t.Account).HasColumnName("Account");
            this.Property(t => t.Surmary).HasColumnName("Surmary");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.Path).HasColumnName("Path");
            this.Property(t => t.CreateTime).HasColumnName("Create_Time");
        }
    }
}
