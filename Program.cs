using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Crawler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string startUrl;
            List<string> fileTypes;
            if (args.Length < 2)
            { 
                Console.WriteLine("参数输入错误,请重新输入.");
                Console.WriteLine("初始网址:");
                startUrl = Console.ReadLine();
                Console.WriteLine("文件类型(以空格分隔):");
                fileTypes = new List<string>(Console.ReadLine().Split(' '));
            }
            else
            {
                startUrl = args[0];
                fileTypes = new List<string>(args[1..]);
            }
            await new Crawler(startUrl, fileTypes).run();
        }
    }
}
