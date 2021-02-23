using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs
{
    public class PostsWithCount
    {
        public int count { get; set; }
        public PostData[]? data { get; set; }
    }
}
