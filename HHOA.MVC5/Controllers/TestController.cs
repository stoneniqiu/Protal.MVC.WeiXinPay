using System.Web.Mvc;
using Niqiu.Core.Services;

namespace Portal.MVC5.Controllers
{
    public class TestController : Controller
    {
        //
        private FileDbService _fileService = new FileDbService();
        // GET: /Test/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Download()
        {
            var list = _fileService.FileRecords();
            return View(list);
        }

        public ActionResult AuthTest()
        {
            return View();
        }


        public ActionResult Talk()
        {
            return View();
        }

        public ActionResult Chat()
        {
            return View();
        }
    }
}
