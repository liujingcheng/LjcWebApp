using System;
using System.Collections.Generic;
using System.Linq;
using LjcWebApp.Helper;
using LjcWebApp.Models.entity;

namespace LjcWebApp.Services.Account
{
    public class MyUserService
    {
        /// <summary>
        /// 获取所有列表
        /// </summary>
        /// <returns></returns>
        public List<MyUser> GetList()
        {
            var words = new List<MyUser>();
            try
            {
                using (var context = new LjcDbContext())
                {
                    words = context.myuser.OrderBy(p => p.CreatedOn).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }

            return words;
        }

        /// <summary>
        /// 模糊查找
        /// </summary>
        /// <returns></returns>
        public List<MyUser> Search(string likeStr)
        {
            var words = new List<MyUser>();
            try
            {
                using (var context = new LjcDbContext())
                {
                    words = context.myuser.Where(p => p.UserName.Contains(likeStr)).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }

            return words;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        public bool Add(MyUser myUser)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    myUser.UserId = Guid.NewGuid().ToString().Replace("-", "");
                    myUser.IsDeleted = 0;
                    myUser.ModifiedOn = myUser.CreatedOn = DateTime.Now;

                    context.myuser.Add(myUser);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        public bool Update(MyUser myUser)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 查找单个实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MyUser Get(string id)
        {
            MyUser entity = null;
            try
            {
                using (var context = new LjcDbContext())
                {
                    entity = context.myuser.First(p => p.UserId == id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
            return entity;
        }
        /// <summary>
        /// 根据用户名查找单个实体
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public MyUser GetByUserName(string userName)
        {
            MyUser entity = null;
            try
            {
                using (var context = new LjcDbContext())
                {
                    entity = context.myuser.FirstOrDefault(p => p.UserName == userName);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
            return entity;
        }

        /// <summary>
        /// 用户名密码是否正确
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool IsPasswordCorrect(string userName, string password)
        {
            using (var context = new LjcDbContext())
            {
                return context.myuser.Any(p => p.UserName == userName && p.Password == password);
            }
        }

        /// <summary>
        /// 用户是否存在
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool IsUserExist(string userName)
        {
            using (var context = new LjcDbContext())
            {
                return context.myuser.Any(p => p.UserName == userName);
            }
        }

        /// <summary>
        /// 删除问题
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(string id)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var entity = context.myuser.First(p => p.UserId == id);
                    entity.IsDeleted = 1;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return false;
            }
            return true;
        }

    }
}
