using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;
using Niqiu.Core.Services;
using Niqiu.Core.Services.Questiones;
using Portal.MVC.Attributes;
using Portal.MVC.Controllers;
using Portal.MVC.Models;
using Portal.MVC.ViewModel;
using System.Transactions;

namespace Portal.MVC.Areas.Admin.Controllers
{


    public class UserController : AdminBaseController
    {
        private readonly IQuestionService _questionService;
        private readonly IUserService _service;

        public UserController(IUserService repository, IQuestionService questionService)
        {
            _service = repository;
            _questionService = questionService;
        }
        [AdminAuthorize("EditUser")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListPage()
        {
            var us = _service.GetAllUsers();
            return PartialView(us);
        }


        //public ActionResult UserPage(string name = "",int page = 0, int take = 20)
        //{
        //    using (var db = new PortalDb())
        //    {
        //        if (page < 0)
        //        {
        //            page = 0;
        //        }
        //        IQueryable<User>  query = db.Users.Where(n=>!n.Deleted);
        //        if (!String.IsNullOrWhiteSpace(name))
        //            query = query.Where(c => c.Username.Contains(name) || c.RealName.Contains(name));//c.RealName.Contains(username)||
        //        query = query.Include(n=>n.UserRoles).OrderByDescending(c => c.Id);

        //        var users = new PagedList<User>(query, page, take);
        //        return PartialView(users);
        //    }
        //}

        public ActionResult PageWarper(string name = "", string schoolname = "",string  number="")
        {
            ViewBag.UserName = name;
            ViewBag.SchoolName = schoolname;
            ViewBag.Number = number;
            return PartialView();
        }

        //[Ninject.Inject]
        [AdminAuthorize("CreateUser")]
        public ActionResult Create(int id = 0)
        {
            ViewBag.Des = "新增操作员";
            RegisterModel user;
            if (id == 0)
            {
               user = new RegisterModel(id);
            }
            else
            {
                var rawuser = _service.GetUserById(id);
                if (rawuser == null) return View("NoData");
                ViewBag.Des = "编辑用户";
                user=new RegisterModel(rawuser);
                Session["NameKey"] = rawuser.Username;
            }

            var roles = _service.GetAllUserRoles().TakeWhile(n=>n.SystemName!=SystemUserRoleNames.Administrators);
            ViewBag.Roles = new SelectList(roles, "Id", "Name"); 

            return View(user);
        }

        [HttpPost]
        [AdminAuthorize("CreateUser")]
        public ActionResult Create(RegisterModel model)
        {
            ViewBag.Des = "新增操作员";
            var roles = _service.GetAllUserRoles().TakeWhile(n => n.SystemName != SystemUserRoleNames.Administrators);
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            User newuser;
            if (model.Id == 0)
            {
                var user = new User
                {
                    UserGuid = Guid.NewGuid(),
                    Username = model.UserName,
                    Email = model.Email,
                    Mobile = model.Mobile,
                    Active = true,
                    //加密存储
                    Password = Encrypt.GetMd5Code(model.Password),
                };
                //默认增加注册角色
                // 先插入
                _service.InsertUser(user);
                newuser = _service.GetUserByUsername(user.Username);
            }
            else
            {
                newuser = _service.GetUserById(model.Id);
                newuser.Username = model.UserName;
                newuser.Password = Encrypt.EncryptString(model.Password);
                newuser.Email = model.Email;
                newuser.Mobile = model.Mobile;
                ViewBag.Des = "新增用户";
            }

            var role = _service.GetUserRoleById(model.RoleId);
            //先只有一个角色
            newuser.UserRoles.Clear();
            newuser.UserRoles.Add(role);

            try
            {
                _service.UpdateUser(newuser);
                Success();
                model.Empty();
            }
            catch (Exception e)
            {
                Error(e.Message);
            }
            return View(model);
        }
        public JsonResult CheckMoble(string mobile)
        {
            var result = _service.GetUserByMobile(mobile) == null;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [AdminAuthorize("EditUser")]
        public ActionResult Edit(int id)
        {
            var user = _service.GetUserById(id);
            if (user == null) return View("NoData");
            return View(user);
        }

        [HttpPost]
        [AdminAuthorize("EditUser")]
        public ActionResult Edit(User model)
        {
            if (ModelState.IsValid)
            {
                var user = _service.GetUserById(model.Id);
                if (user != null)
                {
                    user.Email = model.Email;
                    user.Mobile = model.Mobile;
                    user.RealName = model.RealName;
                    user.WeiXinId = model.WeiXinId;
                    _service.UpdateUser(user);
                    Success();
                    return View(model);
                }
                Error();
            }
            return View(model);
        }

        public JsonResult CheckUserName(string username)
        {
            if (Session["NameKey"] != null)
            {
                //说明正在编辑用户。
                if (string.IsNullOrWhiteSpace(username) || username.Length < 2)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                //不能和其他人同名。
                //说明不是本人
                if (Session["NameKey"].ToString() != username)
                {
                    var tag = _service.GetUserByUsername(username);
                    return Json(tag==null, JsonRequestBehavior.AllowGet);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            var haSame = _service.GetUserByUsername(username);
            return Json(haSame == null, JsonRequestBehavior.AllowGet);
        }

        public FileResult ExportUser()
        {
            var res = _service.GetAllUsers();
            var len = res.Count;
            DataTable table = new DataTable();
            table.Columns.Add("登录名", typeof(string));
            table.Columns.Add("真实姓名", typeof(string));
            table.Columns.Add("角色", typeof(string));
            table.Columns.Add("手机号", typeof(string));
            table.Columns.Add("创建时间", typeof(DateTime));
            table.Columns.Add("最后登录时间", typeof(DateTime));
            for (int i = 0; i < len; i++)
            {
                table.Rows.Add(res[i].Username, res[i].RealName, res[i].RoleName(), res[i].Mobile, res[i].CreateTime, res[i].LastLoginDateUtc);
            }

            return ExportExcel(table, "用户.xls");
        }

    

        public ActionResult UserImport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UserImport(HttpPostedFileBase filebase)
        {
            HttpPostedFileBase file = Request.Files["files"];
            string FileName;
            string savePath;
            if (file == null || file.ContentLength <= 0)
            {
                ViewBag.error = "文件不能为空";
                return View();
            }
            else
            {
                string filename = Path.GetFileName(file.FileName);
                int filesize = file.ContentLength;//获取上传文件的大小单位为字节byte
                string fileEx = System.IO.Path.GetExtension(filename);//获取上传文件的扩展名
                string NoFileName = System.IO.Path.GetFileNameWithoutExtension(filename);//获取无扩展名的文件名
                int Maxsize = 4000 * 1024;//定义上传文件的最大空间大小为4M
                string FileType = ".xls,.xlsx";//定义上传文件的类型字符串

                FileName = NoFileName + DateTime.Now.ToString("yyyyMMddhhmmss") + fileEx;
                if (!FileType.Contains(fileEx))
                {
                    ViewBag.error = "文件类型不对，只能导入xls和xlsx格式的文件";
                    return View();
                }
                if (filesize >= Maxsize)
                {
                    ViewBag.error = "上传文件超过4M，不能上传";
                    return View();
                }
                string path = AppDomain.CurrentDomain.BaseDirectory + @"Content\UploadFiles\excel";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                savePath = Path.Combine(path, FileName);
                file.SaveAs(savePath);
            }


            byte[] filebBytes = new byte[file.ContentLength];
            file.InputStream.Read(filebBytes, 0, file.ContentLength);

            var table = ExcelRender.RenderFromExcel(new MemoryStream(filebBytes), 0, 0);

            List<User> users = new List<User>();
            int casenum = 0;
            //引用事务机制，出错时，事物回滚
            try
            {
                //sing (TransactionScope transaction = new TransactionScope())
                {
                    var db = new PortalDb();
                        
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        if (table.Rows[i][0].ToString().Trim() == "")
                        {
                            break;
                        }

                        var ou = new User()
                        {
                            RealName = table.Rows[i][1].ToString(),
                        };
                        casenum = i;
                        //_service.InsertUser(ou);
                       // db.Users.Add(ou);
                        //if (!db.Schools.Any(n => n.Name == ou.Username))
                        //{
                        //    db.Schools.Add(new School() {Name = ou.SchoolName});
                        //}
                        ////_service.AddSchoolName(ou.SchoolName);
                        users.Add(ou);
                    }
                    if (db.Database.Connection.State != ConnectionState.Open)
                    {
                        db.Database.Connection.Open(); // Connection连接  
                    } 
                    //调用BulkInsert方法,将entitys集合数据批量插入到数据库的tolocation表中  
                    BulkInsert<User>((SqlConnection)db.Database.Connection, "Users", users);

                    if (db.Database.Connection.State != ConnectionState.Closed)
                    {
                        db.Database.Connection.Close(); //关闭Connection连接  
                    }  
                    }

                   // transaction.Complete();
                    ViewBag.error = "导入成功";
            }
            catch (Exception e)
            {
                users.Clear();
                System.IO.File.Delete(savePath);
                ViewBag.error = string.Format("导入失败,姓名{0},错误：{1}", table.Rows[casenum][0], e.Message);
            }

            return View(users);
        }
        public static void BulkInsert<T>(SqlConnection conn, string tableName, IList<T> list)
        {
            using (var bulkCopy = new SqlBulkCopy(conn))
            {
                bulkCopy.BatchSize = list.Count;
                bulkCopy.DestinationTableName = tableName;

                var table = new DataTable();
                var props = TypeDescriptor.GetProperties(typeof(T))

                    .Cast<PropertyDescriptor>()
                    .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                    .ToArray();

                foreach (var propertyInfo in props)
                {
                    bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                    table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                }

                var values = new object[props.Length];
                foreach (var item in list)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = props[i].GetValue(item);
                    }

                    table.Rows.Add(values);
                }

                bulkCopy.WriteToServer(table);
            }
        }  

        public FileResult GetFile()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"Content\UploadFiles\excel\模板\";
            string fileName = "用户导入模板.xls";
            return File(path + fileName, "text/plain", fileName);
        }

    


        #region  历史参与

        public ActionResult JoinedQuestions(int userid)
        {
            var qs =
                _questionService.QuestionsTable()
                    .Include(n => n.Answers)
                    .Where(m => m.Answers.Any(n => n.UserId == userid))
                    .Include(n => n.RewardUsers);
            return View(qs);
        }


        #endregion 



        //
        // POST: /User/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                using (var db = new PortalDb())
                {
                    var taget = db.Users.Find(id);
                    db.Users.Remove(taget);
                    db.SaveChanges();
                    return Json(1, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 检查是否有同名邮件
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public JsonResult CheckMail(string email)
        {
            var result = _service.GetUserByEmail(email) == null;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}