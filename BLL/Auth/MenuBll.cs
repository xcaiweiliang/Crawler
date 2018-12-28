using System;
using System.Linq;
using Model;
using DAL;
using System.Collections.Generic;
using System.Web;

namespace BLL
{

    public class MenuBll : Bll<Menu>
    {
        /// <summary>
        /// ɾ���˵�
        /// </summary>
        /// <param name="menuId"></param>
        public void DeleteMenu(string menuId)
        {
            using (Context db = new Context())
            {
                new MenuRepository(db).DeleteMenu(menuId);
            }
        }

        public string SaveMenu(ref string uuid, string pId, string Code, string Name, string MenuURL, string Remark, bool isShow, string OperationName)
        {
            using (Context db = new Context())
            {
                var rep = new MenuRepository(db);
                string[] operationIds = OperationName.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (!string.IsNullOrEmpty(uuid))//�޸���Դ����
                {
                    if (rep.CheckCodeExists(uuid, Code))
                    {
                        return "�����Ѿ����ڣ�";
                    }
                    var model = this.Get(uuid);
                    model.ParentId = pId;// == "" ? "0000" : pId;
                    model.Code = Code;//���
                    model.Name = Name;//����
                    //model.Sort = nodeIndex;
                    model.Remark = Remark;
                    model.Url = MenuURL;
                    model.IsShow = isShow;
                    rep.Update(model);
                }
                else//������Դ
                {
                    if (rep.CheckCodeExists(null, Code))
                    {
                        return "�����Ѿ����ڣ�";
                    }
                    int maxSort = 0;
                    if (rep.Filter(x => x.ParentId == pId).Any()) { }
                    maxSort = rep.Filter(x => x.ParentId == pId).Max(x => x.Sort);
                    var model = new Model.Menu();
                    model.ParentId = pId;
                    model.Code = Code;//���
                    model.Name = Name;//����
                    model.Sort = maxSort + 10;
                    model.IsShow = isShow;
                    model.Url = MenuURL;
                    model.Remark = Remark;//
                    model.Id = System.Guid.NewGuid().ToString();//Guid
                    rep.Create(model);
                    uuid = model.Id; //�����޸���ԴId
                }
                rep.Save(uuid, operationIds);
                Commit(db);
                return string.Empty;
            }
        }


        public void SortMenu(string currId, string otherId)
        {
            using (Context db = new Context())
            {
                var mRep = new MenuRepository(db);
                Menu curMenu = mRep.Get(currId);
                Menu otherMenu = mRep.Get(otherId);
                var tmp = curMenu.Sort;
                curMenu.Sort = otherMenu.Sort;
                otherMenu.Sort = tmp;
                mRep.Update(curMenu);
                mRep.Update(otherMenu);
                mRep.Commit();
            }
        }


        public IQueryable<Menu> GetCurrUserMenu()
        {
            User u = (User)HttpContext.Current.Session["MGR_USER"];
            return new MenuRepository(new Context()).GetCurrUserMenu(u.Id);
        }

    }
}
