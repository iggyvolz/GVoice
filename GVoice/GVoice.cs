﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace GVoice
{
    public class GVoice
    {
        private string key;
        private string sid;
        private string hsid;
        private string ssid;
        private string apisid;
        private string sapisid;
        private string _cookies = null;
        private static Random rng = new Random();
        private string cookies
        {
            get
            {
                if (_cookies == null)
                {

                    Dictionary<string, string> cookies = new Dictionary<string, string>(){
                        {"SID",sid},
                        {"HSID",hsid},
                        {"SSID",ssid},
                        {"APISID",apisid},
                        {"SAPISID",sapisid},
                    };
                    StringBuilder cookieString = new StringBuilder(cookies.Count * 2);
                    bool first = true;
                    foreach (KeyValuePair<string, string> pair in cookies)
                    {
                        if (first)
                        {
                            first = false;
                            cookieString.Append("cookie: ");
                        }
                        else
                        {
                            cookieString.Append("; ");
                        }
                        cookieString.Append(pair.Key + "=" + pair.Value);
                    }
                    _cookies = cookieString.ToString();
                }
                return _cookies;
            }
        }

        public GVoice(string key,string sid,string hsid,string ssid,string apisid,string sapisid)
        {
            this.key = key;
            this.sid = sid;
            this.hsid = hsid;
            this.ssid = ssid;
            this.apisid = apisid;
            this.sapisid = sapisid;
        }
        private string sapisidhash(string sapisid,string origin)
        {
            int now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] bytes=sha.ComputeHash(Encoding.UTF8.GetBytes(now.ToString() + " " + sapisid + " " + origin));
            return now.ToString()+"_"+string.Join("",bytes.Select(b => b.ToString("x2")).ToArray());
        }
        public LcResponse GetMessages(int limit)
        {
            return HttpRequest<LcResponse>("lc", "{\"request\":{\"limit\":" + limit + ",\"label\":[\"sms\"],\"wantTranscript\":true,\"feature\":{\"mms\":true,\"multiple_label\":true,\"contact_blocked\":true,\"threading\":true},\"timeSpecification\":{\"timeMs\":" + Math.Round(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds) + ",\"timeRelation\":\"BEFORE\"}}}");
        }
        public SmsResponse SendMessage(string messageText, string conversationId)
        {
            byte[] bytes = new byte[8];
            rng.NextBytes(bytes); // Generate random message id
            long messageId = Math.Abs(BitConverter.ToInt64(bytes, 0));
            return HttpRequest<SmsResponse>("sms", "{\"request\":{\"conversationId\":\"" + conversationId + "\",\"feature\":{\"mms\":true,\"multiple_label\":true,\"contact_blocked\":true,\"threading\":true},\"messageId\":[\"" + messageId + "\"],\"outgoingDestination\":null,\"smsMessage\":\"" + messageText + "\"}}");
        }
        public T HttpRequest<T>(string endpoint,string data)
        {
            HttpWebRequest request = WebRequest.CreateHttp("https://clients6.google.com/voice/v0.1internal/"+endpoint+"?alt=json&key="+key);
            request.Method = "POST";
            request.Headers.Add("x-requested-with: XMLHttpRequest");
            request.Headers.Add("origin: https://clients6.google.com");
            request.Headers.Add("x-origin: https://voice.google.com");
            request.Headers.Add("accept-language: en-US,en;q=0.8,fr;q=0.6");
            request.Headers.Add("authorization: SAPISIDHASH "+sapisidhash(sapisid,"https://voice.google.com"));
            request.Headers.Add("x-chrome-uma-enabled: 1");
            request.Headers.Add(cookies);
            request.Headers.Add("x-goog-authuser: 0");
            request.Headers.Add("pragma: no-cache");
            request.Headers.Add("x-clientdetails: appVersion=5.0%20(Windows%20NT%2010.0%3B%20Win64%3B%20x64)%20AppleWebKit%2F537.36%20(KHTML%2C%20like%20Gecko)%20Chrome%2F58.0.3029.110%20Safari%2F537.36&platform=Win32&userAgent=Mozilla%2F5.0%20(Windows%20NT%2010.0%3B%20Win64%3B%20x64)%20AppleWebKit%2F537.36%20(KHTML%2C%20like%20Gecko)%20Chrome%2F58.0.3029.110%20Safari%2F537.36");
            request.Headers.Add("x-goog-encode-response-if-executable: base64");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
            request.ContentType = "application/json";
            request.Accept = "*/*";
            request.Headers.Add("cache-control: no-cache");
            request.Headers.Add("authority: clients6.google.com");
            request.Headers.Add("x-javascript-user-agent: google-api-javascript-client/1.1.0-beta");
            request.Headers.Add("x-referer: https://voice.google.com");
            request.ContentLength = data.Length;
            StreamWriter writer = new StreamWriter(request.GetRequestStream());
            writer.Write(data);
            writer.Flush();
            writer.Close();
            WebResponse response = request.GetResponse();
            StreamReader reader=new StreamReader(request.GetResponse().GetResponseStream());
            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        }
    }
}
