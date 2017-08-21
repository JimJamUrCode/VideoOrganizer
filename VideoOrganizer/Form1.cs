using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Deployment.Application;

namespace VideoOrganizer
{
    public partial class VideoOrganizerForm : Form
    {
        MediaOrganizer myOrganizer;
        List<string> userDirectories;
        private string trailerURL;
        private string SortedBy;
        private bool Catalogued;

        #region Constructors

            public VideoOrganizerForm()
            {
                InitializeComponent();

                //Readjusting the window size
                this.MaximumSize = new System.Drawing.Size(233, 450);
                this.MinimumSize = new System.Drawing.Size(233, 450);
                this.ClientSize = new System.Drawing.Size(233, 450);
                try
                {
                    this.Text = "Video Organizer " + ApplicationDeployment.CurrentDeployment.CurrentVersion;
                }
                catch (Exception ex)
                {
                    this.Text = "Video Organizer...This is not an installed version";
                }
                //Showing and updating the main fram
                this.Show();
                this.Update();

                //Initializing Class Variables
                myOrganizer = new MediaOrganizer();
                userDirectories = new List<string>();
                trailerURL = "";
                SortedBy = "Ascending Alphabetical";
                Catalogued = false;
            
                //Creating directories to use for application files
                string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings";
                string localDatabaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\Index\\Local Database";
                string localDatabaseCoversDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\Index\\Local Database\\Covers";
                string localDatabaseInformationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\Index\\Local Database\\Information";
                string htmlIndexDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\Index";

                Directory.CreateDirectory(directory);
                Directory.CreateDirectory(htmlIndexDirectory);
                Directory.CreateDirectory(localDatabaseDirectory);
                Directory.CreateDirectory(localDatabaseCoversDirectory);
                Directory.CreateDirectory(localDatabaseInformationDirectory);
            
                //Auto-Loading the directories
                ProgressLbl.Text = "Auto-Loading Directories";
                this.Update();
                AutoLoad();
                ProgressLbl.Text = "Done Auto-Loading.";
            }

        #endregion

        #region Component Event Handlers

            #region Button Events
            private void AddBtn_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();

            //Making sure the user seleceted a path
            if (folderBrowserDialog1.SelectedPath != "")
            {
                bool response = myOrganizer.AddDirectory(folderBrowserDialog1.SelectedPath, false);
                if (!response)
                {
                    int choice = (int)MessageBox.Show("Make sure this directory has not already been added", "Duplicate Resource!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Question);
                    if (choice == 4)
                    {
                        AddBtn_Click(sender, e);
                    }
                }
                else
                {
                    userDirectories.Add(folderBrowserDialog1.SelectedPath);
                }
                UpdateMovieList();

                var properties = new Dictionary<string, object>();
                properties["time"] = DateTime.Now;
                myOrganizer.trackAnEvent("Added a Directory", properties);
            }
        }

            private void SaveBtn_Click(object sender, EventArgs e)
            {
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.CreatePrompt = true;
                saveFileDialog1.OverwritePrompt = true;
                saveFileDialog1.DefaultExt = ".txt";
                saveFileDialog1.ShowDialog();

                if (saveFileDialog1.FileName != "")
                {
                    StreamWriter writer = new StreamWriter(saveFileDialog1.FileName, false);

                    for (int i = 0; i < userDirectories.Count; i++)
                    {
                        writer.WriteLine(userDirectories[i]);
                    }

                    writer.Close();


                    //Ask user if they want to enable auto-load for the directories they just saved
                    enableAutoLoad(saveFileDialog1.FileName);
                }

                saveFileDialog1.Dispose();

                var properties = new Dictionary<string, object>();
                properties["time"] = DateTime.Now;
                myOrganizer.trackAnEvent("Saved Directory List", properties);
            }
    
            private void OpenInExplorerBtn_Click(object sender, EventArgs e)
            {
                try
                {
                    //process.Start(DirectoryTB.Text,  @"/select, " + filePath;);
                    Process.Start("explorer.exe", @"/select, " + DirectoryLinkLabel.Text);
                    var properties = new Dictionary<string, object>();
                    properties["time"] = DateTime.Now;
                    myOrganizer.trackAnEvent("Opened Directory To File", properties);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No directory to open...\nAdd a directory and select a file to\nauto fill the directory text box.", "Need to Select a File!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            private void LoadBtn_Click(object sender, EventArgs e)
            {
                openFileDialog1.ShowDialog();
                try
                {
                    StreamReader reader = new StreamReader(openFileDialog1.FileName, false);

                    string buffer = reader.ReadLine();
                    while (buffer != null)
                    {
                        myOrganizer.AddDirectory(buffer, false);
                        userDirectories.Add(buffer);
                        buffer = reader.ReadLine();
                    }

                    reader.Close();

                    UpdateMovieList();

                    var properties = new Dictionary<string, object>();
                    properties["time"] = DateTime.Now;
                    myOrganizer.trackAnEvent("Load File of Directories", properties);
                }
                catch (Exception ex)
                {
                    //No folder selected, user probably closed the dialog box or pressed cancel
                }
            }

            private void ClearDirectoriesBtn_Click(object sender, EventArgs e)
            {
                myOrganizer.clearDirectories();
                myOrganizer.clearFiles();
                userDirectories.Clear();
                listBox1.Items.Clear();

                //Readjusting the window size
                this.MaximumSize = new System.Drawing.Size(233, 450);
                this.MinimumSize = new System.Drawing.Size(233, 450);
                this.ClientSize = new System.Drawing.Size(233, 450);

                var properties = new Dictionary<string, object>();
                properties["time"] = DateTime.Now;
                myOrganizer.trackAnEvent("Clear Directories", properties);
            }

            private void FindBtn_Click(object sender, EventArgs e)
            {
                if (userDirectories.Count != 0)
                {                    
                    FindForm inputForm = new FindForm();
                    //inputForm
                    inputForm.ShowDialog();
                    this.Focus();
                    try
                    {
                        string userInput = inputForm.getInput();
                        List<FileDetails> results = myOrganizer.SearchAllFiles(userInput);

                        var properties = new Dictionary<string, object>();
                        properties["time"] = DateTime.Now;
                        properties["Number of results"] = results.Count;
                        properties["User Input"] = userInput;
                        properties["Error"] = "";
                        myOrganizer.trackAnEvent("Database Search Performed", properties);

                        if (userInput == "")
                        {
                            MessageBox.Show("You must enter a search term to search for a movie", "Nothing to search for", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                            FindBtn_Click(sender, e);
                        }
                        else if (results != null && results.Count > 1)
                        {
                            
                            ResultsForm resultsForm = new ResultsForm(results);
                            resultsForm.ShowDialog();
                            this.Focus();
                            FileDetails userSelectedResult = resultsForm.getSelectedIndex();

                            if (userSelectedResult != null)
                            {
                                UpdateAllInformation(userSelectedResult);
                                List<FileDetails> loadedMovies = myOrganizer.getList();
                                int index = listBox1.Items.IndexOf(userSelectedResult.fileName);
                                listBox1.SelectedIndex = index;
                            }
                        }
                        else if (results.Count == 1)
                        {
                            if (results != null)
                            {
                                UpdateAllInformation(results[0]);
                                List<FileDetails> loadedMovies = myOrganizer.getList();
                                int index = listBox1.Items.IndexOf(results[0].fileName);
                                listBox1.SelectedIndex = index;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Your search did not yeild any results", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                        }
                    }
                    catch (Exception ex)
                    {
                        var properties = new Dictionary<string, object>();
                        properties["time"] = DateTime.Now;
                        properties["Number of results"] = "";
                        properties["User Input"] = "";
                        properties["Error Method"] = "FindBtn_Click";
                        properties["Exception"] = ex.ToString();
                        myOrganizer.trackAnEvent("Error", properties);
                        //MessageBox.Show("No file name entered", "Missing File Name", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }
                }
                else
                {
                    MessageBox.Show("Directories need to be loaded before you can search.", "Load Directories", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }
            }

            private void ExitBtn_Click(object sender, EventArgs e)
            {
                var properties = new Dictionary<string, object>();
                properties["time"] = DateTime.Now;
                myOrganizer.trackAnEvent("Application Closed Gracefully", properties);
                Close();
            }

            private void IndexBtn_Click(object sender, EventArgs e)
            {
                listBox1.setSortBy(0);//back to the first if
                SortedBy = "Ascending Alphabetically";

                listBox1.SetListBoxContents(myOrganizer.getList());

                myOrganizer.CreateHTMLIndex(progressBar1, ProgressLbl, Catalogued);
                
            }

            private void SortByBtn_Click(object sender, EventArgs e)
            {

                listBox1.Sorted = false;

                if (listBox1.getSortBy() == 0)
                {
                    listBox1.setSortBy(1);
                    SortedBy = "Descending Alphabetically";
                }
                else if (listBox1.getSortBy() == 1)
                {
                    listBox1.setSortBy(2);
                    SortedBy = "Ascending Date";
                }
                else if (listBox1.getSortBy() == 2)
                {
                    listBox1.setSortBy(3);//to the next if next time
                    SortedBy = "Descending Date";
                }
                else if (listBox1.getSortBy() == 3)
                {
                    listBox1.setSortBy(4);//back to the first if
                    SortedBy = "Ascending Size";
                }
                else if (listBox1.getSortBy() == 4)
                {
                    listBox1.setSortBy(5);//to the next if next time
                    SortedBy = "Descending Size";
                }
                else if (listBox1.getSortBy() == 5)
                {
                    listBox1.setSortBy(6);//back to the first if
                    SortedBy = "Ascending File Type";
                }
                else if (listBox1.getSortBy() == 6)
                {
                    listBox1.setSortBy(7);//to the next if next time
                    SortedBy = "Descending File Type";
                }
                else if (listBox1.getSortBy() == 7)
                {
                    listBox1.setSortBy(0);//back to the first if
                    SortedBy = "Ascending Alphabetically";
                }

                listBox1.SetListBoxContents(myOrganizer.getList());
                LibraryInformationListView.Items[6].SubItems[1].Text = SortedBy;
                LibraryInformationListView.Update();

                var properties = new Dictionary<string, object>();
                properties["time"] = DateTime.Now;
                properties["Sorted By"] = SortedBy;
                myOrganizer.trackAnEvent("Sorting", properties);
            }

            private void LibraryInfoBtn_Click(object sender, EventArgs e)
            {
                if (pictureBox1.Width == 98)
                {
                    pictureBox1.Width = 210;
                    pictureBox1.Height = 310;
                    pictureBox1.Location = new Point(224, 97);

                    LibraryInformationListView.Visible = false;

                    LibraryInformationLbl.Visible = false;
                    TrailerInfoLbl.Visible = false;
                }
                else
                {
                    pictureBox1.Width = 98;
                    pictureBox1.Height = 152;
                    pictureBox1.Location = new Point(278, 97);

                    LibraryInformationListView.Visible = true;

                    LibraryInformationLbl.Visible = true;
                    TrailerInfoLbl.Visible = true;
                }
                this.Update();

                var properties = new Dictionary<string, object>();
                properties["time"] = DateTime.Now;
                myOrganizer.trackAnEvent("Info Panel Opened", properties);
            }
            #endregion

            #region DoubleClick Events
            private void listBox1_DoubleClick(object sender, EventArgs e)
            {
                UpdateAllInformation(myOrganizer.SearchForExactMatch(listBox1.SelectedItem.ToString()));
                string selectedText = listBox1.SelectedItem.ToString();
                Clipboard.SetText(selectedText);
            }

            private void LibraryInformationListView_DoubleClick(object sender, EventArgs e)
            {
                ListView.SelectedListViewItemCollection selectedItem = LibraryInformationListView.SelectedItems;
                string selectedText = selectedItem[0].SubItems[1].Text;
                Clipboard.SetText(selectedText);
            }

            private void DescriptionrichTextBox1_DoubleClick(object sender, EventArgs e)
            {
                string selectedText = DescriptionrichTextBox1.Text;
                Clipboard.SetText(selectedText);
            }
            #endregion

            #region KeyPress Event
            private void listBox1_KepPress(object sender, EventArgs e)
            {
                listBox1_DoubleClick(sender, EventArgs.Empty);
            }
            #endregion

            #region Click Events
            private void pictureBox1_Click(object sender, EventArgs e)
            {
                FileDetails myFile = myOrganizer.SearchForExactMatch(FileNameLbl.Text);

                string url = myFile.trailerURL;
                Process.Start(trailerURL);
                var properties = new Dictionary<string, object>();
                properties["time"] = DateTime.Now;
                myOrganizer.trackAnEvent("Trailer Viewed", properties);
            }

            private void InternetTitleLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                IMDBParser myIMDBParser = new IMDBParser();
                FileDetails myFile = myOrganizer.SearchForExactMatch(FileNameLbl.Text);
                string url = "";
                if (!Catalogued)
                    url = "http://www.imdb.com/title/" + myIMDBParser.retreiveTitleID(myFile.fileName);
                else
                    url = myFile.IMDBURL;

                InternetTitleLinkLabel.LinkVisited = true;
                Process.Start(url);
                var properties = new Dictionary<string, object>();
                properties["time"] = DateTime.Now;
                myOrganizer.trackAnEvent("IMDB Site Visited", properties);
            }

            private void CreateDBBtn_Click(object sender, EventArgs e)
            {
                if (!Catalogued)
                {
                    ProgressLbl.Text = "Creating local database (this may take a while)...";
                    var properties = new Dictionary<string, object>();
                    properties["time"] = DateTime.Now;
                    myOrganizer.trackAnEvent("Database Created", properties);
                }
                else
                {
                    ProgressLbl.Text = "Updating local database...";
                    var properties = new Dictionary<string, object>();
                    properties["time"] = DateTime.Now;
                    myOrganizer.trackAnEvent("Database Updated", properties);
                }

                this.Update();
                string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings";
                List<string> settingsFile = new List<string>();

                Catalogued = myOrganizer.CreateDatabase(progressBar1, this);

                settingsFile.AddRange(File.ReadAllLines(directory + "\\settings.inf"));
                settingsFile[1] = "1";
                File.WriteAllLines(directory + "\\settings.inf", settingsFile.ToArray());

                ProgressLbl.Text = "Done Updating Database.";
                this.Update();
            }

            private void DirectoryLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                try
                {
                    //process.Start(DirectoryTB.Text,  @"/select, " + filePath;);
                    Process.Start("explorer.exe", @"/select, " + DirectoryLinkLabel.Text);
                    DirectoryLinkLabel.LinkVisited = true;
                    var properties = new Dictionary<string, object>();
                    properties["time"] = DateTime.Now;
                    myOrganizer.trackAnEvent("Opened Directory To File", properties);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No directory to open...\nAdd a directory and select a file to\nauto fill the directory text box.", "Need to Select a File!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            #endregion
            

        #endregion

        #region Utility Funcitons

            private void UpdateAllInformation(FileDetails movieName)
            {
                //Resetting progress bar values
                progressBar1.Value = 0;
                progressBar1.Maximum = 100;
                ProgressLbl.Text = "Updating: " + movieName.fileName + "...";
                this.Update();

                //Updating the file information labels
                UpdateVisualFileInformation(movieName);
                this.Update();
                progressBar1.Increment(5);

                //Updating the labels with new information
                UpdateVisualIMDBInformation(movieName, progressBar1);
                this.Update();

                //Readjusting the window size
                this.MaximumSize = new System.Drawing.Size(756, 450);
                this.MinimumSize = new System.Drawing.Size(756, 450);
                this.ClientSize = new System.Drawing.Size(756, 450);

                progressBar1.Increment(progressBar1.Maximum);
                ProgressLbl.Text = "Done.";
            }

            private void UpdateMovieList()
            {
                List<FileDetails> list = myOrganizer.getList();
                listBox1.BeginUpdate();

                listBox1.SetListBoxContents(list);
                
                listBox1.EndUpdate();

                UpdateVisualLibraryInformation();
            }

            private void UpdateVisualFileInformation(FileDetails myFileDetails)
            {
                //Changing the text in the labels
                FileNameLbl.Text = myFileDetails.fileName;
                FileSizeLbl.Text = myFileDetails.size + " MB";
                FileExtensionLbl.Text = myFileDetails.extension;
                FileReadOnlyLbl.Text = myFileDetails.readOnly.ToString();
                FileCreationDateLbl.Text = myFileDetails.creationDate.ToString();
                DirectoryLinkLabel.Text = myFileDetails.directory;
                DirectoryLinkLabel.LinkArea = new LinkArea(0, myFileDetails.directory.Length);
                DirectoryLinkLabel.LinkVisited = false;
            }

            private void UpdateVisualLibraryInformation()
            {
                LibraryInformationListView.Clear();

                List<FileDetails> mylist = new List<FileDetails>();
                mylist = myOrganizer.getList();
                int largestIndex = 0;
                int smallestIndex = 0;
                Int64 size = 0;
                for (int i = 0; i < mylist.Count; i++)
                {
                    size += mylist[i].size;
                    if (mylist[i].size > mylist[largestIndex].size)
                    {
                        largestIndex = i;
                    }
                    if (mylist[i].size < mylist[smallestIndex].size)
                    {
                        smallestIndex = i;
                    }
                }

                LibraryInformationListView.Columns.Add("Attributes", -2);
                LibraryInformationListView.Columns.Add("Details", -2);

                ListViewItem Size = new ListViewItem("Library Size",0);
                Size.SubItems.Add(size / 1024 + " GB");

                ListViewItem LargeFile = new ListViewItem("Largest File", 0);
                LargeFile.SubItems.Add(mylist[largestIndex].fileName + " " + mylist[largestIndex].extension);

                ListViewItem LargeFileSize = new ListViewItem("Largest File Size", 0);
                LargeFileSize.SubItems.Add(mylist[largestIndex].size + " MB");

                ListViewItem SmallFile = new ListViewItem("Smallest File", 0);
                SmallFile.SubItems.Add(mylist[smallestIndex].fileName + " " + mylist[smallestIndex].extension);

                ListViewItem SmallFileSize = new ListViewItem("Smallest File Size", 0);
                SmallFileSize.SubItems.Add(mylist[smallestIndex].size + " MB");

                ListViewItem NumberOfFiles = new ListViewItem("Number of Files", 0);
                NumberOfFiles.SubItems.Add("" + mylist.Count);

                ListViewItem SortedBy = new ListViewItem("Sorted By", 0);
                SortedBy.SubItems.Add("" + this.SortedBy);

                LibraryInformationListView.Items.Add(Size);
                LibraryInformationListView.Items.Add(LargeFile);
                LibraryInformationListView.Items.Add(LargeFileSize);
                LibraryInformationListView.Items.Add(SmallFile);
                LibraryInformationListView.Items.Add(SmallFileSize);
                LibraryInformationListView.Items.Add(NumberOfFiles);
                LibraryInformationListView.Items.Add(SortedBy);
                LibraryInformationListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }

            private void UpdateVisualIMDBInformation(FileDetails myFileDetails, ProgressBar progressBar1)
            {
                if(!Catalogued){
                    UpdateVisualIMDBInformationNotCatalogued(myFileDetails, progressBar1);
                }

                if (Catalogued)
                {
                    try
                    {
                        //Updating labels
                        InternetDirectorLbl.Text = myFileDetails.director;
                        InternetGenreLbl.Text = myFileDetails.genre;
                        InternetMPAARatingLbl.Text = myFileDetails.MPAARating;
                        DescriptionrichTextBox1.Text = myFileDetails.description;
                        InternetReviewScoreLbl.Text = myFileDetails.reviewScore;
                        InternetRuntimeLbl.Text = myFileDetails.runtime;
                        InternetTitleLinkLabel.Text = myFileDetails.internetTitle;
                        InternetTitleLinkLabel.LinkArea = new LinkArea(0, myFileDetails.internetTitle.Length);
                        InternetTitleLinkLabel.LinkVisited = false;
                        InternetYearLbl.Text = myFileDetails.internetYear;
                        trailerURL = myFileDetails.trailerURL;
                        pictureBox1.ImageLocation = myFileDetails.coverPath;
                    }
                    catch(Exception ex)
                    {
                        var properties = new Dictionary<string, object>();
                        properties["time"] = DateTime.Now;
                        properties["Error Method"] = "UpdateVisualIMDBInformation";
                        properties["Exception"] = ex.ToString();
                        myOrganizer.trackAnEvent("Error", properties);
                        UpdateVisualIMDBInformationNotCatalogued(myFileDetails, progressBar1);
                    }
                }
            }

            private void UpdateVisualIMDBInformationNotCatalogued(FileDetails myFileDetails, ProgressBar progressBar1)
            {
                //Creating the IMDB object to prepare for parsing
                IMDBParser myIMDBParser = new IMDBParser();
                progressBar1.Increment(5);

                //Updating the IMDB information
                myIMDBParser.retreiveTitleID(myFileDetails.fileName);
                progressBar1.Increment(20);
                myIMDBParser.retreiveMoviePageResponse();
                progressBar1.Increment(20);
                myIMDBParser.retreiveGenre();
                progressBar1.Increment(5);
                myIMDBParser.retreiveMetaScore();
                progressBar1.Increment(5);
                myIMDBParser.retreiveMPAARating();
                progressBar1.Increment(5);
                myIMDBParser.retreiveMovieDescription();
                myIMDBParser.retreiveRunTime();
                progressBar1.Increment(5);
                myIMDBParser.retreiveTitleAndYear();
                progressBar1.Increment(5);
                myIMDBParser.retreiveDirector();
                progressBar1.Increment(5);
                myIMDBParser.retreiveTrailerURL();
                progressBar1.Increment(5);

                //Updating labels
                InternetDirectorLbl.Text = myIMDBParser.getDirector();
                InternetGenreLbl.Text = myIMDBParser.getGenre();
                InternetMPAARatingLbl.Text = myIMDBParser.getMPAARating();
                DescriptionrichTextBox1.Text = myIMDBParser.getMovieDescription();
                InternetReviewScoreLbl.Text = myIMDBParser.getMetaScore();
                InternetRuntimeLbl.Text = myIMDBParser.getRunTime();
                InternetTitleLinkLabel.Text = myIMDBParser.getIMDBTitle();
                InternetTitleLinkLabel.LinkArea = new LinkArea(0, myIMDBParser.getIMDBTitle().Length);
                InternetTitleLinkLabel.LinkVisited = false;
                InternetYearLbl.Text = myIMDBParser.getYear();
                trailerURL = myIMDBParser.getTrailerURL();
                progressBar1.Increment(5);

                //Updating the image for the movie from IMDB
                try
                {
                    pictureBox1.Load(myIMDBParser.retreiveArtwork());
                    //pictureBox1.Load(myIMDBParser.retreiveFanArt());
                }
                catch (Exception ex)
                {
                    var properties2 = new Dictionary<string, object>();
                    properties2["time"] = DateTime.Now;
                    properties2["Error Method"] = "UpdateVisualIMDBInformationNotCatalogued";
                    properties2["Exception"] = ex.ToString();
                    myOrganizer.trackAnEvent("Error", properties2);
                    /*the movie response page within the imdb class was not created
                        * This may be because the search yeilded no results
                        */
                }
                progressBar1.Increment(5);
            }

            private void enableAutoLoad(string fileName)
            {
                int decision = (int)MessageBox.Show("Would you like to create an auto load file that corresponds to these directories?", "First Timer!", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

                if (decision == 6)
                {
                    if (fileName != "" && fileName != null)
                    {
                        //string directory = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                        string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\settings.inf";
                        string[] settingsFile = { "1 " + fileName, "0" };
                        File.WriteAllLines(directory, settingsFile);
                        var properties = new Dictionary<string, object>();
                        properties["time"] = DateTime.Now;
                        properties["Enabled"] = "Yes";
                        myOrganizer.trackAnEvent("Auto Save", properties);
                    }
                }
                else
                {
                    //user decieded not to save the auto-load file
                    var properties = new Dictionary<string, object>();
                    properties["time"] = DateTime.Now;
                    properties["Enabled"] = "No";
                    myOrganizer.trackAnEvent("Auto Save", properties);
                }
            }

            private void AutoLoad()
            {
                //Getting the my documents directory and creating a file reader
                string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings";
                List<string> autoLoad = new List<string>();
                string pathToSavedFile = "";

                try
                {
                    autoLoad.AddRange(File.ReadAllLines(directory + "\\settings.inf"));
                    var properties = new Dictionary<string, object>();
                    properties["First Start"] = "No";
                    properties["Version"] = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                    properties["time"] = DateTime.Now;
                    myOrganizer.trackAnEvent("Application Start", properties);
                }
                catch (FileNotFoundException ex)
                {
                    var properties = new Dictionary<string, object>();
                    properties["First Start"] = "Yes";
                    properties["Version"] = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                    properties["time"] = DateTime.Now;
                    myOrganizer.trackAnEvent("Application Start", properties);

                    this.MaximumSize = new System.Drawing.Size(240, 450);
                    MessageBox.Show("This is your first time running the program, we need to create a settings file", "First Timer!", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    string[] settings = new string[] { "0", "0" };
                    File.WriteAllLines(directory + "\\settings.inf", settings);
                }
                catch (Exception ex)
                {
                    var properties2 = new Dictionary<string, object>();
                    properties2["time"] = DateTime.Now;
                    properties2["Error Method"] = "AutoLoad";
                    properties2["Exception"] = ex.ToString();
                    myOrganizer.trackAnEvent("Error", properties2);
                    MessageBox.Show("An error occured while creating the settings file, continuing without it", "Settings file error");
                }

                if (autoLoad.Count > 0)//if there are settings to load
                {
                    #region AutoLoading Local Database Settings
                    try
                    {
                        if (autoLoad[1].ElementAt(0) == '1')
                            Catalogued = true;
                        else
                            Catalogued = false;
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        autoLoad.Add("0");
                        File.WriteAllLines(directory + "\\settings.inf", autoLoad.ToArray());
                    }
                    #endregion

                    #region AutoLoading Directories

                    if (autoLoad[0].ElementAt(0) == '1')
                    {
                        pathToSavedFile = autoLoad[0].Substring(2);
                        progressBar1.Maximum = 0;
                        progressBar1.Value = 0;
                        List<string> buffer = new List<string>();

                        try
                        {
                            buffer.AddRange(File.ReadAllLines(pathToSavedFile));
                        }
                        catch (FileNotFoundException ex)
                        {
                            MessageBox.Show("The path to the saved directories file has either been changed or deleted!\nRe-save the directories and allow auto-loading", "Settings file error");
                        }
                        catch (Exception generalException)
                        {
                            MessageBox.Show("An error occured while reading from the directories file,\ntry resaving your directories and allowing auto-load", "Settings file error");
                        }

                        if (buffer.Count > 0)//If directories exist to load
                        {
                            progressBar1.Maximum = buffer.Count();
                            for (int i = 0; i < buffer.Count; i++)
                            {
                                ProgressLbl.Text = "Loading " + buffer[i];
                                ProgressLbl.Update();
                                myOrganizer.AddDirectory(buffer[i], Catalogued);
                                userDirectories.Add(buffer[i]);
                                progressBar1.Value += 1;
                            }

                            UpdateMovieList();

                            listBox1.SelectedIndex = 0;
                            UpdateAllInformation(myOrganizer.SearchForFirstFile(listBox1.Items[0].ToString()));
                            this.MaximumSize = new System.Drawing.Size(762, 450);
                        }
                    }
                    else
                    {
                        //The auto-loading function has not been set.
                        this.MaximumSize = new System.Drawing.Size(240, 450);
                    }
                    #endregion
                }
            }


        #endregion

        #region tool strip

            private void addDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
            {
                AddBtn_Click(sender, e);
            }

            private void saveDirectoriesToolStripMenuItem_Click(object sender, EventArgs e)
            {
                SaveBtn_Click(sender, e);
            }

            private void loadDirectoriesToolStripMenuItem_Click(object sender, EventArgs e)
            {
                LoadBtn_Click(sender, e);
            }

            private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
            {
                this.Close();
            }

            private void clearToolStripMenuItem_Click(object sender, EventArgs e)
            {
                ClearDirectoriesBtn_Click(sender, e);
            }

            private void findToolStripMenuItem_Click(object sender, EventArgs e)
            {
                FindBtn_Click(sender, e);
            }

        #endregion            

            private void listBox1_KepPress(object sender, KeyPressEventArgs e)
            {

            }

    }
}
