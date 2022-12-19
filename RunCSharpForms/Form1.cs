using RunCSharp2;
using RunCSharpForms.Properties;

namespace RunCSharpForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = Settings.Default.dir;
            var filez = Directory.GetFiles(textBox1.Text, "*.cs");
            listBox1.Items.Clear();
            listBox1.Items.AddRange(filez);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == 0) return;
            string file = listBox1.SelectedItem.ToString();
            ProgramCompile.Run(new string[] { file });
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.dir = textBox1.Text;
            Settings.Default.Save();
        }
    }
}