namespace Fleck_Forms
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.m_depth = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.listView4 = new System.Windows.Forms.ListView();
            this.button2 = new System.Windows.Forms.Button();
            this.m_Redis = new System.Windows.Forms.CheckBox();
            this.m_CloudApi = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btn_clear = new System.Windows.Forms.Button();
            this.btn_reset = new System.Windows.Forms.Button();
            this.listView3 = new System.Windows.Forms.ListView();
            this.listView2 = new System.Windows.Forms.ListView();
            this.listView1 = new System.Windows.Forms.ListView();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btn_closeall = new System.Windows.Forms.Button();
            this.btn_expend = new System.Windows.Forms.Button();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button3 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button3);
            this.panel1.Controls.Add(this.m_depth);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.listView4);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.m_Redis);
            this.panel1.Controls.Add(this.m_CloudApi);
            this.panel1.Controls.Add(this.checkBox1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.btn_clear);
            this.panel1.Controls.Add(this.btn_reset);
            this.panel1.Controls.Add(this.listView3);
            this.panel1.Controls.Add(this.listView2);
            this.panel1.Controls.Add(this.listView1);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.btn_closeall);
            this.panel1.Controls.Add(this.btn_expend);
            this.panel1.Controls.Add(this.treeView1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1315, 425);
            this.panel1.TabIndex = 0;
            // 
            // m_depth
            // 
            this.m_depth.Location = new System.Drawing.Point(1046, 6);
            this.m_depth.Name = "m_depth";
            this.m_depth.Size = new System.Drawing.Size(38, 21);
            this.m_depth.TabIndex = 45;
            this.m_depth.TextChanged += new System.EventHandler(this.m_depth_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(999, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 44;
            this.label1.Text = "深度：";
            // 
            // listView4
            // 
            this.listView4.Location = new System.Drawing.Point(367, 382);
            this.listView4.Name = "listView4";
            this.listView4.Size = new System.Drawing.Size(945, 37);
            this.listView4.TabIndex = 43;
            this.listView4.UseCompatibleStateImageBehavior = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(277, 387);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 42;
            this.button2.Text = "testSQL";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // m_Redis
            // 
            this.m_Redis.AutoSize = true;
            this.m_Redis.Checked = true;
            this.m_Redis.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_Redis.Location = new System.Drawing.Point(939, 11);
            this.m_Redis.Name = "m_Redis";
            this.m_Redis.Size = new System.Drawing.Size(54, 16);
            this.m_Redis.TabIndex = 39;
            this.m_Redis.Text = "Redis";
            this.m_Redis.UseVisualStyleBackColor = true;
            // 
            // m_CloudApi
            // 
            this.m_CloudApi.AutoSize = true;
            this.m_CloudApi.Checked = true;
            this.m_CloudApi.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_CloudApi.Location = new System.Drawing.Point(890, 10);
            this.m_CloudApi.Name = "m_CloudApi";
            this.m_CloudApi.Size = new System.Drawing.Size(48, 16);
            this.m_CloudApi.TabIndex = 40;
            this.m_CloudApi.Text = "云库";
            this.m_CloudApi.UseVisualStyleBackColor = true;
            this.m_CloudApi.CheckedChanged += new System.EventHandler(this.m_CloudApi_CheckedChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(546, 12);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(72, 16);
            this.checkBox1.TabIndex = 41;
            this.checkBox1.Text = "JSON格式";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1205, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 38;
            this.button1.Text = "停止";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_clear
            // 
            this.btn_clear.Location = new System.Drawing.Point(177, 387);
            this.btn_clear.Name = "btn_clear";
            this.btn_clear.Size = new System.Drawing.Size(75, 23);
            this.btn_clear.TabIndex = 37;
            this.btn_clear.Text = "清空消息";
            this.btn_clear.UseVisualStyleBackColor = true;
            this.btn_clear.Click += new System.EventHandler(this.btn_clear_Click);
            // 
            // btn_reset
            // 
            this.btn_reset.Location = new System.Drawing.Point(1099, 6);
            this.btn_reset.Name = "btn_reset";
            this.btn_reset.Size = new System.Drawing.Size(75, 23);
            this.btn_reset.TabIndex = 36;
            this.btn_reset.Text = "重启";
            this.btn_reset.UseVisualStyleBackColor = true;
            this.btn_reset.Click += new System.EventHandler(this.btn_reset_Click);
            // 
            // listView3
            // 
            this.listView3.Location = new System.Drawing.Point(844, 34);
            this.listView3.Name = "listView3";
            this.listView3.Size = new System.Drawing.Size(468, 347);
            this.listView3.TabIndex = 35;
            this.listView3.UseCompatibleStateImageBehavior = false;
            // 
            // listView2
            // 
            this.listView2.Location = new System.Drawing.Point(452, 34);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(387, 347);
            this.listView2.TabIndex = 34;
            this.listView2.UseCompatibleStateImageBehavior = false;
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(178, 34);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(269, 347);
            this.listView1.TabIndex = 33;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(850, 12);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 12);
            this.label8.TabIndex = 30;
            this.label8.Text = "引擎：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(480, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 12);
            this.label7.TabIndex = 29;
            this.label7.Text = "输入：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(202, 15);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 32;
            this.label6.Text = "登录日志：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 31;
            this.label5.Text = "在线用户：";
            // 
            // btn_closeall
            // 
            this.btn_closeall.Location = new System.Drawing.Point(95, 387);
            this.btn_closeall.Name = "btn_closeall";
            this.btn_closeall.Size = new System.Drawing.Size(60, 23);
            this.btn_closeall.TabIndex = 27;
            this.btn_closeall.Text = "折叠";
            this.btn_closeall.UseVisualStyleBackColor = true;
            this.btn_closeall.Click += new System.EventHandler(this.btn_closeall_Click);
            // 
            // btn_expend
            // 
            this.btn_expend.Location = new System.Drawing.Point(11, 387);
            this.btn_expend.Name = "btn_expend";
            this.btn_expend.Size = new System.Drawing.Size(62, 23);
            this.btn_expend.TabIndex = 28;
            this.btn_expend.Text = "展开";
            this.btn_expend.UseVisualStyleBackColor = true;
            this.btn_expend.Click += new System.EventHandler(this.btn_expend_Click);
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(3, 34);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(170, 347);
            this.treeView1.TabIndex = 26;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(764, 5);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 46;
            this.button3.Text = "删除";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1315, 425);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox m_depth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listView4;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox m_Redis;
        private System.Windows.Forms.CheckBox m_CloudApi;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_clear;
        private System.Windows.Forms.Button btn_reset;
        private System.Windows.Forms.ListView listView3;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btn_closeall;
        private System.Windows.Forms.Button btn_expend;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button3;

    }
}

