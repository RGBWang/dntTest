using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DnsTest.Data
{
    public class DnsResult
    {
        public string IP { get; set; }

        public double MiliSecondTime { get; set; }

        public bool IsFailed { get; set; }
    }
    public static class DnsTask
    {
  

        public static async  Task<DnsResult> DoDnsTask(string dnsServer,string url)
        {
            var port = RDZ.Tools.Tool.GetFreeUdpPort();
            var localIP=RDZ.Tools.Tool.GetLocalIP();

            var ie = new IPEndPoint(IPAddress.Parse(dnsServer), 53);
            var sender = new UdpClient(new IPEndPoint(IPAddress.Parse(localIP),port));
            var data = UdpBinary.toDnsData(url);


            try
            {
                var time=DateTime.Now;
                sender.Send(data, data.Length, ie);
                byte[] recieData=   sender.Receive(ref ie);
                var time2=DateTime.Now;
                var result = new DnsResult() {
                    MiliSecondTime =Math.Round( (time2 - time).TotalMilliseconds)

                };
                if (recieData.Length > data.Length)
                {
                  var list= recieData.Skip(recieData.Length - 4).Take(4).Select(s=>s.ToString()).ToList();
                    var ip = list[0];

                    if (list[0] == "0")
                    {
                        result.IsFailed=true;
                        return result;
                    }
                    for (int i = 1; i < list.Count; i++)
                    {
                        ip += "." + list[i].ToString();
                    }
                    result.IP = ip;
                }
                return result;
            }
            catch (Exception ex)
            {
                return new DnsResult() { IsFailed=true};
            }
       
            
        }

    }
}
