using DoranekoDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace TemplateHelper
{
    class Program
    {
        public enum LANG_TYPE
        {
            CS,
            VB
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //カレントフォルダ
            Console.WriteLine(Directory.GetCurrentDirectory());

            //var argsList = new List<string>();
            var argsDictionary = new Dictionary<string,string>();

            //言語のデフォルトはC#
            argsDictionary["lang"] = ((int)LANG_TYPE.CS).ToString();

            for (int i = 0; i < args.Length ; i++)
            {
                string arg = args[i];
                switch (arg.ToLower())
                {
                    case "--connectionstring":
                        if( (i+1) <= (args.Length-1))
                        {
                            argsDictionary["connectionstring"] = args[i + 1];
                            i++;
                        }
                        break;

                    case "--lang":
                        if ((i + 1) <= (args.Length - 1))
                        {
                            if (args[i + 1].ToLower() == Enum.GetName(typeof( LANG_TYPE), LANG_TYPE.CS).ToLower())
                            {
                                argsDictionary["lang"] = ((int)LANG_TYPE.CS).ToString();
                            }
                            else if (args[i + 1].ToLower() == Enum.GetName(typeof(LANG_TYPE), LANG_TYPE.VB).ToLower())
                            {
                                argsDictionary["lang"] = ((int)LANG_TYPE.VB).ToString();
                            }
                                
                            i++;
                        }
                        break;

                }

            }
        
            foreach(var dic in argsDictionary)
            {
                Console.WriteLine(dic.Key + ":" + dic.Value);
            }

            //dotnet TemplateHelper.dll --connectionstring "Data Source=.\SQLEXPRESS;Initial Catalog=DoranekoDB;Integrated Security=True; --lang VB"

            var template = "//";

            var lt = (LANG_TYPE)(int.Parse(argsDictionary["lang"]));
            if (lt == LANG_TYPE.VB)
            {
                template = "'";

            }
            template += "こちらのファイルは自動作成されたものです（TemplateHelperプロジェクト）" + "\r\n";

            template += CreateTemplate.CreateSchemaToJson(argsDictionary["connectionstring"], lt);

            template += CreateTemplate.CreateDbTable(argsDictionary["connectionstring"], lt);

            
            using (var file = new FileStream(Directory.GetCurrentDirectory() + @"\CommonDataInfo." + Enum.GetName(typeof(LANG_TYPE), lt).ToLower() , FileMode.Create)) { 
                using (var writer = new StreamWriter(file, Encoding.UTF8))
                {
                    writer.WriteLine(template);
                }
            }
        }
    }
}
