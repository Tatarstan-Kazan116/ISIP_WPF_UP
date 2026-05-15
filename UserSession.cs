using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP_New
{
    public static class UserSession
    {
        public static int UserId { get; set; } = 0;
        public static int RoleId { get; set; } = 0;
        public static bool IsFrozen { get; set; } = false;
        public static string DisplayName { get; set; } = "";
        public static string Login { get; set; } = "";

        public static bool IsAdmin => RoleId == 3;
        public static bool IsAuthor => RoleId == 2;
    }
}