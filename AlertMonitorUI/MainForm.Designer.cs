namespace AlertMonitorUI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.labelEmergencias = new System.Windows.Forms.Label();
            this.labelEnfermeria = new System.Windows.Forms.Label();
            this.labelMantenimiento = new System.Windows.Forms.Label();
            this.listBoxEmergencias = new System.Windows.Forms.ListBox();
            this.listBoxEnfermeria = new System.Windows.Forms.ListBox();
            this.listBoxMantenimiento = new System.Windows.Forms.ListBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.labelStatus = new System.Windows.Forms.ToolStripStatusLabel();

            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer1.Size = new System.Drawing.Size(1200, 800);
            this.splitContainer1.SplitterDistance = 750;
            this.splitContainer1.TabIndex = 0;

            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tableLayoutPanel1.Controls.Add(this.labelEmergencias, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelEnfermeria, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelMantenimiento, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.listBoxEmergencias, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.listBoxEnfermeria, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.listBoxMantenimiento, 2, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1200, 750);
            this.tableLayoutPanel1.TabIndex = 0;

            // 
            // labelEmergencias
            // 
            this.labelEmergencias.AutoSize = true;
            this.labelEmergencias.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelEmergencias.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.labelEmergencias.ForeColor = Color.DarkRed;
            this.labelEmergencias.Location = new System.Drawing.Point(3, 0);
            this.labelEmergencias.Name = "labelEmergencias";
            this.labelEmergencias.Size = new System.Drawing.Size(394, 30);
            this.labelEmergencias.TabIndex = 0;
            this.labelEmergencias.Text = "Emergencias (0)";
            this.labelEmergencias.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // labelEnfermeria
            // 
            this.labelEnfermeria.AutoSize = true;
            this.labelEnfermeria.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelEnfermeria.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.labelEnfermeria.ForeColor = Color.DarkGreen;
            this.labelEnfermeria.Location = new System.Drawing.Point(403, 0);
            this.labelEnfermeria.Name = "labelEnfermeria";
            this.labelEnfermeria.Size = new System.Drawing.Size(394, 30);
            this.labelEnfermeria.TabIndex = 1;
            this.labelEnfermeria.Text = "Enfermería (0)";
            this.labelEnfermeria.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // labelMantenimiento
            // 
            this.labelMantenimiento.AutoSize = true;
            this.labelMantenimiento.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelMantenimiento.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.labelMantenimiento.ForeColor = Color.DarkBlue;
            this.labelMantenimiento.Location = new System.Drawing.Point(803, 0);
            this.labelMantenimiento.Name = "labelMantenimiento";
            this.labelMantenimiento.Size = new System.Drawing.Size(394, 30);
            this.labelMantenimiento.TabIndex = 2;
            this.labelMantenimiento.Text = "Mantenimiento (0)";
            this.labelMantenimiento.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // listBoxEmergencias
            // 
            this.listBoxEmergencias.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxEmergencias.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxEmergencias.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.listBoxEmergencias.FormattingEnabled = true;
            this.listBoxEmergencias.ItemHeight = 25;
            this.listBoxEmergencias.Location = new System.Drawing.Point(3, 33);
            this.listBoxEmergencias.Name = "listBoxEmergencias";
            this.listBoxEmergencias.Size = new System.Drawing.Size(394, 714);
            this.listBoxEmergencias.TabIndex = 3;
            this.listBoxEmergencias.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ListBox_DrawItem);

            // 
            // listBoxEnfermeria
            // 
            this.listBoxEnfermeria.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxEnfermeria.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxEnfermeria.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.listBoxEnfermeria.FormattingEnabled = true;
            this.listBoxEnfermeria.ItemHeight = 25;
            this.listBoxEnfermeria.Location = new System.Drawing.Point(403, 33);
            this.listBoxEnfermeria.Name = "listBoxEnfermeria";
            this.listBoxEnfermeria.Size = new System.Drawing.Size(394, 714);
            this.listBoxEnfermeria.TabIndex = 4;
            this.listBoxEnfermeria.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ListBox_DrawItem);

            // 
            // listBoxMantenimiento
            // 
            this.listBoxMantenimiento.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxMantenimiento.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxMantenimiento.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.listBoxMantenimiento.FormattingEnabled = true;
            this.listBoxMantenimiento.ItemHeight = 25;
            this.listBoxMantenimiento.Location = new System.Drawing.Point(803, 33);
            this.listBoxMantenimiento.Name = "listBoxMantenimiento";
            this.listBoxMantenimiento.Size = new System.Drawing.Size(394, 714);
            this.listBoxMantenimiento.TabIndex = 5;
            this.listBoxMantenimiento.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ListBox_DrawItem);

            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.labelStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 778);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1200, 22);
            this.statusStrip1.TabIndex = 0;

            // 
            // labelStatus
            // 
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(79, 17);
            this.labelStatus.Text = "Conectando...";

            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hospital Alert System - Monitor";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label labelEmergencias;
        private System.Windows.Forms.Label labelEnfermeria;
        private System.Windows.Forms.Label labelMantenimiento;
        private System.Windows.Forms.ListBox listBoxEmergencias;
        private System.Windows.Forms.ListBox listBoxEnfermeria;
        private System.Windows.Forms.ListBox listBoxMantenimiento;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel labelStatus;

        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            
            if (sender is ListBox listBox && e.Index < listBox.Items.Count)
            {
                e.DrawBackground();
                
                if (listBox.Items[e.Index] is AlertListItem item)
                {
                    using (var brush = new SolidBrush(item.BackColor))
                    {
                        e.Graphics.FillRectangle(brush, e.Bounds);
                    }

                    TextRenderer.DrawText(e.Graphics, item.ToString(), e.Font,
                        e.Bounds, item.ForeColor, TextFormatFlags.VerticalCenter);
                }
                
                e.DrawFocusRectangle();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Personalizar la apariencia de los ListBox
            foreach (var listBox in new[] { listBoxEmergencias, listBoxEnfermeria, listBoxMantenimiento })
            {
                listBox.IntegralHeight = false;
                listBox.BorderStyle = BorderStyle.None;
                listBox.DrawMode = DrawMode.OwnerDrawFixed;
                listBox.ItemHeight = 25;
            }
        }
    }
}