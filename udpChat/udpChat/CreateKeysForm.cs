using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace udpChat
{
    public partial class CreateKeysForm : Form
    {
        public CreateKeysForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Assembly.GetExecutingAssembly().Location;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
                using (BinaryWriter writer = new BinaryWriter(File.Create(saveFileDialog.FileName)))
                {
                    writer.Write(rsa.ToXmlString(true));
                }
            }
        }
    }
}
