﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;


namespace Timeline
{
	public class TimelineGrid : TimelineControlBase
	{
		#region Members

		private List<TimelineRow> m_rows;						// the rows in the grid
		private DragState m_dragState = DragState.Normal;		// the current dragging state
		private Point m_lastMouseLocation;						// the location of the mouse at last draw; used to update the dragging.
																// Relative to the control, not the grid canvas.
		private Point m_selectionRectangleStart;				// the location (on the grid canvas) where the selection box starts.
		private Rectangle m_ignoreDragArea;						// the area in which move movements should be ignored, before we start dragging
		private TimelineElement m_mouseDownElement = null;		// the element under the cursor on a mouse click
		private SortedDictionary<int, SnapDetails> m_snapPixels;	// a mapping of pixel location to details to snap to
		private TimeSpan m_totalTime;							// the total amount of time this grid represents
		private TimeSpan m_cursorPosition;						// the current grid 'cursor' position (line drawn vertically)
		private Size m_dragAutoscrollDistance;					// how far in either dimension the mouse has moved outside a bounding area,
																// so we should scroll the viewable pane that direction


		#endregion


		#region Initialization

		public TimelineGrid()
		{
			this.AutoScroll = true;
			this.SetStyle(ControlStyles.ResizeRedraw, true);

			TotalTime = TimeSpan.FromMinutes(1);
			RowSeparatorColor = Color.Black;
			MajorGridlineColor = Color.FromArgb(120, 120, 120);
			GridlineInterval = TimeSpan.FromSeconds(1.0);
			BackColor = Color.FromArgb(140, 140, 140);
			SelectionColor = Color.FromArgb(100, 40, 100, 160);
			SelectionBorder = Color.Blue;
			CursorColor = Color.FromArgb(150, 50, 50, 50);
			CursorWidth = 2.5F;
			CursorPosition = TimeSpan.Zero;
			OnlySnapToCurrentRow = false;		// setting this to 'true' doesn't quite work yet.
			DragThreshold = 8;
			m_dragAutoscrollDistance.Height = m_dragAutoscrollDistance.Width = 0;

			m_rows = new List<TimelineRow>();
			SnapPoints = new SortedDictionary<TimeSpan, int>();
			ScrollTimer = new Timer();
			ScrollTimer.Interval = 20;
			ScrollTimer.Enabled = false;
			ScrollTimer.Tick += ScrollTimerHandler;

			// thse changed events are static for the class. If we make them per element or row
			//  later, we will need to attach/detach from each event manually.
			TimelineRow.RowChanged += RowChangedHandler;
			TimelineRow.RowSelectedChanged += RowSelectedChangedHandler;
		}

		#endregion


		#region Properties

		/// <summary>
		/// The maximum amount of time represented by this Grid.
		/// </summary>
		public TimeSpan TotalTime
		{
			get { return m_totalTime; }
			set { m_totalTime = value; Invalidate(); }
		}

		/// <summary>
		/// The time at the left of the control (the visible beginning).
		/// </summary>
		public override TimeSpan VisibleTimeStart
		{
			get { return base.VisibleTimeStart; }
			set
			{
				if (value < TimeSpan.Zero)
					value = TimeSpan.Zero;

				if (value > TotalTime - VisibleTimeSpan)
					value = TotalTime - VisibleTimeSpan;

				base.VisibleTimeStart = value;
				AutoScrollPosition = new Point((int)timeToPixels(value), -AutoScrollPosition.Y);
				_VisibleTimeStartChanged();
			}
		}

		// this needs to be overridden to maintain the visible time start accurately
		// (as it isn't stored as a value, but inferred through the AutoScroll position).
		public override TimeSpan TimePerPixel
		{
			get { return base.TimePerPixel; }
			set
			{
				TimeSpan start = VisibleTimeStart;
				base.TimePerPixel = value;
				VisibleTimeStart = start;
			}
		}


		public int VerticalOffset
		{
			get { return -AutoScrollPosition.Y; }
			set
			{
				if (value < 0)
					value = 0;

				if (value > AutoScrollMinSize.Height - ClientSize.Height)
					value = AutoScrollMinSize.Height - ClientSize.Height;

				if (-AutoScrollPosition.Y == value)
					return;

				AutoScrollPosition = new Point(-AutoScrollPosition.X, value);
				_VerticalOffsetChanged();
			}
		}

		public List<TimelineRow> Rows
		{
			get { return m_rows; }
			set { m_rows = value; }
		}

		public List<TimelineElement> SelectedElements
		{
			get
			{
				List<TimelineElement> result = new List<TimelineElement>();
				foreach (TimelineRow row in Rows) {
					result.AddRange(row.SelectedElements);
				}
				return result;
			}
		}

		public TimeSpan CursorPosition
		{
			get { return m_cursorPosition; }
			set { m_cursorPosition = value; _CursorMoved(value); Invalidate(); }
		}

		public SortedDictionary<TimeSpan, int> SnapPoints { get; set; }
		public TimeSpan GridlineInterval { get; set; }
		public bool OnlySnapToCurrentRow { get; set; }
		public int DragThreshold { get; set; }
		public Rectangle SelectedArea { get; set; }
	
		// drawing colours, information, etc.
		public Color RowSeparatorColor { get; set; }
		public Color MajorGridlineColor { get; set; }
		public Color SelectionColor { get; set; }
		public Color SelectionBorder { get; set; }
		public Color CursorColor { get; set; }
		public Single CursorWidth { get; set; }

		// private properties
		private bool CtrlPressed { get { return Form.ModifierKeys.HasFlag(Keys.Control); } }
		private Timer ScrollTimer { get; set; }
		private int CurrentDraggingRowIndex { get; set; }

		#endregion


		#region Events

		public event EventHandler<ElementEventArgs> ElementDoubleClicked;
		public event EventHandler<MultiElementEventArgs> ElementsMoved;
		public event EventHandler<TimeSpanEventArgs> CursorMoved;
		public event EventHandler VisibleTimeStartChanged;
		public event EventHandler VerticalOffsetChanged;

		private void _ElementDoubleClicked(TimelineElement te) { if (ElementDoubleClicked != null) ElementDoubleClicked(this, new ElementEventArgs(te)); }
		private void _ElementsMoved(MultiElementEventArgs args) { if (ElementsMoved != null) ElementsMoved(this, args); }
		private void _CursorMoved(TimeSpan t) { if (CursorMoved != null) CursorMoved(this, new TimeSpanEventArgs(t)); }
		private void _VisibleTimeStartChanged() { if (VisibleTimeStartChanged != null) VisibleTimeStartChanged(this, EventArgs.Empty); }
		private void _VerticalOffsetChanged() { if (VerticalOffsetChanged != null) VerticalOffsetChanged(this, EventArgs.Empty); }

		#endregion


		#region Event Handlers - non-mouse events

		protected void RowChangedHandler(object sender, EventArgs e)
		{
			// when dragging, the control will invalidate after it's done, in case multiple elements are changing.
			if (m_dragState != DragState.Dragging)
				Invalidate();
		}

		protected void RowSelectedChangedHandler(object sender, ModifierKeysEventArgs e)
		{
			TimelineRow selectedRow = sender as TimelineRow;

			// if CTRL wasn't down, then we want to clear all the other rows
			if (!e.ModifierKeys.HasFlag(Keys.Control)) {
				ClearSelectedRows();
				selectedRow.Selected = true;
			}
		}

		protected void ScrollTimerHandler(object sender, EventArgs e)
		{
			// move by the number of pixels we are outside, scaled down by a constant
			if (m_dragAutoscrollDistance.Width != 0) {
				TimeSpan offset = pixelsToTime(m_dragAutoscrollDistance.Width / 8);

				if (m_dragState == DragState.Dragging)
					OffsetElementsByTime(SelectedElements, offset);

				if (m_dragState == DragState.Selecting) {
					Point gridLocation = _translateMouseArgs(m_lastMouseLocation);
					UpdateSelectionRectangle(gridLocation);
				}

				// move the view window. Note: this is done after the element movement to prevent some graphical artifacts.
				VisibleTimeStart += offset;
			}
			if (m_dragAutoscrollDistance.Height != 0) {
				VerticalOffset += m_dragAutoscrollDistance.Height / 8;
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);

			if (e.KeyChar == (char)27)  // ESC
            {
				SelectedElements.Clear();
				endDrag();  // do this regardless of if we're dragging or not.
			}
		}

		protected override void OnScroll(ScrollEventArgs se)
		{
			base.OnScroll(se);
			if (se.ScrollOrientation == ScrollOrientation.HorizontalScroll) {
				base.VisibleTimeStart = pixelsToTime(-AutoScrollPosition.X);
			}
		}

		#endregion


		#region Event Handlers - mouse events

		/// <summary>
		/// Translates a MouseEventArgs so that its coordinates represent the coordinates on the underlying timeline, taking into account scroll position.
		/// </summary>
		/// <param name="e"></param>
		private Point _translateMouseArgs(Point originalLocation)
		{
			// Translate this location based on the auto scroll position.
			Point p = originalLocation;
			p.Offset(-AutoScrollPosition.X, -AutoScrollPosition.Y);
			return p;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			Point gridLocation = _translateMouseArgs(e.Location);
			m_mouseDownElement = elementAt(gridLocation);

			if (e.Button == MouseButtons.Left) {
				// we clicked on the background - clear anything that is selected, and begin a
				// 'selection' drag.
				if (m_mouseDownElement == null) {
					if (!CtrlPressed) {
						ClearSelectedElements();
						ClearSelectedRows();
						m_dragState = DragState.Selecting;
						SelectedArea = new Rectangle(gridLocation.X, gridLocation.Y, 0, 0);
						m_lastMouseLocation = e.Location;
						m_selectionRectangleStart = gridLocation;
						m_dragAutoscrollDistance.Height = m_dragAutoscrollDistance.Width = 0;
					}
				}
					// our mouse is down on something
				else {
					if (m_mouseDownElement.Selected) {
						// unselect
						if (CtrlPressed)
							m_mouseDownElement.Selected = false;
					} else {
						// select
						if (!CtrlPressed) {
							ClearSelectedElements();
							ClearSelectedRows();
						}
						m_mouseDownElement.Selected = true;
					}

					dragWait(e.Location);
				}
				Invalidate();
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			Point gridLocation = _translateMouseArgs(e.Location);

			if (e.Button == MouseButtons.Left) {
				if (m_dragState == DragState.Dragging) {
					MultiElementEventArgs evargs = new MultiElementEventArgs { Elements = SelectedElements };
					_ElementsMoved(evargs);

				} else if (m_dragState == DragState.Selecting) {
					// we will only be Selecting if we clicked on the grid background, so on mouse up, check if
					// we didn't move (or very far): if so, move the cursor to the clicked position.
					if (SelectedArea.Width < 2 && SelectedArea.Height < 2) {
						CursorPosition = pixelsToTime(gridLocation.X);
					}

					SelectedArea = new Rectangle();
				} else {
					// If we're not dragging on mouse up, it could be a click on one of multiple
					// selected elements. (In which case we select only that one)
					if (m_mouseDownElement != null && !CtrlPressed) {
						ClearSelectedElements();
						m_mouseDownElement.Selected = true;
					}
				}

				endDrag();  // we always do this, even if we weren't dragging.

				Invalidate();
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			// don't call the base mouse wheel event; it scrolls the grid left and right
			// if there isn't any vertical movement available (ie. no vertical scrollbar)
			//base.OnMouseWheel(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			Point gridLocation = _translateMouseArgs(e.Location);

			if (m_dragState == DragState.Normal)
				return;

			if (m_dragState == DragState.Waiting) {
				if (!m_ignoreDragArea.Contains(e.Location)) {
					//begin the dragging process
					beginDrag(gridLocation);
				}
			}
			if (m_dragState == DragState.Dragging) {

				// if we don't have anything selected, there's no point dragging anything...
				if (SelectedElements.Count == 0)
					return;

				Point d = new Point(e.X - m_lastMouseLocation.X, e.Y - m_lastMouseLocation.Y);
				m_lastMouseLocation = e.Location;

				// calculate the points at which we should start dragging; ie. account for any selected elements.
				// (we subtract VisibleTimeStart to make it relative to the control, instead of the grid canvas.)
				TimeSpan earliestTime = GetEarliestTimeForElements(SelectedElements);
				TimeSpan latestTime = GetLatestTimeForElements(SelectedElements);
				int leftBoundary = (int)timeToPixels(earliestTime - VisibleTimeStart);
				int rightBoundary = (int)timeToPixels(latestTime - VisibleTimeStart);

				// if the mouse moved left, only add it to the scroll size if:
				// 1) the elements are hard left (or more) in the viewport
				// 2) the elements are hard right, and we are moving right. This provides deceleration.
				//    Cap this value to 0.
				if (d.X < 0) {
					if (leftBoundary <= 0)
						m_dragAutoscrollDistance.Width += d.X;
					else if (rightBoundary >= ClientSize.Width && m_dragAutoscrollDistance.Width > 0)
						m_dragAutoscrollDistance.Width = Math.Max(0, m_dragAutoscrollDistance.Width + d.X);
				}

				// if the mouse moved right, do the inverse of the above rules.
				if (d.X > 0) {
					if (rightBoundary >= ClientSize.Width)
						m_dragAutoscrollDistance.Width += d.X;
					else if (leftBoundary <= 0 && m_dragAutoscrollDistance.Width < 0)
						m_dragAutoscrollDistance.Width = Math.Min(0, m_dragAutoscrollDistance.Width + d.X);
				}

				// if the left and right boundaries are within the viewport, then stop all
				// horizontal scrolling. This can happen if the user scrolls, and mouse-wheels
				// (to zoom out). the control is stuck scrolling, and can't be stopped.
				if (leftBoundary > 0 && rightBoundary < ClientSize.Width)
					m_dragAutoscrollDistance.Width = 0;

				m_dragAutoscrollDistance.Height = (e.Y < 0) ? e.Y : ((e.Y > Height) ? e.Y - Height : 0);

				// if we're scrolling, start the timer if needed. If not, vice-versa.
				if (m_dragAutoscrollDistance.Width != 0 || m_dragAutoscrollDistance.Height != 0) {
					if (!ScrollTimer.Enabled)
						ScrollTimer.Start();
				} else {
					if (ScrollTimer.Enabled)
						ScrollTimer.Stop();
				}
                
				// only move the elements here if we aren't going to be auto-dragging while scrolling in the timer events.
				if (d.X != 0 && m_dragAutoscrollDistance.Width == 0) {
					OffsetElementsByTime(SelectedElements, pixelsToTime(d.X));

					// TODO: when reimplementing the snapping, put it in the OffsetElementsByTime method
					//foreach (TimelineElement element in SelectedElements) {
					//    if (m_snapPixels.ContainsKey(gridLocation.X) && m_snapPixels[gridLocation.X].SnapElements.ContainsKey(element)) {
					//        element.StartTime = m_snapPixels[gridLocation.X].SnapElements[element];
					//    } else {
					//        element.StartTime += pixelsToTime(dX);
					//    }
					//}
				}

				// if we've moved vertically, we may need to move elements between rows
				if (d.Y != 0) {
					MoveElementsVerticallyToLocation(SelectedElements, gridLocation);
				}
			}
			if (m_dragState == DragState.Selecting) {
				// TODO: see if more of this can be merged with the dragging state code above, it's a bit
				// similar, but also different enough
				Point d = new Point(e.X - m_lastMouseLocation.X, e.Y - m_lastMouseLocation.Y);
				m_lastMouseLocation = e.Location;

				m_dragAutoscrollDistance.Width = (e.X < 0) ? e.X : ((e.X > ClientSize.Width) ? e.X - ClientSize.Width : 0);
				m_dragAutoscrollDistance.Height = (e.Y < 0) ? e.Y : ((e.Y > Height) ? e.Y - Height : 0);

				// if we're scrolling, start the timer if needed. If not, vice-versa.
				if (m_dragAutoscrollDistance.Width != 0 || m_dragAutoscrollDistance.Height != 0) {
					if (!ScrollTimer.Enabled)
						ScrollTimer.Start();
				} else {
					if (ScrollTimer.Enabled)
						ScrollTimer.Stop();
				}

				UpdateSelectionRectangle(gridLocation);
			}
		}

		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			base.OnMouseDoubleClick(e);

			Point gridLocation = _translateMouseArgs(e.Location);
			TimelineElement elem = elementAt(gridLocation);

			if (elem != null) {
				_ElementDoubleClicked(elem);
			}
		}

		#endregion


		#region Methods - Rows, Elements

		public void ClearSelectedElements()
		{
			foreach (TimelineElement te in SelectedElements)
				te.Selected = false;
		}

		public void ClearSelectedRows()
		{
			foreach (TimelineRow row in Rows) {
				row.Selected = false;
			}
		}

		/// <summary>
		/// Returns the row located at the current point in screen coordinates
		/// </summary>
		/// <param name="p">Screen coordinates</param>
		/// <returns>Row at given point, or null if none exists.</returns>
		protected TimelineRow rowAt(Point p)
		{
			TimelineRow containingRow = null;
			int curheight = 0;
			foreach (TimelineRow row in Rows) {
				if (!row.Visible)
					continue;

				if (p.Y < curheight + row.Height) {
					containingRow = row;
					break;
				}
				curheight += row.Height;
			}

			return containingRow;
		}

		/// <summary>
		/// Returns the element located at the current point in screen coordinates
		/// </summary>
		/// <param name="p">Screen coordinates</param>
		/// <returns>Element at given point, or null if none exists.</returns>
		protected TimelineElement elementAt(Point p)
		{
			// First figure out which row we are in
			TimelineRow containingRow = rowAt(p);

			if (containingRow == null)
				return null;

			// Now figure out which element we are on
			foreach (TimelineElement elem in containingRow) {
				Single elemX = timeToPixels(elem.StartTime);
				Single elemW = timeToPixels(elem.Duration);
				if (p.X >= elemX && p.X <= elemX + elemW)
					return elem;
			}

			return null;
		}

		public List<TimelineElement> ElementsAtTime(TimeSpan time)
		{
			List<TimelineElement> result = new List<TimelineElement>();
			foreach (TimelineRow row in Rows) {
				foreach (TimelineElement elem in row) {
					if ((time >= elem.StartTime) && (time < (elem.StartTime + elem.Duration)))
						result.Add(elem);
				}
			}

			return result;
		}

		// TODO: as per Jono's comment below, if we find performance lacking with large data sets,
		// implement lists of selected elements for both rows and the grid (listens for events from
		// child elements, and attaches/removes them to/from the list).

		// TODO: Considering that the selection is determined by a flag in each element,
		// this is (IMHO) as optimized as this can be.  To do much better, the "selected elements"
		// should instead be a list. (Which can be cleared O(1)). However, that would cause each element
		// draw() to do a lookup in the selected elements list.
		//
		// With "selected" flag in element:  draw one: O(1)   draw all: O(n)		select: O(n)
		// With "selected elements" list:    draw one: O(n)   draw all: O(n^2)		select: less than O(n_row)
		// 
		// Thus, until proven otherwise, I say we leave it like this. A cursory look at CPU usage says we
		// are no worse than Windows Explorer (although it would be hard to be much worse.)
		private void selectElementsWithin(Rectangle SelectedArea)
		{
			TimelineRow startRow = rowAt(SelectedArea.Location);
			TimelineRow endRow = rowAt(SelectedArea.BottomRight());

			TimeSpan selStart = pixelsToTime(SelectedArea.Left);
			TimeSpan selEnd = pixelsToTime(SelectedArea.Right);

			// Iterate all elements of only the rows within our selection.
			bool startFound = false, endFound = false;
			foreach (var row in Rows) {
				if (
					!row.Visible ||					// row is invisible
					endFound ||					// we already passed the end row
					(!startFound && (row != startRow))	//we haven't found the first row, and this isn't it
					) {
					foreach (TimelineElement e in row)
						e.Selected = false;
					continue;
				}


				// If we already found the first row, or we haven't, but this is it
				if (startFound || row == startRow) {
					startFound = true;

					// This row is in our selection
					foreach (var elem in row) {
						elem.Selected = (elem.StartTime < selEnd && elem.EndTime > selStart);
					}

					if (row == endRow) {
						endFound = true;
						continue;
					}
				}

			} // end foreach
		}

		public TimelineRow GetHighestRowForElements(IEnumerable<TimelineElement> elements, bool visibleOnly)
		{
			for (int i = 0; i < Rows.Count; i++) {
				foreach (TimelineElement element in elements) {
					if (visibleOnly && !Rows[i].Visible)
						continue;

					if (Rows[i].ContainsElement(element)) {
						return Rows[i];
					}
				}
			}

			Debug.WriteLine("GetHighestRowForElements: failed. Rows.count is {0}. elements given is {1}.", Rows.Count, elements);

			return null;
		}

		public TimelineRow GetLowestRowForElements(IEnumerable<TimelineElement> elements, bool visibleOnly)
		{
			for (int i = Rows.Count - 1; i >= 0; i--) {
				foreach (TimelineElement element in elements) {
					if (visibleOnly && !Rows[i].Visible)
						continue;

					if (Rows[i].ContainsElement(element)) {
						return Rows[i];
					}
				}
			}

			Debug.WriteLine("GetLowestRowForElements: failed. Rows.count is {0}. elements given is {1}.", Rows.Count, elements);

			return null;
		}

		public TimeSpan GetEarliestTimeForElements(IEnumerable<TimelineElement> elements)
		{
			TimeSpan result = TimeSpan.MaxValue;
			foreach (TimelineElement e in elements) {
				if (e.StartTime < result)
					result = e.StartTime;
			}

			return result;
		}

		public TimeSpan GetLatestTimeForElements(IEnumerable<TimelineElement> elements)
		{
			TimeSpan result = TimeSpan.MinValue;
			foreach (TimelineElement e in elements) {
				if (e.EndTime > result)
					result = e.EndTime;
			}

			return result;
		}

		public void AlignSelectedElementsLeft()
		{
			// find the earliest time of all elements
			TimeSpan earliest = GetEarliestTimeForElements(SelectedElements);

			// Find the earliest time of each row
			var rowsToStartTimes = new Dictionary<TimelineRow,TimeSpan>();
			foreach (TimelineRow row in Rows) {
				TimeSpan time = GetEarliestTimeForElements(row.SelectedElements);

				if (time != TimeSpan.MaxValue) {
					rowsToStartTimes.Add(row, time);
				}
			}

			// Now adjust all elements in each row, such that the leftmost element (in this row)
			// is at the same start time as the alltogether leftmost element.
			foreach (KeyValuePair<TimelineRow, TimeSpan> kvp in rowsToStartTimes)
			{
				// calculate how much to offset elements of this row
				TimeSpan thisRowAdjust = kvp.Value - earliest;

				if (thisRowAdjust == TimeSpan.Zero)
					continue;

				foreach (TimelineElement elem in kvp.Key.SelectedElements)
					elem.StartTime -= thisRowAdjust;
			}

		}

		public void OffsetElementsByTime(IEnumerable<TimelineElement> elements, TimeSpan offset)
		{
			// TODO: this is where the snapping should happen, will need to add it back in later.

			// check to see if the offset that is applied to all elements will take it outside
			// the total time at all. If so, use a smaller offset, or none at all.

			// if we're going backwards, check that the earliest time isn't already at zero, or
			// that the move will try to take it past 0.
			if (offset.Ticks < 0) {
				TimeSpan earliest = GetEarliestTimeForElements(elements);
				if (earliest < -offset) {
					offset = -earliest;
				}
			}

			// same for moving forwards.
			if (offset.Ticks > 0) {
				TimeSpan latest = GetLatestTimeForElements(elements);
				if (TotalTime - latest < offset)
					offset = TotalTime - latest;
			}

			// if the offset was zero, or was set to it, we don't need to do anything else now.
			if (offset == TimeSpan.Zero)
				return;

			foreach (TimelineElement e in elements) {
				e.StartTime += offset;
			}

			Invalidate();
		}

		public void MoveElementsVerticallyToLocation(IEnumerable<TimelineElement> elements, Point gridLocation)
		{
			TimelineRow destRow = rowAt(gridLocation);

			if (destRow == null)
				return;

			if (Rows.IndexOf(destRow) == CurrentDraggingRowIndex)
				return;

			lock (this) {
				List<TimelineRow> visibleRows = new List<TimelineRow>();

				for (int i = 0; i < Rows.Count; i++) {
					if (Rows[i].Visible)
						visibleRows.Add(Rows[i]);
				}

				int visibleRowsToMove = visibleRows.IndexOf(destRow) - visibleRows.IndexOf(Rows[CurrentDraggingRowIndex]);

				// find the highest and lowest visible rows with selected elements in them
				int topElementVisibleRowIndex = visibleRows.IndexOf(GetHighestRowForElements(elements, true));
				int bottomElementVisibleRowIndex = visibleRows.IndexOf(GetLowestRowForElements(elements, true));

				Debug.WriteLine("-----------------------------------------------------------------------");
				Debug.WriteLine("move elements vertically: at start: {0} visible rows, want to move by {1}. Top visible is {2}, Bottom visible is {3}. Current (real) row index is {4}, which maps to visible row {5}.",
					visibleRows.Count, visibleRowsToMove, topElementVisibleRowIndex, bottomElementVisibleRowIndex, CurrentDraggingRowIndex, visibleRows.IndexOf(Rows[CurrentDraggingRowIndex]));

				if (visibleRowsToMove < 0 && -visibleRowsToMove > topElementVisibleRowIndex)
					visibleRowsToMove = -topElementVisibleRowIndex;

				if (visibleRowsToMove > 0 && visibleRowsToMove > visibleRows.Count - 1 - bottomElementVisibleRowIndex)
					visibleRowsToMove = visibleRows.Count - 1 - bottomElementVisibleRowIndex;

				if (visibleRowsToMove != 0) {

					Debug.WriteLine("move elements vertically: after checking, visible rows to move is now {0}.", visibleRowsToMove);

					Dictionary<TimelineElement, bool> elementsToMove = new Dictionary<TimelineElement, bool>();
					foreach (TimelineElement e in elements) {
						elementsToMove.Add(e, false);
					}


					for (int i = 0; i < visibleRows.Count; i++) {
						List<TimelineElement> elementsMoved = new List<TimelineElement>();

						Debug.WriteLine("move elements vertically: iterating through visible row number {0}.", i);
						// go through each element that hasn't been moved yet, and move it if it's in this row.
						foreach (KeyValuePair<TimelineElement, bool> kvp in elementsToMove) {
							TimelineElement element = kvp.Key;
							bool moved = kvp.Value;

							Debug.WriteLine("move elements vertically:     iterating through element \"{0}\", moved is {1}.", element.Tag, moved);

							// if the element has already been moved, ignore it
							if (moved)
								continue;

							// if the current element is in the current visble row, move it to wherever it needs
							// to go, and also flag that it has been moved
							if (visibleRows[i].ContainsElement(element)) {
								Debug.WriteLine("move elements vertically:         moving element \"{0}\" from row {1} to row {2}.", element.Tag, i, i + visibleRowsToMove);

								TimelineRow source = visibleRows[i];
								source.RemoveElement(element);
								Debug.WriteLine("move elements vertically:             removed element from visible row: {0}, which is named \"{1}\".", i, source.Name);

								TimelineRow destination = visibleRows[i + visibleRowsToMove];
								destination.AddElement(element);
								Debug.WriteLine("move elements vertically:             added element to visible row:     {0}, which is named \"{1}\".", i + visibleRowsToMove, destination.Name);
								Debug.WriteLine("move elements vertically:                 does the destination row think it contains the element? {0}", destination.ContainsElement(element));
								Debug.WriteLine("move elements vertically:                 what is the index of this element in the destination row? {0}", destination.IndexOfElement(element));
								int j = 0;
								foreach (TimelineElement de in destination) {
									Debug.WriteLine("move elements vertically:                 item {0}: {1}, starttime {2}", j, de.Tag, de.StartTime);
									j++;
								}

								elementsMoved.Add(element);
							}
						}

						foreach (TimelineElement e in elementsMoved)
							elementsToMove[e] = true;
					}

					Debug.WriteLine("move elements vertically: CurrentDraggingRowIndex was: {0}", CurrentDraggingRowIndex);

					CurrentDraggingRowIndex = Rows.IndexOf(visibleRows[(visibleRows.IndexOf(Rows[CurrentDraggingRowIndex]) + visibleRowsToMove)]);

					Debug.WriteLine("move elements vertically: CurrentDraggingRowIndex now: {0}", CurrentDraggingRowIndex);




					foreach (KeyValuePair<TimelineElement, bool> kvp in elementsToMove) {
						if (kvp.Value != true) {
							Debug.WriteLine("********************************* ERROR *********************************");
							Debug.WriteLine("element was not moved: {0}", kvp.Key.Tag);
							foreach (TimelineRow r in visibleRows) {
								Debug.WriteLine("    visible row {0} contains element: {1}", r.Name, r.ContainsElement(kvp.Key));
							}
							foreach (TimelineRow r in Rows) {
								Debug.WriteLine("    normal  row {0} contains element: {1}", r.Name, r.ContainsElement(kvp.Key));
							}
							Debug.WriteLine("********************************* ERROR *********************************");
						}

					}

				}
			}





			//// figure out how many visible rows we need to move the elements by
			//int visibleRowsToMove = 0;
			//int direction = (rowIndex - CurrentDraggingRowIndex > 0) ? 1 : -1;

			//for (int i = CurrentDraggingRowIndex; i != rowIndex; i += direction) {
			//    if (Rows[i].Visible)
			//        visibleRowsToMove += direction;
			//}

			//// find the highest and lowest rows with selected elements in them
			//int topElementRowIndex = Rows.IndexOf(GetHighestRowForElements(elements));
			//int bottomElementRowIndex = Rows.IndexOf(GetLowestRowForElements(elements));

			//// if we're moving up in the list, make sure there's enough rows above to move the elements into
			//if (visibleRowsToMove < 0) {
			//    int availableRows = 0;
			//    for (int i = topElementRowIndex - 1; i >= 0; i--) {
			//        if (Rows[i].Visible)
			//            availableRows++;

			//        if (availableRows >= -visibleRowsToMove)
			//            break;
			//    }

			//    if (availableRows < -visibleRowsToMove)
			//        visibleRowsToMove = -availableRows;
			//}


			//// check for row availability for downwards movement, as well
			//if (visibleRowsToMove > 0) {
			//    int availableRows = 0;
			//    for (int i = bottomElementRowIndex + 1; i < Rows.Count; i++) {
			//        if (Rows[i].Visible)
			//            availableRows++;

			//        if (availableRows >= visibleRowsToMove)
			//            break;
			//    }

			//    if (availableRows < visibleRowsToMove)
			//        visibleRowsToMove = availableRows;
			//}

			//// we may have reduced the rows to move to 0 above (if we're already at a limit), if so, we're done
			//if (visibleRowsToMove != 0) {
			//    List<TimelineElement> elementsStillToMove = new List<TimelineElement>(elements);
			//    List<TimelineElement> elementsMovedThisIteration = new List<TimelineElement>();
			//    bool currentRowIndexHasBeenUpdated = false;

			//    // go through all the rows. If any of them contain an element that needs to be moved,
			//    // move it up/down by the appropriate number of visible rows. Skip hidden ones.
			//    for (int i = 0; i < Rows.Count; i++) {
			//        if (!Rows[i].Visible)
			//            continue;

			//        // go through each element that hasn't been moved yet, and move it if it's in this row.
			//        foreach (TimelineElement element in elementsStillToMove) {
			//            if (Rows[i].ContainsElement(element)) {
			//                for (int newRow = i + direction, delta = 0; newRow >= 0 && newRow < Rows.Count; newRow += direction) {
			//                    if (!Rows[newRow].Visible)
			//                        continue;

			//                    delta++;

			//                    if (delta >= Math.Abs(visibleRowsToMove)) {
			//                        Rows[i].RemoveElement(element);
			//                        Rows[newRow].AddElement(element);
			//                        elementsMovedThisIteration.Add(element);

			//                        // while we're here, we can update the current row index that the mouse is on, for future
			//                        // reference. However, only if it's the same as the row being moved.
			//                        if (!currentRowIndexHasBeenUpdated && CurrentDraggingRowIndex == i) {
			//                            CurrentDraggingRowIndex = newRow;
			//                            currentRowIndexHasBeenUpdated = true;
			//                        }

			//                        break;
			//                    }
			//                }
			//            }
			//        }

			//        // what's a nicer way to do this? if we try and remove the element above,
			//        // while iterating through the list, it complains bitterly.
			//        foreach (TimelineElement element in elementsMovedThisIteration) {
			//            elementsStillToMove.Remove(element);
			//        }

			//        elementsMovedThisIteration.Clear();
			//    }
			//}
		}


		#endregion


		#region Methods - UI

		private void dragWait(Point location)
		{
			// begin the dragging process -- calculate a area outside which a drag starts
			m_dragState = DragState.Waiting;
			m_ignoreDragArea = new Rectangle(location.X - DragThreshold, location.Y - DragThreshold, DragThreshold*2, DragThreshold*2);
			m_lastMouseLocation = location;
		}

		private void beginDrag(Point location)
		{
			m_dragState = DragState.Dragging;
			this.Cursor = Cursors.Hand;

			m_dragAutoscrollDistance.Height = m_dragAutoscrollDistance.Width = 0;

			CurrentDraggingRowIndex = Rows.IndexOf(rowAt(location));

			// calculate all the snap points (in pixels) for all selected elements
			// for every visible drag point (and a width either side, so they can snap
			// to non-visible points that are close)
			m_snapPixels = new SortedDictionary<int, SnapDetails>();

			foreach (KeyValuePair<TimeSpan, int> kvp in SnapPoints) {
				if ((kvp.Key >= VisibleTimeStart - VisibleTimeSpan) &&
					(kvp.Key <= VisibleTimeEnd + VisibleTimeSpan)) {

					int snapTimePixelCentre = (int)timeToPixels(kvp.Key);
					int snapRange = kvp.Value;
					int snapLevel = kvp.Value;

					List<TimelineElement> snapElements;

					if (OnlySnapToCurrentRow) {
						TimelineRow row = rowAt(location);
						if (row != null)
							snapElements = row.SelectedElements;
						else
							snapElements = new List<TimelineElement>();
					} else {
						snapElements = SelectedElements;
					}

					foreach (TimelineElement element in snapElements) {
						int elementPixelStart = (int)timeToPixels(element.StartTime);
						int elementPixelEnd = (int)timeToPixels(element.StartTime + element.Duration);

						// iterate through all pixels for this particular snap point, for this element
						for (int offset = -snapRange; offset <= snapRange; offset++) {

							// calculate the relative pixel (to the mouse location) for this point
							int rp = location.X + snapTimePixelCentre + offset - elementPixelStart;

							bool addNewSnapDetail = false;

							// if it doesn't have a Snap entry for this item, make one
							if (!m_snapPixels.ContainsKey(rp)) {
								addNewSnapDetail = true;
							} else {
								// if it does, we have to figure out an intelligent way to combine them. If it's
								// going to snap to the same pixel, then just add it to the list. Also update
								// the priority if needed.
								if (m_snapPixels[rp].DestinationPixel == rp - offset) {
									m_snapPixels[rp].SnapElements[element] = kvp.Key;
									m_snapPixels[rp].SnapLevel = Math.Max(m_snapPixels[rp].SnapLevel, snapLevel);
								}
									// if it's not going to snap to the same pixel as the existing one, then only
									// update it if the new one's of a higher priority.
								else {
									if (m_snapPixels[rp].SnapLevel < snapLevel) {
										addNewSnapDetail = true;
									}
								}
							}

							// add the new one if needed
							if (addNewSnapDetail) {
								SnapDetails sd = new SnapDetails();
								sd.DestinationPixel = rp - offset;
								sd.SnapLevel = snapLevel;
								sd.SnapElements = new Dictionary<TimelineElement, TimeSpan>();
								sd.SnapElements[element] = kvp.Key;
								m_snapPixels[rp] = sd;
							}


							// do the same for the end of the element
							addNewSnapDetail = false;
							rp = location.X + snapTimePixelCentre + offset - elementPixelEnd;




							// if it doesn't have a Snap entry for this item, make one
							if (!m_snapPixels.ContainsKey(rp)) {
								addNewSnapDetail = true;
							} else {
								// if it does, we have to figure out an intelligent way to combine them. If it's
								// going to snap to the same pixel, then just add it to the list. Also update
								// the priority if needed.
								if (m_snapPixels[rp].DestinationPixel == rp - offset) {
									m_snapPixels[rp].SnapElements[element] = kvp.Key - element.Duration;
									m_snapPixels[rp].SnapLevel = Math.Max(m_snapPixels[rp].SnapLevel, snapLevel);
								}
									// if it's not going to snap to the same pixel as the existing one, then only
									// update it if the new one's of a higher priority.
								else {
									if (m_snapPixels[rp].SnapLevel < snapLevel) {
										addNewSnapDetail = true;
									}
								}
							}

							// add the new one if needed
							if (addNewSnapDetail) {
								SnapDetails sd = new SnapDetails();
								sd.DestinationPixel = rp - offset;
								sd.SnapLevel = snapLevel;
								sd.SnapElements = new Dictionary<TimelineElement, TimeSpan>();
								sd.SnapElements[element] = kvp.Key - element.Duration;
								m_snapPixels[rp] = sd;
							}
						}
					}
				}
			}
		}

		private void endDrag()
		{
			m_dragState = DragState.Normal;
			this.Cursor = Cursors.Default;
			if (ScrollTimer.Enabled)
				ScrollTimer.Stop();
		}

		private void UpdateSelectionRectangle(Point gridLocation)
		{
			Point topLeft = new Point(Math.Min(m_selectionRectangleStart.X, gridLocation.X), Math.Min(m_selectionRectangleStart.Y, gridLocation.Y));
			Point bottomRight = new Point(Math.Max(m_selectionRectangleStart.X, gridLocation.X), Math.Max(m_selectionRectangleStart.Y, gridLocation.Y));

			SelectedArea = Util.RectangleFromPoints(topLeft, bottomRight);
			selectElementsWithin(SelectedArea);
			Invalidate();
		}


		#endregion


		#region Drawing

		private int _drawRows(Graphics g)
		{
			// Draw row separators
			int curY = 0;

			using (Pen p = new Pen(RowSeparatorColor))
			using (SolidBrush b = new SolidBrush(SelectionColor))
			{
				foreach (TimelineRow row in Rows)
				{
					if (!row.Visible)
						continue;

					Point selectedTopLeft = new Point((-AutoScrollPosition.X), curY);
					curY += row.Height;
					Point selectedBottomRight = new Point((-AutoScrollPosition.X) + Width, curY);
					Point lineLeft = new Point((-AutoScrollPosition.X), curY);
					Point lineRight = new Point((-AutoScrollPosition.X) + Width, curY);
					
					if (row.Selected)
						g.FillRectangle(b, Util.RectangleFromPoints(selectedTopLeft, selectedBottomRight));
					g.DrawLine(p, lineLeft, lineRight);
				}
			}

			return curY;
		}

		private void _drawGridlines(Graphics g)
		{
			// Draw vertical gridlines
			Single interval = timeToPixels(GridlineInterval);

			// calculate first tick - (it is the first multiple of interval greater than start)
			// believe it or not, this math is correct :-)
			Single start = timeToPixels(VisibleTimeStart) - (timeToPixels(VisibleTimeStart) % interval) + interval;

			using (Pen p = new Pen(MajorGridlineColor))
			{
				p.DashStyle = DashStyle.Dash;
				for (Single x = start; x < start + Width; x += interval)
				{
					g.DrawLine(p, x, (-AutoScrollPosition.Y), x, (-AutoScrollPosition.Y) + Height);
				}
			}
		}

		private void _drawSnapPoints(Graphics g)
		{
			using (Pen p = new Pen(Color.Blue))
			{
				// iterate through all snap points, and if it's visible, draw it
				foreach (KeyValuePair<TimeSpan, int> kvp in SnapPoints)
				{
					if (kvp.Key >= VisibleTimeStart && kvp.Key < VisibleTimeEnd)
					{
						Single x = timeToPixels(kvp.Key);
						p.DashPattern = new float[] { kvp.Value, kvp.Value };
						g.DrawLine(p, x, 0, x, AutoScrollMinSize.Height);
					}
				}
			}
		}

		private void _drawElements(Graphics g)
		{
			// Draw each row
			int top = 0;    // y-coord of top of current row
			foreach (TimelineRow row in Rows) {
				if (!row.Visible)
					continue;

				// Draw each element
				foreach (var element in row) {

					Point location = new Point((int)timeToPixels(element.StartTime), top);
					Size size = new Size((int)timeToPixels(element.Duration), row.Height);

                    // The rectangle where this element will be drawn
                    Rectangle dstRect = new Rectangle(location, size);

                    // The rectangle this element will draw itself in
                    Rectangle srcRect = new Rectangle(new Point(0, 0), size);

                    // Perform the transformation and save the state.
                    GraphicsContainer containerState = g.BeginContainer(dstRect, srcRect, GraphicsUnit.Pixel);

                    // Prevent the element from drawing outside its bounds
                    g.Clip = new System.Drawing.Region(srcRect);
                    
                    element.Draw(g, srcRect);

                    g.EndContainer(containerState);
				}

				top += row.Height;  // next row starts just below this row
			}
		}

		private void _drawSelection(Graphics g)
		{
			if (SelectedArea == null)
				return;

			using (SolidBrush b = new SolidBrush(SelectionColor))
			{
				g.FillRectangle(b, SelectedArea);
			}
			using (Pen p = new Pen(SelectionBorder))
			{
				g.DrawRectangle(p, SelectedArea);
			}
		}

		private void _drawCursor(Graphics g)
		{
			using (Pen p = new Pen(CursorColor, CursorWidth)) {
				g.DrawLine(p, timeToPixels(CursorPosition), 0, timeToPixels(CursorPosition), AutoScrollMinSize.Height);
			}
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			try {
				e.Graphics.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);

				_drawGridlines(e.Graphics);
				int totalHeight = _drawRows(e.Graphics);
				AutoScrollMinSize = new Size((int)timeToPixels(TotalTime), totalHeight);
				_drawSnapPoints(e.Graphics);
				_drawElements(e.Graphics);
				_drawSelection(e.Graphics);
				_drawCursor(e.Graphics);

			} catch (Exception ex) {
				MessageBox.Show("Unhandled Exception while drawing TimelineGrid:\n\n" + ex.Message);
				throw;
			}
		}

		#endregion
    }

	class SnapDetails
	{
		public int DestinationPixel;
		public int SnapLevel;
		public Dictionary<TimelineElement, TimeSpan> SnapElements;
	}

	// Enumerations
	enum DragState
	{
		/// <summary>
		/// Not dragging, mouse is up.
		/// </summary>
		Normal = 0,

		/// <summary>
		/// Mouse down, but hasn't moved past threshold yet to be considered dragging
		/// </summary>
		Waiting,

		/// <summary>
		/// Actively dragging objects
		/// </summary>
		Dragging,

		/// <summary>
		/// Like "Dragging", but dragging on the background, not an object
		/// </summary>
		Selecting,


	}
}
