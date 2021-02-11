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
        public UserProfile userprofile { get; set; }
    }

    /*   
    entry.pushKV("txid", itm["txid"].As<string>());
    if (itm["txidEdit"].As<string>() != "") entry.pushKV("edit", "true");
    if (itm["txidRepost"].As<string>() != "") entry.pushKV("repost", itm["txidRepost"].As<string>());
    entry.pushKV("address", itm["address"].As<string>());
    entry.pushKV("time", itm["time"].As<string>());
    entry.pushKV("l", itm["lang"].As<string>());
    entry.pushKV("c", itm["caption"].As<string>());
    entry.pushKV("m", itm["message"].As<string>());
    entry.pushKV("u", itm["url"].As<string>());

    entry.pushKV("scoreSum", itm["scoreSum"].As<string>());
    entry.pushKV("scoreCnt", itm["scoreCnt"].As<string>());
*/
}
