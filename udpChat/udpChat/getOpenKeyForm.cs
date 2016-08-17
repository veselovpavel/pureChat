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
    public partial class getOpenKeyForm : Form
    {
        public getOpenKeyForm()
        {
            InitializeComponent();
        }

        public getOpenKeyForm(string text)
        {
            InitializeComponent();
            richTextBox1.AppendText(text);
        }

        public void writeKey(string text)
        {
            richTextBox1.AppendText(text);
        }
    }
}
