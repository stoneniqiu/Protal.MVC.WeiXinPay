using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Services;
using Portal.MVC.Attributes;
using Portal.MVC.ViewModel;

namespace Portal.MVC.Areas.Admin.Controllers
{
    public class RoleController : AdminBaseController
    {

        private readonly IPermissionService _permissionService;
        private readonly IUserService _userService;

        public RoleController(IPermissionService permissionService, IUserService userService)
        {
            _permissionService = permissionService;
            _userService = userService;
        }

        [AdminAuthorize("EditRole")]
        public ActionResult Index()
        {
            _permissionService.InstallPermissions(new StandardPermissionProvider());
            var roles = _userService.GetAllUserRoles();
            return View(roles);
        }

        [AdminAuthorize("EditRole")]
        public ActionResult EditRole(int id)
        {
            var role = _userService.GetUserRoleById(id);
            var allps = _permissionService.GetAllPermissionRecords();
            var model = new RoleModel(role, allps);
            return View(model);  
        }

        [HttpPost]
        [AdminAuthorize("EditRole")]
        public ActionResult EditRole(RoleModel model)
        {
            var allps = _permissionService.GetAllPermissionRecords();
            var role = _userService.GetUserRoleById(model.RoleId);
            var modelrole = model.GetRole(allps);
            role.PermissionRecords.Clear();
            role.Name = model.RoleName;
            foreach (var p in modelrole.PermissionRecords)
            {
                role.PermissionRecords.Add(p);
            }
            _userService.UpdateUserRole(role);
            Success();
            return View(model);
        }

        [AdminAuthorize("EditRole")]
        public JsonResult Delete(int id)
        {
            var role = _userService.GetUserRoleById(id);
            role.Active = false;
            _userService.UpdateUserRole(role);
            return Json(1);
        }

        [AdminAuthorize("CreateRole")]
        public ActionResult AddRole()
        {
            var role = new UserRole();
            var allps = _permissionService.GetAllPermissionRecords();
            var model = new RoleModel(role, allps);
            return View(model);
        }

        [HttpPost]
        [AdminAuthorize("CreateRole")]
        public ActionResult AddRole(RoleModel model)
        {
            var sps = model.PermissionRecordModels.Any(n => n.Selected);
            if (!sps)
            {
                Error("请选择至少一个权限,比如进入后台");
                return View(model);
            }
            var exist = _userService.GetAllUserRoles().Any(n => n.Name == model.RoleName);
            if (exist)
            {
                Error("改角色名称已经存在!");
                return View(model);
            }
            var allps = _permissionService.GetAllPermissionRecords();
            var role = model.GetRole(allps);

            _userService.InsertUserRole(role);

            Success("添加成功");
            return View(model);
        }

    }
}
