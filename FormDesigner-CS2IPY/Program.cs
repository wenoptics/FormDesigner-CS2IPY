using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormDesigner_CS2IPY
{
    static class Program
    {

        static string helpText = $@"
Usage:
    exec.exe gui
    or
    exec.exe convert <filename.designer.cs> <output.py>
                ";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {

            // Test if input arguments were supplied.
            if (args.Length == 0)
            {
                Console.WriteLine(helpText);
            }
            else
            {
                switch (args[0].ToLower())
                {
                    case "gui":
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new Form1());
                        return 0;
                    case "convert":
                        if (args.Length != 3)
                        {
                            Console.WriteLine("Not recognized.");
                            Console.WriteLine(helpText);
                            return -1;
                        }
                        CodeConverter.Convert(args[1], args[2]);
                        Console.WriteLine("Done!");
                        return 0;
                    default:
                        Console.WriteLine("Not recognized command");
                        Console.WriteLine(helpText);
                        return -1;
                }

            }
            return 0;
        }
    }
}
