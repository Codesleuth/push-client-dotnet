using System;
using System.Net;
using System.Text;
using System.Web;
using log4net;
using Newtonsoft.Json;

namespace PushClientService
{
    public class PushService
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(PushService));

        public static void Push(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var postBody = string.Format("payload={0}", HttpUtility.UrlEncode(json));

            var request = (HttpWebRequest)WebRequest.Create(Configuration.CiUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("X-Github-Event", "push");

            var dataBytes = Encoding.UTF8.GetBytes(postBody);
            request.ContentLength = dataBytes.Length;
            var requestStream = request.GetRequestStream();
            requestStream.Write(dataBytes, 0, dataBytes.Length);
            requestStream.Close();

            _log.InfoFormat("Forwarding event to {0}...", Configuration.CiUrl);
            try
            {
                var response = request.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                _log.Error("Forwarding request failed", ex);
            }
        }
    }
}