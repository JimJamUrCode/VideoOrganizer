using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net;
using System.Diagnostics;
using Mixpanel.NET.Events;

namespace VideoOrganizer
{
    public class MediaOrganizer
    {
        private List<FileDetails> myFiles;
        private List<string> myDirectories;
        private readonly string[] fileTypes = { ".m2ts", ".ts", ".mkv", ".avi", ".mp4", ".iso", ".bin", ".cue", ".bdmv", ".flv", ".h264", ".m4v", ".mov", ".mpeg", ".mpeg4", ".mpg", ".mpg2", ".vob", ".wmv", ".xvid", ".ISO" };
        private MixpanelTracker tracker;

        public MediaOrganizer()
        {
            myFiles = new List<FileDetails>();
            myDirectories = new List<string>();
            tracker = new MixpanelTracker("dc8a2b5387d84369e002c8cbd2cf14f1");
        }

        public FileDetails SearchForFirstFile(string fileName)
        {
            for (int i = 0; i < myFiles.Count; i++)
            {
                if (myFiles[i].fileName.ToLower().Contains(fileName.ToLower()))
                {
                    return myFiles[i];
                }
            }

            return null;
        }

        public List<FileDetails> SearchAllFiles(string fileName)
        {
            List<FileDetails> matchedResults = new List<FileDetails>();
            for (int i = 0; i < myFiles.Count; i++)
            {
                if (myFiles[i].fileName.ToLower().Contains(fileName.ToLower()))
                {
                    matchedResults.Add(myFiles[i]);
                }
            }
            return matchedResults;
        }

        public FileDetails SearchForExactMatch(string fileName)
        {
            for (int i = 0; i < myFiles.Count; i++)
            {
                if (myFiles[i].fileName.ToLower() == fileName.ToLower())
                {
                    return myFiles[i];
                }
            }

            return null;
        }

        public bool AddDirectory(string path, bool catalogued)
        {
            if (!myDirectories.Contains(path))
            {
                myDirectories.Add(path);
                return AddDirectoryMain(path, catalogued);
            }
            else
            {
                return false;
            }
        }

        private bool AddDirectoryMain(string path, bool catalogued)
        {
            string version = Application.ProductVersion;
            try
            {
                //Getting file names and folder names
                string[] folderPaths = System.IO.Directory.GetDirectories(path);
                string[] filePaths = System.IO.Directory.GetFiles(path);

                //Adding folders
                for (int i = 0; i < folderPaths.Length; i++)
                {
                    AddFolder(folderPaths[i], catalogued);
                }
                //Adding files
                for (int i = 0; i < filePaths.Length; i++)
                {
                    AddFile(filePaths[i], catalogued);
                }
                return true;
            }
            catch (Exception ex)
            {
                var properties2 = new Dictionary<string, object>();
                properties2["time"] = DateTime.Now;
                properties2["Error Method"] = "AddDirectoryMain";
                properties2["Exception"] = ex.ToString();
                trackAnEvent("Error", properties2);

                string flap = ex.ToString();
                MessageBox.Show("A local or network drive with rescources necessary to run Video Organizer has been disconnected", "Drive missing", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return false;
            }
        }

        #region Adding Folders and Files

        /*when refering to a folder in this section and most sections of code, what I am
         * refering to is a movie whose contents are stored in a folder, in a directory that
         * the user loaded or has chose to auto-load. This folder is references as a single 
         * file within this program.
         */

        private void AddFolder(string folderPath, bool catalogued)
        {
            if (!catalogued)//If the library has NOT been catalogued
            {
                DirectoryInfo myDirectoryInfo = new DirectoryInfo(folderPath);
                DateTime creationDate = myDirectoryInfo.CreationTime;
                long size = DirSize(myDirectoryInfo) / 1048576;
                string extension = "Folder";
                string currentPath = folderPath;
                bool readOnly = false;
                string fileName = RemoveKeywordTHE(myDirectoryInfo.Name);
                myFiles.Add(new FileDetails(fileName, creationDate, size, extension, currentPath, readOnly));
            }
            else//If the library has been catalogued
            {
                AddCataloguedFolder(folderPath);
            }
        }

        private void AddFile(string filePath, bool catalogued)
        {
            if (!catalogued)//If the file has NOT been catalogued
            {
                FileInfo myFileInfo = new FileInfo(filePath);
                string ext = myFileInfo.Extension.ToLower();
                if (CheckFileType(ext))
                {
                    DateTime creationDate = myFileInfo.CreationTime;
                    long size = myFileInfo.Length / 1048576;
                    string extension = myFileInfo.Extension;
                    string currentPath = filePath;
                    bool readOnly = myFileInfo.IsReadOnly;
                    string fileName = RemoveKeywordTHE(myFileInfo.Name.Replace(extension, ""));
                    myFiles.Add(new FileDetails(fileName, creationDate, size, extension, currentPath, readOnly));
                }
            }
            else//If the file HAS been catalogued
            {
                AddCataloguedFile(filePath);
            }
        }

        private void AddCataloguedFolder(string folderPath)
        {
            DirectoryInfo myDirectoryInfo = new DirectoryInfo(folderPath);
            string name = RemoveKeywordTHE(myDirectoryInfo.Name);

            //Creating references to local database storage locations
            string localDatabaseCoversDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\Index\\Local Database\\Covers\\";
            string localDatabaseInformationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\Index\\Local Database\\Information\\";

            //Checking to see if the Catalogued media file actually exists
            if (File.Exists(localDatabaseInformationDirectory + name + ".txt"))
            {
                //string[] myFileDetails = File.ReadAllLines(localDatabaseInformationDirectory + name + ".txt");
                string[] FileDetails = File.ReadAllLines(localDatabaseInformationDirectory + name.Replace(":", "").Substring(0, name.Length) + ".txt");

                List<string> myFileDetails = FileDetails.ToList();

                for (int i = 0; i < myFileDetails.Count; i++)
                {
                    if (myFileDetails[i] == "")
                    {
                        myFileDetails.RemoveAt(i);
                        i = 0;
                    }
                }

                string fileName = myFileDetails[0];
                DateTime creationDate = Convert.ToDateTime(myFileDetails[1]);
                long size = (long)Convert.ToDouble(myFileDetails[2]);
                string extension = myFileDetails[3];
                bool readOnly = Convert.ToBoolean(myFileDetails[4]);
                string currentPath = myFileDetails[5];
                string director = myFileDetails[6];
                string genre = myFileDetails[7];
                string MPAARating = myFileDetails[8];
                string description = myFileDetails[9];
                string reviewScore = myFileDetails[10];
                string movieLength = myFileDetails[11];
                string internetMovieName = myFileDetails[12];
                string year = myFileDetails[13];
                string trailerURL = myFileDetails[14];
                string IMDBURL = myFileDetails[15];
                string coverPath = localDatabaseCoversDirectory + name + ".jpg";
                    
                myFiles.Add(new FileDetails(fileName, creationDate, size, extension, currentPath, readOnly, internetMovieName, year, genre, reviewScore, director, movieLength, MPAARating, description, coverPath, IMDBURL, trailerURL));
            }
            else//If the "catalogued" file does not exist the create it.
            {
                AddFolder(folderPath, false);
                CreateSingleDatabaseEntry(myFiles.Count-1);
                //MessageBox.Show("You may need to recreate the database, " + name + " was not found in the database");
            }
        }

        private void AddCataloguedFile(string filePath)
        {
            FileInfo myFileInfo = new FileInfo(filePath);

            //Removing the file extension from the file name
            string name = RemoveKeywordTHE(myFileInfo.Name.Replace(myFileInfo.Extension.ToLower(), "").Replace(myFileInfo.Extension.ToUpper(), ""));

            //Creating references to local database storage locations
            string localDatabaseCoversDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\Index\\Local Database\\Covers\\";
            string localDatabaseInformationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\Index\\Local Database\\Information\\";

            //Checking to see if the Catalogued media file actually exists
            if (File.Exists(localDatabaseInformationDirectory + name + ".txt"))
            {
                //string[] myFileDetails = File.ReadAllLines(localDatabaseInformationDirectory + name + ".txt");
                string[] FileDetails = File.ReadAllLines(localDatabaseInformationDirectory + name.Replace(":", "").Substring(0, name.Length) + ".txt");

                List<string> myFileDetails = FileDetails.ToList();

                //for (int i = 0; i < myFileDetails.Count; i++)
                //{
                //    if (myFileDetails[i] == "")
                //    {
                //        myFileDetails.RemoveAt(i);
                //        i = 0;
                //    }
                //}

                try
                {
                    string fileName = myFileDetails[0];
                    DateTime creationDate = Convert.ToDateTime(myFileDetails[1]);
                    long size = (long)Convert.ToDouble(myFileDetails[2]);
                    string extension = myFileDetails[3];
                    bool readOnly = Convert.ToBoolean(myFileDetails[4]);
                    string currentPath = myFileDetails[5];
                    string director = myFileDetails[6];
                    string genre = myFileDetails[7];
                    string MPAARating = myFileDetails[8];
                    string description = myFileDetails[9];
                    string reviewScore = myFileDetails[10];
                    string movieLength = myFileDetails[11];
                    string internetMovieName = myFileDetails[12];
                    string year = myFileDetails[13];
                    string trailerURL = myFileDetails[14];
                    string IMDBURL = myFileDetails[15];
                    string coverPath = localDatabaseCoversDirectory + name + ".jpg";
                    myFiles.Add(new FileDetails(fileName, creationDate, size, extension, currentPath, readOnly, internetMovieName, year, genre, reviewScore, director, movieLength, MPAARating, description, coverPath, IMDBURL, trailerURL));
                }
                catch (Exception ex)
                {
                    var properties2 = new Dictionary<string, object>();
                    properties2["time"] = DateTime.Now;
                    properties2["Error Method"] = "AddCataloguedFile";
                    properties2["Exception"] = ex.ToString();
                    trackAnEvent("Error", properties2);

                    Console.Write("Another Error");
                    Console.Write(ex.ToString());
                }

                
            }
            else//If the catalogued file does not exist
            {
                AddFile(filePath, false);
                CreateSingleDatabaseEntry(myFiles.Count-1);
                //MessageBox.Show("You may need to recreate the database, " + name + " was not found in the database");
            }
        }
        
        #endregion


        public bool CreateDatabase(ProgressBar progressBar1, Form MainForm)
        {
            //Setting initial progress bar values
            progressBar1.Value = 0;
            progressBar1.Maximum = myFiles.Count;

            //Declaring and initializing the thread count variable
            int threadCount = 0;

            for (int i = 0; i < myFiles.Count-1; i+= 12)
            {
                #region Declaring Threads
                var thread = new Thread(
                (o) =>
                {
                    //Creates a database entry for the file at a particular index of imported files
                    CreateSingleDatabaseEntry(i);
                });

                var thread2 = new Thread(
                (o) =>
                {
                    //Creates a database entry for the file at a particular index of imported files
                    CreateSingleDatabaseEntry(i+1);
                });

                var thread3 = new Thread(
                (o) =>
                {
                    //Creates a database entry for the file at a particular index of imported files
                    CreateSingleDatabaseEntry(i + 2);
                });

                var thread4 = new Thread(
                (o) =>
                {
                    //Creates a database entry for the file at a particular index of imported files
                    CreateSingleDatabaseEntry(i + 3);
                });

                var thread5 = new Thread(
                (o) =>
                {
                    //Creates a database entry for the file at a particular index of imported files
                    CreateSingleDatabaseEntry(i + 4);
                });

                var thread6 = new Thread(
                (o) =>
                {
                    //Creates a database entry for the file at a particular index of imported files
                    CreateSingleDatabaseEntry(i + 5);
                });

                var thread7 = new Thread(
                (o) =>
                {
                    //Creates a database entry for the file at a particular index of imported files
                    CreateSingleDatabaseEntry(i + 6);
                });

                var thread8 = new Thread(
                (o) =>
                {
                    //Creates a database entry for the file at a particular index of imported files
                    CreateSingleDatabaseEntry(i + 7);
                });

                var thread9 = new Thread(
                (o) =>
                {
                    //Creates a database entry for the file at a particular index of imported files
                    CreateSingleDatabaseEntry(i + 8);
                });

                var thread10 = new Thread(
                (o) =>
                {
                    //Creates a database entry for the file at a particular index of imported files
                    CreateSingleDatabaseEntry(i + 9);
                });

                var thread11 = new Thread(
                (o) =>
                {
                    //Creates a database entry for the file at a particular index of imported files
                    CreateSingleDatabaseEntry(i + 10);
                });

                var thread12 = new Thread(
                (o) =>
                {
                    //Creates a database entry for the file at a particular index of imported files
                    CreateSingleDatabaseEntry(i + 11);
                });
                #endregion

                #region Starting Threads
                if (i < myFiles.Count){
                    thread.Start();
                    threadCount++;
                }
                if (i+1 < myFiles.Count){
                    thread2.Start();
                    threadCount++;
                }
                if (i + 2 < myFiles.Count){
                    thread3.Start();
                    threadCount++;
                }
                if (i + 3 < myFiles.Count){
                    thread4.Start();
                    threadCount++;
                }
                if (i + 4 < myFiles.Count){
                    thread5.Start();
                    threadCount++;
                }
                if (i + 5 < myFiles.Count){
                    thread6.Start();
                    threadCount++;
                }
                if (i + 6 < myFiles.Count){
                    thread7.Start();
                    threadCount++;
                }
                if (i + 7 < myFiles.Count){
                    thread8.Start();
                    threadCount++;
                }
                if (i + 8 < myFiles.Count){
                    thread9.Start();
                    threadCount++;
                }
                if (i + 9 < myFiles.Count){
                    thread10.Start();
                    threadCount++;
                }
                if (i + 10 < myFiles.Count){
                    thread11.Start();
                    threadCount++;
                }
                if (i + 11 < myFiles.Count){
                    thread12.Start();
                    threadCount++;
                }
                #endregion

                //Limiting the number of threads that can be created.
                if (threadCount > 48)
                {
                    thread.Join(20000);
                    thread2.Join(20000);
                    thread3.Join(20000);
                    thread4.Join(20000);
                    thread5.Join(20000);
                    thread6.Join(20000);
                    thread7.Join(20000);
                    thread8.Join(20000);
                    thread9.Join(20000);
                    thread10.Join(20000);
                    thread11.Join(20000);
                    thread12.Join(20000);
                }

                #region ProgressBar and threadCount

                if (thread.ThreadState != System.Threading.ThreadState.Unstarted)
                {
                   progressBar1.Increment(1);
                   threadCount--;
                }
                if (thread2.ThreadState != System.Threading.ThreadState.Unstarted)
                {
                    progressBar1.Increment(1);
                    threadCount--;
                }
                if (thread3.ThreadState != System.Threading.ThreadState.Unstarted)
                {
                    progressBar1.Increment(1);
                    threadCount--;
                }
                if (thread4.ThreadState != System.Threading.ThreadState.Unstarted)
                {
                    progressBar1.Increment(1);
                    threadCount--;
                }
                if (thread5.ThreadState != System.Threading.ThreadState.Unstarted)
                {
                    progressBar1.Increment(1);
                    threadCount--;
                }
                if (thread6.ThreadState != System.Threading.ThreadState.Unstarted && thread6.Join(20000))
                {
                    progressBar1.Increment(1);
                    threadCount--;
                }
                if (thread7.ThreadState != System.Threading.ThreadState.Unstarted)
                {
                    progressBar1.Increment(1);
                    threadCount--;
                }
                if (thread8.ThreadState != System.Threading.ThreadState.Unstarted)
                {
                    progressBar1.Increment(1);
                    threadCount--;
                }
                if (thread9.ThreadState != System.Threading.ThreadState.Unstarted)
                {
                    progressBar1.Increment(1);
                    threadCount--;
                }
                if (thread10.ThreadState != System.Threading.ThreadState.Unstarted)
                {
                    progressBar1.Increment(1);
                    threadCount--;
                }
                if (thread11.ThreadState != System.Threading.ThreadState.Unstarted)
                {
                    progressBar1.Increment(1);
                    threadCount--;
                }
                if (thread12.ThreadState != System.Threading.ThreadState.Unstarted && thread12.Join(20000))
                {
                    progressBar1.Increment(1);
                    threadCount--;
                }
                #endregion

                //Updating form
                MainForm.Update();
            }
            return true;
        }

        public List<FileDetails> getList()
        {
            return myFiles;
        }

        public void clearDirectories()
        {
            myDirectories.Clear();
        }

        public void clearFiles()
        {
            myFiles.Clear();
        }

        public void CreateHTMLIndex(ProgressBar progressBar, Label progressLbl, bool Catalogued)
        {
            if (Catalogued)
            {
                var properties = new Dictionary<string, object>();
                properties["time"] = DateTime.Now;
                properties["Was Database Created"] = "Yes";
                trackAnEvent("HTMl Index Created", properties);

                progressBar.Maximum = myFiles.Count;
                progressBar.Value = 0;
                string coversDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/videoorganizersettings/Index/Local Database/Covers/";
                string indexDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\";
                StreamWriter writer = new StreamWriter(indexDirectory + "Index.html", false);

                //Writing the header of the html file
                writer.WriteLine("<html>");
                writer.WriteLine("<title>Index Of Movies</title>");

                writer.WriteLine("<html>");
                writer.WriteLine("<head>");
	            writer.WriteLine("<style type=\"text/css\">");
	            writer.WriteLine("body");
	            writer.WriteLine("{");
	            writer.WriteLine("bgcolor:gray;");
	            writer.WriteLine("font-style:italic");
	            writer.WriteLine("}");
	            writer.WriteLine("p");
	            writer.WriteLine("{");
	            writer.WriteLine("color:lightgray;");
	            writer.WriteLine("}");
	
	            writer.WriteLine("ul#gallery ");
	            writer.WriteLine("{");
	            writer.WriteLine("margin:0 auto;");
	            writer.WriteLine("padding:0;");
	            writer.WriteLine("list-style-type:none;");
	            writer.WriteLine("width:90%;");
	            writer.WriteLine("font-family: Monotype Corsiva, Harlow Solid Italic, serif;");
	            writer.WriteLine("}");

	            writer.WriteLine("ul#gallery li ");
	            writer.WriteLine("{");
	            writer.WriteLine("float: left;");
	            writer.WriteLine("margin:15px;");
	            writer.WriteLine("}");

	            writer.WriteLine("ul#gallery li p ");
	            writer.WriteLine("{");
	            writer.WriteLine("text-align: center;");
	            writer.WriteLine("margin:5px 0;");
	            writer.WriteLine("}");
	
	            writer.WriteLine("</style>");
                writer.WriteLine("</head>");

                writer.WriteLine("<body bgcolor=\"gray\">");
                writer.WriteLine("<div align=\"center\">");

                writer.WriteLine("<div align=\"center\"> This file is located at: " + indexDirectory + "\\videoorganizersettings\\Index.html</div>");
                writer.WriteLine("</div>");

                /*Creating the links at the top of the index page for each 
                 * character that a movie starts with
                 */
                writer.WriteLine("<div align=\"center\">");
                writer.WriteLine("<div id=\"TOP\">");
                for (int i = 0; i < myFiles.Count; i++)
                {
                    char currentIndex = myFiles[i].fileName.ToUpper().ElementAt(0);
                    char previousIndex = currentIndex;

                    //If a comparison needs to be made
                    if (i != 0)
                    {
                        previousIndex = myFiles[i - 1].fileName.ToUpper().ElementAt(0);
                    }

                    if (i == 0)
                    {
                        writer.WriteLine("<a href=\"#" + currentIndex + "\">" + currentIndex + "</a>");
                    }
                    else if (currentIndex != previousIndex)
                    {
                        writer.WriteLine("<a href=\"#" + currentIndex + "\">" + currentIndex + "</a>");
                    }
                }
                //Ending the div tag that contains the above index
                writer.WriteLine("</div>");
                writer.WriteLine("</div>");

                writer.WriteLine("<ul id=\"gallery\">");

                for (int i = 0; i < myFiles.Count; i++)
                {
                    if (i == 0)
                    {
                        writer.WriteLine("<a name=\"" + myFiles[i].fileName.ToUpper().ElementAt(0) + "\">");
                    }
                    else if (i != 0 && myFiles[i].fileName.ToUpper().ElementAt(0) != myFiles[i - 1].fileName.ToUpper().ElementAt(0))
                    {//end section for current letter
                        writer.WriteLine("</a>");
                        writer.WriteLine("<a name=\"" + myFiles[i].fileName.ToUpper().ElementAt(0) + "\">");
                    }

                    progressLbl.Text = "Creating Index for: " + myFiles[i].fileName;
                    progressLbl.Update();

                    //Placing a movie cover next to the title
                    if(myFiles[i].fileName.Length < 16)
                        writer.WriteLine("<li><a href=\"Index/" + myFiles[i].fileName + ".html\"/><img src=\"Index/Local Database/Covers/" + myFiles[i].fileName + ".jpg\" alt=\"" + myFiles[i].fileName + "\" width=\"112\" height=\"190\" /></a><p>" + myFiles[i].fileName + "</p></li>");
                    else
                        writer.WriteLine("<li><a href=\"Index/" + myFiles[i].fileName + ".html\"/><img src=\"Index/Local Database/Covers/" + myFiles[i].fileName + ".jpg\" alt=\"" + myFiles[i].fileName + "\" width=\"112\" height=\"190\" /></a><p>" + myFiles[i].fileName.Substring(0,15) + "</p></li>");

                    //Creating movie specific HTML page
                    CreateHTMLMoviePage(myFiles[i]);

                    progressBar.Increment(1);
                }

                writer.WriteLine("</ul>");
                writer.WriteLine("</html>");
                writer.Close();
                progressLbl.Text = "Done.";

                string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                try
                {
                    Process.Start(directory + "\\videoorganizersettings\\Index.html");
                }
                catch (FileNotFoundException ex)
                {
                    var properties2 = new Dictionary<string, object>();
                    properties2["time"] = DateTime.Now;
                    properties2["Error Method"] = "CreateHTMLIndex";
                    properties2["Exception"] = ex.ToString();
                    trackAnEvent("Error", properties);
                    MessageBox.Show("The index has not been created or has been moved");
                }
            }
            else
            {
                var properties = new Dictionary<string, object>();
                properties["time"] = DateTime.Now;
                properties["Was Database Created"] = "No";
                trackAnEvent("HTMl Index Created", properties);
                MessageBox.Show("You must create a database to do an HTML index");
            }
        }


        private void CreateHTMLMoviePage(FileDetails movie)
        {
            string coversDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/videoorganizersettings/Index/Local Database/Covers/";
            string indexDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\Index\\";
            StreamWriter writer = new StreamWriter(indexDirectory + movie.fileName + ".html", false);

                writer.WriteLine("<html>");
                writer.WriteLine("<head>");
                writer.WriteLine("<style type=\"text/css\">");
                writer.WriteLine("html{");
                writer.WriteLine("height:100%;}");

                writer.WriteLine("body{");
                writer.WriteLine("height:100%;");
                writer.WriteLine("padding:0;");
                writer.WriteLine("margin:0;");
                writer.WriteLine("padding:3px;");
                writer.WriteLine("border:3px solid black;}");

                writer.WriteLine("img.background{");
                writer.WriteLine("width:100%;");
                writer.WriteLine("position:absolute;");
                writer.WriteLine("left:0px;");
                writer.WriteLine("top:0px;");
                writer.WriteLine("z-index:-1;}");

                writer.WriteLine("img.cover{");
                writer.WriteLine("width:28%;");
                writer.WriteLine("border:3px solid black;}");

                writer.WriteLine("div.titlebox ");
                writer.WriteLine("{");
                writer.WriteLine("width:40%;");
                writer.WriteLine("margin:0% 0% 0% 45%;");
                writer.WriteLine("background-color:#ffffff;");
                writer.WriteLine("border:3px solid black;");
                writer.WriteLine("opacity:0.4;");
                writer.WriteLine("filter:alpha(opacity=40);");
                writer.WriteLine("text-align:center;");
                writer.WriteLine("}");

                writer.WriteLine("div.detailsbox {");
                writer.WriteLine("width:65%;");
                writer.WriteLine("margin:0% 0% 3% 30%;");
                writer.WriteLine("background-color:#ffffff;");
                writer.WriteLine("border:3px solid black;");
                writer.WriteLine("opacity:0.4;");
                writer.WriteLine("filter:alpha(opacity=40);}");

                writer.WriteLine("div.descriptionbox {");
                writer.WriteLine("width:65%;");
                writer.WriteLine("margin:0% 0% 0% 30%;");
                writer.WriteLine("background-color:#ffffff;");
                writer.WriteLine("border:3px solid black;");
                writer.WriteLine("opacity:0.4;");
                writer.WriteLine("filter:alpha(opacity=40);}");

                writer.WriteLine("div.descriptionbox p{");
                writer.WriteLine("width:90%;");
                writer.WriteLine("margin:30px 0px 30px 55px;");
                writer.WriteLine("font-weight:bold;");
                writer.WriteLine("opacity:1;");
                writer.WriteLine("color:#000000;}");
                
                writer.WriteLine("div.movietitle");
                writer.WriteLine("{");
                writer.WriteLine("width:auto;");
                writer.WriteLine("margin:0% 0% 0% 0%;");
                writer.WriteLine("font-weight:bold;");
                writer.WriteLine("font-size:40pt;");
                writer.WriteLine("text-decoration:underline;");
                writer.WriteLine("}");

                writer.WriteLine("</style>");
                writer.WriteLine("</head>");

                writer.WriteLine("<body bgcolor=\"white\">");
                writer.WriteLine("<img src=\"Local Database/Covers/" + movie.fileName + "FanArt.jpg\" class=\"background\"/>");

                writer.WriteLine("<div align=\"left\"><img class=\"cover\" align=\"left\" src=\"Local Database/Covers/" + movie.fileName + ".jpg\">");

                writer.WriteLine("<div class=\"titlebox\">");
			    writer.WriteLine("<div class=\"movietitle\">");
                writer.WriteLine(movie.fileName);
			    writer.WriteLine("</div>");
		        writer.WriteLine("</div>");


                writer.WriteLine("<div class=\"descriptionbox\">");
                writer.WriteLine("<p>" + movie.description + "</p>");
                writer.WriteLine("</div>");

                writer.WriteLine("<div class=\"detailsbox\">");
                writer.WriteLine("<table>");
	                writer.WriteLine("<tr><td><b>Creation Date:</b>" + movie.creationDate + "</td></tr>");
	                writer.WriteLine("<tr><td><b>Size:</b>" + movie.size + "</td></tr>");
	                writer.WriteLine("<tr><td><b>Extension:</b>" + movie.extension + "</td></tr>");
	                writer.WriteLine("<tr><td><b>Read Only:</b>" + movie.readOnly + "</td></tr>");
	                writer.WriteLine("<tr><td><b>Directory:</b>" + movie.directory + "</td></tr>");

	                writer.WriteLine("<tr><td><b>Year:</b>" + movie.internetYear + "</td></tr>");
	                writer.WriteLine("<tr><td><b>Genre:</b>" + movie.genre + "</td></tr>");
	                writer.WriteLine("<tr><td><b>Review Score:</b>" + movie.reviewScore + "</td></tr>");
	                writer.WriteLine("<tr><td><b>Director:</b>" + movie.director + "</td></tr>");
	                writer.WriteLine("<tr><td><b>Runtime:</b>" + movie.runtime + "</td></tr>");
	                writer.WriteLine("<tr><td><b>MPAA Rating:</b>" + movie.MPAARating + "</td></tr>");
	                writer.WriteLine("<tr><td><a href=\"http://" + movie.trailerURL + "\"/>Trailer</a></td></tr>");
                writer.WriteLine("</table></div></img></div>");



                



                writer.WriteLine("</body>");
                writer.WriteLine("</html>");


            writer.Close();
        }

        private string RemoveKeywordTHE(string original)
        {
            if (original.IndexOf("The ") == 0 || original.IndexOf("the ") == 0 || original.IndexOf("THE ") == 0)
                {
                    original = original.Substring(4);
                }
            return original;
        }

        private static long DirSize(DirectoryInfo d)
        {
            long Size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                Size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                Size += DirSize(di);
            }
            return (Size);
        }

        private bool CheckFileType(string testExt)
        {
            for (int i = 0; i < fileTypes.Length; i++)
            {
                if (testExt == fileTypes[i])
                {
                    return true;
                }
            }
            return false;
        }

        private string[] StripAbsolutePaths(string[] absoluteNames)
        {
            //Removing everything except for the filename and extension
            for (int i = 0; i < absoluteNames.Length; i++)
            {
                char[] pathToFile = absoluteNames[i].ToCharArray();
                int j = pathToFile.Length - 1;
                int indexOfName = 0;
                while (j > 0)
                {
                    if (pathToFile[j] == '\\')
                    {
                        indexOfName = j;
                        j = 0;
                    }
                    else
                        j--;
                }
                absoluteNames[i] = absoluteNames[i].Substring(indexOfName + 1);
            }

            //Removing the extension
            for (int i = 0; i < absoluteNames.Length; i++)
            {
                if (absoluteNames[i].IndexOf('.') == absoluteNames[i].Length - 4 || absoluteNames[i].IndexOf('.') == absoluteNames[i].Length - 3 || absoluteNames[i].IndexOf('.') == absoluteNames[i].Length - 5)
                {
                    if (absoluteNames[i].IndexOf('.') > 0)
                    {
                        int index = absoluteNames[i].Length;
                        char[] fileName = absoluteNames[i].ToCharArray();
                        for (int j = fileName.Length - 1; fileName[j] != '.'; j--)
                        {
                            index = j - 1;
                        }
                        absoluteNames[i] = absoluteNames[i].Substring(0, index);
                    }
                }
            }

            return absoluteNames;
        }

        private void CreateSingleDatabaseEntry(int index)
        {
            string localDatabaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\Index\\Local Database\\";
            string localDatabaseCoversDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\Index\\Local Database\\Covers\\";
            string localDatabaseInformationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\videoorganizersettings\\Index\\Local Database\\Information\\";
            try
            {
                if (File.Exists(localDatabaseInformationDirectory + myFiles[index].fileName.Replace(":", "").Substring(0, myFiles[index].fileName.Length) + ".txt") == false)
                {
                    //Creating the IMDB object to prepare for parsing
                    IMDBParser myIMDBParser = new IMDBParser();
                    //Updating the IMDB information
                    myIMDBParser.retreiveTitleID(myFiles[index].fileName);
                    myIMDBParser.retreiveMoviePageResponse();
                    myIMDBParser.retreiveGenre();
                    myIMDBParser.retreiveMetaScore();
                    myIMDBParser.retreiveMPAARating();
                    myIMDBParser.retreiveMovieDescription();
                    myIMDBParser.retreiveRunTime();
                    myIMDBParser.retreiveTitleAndYear();
                    myIMDBParser.retreiveDirector();
                    myIMDBParser.retreiveTrailerURL();

                    //Adding all file information to the list
                    List<string> AllFileInfo = new List<string>();
                    AllFileInfo.Add(myFiles[index].fileName);
                    AllFileInfo.Add(myFiles[index].creationDate.ToString());
                    AllFileInfo.Add(Convert.ToString(myFiles[index].size));
                    AllFileInfo.Add(myFiles[index].extension);
                    AllFileInfo.Add(myFiles[index].readOnly.ToString());
                    AllFileInfo.Add(myFiles[index].directory);

                    //Adding all IMDB information to the list
                    AllFileInfo.Add(myIMDBParser.getDirector());
                    AllFileInfo.Add(myIMDBParser.getGenre());
                    AllFileInfo.Add(myIMDBParser.getMPAARating());
                    AllFileInfo.Add(myIMDBParser.getMovieDescription());
                    AllFileInfo.Add(myIMDBParser.getMetaScore());
                    AllFileInfo.Add(myIMDBParser.getRunTime());
                    AllFileInfo.Add(myIMDBParser.getIMDBTitle());
                    AllFileInfo.Add(myIMDBParser.getYear());
                    AllFileInfo.Add(myIMDBParser.getTrailerURL());
                    AllFileInfo.Add("http://www.imdb.com/title/" + myIMDBParser.getTitleID());

                    //Updating file details
                    myFiles[index].director = myIMDBParser.getDirector();
                    myFiles[index].genre = myIMDBParser.getGenre();
                    myFiles[index].MPAARating = myIMDBParser.getMPAARating();
                    myFiles[index].description = myIMDBParser.getMovieDescription();
                    myFiles[index].reviewScore = myIMDBParser.getMetaScore();
                    myFiles[index].runtime = myIMDBParser.getRunTime();
                    myFiles[index].internetTitle = myIMDBParser.getIMDBTitle();
                    myFiles[index].internetYear = myIMDBParser.getYear();
                    myFiles[index].trailerURL = myIMDBParser.getTrailerURL();
                    myFiles[index].IMDBURL = "http://www.imdb.com/title/" + myIMDBParser.getTitleID();
                    myFiles[index].coverPath = localDatabaseCoversDirectory + myFiles[index].fileName + ".jpg";

                    //Writing all of the information gathered for this movie to a file
                    File.WriteAllLines(localDatabaseInformationDirectory + AllFileInfo[0].Replace(":", "").Substring(0, AllFileInfo[0].Length) + ".txt", AllFileInfo.ToArray());

                    //Updating the image for the movie from IMDB
                    try
                    {
                        string imageURL = myIMDBParser.retreiveArtwork();
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile(imageURL, localDatabaseCoversDirectory + AllFileInfo[0].Replace(":", "").Substring(0, AllFileInfo[0].Length) + ".jpg");
                    }
                    catch (Exception ex)
                    {
                        /*the movie response page within the imdb class was not created
                         * This may be because the search yeilded no results
                         */
                        var properties2 = new Dictionary<string, object>();
                        properties2["time"] = DateTime.Now;
                        properties2["Error Method"] = "CreateSingleDatabaseEntry";
                        properties2["Exception"] = ex.ToString();
                        trackAnEvent("Error", properties2);
                    }

                    //Updating the image for the movie from IMDB
                    try
                    {
                        string imageURL = myIMDBParser.retreiveFanArt();
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile(imageURL, localDatabaseCoversDirectory + AllFileInfo[0].Replace(":", "").Substring(0, AllFileInfo[0].Length) + "FanArt.jpg");
                    }
                    catch (Exception ex)
                    {
                        /*the movie response page within the imdb class was not created
                         * This may be because the search yeilded no results
                         */
                        var properties2 = new Dictionary<string, object>();
                        properties2["time"] = DateTime.Now;
                        properties2["Error Method"] = "CreateSingleDatabaseEntry";
                        properties2["Exception"] = ex.ToString();
                        trackAnEvent("Error", properties2);
                    }
                }
            }
            catch (Exception ex)
            {
                string a = ex.ToString();
                var properties2 = new Dictionary<string, object>();
                properties2["time"] = DateTime.Now;
                properties2["Error Method"] = "CreateSingleDatabaseEntry";
                properties2["Exception"] = ex.ToString();
                trackAnEvent("Error", properties2);
            }
        }

        public void trackAnEvent(string eventName, Dictionary<string, object> properties)
        {
            tracker.Track(eventName, properties);
        }

        //Sorting Methods
        
        //public void setSortBy(string sortBy)
        //{
        //    previousSort = this.sortBy;
        //    this.sortBy = sortBy;
        //}

        //public string getSortBy()
        //{
        //    return sortBy;
        //}        
    }
}
