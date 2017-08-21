using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using Mixpanel.NET.Events;

namespace VideoOrganizer
{
    class IMDBParser
    {
        WebClient imdbCom = new WebClient();
        
        private MixpanelTracker tracker;
        //user entered title
        private string title;

        //IMDB variables
        private string titleID;
        private string genre;
        private string imdbTitle;
        private string metaScore;
        private string director;
        private string runtime;
        private string mpaaRating;
        private string mpaaRatingDescription;
        private string movieDescription;
        private string year;
        private string trailerURL;

        private string moviePageResponse;

        //static void Main(string[] args)
        //{
        //    IMDBParser IMDB = new IMDBParser();
        //    if (IMDB.retreiveTitleID("avatar"))
        //    {
        //        Console.WriteLine("Title ID: {0}", IMDB.getTitleID());
        //        Console.WriteLine("Title :   {0}", IMDB.getTitle());
        //    }
        //    else
        //    {
        //        Console.WriteLine("Search term did not have a result");
        //    }

        //    //This retreives the specific movie page
        //    IMDB.retreiveMoviePageResponse();

        //    //Parsing Genre out of movie specific page
        //    IMDB.retreiveGenre();
        //    Console.WriteLine("Genre:    {0}", IMDB.getGenre());

        //    IMDB.retreiveTitle();
        //    Console.WriteLine("Title:    {0}", IMDB.getIMDBTitle());

        //    IMDB.retreiveMetaScore();
        //    Console.WriteLine("MetaScore:{0}", IMDB.getMetaScore());

        //    IMDB.retreiveDirector();
        //    Console.WriteLine("Director: {0}", IMDB.getDirector());

        //    IMDB.retreiveRunTime();
        //    Console.WriteLine("Runtime:  {0}", IMDB.getRunTime());

        //    IMDB.retreiveMPAARating();
        //    Console.WriteLine("MPAA Rating: {0}", IMDB.getMPAARating());
        //    Console.WriteLine("MPAA Rating Description: {0}", IMDB.getMPAARatingDescription());

        //    IMDB.retreiveArtwork();

        //    Thread.Sleep(20000);
        //}

        #region Constructors

        public IMDBParser()
        {
            title = "";
            titleID = "";
            genre = "";
            imdbTitle = "";
            metaScore = "";
            director = "";
            runtime = "";
            year = "";
            trailerURL = "";
            mpaaRating = "";
            mpaaRatingDescription = "";
            tracker = new MixpanelTracker("dc8a2b5387d84369e002c8cbd2cf14f1");
        }

        public IMDBParser(string movieName)
        {
            title = movieName;
            titleID = "";
            genre = "";
            imdbTitle = "";
            metaScore = "";
            director = "";
            runtime = "";
            year = "";
            trailerURL = "";
            mpaaRating = "";
            mpaaRatingDescription = "";
            tracker = new MixpanelTracker("dc8a2b5387d84369e002c8cbd2cf14f1");
        }

        #endregion

        public string retreiveTitleID(string movieName)
        {

            title = movieName;
            titleID = "";
            movieName = movieName.Replace(" ", "+");
            string url = "http://www.imdb.com/find?s=all&q=" + movieName;
            string response;

            try
            {
                response = imdbCom.DownloadString(url);
                //Eliminating the initial part of the response                
                int subStringIndex = response.IndexOf("a href=\"/title/");
                response = response.Substring(subStringIndex + 15);

                //Reading the next characters which are the titleID
                subStringIndex = 0;
                while (response.ElementAt(subStringIndex) != '/')
                {
                    titleID += response.ElementAt(subStringIndex);
                    subStringIndex++;
                }
            }
            catch (Exception ex)
            {
                var properties2 = new Dictionary<string, object>();
                properties2["time"] = DateTime.Now;
                properties2["Error Method"] = "retreiveTitleID";
                properties2["Exception"] = ex.ToString();
                tracker.Track("IMDB Error", properties2);
                return null;
            }
            return titleID;
        }

        public bool retreiveMoviePageResponse()
        {
            string url = "http://www.imdb.com/title/" + titleID + "/";
            try
            {
                moviePageResponse = imdbCom.DownloadString(url);
            }
            catch (Exception ex)
            {
                var properties2 = new Dictionary<string, object>();
                properties2["time"] = DateTime.Now;
                properties2["Error Method"] = "retreiveMoviePageResponse";
                properties2["Exception"] = ex.ToString();
                tracker.Track("IMDB Error", properties2);
                return false;
            }
            return true;
        }




        #region Movie Page Response Parsers
        /*You must have already run the retreiveTitleID and
         * retreiveMoviePageResponse function before running
         * any of these methods
         */
            public void retreiveGenre()
        {
            //Resetting class varibale
            this.genre = "";
            if (moviePageResponse != null)
            {
                try
                {
                    string genreNonParsed = moviePageResponse.Substring(moviePageResponse.LastIndexOf("href=\"/genre/"));
                    genreNonParsed = genreNonParsed.Substring(genreNonParsed.IndexOf("e/") + 2);

                    for (int i = 0; genreNonParsed.ElementAt(i) != '?'; i++)
                    {
                        this.genre += genreNonParsed.ElementAt(i);
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    var properties2 = new Dictionary<string, object>();
                    properties2["time"] = DateTime.Now;
                    properties2["Error Method"] = "retreiveGenre";
                    properties2["Exception"] = ex.ToString();
                    tracker.Track("IMDB Error", properties2);
                    this.genre = "Result not found.";
                }
            }
            else
            {
                this.genre = "Result not found.";
            }
        }

            public void retreiveTitleAndYear()
            {
                //Resetting class variable
                this.imdbTitle = "";
                if (moviePageResponse != null)
                {
                    string titleNonParsed = moviePageResponse.Substring(moviePageResponse.IndexOf("<title>") + 7);

                    for (int i = 0; titleNonParsed.ElementAt(i) != '<'; i++)
                    {
                        this.imdbTitle += titleNonParsed.ElementAt(i);
                    }

                    //Removing imdb from the title of the movie
                    if (imdbTitle.ToLower().IndexOf("imdb") < 1)
                    {
                        this.imdbTitle = this.imdbTitle.Substring(7);
                    }
                    else
                    {
                        this.imdbTitle = this.imdbTitle.Substring(0, this.imdbTitle.Length - 7);
                    }

                    //Replacing the string "&x27;" with its equivalent "'"
                    this.imdbTitle = this.imdbTitle.Replace("&#x27;", "'");
                    this.imdbTitle = this.imdbTitle.Replace("&#x26;", " and ");
                    this.imdbTitle = this.imdbTitle.Replace("&#x22;", "\"");

                    year = imdbTitle.Substring(imdbTitle.Length - 6,6);
                    year = year.Replace("(", "").Replace(")", "").Replace(" ", "");
                    imdbTitle = imdbTitle.Substring(0, imdbTitle.Length - 6);
                }
                else
                {
                    this.imdbTitle = "Result not found.";
                    this.year = "Result not found.";
                }
            }

            public void retreiveMetaScore()
        {
            //Resetting class varibale
            this.metaScore = "";
            if (moviePageResponse != null)
            {
                int ret = moviePageResponse.IndexOf("Metascore:");
                if (moviePageResponse.IndexOf("Metascore:") == -1)
                {
                    this.metaScore = "Unknown";
                }
                else
                {
                    string metaScoreNonParsed = moviePageResponse.Substring(moviePageResponse.IndexOf("Metascore:"));
                    metaScoreNonParsed = metaScoreNonParsed.Substring(metaScoreNonParsed.IndexOf(">") + 2);
                    for (int i = 0; metaScoreNonParsed.ElementAt(i) != '<'; i++)
                    {
                        this.metaScore += metaScoreNonParsed.ElementAt(i);
                    }
                }
            }
            else
            {
                this.metaScore = "Result not found.";
            }

            
        }

            public void retreiveDirector()
        {
            //Resetting class varibale
            this.director = "";
            if (moviePageResponse != null)
            {
                try
                {
                    string directorNonParsed = moviePageResponse.Substring(moviePageResponse.IndexOf("Director:"));
                    directorNonParsed = directorNonParsed.Substring(directorNonParsed.IndexOf("itemprop=\"name\">"));
                    directorNonParsed = directorNonParsed.Substring(directorNonParsed.IndexOf(">") + 1);

                    for (int i = 0; directorNonParsed.ElementAt(i) != '<'; i++)
                    {
                        this.director += directorNonParsed.ElementAt(i);
                    }
                }
                catch (Exception ex)
                {
                    var properties2 = new Dictionary<string, object>();
                    properties2["time"] = DateTime.Now;
                    properties2["Error Method"] = "retreiveDirector";
                    properties2["Exception"] = ex.ToString();
                    tracker.Track("IMDB Error", properties2);

                    this.director = "Unknown";
                }
            }
            else
            {
                this.director = "Result not found.";
            }
        }

            public void retreiveRunTime()
        {
            //Resetting class varibale
            this.runtime = "";
            if (moviePageResponse != null)
            {
                if (moviePageResponse.IndexOf("Runtime:") != -1)
                {
                    string runTimeNonParsed = moviePageResponse.Substring(moviePageResponse.IndexOf("Runtime:"));
                    runTimeNonParsed = runTimeNonParsed.Substring(runTimeNonParsed.IndexOf(">") + 1);
                    runTimeNonParsed = runTimeNonParsed.Substring(runTimeNonParsed.IndexOf(">") + 1);

                    if (runTimeNonParsed.IndexOf(":") < 15 && runTimeNonParsed.IndexOf(":") >= 0)
                    {
                        runTimeNonParsed = runTimeNonParsed.Substring(runTimeNonParsed.IndexOf(":") + 2);
                    }

                    for (int i = 0; runTimeNonParsed.ElementAt(i) != '<'; i++)
                    {
                        this.runtime += runTimeNonParsed.ElementAt(i);
                    }
                }
                else
                {
                    this.runtime = "Result not found.";
                }
            }
            else
            {
                this.runtime = "Result not found.";
            }
        }

            public void retreiveMPAARating()
        {
            //Resetting class varibale
            this.mpaaRating = "";
            this.mpaaRatingDescription = "";
            if (moviePageResponse != null)
            {
                //If an MPAA rating doesnt exist
                if (moviePageResponse.IndexOf(">MPAA<") == -1)
                {
                    //If an alternate rating doesnt exist
                    if (moviePageResponse.IndexOf("<div class=\"infobar\">") == -1)
                    {
                        this.mpaaRating = "Unknown";
                    }
                    else//If and alternate rating does exist
                    {
                        string mpaaRatingNonParsed = moviePageResponse.Substring(moviePageResponse.IndexOf("<div class=\"infobar\">"));
                        mpaaRatingNonParsed = mpaaRatingNonParsed.Substring(mpaaRatingNonParsed.IndexOf("title=") + 7);
                        for (int i = 0; mpaaRatingNonParsed.ElementAt(i) != '\"'; i++)
                        {
                            this.mpaaRating += mpaaRatingNonParsed.ElementAt(i);
                        }
                        this.mpaaRatingDescription = "Unknown";
                    }
                }
                else//If an MPAA rating does exist
                {
                    string mpaaRatingNonParsed = moviePageResponse.Substring(moviePageResponse.IndexOf(">MPAA<"));
                    mpaaRatingNonParsed = mpaaRatingNonParsed.Substring(mpaaRatingNonParsed.IndexOf("contentRating") + 1);
                    mpaaRatingNonParsed = mpaaRatingNonParsed.Substring(mpaaRatingNonParsed.IndexOf(">") + 1);

                    for (int i = 0; mpaaRatingNonParsed.ElementAt(i) != '<'; i++)
                    {
                        this.mpaaRatingDescription += mpaaRatingNonParsed.ElementAt(i);
                    }

                    this.mpaaRating = this.mpaaRatingDescription;
                }
            }
            else
            {
                this.mpaaRating = "Result not found.";
                this.mpaaRatingDescription = "Result not found.";
            }
        }

            public void retreiveMovieDescription()
            {
                //Resetting class varibale
                this.movieDescription = "";
                if (moviePageResponse != null)
                {
                    string MovieDescriptionNonParsed = moviePageResponse.Substring(moviePageResponse.IndexOf("<h2>Storyline</h2>"));
                    MovieDescriptionNonParsed = MovieDescriptionNonParsed.Substring( MovieDescriptionNonParsed.IndexOf("<p>")+3);

                    for (int i = 0; MovieDescriptionNonParsed.ElementAt(i) != '<'; i++)
                    {
                        this.movieDescription += MovieDescriptionNonParsed.ElementAt(i);
                    }

                    this.movieDescription = this.movieDescription.Replace("\n", "");
                    this.movieDescription = this.movieDescription.Replace("&#x27;", "'");
                    this.movieDescription = this.movieDescription.Replace("&#x26;", " and ");
                    this.movieDescription = this.movieDescription.Replace("&#x22;", "\"");
                }
                else
                {
                    this.movieDescription = "Result not found.";
                }
            }

            public string retreiveArtwork()
            {
                //Creating a urlstring for the artwork
                string imageURL = "";
                if (moviePageResponse != null)
                {
                    string imageURLNonParsed = moviePageResponse.Substring(moviePageResponse.IndexOf("img_primary"));
                    imageURLNonParsed = imageURLNonParsed.Substring(imageURLNonParsed.IndexOf("src=\"") + 5);

                    for (int i = 0; imageURLNonParsed.ElementAt(i) != '\"'; i++)
                    {
                        imageURL += imageURLNonParsed.ElementAt(i);
                    }

                    Console.WriteLine("Image URL: {0}", imageURL);

                    return imageURL;
                }
                else
                {
                    return "Result not found.";
                }
                //string response = imdbCom.DownloadString(imageURL);

                //WebClient webClient = new WebClient();
                //webClient.DownloadFile(imageURL, "image.jpg");
            }

            public string retreiveFanArt()
            {
                //Creating a urlstring for the artwork
                string imageURL = "";
                if (moviePageResponse != null)
                {
                    string fanartResponse = moviePageResponse.Substring(moviePageResponse.IndexOf("\"mediastrip\""));
                    fanartResponse = fanartResponse.Substring(fanartResponse.IndexOf("href") + 6);

                    for (int i = 0; fanartResponse.ElementAt(i) != '\"'; i++)
                    {
                        imageURL += fanartResponse.ElementAt(i);
                    }

                    imageURL = "http://www.imdb.com" + imageURL;

                    fanartResponse = imdbCom.DownloadString(imageURL);
                    fanartResponse = fanartResponse.Substring(fanartResponse.IndexOf("\"photo\""));
                    fanartResponse = fanartResponse.Substring(fanartResponse.IndexOf("src") + 5);

                    imageURL = "";
                    for (int i = 0; fanartResponse.ElementAt(i) != '\"'; i++)
                    {
                        imageURL += fanartResponse.ElementAt(i);
                    }


                    Console.WriteLine("Image URL: {0}", imageURL);
                    return imageURL;
                }
                else
                {
                    return "Result not found.";
                }
                //string response = imdbCom.DownloadString(imageURL);

                //WebClient webClient = new WebClient();
                //webClient.DownloadFile(imageURL, "image.jpg");
            }

            public void retreiveTrailerURL()
            {
                //Resetting class varibale
                this.trailerURL = "";
                if (moviePageResponse != null)
                {
                    string trailerNonParsed = moviePageResponse.Substring(moviePageResponse.IndexOf("/video") + 6);
                    //trailerNonParsed = trailerNonParsed.Substring(trailerNonParsed.IndexOf("\"") + 1);

                    for (int i = 0; trailerNonParsed.ElementAt(i) != '\"'; i++)
                    {
                        this.trailerURL += trailerNonParsed.ElementAt(i);
                    }
                    this.trailerURL = "www.imdb.com/video" + trailerURL;
                }
                else
                {
                    this.trailerURL = "Result not found.";
                }
            }

        #endregion



        #region Public Accessors

            public void getAllInformation(string movieName)
            {
                retreiveTitleID(movieName);
                retreiveMoviePageResponse();
                retreiveGenre();
                retreiveMetaScore();
                retreiveMPAARating();
                retreiveRunTime();
                retreiveTitleAndYear();
                retreiveDirector();
            }

            public string getTitleID()
            {
                if (titleID == "")
                    return "\n";
                else
                    return titleID;
            }

            public string getTitle()
            {
                if (title == "")
                    return "\n";
                else
                    return title;
            }

            public string getGenre()
            {
                if (genre == "")
                    return "\n";
                else
                    return genre;
            }

            public string getIMDBTitle()
            {
                if (imdbTitle == "")
                    return "\n";
                else
                    return imdbTitle;
            }

            public string getMetaScore()
            {
                if (metaScore == "")
                    return "\n";
                else
                    return metaScore;
            }

            public string getDirector()
            {
                if (director == "")
                    return "\n";
                else
                    return director;
            }

            public string getRunTime()
            {
                if (runtime == "")
                    return "\n";
                else
                    return runtime;
            }

            public string getMPAARatingDescription()
            {
                if (mpaaRatingDescription == "")
                    return "\n";
                else
                    return mpaaRatingDescription;
            }

            public string getMPAARating()
            {
                if (mpaaRating == "")
                    return "\n";
                else
                    return mpaaRating;
            }

            public string getMovieDescription()
            {
                if (movieDescription == "")
                    return "\n";
                else
                    return movieDescription;
            }

            public string getYear()
            {
                if (year == "")
                    return "\n";
                else
                    return year;
            }

            public string getTrailerURL()
            {
                if (trailerURL == "")
                    return "\n";
                else
                    return trailerURL;
            }

        #endregion
    }
}
