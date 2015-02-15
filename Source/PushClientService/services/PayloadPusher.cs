using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using log4net;
using Newtonsoft.Json;

namespace PushClientService.services
{
    public class PayloadPusher : IPayloadPusher
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(PayloadPusher));

        private readonly string _pushTargetUrl;

        public PayloadPusher(string pushTargetUrl)
        {
            _pushTargetUrl = pushTargetUrl;
        }

        public void Push(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var postBody = string.Format("payload={0}", HttpUtility.UrlEncode(json));

            var request = (HttpWebRequest)WebRequest.Create(_pushTargetUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("X-Github-Event", "push");

            var dataBytes = Encoding.UTF8.GetBytes(postBody);
            request.ContentLength = dataBytes.Length;

            Stream requestStream;
            try
            {
                requestStream = request.GetRequestStream();
            }
            catch (Exception ex)
            {
                _log.Error(string.Format("Unable to connect to {0}", _pushTargetUrl), ex);
                return;
            }

            requestStream.Write(dataBytes, 0, dataBytes.Length);
            requestStream.Close();

            _log.InfoFormat("Sending HTTP POST to {0}...", _pushTargetUrl);
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