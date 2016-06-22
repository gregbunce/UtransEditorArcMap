namespace UtransEditorAGRC
{
    partial class frmUserInputNotes
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtNotesInput = new System.Windows.Forms.TextBox();
            this.btnUserInputSubmit = new System.Windows.Forms.Button();
            this.lblGoogleAccessCode = new System.Windows.Forms.Label();
            this.txtGoogleAccessCode = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Notes Field:";
            // 
            // txtNotesInput
            // 
            this.txtNotesInput.Location = new System.Drawing.Point(16, 30);
            this.txtNotesInput.Name = "txtNotesInput";
            this.txtNotesInput.Size = new System.Drawing.Size(691, 20);
            this.txtNotesInput.TabIndex = 1;
            // 
            // btnUserInputSubmit
            // 
            this.btnUserInputSubmit.Location = new System.Drawing.Point(602, 75);
            this.btnUserInputSubmit.Name = "btnUserInputSubmit";
            this.btnUserInputSubmit.Size = new System.Drawing.Size(105, 23);
            this.btnUserInputSubmit.TabIndex = 3;
            this.btnUserInputSubmit.Text = "Submit";
            this.btnUserInputSubmit.UseVisualStyleBackColor = true;
            this.btnUserInputSubmit.Click += new System.EventHandler(this.btnUserInputSubmit_Click);
            // 
            // lblGoogleAccessCode
            // 
            this.lblGoogleAccessCode.AutoSize = true;
            this.lblGoogleAccessCode.Location = new System.Drawing.Point(13, 61);
            this.lblGoogleAccessCode.Name = "lblGoogleAccessCode";
            this.lblGoogleAccessCode.Size = new System.Drawing.Size(230, 13);
            this.lblGoogleAccessCode.TabIndex = 4;
            this.lblGoogleAccessCode.Text = "Paste Google Access Code from Web Browser:";
            // 
            // txtGoogleAccessCode
            // 
            this.txtGoogleAccessCode.Location = new System.Drawing.Point(16, 78);
            this.txtGoogleAccessCode.Name = "txtGoogleAccessCode";
            this.txtGoogleAccessCode.Size = new System.Drawing.Size(331, 20);
            this.txtGoogleAccessCode.TabIndex = 5;
            // 
            // frmUserInputNotes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(722, 116);
            this.Controls.Add(this.txtGoogleAccessCode);
            this.Controls.Add(this.lblGoogleAccessCode);
            this.Controls.Add(this.btnUserInputSubmit);
            this.Controls.Add(this.txtNotesInput);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmUserInputNotes";
            this.ShowIcon = false;
            this.Text = "Notes Field for Notify Counties Google Spreadsheet";
            this.Load += new System.EventHandler(this.frmUserInputNotes_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNotesInput;
        private System.Windows.Forms.Button btnUserInputSubmit;
        private System.Windows.Forms.Label lblGoogleAccessCode;
        private System.Windows.Forms.TextBox txtGoogleAccessCode;
    }
}