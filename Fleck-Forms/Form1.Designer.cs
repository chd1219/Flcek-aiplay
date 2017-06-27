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
            this.m_connectionList = new System.Windows.Forms.ListBox();
            this.btn_expend = new System.Windows.Forms.Button();
            this.btn_closeall = new System.Windows.Forms.Button();
            this.m_msgList = new System.Windows.Forms.ListBox();
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
            this.treeView1.Location = new System.Drawing.Point(10, 36);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(170, 352);
            this.treeView1.TabIndex = 2;
            // 
            // m_connectionList
            // 
            this.m_connectionList.FormattingEnabled = true;
            this.m_connectionList.ItemHeight = 12;
            this.m_connectionList.Location = new System.Drawing.Point(202, 36);
            this.m_connectionList.Name = "m_connectionList";
            this.m_connectionList.Size = new System.Drawing.Size(253, 352);
            this.m_connectionList.TabIndex = 1;
            // 
            // btn_expend
            // 
            this.btn_expend.Location = new System.Drawing.Point(18, 414);
            this.btn_expend.Name = "btn_expend";
            this.btn_expend.Size = new System.Drawing.Size(62, 23);
            this.btn_expend.TabIndex = 3;
            this.btn_expend.Text = "展开";
            this.btn_expend.UseVisualStyleBackColor = true;
            this.btn_expend.Click += new System.EventHandler(this.btn_expend_Click);
            // 
            // btn_closeall
            // 
            this.btn_closeall.Location = new System.Drawing.Point(106, 414);
            this.btn_closeall.Name = "btn_closeall";
            this.btn_closeall.Size = new System.Drawing.Size(60, 23);
            this.btn_closeall.TabIndex = 3;
            this.btn_closeall.Text = "折叠";
            this.btn_closeall.UseVisualStyleBackColor = true;
            this.btn_closeall.Click += new System.EventHandler(this.btn_closeall_Click);
            // 
            // m_msgList
            // 
            this.m_msgList.FormattingEnabled = true;
            this.m_msgList.ItemHeight = 12;
            this.m_msgList.Location = new System.Drawing.Point(477, 34);
            this.m_msgList.Name = "m_msgList";
            this.m_msgList.Size = new System.Drawing.Size(89, 352);
            this.m_msgList.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(200, 420);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "在线用户：";
            // 
            // m_online
            // 
            this.m_online.AutoSize = true;
            this.m_online.Location = new System.Drawing.Point(272, 420);
            this.m_online.Name = "m_online";
            this.m_online.Size = new System.Drawing.Size(41, 12);
            this.m_online.TabIndex = 5;
            this.m_online.Text = "label2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(813, 420);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "启动时间：";
            // 
            // m_time
            // 
            this.m_time.AutoSize = true;
            this.m_time.Location = new System.Drawing.Point(873, 420);
            this.m_time.Name = "m_time";
            this.m_time.Size = new System.Drawing.Size(41, 12);
            this.m_time.TabIndex = 6;
            this.m_time.Text = "label2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(988, 420);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "运行时间：";
            // 
            // m_span
            // 
            this.m_span.AutoSize = true;
            this.m_span.Location = new System.Drawing.Point(1053, 420);
            this.m_span.Name = "m_span";
            this.m_span.Size = new System.Drawing.Size(41, 12);
            this.m_span.TabIndex = 6;
            this.m_span.Text = "label2";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(478, 420);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "接收数据：";
            // 
            // m_msg
            // 
            this.m_msg.AutoSize = true;
            this.m_msg.Location = new System.Drawing.Point(540, 420);
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
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 7;
            this.label7.Text = "消息队列：";
            // 
            // m_speed
            // 
            this.m_speed.AutoSize = true;
            this.m_speed.Location = new System.Drawing.Point(686, 420);
            this.m_speed.Name = "m_speed";
            this.m_speed.Size = new System.Drawing.Size(41, 12);
            this.m_speed.TabIndex = 9;
            this.m_speed.Text = "label2";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(625, 420);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 12);
            this.label9.TabIndex = 8;
            this.label9.Text = "处理速度：";
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(572, 36);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(562, 352);
            this.listView1.TabIndex = 10;
            this.listView1.UseCompatibleStateImageBehavior = false;
            
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1146, 461);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.m_speed);
            this.Controls.Add(this.label9);
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
            this.Controls.Add(this.m_msgList);
            this.Controls.Add(this.m_connectionList);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ListBox m_connectionList;
        private System.Windows.Forms.Button btn_expend;
        private System.Windows.Forms.Button btn_closeall;
        private System.Windows.Forms.ListBox m_msgList;
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
    }
}

