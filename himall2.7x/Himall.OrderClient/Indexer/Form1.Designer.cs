namespace Indexer
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
            if (timeThread!=null)
                timeThread.Abort();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.createIndexBtn = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.show = new System.Windows.Forms.Label();
            this.isEmptyData = new System.Windows.Forms.CheckBox();
            this.threadBtn = new System.Windows.Forms.Button();
            this.msg = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menu_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // createIndexBtn
            // 
            this.createIndexBtn.Location = new System.Drawing.Point(12, 47);
            this.createIndexBtn.Name = "createIndexBtn";
            this.createIndexBtn.Size = new System.Drawing.Size(75, 23);
            this.createIndexBtn.TabIndex = 0;
            this.createIndexBtn.Text = "立即重建索引";
            this.createIndexBtn.UseVisualStyleBackColor = true;
            this.createIndexBtn.Click += new System.EventHandler(this.index_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 239);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(284, 23);
            this.progressBar1.TabIndex = 5;
            // 
            // show
            // 
            this.show.AutoSize = true;
            this.show.Location = new System.Drawing.Point(13, 221);
            this.show.Name = "show";
            this.show.Size = new System.Drawing.Size(17, 12);
            this.show.TabIndex = 6;
            this.show.Text = "[]";
            // 
            // isEmptyData
            // 
            this.isEmptyData.AutoSize = true;
            this.isEmptyData.Location = new System.Drawing.Point(12, 16);
            this.isEmptyData.Name = "isEmptyData";
            this.isEmptyData.Size = new System.Drawing.Size(144, 16);
            this.isEmptyData.TabIndex = 8;
            this.isEmptyData.Text = "是否清空现有所有索引";
            this.isEmptyData.UseVisualStyleBackColor = true;
            // 
            // threadBtn
            // 
            this.threadBtn.Location = new System.Drawing.Point(106, 47);
            this.threadBtn.Name = "threadBtn";
            this.threadBtn.Size = new System.Drawing.Size(88, 23);
            this.threadBtn.TabIndex = 9;
            this.threadBtn.Text = "启动索引线程";
            this.threadBtn.UseVisualStyleBackColor = true;
            this.threadBtn.Click += new System.EventHandler(this.thread_Click);
            // 
            // msg
            // 
            this.msg.AutoSize = true;
            this.msg.Location = new System.Drawing.Point(192, 220);
            this.msg.Name = "msg";
            this.msg.Size = new System.Drawing.Size(53, 12);
            this.msg.TabIndex = 10;
            this.msg.Text = "没有信息";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Himall搜索索引工具";
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_exit});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(101, 26);
            // 
            // menu_exit
            // 
            this.menu_exit.Name = "menu_exit";
            this.menu_exit.Size = new System.Drawing.Size(100, 22);
            this.menu_exit.Text = "退出";
            this.menu_exit.Click += new System.EventHandler(this.menu_exit_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.msg);
            this.Controls.Add(this.threadBtn);
            this.Controls.Add(this.isEmptyData);
            this.Controls.Add(this.show);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.createIndexBtn);
            this.Name = "Form1";
            this.Text = "商品搜索辅助工具";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button createIndexBtn;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label show;
        private System.Windows.Forms.CheckBox isEmptyData;
        private System.Windows.Forms.Button threadBtn;
        private System.Windows.Forms.Label msg;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ToolStripMenuItem menu_exit;
    }
}

