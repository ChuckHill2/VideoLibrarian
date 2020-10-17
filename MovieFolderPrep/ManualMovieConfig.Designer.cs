namespace MovieFolderPrep
{
    partial class ManualMovieConfig
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManualMovieConfig));
            this.m_btnSelectMovieFolder = new System.Windows.Forms.Button();
            this.m_btnSave = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_pnlAllProperties = new System.Windows.Forms.Panel();
            this.m_btnDownloadPoster = new System.Windows.Forms.Button();
            this.m_btnDownloadWebpage = new System.Windows.Forms.Button();
            this.m_txtCreators = new MovieFolderPrep.LabeledTextBox();
            this.m_grpVideoType = new System.Windows.Forms.GroupBox();
            this.m_clbVideoType = new System.Windows.Forms.CheckedListBox();
            this.m_grpGenre = new System.Windows.Forms.GroupBox();
            this.m_clbGenre = new System.Windows.Forms.CheckedListBox();
            this.m_txtMovieName = new MovieFolderPrep.LabeledTextBox();
            this.m_txtEpisodeName = new MovieFolderPrep.LabeledTextBox();
            this.m_lblReleaseDate = new System.Windows.Forms.Label();
            this.m_lblRating = new System.Windows.Forms.Label();
            this.m_numRating = new System.Windows.Forms.NumericUpDown();
            this.m_txtPosterUrl = new MovieFolderPrep.LabeledTextBox();
            this.m_txtPlot = new MovieFolderPrep.LabeledTextBox();
            this.m_txtSummary = new MovieFolderPrep.LabeledTextBox();
            this.m_txtDirectors = new MovieFolderPrep.LabeledTextBox();
            this.m_txtWriters = new MovieFolderPrep.LabeledTextBox();
            this.m_txtCast = new MovieFolderPrep.LabeledTextBox();
            this.m_grpSeriesEpisode = new System.Windows.Forms.GroupBox();
            this.m_grpEpisode = new System.Windows.Forms.GroupBox();
            this.m_lblEpisode = new System.Windows.Forms.Label();
            this.m_numEpisode = new System.Windows.Forms.NumericUpDown();
            this.m_lblSeason = new System.Windows.Forms.Label();
            this.m_numSeason = new System.Windows.Forms.NumericUpDown();
            this.m_grpSeries = new System.Windows.Forms.GroupBox();
            this.m_lblEpisodeCount = new System.Windows.Forms.Label();
            this.m_numEpisodeCount = new System.Windows.Forms.NumericUpDown();
            this.m_dtReleaseDate = new System.Windows.Forms.DateTimePicker();
            this.m_txtImdbUrl = new MovieFolderPrep.LabeledTextBox();
            this.m_chkImdbUrl = new System.Windows.Forms.CheckBox();
            this.m_grpWatched = new System.Windows.Forms.GroupBox();
            this.m_grpWatchedDt = new System.Windows.Forms.GroupBox();
            this.m_dtWatched = new System.Windows.Forms.DateTimePicker();
            this.m_chkWatched = new System.Windows.Forms.CheckBox();
            this.m_grpExtractedVidProps = new System.Windows.Forms.GroupBox();
            this.m_grpVerticalDivider = new System.Windows.Forms.GroupBox();
            this.m_lnkRecompute = new System.Windows.Forms.LinkLabel();
            this.m_lblDimensions = new System.Windows.Forms.Label();
            this.m_lblAspectRatio = new System.Windows.Forms.Label();
            this.m_lblRuntime = new System.Windows.Forms.Label();
            this.m_lblDownloadDate = new System.Windows.Forms.Label();
            this.m_lblDimensionsLabel = new System.Windows.Forms.Label();
            this.m_lblAspectRatioLabel = new System.Windows.Forms.Label();
            this.m_lblRuntimeLabel = new System.Windows.Forms.Label();
            this.m_lblDownloadDateLabel = new System.Windows.Forms.Label();
            this.m_btnSelectMovieFile = new System.Windows.Forms.Button();
            this.m_txtMoviePath = new MovieFolderPrep.LabeledTextBox();
            this.m_pnlAllProperties.SuspendLayout();
            this.m_grpVideoType.SuspendLayout();
            this.m_grpGenre.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_numRating)).BeginInit();
            this.m_grpSeriesEpisode.SuspendLayout();
            this.m_grpEpisode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_numEpisode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_numSeason)).BeginInit();
            this.m_grpSeries.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_numEpisodeCount)).BeginInit();
            this.m_grpWatched.SuspendLayout();
            this.m_grpWatchedDt.SuspendLayout();
            this.m_grpExtractedVidProps.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_btnSelectMovieFolder
            // 
            this.m_btnSelectMovieFolder.AccessibleDescription = "Select TV Series movie folder.";
            this.m_btnSelectMovieFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnSelectMovieFolder.Image = ((System.Drawing.Image)(resources.GetObject("m_btnSelectMovieFolder.Image")));
            this.m_btnSelectMovieFolder.Location = new System.Drawing.Point(613, 12);
            this.m_btnSelectMovieFolder.Name = "m_btnSelectMovieFolder";
            this.m_btnSelectMovieFolder.Size = new System.Drawing.Size(22, 22);
            this.m_btnSelectMovieFolder.TabIndex = 1;
            this.m_btnSelectMovieFolder.UseVisualStyleBackColor = true;
            this.m_btnSelectMovieFolder.Click += new System.EventHandler(this.m_btnSelectMovieFolder_Click);
            // 
            // m_btnSave
            // 
            this.m_btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnSave.Location = new System.Drawing.Point(479, 523);
            this.m_btnSave.Name = "m_btnSave";
            this.m_btnSave.Size = new System.Drawing.Size(75, 23);
            this.m_btnSave.TabIndex = 4;
            this.m_btnSave.Text = "Save";
            this.m_btnSave.UseVisualStyleBackColor = true;
            this.m_btnSave.Click += new System.EventHandler(this.m_btnSave_Click);
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnCancel.Location = new System.Drawing.Point(560, 523);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
            this.m_btnCancel.TabIndex = 5;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
            // 
            // m_pnlAllProperties
            // 
            this.m_pnlAllProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pnlAllProperties.Controls.Add(this.m_btnDownloadPoster);
            this.m_pnlAllProperties.Controls.Add(this.m_btnDownloadWebpage);
            this.m_pnlAllProperties.Controls.Add(this.m_txtCreators);
            this.m_pnlAllProperties.Controls.Add(this.m_grpVideoType);
            this.m_pnlAllProperties.Controls.Add(this.m_grpGenre);
            this.m_pnlAllProperties.Controls.Add(this.m_txtMovieName);
            this.m_pnlAllProperties.Controls.Add(this.m_txtEpisodeName);
            this.m_pnlAllProperties.Controls.Add(this.m_lblReleaseDate);
            this.m_pnlAllProperties.Controls.Add(this.m_lblRating);
            this.m_pnlAllProperties.Controls.Add(this.m_numRating);
            this.m_pnlAllProperties.Controls.Add(this.m_txtPosterUrl);
            this.m_pnlAllProperties.Controls.Add(this.m_txtPlot);
            this.m_pnlAllProperties.Controls.Add(this.m_txtSummary);
            this.m_pnlAllProperties.Controls.Add(this.m_txtDirectors);
            this.m_pnlAllProperties.Controls.Add(this.m_txtWriters);
            this.m_pnlAllProperties.Controls.Add(this.m_txtCast);
            this.m_pnlAllProperties.Controls.Add(this.m_grpSeriesEpisode);
            this.m_pnlAllProperties.Controls.Add(this.m_dtReleaseDate);
            this.m_pnlAllProperties.Controls.Add(this.m_txtImdbUrl);
            this.m_pnlAllProperties.Controls.Add(this.m_chkImdbUrl);
            this.m_pnlAllProperties.Controls.Add(this.m_grpWatched);
            this.m_pnlAllProperties.Controls.Add(this.m_grpExtractedVidProps);
            this.m_pnlAllProperties.Location = new System.Drawing.Point(12, 39);
            this.m_pnlAllProperties.Name = "m_pnlAllProperties";
            this.m_pnlAllProperties.Size = new System.Drawing.Size(623, 482);
            this.m_pnlAllProperties.TabIndex = 3;
            // 
            // m_btnDownloadPoster
            // 
            this.m_btnDownloadPoster.AccessibleDescription = "Retrieve movie poster image\\noverwriting current image.";
            this.m_btnDownloadPoster.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnDownloadPoster.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlDark;
            this.m_btnDownloadPoster.Image = global::MovieFolderPrep.Properties.Resources.FileSave16Black;
            this.m_btnDownloadPoster.Location = new System.Drawing.Point(601, 128);
            this.m_btnDownloadPoster.Name = "m_btnDownloadPoster";
            this.m_btnDownloadPoster.Size = new System.Drawing.Size(22, 22);
            this.m_btnDownloadPoster.TabIndex = 55;
            this.m_btnDownloadPoster.UseVisualStyleBackColor = true;
            this.m_btnDownloadPoster.Click += new System.EventHandler(this.m_btnDownloadPoster_Click);
            // 
            // m_btnDownloadWebpage
            // 
            this.m_btnDownloadWebpage.AccessibleDescription = "Retrieve properties from IMDB web\\nsite overwriting current values.";
            this.m_btnDownloadWebpage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnDownloadWebpage.Enabled = false;
            this.m_btnDownloadWebpage.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlDark;
            this.m_btnDownloadWebpage.Image = global::MovieFolderPrep.Properties.Resources.FileSave16Black;
            this.m_btnDownloadWebpage.Location = new System.Drawing.Point(601, 101);
            this.m_btnDownloadWebpage.Name = "m_btnDownloadWebpage";
            this.m_btnDownloadWebpage.Size = new System.Drawing.Size(22, 22);
            this.m_btnDownloadWebpage.TabIndex = 54;
            this.m_btnDownloadWebpage.UseVisualStyleBackColor = true;
            this.m_btnDownloadWebpage.Click += new System.EventHandler(this.m_btnDownloadWebpage_Click);
            // 
            // m_txtCreators
            // 
            this.m_txtCreators.AccessibleDescription = "Creators: A comma-delimited list of creator names.";
            this.m_txtCreators.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtCreators.Location = new System.Drawing.Point(247, 306);
            this.m_txtCreators.Name = "m_txtCreators";
            this.m_txtCreators.Size = new System.Drawing.Size(375, 20);
            this.m_txtCreators.TabIndex = 53;
            this.m_txtCreators.TextLabel = "Creators";
            // 
            // m_grpVideoType
            // 
            this.m_grpVideoType.AccessibleDescription = "Select the type of video this refers\\nto. Must select exactly one.";
            this.m_grpVideoType.Controls.Add(this.m_clbVideoType);
            this.m_grpVideoType.Location = new System.Drawing.Point(119, 3);
            this.m_grpVideoType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpVideoType.Name = "m_grpVideoType";
            this.m_grpVideoType.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpVideoType.Size = new System.Drawing.Size(121, 219);
            this.m_grpVideoType.TabIndex = 1;
            this.m_grpVideoType.TabStop = false;
            this.m_grpVideoType.Text = "Video Type";
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
            "Clip",
            "Documentary",
            "Feature Movie",
            "Home Video",
            "Short",
            "TV Episode",
            "TV Mini-Series",
            "TV Movie",
            "TV Series",
            "TV Short",
            "TV Special",
            "Video"});
            this.m_clbVideoType.Location = new System.Drawing.Point(10, 20);
            this.m_clbVideoType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_clbVideoType.Name = "m_clbVideoType";
            this.m_clbVideoType.Size = new System.Drawing.Size(101, 188);
            this.m_clbVideoType.TabIndex = 0;
            this.m_clbVideoType.ThreeDCheckBoxes = true;
            this.m_clbVideoType.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.m_clbVideoType_ItemCheck);
            this.m_clbVideoType.Leave += new System.EventHandler(this.JustHideError_Leave);
            // 
            // m_grpGenre
            // 
            this.m_grpGenre.AccessibleDescription = "Select one or more genres that this video\\nfits into. Must select at least one.";
            this.m_grpGenre.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_grpGenre.Controls.Add(this.m_clbGenre);
            this.m_grpGenre.Location = new System.Drawing.Point(0, 3);
            this.m_grpGenre.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpGenre.Name = "m_grpGenre";
            this.m_grpGenre.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpGenre.Size = new System.Drawing.Size(111, 476);
            this.m_grpGenre.TabIndex = 0;
            this.m_grpGenre.TabStop = false;
            this.m_grpGenre.Text = "Genre";
            // 
            // m_clbGenre
            // 
            this.m_clbGenre.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_clbGenre.BackColor = System.Drawing.Color.AliceBlue;
            this.m_clbGenre.CheckOnClick = true;
            this.m_clbGenre.FormattingEnabled = true;
            this.m_clbGenre.IntegralHeight = false;
            this.m_clbGenre.Items.AddRange(new object[] {
            "Action",
            "Adult",
            "Adventure",
            "Animation",
            "Biography",
            "Clip",
            "Comedy",
            "Crime",
            "Documentary",
            "Drama",
            "Family",
            "Fantasy",
            "Film-Noir",
            "Game-Show",
            "History",
            "Horror",
            "Music",
            "Musical",
            "Mystery",
            "News",
            "Reality-TV",
            "Romance",
            "Sci-Fi",
            "Sport",
            "Short",
            "Talk-Show",
            "Thriller",
            "War",
            "Western"});
            this.m_clbGenre.Location = new System.Drawing.Point(10, 20);
            this.m_clbGenre.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_clbGenre.Name = "m_clbGenre";
            this.m_clbGenre.Size = new System.Drawing.Size(91, 449);
            this.m_clbGenre.TabIndex = 0;
            this.m_clbGenre.ThreeDCheckBoxes = true;
            this.m_clbGenre.Leave += new System.EventHandler(this.JustHideError_Leave);
            // 
            // m_txtMovieName
            // 
            this.m_txtMovieName.AccessibleDescription = "Enter the name of this movie or series. If it is\\nan episode, enter the episode n" +
    "ame below.";
            this.m_txtMovieName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtMovieName.Location = new System.Drawing.Point(247, 9);
            this.m_txtMovieName.Name = "m_txtMovieName";
            this.m_txtMovieName.Size = new System.Drawing.Size(375, 20);
            this.m_txtMovieName.TabIndex = 2;
            this.m_txtMovieName.TextLabel = "Movie or Series name";
            this.m_txtMovieName.Leave += new System.EventHandler(this.JustHideError_Leave);
            // 
            // m_txtEpisodeName
            // 
            this.m_txtEpisodeName.AccessibleDescription = "The name of just this video episode.\\nNot the series name.";
            this.m_txtEpisodeName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtEpisodeName.BackColor = System.Drawing.SystemColors.Window;
            this.m_txtEpisodeName.Enabled = false;
            this.m_txtEpisodeName.Location = new System.Drawing.Point(247, 35);
            this.m_txtEpisodeName.Name = "m_txtEpisodeName";
            this.m_txtEpisodeName.Size = new System.Drawing.Size(375, 20);
            this.m_txtEpisodeName.TabIndex = 3;
            this.m_txtEpisodeName.TextLabel = "Episode name";
            // 
            // m_lblReleaseDate
            // 
            this.m_lblReleaseDate.AccessibleDescription = "The date this movie was released to theaters.\\nIf this is a home movie, the date " +
    "it was created.";
            this.m_lblReleaseDate.AutoSize = true;
            this.m_lblReleaseDate.Location = new System.Drawing.Point(246, 59);
            this.m_lblReleaseDate.Margin = new System.Windows.Forms.Padding(0);
            this.m_lblReleaseDate.Name = "m_lblReleaseDate";
            this.m_lblReleaseDate.Size = new System.Drawing.Size(72, 13);
            this.m_lblReleaseDate.TabIndex = 4;
            this.m_lblReleaseDate.Text = "Release Date";
            // 
            // m_lblRating
            // 
            this.m_lblRating.AccessibleDescription = "The popularity/quality/how well liked, this movie is. The value\\nranges from 0.0 " +
    "to 10.0 where 0.0 is assumed to be undefined.";
            this.m_lblRating.AutoSize = true;
            this.m_lblRating.Location = new System.Drawing.Point(354, 59);
            this.m_lblRating.Margin = new System.Windows.Forms.Padding(0);
            this.m_lblRating.Name = "m_lblRating";
            this.m_lblRating.Size = new System.Drawing.Size(70, 13);
            this.m_lblRating.TabIndex = 6;
            this.m_lblRating.Text = "Movie Rating";
            // 
            // m_numRating
            // 
            this.m_numRating.AccessibleDescription = "The popularity/quality/how well liked, this movie is. The value\\nranges from 0.0 " +
    "to 10.0 where 0.0 is assumed to be undefined.";
            this.m_numRating.DecimalPlaces = 1;
            this.m_numRating.Location = new System.Drawing.Point(356, 74);
            this.m_numRating.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.m_numRating.Name = "m_numRating";
            this.m_numRating.Size = new System.Drawing.Size(73, 20);
            this.m_numRating.TabIndex = 7;
            // 
            // m_txtPosterUrl
            // 
            this.m_txtPosterUrl.AccessibleDescription = resources.GetString("m_txtPosterUrl.AccessibleDescription");
            this.m_txtPosterUrl.AllowDrop = true;
            this.m_txtPosterUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtPosterUrl.Location = new System.Drawing.Point(247, 129);
            this.m_txtPosterUrl.Name = "m_txtPosterUrl";
            this.m_txtPosterUrl.Size = new System.Drawing.Size(353, 20);
            this.m_txtPosterUrl.TabIndex = 10;
            this.m_txtPosterUrl.TextLabel = "Poster Url";
            this.m_txtPosterUrl.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_txtPosterUrl_DragDrop);
            this.m_txtPosterUrl.DragEnter += new System.Windows.Forms.DragEventHandler(this.m_txtPosterUrl_DragEnter);
            this.m_txtPosterUrl.Leave += new System.EventHandler(this.m_txtPosterUrl_Leave);
            this.m_txtPosterUrl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TextBoxRun_MouseDoubleClick);
            // 
            // m_txtPlot
            // 
            this.m_txtPlot.AccessibleDescription = "Plot: Relativly short description of the movie.\\nUse Summary for a more detailed " +
    "description.";
            this.m_txtPlot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtPlot.Location = new System.Drawing.Point(247, 156);
            this.m_txtPlot.Multiline = true;
            this.m_txtPlot.Name = "m_txtPlot";
            this.m_txtPlot.Size = new System.Drawing.Size(375, 58);
            this.m_txtPlot.TabIndex = 11;
            this.m_txtPlot.TextLabel = "Plot";
            // 
            // m_txtSummary
            // 
            this.m_txtSummary.AccessibleDescription = "Summary: A more detailed description of the movie.";
            this.m_txtSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtSummary.Location = new System.Drawing.Point(247, 221);
            this.m_txtSummary.Multiline = true;
            this.m_txtSummary.Name = "m_txtSummary";
            this.m_txtSummary.Size = new System.Drawing.Size(375, 78);
            this.m_txtSummary.TabIndex = 12;
            this.m_txtSummary.TextLabel = "Summary";
            // 
            // m_txtDirectors
            // 
            this.m_txtDirectors.AccessibleDescription = "Directors: A comma-delimited list of director names.";
            this.m_txtDirectors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtDirectors.Location = new System.Drawing.Point(247, 333);
            this.m_txtDirectors.Name = "m_txtDirectors";
            this.m_txtDirectors.Size = new System.Drawing.Size(375, 20);
            this.m_txtDirectors.TabIndex = 13;
            this.m_txtDirectors.TextLabel = "Directors";
            // 
            // m_txtWriters
            // 
            this.m_txtWriters.AccessibleDescription = "Writers: A comma-delimited list of writer and author names.";
            this.m_txtWriters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtWriters.Location = new System.Drawing.Point(247, 360);
            this.m_txtWriters.Name = "m_txtWriters";
            this.m_txtWriters.Size = new System.Drawing.Size(375, 20);
            this.m_txtWriters.TabIndex = 14;
            this.m_txtWriters.TextLabel = "Writers";
            // 
            // m_txtCast
            // 
            this.m_txtCast.AccessibleDescription = "Cast: A comma-delimited list of actor/actress names.";
            this.m_txtCast.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtCast.Location = new System.Drawing.Point(247, 387);
            this.m_txtCast.Name = "m_txtCast";
            this.m_txtCast.Size = new System.Drawing.Size(375, 20);
            this.m_txtCast.TabIndex = 15;
            this.m_txtCast.TextLabel = "Cast";
            // 
            // m_grpSeriesEpisode
            // 
            this.m_grpSeriesEpisode.Controls.Add(this.m_grpEpisode);
            this.m_grpSeriesEpisode.Controls.Add(this.m_grpSeries);
            this.m_grpSeriesEpisode.Location = new System.Drawing.Point(119, 225);
            this.m_grpSeriesEpisode.Name = "m_grpSeriesEpisode";
            this.m_grpSeriesEpisode.Size = new System.Drawing.Size(121, 65);
            this.m_grpSeriesEpisode.TabIndex = 52;
            this.m_grpSeriesEpisode.TabStop = false;
            // 
            // m_grpEpisode
            // 
            this.m_grpEpisode.Controls.Add(this.m_lblEpisode);
            this.m_grpEpisode.Controls.Add(this.m_numEpisode);
            this.m_grpEpisode.Controls.Add(this.m_lblSeason);
            this.m_grpEpisode.Controls.Add(this.m_numSeason);
            this.m_grpEpisode.Location = new System.Drawing.Point(0, 0);
            this.m_grpEpisode.Name = "m_grpEpisode";
            this.m_grpEpisode.Size = new System.Drawing.Size(121, 65);
            this.m_grpEpisode.TabIndex = 0;
            this.m_grpEpisode.TabStop = false;
            this.m_grpEpisode.Text = "Episode";
            // 
            // m_lblEpisode
            // 
            this.m_lblEpisode.AccessibleDescription = "The episode number within the season. Episode numbers\\nmust be unique within the " +
    "season. Episode numbers\\nnormally start at 1 but specials may have a number < 1." +
    "";
            this.m_lblEpisode.AutoSize = true;
            this.m_lblEpisode.Location = new System.Drawing.Point(66, 20);
            this.m_lblEpisode.Name = "m_lblEpisode";
            this.m_lblEpisode.Size = new System.Drawing.Size(45, 13);
            this.m_lblEpisode.TabIndex = 3;
            this.m_lblEpisode.Text = "Episode";
            // 
            // m_numEpisode
            // 
            this.m_numEpisode.AccessibleDescription = "The episode number within the season. Episode numbers\\nmust be unique within the " +
    "season. Episode numbers\\nnormally start at 1 but specials may have a number < 1." +
    "";
            this.m_numEpisode.Location = new System.Drawing.Point(69, 35);
            this.m_numEpisode.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.m_numEpisode.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            -2147483648});
            this.m_numEpisode.Name = "m_numEpisode";
            this.m_numEpisode.Size = new System.Drawing.Size(40, 20);
            this.m_numEpisode.TabIndex = 2;
            this.m_numEpisode.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // m_lblSeason
            // 
            this.m_lblSeason.AccessibleDescription = "The season that this episode was from.";
            this.m_lblSeason.AutoSize = true;
            this.m_lblSeason.Location = new System.Drawing.Point(7, 20);
            this.m_lblSeason.Name = "m_lblSeason";
            this.m_lblSeason.Size = new System.Drawing.Size(43, 13);
            this.m_lblSeason.TabIndex = 1;
            this.m_lblSeason.Text = "Season";
            // 
            // m_numSeason
            // 
            this.m_numSeason.AccessibleDescription = "The season that this episode was from.";
            this.m_numSeason.Location = new System.Drawing.Point(10, 34);
            this.m_numSeason.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.m_numSeason.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_numSeason.Name = "m_numSeason";
            this.m_numSeason.Size = new System.Drawing.Size(40, 20);
            this.m_numSeason.TabIndex = 0;
            this.m_numSeason.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // m_grpSeries
            // 
            this.m_grpSeries.Controls.Add(this.m_lblEpisodeCount);
            this.m_grpSeries.Controls.Add(this.m_numEpisodeCount);
            this.m_grpSeries.Location = new System.Drawing.Point(0, 0);
            this.m_grpSeries.Name = "m_grpSeries";
            this.m_grpSeries.Size = new System.Drawing.Size(121, 65);
            this.m_grpSeries.TabIndex = 38;
            this.m_grpSeries.TabStop = false;
            this.m_grpSeries.Text = "Series";
            // 
            // m_lblEpisodeCount
            // 
            this.m_lblEpisodeCount.AccessibleDescription = "A series is special in that the series does not have a video\\nassociated with it." +
    " Only multiple episodes. This is a count\\nof episodes within the series.";
            this.m_lblEpisodeCount.AutoSize = true;
            this.m_lblEpisodeCount.Location = new System.Drawing.Point(24, 20);
            this.m_lblEpisodeCount.Margin = new System.Windows.Forms.Padding(0);
            this.m_lblEpisodeCount.Name = "m_lblEpisodeCount";
            this.m_lblEpisodeCount.Size = new System.Drawing.Size(76, 13);
            this.m_lblEpisodeCount.TabIndex = 3;
            this.m_lblEpisodeCount.Text = "Episode Count";
            // 
            // m_numEpisodeCount
            // 
            this.m_numEpisodeCount.AccessibleDescription = "A series is special in that the series does not have a video\\nassociated with it." +
    " Only multiple episodes. This is a count\\nof episodes within the series.";
            this.m_numEpisodeCount.Location = new System.Drawing.Point(27, 35);
            this.m_numEpisodeCount.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.m_numEpisodeCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_numEpisodeCount.Name = "m_numEpisodeCount";
            this.m_numEpisodeCount.Size = new System.Drawing.Size(68, 20);
            this.m_numEpisodeCount.TabIndex = 2;
            this.m_numEpisodeCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // m_dtReleaseDate
            // 
            this.m_dtReleaseDate.AccessibleDescription = "The date this movie was released to theaters.\\nIf this is a home movie, the date " +
    "it was created.";
            this.m_dtReleaseDate.CustomFormat = "MM/dd/yyyy";
            this.m_dtReleaseDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.m_dtReleaseDate.Location = new System.Drawing.Point(247, 74);
            this.m_dtReleaseDate.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.m_dtReleaseDate.Name = "m_dtReleaseDate";
            this.m_dtReleaseDate.Size = new System.Drawing.Size(101, 20);
            this.m_dtReleaseDate.TabIndex = 5;
            this.m_dtReleaseDate.Value = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.m_dtReleaseDate.Leave += new System.EventHandler(this.JustHideError_Leave);
            // 
            // m_txtImdbUrl
            // 
            this.m_txtImdbUrl.AccessibleDescription = "The IMDB url that this video refers to. If unchecked, then this\\nis a video not a" +
    "vailable on IMDB such as home movies.";
            this.m_txtImdbUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtImdbUrl.BackColor = System.Drawing.SystemColors.Window;
            this.m_txtImdbUrl.Enabled = false;
            this.m_txtImdbUrl.Location = new System.Drawing.Point(261, 102);
            this.m_txtImdbUrl.Name = "m_txtImdbUrl";
            this.m_txtImdbUrl.Size = new System.Drawing.Size(339, 20);
            this.m_txtImdbUrl.TabIndex = 9;
            this.m_txtImdbUrl.TextLabel = "IMDB Url";
            this.m_txtImdbUrl.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_txtImdbUrl_DragDrop);
            this.m_txtImdbUrl.DragEnter += new System.Windows.Forms.DragEventHandler(this.m_txtImdbUrl_DragEnter);
            this.m_txtImdbUrl.Leave += new System.EventHandler(this.m_txtImdbUrl_Leave);
            this.m_txtImdbUrl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TextBoxRun_MouseDoubleClick);
            // 
            // m_chkImdbUrl
            // 
            this.m_chkImdbUrl.AccessibleDescription = "The IMDB url that this video refers to. If unchecked, then this\\nis a video not a" +
    "vailable on IMDB such as home movies.";
            this.m_chkImdbUrl.AutoSize = true;
            this.m_chkImdbUrl.Location = new System.Drawing.Point(247, 105);
            this.m_chkImdbUrl.Name = "m_chkImdbUrl";
            this.m_chkImdbUrl.Size = new System.Drawing.Size(15, 14);
            this.m_chkImdbUrl.TabIndex = 8;
            this.m_chkImdbUrl.UseVisualStyleBackColor = true;
            this.m_chkImdbUrl.CheckedChanged += new System.EventHandler(this.m_chkImdbUrl_CheckedChanged);
            // 
            // m_grpWatched
            // 
            this.m_grpWatched.AccessibleDescription = "The date that this video was last viewed.";
            this.m_grpWatched.Controls.Add(this.m_grpWatchedDt);
            this.m_grpWatched.Controls.Add(this.m_chkWatched);
            this.m_grpWatched.Location = new System.Drawing.Point(119, 294);
            this.m_grpWatched.Name = "m_grpWatched";
            this.m_grpWatched.Size = new System.Drawing.Size(122, 54);
            this.m_grpWatched.TabIndex = 16;
            this.m_grpWatched.TabStop = false;
            // 
            // m_grpWatchedDt
            // 
            this.m_grpWatchedDt.Controls.Add(this.m_dtWatched);
            this.m_grpWatchedDt.Location = new System.Drawing.Point(10, 17);
            this.m_grpWatchedDt.Margin = new System.Windows.Forms.Padding(0);
            this.m_grpWatchedDt.Name = "m_grpWatchedDt";
            this.m_grpWatchedDt.Padding = new System.Windows.Forms.Padding(0);
            this.m_grpWatchedDt.Size = new System.Drawing.Size(101, 29);
            this.m_grpWatchedDt.TabIndex = 2;
            this.m_grpWatchedDt.TabStop = false;
            // 
            // m_dtWatched
            // 
            this.m_dtWatched.AccessibleDescription = "The date that this video was last viewed.";
            this.m_dtWatched.CustomFormat = "MM/dd/yyyy";
            this.m_dtWatched.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.m_dtWatched.Location = new System.Drawing.Point(0, 7);
            this.m_dtWatched.Margin = new System.Windows.Forms.Padding(0);
            this.m_dtWatched.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.m_dtWatched.Name = "m_dtWatched";
            this.m_dtWatched.Size = new System.Drawing.Size(101, 20);
            this.m_dtWatched.TabIndex = 2;
            this.m_dtWatched.Value = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            // 
            // m_chkWatched
            // 
            this.m_chkWatched.AccessibleDescription = "Flag showing that this video was viewed.";
            this.m_chkWatched.AutoSize = true;
            this.m_chkWatched.Location = new System.Drawing.Point(6, 0);
            this.m_chkWatched.Name = "m_chkWatched";
            this.m_chkWatched.Size = new System.Drawing.Size(70, 17);
            this.m_chkWatched.TabIndex = 0;
            this.m_chkWatched.Text = "Watched";
            this.m_chkWatched.UseVisualStyleBackColor = true;
            this.m_chkWatched.CheckedChanged += new System.EventHandler(this.m_chkWatched_CheckedChanged);
            // 
            // m_grpExtractedVidProps
            // 
            this.m_grpExtractedVidProps.AccessibleDescription = "Cached video file properties. If\\nthe video file has been changed,\\nthese should " +
    "be recomputed.";
            this.m_grpExtractedVidProps.Controls.Add(this.m_grpVerticalDivider);
            this.m_grpExtractedVidProps.Controls.Add(this.m_lnkRecompute);
            this.m_grpExtractedVidProps.Controls.Add(this.m_lblDimensions);
            this.m_grpExtractedVidProps.Controls.Add(this.m_lblAspectRatio);
            this.m_grpExtractedVidProps.Controls.Add(this.m_lblRuntime);
            this.m_grpExtractedVidProps.Controls.Add(this.m_lblDownloadDate);
            this.m_grpExtractedVidProps.Controls.Add(this.m_lblDimensionsLabel);
            this.m_grpExtractedVidProps.Controls.Add(this.m_lblAspectRatioLabel);
            this.m_grpExtractedVidProps.Controls.Add(this.m_lblRuntimeLabel);
            this.m_grpExtractedVidProps.Controls.Add(this.m_lblDownloadDateLabel);
            this.m_grpExtractedVidProps.Location = new System.Drawing.Point(119, 415);
            this.m_grpExtractedVidProps.Name = "m_grpExtractedVidProps";
            this.m_grpExtractedVidProps.Size = new System.Drawing.Size(379, 63);
            this.m_grpExtractedVidProps.TabIndex = 17;
            this.m_grpExtractedVidProps.TabStop = false;
            this.m_grpExtractedVidProps.Text = "Extracted Video Properties";
            // 
            // m_grpVerticalDivider
            // 
            this.m_grpVerticalDivider.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.m_grpVerticalDivider.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_grpVerticalDivider.Location = new System.Drawing.Point(205, 15);
            this.m_grpVerticalDivider.Margin = new System.Windows.Forms.Padding(0);
            this.m_grpVerticalDivider.Name = "m_grpVerticalDivider";
            this.m_grpVerticalDivider.Padding = new System.Windows.Forms.Padding(0);
            this.m_grpVerticalDivider.Size = new System.Drawing.Size(2, 38);
            this.m_grpVerticalDivider.TabIndex = 9;
            this.m_grpVerticalDivider.TabStop = false;
            // 
            // m_lnkRecompute
            // 
            this.m_lnkRecompute.AccessibleDescription = "Re-extract video\\nfile properties.";
            this.m_lnkRecompute.AutoSize = true;
            this.m_lnkRecompute.LinkColor = System.Drawing.Color.Navy;
            this.m_lnkRecompute.Location = new System.Drawing.Point(309, 0);
            this.m_lnkRecompute.Name = "m_lnkRecompute";
            this.m_lnkRecompute.Size = new System.Drawing.Size(62, 13);
            this.m_lnkRecompute.TabIndex = 0;
            this.m_lnkRecompute.TabStop = true;
            this.m_lnkRecompute.Text = "Recompute";
            this.m_lnkRecompute.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_lnkRecompute_LinkClicked);
            // 
            // m_lblDimensions
            // 
            this.m_lblDimensions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lblDimensions.AutoSize = true;
            this.m_lblDimensions.Location = new System.Drawing.Point(280, 39);
            this.m_lblDimensions.Name = "m_lblDimensions";
            this.m_lblDimensions.Size = new System.Drawing.Size(95, 13);
            this.m_lblDimensions.TabIndex = 7;
            this.m_lblDimensions.Text = "1280 x 1024 pixels";
            // 
            // m_lblAspectRatio
            // 
            this.m_lblAspectRatio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lblAspectRatio.AutoSize = true;
            this.m_lblAspectRatio.Location = new System.Drawing.Point(280, 20);
            this.m_lblAspectRatio.Name = "m_lblAspectRatio";
            this.m_lblAspectRatio.Size = new System.Drawing.Size(28, 13);
            this.m_lblAspectRatio.TabIndex = 6;
            this.m_lblAspectRatio.Text = "16:9";
            // 
            // m_lblRuntime
            // 
            this.m_lblRuntime.AutoSize = true;
            this.m_lblRuntime.Location = new System.Drawing.Point(86, 39);
            this.m_lblRuntime.Name = "m_lblRuntime";
            this.m_lblRuntime.Size = new System.Drawing.Size(94, 13);
            this.m_lblRuntime.TabIndex = 5;
            this.m_lblRuntime.Text = "121 minutes (2:01)";
            // 
            // m_lblDownloadDate
            // 
            this.m_lblDownloadDate.AutoSize = true;
            this.m_lblDownloadDate.Location = new System.Drawing.Point(86, 20);
            this.m_lblDownloadDate.Margin = new System.Windows.Forms.Padding(0);
            this.m_lblDownloadDate.Name = "m_lblDownloadDate";
            this.m_lblDownloadDate.Size = new System.Drawing.Size(112, 13);
            this.m_lblDownloadDate.TabIndex = 4;
            this.m_lblDownloadDate.Text = "12/24/2020 12:31 pm";
            // 
            // m_lblDimensionsLabel
            // 
            this.m_lblDimensionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lblDimensionsLabel.AutoSize = true;
            this.m_lblDimensionsLabel.Location = new System.Drawing.Point(214, 39);
            this.m_lblDimensionsLabel.Name = "m_lblDimensionsLabel";
            this.m_lblDimensionsLabel.Size = new System.Drawing.Size(64, 13);
            this.m_lblDimensionsLabel.TabIndex = 3;
            this.m_lblDimensionsLabel.Text = "Dimensions:";
            // 
            // m_lblAspectRatioLabel
            // 
            this.m_lblAspectRatioLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lblAspectRatioLabel.AutoSize = true;
            this.m_lblAspectRatioLabel.Location = new System.Drawing.Point(214, 20);
            this.m_lblAspectRatioLabel.Name = "m_lblAspectRatioLabel";
            this.m_lblAspectRatioLabel.Size = new System.Drawing.Size(71, 13);
            this.m_lblAspectRatioLabel.TabIndex = 2;
            this.m_lblAspectRatioLabel.Text = "Aspect Ratio:";
            // 
            // m_lblRuntimeLabel
            // 
            this.m_lblRuntimeLabel.AutoSize = true;
            this.m_lblRuntimeLabel.Location = new System.Drawing.Point(7, 39);
            this.m_lblRuntimeLabel.Name = "m_lblRuntimeLabel";
            this.m_lblRuntimeLabel.Size = new System.Drawing.Size(49, 13);
            this.m_lblRuntimeLabel.TabIndex = 1;
            this.m_lblRuntimeLabel.Text = "Runtime:";
            // 
            // m_lblDownloadDateLabel
            // 
            this.m_lblDownloadDateLabel.AutoSize = true;
            this.m_lblDownloadDateLabel.Location = new System.Drawing.Point(7, 20);
            this.m_lblDownloadDateLabel.Name = "m_lblDownloadDateLabel";
            this.m_lblDownloadDateLabel.Size = new System.Drawing.Size(84, 13);
            this.m_lblDownloadDateLabel.TabIndex = 0;
            this.m_lblDownloadDateLabel.Text = "Download Date:";
            // 
            // m_btnSelectMovieFile
            // 
            this.m_btnSelectMovieFile.AccessibleDescription = "Select movie filename.";
            this.m_btnSelectMovieFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnSelectMovieFile.Image = global::MovieFolderPrep.Properties.Resources.File_Video_16;
            this.m_btnSelectMovieFile.Location = new System.Drawing.Point(592, 12);
            this.m_btnSelectMovieFile.Name = "m_btnSelectMovieFile";
            this.m_btnSelectMovieFile.Size = new System.Drawing.Size(22, 22);
            this.m_btnSelectMovieFile.TabIndex = 0;
            this.m_btnSelectMovieFile.UseVisualStyleBackColor = true;
            this.m_btnSelectMovieFile.Click += new System.EventHandler(this.m_btnSelectMovieFile_Click);
            // 
            // m_txtMoviePath
            // 
            this.m_txtMoviePath.AccessibleDescription = "Full name of movie or series folder to configure. Add filename\\nor folder via cli" +
    "ck-n-drag, or using open file dialog to the right.";
            this.m_txtMoviePath.AllowDrop = true;
            this.m_txtMoviePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtMoviePath.ForeColor = System.Drawing.SystemColors.ControlText;
            this.m_txtMoviePath.Location = new System.Drawing.Point(12, 13);
            this.m_txtMoviePath.Name = "m_txtMoviePath";
            this.m_txtMoviePath.Size = new System.Drawing.Size(580, 20);
            this.m_txtMoviePath.TabIndex = 2;
            this.m_txtMoviePath.TextLabel = "Select video file or folder...";
            this.m_txtMoviePath.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_txtMoviePath_DragDrop);
            this.m_txtMoviePath.DragEnter += new System.Windows.Forms.DragEventHandler(this.m_txtMoviePath_DragEnter);
            this.m_txtMoviePath.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.m_txtMoviePath_KeyPress);
            // 
            // ManualMovieConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(647, 558);
            this.Controls.Add(this.m_btnSelectMovieFile);
            this.Controls.Add(this.m_btnSelectMovieFolder);
            this.Controls.Add(this.m_txtMoviePath);
            this.Controls.Add(this.m_btnSave);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_pnlAllProperties);
            this.Icon = global::MovieFolderPrep.Properties.Resources.favicon;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(9999, 597);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(540, 597);
            this.Name = "ManualMovieConfig";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manual Movie Configuration";
            this.m_pnlAllProperties.ResumeLayout(false);
            this.m_pnlAllProperties.PerformLayout();
            this.m_grpVideoType.ResumeLayout(false);
            this.m_grpGenre.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_numRating)).EndInit();
            this.m_grpSeriesEpisode.ResumeLayout(false);
            this.m_grpEpisode.ResumeLayout(false);
            this.m_grpEpisode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_numEpisode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_numSeason)).EndInit();
            this.m_grpSeries.ResumeLayout(false);
            this.m_grpSeries.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_numEpisodeCount)).EndInit();
            this.m_grpWatched.ResumeLayout(false);
            this.m_grpWatched.PerformLayout();
            this.m_grpWatchedDt.ResumeLayout(false);
            this.m_grpExtractedVidProps.ResumeLayout(false);
            this.m_grpExtractedVidProps.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button m_btnSelectMovieFolder;
        private MovieFolderPrep.LabeledTextBox m_txtMoviePath;
        private System.Windows.Forms.Button m_btnSave;
        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.Panel m_pnlAllProperties;
        private System.Windows.Forms.GroupBox m_grpVideoType;
        private System.Windows.Forms.CheckedListBox m_clbVideoType;
        private System.Windows.Forms.GroupBox m_grpGenre;
        private System.Windows.Forms.CheckedListBox m_clbGenre;
        private LabeledTextBox m_txtMovieName;
        private LabeledTextBox m_txtEpisodeName;
        private System.Windows.Forms.Label m_lblReleaseDate;
        private System.Windows.Forms.Label m_lblRating;
        private System.Windows.Forms.NumericUpDown m_numRating;
        private LabeledTextBox m_txtPosterUrl;
        private LabeledTextBox m_txtPlot;
        private LabeledTextBox m_txtSummary;
        private LabeledTextBox m_txtDirectors;
        private LabeledTextBox m_txtWriters;
        private LabeledTextBox m_txtCast;
        private System.Windows.Forms.GroupBox m_grpSeriesEpisode;
        private System.Windows.Forms.GroupBox m_grpEpisode;
        private System.Windows.Forms.Label m_lblEpisode;
        private System.Windows.Forms.NumericUpDown m_numEpisode;
        private System.Windows.Forms.Label m_lblSeason;
        private System.Windows.Forms.NumericUpDown m_numSeason;
        private System.Windows.Forms.GroupBox m_grpSeries;
        private System.Windows.Forms.Label m_lblEpisodeCount;
        private System.Windows.Forms.NumericUpDown m_numEpisodeCount;
        private System.Windows.Forms.DateTimePicker m_dtReleaseDate;
        private System.Windows.Forms.CheckBox m_chkImdbUrl;
        private LabeledTextBox m_txtImdbUrl;
        private System.Windows.Forms.GroupBox m_grpWatched;
        private System.Windows.Forms.GroupBox m_grpWatchedDt;
        private System.Windows.Forms.DateTimePicker m_dtWatched;
        private System.Windows.Forms.CheckBox m_chkWatched;
        private System.Windows.Forms.GroupBox m_grpExtractedVidProps;
        private System.Windows.Forms.LinkLabel m_lnkRecompute;
        private System.Windows.Forms.Label m_lblDimensions;
        private System.Windows.Forms.Label m_lblAspectRatio;
        private System.Windows.Forms.Label m_lblRuntime;
        private System.Windows.Forms.Label m_lblDownloadDate;
        private System.Windows.Forms.Label m_lblDimensionsLabel;
        private System.Windows.Forms.Label m_lblAspectRatioLabel;
        private System.Windows.Forms.Label m_lblRuntimeLabel;
        private System.Windows.Forms.Label m_lblDownloadDateLabel;
        private System.Windows.Forms.Button m_btnSelectMovieFile;
        private LabeledTextBox m_txtCreators;
        private System.Windows.Forms.GroupBox m_grpVerticalDivider;
        private System.Windows.Forms.Button m_btnDownloadWebpage;
        private System.Windows.Forms.Button m_btnDownloadPoster;
    }
}