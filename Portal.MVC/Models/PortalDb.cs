using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.Reports;
using Niqiu.Core.Domain.Security;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Mapping.Security;
using Niqiu.Core.Mapping.User;

namespace Portal.MVC.Models
{
    public class PortalDb : DbContext, IDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> Roles { get; set; }
        public DbSet<PermissionRecord> PermissionRecords { get; set; }


        #region 谜题相关

        public DbSet<Question> Questions { get; set; }

        //问题购买服务
        public DbSet<QuestionStrategy> QuestionStrategies { get; set; } 

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Answer> Answers { get; set; }

        public DbSet<RewardUser> RewardUsers { get; set; }

        public DbSet<PraiseLog>  Praises { get; set; }

        #endregion


        #region 支付相关
        //钱包
        public DbSet<Wallet> Wallets { get; set; }
        //银行卡
        public DbSet<BankCard> BankCards { get; set; }
        //订单
        public DbSet<Order> Orders { get; set; }

        //充值记录
        public DbSet<PaymentLog> PaymentLogs { get; set; } 

        #endregion 

        #region 好友

        public DbSet<Firend> Firends { get; set; } 

        #endregion

        public DbSet<Message> Messages { get; set; }

        public DbSet<Feeback> Feebacks { get; set; }

        //统计相关
        public DbSet<MenuStatistic> MenuStatistics { get; set; } 

        /// <summary>
        /// 举报信息
        /// </summary>
        public DbSet<Report> Reports { get; set; }
        public DbSet<PhoneCode> PhoneCodes { get; set; } 

        public PortalDb()
            : base("DefaultConnection")
        {
            this.Database.Initialize(false);
            // Database.SetInitializer(new MigrateDatabaseToLatestVersion<PortalDb, Configuration<PortalDb>>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Configurations.Add(new UserMap());
            modelBuilder.Configurations.Add(new UserRoleMap());
            modelBuilder.Configurations.Add(new PermissionRecordMap());

            modelBuilder.Entity<User>().Ignore(n => n.PasswordFormat);

            modelBuilder.Entity<Question>().HasMany(n => n.Commnets).WithRequired(n => n.Question).WillCascadeOnDelete(false);
            modelBuilder.Entity<Question>().HasMany(n => n.Answers).WithRequired(n => n.Question).WillCascadeOnDelete(false);
            
            modelBuilder.Entity<User>().HasMany(n => n.Answers).WithRequired(n => n.User).WillCascadeOnDelete(false);
            modelBuilder.Entity<User>().HasMany(n => n.Messages).WithRequired(n => n.FromUser).WillCascadeOnDelete(false);
            modelBuilder.Entity<User>().HasMany(n => n.Messages).WithRequired(n => n.ToUser).WillCascadeOnDelete(false);

           

            //modelBuilder.Entity<PaymentLog>().HasOptional(n => n.FromWallet);
            //modelBuilder.Entity<PaymentLog>().HasOptional(n => n.ToWallet);
            //modelBuilder.Entity<PaymentLog>().HasOptional(n => n.Order);

            base.OnModelCreating(modelBuilder);
        }


        public new IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            if (parameters != null && parameters.Length > 0)
            {
                for (int i = 0; i <= parameters.Length - 1; i++)
                {
                    var p = parameters[i] as DbParameter;
                    if (p == null)
                        throw new Exception("Not support parameter type");

                    commandText += i == 0 ? " " : ", ";

                    commandText += "@" + p.ParameterName;
                    if (p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output)
                    {
                        //output parameter
                        commandText += " output";
                    }
                }
            }

            var result = this.Database.SqlQuery<TEntity>(commandText, parameters).ToList();

            //performance hack applied as described here - http://www.nopcommerce.com/boards/t/25483/fix-very-important-speed-improvement.aspx
            bool acd = this.Configuration.AutoDetectChangesEnabled;
            try
            {
                this.Configuration.AutoDetectChangesEnabled = false;

                for (int i = 0; i < result.Count; i++)
                    result[i] = AttachEntityToContext(result[i]);
            }
            finally
            {
                this.Configuration.AutoDetectChangesEnabled = acd;
            }
            return result;
        }

        public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            return this.Database.SqlQuery<TElement>(sql, parameters);
        }

        protected virtual TEntity AttachEntityToContext<TEntity>(TEntity entity) where TEntity : BaseEntity, new()
        {
            //little hack here until Entity Framework really supports stored procedures
            //otherwise, navigation properties of loaded entities are not loaded until an entity is attached to the context
            var alreadyAttached = Set<TEntity>().Local.FirstOrDefault(x => x.Id == entity.Id);
            if (alreadyAttached == null)
            {
                //attach new entity
                Set<TEntity>().Attach(entity);
                return entity;
            }

            //entity is already loaded
            return alreadyAttached;
        }

        public int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            int? previousTimeout = null;
            if (timeout.HasValue)
            {
                //store previous timeout
                previousTimeout = ((IObjectContextAdapter)this).ObjectContext.CommandTimeout;
                ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = timeout;
            }

            var transactionalBehavior = doNotEnsureTransaction
                ? TransactionalBehavior.DoNotEnsureTransaction
                : TransactionalBehavior.EnsureTransaction;
            var result = this.Database.ExecuteSqlCommand(transactionalBehavior, sql, parameters);

            if (timeout.HasValue)
            {
                //Set previous timeout back
                ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = previousTimeout;
            }

            //return result
            return result;
        }
    }

    internal sealed class Configuration<TContext> : DbMigrationsConfiguration<TContext> where TContext : DbContext
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }
    }
}