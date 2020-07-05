using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using SB3Utility;

namespace GDImport
{
    public partial class Program {
        static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: gdimport <folderWithPPFiles> <dstdir> <basename>");
                return 1;
            }

            string dstDir = args[1]+"/";
            string dName = args[2];
            Directory.CreateDirectory(dstDir + dName);
            var wr = new XXGLTFWriter(dstDir, dName);

            foreach (var ppName in Directory.GetFiles(args[0], "*.pp", SearchOption.AllDirectories))
            {
                var name = Path.GetFileName(ppName);
                var bn = Path.GetFileNameWithoutExtension(name);
                //Directory.CreateDirectory(dstDir + bn);
                using var ppStream = File.OpenRead(ppName);
                var fmt = ppFormat.GetFormat(ppStream, out var hdr);
                if (fmt == null)
                {
                    continue;
                }
                Console.WriteLine(ppName);
                var parser = new ppParser(ppStream, fmt);
                foreach (ppSubfile sub in parser.Subfiles)
                {
                    var ext = Path.GetExtension(sub.Name).Substring(1);
                    switch (ext)
                    {
                        case "tga":
                        case "bmp":
                            wr.SaveTexture(sub.CreateReadStream(), sub.Name);
                            break;
                        case "lst":
                            continue;
                            var lst = new lstParser(sub.CreateReadStream(), sub.Name);
                            File.WriteAllText(dstDir + bn + "/" + sub.Name, lst.Text);
                            break;
                        case "xx":
                            Console.WriteLine(sub.Name);
                            var xx = new xxParser(sub.CreateReadStream(), Path.GetFileNameWithoutExtension(sub.Name));
                            wr.AddScene(xx);
                            break;
                        default:
//                            Console.WriteLine("Unhandled "+sub.Name);
                            break;
                    }
                    // stop on skeleton
                    if (wr.skins != null && wr.skins.Count > 0)
                        break;
                }
            }
            wr.Save();
            var usedImages = new HashSet<string>();
            foreach (var i in wr.images)
            {
                if (!File.Exists(dstDir + i.uri))
                {
                    Console.WriteLine("{0} referenced, but does not exist", i.uri);
                }
                usedImages.Add(i.uri);
            }

            foreach (var pic in Directory.GetFiles(dstDir+dName))
            {
                if (!pic.EndsWith(".png")) continue;
                var cp = dName + "/" + Path.GetFileName(pic);
                if (!usedImages.Contains(cp))
                {
                    Console.WriteLine("{0} pruned.", cp);
                    File.Delete(pic);
                }
            }
            return 0;
        }
    }
}