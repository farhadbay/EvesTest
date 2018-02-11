using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Security.AccessControl;

namespace EvesTest
{
    class Program
    {
        public static long GetDirectorySize(DirectoryInfo dir)
        {
            long size = 0;
            var FileList = dir.EnumerateFiles();
            var DirList = dir.EnumerateDirectories();
            foreach (var item in FileList)
            {
                size += item.Length;
            }
            foreach (var item in DirList)
            {
                size += GetDirectorySize(item);
            }
            return size;
        }

        public static List<Entity> GetDirectoryFileList(DirectoryInfo dir)
        {        
            List<Entity> EntityList = new List<Entity>();
            if (dir.Exists)
            {
                var DirectoryList = dir.EnumerateDirectories();
                var FileList = dir.EnumerateFiles();
                foreach (var item in DirectoryList)
                {
                    EntityList.Add(new Entity()
                    {
                        Name = item.Name,
                        EntityType = EntityType.directory,
                        Size = GetDirectorySize(item),
                        ParentName = dir.Name,
                        Entity_List = GetDirectoryFileList(item)
                    });
                }
                foreach (var item in FileList)
                {
                    EntityList.Add(new Entity()
                    {
                        Name = item.Name,
                        EntityType = EntityType.file,
                        Size = item.Length,
                        MimeType = MimeMapping.GetMimeMapping(item.FullName),
                        ParentName = dir.Name
                    });
                }

            }
            else
            {
                Console.WriteLine("The directory is not exist");
            }

            return EntityList;
        }

        public static void CreateHtmlOutput(List<Entity> Entitylist)
        {
            try
            {
                JavaScriptSerializer sr = new System.Web.Script.Serialization.JavaScriptSerializer();
                var SerializedEntity = sr.Serialize(Entitylist);
                var foldername = Directory.GetCurrentDirectory();

                DirectorySecurity DS = Directory.GetAccessControl(foldername);
                AuthorizationRuleCollection collection = DS.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                bool hasWriteAccess = false;
                foreach (FileSystemAccessRule rule in collection)
                {
                    if (rule.AccessControlType == AccessControlType.Allow)
                    {
                        hasWriteAccess = true;
                        break;
                    }
                }

                if(!hasWriteAccess)
                {
                    Exception ex = new Exception("In the containing folder you dont have write access to create output.html");
                    throw ex;
                }
                var outputpath = Path.Combine(foldername, "output.html");
                SerializedEntity = HttpUtility.JavaScriptStringEncode(SerializedEntity, true);

                using (FileStream fs = File.Open(outputpath, FileMode.Create, FileAccess.ReadWrite))
                {

                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.AutoFlush = true;
                        var sb = new StringBuilder();
                        sb.AppendLine("<head>");
                        sb.AppendLine("<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js\"></script>");
                        sb.AppendLine("<script>");
                        sb.AppendLine("var EntityListSerilized =" + SerializedEntity);
                        sb.AppendLine("$(document).ready(function () {");
                        sb.AppendLine("var EntityList = $.parseJSON(EntityListSerilized);");
                        sb.AppendLine("$('body').append(CreateTree(EntityList));");
                        sb.AppendLine("});");
                        sb.AppendLine("function CreateTree(EntityList){");
                        sb.AppendLine("var root = $('<ul>');");
                        sb.AppendLine("$(EntityList).each(function () {");
                        sb.AppendLine("var child = $('<li>');");
                        sb.AppendLine("var value = this.Name + \"(\" + this.Size + \"bytes)\";");
                        sb.AppendLine("if (this.EntityType == 1)");
                        sb.AppendLine("value += \" MimeType: \" + this.MimeType");
                        sb.AppendLine("if (this.Entity_List != null && this.Entity_List.length != 0) {");
                        sb.AppendLine("value += CreateTree(this.Entity_List)[0].outerHTML;");
                        sb.AppendLine("}");
                        sb.AppendLine("child.html(value);");
                        sb.AppendLine("root.append(child);");
                        sb.AppendLine("});");
                        sb.AppendLine("return root;");
                        sb.AppendLine("}");
                        sb.AppendLine("</script>");
                        sb.AppendLine("</head>");
                        sb.AppendLine("<body>");
                        sb.AppendLine("</body>");
                        
                        sw.Write(sb.ToString());
                        
                    }
                }
                Console.WriteLine("The \"output.html\" created in the {0} successfuly", foldername);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Please insert directory path");
            string path = Console.ReadLine();
            if (path.Trim() != "")
            {
                DirectoryInfo dirinfo = new DirectoryInfo(path);

                var Entitylist = GetDirectoryFileList(dirinfo);


                
                try
                {
                    Entitylist.ForEach((item) =>
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(item.Name + "::");
                        sb.Append(item.EntityType + "::");
                        sb.Append(item.Size + "::");
                        sb.Append(item.ParentName);
                        if (item.EntityType == EntityType.file)
                            sb.Append("::" + item.MimeType + "::");
                        sb.Append("\n-------------------------------------------");
                        Console.WriteLine(sb.ToString());
                    });

                    ///Create output.html
                    CreateHtmlOutput(Entitylist);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Console.ReadLine();
            }

        }
    }
}
