using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Fleck_Forms
{
    public partial class Form2 : Form
    {
        //1.声明自适应类实例
        AutoSizeFormClass asc = new AutoSizeFormClass();
        public Form2()
        {
            InitializeComponent();

            Form1 form1 = new Form1("9001");
            form1.TopLevel = false;
            form1.ControlBox = false;
            form1.Dock = System.Windows.Forms.DockStyle.Fill;
            form1.Show();  

            TabPage tabpage = new System.Windows.Forms.TabPage("9001");
            tabpage.Name = "9001";
            tabpage.AutoScroll = true;
            tabpage.Text = "9001";
            this.tabControl1.Controls.Add(tabpage);
            tabpage.Controls.Add(form1);

//             Form1 form2 = new Form1("9003");
//             form2.TopLevel = false;
//             form2.ControlBox = false;
//             form2.Dock = System.Windows.Forms.DockStyle.Fill;
//             form2.Show();
// 
//             TabPage tabpage2 = new System.Windows.Forms.TabPage("9003");
//             tabpage2.Name = "9003";
//             tabpage2.AutoScroll = true;
//             tabpage2.Text = "9003";
//             this.tabControl1.Controls.Add(tabpage2);
//             tabpage2.Controls.Add(form2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
        }

        private void Form2_SizeChanged(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            asc.controllInitializeSize(this);
        }
    }
}
