using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DnsTest.Data
{
    public class UdpBinary
    {
        public static byte[] toDnsData (string url)
        {
            var id = Guid.NewGuid().ToByteArray().Take(2).ToArray();
            var tag = new byte[2] { 1, 0 };
            var qdCount= new byte[2] { 0, 1 };
            var anCount= new byte[2] { 0, 0 };
            var asConnt = new byte[4] { 0, 0, 0, 0 };
            return id.Concat(tag).Concat(qdCount).Concat(anCount).Concat(asConnt).Concat(toQuestion(toServerName(url))).ToArray();
        }


        public static byte[] toQuestion(string serverPath)
        {
            //ending 0
            //type A 00 01  -- v6  0,0x1c
            //class in 00 01
            var ending = new byte[5] { 0,0,1,0,1 };
            return serverPath.Split(new char[] { '.' }).SelectMany(s => toFrag(s)).Concat(ending).ToArray();

            byte[] toFrag(string name)
            {
                var list= Encoding.ASCII.GetBytes(name).ToList();
                list.Insert(0, (byte)name.Length);
                return list.ToArray();
            }
        }


    

        public static string toServerName(string url)
        {
            var reg = new Regex(@"(http[s]?://)?([\w\.]*)");
            var res=reg.Match(url);
            if (res.Success)
            {
                foreach (var item in res.Groups)
                {
                    var val = item.ToString();
                    if (Regex.IsMatch(val, "http", RegexOptions.IgnoreCase)==false)
                    {
                        return val;
                    }
                }
            }
            return "";
        }
    }
}
