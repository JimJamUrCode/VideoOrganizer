using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VideoOrganizer
{
    public class FileDetails
    {
        //Local Information
        public string fileName { get; set; }
        public DateTime creationDate { get; set; }
        public long size { get; set; }
        public string extension { get; set; }
        public string directory { get; set; }
        public bool readOnly { get; set; }

        //Information retreived from the internet
        public string internetTitle { get; set; }
        public string internetYear { get; set; }
        public string genre { get; set; }
        public string reviewScore { get; set; }
        public string director { get; set; }
        public string runtime { get; set; }
        public string MPAARating { get; set; }
        public string description { get; set; }
        public string coverPath { get; set; }
        public string IMDBURL { get; set; }
        public string trailerURL { get; set; }

        public FileDetails()
        {
        }

        public FileDetails(string fileName)
        {
            this.fileName = fileName;
        }

        public FileDetails(string fileName, DateTime creationDate, long size, string extension, string directory, bool readOnly)
        {
            this.fileName = fileName;
            this.creationDate = creationDate;
            this.size = size;
            this.extension = extension;
            this.directory = directory;
            this.readOnly = readOnly;
        }

        public FileDetails(string fileName, DateTime creationDate, long size, string extension, string directory, bool readOnly,
            string internetTitle, string internetYear, string genre, string reviewScore, string director, string runtime, string MPAARating,
            string description, string coverPath, string IMDBURL, string trailerURL)
        {
            this.fileName = fileName;
            this.creationDate = creationDate;
            this.size = size;
            this.extension = extension;
            this.directory = directory;
            this.readOnly = readOnly;
            this.internetTitle = internetTitle;
            this.internetYear = internetYear;
            this.genre = genre;
            this.reviewScore = reviewScore;
            this.director = director;
            this.runtime = runtime;
            this.MPAARating = MPAARating;
            this.description = description;
            this.coverPath = coverPath;
            this.IMDBURL = IMDBURL;
            this.trailerURL = trailerURL;
        }
    }
}
