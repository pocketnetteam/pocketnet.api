using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs
{
    public class Score
    {

        public string cmntid { get; set; }
        public int? scoreUp { get; set; }
        public int? scoreDown { get; set; }
        public int? reputation { get; set; }
        public int? myScore { get; set; }
    }
}
