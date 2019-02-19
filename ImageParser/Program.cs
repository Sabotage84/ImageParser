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
            site = @"http://www.ptime.ru/Metronom/servers/Metronom3000.html";
            //site = @"http://ptime.ru/";
            //site = @"https://ogo1.ru";


            Console.WriteLine("Start parsing...");

            //string html = GET(@"http://www.ptime.ru/Metronom/servers/Metronom300.html/..");

            //Console.WriteLine(html);


            
            //images = FindTagSRC(html, "img", "src=\"");

            links = GetAllLinksFromSite(site, 3, site);
            ShowList(links);

            foreach (var item in links)
            {
                List<string> ls = new List<string>();
                string[] words = site.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                string tempLink = "";
                if (item.StartsWith(words[words.Length - 1]))
                    tempLink = "http://" + item;
                else
                    tempLink = site + item;
                ls = (FindTagSRC(GET(tempLink), "img", "src=\""));
                ls = CorrectLilks(site+item, ls);
                images.AddRange(ls);
            }

            
            Console.WriteLine();
            Console.WriteLine();
            ShowList(images);
            List<string> unicImages = new List<string> (images.Distinct());

            ShowList(unicImages);

            foreach (var item in unicImages)
            {
                string fullLink = site +  item;
                DowadImage(fullLink);
            }


            Console.WriteLine("Parsing finished...");

            Console.ReadKey();
        }

        private static List<string> GetAllLinksFromSite(string site, int deep, string originalSite)
        {
            List<string> temp = new List<string>();
            List<string> temp2 = new List<string>();

            if (deep!=0 )
            {
                string html = GET(site);
                temp = GetLinksFromPage(html);
                ShowList(temp);

                temp=CorrectLilks(site, temp);

                ShowList(temp);
                foreach (var item in temp)
                {
                     temp2.AddRange(GetAllLinksFromSite(originalSite+item, deep-1, originalSite));
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
            string[] levels = site.Split('/');
           
            foreach (var item in temp)
            {
                List<string> l = levels.ToList();
                l.Remove(l.Last());
                string[] linkLevels = item.Split('/');
                int up = 0;
                for (int i = 0; i <linkLevels.Length; i++)
                {
                    if (linkLevels[i] == "..")
                    {
                        up++;
                    }
                }
                string str = "";
                if (up > 0)
                    str = l[l.Count - 1 - up]+"/";
                if (up == 0)
                    str = l[l.Count - 1] + "/";
                
                foreach (var item2 in linkLevels)
                {
                    if (item2!="..")
                        str += item2 + @"/";
                }
                correctedList.Add(str.Substring(0,str.Length-1));
            }


            return correctedList;
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
                WebRequest req = WebRequest.Create(Url);
                WebResponse resp = req.GetResponse();
                //Console.WriteLine(resp.Headers.ToString());
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
                return Out; 
            }
        }
    }
}
