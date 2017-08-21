using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VideoOrganizer
{
    public partial class ResultsForm : Form
    {
        List<FileDetails> fileResults;
        bool cancelSelected;
        public ResultsForm()
        {
            InitializeComponent();
            fileResults = new List<FileDetails>();
            cancelSelected = false;
        }

        public ResultsForm(List<FileDetails> myResults)
        {
            InitializeComponent();
            fileResults = myResults;
            for (int i = 0; i < myResults.Count; i++)
            {
                ResultsListBox.Items.Add(fileResults[i].fileName);
            }
        }

        //getters and setter
        public FileDetails getSelectedIndex()
        {
            if (cancelSelected == true)
            {
                return null;
            }

            string selectedResult = ResultsListBox.Items[ResultsListBox.SelectedIndex].ToString();

            for (int i = 0; i < fileResults.Count; i++)
            {
                if (selectedResult.CompareTo(fileResults[i].fileName) == 0)
                {
                    return fileResults[i];
                }
            }

            return null;
        }

        //Buttons handlers
        private void OKBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            cancelSelected = true;
            this.Close();
        }

        private void ResultsListBox_DoubleClick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
