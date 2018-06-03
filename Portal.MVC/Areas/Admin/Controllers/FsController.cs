using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Services.Firends;

namespace Portal.MVC.Areas.Admin.Controllers
{
    public class FsController : AdminBaseController
    {
        //
        private readonly IFirendService _firendService;
        // GET: /Admin/Fs/
        public FsController(IFirendService firendService)
        {
            _firendService = firendService;
        }

        public ActionResult Index(int userid)
        {
            var fs=_firendService.GetAllFirends(userid);
            return View(fs);
        }

        public ActionResult Delete(int id)
        {
            _firendService.DeleteFirend(id);

            return Json(1);
        }

    }
}
