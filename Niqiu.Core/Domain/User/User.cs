using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.User;

namespace Niqiu.Core.Domain.User
{
    [Serializable]
    public class User : BaseEntity
    {
        private ICollection<UserRole> _userRoles;
        private ICollection<Answer> _answers;
        private ICollection<Message> _messages;
        private ICollection<Question> _questions1;

        public User()
        {
            UserGuid = Guid.NewGuid();
            LastActivityDateUtc = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the customer Guid
        /// </summary>
        public Guid UserGuid { get; set; }

        [Display(Name = "性别")]
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }

        public string Mobile { get; set; }

        public string Password { get; set; }
        [Display(Name="支付密码")]
        public string PaymentPassword { get; set; }

        /// <summary>
        /// Gets or sets the password format
        /// </summary>
        public int PasswordFormatId { get; set; }
       
        /// <summary>
        /// Gets or sets the password salt
        /// </summary>
        public string PasswordSalt { get; set; }


        public string RealName { get; set; }

        public string Description { get; set; }


        public string ImgUrl { get; set; }

        [Display(Name = "常驻位置")]
        public string Address { get; set; }

        public string IdCared { get; set; }

        public PasswordFormat PasswordFormat
        {
            get { return (PasswordFormat)PasswordFormatId; }
            set { PasswordFormatId = (int)value; }
        }


        /// <summary>
        /// Gets or sets the vendor identifier with which this customer is associated (maganer)
        /// </summary>
        //public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        //是否被禁
        public bool IsIllegal { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the customer account is system
        /// </summary>
        public bool IsSystemAccount { get; set; }


        /// <summary>
        /// Gets or sets the customer system name
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the last IP address
        /// </summary>
        public string LastIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last login
        /// </summary>
        public DateTime? LastLoginDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last activity
        /// </summary>
        public DateTime LastActivityDateUtc { get; set; }

        public GradeType GradeType { get; set; }
        public string WeiXinId { get; set; }
        public AuthType AuthType { get; set; }

        public string OpenId { get; set; }

        public string Province { get; set; }
        public string Country { get; set; }
        public string City { get; set; }



        public virtual ICollection<UserRole> UserRoles
        {
            get { return _userRoles ?? (_userRoles = new Collection<UserRole>()); }
            protected set { _userRoles = value; }
        }

        public virtual ICollection<Answer> Answers
        {
            get { return _answers??(_answers=new Collection<Answer>()); }
            set { _answers = value; }
        }

        public virtual ICollection<Message> Messages
        {
            get { return _messages??(_messages=new Collection<Message>()); }
            set { _messages = value; }
        }

        public virtual ICollection<Question> Questions
        {
            get { return _questions1??(_questions1=new Collection<Question>()); }
            set { _questions1 = value; }
        }

        public virtual ICollection<CommentPraise> CommentPraises { get; set; }

    }

    public enum AuthType
    {
        None,
        WeiXin,
        QQ,
        WeiBo,
        GitHub,
        AliPay
    }
}
