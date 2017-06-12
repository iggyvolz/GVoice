using System;
using System.Collections.Generic;
using System.Net;

namespace GVoice
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("First, open voice.google.com in Google Chrome (other browsers coming soon), and log in.");
            Console.WriteLine("Cookie *must* be saved - you cannot use Incognito mode.");
            Console.WriteLine("Then type javascript:frames.___jsl.cfg.client.apiKey (and press enter) in the browser bar to get your api key");
            Console.Write("API key? ");
            string apikey = Console.ReadLine();
            string sid = "";
            string hsid = "";
            string ssid = "";
            string apisid = "";
            string sapisid = "";
            ChromeCookieReader reader = new ChromeCookieReader();
            IEnumerable<Tuple<string, string>> cookies = reader.ReadCookies(".google.com");
            foreach (Tuple<string, string> cookie in cookies)
            {
                switch(cookie.Item1)
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
            try
            {
                LcResponse response=new GVoice(apikey, sid, hsid, ssid, apisid, sapisid).GetMessages();
                foreach (LcResponse.Response.Conversation conversation in response.response.conversation)
                {
                    Console.WriteLine("Conversation with " + conversation.headingContact[0].name + ":");
                    foreach(LcResponse.Response.Conversation.PhoneCall call in conversation.phoneCall)
                    {
                        // 5: Sent message
                        // 6: Received message
                        Console.WriteLine(((call.coarseType==5)?"Me":call.contact.name) + ": "+call.messageText);
                    }
                }
            }
            catch(WebException e)
            {
                Console.WriteLine("Caught a "+e.GetType()+", check your API key?");
            }
            Console.Write("Press any key to quit...");
            Console.ReadKey();
        }
    }
}
