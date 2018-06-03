using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain.Common;
using Portal.MVC.Attributes;

namespace Portal.MVC.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminBaseController : Controller
    {
        public void Success(string str = "操作成功")
        {
            TempData["msg"] = str;
        }

        public void Error(string str = "操作失败")
        {
            TempData["err"] = str;
        }
        public FileResult ExportExcel(DataTable table, string path)
        {
            MemoryStream ms = ExcelRender.RenderToExcel(table);

            return File(ms, "application/vnd.ms-excel", path);
        }

    }
}
