using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs
{
    public class UserProfile
    {
        public string address { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public string i { get; set; }
        public string b { get; set; }
        public string r { get; set; }
        public int reputation { get; set; }
        public int postcnt { get; set; }
        public int rc { get; set; }
        public string? a { get; set; }
        public int? regdate { get; set; }
        public string? l { get; set; }
        public string? s { get; set; }
        public int? update { get; set; }
        public string? k { get; set; }
        public Subscription[]? subscribes { get; set; }
        public string[]? subscribers { get; set; }
        public string[]? blocking { get; set; }


    }
}
