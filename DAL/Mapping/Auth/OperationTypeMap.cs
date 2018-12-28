using Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
namespace DAL
{



    internal partial class OperationTypeMap : EntityTypeConfiguration<Operation>
    {
        public OperationTypeMap()
        {
            this.HasKey(t => t.Id);
            this.ToTable("MGR_OPERATION"); 
            this.Property(t => t.Id).HasColumnName("ID");
            this.Property(t => t.Code).HasColumnName("Code");
            this.Property(t => t.Name).HasColumnName("Name");
        }
    }
}
