using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Niqiu.Core.Domain.Common;

namespace Niqiu.Core.Services
{
   public interface IPhoneCodeSerice
   {
       void Insert(PhoneCode model);

       bool Valid(string code, string phone);
   }
}
