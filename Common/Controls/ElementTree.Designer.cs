﻿namespace Common.Controls
{
	partial class ElementTree
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
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ElementTree));
			this.treeIconsImageList = new System.Windows.Forms.ImageList(this.components);
			this.contextMenuStripTreeView = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.cutNodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyNodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pasteNodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.nodePropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pastePropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.addNewNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.addMultipleNewNodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteNodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.createGroupWithNodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.renameNodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripDragging = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.moveHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.treeview = new Common.Controls.MultiSelectTreeview();
			this.contextMenuStripTreeView.SuspendLayout();
			this.contextMenuStripDragging.SuspendLayout();
			this.SuspendLayout();
			// 
			// treeIconsImageList
			// 
			this.treeIconsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treeIconsImageList.ImageStream")));
			this.treeIconsImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.treeIconsImageList.Images.SetKeyName(0, "Group");
			this.treeIconsImageList.Images.SetKeyName(1, "GreyBall");
			this.treeIconsImageList.Images.SetKeyName(2, "RedBall");
			this.treeIconsImageList.Images.SetKeyName(3, "GreenBall");
			this.treeIconsImageList.Images.SetKeyName(4, "YellowBall");
			this.treeIconsImageList.Images.SetKeyName(5, "BlueBall");
			this.treeIconsImageList.Images.SetKeyName(6, "WhiteBall");
			// 
			// contextMenuStripTreeView
			// 
			this.contextMenuStripTreeView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutNodesToolStripMenuItem,
            this.copyNodesToolStripMenuItem,
            this.pasteNodesToolStripMenuItem,
            this.toolStripSeparator1,
            this.nodePropertiesToolStripMenuItem,
            this.toolStripSeparator2,
            this.addNewNodeToolStripMenuItem,
            this.addMultipleNewNodesToolStripMenuItem,
            this.deleteNodesToolStripMenuItem,
            this.createGroupWithNodesToolStripMenuItem,
            this.renameNodesToolStripMenuItem});
			this.contextMenuStripTreeView.Name = "contextMenuStripTreeView";
			this.contextMenuStripTreeView.Size = new System.Drawing.Size(155, 236);
			this.contextMenuStripTreeView.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripTreeView_Opening);
			// 
			// cutNodesToolStripMenuItem
			// 
			this.cutNodesToolStripMenuItem.Name = "cutNodesToolStripMenuItem";
			this.cutNodesToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			this.cutNodesToolStripMenuItem.Text = "Cut";
			this.cutNodesToolStripMenuItem.Click += new System.EventHandler(this.cutNodesToolStripMenuItem_Click);
			// 
			// copyNodesToolStripMenuItem
			// 
			this.copyNodesToolStripMenuItem.Name = "copyNodesToolStripMenuItem";
			this.copyNodesToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			this.copyNodesToolStripMenuItem.Text = "Copy";
			this.copyNodesToolStripMenuItem.Click += new System.EventHandler(this.copyNodesToolStripMenuItem_Click);
			// 
			// pasteNodesToolStripMenuItem
			// 
			this.pasteNodesToolStripMenuItem.Name = "pasteNodesToolStripMenuItem";
			this.pasteNodesToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			this.pasteNodesToolStripMenuItem.Text = "Paste";
			this.pasteNodesToolStripMenuItem.Click += new System.EventHandler(this.pasteNodesToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(151, 6);
			// 
			// nodePropertiesToolStripMenuItem
			// 
			this.nodePropertiesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyPropertiesToolStripMenuItem,
            this.pastePropertiesToolStripMenuItem});
			this.nodePropertiesToolStripMenuItem.Name = "nodePropertiesToolStripMenuItem";
			this.nodePropertiesToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			this.nodePropertiesToolStripMenuItem.Text = "Item properties";
			// 
			// copyPropertiesToolStripMenuItem
			// 
			this.copyPropertiesToolStripMenuItem.Name = "copyPropertiesToolStripMenuItem";
			this.copyPropertiesToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
			this.copyPropertiesToolStripMenuItem.Text = "Copy properties";
			this.copyPropertiesToolStripMenuItem.Click += new System.EventHandler(this.copyPropertiesToolStripMenuItem_Click);
			// 
			// pastePropertiesToolStripMenuItem
			// 
			this.pastePropertiesToolStripMenuItem.Name = "pastePropertiesToolStripMenuItem";
			this.pastePropertiesToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
			this.pastePropertiesToolStripMenuItem.Text = "Paste properties";
			this.pastePropertiesToolStripMenuItem.Click += new System.EventHandler(this.pastePropertiesToolStripMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(151, 6);
			// 
			// addNewNodeToolStripMenuItem
			// 
			this.addNewNodeToolStripMenuItem.Name = "addNewNodeToolStripMenuItem";
			this.addNewNodeToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			this.addNewNodeToolStripMenuItem.Text = "Add";
			this.addNewNodeToolStripMenuItem.Click += new System.EventHandler(this.addNewNodeToolStripMenuItem_Click);
			// 
			// addMultipleNewNodesToolStripMenuItem
			// 
			this.addMultipleNewNodesToolStripMenuItem.Name = "addMultipleNewNodesToolStripMenuItem";
			this.addMultipleNewNodesToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			this.addMultipleNewNodesToolStripMenuItem.Text = "Add Multiple";
			this.addMultipleNewNodesToolStripMenuItem.Click += new System.EventHandler(this.addMultipleNewNodesToolStripMenuItem_Click);
			// 
			// deleteNodesToolStripMenuItem
			// 
			this.deleteNodesToolStripMenuItem.Name = "deleteNodesToolStripMenuItem";
			this.deleteNodesToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			this.deleteNodesToolStripMenuItem.Text = "Delete";
			this.deleteNodesToolStripMenuItem.Click += new System.EventHandler(this.deleteNodesToolStripMenuItem_Click);
			// 
			// createGroupWithNodesToolStripMenuItem
			// 
			this.createGroupWithNodesToolStripMenuItem.Name = "createGroupWithNodesToolStripMenuItem";
			this.createGroupWithNodesToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			this.createGroupWithNodesToolStripMenuItem.Text = "Create group";
			this.createGroupWithNodesToolStripMenuItem.Click += new System.EventHandler(this.createGroupWithNodesToolStripMenuItem_Click);
			// 
			// renameNodesToolStripMenuItem
			// 
			this.renameNodesToolStripMenuItem.Name = "renameNodesToolStripMenuItem";
			this.renameNodesToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			this.renameNodesToolStripMenuItem.Text = "Rename";
			this.renameNodesToolStripMenuItem.Click += new System.EventHandler(this.renameNodesToolStripMenuItem_Click);
			// 
			// contextMenuStripDragging
			// 
			this.contextMenuStripDragging.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.moveHereToolStripMenuItem,
            this.copyHereToolStripMenuItem});
			this.contextMenuStripDragging.Name = "contextMenuStripProperties";
			this.contextMenuStripDragging.Size = new System.Drawing.Size(131, 48);
			// 
			// moveHereToolStripMenuItem
			// 
			this.moveHereToolStripMenuItem.Name = "moveHereToolStripMenuItem";
			this.moveHereToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.moveHereToolStripMenuItem.Text = "Move here";
			// 
			// copyHereToolStripMenuItem
			// 
			this.copyHereToolStripMenuItem.Name = "copyHereToolStripMenuItem";
			this.copyHereToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.copyHereToolStripMenuItem.Text = "Copy here";
			// 
			// treeview
			// 
			this.treeview.AllowDrop = true;
			this.treeview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeview.ContextMenuStrip = this.contextMenuStripTreeView;
			this.treeview.Cursor = System.Windows.Forms.Cursors.Default;
			this.treeview.CustomDragCursor = null;
			this.treeview.DragDefaultMode = System.Windows.Forms.DragDropEffects.Move;
			this.treeview.DragDestinationNodeBackColor = System.Drawing.SystemColors.Highlight;
			this.treeview.DragDestinationNodeForeColor = System.Drawing.SystemColors.HighlightText;
			this.treeview.DragSourceNodeBackColor = System.Drawing.SystemColors.ControlLight;
			this.treeview.DragSourceNodeForeColor = System.Drawing.SystemColors.ControlText;
			this.treeview.HideSelection = false;
			this.treeview.ImageIndex = 0;
			this.treeview.ImageList = this.treeIconsImageList;
			this.treeview.Location = new System.Drawing.Point(0, 0);
			this.treeview.Name = "treeview";
			this.treeview.SelectedImageIndex = 0;
			this.treeview.SelectedNodes = ((System.Collections.Generic.List<System.Windows.Forms.TreeNode>)(resources.GetObject("treeview.SelectedNodes")));
			this.treeview.Size = new System.Drawing.Size(200, 400);
			this.treeview.TabIndex = 13;
			this.treeview.UsingCustomDragCursor = false;
			this.treeview.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeview_KeyDown);
			// 
			// ElementTree
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.treeview);
			this.Name = "ElementTree";
			this.Size = new System.Drawing.Size(200, 400);
			this.Load += new System.EventHandler(this.ElementTree_Load);
			this.contextMenuStripTreeView.ResumeLayout(false);
			this.contextMenuStripDragging.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private MultiSelectTreeview treeview;
		private System.Windows.Forms.ImageList treeIconsImageList;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripTreeView;
		private System.Windows.Forms.ToolStripMenuItem cutNodesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyNodesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pasteNodesToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem nodePropertiesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyPropertiesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pastePropertiesToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem addNewNodeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addMultipleNewNodesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteNodesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem createGroupWithNodesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem renameNodesToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripDragging;
		private System.Windows.Forms.ToolStripMenuItem moveHereToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyHereToolStripMenuItem;
	}
}