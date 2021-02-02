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

        /*
        entry.pushKV("address", _address);
        entry.pushKV("name", itm["name"].As<string>());
        entry.pushKV("id", itm["id"].As<int>() + 1);
        entry.pushKV("i", itm["avatar"].As<string>());
        entry.pushKV("b", itm["donations"].As<string>());
        entry.pushKV("r", itm["referrer"].As<string>());
        entry.pushKV("reputation", itm["reputation"].As<int>() / 10.0);

        if (_posts_cnt.find(_address) != _posts_cnt.end()) {
            entry.pushKV("postcnt", _posts_cnt[_address]);
        }*/
    }
}
