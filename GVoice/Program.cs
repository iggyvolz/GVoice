using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;

namespace GVoice
{
    class Program
    {
        static void GetCookies(out string apikey, out string sid, out string hsid, out string ssid, out string apisid, out string sapisid)
        {
            Console.WriteLine("First, open voice.google.com in Google Chrome (other browsers coming soon), and log in.");
            Console.WriteLine("Cookie *must* be saved - you cannot use Incognito mode.");
            Console.WriteLine("Then type javascript:frames.___jsl.cfg.client.apiKey (and press enter) in the browser bar to get your api key");
            Console.Write("API key? ");
            apikey = Console.ReadLine();
            sid = "";
            hsid = "";
            ssid = "";
            apisid = "";
            sapisid = "";
            ChromeCookieReader reader = new ChromeCookieReader();
            IEnumerable<Tuple<string, string>> cookies = reader.ReadCookies(".google.com");
            foreach (Tuple<string, string> cookie in cookies)
            {
                switch (cookie.Item1)
                {
                    case "SID":
                        sid = cookie.Item2;
                        break;
                    case "HSID":
                        hsid = cookie.Item2;
                        break;
                    case "SSID":
                        ssid = cookie.Item2;
                        break;
                    case "APISID":
                        apisid = cookie.Item2;
                        break;
                    case "SAPISID":
                        sapisid = cookie.Item2;
                        break;
                }
            }
            if (sid == "" || hsid == "" || ssid == "" || apisid == "" || sapisid == "")
            {
                Console.WriteLine("Could not read cookies");
                Console.WriteLine("Please log into voice.google.com using Google Chrome");
                return;
            }
        }
        static void Main(string[] args)
        {
            string apikey, sid, hsid, ssid, apisid, sapisid;
            GetCookies(out apikey, out sid, out hsid, out ssid, out apisid, out sapisid);
            GVoice gVoice = new GVoice(apikey, sid, hsid, ssid, apisid, sapisid);
            Console.WriteLine("Phone number?");
            Console.WriteLine("Use 10 digits (ex. 15555555555)");
            string conversationId = "";
            while(true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if(Regex.IsMatch(line, "[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]"))
                {
                    conversationId = "t.+" + line;
                    break;
                }
                Console.WriteLine("Invalid phone number");
            }
            List<LcResponse.Response.Conversation.PhoneCall> messages = new List<LcResponse.Response.Conversation.PhoneCall>();
            Thread checkMessagesThread = new Thread(() =>
                {
                    while(true)
                    {
                        LcResponse response = gVoice.GetMessages(20);
                        LcResponse.Response.Conversation conversation = response.response.conversation.First((LcResponse.Response.Conversation c) => c.id == conversationId);
                        for (int i = conversation.phoneCall.Length - 1; i >= 0; i--)
                        {
                            LcResponse.Response.Conversation.PhoneCall call = conversation.phoneCall[i];
                            if (!messages.Contains(call))
                            {
                                messages.Add(call);
                                Console.WriteLine(((call.coarseType == 5) ? "Me" : call.contact.name) + ": " + call.messageText);
                            }
                        }
                        Thread.Sleep(5000);
                    }
                });
            checkMessagesThread.Start();
            while(true)
            {
                string message = Console.ReadLine();
                gVoice.SendMessage(message, conversationId);
            }
        }
    }
}
