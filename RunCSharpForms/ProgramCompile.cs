using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
namespace RunCSharp2
{
    public class ProgramCompile
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        public const int OF_READWRITE = 2;
        public const int OF_SHARE_DENY_NONE = 0x40;
        public static readonly IntPtr HFILE_ERROR = new IntPtr(-1);
        public static CodeDomProvider codeDomProvider = null;

        public static void Run(string[] args)
        {
            //args = new string[] { @"M:\小程式\亂撥音樂.cs" };
            if (args.Length == 0) return;
            try
            {
                string path = args[0];
                Console.WriteLine(path);
                string text = File.ReadAllText(path);
                var script = CSharpScript.Create(text, ScriptOptions.Default.WithReferences(
                    typeof(ProgramCompile).Assembly).WithImports("RunCSharp2").WithImports("System"));
                script.Compile();
                script.RunAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                /*Console.WriteLine(ex);
                Console.ReadLine();*/
            }
        }
    }
}