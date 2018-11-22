using SnapshotAndAirdrop.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapshotAndAirdrop.Handle
{
    interface ITask
    {
        void StartTask(params object[] args);
        void Handle(params object[] args);
        void StopTask();
    }


    public abstract class PTask : ITask
    {
        public delegate void DeleRuntime(params string[] describes);
        public DeleRuntime deleRuntime;

        public delegate void DeleResult(params string[] describes);
        public DeleResult deleResult;

        public void StartTask(params object[] args)
        {
            Task task = new Task(() => {
                Handle(args);
            });
            task.Start();
        }

        public abstract void Handle(params object[] args);

        public void StopTask()
        {

        }

        public AssetInfo GetAssetInfo(string assetid)
        {
            AssetInfo assetInfo = new AssetInfo();
            if (!assetid.StartsWith("0x"))
                assetid = string.Format("{0}{1}", "0x", assetid);
            if (assetid.Length == 64)
            {//全局资产
                string fliter = "{id:\"" + assetid + "\"}";
                var JAData = mongoHelper.GetData(Config.Ins.Asset_Conn, Config.Ins.Asset_DB, Config.Ins.Asset_Coll, fliter);
                if (JAData.Count > 0)
                {
                    assetInfo.assetid = assetid;
                    assetInfo.decimals = decimal.Parse(JAData[0].AsDict()["precision"].ToString());
                    assetInfo.totoalSupply = decimal.Parse(JAData[0].AsDict()["amount"].ToString());
                }
            }
            else
            {
                string fliter = "{assetid:\"" + assetid + "\"}";
                var JAData = mongoHelper.GetData(Config.Ins.Asset_Conn, Config.Ins.Asset_DB, Config.Ins.NEP5Asset_Coll, fliter);
                if (JAData.Count > 0)
                {
                    assetInfo.assetid = assetid;
                    assetInfo.decimals = decimal.Parse(JAData[0].AsDict()["decimals"].ToString());
                    assetInfo.totoalSupply = decimal.Parse(JAData[0].AsDict()["totalsupply"].ToString());
                }
            }

            return assetInfo;
        }
    }
}
