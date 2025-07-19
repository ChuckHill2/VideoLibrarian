namespace VideoLibrarian
{
    partial class FilterDialog
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
            this.components = new System.ComponentModel.Container();
            this.m_clbVideoType = new System.Windows.Forms.CheckedListBox();
            this.m_btnVtypeSelectNone = new System.Windows.Forms.Button();
            this.m_btnVtypeSelectAll = new System.Windows.Forms.Button();
            this.m_cbRating = new System.Windows.Forms.ComboBox();
            this.m_chkUnrated = new System.Windows.Forms.CheckBox();
            this.m_radWatched = new System.Windows.Forms.RadioButton();
            this.m_radUnwatched = new System.Windows.Forms.RadioButton();
            this.m_radBothWatched = new System.Windows.Forms.RadioButton();
            this.m_grpGenre = new System.Windows.Forms.GroupBox();
            this.m_btnGenreSelectNone = new System.Windows.Forms.Button();
            this.m_btnGenreSelectAll = new System.Windows.Forms.Button();
            this.m_clbGenre = new System.Windows.Forms.CheckedListBox();
            this.m_grpVideoType = new System.Windows.Forms.GroupBox();
            this.m_grpRating = new System.Windows.Forms.GroupBox();
            this.m_grpWatch = new System.Windows.Forms.GroupBox();
            this.m_grpReleaseYear = new System.Windows.Forms.GroupBox();
            this.m_numReleaseTo = new System.Windows.Forms.NumericUpDown();
            this.m_numReleaseFrom = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_chkDisabled = new System.Windows.Forms.CheckBox();
            this.m_grpContains = new System.Windows.Forms.GroupBox();
            this.m_btnContainsClear = new System.Windows.Forms.Button();
            this.m_txtContains = new System.Windows.Forms.TextBox();
            this.m_lbIn = new System.Windows.Forms.Label();
            this.m_cbIn = new System.Windows.Forms.ComboBox();
            this.m_grpCustomGroup = new System.Windows.Forms.GroupBox();
            this.m_cbCustomGroup = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.m_grpGenre.SuspendLayout();
            this.m_grpVideoType.SuspendLayout();
            this.m_grpRating.SuspendLayout();
            this.m_grpWatch.SuspendLayout();
            this.m_grpReleaseYear.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_numReleaseTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_numReleaseFrom)).BeginInit();
            this.m_grpContains.SuspendLayout();
            this.m_grpCustomGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_clbVideoType
            // 
            this.m_clbVideoType.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_clbVideoType.BackColor = System.Drawing.Color.AliceBlue;
            this.m_clbVideoType.CheckOnClick = true;
            this.m_clbVideoType.FormattingEnabled = true;
            this.m_clbVideoType.IntegralHeight = false;
            this.m_clbVideoType.Items.AddRange(new object[] {
            "Feature Movie (800)",
            "TV Episode (800)",
            "TV Mini (800)",
            "TV Movie (800)",
            "TV Series (800)",
            "Video (800)"});
            this.m_clbVideoType.Location = new System.Drawing.Point(9, 29);
            this.m_clbVideoType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_clbVideoType.Name = "m_clbVideoType";
            this.m_clbVideoType.Size = new System.Drawing.Size(205, 195);
            this.m_clbVideoType.TabIndex = 5;
            this.m_clbVideoType.ThreeDCheckBoxes = true;
            // 
            // m_btnVtypeSelectNone
            // 
            this.m_btnVtypeSelectNone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnVtypeSelectNone.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.m_btnVtypeSelectNone.Location = new System.Drawing.Point(139, 231);
            this.m_btnVtypeSelectNone.Margin = new System.Windows.Forms.Padding(0);
            this.m_btnVtypeSelectNone.Name = "m_btnVtypeSelectNone";
            this.m_btnVtypeSelectNone.Size = new System.Drawing.Size(75, 56);
            this.m_btnVtypeSelectNone.TabIndex = 8;
            this.m_btnVtypeSelectNone.Text = "Select None";
            this.m_btnVtypeSelectNone.UseVisualStyleBackColor = true;
            this.m_btnVtypeSelectNone.Click += new System.EventHandler(this.m_btnSelectNone_Click);
            // 
            // m_btnVtypeSelectAll
            // 
            this.m_btnVtypeSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnVtypeSelectAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.m_btnVtypeSelectAll.Location = new System.Drawing.Point(9, 232);
            this.m_btnVtypeSelectAll.Margin = new System.Windows.Forms.Padding(0);
            this.m_btnVtypeSelectAll.Name = "m_btnVtypeSelectAll";
            this.m_btnVtypeSelectAll.Size = new System.Drawing.Size(75, 56);
            this.m_btnVtypeSelectAll.TabIndex = 7;
            this.m_btnVtypeSelectAll.Text = "Select All";
            this.m_btnVtypeSelectAll.UseVisualStyleBackColor = true;
            this.m_btnVtypeSelectAll.Click += new System.EventHandler(this.m_btnSelectAll_Click);
            // 
            // m_cbRating
            // 
            this.m_cbRating.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cbRating.BackColor = System.Drawing.Color.AliceBlue;
            this.m_cbRating.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cbRating.FormattingEnabled = true;
            this.m_cbRating.Location = new System.Drawing.Point(9, 29);
            this.m_cbRating.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbRating.MaxDropDownItems = 16;
            this.m_cbRating.Name = "m_cbRating";
            this.m_cbRating.Size = new System.Drawing.Size(134, 28);
            this.m_cbRating.TabIndex = 9;
            this.toolTip1.SetToolTip(this.m_cbRating, "Filter for all movies with a rating >= to this number (+)\r\nor filter all movies w" +
        "ith a rating <= to this number (-)");
            // 
            // m_chkUnrated
            // 
            this.m_chkUnrated.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_chkUnrated.AutoSize = true;
            this.m_chkUnrated.BackColor = System.Drawing.Color.Transparent;
            this.m_chkUnrated.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.m_chkUnrated.Location = new System.Drawing.Point(10, 73);
            this.m_chkUnrated.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_chkUnrated.Name = "m_chkUnrated";
            this.m_chkUnrated.Size = new System.Drawing.Size(127, 21);
            this.m_chkUnrated.TabIndex = 10;
            this.m_chkUnrated.Text = "Include Unrated";
            this.toolTip1.SetToolTip(this.m_chkUnrated, "Include unrated (aka \r\nrating=0) in the filter.");
            this.m_chkUnrated.UseVisualStyleBackColor = false;
            // 
            // m_radWatched
            // 
            this.m_radWatched.AutoSize = true;
            this.m_radWatched.Location = new System.Drawing.Point(16, 29);
            this.m_radWatched.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_radWatched.Name = "m_radWatched";
            this.m_radWatched.Size = new System.Drawing.Size(91, 24);
            this.m_radWatched.TabIndex = 12;
            this.m_radWatched.TabStop = true;
            this.m_radWatched.Text = "Watched";
            this.m_radWatched.UseVisualStyleBackColor = true;
            // 
            // m_radUnwatched
            // 
            this.m_radUnwatched.AutoSize = true;
            this.m_radUnwatched.Location = new System.Drawing.Point(16, 62);
            this.m_radUnwatched.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_radUnwatched.Name = "m_radUnwatched";
            this.m_radUnwatched.Size = new System.Drawing.Size(108, 24);
            this.m_radUnwatched.TabIndex = 13;
            this.m_radUnwatched.TabStop = true;
            this.m_radUnwatched.Text = "Unwatched";
            this.m_radUnwatched.UseVisualStyleBackColor = true;
            // 
            // m_radBothWatched
            // 
            this.m_radBothWatched.AutoSize = true;
            this.m_radBothWatched.Location = new System.Drawing.Point(16, 94);
            this.m_radBothWatched.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_radBothWatched.Name = "m_radBothWatched";
            this.m_radBothWatched.Size = new System.Drawing.Size(61, 24);
            this.m_radBothWatched.TabIndex = 14;
            this.m_radBothWatched.TabStop = true;
            this.m_radBothWatched.Text = "Both";
            this.m_radBothWatched.UseVisualStyleBackColor = true;
            // 
            // m_grpGenre
            // 
            this.m_grpGenre.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_grpGenre.Controls.Add(this.m_btnGenreSelectNone);
            this.m_grpGenre.Controls.Add(this.m_btnGenreSelectAll);
            this.m_grpGenre.Controls.Add(this.m_clbGenre);
            this.m_grpGenre.Location = new System.Drawing.Point(16, 170);
            this.m_grpGenre.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpGenre.Name = "m_grpGenre";
            this.m_grpGenre.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpGenre.Size = new System.Drawing.Size(212, 299);
            this.m_grpGenre.TabIndex = 15;
            this.m_grpGenre.TabStop = false;
            this.m_grpGenre.Text = "Genre";
            this.toolTip1.SetToolTip(this.m_grpGenre, "Filter by genre. The number in parentheses \r\nis the count of movies included in t" +
        "his genre. \r\nMovies may have multiple generes.");
            // 
            // m_btnGenreSelectNone
            // 
            this.m_btnGenreSelectNone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnGenreSelectNone.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.m_btnGenreSelectNone.Location = new System.Drawing.Point(126, 231);
            this.m_btnGenreSelectNone.Margin = new System.Windows.Forms.Padding(0);
            this.m_btnGenreSelectNone.Name = "m_btnGenreSelectNone";
            this.m_btnGenreSelectNone.Size = new System.Drawing.Size(75, 56);
            this.m_btnGenreSelectNone.TabIndex = 7;
            this.m_btnGenreSelectNone.Text = "Select None";
            this.m_btnGenreSelectNone.UseVisualStyleBackColor = true;
            this.m_btnGenreSelectNone.Click += new System.EventHandler(this.m_btnSelectNone_Click);
            // 
            // m_btnGenreSelectAll
            // 
            this.m_btnGenreSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnGenreSelectAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.m_btnGenreSelectAll.Location = new System.Drawing.Point(9, 232);
            this.m_btnGenreSelectAll.Margin = new System.Windows.Forms.Padding(0);
            this.m_btnGenreSelectAll.Name = "m_btnGenreSelectAll";
            this.m_btnGenreSelectAll.Size = new System.Drawing.Size(75, 56);
            this.m_btnGenreSelectAll.TabIndex = 6;
            this.m_btnGenreSelectAll.Text = "Select All";
            this.m_btnGenreSelectAll.UseVisualStyleBackColor = true;
            this.m_btnGenreSelectAll.Click += new System.EventHandler(this.m_btnSelectAll_Click);
            // 
            // m_clbGenre
            // 
            this.m_clbGenre.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_clbGenre.BackColor = System.Drawing.Color.AliceBlue;
            this.m_clbGenre.CheckOnClick = true;
            this.m_clbGenre.FormattingEnabled = true;
            this.m_clbGenre.IntegralHeight = false;
            this.m_clbGenre.Items.AddRange(new object[] {
            "Action (800)",
            "Adventure (800)",
            "Animation (800)",
            "Biography (800)",
            "Comedy (800)",
            "Crime (800)",
            "Drama (800)",
            "Family (800)",
            "Fantasy (800)",
            "History (800)",
            "Horror (800)",
            "Music (800)",
            "Musical (800)",
            "Mystery (800)",
            "Romance (800)",
            "Sci-Fi (800)",
            "Short (800)",
            "Sport (800)",
            "Thriller (800)",
            "War (800)",
            "Western (800)"});
            this.m_clbGenre.Location = new System.Drawing.Point(9, 29);
            this.m_clbGenre.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_clbGenre.Name = "m_clbGenre";
            this.m_clbGenre.Size = new System.Drawing.Size(192, 195);
            this.m_clbGenre.TabIndex = 5;
            this.m_clbGenre.ThreeDCheckBoxes = true;
            // 
            // m_grpVideoType
            // 
            this.m_grpVideoType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_grpVideoType.Controls.Add(this.m_clbVideoType);
            this.m_grpVideoType.Controls.Add(this.m_btnVtypeSelectAll);
            this.m_grpVideoType.Controls.Add(this.m_btnVtypeSelectNone);
            this.m_grpVideoType.Location = new System.Drawing.Point(238, 170);
            this.m_grpVideoType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpVideoType.Name = "m_grpVideoType";
            this.m_grpVideoType.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpVideoType.Size = new System.Drawing.Size(225, 299);
            this.m_grpVideoType.TabIndex = 16;
            this.m_grpVideoType.TabStop = false;
            this.m_grpVideoType.Text = "Video Type";
            this.toolTip1.SetToolTip(this.m_grpVideoType, "Filter by video type. The number in parentheses \r\nis the count of movies included" +
        " in this video type. ");
            // 
            // m_grpRating
            // 
            this.m_grpRating.Controls.Add(this.m_chkUnrated);
            this.m_grpRating.Controls.Add(this.m_cbRating);
            this.m_grpRating.Location = new System.Drawing.Point(475, 91);
            this.m_grpRating.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpRating.Name = "m_grpRating";
            this.m_grpRating.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpRating.Size = new System.Drawing.Size(154, 108);
            this.m_grpRating.TabIndex = 17;
            this.m_grpRating.TabStop = false;
            this.m_grpRating.Text = "Rating";
            this.toolTip1.SetToolTip(this.m_grpRating, "The popularity/quality/how \r\nwell liked, this movie is.");
            // 
            // m_grpWatch
            // 
            this.m_grpWatch.Controls.Add(this.m_radWatched);
            this.m_grpWatch.Controls.Add(this.m_radUnwatched);
            this.m_grpWatch.Controls.Add(this.m_radBothWatched);
            this.m_grpWatch.Location = new System.Drawing.Point(475, 335);
            this.m_grpWatch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpWatch.Name = "m_grpWatch";
            this.m_grpWatch.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpWatch.Size = new System.Drawing.Size(154, 134);
            this.m_grpWatch.TabIndex = 18;
            this.m_grpWatch.TabStop = false;
            this.m_grpWatch.Text = "Watch";
            this.toolTip1.SetToolTip(this.m_grpWatch, "Filter by the watched flag.");
            // 
            // m_grpReleaseYear
            // 
            this.m_grpReleaseYear.Controls.Add(this.m_numReleaseTo);
            this.m_grpReleaseYear.Controls.Add(this.m_numReleaseFrom);
            this.m_grpReleaseYear.Controls.Add(this.label1);
            this.m_grpReleaseYear.Controls.Add(this.label2);
            this.m_grpReleaseYear.Location = new System.Drawing.Point(475, 207);
            this.m_grpReleaseYear.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpReleaseYear.Name = "m_grpReleaseYear";
            this.m_grpReleaseYear.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpReleaseYear.Size = new System.Drawing.Size(154, 117);
            this.m_grpReleaseYear.TabIndex = 19;
            this.m_grpReleaseYear.TabStop = false;
            this.m_grpReleaseYear.Text = "Release Year";
            this.toolTip1.SetToolTip(this.m_grpReleaseYear, "Filter by the movie \r\nrelease year range.");
            // 
            // m_numReleaseTo
            // 
            this.m_numReleaseTo.BackColor = System.Drawing.Color.AliceBlue;
            this.m_numReleaseTo.Location = new System.Drawing.Point(60, 69);
            this.m_numReleaseTo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_numReleaseTo.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.m_numReleaseTo.Minimum = new decimal(new int[] {
            1900,
            0,
            0,
            0});
            this.m_numReleaseTo.Name = "m_numReleaseTo";
            this.m_numReleaseTo.Size = new System.Drawing.Size(78, 26);
            this.m_numReleaseTo.TabIndex = 1;
            this.m_numReleaseTo.Value = new decimal(new int[] {
            2025,
            0,
            0,
            0});
            this.m_numReleaseTo.ValueChanged += new System.EventHandler(this.m_numReleaseTo_ValueChanged);
            // 
            // m_numReleaseFrom
            // 
            this.m_numReleaseFrom.BackColor = System.Drawing.Color.AliceBlue;
            this.m_numReleaseFrom.Location = new System.Drawing.Point(60, 29);
            this.m_numReleaseFrom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_numReleaseFrom.Maximum = new decimal(new int[] {
            2018,
            0,
            0,
            0});
            this.m_numReleaseFrom.Minimum = new decimal(new int[] {
            1900,
            0,
            0,
            0});
            this.m_numReleaseFrom.Name = "m_numReleaseFrom";
            this.m_numReleaseFrom.Size = new System.Drawing.Size(78, 26);
            this.m_numReleaseFrom.TabIndex = 0;
            this.m_numReleaseFrom.Value = new decimal(new int[] {
            1900,
            0,
            0,
            0});
            this.m_numReleaseFrom.ValueChanged += new System.EventHandler(this.m_numReleaseFrom_ValueChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 31);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 28);
            this.label1.TabIndex = 2;
            this.label1.Text = "From:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(6, 69);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 28);
            this.label2.TabIndex = 3;
            this.label2.Text = "To:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // m_btnOK
            // 
            this.m_btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Location = new System.Drawing.Point(395, 482);
            this.m_btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(112, 34);
            this.m_btnOK.TabIndex = 20;
            this.m_btnOK.Text = "OK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(517, 482);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(112, 34);
            this.m_btnCancel.TabIndex = 21;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
            // 
            // m_chkDisabled
            // 
            this.m_chkDisabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_chkDisabled.Appearance = System.Windows.Forms.Appearance.Button;
            this.m_chkDisabled.AutoSize = true;
            this.m_chkDisabled.Location = new System.Drawing.Point(18, 485);
            this.m_chkDisabled.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_chkDisabled.Name = "m_chkDisabled";
            this.m_chkDisabled.Size = new System.Drawing.Size(153, 30);
            this.m_chkDisabled.TabIndex = 22;
            this.m_chkDisabled.Text = "Disable All Filtering";
            this.toolTip1.SetToolTip(this.m_chkDisabled, "Enable/Disable all filtering. Easier than manually \r\nclearing all of the above. E" +
        "ven when disabled, the \r\nlast values are maintained until program exit.");
            this.m_chkDisabled.UseVisualStyleBackColor = true;
            this.m_chkDisabled.CheckedChanged += new System.EventHandler(this.m_chkDisabled_CheckedChanged);
            // 
            // m_grpContains
            // 
            this.m_grpContains.Controls.Add(this.m_btnContainsClear);
            this.m_grpContains.Controls.Add(this.m_txtContains);
            this.m_grpContains.Controls.Add(this.m_lbIn);
            this.m_grpContains.Controls.Add(this.m_cbIn);
            this.m_grpContains.Location = new System.Drawing.Point(16, 12);
            this.m_grpContains.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpContains.Name = "m_grpContains";
            this.m_grpContains.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpContains.Size = new System.Drawing.Size(613, 69);
            this.m_grpContains.TabIndex = 23;
            this.m_grpContains.TabStop = false;
            this.m_grpContains.Text = "Contains Text";
            // 
            // m_btnContainsClear
            // 
            this.m_btnContainsClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnContainsClear.BackColor = System.Drawing.Color.AliceBlue;
            this.m_btnContainsClear.BackgroundImage = global::VideoLibrarian.Properties.Resources.ClearText;
            this.m_btnContainsClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.m_btnContainsClear.FlatAppearance.BorderSize = 0;
            this.m_btnContainsClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_btnContainsClear.ForeColor = System.Drawing.Color.DarkGray;
            this.m_btnContainsClear.Location = new System.Drawing.Point(410, 30);
            this.m_btnContainsClear.Name = "m_btnContainsClear";
            this.m_btnContainsClear.Size = new System.Drawing.Size(24, 24);
            this.m_btnContainsClear.TabIndex = 12;
            this.toolTip1.SetToolTip(this.m_btnContainsClear, "Clear the text \r\nin this field.");
            this.m_btnContainsClear.UseVisualStyleBackColor = false;
            this.m_btnContainsClear.Click += new System.EventHandler(this.m_btnContainsClear_Click);
            // 
            // m_txtContains
            // 
            this.m_txtContains.BackColor = System.Drawing.Color.AliceBlue;
            this.m_txtContains.Location = new System.Drawing.Point(9, 29);
            this.m_txtContains.Name = "m_txtContains";
            this.m_txtContains.Size = new System.Drawing.Size(427, 26);
            this.m_txtContains.TabIndex = 11;
            this.toolTip1.SetToolTip(this.m_txtContains, "Filter on this case-insensitive text found in one\r\nof the movie properties listed" +
        " to the right.");
            // 
            // m_lbIn
            // 
            this.m_lbIn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lbIn.AutoSize = true;
            this.m_lbIn.Location = new System.Drawing.Point(442, 32);
            this.m_lbIn.Name = "m_lbIn";
            this.m_lbIn.Size = new System.Drawing.Size(21, 20);
            this.m_lbIn.TabIndex = 10;
            this.m_lbIn.Text = "in";
            // 
            // m_cbIn
            // 
            this.m_cbIn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cbIn.BackColor = System.Drawing.Color.AliceBlue;
            this.m_cbIn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cbIn.FormattingEnabled = true;
            this.m_cbIn.Location = new System.Drawing.Point(468, 29);
            this.m_cbIn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbIn.Name = "m_cbIn";
            this.m_cbIn.Size = new System.Drawing.Size(134, 28);
            this.m_cbIn.TabIndex = 9;
            this.toolTip1.SetToolTip(this.m_cbIn, "The movie properties to search.");
            // 
            // m_grpCustomGroup
            // 
            this.m_grpCustomGroup.AccessibleDescription = "Select a single user-defined movie attribute\\nto filter on. \"Any\" assumes no filt" +
    "ering.";
            this.m_grpCustomGroup.Controls.Add(this.m_cbCustomGroup);
            this.m_grpCustomGroup.Location = new System.Drawing.Point(16, 91);
            this.m_grpCustomGroup.Name = "m_grpCustomGroup";
            this.m_grpCustomGroup.Size = new System.Drawing.Size(447, 69);
            this.m_grpCustomGroup.TabIndex = 24;
            this.m_grpCustomGroup.TabStop = false;
            this.m_grpCustomGroup.Text = "Custom Group";
            this.toolTip1.SetToolTip(this.m_grpCustomGroup, "Filter by custom user-defined groups. These \r\ngroups are manually defined in \"Edi" +
        "t Video \r\nProperties\" within the VideoOrganizer app.");
            // 
            // m_cbCustomGroup
            // 
            this.m_cbCustomGroup.AccessibleDescription = "Select a single user-defined movie attribute\\nto filter on. \"Any\" assumes no filt" +
    "ering.";
            this.m_cbCustomGroup.BackColor = System.Drawing.Color.AliceBlue;
            this.m_cbCustomGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cbCustomGroup.FormattingEnabled = true;
            this.m_cbCustomGroup.Location = new System.Drawing.Point(9, 27);
            this.m_cbCustomGroup.Name = "m_cbCustomGroup";
            this.m_cbCustomGroup.Size = new System.Drawing.Size(427, 28);
            this.m_cbCustomGroup.TabIndex = 0;
            // 
            // FilterDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(644, 535);
            this.Controls.Add(this.m_grpCustomGroup);
            this.Controls.Add(this.m_grpContains);
            this.Controls.Add(this.m_chkDisabled);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_grpReleaseYear);
            this.Controls.Add(this.m_grpWatch);
            this.Controls.Add(this.m_grpRating);
            this.Controls.Add(this.m_grpVideoType);
            this.Controls.Add(this.m_grpGenre);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(660, 824);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(660, 569);
            this.Name = "FilterDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Movie Filters";
            this.m_grpGenre.ResumeLayout(false);
            this.m_grpVideoType.ResumeLayout(false);
            this.m_grpRating.ResumeLayout(false);
            this.m_grpRating.PerformLayout();
            this.m_grpWatch.ResumeLayout(false);
            this.m_grpWatch.PerformLayout();
            this.m_grpReleaseYear.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_numReleaseTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_numReleaseFrom)).EndInit();
            this.m_grpContains.ResumeLayout(false);
            this.m_grpContains.PerformLayout();
            this.m_grpCustomGroup.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox m_clbVideoType;
        private System.Windows.Forms.Button m_btnVtypeSelectNone;
        private System.Windows.Forms.Button m_btnVtypeSelectAll;
        private System.Windows.Forms.ComboBox m_cbRating;
        private System.Windows.Forms.CheckBox m_chkUnrated;
        private System.Windows.Forms.RadioButton m_radWatched;
        private System.Windows.Forms.RadioButton m_radUnwatched;
        private System.Windows.Forms.RadioButton m_radBothWatched;
        private System.Windows.Forms.GroupBox m_grpGenre;
        private System.Windows.Forms.Button m_btnGenreSelectNone;
        private System.Windows.Forms.Button m_btnGenreSelectAll;
        private System.Windows.Forms.CheckedListBox m_clbGenre;
        private System.Windows.Forms.GroupBox m_grpVideoType;
        private System.Windows.Forms.GroupBox m_grpRating;
        private System.Windows.Forms.GroupBox m_grpWatch;
        private System.Windows.Forms.GroupBox m_grpReleaseYear;
        private System.Windows.Forms.NumericUpDown m_numReleaseTo;
        private System.Windows.Forms.NumericUpDown m_numReleaseFrom;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button m_btnOK;
        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.CheckBox m_chkDisabled;
        private System.Windows.Forms.GroupBox m_grpContains;
        private System.Windows.Forms.TextBox m_txtContains;
        private System.Windows.Forms.Label m_lbIn;
        private System.Windows.Forms.ComboBox m_cbIn;
        private System.Windows.Forms.Button m_btnContainsClear;
        private System.Windows.Forms.GroupBox m_grpCustomGroup;
        private System.Windows.Forms.ComboBox m_cbCustomGroup;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}