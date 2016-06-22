using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtransEditorAGRC
{
    public partial class frmUserInputNotes : Form
    {
        public frmUserInputNotes()
        {
            InitializeComponent();
        }

        private void btnUserInputSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                //assign variables for google spreadsheet
                clsGlobals.strUserInputForSpreadsheet = txtNotesInput.Text.Trim();
                clsGlobals.strUserInputGoogleAccessCode = txtGoogleAccessCode.Text.Trim();


                //close the form
                clsGlobals.UserInputNotes.Close();
                clsGlobals.UserInputNotes = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Message: " + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine +
                "Error Source: " + Environment.NewLine + ex.Source + Environment.NewLine + Environment.NewLine +
                "Error Location:" + Environment.NewLine + ex.StackTrace,
                "UTRANS Editor tool error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void frmUserInputNotes_Load(object sender, EventArgs e)
        {
            try
            {
                if (clsGlobals.boolGoogleHasAccessCode == true)
                {
                    //show the imput box and label for the acces code
                    lblGoogleAccessCode.Visible = false;
                    txtGoogleAccessCode.Visible = false;
                }
                else
                {
                    lblGoogleAccessCode.Visible = true;
                    txtGoogleAccessCode.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Message: " + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine +
                "Error Source: " + Environment.NewLine + ex.Source + Environment.NewLine + Environment.NewLine +
                "Error Location:" + Environment.NewLine + ex.StackTrace,
                "UTRANS Editor tool error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }



        }
    }
}
