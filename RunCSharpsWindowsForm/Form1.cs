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
                listBox1.Items.Clear();
                var asmEnumerator = save.asamblyHashSet.GetEnumerator();
                while (asmEnumerator.MoveNext())
                {
                    listBox1.Items.Add(asmEnumerator.Current);
                }
            }
            else
            {
                save = new Save();
                save.asamblyHashSet.Add("System.dll");
                save.asamblyHashSet.Add("mscorlib.dll");
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

            FileInfo fileInfo = new FileInfo(file);
            string content = File.ReadAllText(file);
            if (save.fileToEditDateTime.TryGetValue(file, out var lastWriteTime))
            {
                if (fileInfo.LastWriteTime == lastWriteTime)
                {
                    string exeToRun = save.exeOutputLastPath[file];
                    if (File.Exists(exeToRun))
                    {
                        Process.Start(exeToRun);
                        label1.Text = "cache";
                        return;
                    }
                }
            }
            string exe = RunCSharp2.Program.compileAndRun(file, out var ms, save.asamblyHashSet.ToArray());
            if (exe != null)
            {
                save.fileToEditDateTime[file] = fileInfo.LastWriteTime;
                save.exeOutputLastPath[file] = exe;
            }
            label1.Text = $"{ms} ms";
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
            updateAsamblyHashSet();
            File.WriteAllText("save", JsonConvert.SerializeObject(save));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.FindString(textBox2.Text) != -1) return;
            listBox1.Items.Add(textBox2.Text);
            updateAsamblyHashSet();
            textBox2.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            listBox1.Items.Remove(listBox1.SelectedItem);
            updateAsamblyHashSet();
        }
        void updateAsamblyHashSet()
        {
            save.asamblyHashSet.Clear();
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                save.asamblyHashSet.Add(listBox1.Items[i].ToString());
            }
        }
    }
}
public class Save
{
    public Dictionary<string, DateTime> fileToEditDateTime = new Dictionary<string, DateTime>();
    public Dictionary<string, string> exeOutputLastPath = new Dictionary<string, string>();
    public HashSet<string> asamblyHashSet = new HashSet<string>();
}
