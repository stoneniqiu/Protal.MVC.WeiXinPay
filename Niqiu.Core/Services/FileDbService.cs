using System.Collections.Generic;
using System.Linq;
using Niqiu.Core.Domain.Files;

namespace Niqiu.Core.Services
{
    public class FileDbService
    {
        private PortalDb db = new PortalDb();

        public bool Insert(FileRecord model)
        {
            if (model == null || string.IsNullOrEmpty(model.RawName)||model.Size<=0) return false;

            using(var _db=new PortalDb()){
                _db.FileRecords.Add(model);
                _db.SaveChanges();
                return true;
            }
        }

        public void Remove(FileRecord model)
        {
            using (var _db = new PortalDb())
            {
               var f =_db.FileRecords.Find(model.Id);
               _db.FileRecords.Remove(f);
                _db.SaveChanges();
            }
        }

        public void Remove(string guid)
        {
            using (var _db = new PortalDb())
            {
                var f = _db.FileRecords.FirstOrDefault(n=>n.GuId==guid);
                _db.FileRecords.Remove(f);
                _db.SaveChanges();
            }
        }

        /// <summary>
        /// 是否存在这个文件
        /// </summary>
        /// <param name="md5"></param>
        /// <returns></returns>
        public bool Check(string md5)
        {
            return db.FileRecords.Any(n => n.MD5 == md5);
        }

        public FileRecord GetFile(string md5)
        {
            return db.FileRecords.FirstOrDefault(n => n.MD5 == md5);
        }
        public FileRecord GetFileByGuid(string guid)
        {
            return db.FileRecords.FirstOrDefault(n => n.GuId== guid);
        }


        public List<FileRecord> FileRecords()
        {
            return db.FileRecords.ToList();
        }

        public List<FileRecord> GetFileByUserId(string id)
        {
            return db.FileRecords.Where(n =>n.UserGuid == id).ToList();
        }

    }
}