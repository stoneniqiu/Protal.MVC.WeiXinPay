using System.Web.Http;
using Portal.MVC5.Models;
using Niqiu.Core.Services;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Customer;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;

namespace Portal.MVC5.Controllers.API
{
    public class ClientController : ApiController
    {
        public IWebHelper WebHelper = new WebHelper();

        [HttpPost]
        public string AddClient(ClientDto dto)
        {
            if (string.IsNullOrEmpty(dto.phone)) return "104";
            if (!CommonHelper.ValidateString(dto.phone, ValidataType.Mobile))
            {
                return "101";
            }
            var ip = WebHelper.GetCurrentIpAddress();
            var model = new Customer
            {
                Ip = ip,
                Name =dto.Name,
                Mobile = dto.phone,
                Referrer = dto.sourceUrl,
                Url = dto.Url,
                SearchEngine = dto.engin,
                Keyword = dto.keyword,
                Title=dto.title
            };
            using (var db = new PortalDb())
            {
               // db.Customers.Add(model);
                db.SaveChanges();
                return "200";
            }
        }
    }
}
