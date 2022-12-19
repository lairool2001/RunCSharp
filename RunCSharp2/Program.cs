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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Runtime.InteropServices;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;

namespace RunCSharp2
{
    public class Program
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        public const int OF_READWRITE = 2;
        public const int OF_SHARE_DENY_NONE = 0x40;
        public static readonly IntPtr HFILE_ERROR = new IntPtr(-1);
        public static CodeDomProvider codeDomProvider = null;

        public static void preloadCompiler()
        {
            codeDomProvider = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider();
        }
        public static void Main(string[] args)
        {
            compileAndRun(args);
        }
        /// <summary>
        /// compile cs file and run
        /// </summary>
        /// <param name="args"></param>
        /// <returns>exe output file</returns>
        public static string compileAndRun(string[] args)
        {
            //args = new string[] { @"M:\小程式\亂撥音樂.cs" };
            if (args.Length == 0) return null;
            try
            {
                string path = args[0];
                Console.WriteLine(path);
                string text = File.ReadAllText(path);
                int num = 1;
                string outputName = Path.GetFileNameWithoutExtension(path) + "-output.exe";
                string outputPath;
            output:
                outputPath = Path.Combine(Path.GetDirectoryName(path), outputName);
                if (File.Exists(outputPath))
                {
                    IntPtr vHandle = _lopen(outputPath, OF_READWRITE | OF_SHARE_DENY_NONE);
                    if (vHandle == HFILE_ERROR)
                    {
                        num++;
                        outputName = Path.GetFileNameWithoutExtension(path) + "-" + num + "-output.exe";
                        //MessageBox.Show("文件被占用！");
                        CloseHandle(vHandle);
                        goto output;
                    }
                    CloseHandle(vHandle);
                }


                /*var frameworkPath = RuntimeEnvironment.GetRuntimeDirectory();
                var cscPath = Path.Combine(frameworkPath, "csc.exe");

                Process.Start(cscPath, path);*/
                //MessageBox.Show(cscPath);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (codeDomProvider == null)
                    codeDomProvider = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider();
                System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
                parameters.GenerateExecutable = true;
                //parameters.GenerateExecutable = false;
                //parameters.GenerateInMemory = true;
                parameters.OutputAssembly = outputPath;
                CompilerResults results = codeDomProvider.CompileAssemblyFromSource(parameters, text);
                stopwatch.Stop();
                Debug.WriteLine(stopwatch.ElapsedMilliseconds + "ms");
                //Console.WriteLine("Error Count:" + results.Errors.Count);
                if (results.Errors.Count > 0)
                {
                    StringBuilder errors = new StringBuilder();
                    foreach (CompilerError item in results.Errors)
                    {
                        if (item.ErrorNumber == "CS2012")
                        {
                            goto output;
                        }
                        errors.AppendLine(item.ToString());
                    }
                    MessageBox.Show(errors.ToString());
                    //Console.ReadLine();
                }
                string[] args2 = new string[0];
                //results.CompiledAssembly.EntryPoint.Invoke(null,new object[] { args2 });
                Process.Start(outputPath);
                //File.Delete(outputPath);
                //Console.ReadLine();
                return outputPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
                /*Console.WriteLine(ex);
                Console.ReadLine();*/
            }
        }
    }
}