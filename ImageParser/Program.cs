using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace ImageParser
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> images = new List<string>();
            string site = "";
            site = @"http://ptime.ru/";
            //site = @"https://ogo1.ru";


            Console.WriteLine("Start parsing...");

            string html = GET(site);
            
            images = FindTagSRC(html, "img");
            
            Console.WriteLine();
            Console.WriteLine();

            var unicImages = images.Distinct();

            foreach (var item in unicImages)
            {
                Console.WriteLine(item);
            }

            foreach (var item in unicImages)
            {
                string fullLink = site +  item;
                DowadImage(fullLink);
            }


            Console.WriteLine("Parsing finished...");

            Console.ReadKey();
        }

        private static string FindImages(string test, int startIndex, ref List<string> ttt)
        {
            int imgIndex = test.IndexOf("<img", startIndex);
            if (imgIndex != -1)
            {
                int srcIndex = test.IndexOf("src=\"", imgIndex + 4);
                int endIndex = test.IndexOf("\"", srcIndex + 5);
                string temp = "";
                temp = test.Substring(srcIndex + 5, endIndex - srcIndex - 5) ;

                if (endIndex >= test.Length || imgIndex == -1)
                {
                    ttt.Add(temp);
                    return temp;
                }
                else
                {
                    ttt.Add(temp);
                    return temp = temp + FindImages(test, endIndex, ref ttt);
                }
            }
            else
                return "";
        }

        private static List<string> FindTagSRC(string test, string tag)
        {
            int startIndex = 0;
            int endIndex = 0;
            int imgIndex = 0;
            string temp = "";
            List<string> images = new List<string>();
            while (imgIndex != -1)
            {
                imgIndex = test.IndexOf("<"+tag, startIndex);
                if (endIndex <= test.Length && imgIndex != -1)
                {
                    int srcIndex = test.IndexOf("src=\"", imgIndex + 4);
                    endIndex = test.IndexOf("\"", srcIndex + 5);
                    
                    temp = test.Substring(srcIndex + 5, endIndex - srcIndex - 5);
                    
                    images.Add(temp);
                    startIndex = endIndex;
                }
            }
            return images;
        }

        private static void DowadImage(string link)
        {
            string[] words = link.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            string fileName = words[words.Length - 1];
            using (WebClient wClient = new WebClient())
            {
                string path = @"img\"+fileName;
                Console.WriteLine("Download - " + link);
                Uri url = new Uri(link);
                try
                {
                    wClient.DownloadFile(url, path);
                }
                catch
                {
                    Console.WriteLine("BAD LINK!");
                }
            }
        }

        private static string GET(string Url)
        {
            WebRequest req = WebRequest.Create(Url);
            WebResponse resp = req.GetResponse();
            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string Out = sr.ReadToEnd();
            sr.Close();
            return Out;
        }
    }
}
