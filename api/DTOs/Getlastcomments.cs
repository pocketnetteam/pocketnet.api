namespace Catalog.API.Entities
{
    public class Getlastcomments
    {
        public string id { get; set; }
        public string postid { get; set; }
        public string address { get; set; }
        public int time { get; set; }
        public int timeUpd { get; set; }
        public int block { get; set; }
        public string msg { get; set; }
        public string parentid { get; set; }
        public string answerid { get; set; }
        public int? scoreUp { get; set; }
        public int? scoreDown { get; set; }
        public int? reputation { get; set; }
        public int edit { get; set; }
        public int deleted { get; set; }
        public int? myScore { get; set; }
    }

    /*
        oCmnt.pushKV("id", cmntItm["otxid"].As<string>());
        oCmnt.pushKV("postid", cmntItm["postid"].As<string>());
        oCmnt.pushKV("address", cmntItm["address"].As<string>());
        oCmnt.pushKV("time", ocmntItm["time"].As<string>());
        oCmnt.pushKV("timeUpd", cmntItm["time"].As<string>());
        oCmnt.pushKV("block", cmntItm["block"].As<string>());
        oCmnt.pushKV("msg", cmntItm["msg"].As<string>());
        oCmnt.pushKV("parentid", cmntItm["parentid"].As<string>());
        oCmnt.pushKV("answerid", cmntItm["answerid"].As<string>());
        oCmnt.pushKV("scoreUp", cmntItm["scoreUp"].As<string>());
        oCmnt.pushKV("scoreDown", cmntItm["scoreDown"].As<string>());
        oCmnt.pushKV("reputation", cmntItm["reputation"].As<string>());
        oCmnt.pushKV("edit", cmntItm["otxid"].As<string>() != cmntItm["txid"].As<string>());
        oCmnt.pushKV("deleted", cmntItm["msg"].As<string>() == "");
        //oCmnt.pushKV("myScore", myScore);
*/
}
