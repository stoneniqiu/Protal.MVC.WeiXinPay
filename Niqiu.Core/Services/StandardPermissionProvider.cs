using System.Collections.Generic;
using Niqiu.Core.Domain.Security;
using Niqiu.Core.Domain.User;

namespace Niqiu.Core.Services
{
    /// <summary>
    /// Standard permission provider
    /// </summary>
    public partial class StandardPermissionProvider : IPermissionProvider
    {
        //进入后台
        public static readonly PermissionRecord AccessAdminPanel = new PermissionRecord { Name = "进入后台", SystemName = "AccessAdminPanel", Category = "Standard" };
        //角色管理
        public static readonly PermissionRecord EditRole = new PermissionRecord { Name = "编辑角色", SystemName = "EditRole", Category = "Standard" };
        public static readonly PermissionRecord CreateRole = new PermissionRecord { Name = "添加角色", SystemName = "CreateRole", Category = "Standard" };
        public static readonly PermissionRecord EditUser = new PermissionRecord { Name = "编辑用户", SystemName = "EditUser", Category = "Standard" };
        public static readonly PermissionRecord CreateUser = new PermissionRecord { Name = "添加用户", SystemName = "CreateUser", Category = "Standard" };
        public static readonly PermissionRecord EditAdmin = new PermissionRecord { Name = "编辑管理员", SystemName = "EditAdmin", Category = "Standard" };
        public static readonly PermissionRecord CreateAdmin = new PermissionRecord { Name = "添加管理员", SystemName = "CreateAdmin", Category = "Standard" };
        //管理产品
        //public static readonly PermissionRecord ManageProducts = new PermissionRecord { Name = "管理产品", SystemName = "ManageProducts", Category = "Catalog" };
        ////管理分类
        //public static readonly PermissionRecord ManageCategories = new PermissionRecord { Name = "管理分类", SystemName = "ManageCategories", Category = "Catalog" };
        ////管理用户
        //public static readonly PermissionRecord ManageUsers = new PermissionRecord { Name = "管理用户", SystemName = "ManageUsers", Category = "Users" };
        ////管理订单
        //public static readonly PermissionRecord ManageOrders = new PermissionRecord { Name = "管理订单", SystemName = "ManageOrders", Category = "Orders" };
        ////管理新闻
        //public static readonly PermissionRecord ManageNews = new PermissionRecord { Name = "管理新闻", SystemName = "ManageNews", Category = "Content Management" };
        ////管理博客 及文案
        //public static readonly PermissionRecord ManageBlog = new PermissionRecord { Name = "管理博客", SystemName = "ManageBlog", Category = "Content Management" };
        ////管理插件
        //public static readonly PermissionRecord ManagePlugins = new PermissionRecord { Name = "管理插件", SystemName = "ManagePlugins", Category = "Configuration" };
        ////管理话题
        //public static readonly PermissionRecord ManageTopics = new PermissionRecord { Name = "管理话题", SystemName = "ManageTopics", Category = "Content Management" };
        ////管理论坛
        //public static readonly PermissionRecord ManageForums = new PermissionRecord { Name = "管理论坛", SystemName = "ManageForums", Category = "Content Management" };
        ////管理系统日志
        //public static readonly PermissionRecord ManageSystemLog = new PermissionRecord { Name = "管理日志", SystemName = "ManageSystemLog", Category = "Configuration" };
     
        ////管理软件下载
        //public static readonly PermissionRecord ManageDownloadFiles = new PermissionRecord { Name = "管理文件", SystemName = "DownloadFiles", Category = "Content Management" };
        ////管理工程师-->指定谁是工程师
        //public static readonly PermissionRecord ManageEngineers = new PermissionRecord { Name = "管理工程师", SystemName = "ManageEngineers", Category = "Users" };
        ////工程师才有权利回答问题
        //public static readonly PermissionRecord ManageQuestiones = new PermissionRecord { Name = "管理问题", SystemName = "ManageQuestiones", Category = "Content Management" };

        ////给客户的权限 可以向工程师提问
        //public static readonly PermissionRecord AskQuestion = new PermissionRecord { Name = "管理问答", SystemName = "AskQuestion", Category = "Support" };

        ////管理商户
        //public static readonly PermissionRecord ManageTenant = new PermissionRecord { Name = "管理商户", SystemName = "ManageTenant", Category = "Users" };

        //public static readonly PermissionRecord SearchOrder = new PermissionRecord { Name = "订单查询", SystemName = "SearchOrder", Category = "Users" };
         


        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[] 
            {
                AccessAdminPanel,EditRole,CreateRole,EditUser,CreateUser,CreateAdmin,EditAdmin
            };
        }


        public virtual IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return new[] 
            {
                new DefaultPermissionRecord 
                {
                    UserRoleSystemName = SystemUserRoleNames.Administrators,
                    PermissionRecords =GetPermissions() 
                },
                new DefaultPermissionRecord
                {
                   UserRoleSystemName   = SystemUserRoleNames.Admin,
                   PermissionRecords = new []
                   {
                       AccessAdminPanel,
                       EditUser,
                       CreateUser,
                   }
                },

               new DefaultPermissionRecord
                {
                   UserRoleSystemName   = SystemUserRoleNames.Employeer,
                   PermissionRecords = new []
                   {
                       AccessAdminPanel,
                   }
                },
 
            };
        }
    }
}