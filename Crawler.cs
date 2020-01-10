using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using AngleSharp;

namespace Crawler
{
    class Crawler
    {
        private static HttpClient httpClient = new HttpClient();
        private Uri StartUrl { get; set; }
        private List<string> FileTypes { get; set; }
        private Queue<string> PendingUrl { get; set; } = new Queue<string>();
        private HashSet<string> ErrorUrl { get; set; } = new HashSet<string>();
        private HashSet<string> VisitedUrl { get; set; } = new HashSet<string>();

        public Crawler(string startUrl, IEnumerable<string> fileTypes)
        {
            try
            {
                StartUrl = new Uri(startUrl);
            }
            catch (System.UriFormatException)
            {
                CheckURL: Console.WriteLine("初始网址输入有误, 请重新输入:");
                try
                {
                    StartUrl = new Uri(Console.ReadLine());
                }
                catch (System.UriFormatException)
                {
                    if (StartUrl == null) goto CheckURL;
                }
            }
            finally
            {
                FileTypes = fileTypes.ToList();
            }
        }

        public async Task run()
        {
            Console.WriteLine($"\n初始网址: {StartUrl}\n");
            Console.WriteLine("文件类型:");
            foreach (var fileType in FileTypes)
            {
                Console.WriteLine($"    {fileType}");
            }
            PendingUrl.Enqueue(StartUrl.ToString());
            while (PendingUrl.Count > 0)
            {
                await handle(PendingUrl.Dequeue());
            }
        }

        async Task handle(string url)
        {
            if (VisitedUrl.Add(url))
            {
                var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
                try
                {
                    var document = await context.OpenAsync(url);
                    
                    foreach (var item in document.QuerySelectorAll("[href]"))
                    {
                        var u = fixUrl(item.GetAttribute("href"));
                        try
                        {
                            if (new Uri(u).Host.EndsWith(StartUrl.Host) && !PendingUrl.Contains(u) && !VisitedUrl.Contains(u)) PendingUrl.Enqueue(u);
                        }
                        catch (System.UriFormatException)
                        {
                            ErrorUrl.Add(u);
                        }
                    }

                    foreach (var item in document.QuerySelectorAll("[src]"))
                    {
                        var u = fixUrl(item.GetAttribute("src"));
                        if (FileTypes.Contains(u.Split('.')[^1]) && VisitedUrl.Add(u)) await download(u);
                    }
                }
                catch (System.Exception e)
                {   
                    Console.WriteLine($"IGNORE ERROR: {e} on URL: {url}");
                }
            }
        }

        async Task download(string url)
        {
            try
            {
                var path = $@"./Downloads/{StartUrl.Host}";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                using (var webFileStream = await httpClient.GetStreamAsync(url))
                {
                    using (var localFileStream = File.OpenWrite(path + $"/{url.Split('/')[^1]}"))
                    {
                        Console.WriteLine($"正在下载: {url.Split('/')[^1]} - {url}");
                        await webFileStream.CopyToAsync(localFileStream);
                        Console.WriteLine($"已下载:   {url.Split('/')[^1]} - {url}");
                    }
                }
            }
            catch(SystemException e)
            {
                throw e;
            }
        }

        string fixUrl(string url)
        {
            if (url.StartsWith("//")) url = $"{StartUrl.Scheme}:{url}";
            if (url.StartsWith("/")) url = $"{StartUrl}{url.TrimStart('/')}";
            return url;
        }
    }
}
