using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SnapshotAndAirdrop.Helper
{
    class ToolHelper
    {
        public static string RemoveUndefinedParams(MyJson.JsonNode_Object jo)
        {
            string[] keys = new string[jo.Keys.Count];
            jo.Keys.CopyTo(keys, 0);

            for (var i = 0;i< keys.Length; i++)
            {
                var key = keys[i];
                var j = jo.GetDictItem(key);
                if (j.IsNull())
                {
                    jo.Remove(key);
                }
                else if (j.ToString() == "0")
                {
                    jo.Remove(key);
                }
                else if (key[0] == '_' && key[1] == '_')
                {
                    jo.Remove(key);
                }
            }

            return jo.ToString();
        }


        public static string RemoveRedundantParams(MyJson.JsonNode_Object jo)
        {
            string[] keys = new string[jo.Keys.Count];
            jo.Keys.CopyTo(keys, 0);

            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                var j = jo.GetDictItem(key);
                if (key[0] == '_' && key[1] == '_')
                {
                    jo.Remove(key);
                }
            }

            return jo.ToString();
        }
    }
}
