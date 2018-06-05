using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Niqiu.Core.Domain
{
    public class Appointment : BaseEntity
    {
        [Display(Name = "姓名")]
        [Required(ErrorMessage = "请输入姓名")]
        public string Name { get; set; }

        public int UserId { get; set; }

        [Display(Name = "性别")]
        [Required(ErrorMessage = "请选择性别")]
        public Gender Gender { get; set; }

        [Display(Name = "身份")]
       // [Required(ErrorMessage = "请选择身份")]
        public int Identity { get; set; }


        [Display(Name = "学(工)号")]
        [Required(ErrorMessage = "请输入学(工)号")]
        public string SchoolNumber { get; set; }
        [Display(Name = "手机号")]
        //[Required(ErrorMessage = "请输入手机")]
       // [RegularExpression(@"^1[3458][0-9]{9}$", ErrorMessage = "手机号格式不正确")]
        public string Mobile { get; set; }
        [Display(Name = "身份证号")]
        //[Required(ErrorMessage = "请输入身份证号")]
       // [RegularExpression(@"(^[1-9][0-9]{5}((19[0-9]{2})|(200[0-9])|2011)(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])[0-9]{3}[0-9xX]$)", ErrorMessage = "请输入正确的身份证号")]
        public string IdCard { get; set; }

        [Display(Name = "院系")]
        [Required(ErrorMessage = "请输入院系")]
        public string School { get; set; }
        [Display(Name = "附件")]
        public string Attachment { get; set; }
        [Display(Name = "预约类型")]
        public AppointmentType AppointmentType { get; set; }

        [Display(Name = "审核时间")]
        [Required(ErrorMessage = "请选择现场审核时间")]
        [DataType(DataType.DateTime)]
        //Remote只能在输入的时候触发？
        // [Remote("ValidDate", "Appointment",ErrorMessage = "选择日期无效")]
        public DateTime VerifyTime { get; set; }

        [Display(Name = "备注")]
        [DataType(DataType.MultilineText)]
        public string Remarks { get; set; }

        [Display(Name = "申请状态")]
        public VerifyState State { get; set; }

        #region 车证专用

        [Display(Name = "员工卡")]
        public string WrokImg { get; set; }

        [Display(Name = "身份证正面照")]
        public string IDCardImg { get; set; }

        [Display(Name = "身份证反面照")]
        public string IDCardImgBack { get; set; }

        [Display(Name = "驾驶证/行驶证正面照片")]
        public string DriveCardImg { get; set; }

        [Display(Name = "驾驶证/行驶证反面照片")]
        public string DriveCardImgBack { get; set; }

        [Display(Name = "结婚证照片")]
        public string MarryCardImg { get; set; }

        [Display(Name = "车牌号")]
        public string CardNumber { get; set; }

        [Display(Name = "品牌")]
        public string Brand { get; set; }

        [Display(Name = "颜色")]
        public string Color { get; set; }


        [Display(Name = "编号")]
        public string BianHao { get; set; }

        [Display(Name = "车牌号")]
        public string CardNumber2 { get; set; }

        [Display(Name = "车型")]
        public string CarStyle { get; set; }

        #endregion

        //[Display(Name = "审核人")]
        //public string UserName { get; set; }

        //[Display(Name = "申请人")]
        //[ForeignKey("UserId")]
        //public virtual User.User ApplyUser { get; set; }

        //[Display(Name = "申请人id")]
        //public virtual int UserId { get; set; }

        //[Display(Name = "申请人")]
        //public virtual string ApplyUserName { get; set; }
    }



    public enum Identity
    {
        [Display(Name = "学生")]
        Student,
        [Display(Name = "教师")]
        Teacher
    }

    public enum Gender
    {
        [Display(Name = "男")]
        Man,
        [Display(Name = "女")]
        Woman
    }

    [Flags]
    public enum AppointmentType
    {
        [Display(Name = "新生集体户口户口迁入")]
        Freshmen,
        [Display(Name = "北京新生源新分配")]
        InBeiJing,
        [Display(Name = "京外生源新分配")]
        OutBeiJing,
        [Display(Name = "新调入教职工")]
        NewStaff,
        [Display(Name = "博士、研究生毕业分配")]
        DocAndGraduate,
        [Display(Name = "博士后出站")]
        PostDoctor,
        [Display(Name = "博士后出站家属随迁")]
        DoctorFamily,
        [Display(Name = "教职工新生子女随父母入集体户")]
        StaffKid,
        [Display(Name = "人才引进教职工子女京外户籍迁入")]
        ComingInStaffKid,

        [Display(Name = "学生集体户口迁出")]
        Students,
        [Display(Name = "教职工集体户口迁出")]
        Staffs,
        [Display(Name = "集体户口借用")]
        BorrowHuji,
        [Display(Name = "办理暂住证")]
        Temporary,
        [Display(Name = "户口迁移证丢失补办")]
        MoveIssue,
        [Display(Name = "集体户口卡片丢失补办")]
        CollectiveIssue,
        [Display(Name = "户口(未)在校证明(教职工和学生)")]
        SchoolProve,
        [Display(Name = "原户籍身份证未丢失")]
        UnLostCard,
        [Display(Name = "原户籍身份证丢失")]
        LostCard,

        [Display(Name = "活动申报")]
        ActivityAsk,
        [Display(Name = "车证办理")]
        CarAsk,

        [Display(Name = "办理动火证")]
        Permit,
        [Display(Name = "安全协议流程")]
        Safe
    }
}
