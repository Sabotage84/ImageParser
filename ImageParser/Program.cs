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
            Console.WriteLine("Start parsing...");
            string html = GET(@"http://ptime.ru");
            //Console.WriteLine(html);
            //string test = "<img src=\"images / ptime_07.gif\" width=\"519\" height=\"3\" alt=\"\">";
            string testImg = FindImages(html,0, ref images);

            Console.WriteLine();
            //Console.WriteLine(testImg);
            

            foreach (var item in images)
            {
                Console.WriteLine(item);
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
