namespace MovieGuide
{
    partial class SortDialog
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_grid = new System.Windows.Forms.DataGridView();
            this.m_btnUp = new System.Windows.Forms.Button();
            this.m_btnDown = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.m_grid)).BeginInit();
            this.SuspendLayout();
            // 
            // m_btnOK
            // 
            this.m_btnOK.AccessibleDescription = "Save changes and sort.";
            this.m_btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.m_btnOK.Location = new System.Drawing.Point(18, 224);
            this.m_btnOK.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(73, 36);
            this.m_btnOK.TabIndex = 2;
            this.m_btnOK.Text = "OK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.AccessibleDescription = "Cancel changes.";
            this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.m_btnCancel.Location = new System.Drawing.Point(205, 224);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(73, 36);
            this.m_btnCancel.TabIndex = 3;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
            // 
            // m_grid
            // 
            this.m_grid.AllowDrop = true;
            this.m_grid.AllowUserToAddRows = false;
            this.m_grid.AllowUserToDeleteRows = false;
            this.m_grid.AllowUserToResizeRows = false;
            this.m_grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.m_grid.BackgroundColor = System.Drawing.Color.AliceBlue;
            this.m_grid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.LightSteelBlue;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.SkyBlue;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.m_grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.m_grid.ColumnHeadersHeight = 26;
            this.m_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.AliceBlue;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.SkyBlue;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.m_grid.DefaultCellStyle = dataGridViewCellStyle2;
            this.m_grid.GridColor = System.Drawing.Color.LightSteelBlue;
            this.m_grid.Location = new System.Drawing.Point(18, 19);
            this.m_grid.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_grid.Name = "m_grid";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.LightSteelBlue;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.SkyBlue;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.m_grid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.m_grid.RowHeadersVisible = false;
            this.m_grid.RowTemplate.Height = 26;
            this.m_grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.m_grid.Size = new System.Drawing.Size(259, 184);
            this.m_grid.TabIndex = 4;
            this.m_grid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_grid_CellClick);
            this.m_grid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.m_grid_DataError);
            this.m_grid.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_grid_DragDrop);
            this.m_grid.DragEnter += new System.Windows.Forms.DragEventHandler(this.m_grid_DragEnter);
            this.m_grid.DragOver += new System.Windows.Forms.DragEventHandler(this.m_grid_DragOver);
            this.m_grid.Leave += new System.EventHandler(this.m_grid_Leave);
            this.m_grid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_grid_MouseDown);
            // 
            // m_btnUp
            // 
            this.m_btnUp.AccessibleDescription = "Move sort key UP to increase sort priority.\\nOr click \'n drag row to change sort " +
    "priority.";
            this.m_btnUp.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.m_btnUp.Font = new System.Drawing.Font("Wingdings 3", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.m_btnUp.Location = new System.Drawing.Point(112, 224);
            this.m_btnUp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_btnUp.Name = "m_btnUp";
            this.m_btnUp.Size = new System.Drawing.Size(35, 36);
            this.m_btnUp.TabIndex = 5;
            this.m_btnUp.Text = "p";
            this.m_btnUp.UseVisualStyleBackColor = true;
            this.m_btnUp.Click += new System.EventHandler(this.m_btnUp_Click);
            // 
            // m_btnDown
            // 
            this.m_btnDown.AccessibleDescription = "Move sort key DOWN to decrease sort priority.\\nOr click \'n drag row to change sor" +
    "t priority.";
            this.m_btnDown.AccessibleName = "";
            this.m_btnDown.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.m_btnDown.Font = new System.Drawing.Font("Wingdings 3", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.m_btnDown.Location = new System.Drawing.Point(152, 224);
            this.m_btnDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_btnDown.Name = "m_btnDown";
            this.m_btnDown.Size = new System.Drawing.Size(35, 36);
            this.m_btnDown.TabIndex = 6;
            this.m_btnDown.Text = "q";
            this.m_btnDown.UseVisualStyleBackColor = true;
            this.m_btnDown.Click += new System.EventHandler(this.m_btnDown_Click);
            // 
            // SortDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(295, 277);
            this.Controls.Add(this.m_btnDown);
            this.Controls.Add(this.m_btnUp);
            this.Controls.Add(this.m_grid);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SortDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sort Order";
            this.Load += new System.EventHandler(this.FormSort_Load);
            ((System.ComponentModel.ISupportInitialize)(this.m_grid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button m_btnOK;
        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.DataGridView m_grid;
        private System.Windows.Forms.Button m_btnUp;
        private System.Windows.Forms.Button m_btnDown;
    }
}