namespace udpChat
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.действияToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.создатьПаруКлючейToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.открытьФайлСКлючамиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.получитьСвойОткрытыйКлючToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.добавитьОткрытыйКлючToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 68);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(625, 311);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // richTextBox2
            // 
            this.richTextBox2.Location = new System.Drawing.Point(12, 385);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.Size = new System.Drawing.Size(625, 91);
            this.richTextBox2.TabIndex = 1;
            this.richTextBox2.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 482);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(139, 50);
            this.button1.TabIndex = 2;
            this.button1.Text = "Отправить";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(12, 38);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(211, 24);
            this.comboBox1.TabIndex = 3;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.действияToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(658, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // действияToolStripMenuItem
            // 
            this.действияToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.создатьПаруКлючейToolStripMenuItem,
            this.открытьФайлСКлючамиToolStripMenuItem,
            this.получитьСвойОткрытыйКлючToolStripMenuItem,
            this.добавитьОткрытыйКлючToolStripMenuItem});
            this.действияToolStripMenuItem.Name = "действияToolStripMenuItem";
            this.действияToolStripMenuItem.Size = new System.Drawing.Size(86, 24);
            this.действияToolStripMenuItem.Text = "Действия";
            // 
            // создатьПаруКлючейToolStripMenuItem
            // 
            this.создатьПаруКлючейToolStripMenuItem.Name = "создатьПаруКлючейToolStripMenuItem";
            this.создатьПаруКлючейToolStripMenuItem.Size = new System.Drawing.Size(298, 26);
            this.создатьПаруКлючейToolStripMenuItem.Text = "Создать пару ключей";
            this.создатьПаруКлючейToolStripMenuItem.Click += new System.EventHandler(this.создатьПаруКлючейToolStripMenuItem_Click);
            // 
            // открытьФайлСКлючамиToolStripMenuItem
            // 
            this.открытьФайлСКлючамиToolStripMenuItem.Name = "открытьФайлСКлючамиToolStripMenuItem";
            this.открытьФайлСКлючамиToolStripMenuItem.Size = new System.Drawing.Size(298, 26);
            this.открытьФайлСКлючамиToolStripMenuItem.Text = "Открыть файл с ключами";
            this.открытьФайлСКлючамиToolStripMenuItem.Click += new System.EventHandler(this.открытьФайлСКлючамиToolStripMenuItem_Click);
            // 
            // получитьСвойОткрытыйКлючToolStripMenuItem
            // 
            this.получитьСвойОткрытыйКлючToolStripMenuItem.Name = "получитьСвойОткрытыйКлючToolStripMenuItem";
            this.получитьСвойОткрытыйКлючToolStripMenuItem.Size = new System.Drawing.Size(298, 26);
            this.получитьСвойОткрытыйКлючToolStripMenuItem.Text = "Получить свой открытый ключ";
            this.получитьСвойОткрытыйКлючToolStripMenuItem.Click += new System.EventHandler(this.получитьСвойОткрытыйКлючToolStripMenuItem_Click);
            // 
            // добавитьОткрытыйКлючToolStripMenuItem
            // 
            this.добавитьОткрытыйКлючToolStripMenuItem.Name = "добавитьОткрытыйКлючToolStripMenuItem";
            this.добавитьОткрытыйКлючToolStripMenuItem.Size = new System.Drawing.Size(298, 26);
            this.добавитьОткрытыйКлючToolStripMenuItem.Text = "Добавить контакт";
            this.добавитьОткрытыйКлючToolStripMenuItem.Click += new System.EventHandler(this.добавитьОткрытыйКлючToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(275, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(299, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "Выполняется подключение к сети";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(658, 544);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBox2);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "pureChat";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem действияToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem открытьФайлСКлючамиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem добавитьОткрытыйКлючToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem создатьПаруКлючейToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem получитьСвойОткрытыйКлючToolStripMenuItem;
        private System.Windows.Forms.Label label1;
    }
}

