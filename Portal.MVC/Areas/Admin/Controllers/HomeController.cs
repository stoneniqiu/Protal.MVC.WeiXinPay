using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Helpers;
using Niqiu.Core.Services;
using Portal.MVC.Attributes;
using Portal.MVC.Controllers;
using Portal.MVC.Models;

namespace Portal.MVC.Areas.Admin.Controllers
{
    public class HomeController : AdminBaseController
    {
        private readonly IPermissionService _permissionService;
        private readonly IUserService _service;
        private readonly IWorkContext _context;
        public HomeController(IUserService repository, IPermissionService permissionService, IWorkContext workContext)
        {
            _permissionService = permissionService;
            _service = repository;
            _context = workContext;
        }

        public ActionResult Index()
        {
            _permissionService.InstallPermissions(new StandardPermissionProvider());
            using (var db = new PortalDb())
            {
                ViewBag.UserCount = db.Users.Count();
            }
            var user = _context.CurrentUser;
            Logger.Info(string.Format("{0}用户进入后台,id:{1}", user.Username, user.Id));
            return View();
        }


        [HttpPost]
        public ActionResult UploadImg(HttpPostedFileBase file)
        {
            if (CheckImg(file) != "ok") return Json(new { Success = false, Message = "文件格式不对！" }, JsonRequestBehavior.AllowGet);

            if (file != null)
            {
                var uploadpath = Server.MapPath("~/Content/UploadFiles/UserImg/");
                if (!Directory.Exists(uploadpath))
                {
                    Directory.CreateDirectory(uploadpath);
                }
                string fileName = Path.GetFileName(file.FileName);// 原始文件名称
                string fileExtension = Path.GetExtension(fileName); // 文件扩展名
                //string saveName = Guid.NewGuid() + fileExtension; // 保存文件名称 这是个好方法。
                string saveName = Encrypt.GenerateOrderNumber() + fileExtension; // 保存文件名称 这是个好方法。
                file.SaveAs(uploadpath + saveName);

                return Json(new { Success = true, SaveName = "/Content/UploadFiles/UserImg/" + saveName });
            }

            return Json(new { Success = false, Message = "请选择要上传的文件！" }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult UploadVideo(HttpPostedFileBase file)
        {
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension != ".mp4")
                {
                    return Json(new { Success = false, Message = "文件格式不对！" }, JsonRequestBehavior.AllowGet);

                }

                var uploadpath = Server.MapPath("~/Content/videos/");
                if (!Directory.Exists(uploadpath))
                {
                    Directory.CreateDirectory(uploadpath);
                }
                string fileName = Path.GetFileName(file.FileName);// 原始文件名称
                string fileExtension = Path.GetExtension(fileName); // 文件扩展名
                //string saveName = Guid.NewGuid() + fileExtension; // 保存文件名称 这是个好方法。
                string saveName = Encrypt.GenerateOrderNumber() + fileExtension; // 保存文件名称 这是个好方法。
                file.SaveAs(uploadpath + saveName);

                return Json(new { Success = true, SaveName = "/Content/videos/" + saveName });
            }

            return Json(new { Success = false, Message = "请选择要上传的文件！" }, JsonRequestBehavior.AllowGet);

        }

        private const int Width = 70;
        private const int Height = 50;
        //会生产缩略图
        public ActionResult UploadImgAndThumbnail(HttpPostedFileBase file)
        {
            if (CheckImg(file) != "ok") return Json(new { Success = false, Message = "文件格式不对！" }, JsonRequestBehavior.AllowGet);

            if (file != null)
            {
                var path = "/Content/UploadFiles/Slider/";
                var uploadpath = Server.MapPath(path);
                if (!Directory.Exists(uploadpath))
                {
                    Directory.CreateDirectory(uploadpath);
                }
                string fileName = Path.GetFileName(file.FileName);// 原始文件名称
                string fileExtension = Path.GetExtension(fileName); // 文件扩展名
                //string saveName = Guid.NewGuid() + fileExtension; // 保存文件名称 这是个好方法。
                var random = Encrypt.GenerateOrderNumber();
                string saveName = random + fileExtension; // 保存文件名称 这是个好方法。
                var savepath = uploadpath + saveName;
                file.SaveAs(uploadpath + saveName);
                var thumbnailName = random + "_" + Width + fileExtension;
                ImageManageHelper.GetThumbnail(savepath, uploadpath + thumbnailName, Width, Height);
                return Json(new { Success = true, SaveName = path + saveName, ThumbnailName = path + thumbnailName });
            }

            return Json(new { Success = false, Message = "请选择要上传的文件！" }, JsonRequestBehavior.AllowGet);
        }

        private string[] imgtypes = { ".bmp", ".png", ".gif", ".jpg", ".jpeg" };
        /// <summary>
        /// 核对图片
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private string CheckImg(HttpPostedFileBase file)
        {
            if (file == null) return "图片不能空！";
            var extension = Path.GetExtension(file.FileName);
            if (extension != null)
            {
                var image = extension.ToLower();
                return imgtypes.Contains(image) ? "ok" : "文件格式不对";
            }
            return "ok";
        }

    }
}
