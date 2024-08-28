namespace arduino_awaiter
{
    partial class MyForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            buttonHome = new Button();
            richTextBox = new RichTextBox();
            SuspendLayout();
            // 
            // buttonHome
            // 
            buttonHome.Location = new Point(12, 33);
            buttonHome.Name = "buttonHome";
            buttonHome.Size = new Size(132, 34);
            buttonHome.TabIndex = 0;
            buttonHome.Text = "Home";
            buttonHome.UseVisualStyleBackColor = true;
            // 
            // richTextBox
            // 
            richTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox.Location = new Point(169, 33);
            richTextBox.Name = "richTextBox";
            richTextBox.Size = new Size(586, 399);
            richTextBox.TabIndex = 1;
            richTextBox.Text = "";
            // 
            // MyForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(767, 444);
            Controls.Add(richTextBox);
            Controls.Add(buttonHome);
            Name = "MyForm";
            Text = "MyForm";
            ResumeLayout(false);
        }

        #endregion

        private Button buttonHome;
        private RichTextBox richTextBox;
    }
}
