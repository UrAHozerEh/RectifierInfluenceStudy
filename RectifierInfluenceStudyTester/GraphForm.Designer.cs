namespace RectifierInfluenceStudyTester
{
    partial class GraphForm
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.ListFiles = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TextOffset = new System.Windows.Forms.TextBox();
            this.CheckApproved = new System.Windows.Forms.CheckBox();
            this.ButtonExportOne = new System.Windows.Forms.Button();
            this.ButtonExportApproved = new System.Windows.Forms.Button();
            this.GuessButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Chart)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.splitContainer1);
            this.groupBox1.Location = new System.Drawing.Point(1, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(737, 323);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 16);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.Chart);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ListFiles);
            this.splitContainer1.Size = new System.Drawing.Size(731, 304);
            this.splitContainer1.SplitterDistance = 591;
            this.splitContainer1.TabIndex = 0;
            // 
            // Chart
            // 
            this.Chart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.Chart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.Chart.Legends.Add(legend1);
            this.Chart.Location = new System.Drawing.Point(3, 3);
            this.Chart.Name = "Chart";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.Chart.Series.Add(series1);
            this.Chart.Size = new System.Drawing.Size(585, 304);
            this.Chart.TabIndex = 0;
            this.Chart.Text = "chart1";
            // 
            // ListFiles
            // 
            this.ListFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ListFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.ListFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ListFiles.HideSelection = false;
            this.ListFiles.Location = new System.Drawing.Point(3, 0);
            this.ListFiles.MultiSelect = false;
            this.ListFiles.Name = "ListFiles";
            this.ListFiles.Size = new System.Drawing.Size(133, 301);
            this.ListFiles.TabIndex = 3;
            this.ListFiles.UseCompatibleStateImageBehavior = false;
            this.ListFiles.View = System.Windows.Forms.View.Details;
            this.ListFiles.SelectedIndexChanged += new System.EventHandler(this.ListFiles_SelectedIndexChanged_1);
            // 
            // columnHeader2
            // 
            this.columnHeader2.Width = 999;
            // 
            // TextOffset
            // 
            this.TextOffset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TextOffset.Location = new System.Drawing.Point(7, 370);
            this.TextOffset.Name = "TextOffset";
            this.TextOffset.Size = new System.Drawing.Size(100, 20);
            this.TextOffset.TabIndex = 1;
            this.TextOffset.TextChanged += new System.EventHandler(this.TextOffset_TextChanged);
            // 
            // CheckApproved
            // 
            this.CheckApproved.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CheckApproved.AutoSize = true;
            this.CheckApproved.Location = new System.Drawing.Point(7, 329);
            this.CheckApproved.Name = "CheckApproved";
            this.CheckApproved.Size = new System.Drawing.Size(72, 17);
            this.CheckApproved.TabIndex = 2;
            this.CheckApproved.Text = "Approved";
            this.CheckApproved.UseVisualStyleBackColor = true;
            this.CheckApproved.CheckedChanged += new System.EventHandler(this.CheckApproved_CheckedChanged);
            // 
            // ButtonExportOne
            // 
            this.ButtonExportOne.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonExportOne.Location = new System.Drawing.Point(624, 367);
            this.ButtonExportOne.Name = "ButtonExportOne";
            this.ButtonExportOne.Size = new System.Drawing.Size(102, 23);
            this.ButtonExportOne.TabIndex = 3;
            this.ButtonExportOne.Text = "Export";
            this.ButtonExportOne.UseVisualStyleBackColor = true;
            this.ButtonExportOne.Click += new System.EventHandler(this.ButtonExportOne_Click);
            // 
            // ButtonExportApproved
            // 
            this.ButtonExportApproved.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonExportApproved.Location = new System.Drawing.Point(624, 338);
            this.ButtonExportApproved.Name = "ButtonExportApproved";
            this.ButtonExportApproved.Size = new System.Drawing.Size(102, 23);
            this.ButtonExportApproved.TabIndex = 4;
            this.ButtonExportApproved.Text = "Export Approved";
            this.ButtonExportApproved.UseVisualStyleBackColor = true;
            this.ButtonExportApproved.Click += new System.EventHandler(this.ButtonExportApproved_Click);
            // 
            // GuessButton
            // 
            this.GuessButton.Location = new System.Drawing.Point(113, 368);
            this.GuessButton.Name = "GuessButton";
            this.GuessButton.Size = new System.Drawing.Size(75, 23);
            this.GuessButton.TabIndex = 5;
            this.GuessButton.Text = "Guess";
            this.GuessButton.UseVisualStyleBackColor = true;
            this.GuessButton.Click += new System.EventHandler(this.ButtonGuess_Click);
            // 
            // GraphForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(738, 402);
            this.Controls.Add(this.GuessButton);
            this.Controls.Add(this.ButtonExportApproved);
            this.Controls.Add(this.ButtonExportOne);
            this.Controls.Add(this.CheckApproved);
            this.Controls.Add(this.TextOffset);
            this.Controls.Add(this.groupBox1);
            this.KeyPreview = true;
            this.Name = "GraphForm";
            this.Text = "GaphForm";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.GraphForm_KeyPress);
            this.groupBox1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Chart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataVisualization.Charting.Chart Chart;
        private System.Windows.Forms.TextBox TextOffset;
        private System.Windows.Forms.CheckBox CheckApproved;
        private System.Windows.Forms.ListView ListFiles;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button ButtonExportOne;
        private System.Windows.Forms.Button ButtonExportApproved;
        private System.Windows.Forms.Button GuessButton;
    }
}