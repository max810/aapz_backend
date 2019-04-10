using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Models
{
    public class DriverRegisterModel: RegisterModel
    {
        public Driver Driver { get; set; }
    }
}
