namespace api.DTOs
{
    public class PostData
    {
        public string txid { get; set; }
        public bool txidEdit { get; set; }
        public string txidRepost { get; set; }
        public string address { get; set; }
        public int time { get; set; }

        public string l { get; set; }
        public string c { get; set; }
        public string m { get; set; }
        public string u { get; set; }
        public string scoreSum { get; set; }
        public string scoreCnt { get; set; }

        public string[] t { get; set; }
        public string[] i { get; set; }
        public Settings s { get; set; }
        public string myVal { get; set; }
        public int comments { get; set; }
        public Comment lastComment { get; set; }
        public int reposted { get; set; }
        public UserProfile? userprofile { get; set; }
    }
}
