namespace AsyncElastic
{
    partial class Form1
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
            filterGrid = new DataGridView();
            listView1 = new ListView();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)filterGrid).BeginInit();
            SuspendLayout();
            // 
            // filterGrid
            // 
            filterGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            filterGrid.Location = new Point(12, 12);
            filterGrid.Name = "filterGrid";
            filterGrid.RowHeadersWidth = 51;
            filterGrid.RowTemplate.Height = 25;
            filterGrid.Size = new Size(1177, 342);
            filterGrid.TabIndex = 0;
            // 
            // listView1
            // 
            listView1.Location = new Point(12, 360);
            listView1.Name = "listView1";
            listView1.Size = new Size(1177, 140);
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // button1
            // 
            button1.Location = new Point(12, 506);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 2;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // Form1
            // 
            ClientSize = new Size(1201, 554);
            Controls.Add(button1);
            Controls.Add(listView1);
            Controls.Add(filterGrid);
            Name = "Form1";
            ((System.ComponentModel.ISupportInitialize)filterGrid).EndInit();
            ResumeLayout(false);
        }

        #endregion


        private DataGridView filterGrid;
        private DataGridView dataGridView1;
        private ListView listView1;
        private Button button1;
    }
}
