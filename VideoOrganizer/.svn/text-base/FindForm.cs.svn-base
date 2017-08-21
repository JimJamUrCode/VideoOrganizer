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
    public partial class FindForm : Form
    {
        bool newValueEntered;
        public FindForm()
        {
            InitializeComponent();
            newValueEntered = false;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            newValueEntered = true;
            this.Close();
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            newValueEntered = false;
            this.Close();
        }

        public string getInput()
        {
            if (newValueEntered)
            {
                return InputTB.Text;
            }
            return null;
        }

        private void CheckKeys(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                OKBtn_Click(sender, e);
            }
        }
    }
}
