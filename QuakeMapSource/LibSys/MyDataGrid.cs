/*
* Copyright (c) 2002..., Sergei Grichine
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of Sergei Grichine nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY Sergei Grichine ''AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL Sergei Grichine BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*
* this is a X11 (BSD Revised) license - you do not have to publish your changes,
* although doing so, donating and contributing is always appreciated
*/

using System;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;

namespace LibSys
{
	public delegate void BoolValueChangedEventHandler(object sender, BoolValueChangedEventArgs e);

	#region class MyDataGrid allows to scroll to particular row:
	public class MyDataGrid : System.Windows.Forms.DataGrid
	{
		public void ScrollToRow(int row)
		{
			if(this.DataSource != null)
			{
				this.GridVScrolled(this, new ScrollEventArgs(ScrollEventType.LargeIncrement, row)); 
			}
		}
	}
	#endregion

	#region DataGrid custom event - BoolValueChangedEventHandler

	public class BoolValueChangedEventArgs : EventArgs
	{
		private int _column;
		private int _row;
		private bool _value;

		public BoolValueChangedEventArgs(int row, int col, bool val)
		{
			_row = row;
			_column = col;
			_value = val;
		}

		public int Column {	get{ return _column;} set{ _column = value;} }
		public int Row { get{ return _row;} set{ _row = value;} }
		public bool BoolValue {	get{ return _value;} set{ _value = value;} }
	}
	#endregion

	#region MyDataGridBoolColumn - a derived ColumnStyle with a BoolValueChanged event exposed

	public class MyDataGridBoolColumn: DataGridBoolColumn
	{
		public event BoolValueChangedEventHandler BoolValueChanged;

		private bool saveValue;
		private int editedRow;
		private bool lockValue;
		private bool beingEdited;
		private int _column;
		public const int VK_SPACE = 32;		// 0x20

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		static extern short GetKeyState(int nVirtKey);

		public MyDataGridBoolColumn(int column)
		{
			saveValue = false;
			editedRow = -1;
			lockValue = false;
			beingEdited = false;
			_column = column;
		}

		protected override void Edit(System.Windows.Forms.CurrencyManager source, int rowNum,
			System.Drawing.Rectangle bounds, bool readOnly,
			string instantText, bool cellIsVisible)
		{
			lockValue = true;
			beingEdited = true;
			editedRow = rowNum;
			saveValue = (bool) base.GetColumnValueAtRow(source, rowNum);
			base.Edit(source, rowNum,  bounds, readOnly, instantText, cellIsVisible);
			
		}
		
		protected  override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds,
			System.Windows.Forms.CurrencyManager source, int rowNum,
			System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
		{
			if(beingEdited && editedRow == rowNum)
			{
				Point mousePos = this.DataGridTableStyle.DataGrid.PointToClient(Control.MousePosition);
				DataGrid dg = this.DataGridTableStyle.DataGrid;
				bool isClickInCell = ((Control.MouseButtons == MouseButtons.Left) && dg.GetCellBounds(dg.CurrentCell).Contains(mousePos));
				bool changing = dg.Focused && ( isClickInCell || GetKeyState(VK_SPACE) < 0 ); // click or spacebar

				if(changing && !lockValue)
				{
					saveValue = !saveValue;
					lockValue = false;

					if(BoolValueChanged != null)
					{
						//fire the event - ensures immediate reaction to checkbox changes
						BoolValueChangedEventArgs e = new BoolValueChangedEventArgs(rowNum, _column, saveValue);
						BoolValueChanged(this, e);
					}
				}
				lockValue = false;	
			}

			base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
		}

		protected override bool Commit(System.Windows.Forms.CurrencyManager dataSource, int rowNum)
		{
			bool ret = true;
			try 
			{
				if(editedRow == rowNum)
				{
					beingEdited = false;
					lockValue = true;
					if(BoolValueChanged != null)
					{
						//fire the event - ensures sync when commit is made.
						BoolValueChangedEventArgs e = new BoolValueChangedEventArgs(rowNum, _column, saveValue);
						BoolValueChanged(this, e);
					}
					ret = base.Commit(dataSource, rowNum);
					editedRow = -1;
					lockValue = false;
					beingEdited = false;
				}
				else
				{
					ret = base.Commit(dataSource, rowNum);
				}
			} 
			catch (Exception e)
			{
#if DEBUG
				LibSys.StatusBar.Error("MyDataGridBoolColumn:Commit() exception " + e.Message);
#endif
				editedRow = -1;
				lockValue = false;
				beingEdited = false;
			}
			return ret;
		}
	}
	#endregion

	#region MyDataGridButtonColumn - a derived ColumnStyle with CellButtonClick event exposed

	// inspired by http://www.i-syn.gmxhome.de/devcom/colstyles/controlbutton.htm

	public delegate void CellButtonClickEventHandler( object sender, CellButtonClickEventArgs e );

	public class CellButtonClickEventArgs : EventArgs
	{
		private int row;

		public int Row // property accessor
		{	
			get { return this.row; }
		}
		
		private int column;

		public int Column // property accessor
		{
			get { return this.column; }
		}

		public CellButtonClickEventArgs( int row, int column )
		{
			// initializing constructor
			this.row = row;
			this.column = column;
		}
	}

	public class MyDataGridButtonColumn : DataGridColumnStyle
	{
		public event CellButtonClickEventHandler CellButtonClick;  // the event source

		private DataGrid grid;
		private int clickColumn = -1;
		private int clickRow = -1;		
		private int minimumHeight = 15;				// returned by GetMinimumHeight()
				
		public override bool ReadOnly			// property accessor
		{
			get { return true; } // The column is always read only
		}
		
		public MyDataGridButtonColumn()
		{
			return; // default constructor has nothing to do
		}
		
		protected override void Abort( int rowNum )
		{
			return;
		}
		
		protected override bool Commit( CurrencyManager dataSource, int rowNum )
		{	
			return false;
		}
		
		protected override void Edit( CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible )
		{
			return;
		}
		
		protected override int GetMinimumHeight()
		{
			return this.minimumHeight + 1;
		}
		
		protected override int GetPreferredHeight( Graphics g, object value )
		{
			return 0;
		}
		
		protected override Size GetPreferredSize( Graphics g, object value )
		{
			return new System.Drawing.Size( 0, 0 );
		}
		
		protected override void Paint( Graphics g, Rectangle bounds, CurrencyManager source, int rowNum )
		{
			this.Paint( g, bounds, source, rowNum, false );
		}

		private void PaintBtn(Graphics g, Rectangle bounds, bool isPressed)
		{
			Brush backBrush = new SolidBrush( this.DataGridTableStyle.BackColor );
			g.FillRectangle(backBrush,bounds);
			Brush btnBrush = new SolidBrush(Color.Gray);
			Rectangle bounds1 = bounds;
			bounds1.Inflate(-7, -3);
			g.FillRectangle(btnBrush,bounds1);
			btnBrush = new SolidBrush(isPressed? Color.Gray : Color.LightGray);
			bounds1.Inflate(-1, -1);
			g.FillRectangle(btnBrush,bounds1);
		}
		
		protected override void Paint( Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight )
		{
			//draw the button
			PaintBtn(g, bounds, this.clickRow == rowNum ); 
		}
		
		protected override void SetDataGrid( DataGrid value )
		{
			// not called yet....
			this.grid = value;		
		}
		
		protected override void SetDataGridInColumn( DataGrid grid )
		{
			// store a referernce to the grid and attach event mouse handlers
			if ( grid != this.grid && this.grid != null )
			{
				// detach mouse event handlers from the grid event sources
				this.grid.MouseDown -= new MouseEventHandler( this.grid_MouseDown );
				this.grid.MouseUp -= new MouseEventHandler( this.grid_MouseUp );
				this.grid = null;
			}
			if ( grid != this.grid && grid != null )
			{
				// attach mouse event handlers to grid the event sources
				grid.MouseDown += new MouseEventHandler( this.grid_MouseDown );
				grid.MouseUp += new MouseEventHandler( this.grid_MouseUp );				
			}
			this.grid = grid;
		}
		
		private void grid_MouseDown( object sender, MouseEventArgs e )
		{
			// Determines a cell button click
			DataGrid.HitTestInfo hti = this.grid.HitTest( new Point( e.X, e.Y ));
			this.clickColumn = this.DataGridTableStyle.GridColumnStyles.IndexOf( this );
			// column hit?
			if ( hti.Column != this.clickColumn || hti.Row == -1 ) return;
			Rectangle rect = this.grid.GetCellBounds( hti.Row, hti.Column );
			this.clickRow = hti.Row;
			// draw button down
			Graphics g = Graphics.FromHwnd( this.grid.Handle );
			PaintBtn(g, rect, true); 
			g.Dispose();
			this.grid.Capture = true;
		}

		private void grid_MouseUp( object sender, MouseEventArgs e )
		{
			// Draw button up and fire ButtonClick events
			if ( this.clickRow == -1 ) return;
			this.grid.Capture = false;
			// draw button up image
			Rectangle rect = this.grid.GetCellBounds( this.clickRow, this.clickColumn );
			int x = rect.Right;
			Graphics g = Graphics.FromHwnd( this.grid.Handle );
			PaintBtn(g, rect, false); 
			g.Dispose();
			// click test
			DataGrid.HitTestInfo hti = this.grid.HitTest( new Point( e.X, e.Y ));
			if ( hti.Column == this.clickColumn 
				&& hti.Row == this.clickRow 
				&& this.CellButtonClick != null ) 
			{
				this.CellButtonClick( this, new CellButtonClickEventArgs( this.clickRow, this.clickColumn ));
			}
			this.clickRow = -1;
		}
	}
	#endregion

	#region DataGridDigitsTextBoxColumn for numeric fields 
	public class DataGridDigitsTextBoxColumn : DataGridTextBoxColumn
	{
		public DataGridDigitsTextBoxColumn(System.ComponentModel.PropertyDescriptor pd, string format, bool b)
			: base(pd, format, b)
		{
			this.TextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(HandleKeyPress);
		}

		private void HandleKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			//ignore if not digit or control key
			if(!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
				e.Handled = true;

			//ignore if more than 3 digits 
			//if(this.TextBox.Text.Length >= 3 && !char.IsControl(e.KeyChar) && this.TextBox.SelectionLength == 0)
			//	e.Handled = true;
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
				this.TextBox.KeyPress -= new System.Windows.Forms.KeyPressEventHandler(HandleKeyPress);

			base.Dispose(disposing);
		}
	}
	#endregion
}
