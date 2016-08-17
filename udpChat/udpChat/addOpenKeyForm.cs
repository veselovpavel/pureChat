using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace udpChat
{
    public partial class addOpenKeyForm : Form
    {
        public string contactName;
        public string openKey;

        public addOpenKeyForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            contactName = textBox1.Text;
            openKey = richTextBox1.Text;
            Close();
        }
    }
}
