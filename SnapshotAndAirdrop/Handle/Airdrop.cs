using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SnapshotAndAirdrop.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace SnapshotAndAirdrop.Handle
{
    class AirdropTask : PTask
    {
        public override void Handle(params object[] args)
        {
            //分析参数
            byte[] priKey = (byte[])args[0];
            decimal totalValue = (decimal)args[1];
            string snapshotColl = (string)args[2];

            var assetid = "";
            UInt32 height = 0;
            decimal sendAssetDecimals = 0;
            //获取已有的所有的地址 （分段）
            var count = mongoHelper.GetDataCount(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshotColl);
            var looptime = count / 1000 + 1;
            for (var i = 1; i < looptime + 1; i++)
            {
                Console.WriteLine("总共要循环：" + looptime + "~~现在循环到：" + i);
                MyJson.JsonNode_Array Ja_addressInfo = mongoHelper.GetDataPages(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshotColl, "{}", 1000, i);
                for (var ii = 0; ii < Ja_addressInfo.Count; ii++)
                {
                    string txid = Ja_addressInfo[ii].AsDict()["txid"].ToString();
                    bool applied = Ja_addressInfo[ii].AsDict()["applied"].AsBool();
                    if (!string.IsNullOrEmpty(txid)&& txid!="null")
                        continue;
                    if (!applied)
                        continue;
                    var sendAssetid = Ja_addressInfo[ii].AsDict()["sendAssetid"].ToString();
                    if (sendAssetDecimals == 0)
                        sendAssetDecimals = GetAssetInfo(sendAssetid).decimals;
                    var addr = Ja_addressInfo[ii].AsDict()["addr"].ToString();
                    assetid = Ja_addressInfo[ii].AsDict()["assetid"].ToString();
                    var send =decimal.Parse(Ja_addressInfo[ii].AsDict()["send"].AsDict()["$numberDecimal"].ToString());
                    height = (uint)Ja_addressInfo[ii].AsDict()["height"].AsInt();
                    var balance = decimal.Parse(Ja_addressInfo[ii].AsDict()["balance"].AsDict()["$numberDecimal"].ToString(),System.Globalization.NumberStyles.Float);
                    Send(priKey,assetid,sendAssetid,addr, balance, send, sendAssetDecimals, height, snapshotColl);
                    System.Threading.Thread.Sleep(500);
                    deleRuntime(((i - 1) * 1000 + ii + 1) + "/" + count);
                }
            }
            deleResult("完成");

            var snapshotCollTotal = "TotalSnapShot";
            TotalSnapshot totalSnapshot = new TotalSnapshot();
            totalSnapshot.assetid = assetid;
            totalSnapshot.totalValue = BsonDecimal128.Create(totalValue);
            totalSnapshot.snapshotColl = snapshotColl;
            totalSnapshot.height = height;

            mongoHelper.ReplaceData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshotCollTotal,"{\"snapshotColl\":\""+ snapshotColl + "\"}",totalSnapshot);
        }


        private void Send(byte[] priKey, string assetid, string sendAssetid,string addr, decimal balance, decimal send,decimal sendAssetDecimals,UInt32 height,string snapshotColl)
        {
            try
            {

                decimal decimals = decimal.Parse(sendAssetDecimals.ToString()) ;

                byte[] data = null;

                byte[] pubKey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(priKey);
                string address = ThinNeo.Helper.GetAddressFromPublicKey(pubKey);

                if (addr == address)
                    return;
                //MakeTran
                ThinNeo.Transaction tran = new Transaction();
                {
                    var _send =send * decimal.Parse(Math.Pow(10,(double)decimals).ToString());
                    using (ScriptBuilder sb = new ScriptBuilder())
                    {
                        MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();

                        byte[] randombytes = new byte[32];
                        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                        {
                            rng.GetBytes(randombytes);
                        }
                        BigInteger randomNum = new BigInteger(randombytes);
                        sb.EmitPushNumber(randomNum);
                        sb.Emit(ThinNeo.VM.OpCode.DROP);
                        array.AddArrayValue("(addr)" + address);
                        array.AddArrayValue("(addr)" + addr);
                        array.AddArrayValue("(int)" + (BigInteger)_send);
                        sb.EmitParamJson(array);
                        sb.EmitPushString("transfer");
                        sb.EmitAppCall(new Hash160(assetid));
                        data = sb.ToArray();
                    }

                    tran.type = ThinNeo.TransactionType.InvocationTransaction;
                    var idata = new ThinNeo.InvokeTransData();
                    tran.extdata = idata;
                    idata.script = data;
                    idata.gas = 0;
                    tran.inputs = new ThinNeo.TransactionInput[0];
                    tran.outputs = new ThinNeo.TransactionOutput[0];
                    tran.attributes = new ThinNeo.Attribute[1];
                    tran.attributes[0] = new ThinNeo.Attribute();
                    tran.attributes[0].usage = TransactionAttributeUsage.Script;
                    tran.attributes[0].data = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

                    //sign and broadcast
                    var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), priKey);
                    tran.AddWitness(signdata, pubKey, address);
                    var trandata = tran.GetRawData();
                    var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);
                    byte[] postdata;
                    var url = HttpHelper.MakeRpcUrlPost(Config.Ins.url, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
                    var result = HttpHelper.HttpPost(url, postdata);
                    var j_result = MyJson.Parse(result).AsDict()["result"].AsList()[0].AsDict();
                    if (j_result["sendrawtransactionresult"].AsBool())
                    {
                        Snapshot snapshot = new Snapshot();
                        snapshot.addr = addr;
                        snapshot.balance = BsonDecimal128.Create(balance);
                        snapshot.send = BsonDecimal128.Create(send);
                        snapshot.txid = j_result["txid"].AsString();
                        snapshot.sendAssetid = sendAssetid;
                        snapshot.applied = true;
                        snapshot.assetid = assetid;
                        snapshot.height = height;

                        mongoHelper.ReplaceData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshotColl, ToolHelper.RemoveUndefinedParams(JsonConvert.SerializeObject(new Snapshot() { addr = addr ,applied = true })), snapshot);

                        mongoHelper.InsetOne(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB,Config.Ins.Snapshot_Current_Coll, snapshot);
                    }
                    else
                    {

                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

        }
    }
}
