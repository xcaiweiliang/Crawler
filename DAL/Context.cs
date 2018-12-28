using Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public partial class Context : DbContext
    {
        static Context()
        {
            Database.SetInitializer<Context>(null);
            //System.Data.Entity.Database.SetInitializer(new DropCreateDatabaseIfMODELChanges<BloggingContext>());
        }

        public Context()
            : base(Common.Utility.GetConnectionString())
        {
            this.Database.Initialize(false);
            this.Database.Log = (x) =>
            {
                System.Diagnostics.Debug.Write(x);
            }; //Console.Write;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new A_LeagueMatchTypeMap());
            modelBuilder.Configurations.Add(new A_MatchScoreRecordTypeMap());
            modelBuilder.Configurations.Add(new A_MatchTypeMap());
            modelBuilder.Configurations.Add(new A_TeamTypeMap());
            modelBuilder.Configurations.Add(new A_MatchResultTypeMap());

            modelBuilder.Configurations.Add(new O_OddsTypeMap());
            modelBuilder.Configurations.Add(new O_OddsRecordTypeMap());
            modelBuilder.Configurations.Add(new S_BetCodeTypeMap());
            modelBuilder.Configurations.Add(new S_SectionTypeMap());
            
            
            #region 系统管理
          
            modelBuilder.Configurations.Add(new ExceptionTypeMap());
         

            #endregion

            #region 权限
            modelBuilder.Configurations.Add(new MenuTypeMap());
            modelBuilder.Configurations.Add(new OperationTypeMap());
            modelBuilder.Configurations.Add(new RoleTypeMap());
            modelBuilder.Configurations.Add(new UserTypeMap());
            modelBuilder.Configurations.Add(new MenuOperationTypeMap());
            modelBuilder.Configurations.Add(new RolePermissionTypeMap());
            modelBuilder.Configurations.Add(new RoleUserTypeMap());
            #endregion
        }

        public virtual DbSet<A_LeagueMatch> A_LeagueMatch { get; set; }
        public virtual DbSet<A_MatchScoreRecord> A_MatchScoreRecord { get; set; }
        public virtual DbSet<A_Match> A_Match { get; set; }
        public virtual DbSet<A_Team> A_Team { get; set; }
        public virtual DbSet<A_MatchResult> A_MatchResult { get; set; }

        public virtual DbSet<O_Odds> O_Odds { get; set; }
        public virtual DbSet<O_OddsRecord> O_OddsRecord { get; set; }
        public virtual DbSet<S_BetCode> S_BetCode { get; set; }

        public virtual DbSet<S_Section> S_Section { get; set; }
        

        #region 权限
        /// <summary>
        /// 菜单
        /// </summary>
        public IDbSet<Menu> Menus { get; set; }

        /// <summary>
        /// 操作
        /// </summary>
        public IDbSet<Operation> Operations { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        public IDbSet<Role> Roles { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public IDbSet<User> Users { get; set; }

        /// <summary>
        /// 菜单操作
        /// </summary>
        public IDbSet<MenuOperation> MenuOperations { get; set; }


        /// <summary>
        /// 角色权限表
        /// </summary>
        public IDbSet<RolePermission> RolePermissions { get; set; }

        /// <summary>
        /// 角色用户
        /// </summary>
        public IDbSet<RoleUser> RoleUsers { get; set; }


        #endregion

        #region 系统管理
     
        public virtual DbSet<ExceptionLog> ExceptionLog { get; set; }
     

        #endregion
    }
}
