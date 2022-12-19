using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RunCSharpsWindowsForm.Properties;
using Newtonsoft.Json;
using System.Diagnostics;

namespace RunCSharpsWindowsForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Save save;
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = Settings.Default.dir;
            if (!Directory.Exists(textBox1.Text)) return;
            loadDir();
            RunCSharp2.Program.preloadCompiler();
            if (File.Exists("save"))
            {
                save = JsonConvert.DeserializeObject<Save>(File.ReadAllText("save"));
            }
            else
            {
                save = new Save();
            }
        }
        void loadDir()
        {
            if (!Directory.Exists(textBox1.Text)) return;
            var filez = Directory.GetFiles(textBox1.Text, "*.cs");
            flowLayoutPanel1.Controls.Clear();
            foreach (var file in filez)
            {
                Button button = new Button();
                button.AutoSize = true;
                button.Text = Path.GetFileNameWithoutExtension(file);
                flowLayoutPanel1.Controls.Add(button);
                button.MouseDown += Button_Click;
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string file = Path.Combine(textBox1.Text, button.Text + ".cs");

            string content = File.ReadAllText(file);
            if (save.fileToOldContent.TryGetValue(file, out var oldContent))
            {
                if (content.Length == oldContent.Length && content == oldContent)
                {
                    string exeToRun = save.exeOutputLastPath[file];
                    if (File.Exists(exeToRun))
                    {
                        Process.Start(exeToRun);
                        return;
                    }
                }
            }
            string exe = RunCSharp2.Program.compileAndRun(new string[] { file });
            if (exe != null)
            {
                save.fileToOldContent[file] = content;
                save.exeOutputLastPath[file] = exe;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            fileSystemWatcher1.Path = textBox1.Text;
            Settings.Default.dir = textBox1.Text;
            Settings.Default.Save();
            loadDir();
        }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            loadDir();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText("save", JsonConvert.SerializeObject(save));
        }
    }
}
public class Save
{
    public Dictionary<string, string> fileToOldContent = new Dictionary<string, string>();
    public Dictionary<string, string> exeOutputLastPath = new Dictionary<string, string>();
}