using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs
{
    public class Content
    {
        public string content { get; set; }
        public string txid { get; set; }
        public string time { get; set; }
        public string reputation { get; set; }
        public string settings { get; set; }
        public string scoreSum { get; set; }
        public string scoreCnt { get; set; }
    }
}
