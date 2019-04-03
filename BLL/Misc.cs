using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BLL
{
    public static class Misc
    {
        public static string GetMD5HashB64(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(data);
            }
        }
    }
}
