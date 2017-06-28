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
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.btn_expend = new System.Windows.Forms.Button();
            this.btn_closeall = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.m_online = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.m_time = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.m_span = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.m_msg = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.m_speed = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.listView2 = new System.Windows.Forms.ListView();
            this.m_undo = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.listView3 = new System.Windows.Forms.ListView();
            this.label8 = new System.Windows.Forms.Label();
            this.btn_reset = new System.Windows.Forms.Button();
            this.btn_clear = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(1, 36);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(170, 498);
            this.treeView1.TabIndex = 2;
            // 
            // btn_expend
            // 
            this.btn_expend.Location = new System.Drawing.Point(3, 550);
            this.btn_expend.Name = "btn_expend";
            this.btn_expend.Size = new System.Drawing.Size(62, 23);
            this.btn_expend.TabIndex = 3;
            this.btn_expend.Text = "展开";
            this.btn_expend.UseVisualStyleBackColor = true;
            this.btn_expend.Click += new System.EventHandler(this.btn_expend_Click);
            // 
            // btn_closeall
            // 
            this.btn_closeall.Location = new System.Drawing.Point(87, 550);
            this.btn_closeall.Name = "btn_closeall";
            this.btn_closeall.Size = new System.Drawing.Size(60, 23);
            this.btn_closeall.TabIndex = 3;
            this.btn_closeall.Text = "折叠";
            this.btn_closeall.UseVisualStyleBackColor = true;
            this.btn_closeall.Click += new System.EventHandler(this.btn_closeall_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(389, 555);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "在线用户：";
            // 
            // m_online
            // 
            this.m_online.AutoSize = true;
            this.m_online.Location = new System.Drawing.Point(458, 555);
            this.m_online.Name = "m_online";
            this.m_online.Size = new System.Drawing.Size(41, 12);
            this.m_online.TabIndex = 5;
            this.m_online.Text = "label2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(975, 555);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "启动时间：";
            // 
            // m_time
            // 
            this.m_time.AutoSize = true;
            this.m_time.Location = new System.Drawing.Point(1044, 555);
            this.m_time.Name = "m_time";
            this.m_time.Size = new System.Drawing.Size(41, 12);
            this.m_time.TabIndex = 6;
            this.m_time.Text = "label2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1167, 555);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "运行时间：";
            // 
            // m_span
            // 
            this.m_span.AutoSize = true;
            this.m_span.Location = new System.Drawing.Point(1232, 555);
            this.m_span.Name = "m_span";
            this.m_span.Size = new System.Drawing.Size(41, 12);
            this.m_span.TabIndex = 6;
            this.m_span.Text = "label2";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(542, 555);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "接收请求：";
            // 
            // m_msg
            // 
            this.m_msg.AutoSize = true;
            this.m_msg.Location = new System.Drawing.Point(610, 555);
            this.m_msg.Name = "m_msg";
            this.m_msg.Size = new System.Drawing.Size(41, 12);
            this.m_msg.TabIndex = 5;
            this.m_msg.Text = "label2";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 7;
            this.label5.Text = "在线用户：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(200, 17);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 7;
            this.label6.Text = "登录日志：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(478, 17);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 12);
            this.label7.TabIndex = 7;
            this.label7.Text = "输入：";
            // 
            // m_speed
            // 
            this.m_speed.AutoSize = true;
            this.m_speed.Location = new System.Drawing.Point(746, 555);
            this.m_speed.Name = "m_speed";
            this.m_speed.Size = new System.Drawing.Size(41, 12);
            this.m_speed.TabIndex = 9;
            this.m_speed.Text = "label2";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(679, 555);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 12);
            this.label9.TabIndex = 8;
            this.label9.Text = "处理速度：";
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(182, 36);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(269, 498);
            this.listView1.TabIndex = 10;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // listView2
            // 
            this.listView2.Location = new System.Drawing.Point(462, 36);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(387, 498);
            this.listView2.TabIndex = 11;
            this.listView2.UseCompatibleStateImageBehavior = false;
            // 
            // m_undo
            // 
            this.m_undo.AutoSize = true;
            this.m_undo.Location = new System.Drawing.Point(885, 555);
            this.m_undo.Name = "m_undo";
            this.m_undo.Size = new System.Drawing.Size(41, 12);
            this.m_undo.TabIndex = 13;
            this.m_undo.Text = "label2";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(833, 555);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(53, 12);
            this.label10.TabIndex = 12;
            this.label10.Text = "未处理：";
            // 
            // listView3
            // 
            this.listView3.Location = new System.Drawing.Point(856, 36);
            this.listView3.Name = "listView3";
            this.listView3.Size = new System.Drawing.Size(468, 498);
            this.listView3.TabIndex = 14;
            this.listView3.UseCompatibleStateImageBehavior = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(883, 17);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 7;
            this.label8.Text = "引擎输出：";
            // 
            // btn_reset
            // 
            this.btn_reset.Location = new System.Drawing.Point(954, 9);
            this.btn_reset.Name = "btn_reset";
            this.btn_reset.Size = new System.Drawing.Size(75, 23);
            this.btn_reset.TabIndex = 15;
            this.btn_reset.Text = "重启";
            this.btn_reset.UseVisualStyleBackColor = true;
            this.btn_reset.Click += new System.EventHandler(this.btn_reset_Click);
            // 
            // btn_clear
            // 
            this.btn_clear.Location = new System.Drawing.Point(169, 550);
            this.btn_clear.Name = "btn_clear";
            this.btn_clear.Size = new System.Drawing.Size(75, 23);
            this.btn_clear.TabIndex = 16;
            this.btn_clear.Text = "清空消息";
            this.btn_clear.UseVisualStyleBackColor = true;
            this.btn_clear.Click += new System.EventHandler(this.btn_clear_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1079, 9);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 17;
            this.button1.Text = "停止";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(544, 14);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(72, 16);
            this.checkBox1.TabIndex = 18;
            this.checkBox1.Text = "JSON格式";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(269, 550);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 19;
            this.button2.Text = "testSQL";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1328, 580);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btn_clear);
            this.Controls.Add(this.btn_reset);
            this.Controls.Add(this.listView3);
            this.Controls.Add(this.m_undo);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.listView2);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.m_speed);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.m_span);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.m_time);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.m_msg);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.m_online);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_closeall);
            this.Controls.Add(this.btn_expend);
            this.Controls.Add(this.treeView1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button btn_expend;
        private System.Windows.Forms.Button btn_closeall;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label m_online;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label m_time;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label m_span;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label m_msg;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label m_speed;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.Label m_undo;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ListView listView3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btn_reset;
        private System.Windows.Forms.Button btn_clear;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button2;
    }
}

