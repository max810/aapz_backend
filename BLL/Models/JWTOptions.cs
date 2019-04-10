using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Models
{
    public static class JWTOptions
    {
        public static string ISSUER { get; } = "AAPZ_Backend";
        public static string AUDIENCE { get; } = "localhost_user";
        public static TimeSpan LIFETIME { get; } = TimeSpan.FromHours(1);
        public static string KEY { get; } = "duduhastduhastmich"; 
    }
}
