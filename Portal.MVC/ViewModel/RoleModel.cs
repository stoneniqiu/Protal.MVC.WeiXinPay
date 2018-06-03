using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.Ajax.Utilities;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.Security;
using Niqiu.Core.Domain.User;

namespace Portal.MVC.ViewModel
{
    public class RoleModel
    {
        private List<PermissionRecordModel> _permissionRecordModels;
        public RoleModel() { }
        public RoleModel(UserRole role, IList<PermissionRecord> records)
        {
            RoleId = role.Id;
            RoleName = role.Name;
            foreach (var permissionRecord in records)
            {
                PermissionRecordModels.Add(new PermissionRecordModel()
                {
                    Id = permissionRecord.Id,
                    PermissionRecordName = permissionRecord.Name,
                    Selected = role.PermissionRecords.Any(n => n.Name == permissionRecord.Name)
                });
            }
            foreach (var m in PermissionRecordModels)
            {
                if (m.PermissionRecordName == "进入后台")
                {
                    m.Selected = true;
                }
            }

        }

        public int RoleId { get; set; }

        [Required]
        [MinLength(2)]
        public string RoleName { get; set; }

        public List<PermissionRecordModel> PermissionRecordModels
        {
            get { return _permissionRecordModels ?? (_permissionRecordModels = new List<PermissionRecordModel>()); }
            set { _permissionRecordModels = value; }
        }

        public UserRole GetRole(IEnumerable<PermissionRecord> records )
        {
            var role= new UserRole()
            {
                IsSystemRole = false,
                Name = RoleName
            };

            var sps = PermissionRecordModels.Where(n => n.Selected).ToList();
            foreach (var item in sps)
            {
                var s = records.FirstOrDefault(n => n.Id == item.Id);
                role.PermissionRecords.Add(s);
            }

            return role;
        }

    }


    public class PermissionRecordModel
    {
        public int Id { get; set; }
        public string PermissionRecordName { get; set; }

        public bool Selected { get; set; }
    }
}