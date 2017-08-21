using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VideoOrganizer
{
    class CustomListBox : ListBox
    {
        //public List<string> listBoxStrings = new List<string>();
        public List<FileDetails> fileDetailList;
        int sortBy;

        public CustomListBox()
            : base()
        {
            fileDetailList = new List<FileDetails>();
            sortBy = 0;
        }
        protected override void Sort()
        {
            DetermineSortType();
        }

        //Helper Methods
        public void DetermineSortType()
        {
            if (sortBy == 0)
            {
                SortAscendingAlphabetically();
            }
            else if (sortBy == 1)
            {
                SortDescending();
            }
            else if (sortBy == 2)
            {
                SortAscendingCreationDate();
            }
            else if (sortBy == 3)
            {
                SortDescending();
            }
            else if (sortBy == 4)
            {
                SortAscendingSize();
            }
            else if (sortBy == 5)
            {
                SortDescending();
            }
            else if (sortBy == 6)
            {
                SortAscendingFileType();
            }
            else if (sortBy == 7)
            {
                SortDescending();
            }
        }

        private void UpdateListBox()
        {
            this.Items.Clear();

            for (int i = 0; i < fileDetailList.Count; i++)
            {
                this.Items.Add(fileDetailList[i].fileName);
            }
        }


        //Sorting Methods
        public void SortAscendingAlphabetically()
        {
            int swaps = 1;

            while (swaps != 0)
            {
                swaps = 0;
                for (int i = 0; i < fileDetailList.Count-1; i++)
                {
                    if (fileDetailList[i].fileName.CompareTo(fileDetailList[i + 1].fileName) > 0)
                    {
                        FileDetails tempFileDetails = fileDetailList[i];
                        fileDetailList[i] = fileDetailList[i + 1];
                        fileDetailList[i + 1] = tempFileDetails;
                        swaps++;
                    }
                }
            }
            UpdateListBox();
        }

        public void SortAscendingCreationDate()
        {
            int swaps = 1;

            while (swaps != 0)
            {
                swaps = 0;
                for (int i = 0; i < fileDetailList.Count-1; i++)
                {
                    if (fileDetailList[i].creationDate < fileDetailList[i + 1].creationDate)
                    {
                        FileDetails tempFileDetails = fileDetailList[i];
                        fileDetailList[i] = fileDetailList[i + 1];
                        fileDetailList[i + 1] = tempFileDetails;
                        swaps++;
                    }
                }
            }
            UpdateListBox();
        }

        public void SortAscendingSize()
        {
            int swaps = 1;

            while (swaps != 0)
            {
                swaps = 0;
                for (int i = 0; i < fileDetailList.Count - 1; i++)
                {
                    if (fileDetailList[i].size > fileDetailList[i + 1].size)
                    {
                        FileDetails tempFileDetails = fileDetailList[i];
                        fileDetailList[i] = fileDetailList[i + 1];
                        fileDetailList[i + 1] = tempFileDetails;
                        swaps++;
                    }
                }
            }
            UpdateListBox();
        }

        public void SortAscendingFileType()
        {
            SortAscendingAlphabetically();

            int swaps = 1;

            while (swaps != 0)
            {
                swaps = 0;
                for (int i = 0; i < fileDetailList.Count - 1; i++)
                {
                    //string first = fileDetailList[i].fileName + fileDetailList[i].getExtension();
                    //string second = fileDetailList[i + 1].fileName + fileDetailList[i + 1].getExtension();

                    string first = fileDetailList[i].extension;
                    string second = fileDetailList[i + 1].extension;

                    if (first.CompareTo(second) > 0)
                    {
                        FileDetails tempFileDetails = fileDetailList[i];
                        fileDetailList[i] = fileDetailList[i + 1];
                        fileDetailList[i + 1] = tempFileDetails;
                        swaps++;
                    }
                }
            }
            UpdateListBox();
        }

        public void SortDescending()
        {
            /*This method must be called directly after one of the sort ascending methods,
             * Otherwise it will not sort correctly
             */
            List<FileDetails> tempFileDetails = new List<FileDetails>();

                for (int i = fileDetailList.Count-1; i >= 0 ; i--)
                {
                       tempFileDetails.Add(fileDetailList[i]);
                }
                fileDetailList = tempFileDetails;
            UpdateListBox();
        }

        //Getters and Setters
        public void setSortBy(int sort)
        {
            this.sortBy = sort;
        }

        public int getSortBy()
        {
            return sortBy;
        }

        public void SetListBoxContents(List<FileDetails> newDetails)
        {            
            //Setting the list to its new values
            this.fileDetailList = newDetails;

            //Sorting the list according to the type selected
            Sort();
            this.Update();
        }
    }
}
