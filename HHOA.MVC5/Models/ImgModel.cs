using System.ComponentModel.DataAnnotations;

namespace Portal.MVC5.Models
{
    public class ImgModel
    {
        [DataType(DataType.ImageUrl)]
        public string Img { get; set; }
    }
}