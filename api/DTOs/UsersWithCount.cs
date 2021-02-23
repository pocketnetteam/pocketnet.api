using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs
{
    public class UsersWithCount
    {
        public int count { get; set; }
        public UserProfile[]? data { get; set; }
    }
}
