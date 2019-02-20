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
            List<string> links = new List<string>();
            string site = "";
            
            site = @"http://ptime.ru/";
            //site = @"https://ogo1.ru";


            Console.WriteLine("Start parsing...");

            links = GetAllLinksFromSite(site, 3, site);
            //ShowList(links);

            //CheckLinks(links);

            foreach (var item in links)
            {
                List<string> ls = new List<string>();
               
                ls = (FindTagSRC(GET(item), "img", "src=\""));
                ls = CorrectLilks(item, ls);
                images.AddRange(ls);
            }

            
            Console.WriteLine();
            Console.WriteLine();
            //ShowList(images);
            List<string> unicImages = new List<string> (images.Distinct());

            ShowList(unicImages);

            foreach (var item in unicImages)
            {
                DowadImage(item);
            }


            Console.WriteLine("Parsing finished...");

            Console.ReadKey();
        }

        private static void CheckLinks(List<string> links)
        {
            foreach (var item in links)
            {
                GET(item);
            }
        }

        private static List<string> GetAllLinksFromSite(string site, int deep, string originalSite)
        {
            List<string> temp = new List<string>();
            List<string> temp2 = new List<string>();

            if (deep!=0 )
            {
                string html = GET(site);
                temp = GetLinksFromPage(html);
                //ShowList(temp);
                temp=CorrectLilks(site, temp);
                //ShowList(temp);
                foreach (var item in temp)
                {
                     temp2.AddRange(GetAllLinksFromSite(item, deep-1, originalSite));
                }
                
                temp.AddRange(new List<string>( temp2.Distinct()));
                
            }
            
            temp = new List<string>(temp.Distinct());
            //ShowList(temp);
            return temp;
        }

        private static List<string> CorrectLilks(string site, List<string> temp)
        {
            List<string> correctedList = new List<string>();
            string[] levels = site.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
           
            foreach (var item in temp)
            {
                List<string> l = levels.ToList();
                string correctedLink = "";
                if (item.Contains(@"../"))
                {
                    l.Remove(l.Last());
                    correctedLink = LevelUpCorrected(item, l);
                }
                else
                    correctedLink = LocationCorrected(item, l);
                
                correctedList.Add(correctedLink);
            }
            return correctedList;
        }

        private static string LocationCorrected(string item, List<string> l)
        {
            string str = @"http://";
            if (l.Last().EndsWith("html"))
                l.Remove(l.Last());
            for (int i = 1; i < l.Count; i++)
            {
                str += l[i] + "/";
            }
            return str + item;
        }

        private static string LevelUpCorrected(string item, List<string> l)
        {
            string[] linkLevels = item.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string corLink = "";
            int up = 0;
            for (int i = 0; i < linkLevels.Length; i++)
            {
                if (linkLevels[i] == "..")
                    up++;
            }
            string str = @"http://";
            for (int i = 1; i < l.Count-up; i++)
            {
                str += l[i]+"/";
            }
            for (int i = 0; i < linkLevels.Length; i++)
            {
                if (linkLevels[i] != "..")
                    str += linkLevels[i] + @"/";
            }

            return corLink=str.Substring(0,str.Length-1);
        }

        private static void ShowList(List<string> ls)
        {
            foreach (var item in ls)
            {
                Console.WriteLine(item);
            }
        }

        private static List<string> GetLinksFromPage(string page)
        {
            List<string> temp = new List<string>();
            List<string> temp3 = new List<string>();
            temp = FindTagSRC(page, @"a ", "href=\"");
            foreach (var item in temp)
            {
                if (item.EndsWith(".html"))
                    temp3.Add(item);
            }

            temp = new List<string>(temp3.Distinct());
            return temp;
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

        private static List<string> FindTagSRC(string test, string tag, string tagAtrr)
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
                    int srcIndex = test.IndexOf(tagAtrr, imgIndex +tag.Length+ 1);
                    endIndex = test.IndexOf("\"", srcIndex + tagAtrr.Length);
                    
                    temp = test.Substring(srcIndex + tagAtrr.Length, endIndex - srcIndex - tagAtrr.Length);
                    
                    images.Add(temp);
                    startIndex = endIndex;
                }
            }
            images = new List<string>(images.Distinct());
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
            string Out = "";
            try
            {
                //Console.WriteLine("Try GET: "+Url);
                WebRequest req = WebRequest.Create(Url);
                WebResponse resp = req.GetResponse();
                
                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
            
            Out = sr.ReadToEnd();
            sr.Close();
            return Out;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(Url);
                Console.ReadKey();
                return Out; 
            }
        }
    }
}
