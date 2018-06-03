using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Niqiu.Core.Domain.Common;

namespace Niqiu.Core.Services
{
    public class PhoneCodeSerice : IPhoneCodeSerice
    {
        private readonly IRepository<PhoneCode> _pRepository;
        public PhoneCodeSerice(IRepository<PhoneCode> repository)
        {
            _pRepository = repository;
        }

        public void Insert(PhoneCode model)
        {
           _pRepository.Insert(model);
        }

        public bool Valid(string code, string phone)
        {
            //多长时间内有效  15分钟呢
            var endTime = DateTime.Now.AddMinutes(-15);
            return _pRepository.Table.Any(n => n.CreateTime >= endTime && n.Mobile == phone && n.Code == code);
        }
    }
}
