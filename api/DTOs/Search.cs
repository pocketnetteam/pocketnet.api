using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs
{
    public class Search
    {
        public string[]? fastsearch { get; set; }
        public PostsWithCount? posts { get; set; }
        public UsersWithCount? users { get; set; }
    }
}
