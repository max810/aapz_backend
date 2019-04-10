using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Models
{
    public class ManagerRegisterModel: RegisterModel
    {
        public Manager Manager { get; set; }
    }
}
