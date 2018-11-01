using MongoDB.Bson;
using Newtonsoft.Json;
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
    class NnsAwardTask : PTask
    {
        public override void Handle(params object[] args)
        {
            byte[] priKey = (byte[])args[0];
            string assetid = (string)args[1]; //空投出去的钱id

            //获取已有的所有的地址 （分段）
            var count = mongoHelper.GetDataCount(Config.Ins.Applyfornnc_Conn, Config.Ins.Applyfornnc_DB, Config.Ins.Applyfornnc_Coll);
            var looptime = count / 1000 + 1;
            for (var i = 1; i < looptime + 1; i++)
            {
                MyJson.JsonNode_Array Ja_AwardInfo = mongoHelper.GetDataPages(Config.Ins.Applyfornnc_Conn, Config.Ins.Applyfornnc_DB, Config.Ins.Applyfornnc_Coll, "{}", 1000, i);
                for (var ii = 0; ii < Ja_AwardInfo.Count; ii++)
                {
                    NnsAward nnsAward = new NnsAward();
                    var address = Ja_AwardInfo[ii].AsDict()["maxBuyer"].AsString();
                    var value =decimal.Parse(Ja_AwardInfo[ii].AsDict()["maxBonus"].AsDict()["$numberDecimal"].AsString());
                    var txid = "";
                    if (Ja_AwardInfo[ii].AsDict().ContainsKey("txid"))
                        txid = Ja_AwardInfo[ii].AsDict()["txid"].AsString();
                    if (!string.IsNullOrEmpty(txid))
                        continue;
                    Send(priKey, assetid, address, value, nnsAward);
                    System.Threading.Thread.Sleep(1000);
                    deleRuntime(((i - 1) * 1000 + ii + 1) + "/" + count);
                }
            }
            deleResult("完成");
        }

        private void Send(byte[] priKey, string assetid, string addr, decimal value,NnsAward nnsAward)
        {
            try
            {
                //获取资产的精度
                byte[] postdata;
                var url = HttpHelper.MakeRpcUrlPost(Config.Ins.url, "getnep5asset", out postdata, new MyJson.JsonNode_ValueString(assetid));
                var result = HttpHelper.HttpPost(url, postdata);
                var Jo_result = MyJson.Parse(result) as MyJson.JsonNode_Object;
                decimal decimals = 0;
                if (Jo_result.ContainsKey("result"))
                {
                    decimals = (decimal)Math.Pow(10, Jo_result["result"].AsList()[0].AsDict()["decimals"].AsInt());
                }
                else
                {
                    return;
                }


                byte[] data = null;

                byte[] pubKey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(priKey);
                string address = ThinNeo.Helper.GetAddressFromPublicKey(pubKey);

                if (addr == address)
                    return;
                //MakeTran
                ThinNeo.Transaction tran = new Transaction();
                {
                    BigInteger sendValue = (BigInteger)((decimal)value* decimals);
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
                        array.AddArrayValue("(int)" + sendValue);
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

                    url = HttpHelper.MakeRpcUrlPost(Config.Ins.url, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
                    result = HttpHelper.HttpPost(url, postdata);
                    Console.WriteLine(result);
                    var j_result = MyJson.Parse(result).AsDict()["result"].AsList()[0].AsDict();
                    if (j_result["sendrawtransactionresult"].AsBool())
                    {
                        nnsAward.txid = j_result["txid"].AsString();
                        nnsAward.maxBuyer = addr;
                        nnsAward.maxBonus =Decimal128.Parse(value.ToString());
                        mongoHelper.ReplaceData(Config.Ins.Applyfornnc_Conn, Config.Ins.Applyfornnc_DB, Config.Ins.Applyfornnc_Coll,"{maxBuyer:\""+ addr + "\"}" ,nnsAward);
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
