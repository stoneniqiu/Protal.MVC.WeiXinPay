using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Portal.MVC.Models
{
    public class ImgModel
    {
        [DataType(DataType.ImageUrl)]
        public string Img { get; set; }
    }
}