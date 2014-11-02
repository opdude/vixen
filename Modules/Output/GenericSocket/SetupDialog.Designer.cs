namespace VixenModules.Output.GenericSocket
{
    partial class SetupDialog
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOkay = new System.Windows.Forms.Button();
            this.gbPacketData = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbPort = new System.Windows.Forms.TextBox();
            this.gbPacketData.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(220, 79);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOkay
            // 
            this.btnOkay.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOkay.Location = new System.Drawing.Point(139, 79);
            this.btnOkay.Name = "btnOkay";
            this.btnOkay.Size = new System.Drawing.Size(75, 23);
            this.btnOkay.TabIndex = 1;
            this.btnOkay.Text = "OK";
            this.btnOkay.UseVisualStyleBackColor = true;
            this.btnOkay.Click += new System.EventHandler(this.btnOkay_Click);
            // 
            // gbPacketData
            // 
            this.gbPacketData.Controls.Add(this.label2);
            this.gbPacketData.Controls.Add(this.tbPort);
            this.gbPacketData.Location = new System.Drawing.Point(14, 12);
            this.gbPacketData.Name = "gbPacketData";
            this.gbPacketData.Size = new System.Drawing.Size(281, 61);
            this.gbPacketData.TabIndex = 2;
            this.gbPacketData.TabStop = false;
            this.gbPacketData.Text = "Socekt Data";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Port";
            // 
            // tbPort
            // 
            this.tbPort.Location = new System.Drawing.Point(67, 26);
            this.tbPort.Name = "tbPort";
            this.tbPort.Size = new System.Drawing.Size(208, 20);
            this.tbPort.TabIndex = 3;
            // 
            // SetupDialog
            // 
            this.AcceptButton = this.btnOkay;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(307, 114);
            this.Controls.Add(this.gbPacketData);
            this.Controls.Add(this.btnOkay);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generic Serial Setup";
            this.gbPacketData.ResumeLayout(false);
            this.gbPacketData.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOkay;
        private System.Windows.Forms.GroupBox gbPacketData;
        private System.Windows.Forms.TextBox tbPort;
        private System.Windows.Forms.Label label2;
    }
}