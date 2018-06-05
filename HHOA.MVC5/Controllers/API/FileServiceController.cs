using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Files;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;
using Niqiu.Core.Services;
using Portal.MVC5.Attributes;

namespace Portal.MVC5.Controllers.API
{
    public class FileServiceController : ApiController
    {
        private string Root = "Files";
        private FileDbService _fileService = new FileDbService();
        private UserDbService _userService = new UserDbService();
        public IWebHelper WebHelper = new WebHelper();


        [HttpPost]
        [ValidToken]
        public Result Upload()
        {
            Result result = new Result();
            var tokens = _userService.DecryptToken();
            var userId =tokens[0];
            var userName = tokens[1];

            //获取配置文件
            var maxSize = ConfigHelper.GetUploadMaxSize();
            var maxTotal =ConfigHelper.GetUploadNum();

            HttpFileCollection filelist = HttpContext.Current.Request.Files;
            var recordlist=new List<FileRecord>();
            if (filelist.Count > 0)
            {
                //先过滤文件
                var filecol = new List<HttpPostedFile>();
                for (int i = 0; i < filelist.Count; i++)
                {
                    if (filelist[i].ContentLength >0)
                    {
                        filecol.Add(filelist[i]);
                    }
                }
                //大小限制
                if (filecol.Sum(n => n.ContentLength) > maxSize)
                {
                    return new Result("002","文件太大");
                }

                if (filecol.Count == 0)
                {
                   return result = new Result("000");
                }

                for (int i = 0; i < filecol.Count; i++)
                {
                    if (i > maxTotal) break;
                    HttpPostedFile file = filelist[i];
                    var savePath = HostingEnvironment.MapPath("~/" + Root);
                    String FilePath = savePath + "/" + userName + "/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";
                    string filename = file.FileName;
                    string diff = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    DirectoryInfo di = new DirectoryInfo(FilePath);
                    if (!di.Exists) { di.Create(); }

                    var md5 = Encrypt.GetMD5HashFromFile(file.InputStream);
                    var saveName = FilePath + diff + "-" + filename;
                    var fr = new FileRecord()
                    {
                        RawName = filename,
                        SavePath = saveName,
                        UserGuid = userId,
                        MD5 = md5,
                        Ip = WebHelper.GetCurrentIpAddress(),
                        Size = file.ContentLength,
                        WebPath = "/" + Root + "/" + userName + "/" + DateTime.Now.ToString("yyyy-MM-dd") + "/" + diff + "-" + filename,
                    };

                    if (_fileService.Check(md5))
                    {
                        //再检测下文件是否真的存在
                        var oldfile = _fileService.GetFile(md5);
                        fr.SavePath = oldfile.SavePath;
                        fr.WebPath = oldfile.WebPath;
                        _fileService.Insert(fr);
                        //还是要存在一次数据库 便于管理
                        recordlist.Add(fr);
                        if (filecol.Count == 1)
                        {
                            return new Result("001", "文件已经存在") { Data = recordlist, Count = recordlist.Count };
                        }
                    }
                    else
                    {
                        try
                        {
                            file.SaveAs(saveName);
                            _fileService.Insert(fr);
                            recordlist.Add(fr);
                        }
                        catch (Exception ex)
                        {
                            result = new Result("004", "上传文件写入失败：" + ex.Message);
                        }
                    }

                   
                }

                if (filecol.Count > maxTotal)
                {
                    //这里要注意
                    return new Result("001", string.Format("文件个数超过限制，前{0}，已结上传完成", maxTotal));
                }
            }
            else
            {
                result = new Result("000") ;
            }
            //{ Data = recordlist,Count = recordlist.Count}
            result.Data = recordlist;
            result.Count = recordlist.Count;
            return result;
        }

        [HttpGet]
        public HttpResponseMessage Download(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            try
            {
                var record = _fileService.GetFileByGuid(guid);
                //返回文件流
                var FilePath = System.Web.Hosting.HostingEnvironment.MapPath(record.WebPath);
                var stream = new FileStream(FilePath, FileMode.Open);
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(stream);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = record.RawName
                };

                return response;
            }
            catch (Exception e)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

        }
    }
}
