namespace Control_SB
{
    partial class MainOld
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btn_Start = new System.Windows.Forms.Button();
            this.pnl_BetArea = new System.Windows.Forms.Panel();
            this.grp_Result = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txt_SG_MS = new System.Windows.Forms.TextBox();
            this.chk_Football = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.grp_Today = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_JR_MS = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chk_JR_BD = new System.Windows.Forms.CheckBox();
            this.chk_JR_BCQC = new System.Windows.Forms.CheckBox();
            this.chk_JR_DRDD = new System.Windows.Forms.CheckBox();
            this.grp_Live = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_GQ_MS = new System.Windows.Forms.TextBox();
            this.grp_Football_GQ = new System.Windows.Forms.GroupBox();
            this.chk_GQ_BD = new System.Windows.Forms.CheckBox();
            this.chk_GQ_DRDD = new System.Windows.Forms.CheckBox();
            this.grp_Early = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_ZP_MS = new System.Windows.Forms.TextBox();
            this.chk_ZP_AllDay = new System.Windows.Forms.CheckBox();
            this.grp_Football_ZP = new System.Windows.Forms.GroupBox();
            this.chk_ZP_GJ = new System.Windows.Forms.CheckBox();
            this.chk_ZP_BD = new System.Windows.Forms.CheckBox();
            this.chk_ZP_BCQC = new System.Windows.Forms.CheckBox();
            this.chk_ZP_DRDD = new System.Windows.Forms.CheckBox();
            this.btn_Stop = new System.Windows.Forms.Button();
            this.chk_VisibleChrome = new System.Windows.Forms.CheckBox();
            this.btn_Init = new System.Windows.Forms.Button();
            this.chk_Logined = new System.Windows.Forms.CheckBox();
            this.rdo_TYC = new System.Windows.Forms.RadioButton();
            this.rdo_MS = new System.Windows.Forms.RadioButton();
            this.pnl_SourcePlatform = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbl_runtime = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.pnl_BetArea.SuspendLayout();
            this.grp_Result.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.grp_Today.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.grp_Live.SuspendLayout();
            this.grp_Football_GQ.SuspendLayout();
            this.grp_Early.SuspendLayout();
            this.grp_Football_ZP.SuspendLayout();
            this.pnl_SourcePlatform.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_Start
            // 
            this.btn_Start.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Start.Location = new System.Drawing.Point(9, 12);
            this.btn_Start.Name = "btn_Start";
            this.btn_Start.Size = new System.Drawing.Size(120, 42);
            this.btn_Start.TabIndex = 0;
            this.btn_Start.Text = "开始";
            this.btn_Start.UseVisualStyleBackColor = true;
            this.btn_Start.Click += new System.EventHandler(this.btn_Start_Click);
            // 
            // pnl_BetArea
            // 
            this.pnl_BetArea.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_BetArea.Controls.Add(this.grp_Result);
            this.pnl_BetArea.Controls.Add(this.grp_Today);
            this.pnl_BetArea.Controls.Add(this.grp_Live);
            this.pnl_BetArea.Controls.Add(this.grp_Early);
            this.pnl_BetArea.Location = new System.Drawing.Point(12, 66);
            this.pnl_BetArea.Name = "pnl_BetArea";
            this.pnl_BetArea.Size = new System.Drawing.Size(819, 460);
            this.pnl_BetArea.TabIndex = 3;
            // 
            // grp_Result
            // 
            this.grp_Result.Controls.Add(this.groupBox1);
            this.grp_Result.Location = new System.Drawing.Point(1, 372);
            this.grp_Result.Name = "grp_Result";
            this.grp_Result.Size = new System.Drawing.Size(818, 88);
            this.grp_Result.TabIndex = 7;
            this.grp_Result.TabStop = false;
            this.grp_Result.Text = "赛果";
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(this.txt_SG_MS);
            this.groupBox1.Controls.Add(this.chk_Football);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(818, 88);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "赛果";
            // 
            // txt_SG_MS
            // 
            this.txt_SG_MS.Location = new System.Drawing.Point(138, 24);
            this.txt_SG_MS.Name = "txt_SG_MS";
            this.txt_SG_MS.Size = new System.Drawing.Size(100, 21);
            this.txt_SG_MS.TabIndex = 7;
            this.txt_SG_MS.Text = "30";
            // 
            // chk_Football
            // 
            this.chk_Football.AutoCheck = false;
            this.chk_Football.AutoSize = true;
            this.chk_Football.Checked = true;
            this.chk_Football.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_Football.Location = new System.Drawing.Point(45, 56);
            this.chk_Football.Name = "chk_Football";
            this.chk_Football.Size = new System.Drawing.Size(48, 16);
            this.chk_Football.TabIndex = 8;
            this.chk_Football.Tag = "type-1";
            this.chk_Football.Text = "足球";
            this.chk_Football.UseVisualStyleBackColor = true;
            this.chk_Football.CheckedChanged += new System.EventHandler(this.chk_Football_CheckedChanged);
            this.chk_Football.Click += new System.EventHandler(this.chk_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(43, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "抓取间隔(秒)：";
            // 
            // grp_Today
            // 
            this.grp_Today.Controls.Add(this.label2);
            this.grp_Today.Controls.Add(this.txt_JR_MS);
            this.grp_Today.Controls.Add(this.groupBox2);
            this.grp_Today.ForeColor = System.Drawing.SystemColors.ControlText;
            this.grp_Today.Location = new System.Drawing.Point(285, 1);
            this.grp_Today.Name = "grp_Today";
            this.grp_Today.Size = new System.Drawing.Size(249, 358);
            this.grp_Today.TabIndex = 6;
            this.grp_Today.TabStop = false;
            this.grp_Today.Text = "今日赛事";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "抓取间隔(秒)：";
            // 
            // txt_JR_MS
            // 
            this.txt_JR_MS.Location = new System.Drawing.Point(125, 20);
            this.txt_JR_MS.Name = "txt_JR_MS";
            this.txt_JR_MS.Size = new System.Drawing.Size(100, 21);
            this.txt_JR_MS.TabIndex = 7;
            this.txt_JR_MS.Text = "10";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chk_JR_BD);
            this.groupBox2.Controls.Add(this.chk_JR_BCQC);
            this.groupBox2.Controls.Add(this.chk_JR_DRDD);
            this.groupBox2.Location = new System.Drawing.Point(6, 43);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(238, 111);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "足球";
            // 
            // chk_JR_BD
            // 
            this.chk_JR_BD.AutoCheck = false;
            this.chk_JR_BD.AutoSize = true;
            this.chk_JR_BD.Checked = true;
            this.chk_JR_BD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_JR_BD.Location = new System.Drawing.Point(39, 82);
            this.chk_JR_BD.Name = "chk_JR_BD";
            this.chk_JR_BD.Size = new System.Drawing.Size(48, 16);
            this.chk_JR_BD.TabIndex = 2;
            this.chk_JR_BD.Tag = "2-1-3";
            this.chk_JR_BD.Text = "波胆";
            this.chk_JR_BD.UseVisualStyleBackColor = true;
            this.chk_JR_BD.Click += new System.EventHandler(this.chk_Click);
            // 
            // chk_JR_BCQC
            // 
            this.chk_JR_BCQC.AutoCheck = false;
            this.chk_JR_BCQC.AutoSize = true;
            this.chk_JR_BCQC.Checked = true;
            this.chk_JR_BCQC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_JR_BCQC.Location = new System.Drawing.Point(39, 52);
            this.chk_JR_BCQC.Name = "chk_JR_BCQC";
            this.chk_JR_BCQC.Size = new System.Drawing.Size(90, 16);
            this.chk_JR_BCQC.TabIndex = 1;
            this.chk_JR_BCQC.Tag = "2-1-2";
            this.chk_JR_BCQC.Text = "半场 / 全场";
            this.chk_JR_BCQC.UseVisualStyleBackColor = true;
            this.chk_JR_BCQC.Click += new System.EventHandler(this.chk_Click);
            // 
            // chk_JR_DRDD
            // 
            this.chk_JR_DRDD.AutoCheck = false;
            this.chk_JR_DRDD.AutoSize = true;
            this.chk_JR_DRDD.Checked = true;
            this.chk_JR_DRDD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_JR_DRDD.ImageKey = "(无)";
            this.chk_JR_DRDD.Location = new System.Drawing.Point(39, 22);
            this.chk_JR_DRDD.Name = "chk_JR_DRDD";
            this.chk_JR_DRDD.Size = new System.Drawing.Size(180, 16);
            this.chk_JR_DRDD.TabIndex = 0;
            this.chk_JR_DRDD.Tag = "2-1-1";
            this.chk_JR_DRDD.Text = "独赢 && 让球 && 大小 && 单/双";
            this.chk_JR_DRDD.UseVisualStyleBackColor = true;
            this.chk_JR_DRDD.Click += new System.EventHandler(this.chk_Click);
            // 
            // grp_Live
            // 
            this.grp_Live.Controls.Add(this.label3);
            this.grp_Live.Controls.Add(this.txt_GQ_MS);
            this.grp_Live.Controls.Add(this.grp_Football_GQ);
            this.grp_Live.Location = new System.Drawing.Point(570, 3);
            this.grp_Live.Name = "grp_Live";
            this.grp_Live.Size = new System.Drawing.Size(248, 358);
            this.grp_Live.TabIndex = 5;
            this.grp_Live.TabStop = false;
            this.grp_Live.Text = "滚球";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "抓取间隔(秒)：";
            // 
            // txt_GQ_MS
            // 
            this.txt_GQ_MS.Location = new System.Drawing.Point(124, 18);
            this.txt_GQ_MS.Name = "txt_GQ_MS";
            this.txt_GQ_MS.Size = new System.Drawing.Size(100, 21);
            this.txt_GQ_MS.TabIndex = 7;
            this.txt_GQ_MS.Text = "3";
            // 
            // grp_Football_GQ
            // 
            this.grp_Football_GQ.Controls.Add(this.chk_GQ_BD);
            this.grp_Football_GQ.Controls.Add(this.chk_GQ_DRDD);
            this.grp_Football_GQ.Location = new System.Drawing.Point(6, 42);
            this.grp_Football_GQ.Name = "grp_Football_GQ";
            this.grp_Football_GQ.Size = new System.Drawing.Size(238, 80);
            this.grp_Football_GQ.TabIndex = 4;
            this.grp_Football_GQ.TabStop = false;
            this.grp_Football_GQ.Text = "足球";
            // 
            // chk_GQ_BD
            // 
            this.chk_GQ_BD.AutoCheck = false;
            this.chk_GQ_BD.AutoSize = true;
            this.chk_GQ_BD.Checked = true;
            this.chk_GQ_BD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_GQ_BD.Location = new System.Drawing.Point(39, 52);
            this.chk_GQ_BD.Name = "chk_GQ_BD";
            this.chk_GQ_BD.Size = new System.Drawing.Size(48, 16);
            this.chk_GQ_BD.TabIndex = 2;
            this.chk_GQ_BD.Tag = "3-1-3";
            this.chk_GQ_BD.Text = "波胆";
            this.chk_GQ_BD.UseVisualStyleBackColor = true;
            this.chk_GQ_BD.Click += new System.EventHandler(this.chk_Click);
            // 
            // chk_GQ_DRDD
            // 
            this.chk_GQ_DRDD.AutoCheck = false;
            this.chk_GQ_DRDD.AutoSize = true;
            this.chk_GQ_DRDD.Checked = true;
            this.chk_GQ_DRDD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_GQ_DRDD.Location = new System.Drawing.Point(39, 22);
            this.chk_GQ_DRDD.Name = "chk_GQ_DRDD";
            this.chk_GQ_DRDD.Size = new System.Drawing.Size(180, 16);
            this.chk_GQ_DRDD.TabIndex = 0;
            this.chk_GQ_DRDD.Tag = "3-1-1";
            this.chk_GQ_DRDD.Text = "独赢 && 让球 && 大小 && 单/双";
            this.chk_GQ_DRDD.UseVisualStyleBackColor = true;
            this.chk_GQ_DRDD.Click += new System.EventHandler(this.chk_Click);
            // 
            // grp_Early
            // 
            this.grp_Early.Controls.Add(this.label1);
            this.grp_Early.Controls.Add(this.txt_ZP_MS);
            this.grp_Early.Controls.Add(this.chk_ZP_AllDay);
            this.grp_Early.Controls.Add(this.grp_Football_ZP);
            this.grp_Early.ForeColor = System.Drawing.SystemColors.ControlText;
            this.grp_Early.Location = new System.Drawing.Point(0, 3);
            this.grp_Early.Name = "grp_Early";
            this.grp_Early.Size = new System.Drawing.Size(249, 358);
            this.grp_Early.TabIndex = 4;
            this.grp_Early.TabStop = false;
            this.grp_Early.Text = "早盘";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "抓取间隔(秒)：";
            // 
            // txt_ZP_MS
            // 
            this.txt_ZP_MS.Location = new System.Drawing.Point(125, 18);
            this.txt_ZP_MS.Name = "txt_ZP_MS";
            this.txt_ZP_MS.Size = new System.Drawing.Size(100, 21);
            this.txt_ZP_MS.TabIndex = 5;
            this.txt_ZP_MS.Text = "30";
            // 
            // chk_ZP_AllDay
            // 
            this.chk_ZP_AllDay.AutoCheck = false;
            this.chk_ZP_AllDay.AutoSize = true;
            this.chk_ZP_AllDay.Location = new System.Drawing.Point(33, 46);
            this.chk_ZP_AllDay.Name = "chk_ZP_AllDay";
            this.chk_ZP_AllDay.Size = new System.Drawing.Size(72, 16);
            this.chk_ZP_AllDay.TabIndex = 4;
            this.chk_ZP_AllDay.Tag = "";
            this.chk_ZP_AllDay.Text = "所有日期";
            this.chk_ZP_AllDay.UseVisualStyleBackColor = true;
            this.chk_ZP_AllDay.Click += new System.EventHandler(this.chk_Click);
            // 
            // grp_Football_ZP
            // 
            this.grp_Football_ZP.Controls.Add(this.chk_ZP_GJ);
            this.grp_Football_ZP.Controls.Add(this.chk_ZP_BD);
            this.grp_Football_ZP.Controls.Add(this.chk_ZP_BCQC);
            this.grp_Football_ZP.Controls.Add(this.chk_ZP_DRDD);
            this.grp_Football_ZP.Location = new System.Drawing.Point(6, 66);
            this.grp_Football_ZP.Name = "grp_Football_ZP";
            this.grp_Football_ZP.Size = new System.Drawing.Size(238, 142);
            this.grp_Football_ZP.TabIndex = 3;
            this.grp_Football_ZP.TabStop = false;
            this.grp_Football_ZP.Text = "足球";
            // 
            // chk_ZP_GJ
            // 
            this.chk_ZP_GJ.AutoCheck = false;
            this.chk_ZP_GJ.AutoSize = true;
            this.chk_ZP_GJ.Checked = true;
            this.chk_ZP_GJ.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_ZP_GJ.Location = new System.Drawing.Point(39, 112);
            this.chk_ZP_GJ.Name = "chk_ZP_GJ";
            this.chk_ZP_GJ.Size = new System.Drawing.Size(48, 16);
            this.chk_ZP_GJ.TabIndex = 3;
            this.chk_ZP_GJ.Tag = "1-1-4";
            this.chk_ZP_GJ.Text = "冠军";
            this.chk_ZP_GJ.UseVisualStyleBackColor = true;
            this.chk_ZP_GJ.Click += new System.EventHandler(this.chk_Click);
            // 
            // chk_ZP_BD
            // 
            this.chk_ZP_BD.AutoCheck = false;
            this.chk_ZP_BD.AutoSize = true;
            this.chk_ZP_BD.Checked = true;
            this.chk_ZP_BD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_ZP_BD.Location = new System.Drawing.Point(39, 82);
            this.chk_ZP_BD.Name = "chk_ZP_BD";
            this.chk_ZP_BD.Size = new System.Drawing.Size(48, 16);
            this.chk_ZP_BD.TabIndex = 2;
            this.chk_ZP_BD.Tag = "1-1-3";
            this.chk_ZP_BD.Text = "波胆";
            this.chk_ZP_BD.UseVisualStyleBackColor = true;
            this.chk_ZP_BD.Click += new System.EventHandler(this.chk_Click);
            // 
            // chk_ZP_BCQC
            // 
            this.chk_ZP_BCQC.AutoCheck = false;
            this.chk_ZP_BCQC.AutoSize = true;
            this.chk_ZP_BCQC.Checked = true;
            this.chk_ZP_BCQC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_ZP_BCQC.Location = new System.Drawing.Point(39, 52);
            this.chk_ZP_BCQC.Name = "chk_ZP_BCQC";
            this.chk_ZP_BCQC.Size = new System.Drawing.Size(90, 16);
            this.chk_ZP_BCQC.TabIndex = 1;
            this.chk_ZP_BCQC.Tag = "1-1-2";
            this.chk_ZP_BCQC.Text = "半场 / 全场";
            this.chk_ZP_BCQC.UseVisualStyleBackColor = true;
            this.chk_ZP_BCQC.Click += new System.EventHandler(this.chk_Click);
            // 
            // chk_ZP_DRDD
            // 
            this.chk_ZP_DRDD.AutoCheck = false;
            this.chk_ZP_DRDD.AutoSize = true;
            this.chk_ZP_DRDD.Checked = true;
            this.chk_ZP_DRDD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_ZP_DRDD.ImageKey = "(无)";
            this.chk_ZP_DRDD.Location = new System.Drawing.Point(39, 22);
            this.chk_ZP_DRDD.Name = "chk_ZP_DRDD";
            this.chk_ZP_DRDD.Size = new System.Drawing.Size(180, 16);
            this.chk_ZP_DRDD.TabIndex = 0;
            this.chk_ZP_DRDD.Tag = "1-1-1";
            this.chk_ZP_DRDD.Text = "独赢 && 让球 && 大小 && 单/双";
            this.chk_ZP_DRDD.UseVisualStyleBackColor = true;
            this.chk_ZP_DRDD.Click += new System.EventHandler(this.chk_Click);
            // 
            // btn_Stop
            // 
            this.btn_Stop.Enabled = false;
            this.btn_Stop.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Stop.Location = new System.Drawing.Point(141, 12);
            this.btn_Stop.Name = "btn_Stop";
            this.btn_Stop.Size = new System.Drawing.Size(120, 42);
            this.btn_Stop.TabIndex = 4;
            this.btn_Stop.Text = "停止";
            this.btn_Stop.UseVisualStyleBackColor = true;
            this.btn_Stop.Click += new System.EventHandler(this.btn_Stop_Click);
            // 
            // chk_VisibleChrome
            // 
            this.chk_VisibleChrome.AutoCheck = false;
            this.chk_VisibleChrome.AutoSize = true;
            this.chk_VisibleChrome.Location = new System.Drawing.Point(747, 39);
            this.chk_VisibleChrome.Name = "chk_VisibleChrome";
            this.chk_VisibleChrome.Size = new System.Drawing.Size(84, 16);
            this.chk_VisibleChrome.TabIndex = 5;
            this.chk_VisibleChrome.Tag = "";
            this.chk_VisibleChrome.Text = "显示浏览器";
            this.chk_VisibleChrome.UseVisualStyleBackColor = true;
            this.chk_VisibleChrome.Click += new System.EventHandler(this.chk_Click);
            // 
            // btn_Init
            // 
            this.btn_Init.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Init.Location = new System.Drawing.Point(329, 12);
            this.btn_Init.Name = "btn_Init";
            this.btn_Init.Size = new System.Drawing.Size(120, 42);
            this.btn_Init.TabIndex = 6;
            this.btn_Init.Text = "初始化";
            this.btn_Init.UseVisualStyleBackColor = true;
            this.btn_Init.Click += new System.EventHandler(this.btn_Init_Click);
            // 
            // chk_Logined
            // 
            this.chk_Logined.AutoCheck = false;
            this.chk_Logined.AutoSize = true;
            this.chk_Logined.Checked = true;
            this.chk_Logined.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_Logined.Location = new System.Drawing.Point(747, 12);
            this.chk_Logined.Name = "chk_Logined";
            this.chk_Logined.Size = new System.Drawing.Size(48, 16);
            this.chk_Logined.TabIndex = 7;
            this.chk_Logined.Tag = "";
            this.chk_Logined.Text = "登录";
            this.chk_Logined.UseVisualStyleBackColor = true;
            this.chk_Logined.Click += new System.EventHandler(this.chk_Click);
            // 
            // rdo_TYC
            // 
            this.rdo_TYC.AutoSize = true;
            this.rdo_TYC.Checked = true;
            this.rdo_TYC.Location = new System.Drawing.Point(12, 6);
            this.rdo_TYC.Name = "rdo_TYC";
            this.rdo_TYC.Size = new System.Drawing.Size(59, 16);
            this.rdo_TYC.TabIndex = 8;
            this.rdo_TYC.TabStop = true;
            this.rdo_TYC.Tag = "TYC";
            this.rdo_TYC.Text = "太阳城";
            this.rdo_TYC.UseVisualStyleBackColor = true;
            // 
            // rdo_MS
            // 
            this.rdo_MS.AutoSize = true;
            this.rdo_MS.Location = new System.Drawing.Point(12, 32);
            this.rdo_MS.Name = "rdo_MS";
            this.rdo_MS.Size = new System.Drawing.Size(47, 16);
            this.rdo_MS.TabIndex = 9;
            this.rdo_MS.Tag = "MS";
            this.rdo_MS.Text = "明升";
            this.rdo_MS.UseVisualStyleBackColor = true;
            // 
            // pnl_SourcePlatform
            // 
            this.pnl_SourcePlatform.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_SourcePlatform.Controls.Add(this.rdo_TYC);
            this.pnl_SourcePlatform.Controls.Add(this.rdo_MS);
            this.pnl_SourcePlatform.Location = new System.Drawing.Point(537, 5);
            this.pnl_SourcePlatform.Name = "pnl_SourcePlatform";
            this.pnl_SourcePlatform.Size = new System.Drawing.Size(188, 58);
            this.pnl_SourcePlatform.TabIndex = 10;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbl_runtime);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Location = new System.Drawing.Point(297, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(225, 58);
            this.panel1.TabIndex = 11;
            // 
            // lbl_runtime
            // 
            this.lbl_runtime.AutoSize = true;
            this.lbl_runtime.Location = new System.Drawing.Point(89, 23);
            this.lbl_runtime.Name = "lbl_runtime";
            this.lbl_runtime.Size = new System.Drawing.Size(11, 12);
            this.lbl_runtime.TabIndex = 1;
            this.lbl_runtime.Text = "-";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(30, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "已运行：";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(843, 538);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnl_SourcePlatform);
            this.Controls.Add(this.chk_Logined);
            this.Controls.Add(this.btn_Init);
            this.Controls.Add(this.chk_VisibleChrome);
            this.Controls.Add(this.btn_Stop);
            this.Controls.Add(this.pnl_BetArea);
            this.Controls.Add(this.btn_Start);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "数据抓取-沙巴";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.pnl_BetArea.ResumeLayout(false);
            this.grp_Result.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grp_Today.ResumeLayout(false);
            this.grp_Today.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.grp_Live.ResumeLayout(false);
            this.grp_Live.PerformLayout();
            this.grp_Football_GQ.ResumeLayout(false);
            this.grp_Football_GQ.PerformLayout();
            this.grp_Early.ResumeLayout(false);
            this.grp_Early.PerformLayout();
            this.grp_Football_ZP.ResumeLayout(false);
            this.grp_Football_ZP.PerformLayout();
            this.pnl_SourcePlatform.ResumeLayout(false);
            this.pnl_SourcePlatform.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Start;
        private System.Windows.Forms.Panel pnl_BetArea;
        private System.Windows.Forms.GroupBox grp_Football_ZP;
        private System.Windows.Forms.CheckBox chk_ZP_GJ;
        private System.Windows.Forms.CheckBox chk_ZP_BD;
        private System.Windows.Forms.CheckBox chk_ZP_BCQC;
        private System.Windows.Forms.CheckBox chk_ZP_DRDD;
        private System.Windows.Forms.GroupBox grp_Live;
        private System.Windows.Forms.GroupBox grp_Football_GQ;
        private System.Windows.Forms.CheckBox chk_GQ_BD;
        private System.Windows.Forms.CheckBox chk_GQ_DRDD;
        private System.Windows.Forms.GroupBox grp_Early;
        private System.Windows.Forms.Button btn_Stop;
        private System.Windows.Forms.GroupBox grp_Today;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chk_JR_BD;
        private System.Windows.Forms.CheckBox chk_JR_BCQC;
        private System.Windows.Forms.CheckBox chk_JR_DRDD;
        private System.Windows.Forms.CheckBox chk_ZP_AllDay;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_JR_MS;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txt_GQ_MS;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_ZP_MS;
        private System.Windows.Forms.CheckBox chk_VisibleChrome;
        private System.Windows.Forms.Button btn_Init;
        private System.Windows.Forms.CheckBox chk_Logined;
        private System.Windows.Forms.RadioButton rdo_TYC;
        private System.Windows.Forms.RadioButton rdo_MS;
        private System.Windows.Forms.Panel pnl_SourcePlatform;
        private System.Windows.Forms.GroupBox grp_Result;
        private System.Windows.Forms.CheckBox chk_Football;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txt_SG_MS;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lbl_runtime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
    }
}

