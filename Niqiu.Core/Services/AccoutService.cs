using System;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;

namespace Niqiu.Core.Services
{
    public class AccoutService : IAccountService
    {
        private readonly IUserService _userService;

        public AccoutService(IUserService userService)
        {
            _userService = userService;
        }

        public UserLoginResults ValidateUser(string usernameOrEmail, string password)
        {
            User user = null;

            user = _userService.GetUserByUsername(usernameOrEmail);
            if (user == null && CommonHelper.ValidateString(usernameOrEmail, ValidataType.Email))
            {
                user = _userService.GetUserByEmail(usernameOrEmail);
            }
            if (user == null && CommonHelper.ValidateString(usernameOrEmail, ValidataType.Mobile))
            {
                user = _userService.GetUserByEmail(usernameOrEmail);
            }
            return Valid(user, password);
        }

        public UserLoginResults ValidateUserByMobile(string mobile, string password)
        {
            var user = _userService.GetUserByMobile(mobile);
            return Valid(user, password);
        }

        private UserLoginResults Valid(User user, string password)
        {
            if (user == null)
                return UserLoginResults.UserNotExist;
            if (user.Deleted)
                return UserLoginResults.Deleted;
            if (!user.Active)
                return UserLoginResults.NotActive;
            //only registered can login
            //if (!user.IsRegistered())
            //    return UserLoginResults.NotRegistered;

            bool isValid = Encrypt.GetMd5Code(password) == user.Password;
            if (!isValid)
                return UserLoginResults.WrongPassword;

            //save last login date
            user.LastLoginDateUtc = DateTime.Now;
            _userService.UpdateUser(user);
            return UserLoginResults.Successful;
        }

        public void SetEmail(User user, string newEmail)
        {

        }

        public void SetUsername(User user, string newUsername)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            newUsername = newUsername.Trim();

            if (newUsername.Length > 100)
                throw new PortalException("用户名太长");

            var user2 = _userService.GetUserByUsername(newUsername);
            if (user2 != null && user.Id != user2.Id)
                throw new PortalException("用户名已经存在");

            user.Username = newUsername;
            _userService.UpdateUser(user);
        }

        public UserRegistrationResult RegisterUser(UserRegistrationRequest request,bool isMobile=false)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.User == null)
                throw new ArgumentException("当前用户为空");

            var result = new UserRegistrationResult();

            if (request.User.IsRegistered())
            {
                result.AddError("当前用户已经注册");
                return result;
            }
            if (String.IsNullOrEmpty(request.Email)&&!isMobile)
            {
                result.AddError("邮箱不能为空");
                return result;
            }
            if (!CommonHelper.IsValidEmail(request.Email) && !isMobile)
            {
                result.AddError("邮件格式错误");
                return result;
            }
            if (String.IsNullOrWhiteSpace(request.Password))
            {
                result.AddError("密码不能为空");
                return result;
            }
            if (String.IsNullOrWhiteSpace(request.Mobile))
            {
                result.AddError("手机号码不能为空");
                return result;
            }
            if (_userService.GetUserByUsername(request.Username) != null && !isMobile)
            {
                result.AddError("用户名已经存在");
                return result;
            }

            //手机注册更新用户 而不是新增用户
         
              if (isMobile)
              {
                  request.User = _userService.GetUserBySchoolNumber(request.SchoolNumber)??new User();
              }
            request.User.Username = request.Username;
            request.User.Email = request.Email;
            request.User.PasswordFormat = request.PasswordFormat;
            request.User.Mobile = request.Mobile;
            request.User.SchoolNumber = request.SchoolNumber;
            request.User.Gender = request.Gender;
            request.User.ImgUrl = "/Content/user_img.jpg";
            request.User.Password = Encrypt.GetMd5Code(request.Password);
            request.User.Active = request.IsApproved;

            // 添加基本角色。
            //var registeredRole = _userService.GetUserRoleBySystemName(SystemUserRoleNames.Registered);
            //if (registeredRole == null)
            //    throw new PortalException("'Registered' 角色加载失败");

           

            if (request.User.Id == 0)
            {
                _userService.InsertUser(request.User);
                request.User = _userService.GetUserByUsername(request.Username);
            }
            else
            {
                _userService.UpdateUser(request.User);
            }
            //request.User.UserRoles.Add(registeredRole);
            //_userService.UpdateUser(request.User);
            return result;
        }

        public PasswordChangeResult ChangePassword(ChangePasswordRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var result = new PasswordChangeResult();
            if (String.IsNullOrWhiteSpace(request.Email))
            {
                result.AddError("邮件不能为空");
                return result;
            }
            if (String.IsNullOrWhiteSpace(request.NewPassword))
            {
                result.AddError("密码不能为空");
                return result;
            }

            var customer = _userService.GetUserByEmail(request.Email);
            if (customer == null)
            {
                result.AddError("邮件不存在");
                return result;
            }

            var requestIsValid = false;
            if (request.ValidateRequest)
            {
                //password
                string oldPwd = "";
                customer.Password = Encrypt.GetMd5Code(request.NewPassword);
                bool oldPasswordIsValid = oldPwd == customer.Password;
                if (!oldPasswordIsValid)
                    result.AddError("旧密码错误");

                if (oldPasswordIsValid)
                    requestIsValid = true;
            }
            else
                requestIsValid = true;

            if (requestIsValid)
            {
                customer.Password = Encrypt.GetMd5Code(request.NewPassword);
                _userService.UpdateUser(customer);
            }

            return result;
        }

        public bool ChangePassword(int userid, string password)
        {
            var rawuser = _userService.GetUserById(userid);
            if (rawuser != null)
            {
                rawuser.Password = Encrypt.GetMd5Code(password);
                _userService.UpdateUser(rawuser);
                return true;
            }
            return false;
        }
    }
}
