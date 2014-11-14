using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using System.Windows;
using System.Net;
using System.IO;
using mshtml;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Globalization;

namespace ChatTranslator
{
    class Program
    {
        public static Menu Config;
        public static String[] fromArray = new String[] { "auto", "en", "de", "es", "fr", "pl", "hu", "sq", "sv", "ro", "da", "bg", "sr", "sk", "sl", "sv", "tr", "it" };
        public static String[] toArray = new String[] { "en", "de", "es", "fr", "pl", "hu", "sq", "sv", "ro", "da", "bg", "sr", "sk", "sl", "sv", "tr", "it" };
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;


        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("ChatTranslator", "ChatTranslator", true);
            Config.AddSubMenu(new Menu("Options", "Options"));
            Config.SubMenu("Options").AddItem(new MenuItem("From", "From: ").SetValue(new StringList(fromArray)));
            Config.SubMenu("Options").AddItem(new MenuItem("To", "To: ").SetValue(new StringList(toArray)));
            Config.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            Config.AddToMainMenu();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- ChatTranslator</font>");
            //Game.OnGameInput += Game_GameInput;
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        private static void Game_GameInput(GameInputEventArgs args)
        {


        }
        static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == 0x68)
            {
                var p = new GamePacket(args);
                string textFromChat = p.ReadString(54);
                Echo(textFromChat);
                //System.IO.File.WriteAllText(@"C:\Users\Public\TestFolder\WriteText.txt", textFromChat);
                //Game.PrintChat(p.Dump());

            }
        }


        private static async void Echo(string text)
        {

            if (text.Length > 1)
            {
                string from = fromArray[Config.Item("From").GetValue<StringList>().SelectedIndex];
                string to = toArray[Config.Item("To").GetValue<StringList>().SelectedIndex];
                string x = await TranslateGoogle(text, from, to);
                Game.PrintChat(x);

            }

        }

        private static async Task<string> TranslateGoogle(string text, string fromCulture, string toCulture)
        {


            string url = string.Format(@"http://translate.google.com/translate_a/t?client=j&text={0}&hl=en&sl={1}&tl={2}",
                               text.Replace(' ', '+'), fromCulture, toCulture);
            //System.IO.File.WriteAllText(@"C:\Users\Public\TestFolder\WriteText.txt", url);

            string html;
            try
            {
                WebClient web = new WebClient();


                web.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0");
                web.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");


                web.Encoding = Encoding.UTF8;
                System.Uri uri = new System.Uri(url);
                html = await DownloadStringAsync(uri);

            }
            catch (Exception ex)
            {

                return "Error: Can't connect to Google Translate services.";
            }
            JavaScriptSerializer ser = new JavaScriptSerializer();
            if (toCulture.Equals(ser.Deserialize(Regex.Match(html, "src\":(\".*?\"),\"", RegexOptions.IgnoreCase).Groups[1].Value, typeof(string)) as string))
            {
                return "";
            }

            string result = "(" + ser.Deserialize(Regex.Match(html, "src\":(\".*?\"),\"", RegexOptions.IgnoreCase).Groups[1].Value, typeof(string)) as string + " => " + toCulture + ")";
            result += ser.Deserialize(Regex.Match(html, "trans\":(\".*?\"),\"", RegexOptions.IgnoreCase).Groups[1].Value, typeof(string)) as string;

            if (string.IsNullOrEmpty(result))
            {

                return "Error: Can't translate the message.";
            }


            return result;
        }
        public static Task<string> DownloadStringAsync(Uri url)
        {
            var tcs = new TaskCompletionSource<string>();
            var wc = new WebClient();
            wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0");
            wc.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");
            wc.Encoding = Encoding.UTF8;
            wc.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.TrySetException(e.Error);
                else if (e.Cancelled) tcs.TrySetCanceled();
                else tcs.TrySetResult(e.Result);
            };
            wc.DownloadStringAsync(url);
            return tcs.Task;
        }
    }
}
