using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Common
{
    public class PortalResult
    {
        public PortalResult()
        {
            IsSuccess = false;
        }
        public PortalResult(bool isSuccess, string msg = "操作成功")
        {
            IsSuccess = isSuccess;
            Message = msg;
        }
        public PortalResult(string msg)
        {
            IsSuccess = false;
            Message = msg;
        }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public int Num { get; set; }
    }
}
