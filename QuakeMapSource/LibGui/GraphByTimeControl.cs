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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;

using LibSys;

namespace LibGui
{
	/// <summary>
	/// Summary description for GraphByTimeControl.
	/// </summary>
	public class GraphByTimeControl : System.Windows.Forms.UserControl
	{
		// switch to simplified display if resized to smaller than:
		protected const int		ICON_THRESHOLD_WIDTH  = 200;
		protected const int		ICON_THRESHOLD_HEIGHT = 200;

		// EnDis parameters:
		public const int ED_SELECTED		= 0;
		public const int ED_ALLDATA			= 1;
		public const int ED_ZOOM			= 2;
		public const int ED_BROWSE			= 3;

		// margins:
		private const int MARGIN_TOP		= 40;
		private const int MARGIN_TOP_EXTRA	= 17;	// room for message string on top of the graph
		private int MARGIN_LEFT				= 35;
		private int MARGIN_RIGHT			= 20;
		private const int MARGIN_BOTTOM		= 45;

		public int MarginLeft { set { MARGIN_LEFT = value; } }
		public int MarginRight { set { MARGIN_RIGHT = value; } }

		// colors:
		protected static Color	m_colorBright		= Color.FromArgb(0, 255, 0);
		protected static Color	m_colorDim			= Color.FromArgb(0, 180, 0);
		protected static Color	m_colorMoreDim		= Color.FromArgb(0, 100, 0);
		protected static Color	m_colorVeryDim		= Color.FromArgb(0, 80, 0);

		private Brush m_selectionBrush = new SolidBrush(Color.DarkOliveGreen);

		// colors to draw graphs:
		protected Color[]		m_allColors	= new Color[] {
																Color.Yellow,
																Color.Blue,
																Color.Cyan,
																Color.Green,
																Color.Magenta,
																Color.Orange,
																Color.Pink,
																Color.Red,
																Color.White,
																Color.LightGray,
																Color.Gray
															};

		public const int GRAPH_MODE_STEPPED			= 1;
		public const int GRAPH_MODE_INTERPOLATE		= 2;
		public const int GRAPH_MODE_PEAK			= 3;

		protected Size			m_currSize;

		protected int			m_xMarginLeft;
		protected int			m_xMarginRight;
		protected int			m_yMarginTop;
		protected int			m_yMarginBottom;
		protected double		m_valueFactor;	//	= 20.0;
		protected double		m_maxValue			= 10.0;
		public double MaxValueY { set { m_maxValue = value; } }
		protected double		m_stepY				= 1.0;
		public double StepY { set { m_stepY = value; } }
		protected int			m_graphsCount		= -1;
		protected int			m_graphMode			= GRAPH_MODE_PEAK;
		protected bool			m_iconMode			= false;
		protected bool			m_twoLineMessage	= false;
		protected string[]		m_yLabels			= null;
		protected int			m_maxLabelLength;
		protected int			m_format;
		protected int			m_xAxisSize;
		protected int			m_yAxisSize;
		protected int			m_xZeroMark;		// position of Y axis
		protected int			m_yZeroMark;
		protected int			m_startSelection	= -1;
		protected int			m_endSelection		= -1;
		protected int			m_startSelectionRect = -1;
		protected int			m_endSelectionRect	= -1;
		protected int			m_selected			= -1;
		protected static long	m_selectedId		= -1;
		public long SelectedId { get { return m_selectedId; } set { m_selectedId = value; } }
		protected int			m_lastX				= -1;
		protected DateTime		m_startDate;
		protected DateTime		m_endDate;
		protected long	m_minTimeMargin		= 10 * 60 * 1000; //ms 10 minutes
		public long MinTimeMargin { set { m_minTimeMargin = value; } }
		protected long			m_startTime;		// milliseconds, UTC
		protected long			m_endTime;			// milliseconds, UTC
		protected long			m_timeSpan;			// milliseconds between start and end of the graph
		protected long			m_timeStep;			// milliseconds per horizontal pixel on the graph
		protected long			m_timeMargin;		// milliseconds
		protected DatedEvent[]	m_graphEvents		= null;
		protected SortedList	m_allEvents;		// of DatedEvent
		protected DatedEvent	m_lastEvent;
		protected string		m_messageMain		= "";
		protected string		m_messageSecondary	= "";	// drawn if Main is empty
		protected string		m_messageInitial	= "";
		protected string		m_selHint			= "";
		public string			initialHint			= "Click on peaks to see details. Drag mouse to select time interval.";
		protected IEnableDisable m_enDis;
		protected Font			m_fontLarge;
		protected Font			m_fontMedium;
		protected Font			m_fontSmall;
		protected int			m_charWidthLarge;
		protected int			m_charWidthSmall;
		protected Color[]		m_colors;
		private StringById		m_descrById = null;
		private StringById		m_zoomById = null;
		private StringById		m_browseById = null;
		private MethodInvoker	m_showSelected = null;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public GraphByTimeControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			m_fontLarge = Project.getLabelFont(9);
			m_fontMedium = Project.getLabelFont(8);
			m_fontSmall = Project.getLabelFont(7);

			m_charWidthLarge = 6; // fontMetrics.charWidth('0');
			m_charWidthSmall = 5;
		}

		// can be called repeatedly
		public void init(IEnableDisable endis, string message, string message0, string selHint,
							StringById descrById, StringById zoomById, StringById browseById, MethodInvoker showSelected)
		{
			m_enDis = endis;			// has to be not null at this point
			m_enDis.disable(ED_SELECTED);
			m_enDis.disable(ED_ALLDATA);
			m_enDis.disable(ED_ZOOM);
			m_enDis.disable(ED_BROWSE);

			m_messageSecondary = message;
			m_messageInitial = message0;
			m_selHint = selHint;

			m_descrById = descrById;
			m_zoomById = zoomById;
			m_browseById = browseById;
			m_showSelected = showSelected;

			m_startSelection = -1;
			m_endSelection = -1;
			m_startSelectionRect = -1;
			m_endSelectionRect = -1;

			m_selected = -1;		// keep m_selectedId though to select it again
		}

		public string browse()
		{ 
			string ret = "";
			if(m_browseById != null && m_selectedId != -1)
			{
				ret = m_browseById(m_selectedId);
			}
			return ret;
		}

		public string zoom()
		{
			string ret = "";
			if(m_zoomById != null && m_selectedId != -1)
			{
				ret = m_zoomById(m_selectedId);
			}
			return ret;
		}

		#region Graph Data building

		public void refreshGraphData()
		{
			setGraphData(null, m_format, -1, true);
		}

		public void setGraphData(SortedList events, int format, int graphsCount, bool staySelected)
		{
			m_currSize = this.ClientRectangle.Size;

			//LibSys.StatusBar.Trace("size=" + m_currSize.Width 
			//					 + "x" + m_currSize.Height);

			if(events != null) 
			{	// if null, leave m_allEvents intact
				m_allEvents = events;
				//LibSys.StatusBar.Trace("N events: " + m_allEvents.size());
			}
			m_format = format;

			switch (m_format) 
			{
				case Project.FORMAT_EARTHQUAKES_INTENSITY:
					m_graphMode = GRAPH_MODE_STEPPED;
					break;
				case Project.FORMAT_EARTHQUAKES_STRONGEST:
					m_graphMode = GRAPH_MODE_PEAK;
					break;
				case Project.FORMAT_SERVER_AVAILABILITY:
					m_graphMode = GRAPH_MODE_STEPPED;
					break;
				case Project.FORMAT_UNKNOWN:
					m_graphMode = GRAPH_MODE_INTERPOLATE;
					break;
				case Project.FORMAT_TRACK_ELEVATION:
					m_graphMode = GRAPH_MODE_INTERPOLATE;
					//m_graphMode = GRAPH_MODE_STEPPED;
					break;
			}

			if(graphsCount != m_graphsCount && graphsCount != -1) 
			{
				m_colors = new Color[ graphsCount ];
				int i;
				for(i=0; i < graphsCount ;i++) 
				{
					m_colors[i] = m_allColors[i % m_allColors.Length];
				}
				m_graphsCount = graphsCount;
			}

			m_startSelection = -1;
			m_endSelection = -1;

			if(m_currSize.Width == 0 || m_currSize.Height == 0) 
			{
				// can't do anything until the size is defined.
				return;
			}

			if(m_currSize.Width < ICON_THRESHOLD_WIDTH
				|| m_currSize.Height < ICON_THRESHOLD_HEIGHT) 
			{
				m_iconMode = true;
				//LibSys.StatusBar.Trace("icon mode");
				m_xMarginLeft	= 3;
				m_xMarginRight	= 3;
				m_yMarginTop	= 0;
				m_yMarginBottom = 3;
			} 
			else 
			{
				m_iconMode = false;
				//LibSys.StatusBar.Trace("full mode");
				m_xMarginLeft	= MARGIN_LEFT;
				m_xMarginRight	= MARGIN_RIGHT;
				m_yMarginTop	= (m_twoLineMessage ? (MARGIN_TOP + 10) : MARGIN_TOP) + MARGIN_TOP_EXTRA;
				m_yMarginBottom = MARGIN_BOTTOM;
			}

			m_xAxisSize = m_currSize.Width - m_xMarginLeft - m_xMarginRight;
			m_yAxisSize = m_currSize.Height - m_yMarginTop - m_yMarginBottom;
			//LibSys.StatusBar.Trace("m_xAxisSize=" + m_xAxisSize + "  m_yAxisSize=" + m_yAxisSize);

			m_valueFactor = ((double)m_yAxisSize) / m_maxValue;
			m_xZeroMark = m_xMarginLeft;				// awt coordinates
			m_yZeroMark = m_currSize.Height - m_yMarginBottom;	// awt coordinates

			if(m_allEvents == null) 
			{
				return;
			}

			if(!staySelected)
			{
				if(m_allEvents.Count >= 1) 
				{
					DatedEvent ev = (DatedEvent)m_allEvents.GetByIndex(0);
					//LibSys.StatusBar.Trace("N events: " + m_allEvents.size());
					//LibSys.StatusBar.Trace("first event: " + ev.toTableString2());
					m_startDate = ev.DateTime;
					ev = (DatedEvent)m_allEvents.GetByIndex(m_allEvents.Count-1);
					//LibSys.StatusBar.Trace("last event : " + ev.toTableString2());
					m_endDate = ev.DateTime;
				} 
				else 
				{
					m_startDate = new DateTime();
					m_endDate = new DateTime();
				}
				m_startTime = m_startDate.Ticks / 10000;	// the number of milliseconds
				// since January 1, 1970, 00:00:00 GMT
				m_endTime = m_endDate.Ticks / 10000;
			}

			if(m_graphEvents == null) 
			{
				m_graphEvents = new DatedEvent[m_xAxisSize];
			}
			fillGraphEvents();				// will also select event by m_selectedId, if possible
		}
    
		protected void fillGraphEvents()
		{
			m_selected = -1;
			m_timeSpan = m_endTime - m_startTime;
			m_timeMargin = m_timeSpan / 80;

			//LibSys.StatusBar.Trace("m_timeMargin=" + m_timeMargin);

			if(m_timeMargin < m_minTimeMargin) 
			{ // probably one event only
				m_timeMargin = m_minTimeMargin;
			}
			m_startTime -= m_timeMargin;
			m_endTime   += m_timeMargin;
			m_timeSpan = m_endTime - m_startTime;
        
			m_timeStep = m_timeSpan / (m_graphEvents.Length - 1);
			long time = m_startTime;

			int x = 0;
			int xMax = m_graphEvents.Length;
			try 
			{
				for (x=0; x < xMax ;x++,time+=m_timeStep) 
				{
					m_graphEvents[x] = makeGraphEvent(x, time, m_timeStep);
					// makeGraphEvent() will set m_selected and m_selectedId
				}
			} 
			catch (Exception e) 
			{
				;
			}
			if(m_selected == -1)
			{
				m_selectedId = -1;
				m_messageMain = "";
			}
		}

		private DatedEvent makeGraphEvent(int x, long time, long timeStep)
		{
			int index = 0;
			int indexStart = -2;
			int indexEnd = -2;
			// find index interval correspondent to the time interval:
			try 
			{
				for(; index < m_allEvents.Count ; index++) 
				{
					DatedEvent ev = (DatedEvent)m_allEvents.GetByIndex(index);
					long evTime = ev.DateTime.Ticks / 10000;
					if(evTime >= time && evTime < time + timeStep ) 
					{
						if(indexStart == -2) 
						{
							//LibSys.StatusBar.Trace("Start: " + ev + " : "
							//				+ indexStart + "->" + index + "/" + indexEnd);
							indexStart = index;
						}
					}
					if(evTime > time + timeStep && indexStart != -2) 
					{
						if(indexEnd == -2) 
						{
							//LibSys.StatusBar.Trace("End: "+ ev + " : "
							//				+ indexStart + "/" + indexEnd + "->" + (index-1));
							indexEnd = index - 1;
						}
					}
				}
				if(indexEnd == -2) 
				{
					indexEnd = index - 1;	// last sample
				}

				if(indexStart != -2) 
				{
					DatedEvent ev;
					switch (m_format) 
					{
						case Project.FORMAT_EARTHQUAKES_STRONGEST:
							// strongest earthquake for the interval:
							ev = (DatedEvent)m_allEvents.GetByIndex(indexStart);
							for(int i=indexStart+1; i <= indexEnd ;i++) 
							{
								DatedEvent evv = (DatedEvent)m_allEvents.GetByIndex(i);
								double curMaxMagn = (ev.values(m_format))[0];
								double curMagn =    (evv.values(m_format))[0];
								if(curMagn > curMaxMagn) 
								{
									ev = evv;
								}
							}
							if(ev.ids() != null && ev.ids()[0] == m_selectedId)
							{
								selectEvent(ev, x);
							}
							return ev;

						case Project.FORMAT_EARTHQUAKES_INTENSITY:
							// number of earthquakes for this interval:
							double[] values = new double[1];
							string[] sValues = new string[1];
							values[0] = ((double)(indexEnd - indexStart + 1))/1.0;
							ev = (DatedEvent)m_allEvents.GetByIndex(indexStart);
							// and first eartquake's description:
							sValues[0] = ev.toTableString1();
							return new DatedEvent(new DateTime(time * 10000), values, sValues, null, 1);

						case Project.FORMAT_SERVER_AVAILABILITY:
							ev = (DatedEvent)m_allEvents.GetByIndex(indexStart);
							//LibSys.StatusBar.Trace("-- event: " + ev.toTableString() + " : "
							//							+ indexStart + "/" + indexEnd);
							return ev;

						case Project.FORMAT_TRACK_ELEVATION:
							ev = (DatedEvent)m_allEvents.GetByIndex(indexStart);
							return ev;
					}

				}
			} 
			catch (Exception e) 
			{
				LibSys.StatusBar.Error("" + x + " " + e.Message);
			}
			return null;
		}

		/*
		// not used?
		public void setYLabels(string[] yLabels)
		{
			m_yLabels = yLabels;
			// calculate max label length here to avoid doing it in paint():
			m_maxLabelLength = 0;
			for(int j=0; j < m_yLabels.Length ;j++) 
			{
				int len = m_yLabels[j].Length;
				if(len > m_maxLabelLength) 
				{
					m_maxLabelLength = len;
				}
			}
		}
		*/

		// zulu time
		public DateTime getStartSelectedDateTime()
		{
			long selectionOffset = m_startSelection * m_timeStep;
			if(selectionOffset < m_timeMargin) 
			{
				selectionOffset = m_timeMargin;
			}
			long timeStartSelection = m_startTime + selectionOffset;
			return new DateTime(timeStartSelection * 10000);
		}

		// zulu time
		public DateTime getEndSelectedDateTime()
		{
			long selectionOffset = m_endSelection * m_timeStep;
			if( selectionOffset > m_xAxisSize * m_timeStep - m_timeMargin) 
			{
				selectionOffset = m_xAxisSize * m_timeStep - m_timeMargin;
			}
			long timeEndSelection = m_startTime + selectionOffset;
			return new DateTime(timeEndSelection * 10000);
		}

		// rebuild the graph, possibly zooming into selected area (in which case true is returned):
		public bool GraphSelectedArea()
		{
			m_enDis.disable(ED_SELECTED);

			if(    m_allEvents == null || m_allEvents.Count < 1
				|| m_startSelection < 0 
				|| m_endSelection < 0
				|| m_startSelection >= m_xAxisSize
				|| m_endSelection >= m_xAxisSize
				|| m_startSelection >= m_endSelection
			  ) 
			{
				return false;
			}

			// adjust for time margins:
			long selectionOffset = m_startSelection * m_timeStep;
			if(selectionOffset < m_timeMargin) 
			{
				//LibSys.StatusBar.Trace("m_timeMargin=" + m_timeMargin 
				//				+ "  selectionOffset=" + selectionOffset);
				selectionOffset = m_timeMargin;
			}
			long timeStartSelection = m_startTime + selectionOffset;
			selectionOffset = m_endSelection * m_timeStep;
			if( selectionOffset > m_xAxisSize * m_timeStep - m_timeMargin) 
			{
				//LibSys.StatusBar.Trace("m_timeMargin=" + (m_xAxisSize * m_timeStep - m_timeMargin) 
				//				+ "  selectionOffset=" + selectionOffset);
				selectionOffset = m_xAxisSize * m_timeStep - m_timeMargin;
			}
			long timeEndSelection = m_startTime + selectionOffset;
		
			m_startTime = timeStartSelection;
			m_endTime = timeEndSelection;
			fillGraphEvents();				// will also select event by m_selectedId, if possible

			m_startSelection = m_endSelection = -1;
			m_enDis.enable(ED_ALLDATA);

			this.Invalidate(this.ClientRectangle);

			return true;
		}
		#endregion

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		private void GraphByTimeControl_Resize(object sender, System.EventArgs e)
		{
			m_graphEvents = null;	// make sure the X axis will be rebuilt

			long startTime = m_startTime; 
			long endTime = m_endTime;

			refreshGraphData();

			m_startTime = startTime; 
			m_endTime = endTime;
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// GraphByTimeControl
			// 
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Name = "GraphByTimeControl";
			this.Size = new System.Drawing.Size(752, 360);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.GraphByTimeControl_KeyPress);
			this.Click += new System.EventHandler(this.GraphByTimeControl_Click);
			this.Resize += new System.EventHandler(this.GraphByTimeControl_Resize);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GraphByTimeControl_MouseUp);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.GraphByTimeControl_Paint);
			this.DoubleClick += new System.EventHandler(this.GraphByTimeControl_DoubleClick);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GraphByTimeControl_MouseMove);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GraphByTimeControl_MouseDown);

		}
		#endregion

		#region Paint - related functions

		private void GraphByTimeControl_Paint(object sender, System.Windows.Forms.PaintEventArgs pe)
		{
			//Brush bb = new LinearGradientBrush(this.ClientRectangle, Color.Red, Color.Yellow, 90, false);
			Brush bb = new SolidBrush(Color.Black);
              
			Graphics g = pe.Graphics;
			g.FillRectangle(bb, this.ClientRectangle);

			if(dragging)
			{
				g.FillRectangle(m_selectionBrush, currentRubberRect);
				prevRubberRect = currentRubberRect;
			}

			m_currSize = this.ClientRectangle.Size;

			Pen pen = new Pen(m_colorBright);
			g.DrawRectangle(pen, 1, 1, m_currSize.Width-3, m_currSize.Height-3);
			if(m_iconMode)
			{
				pen = new Pen(m_colorDim);
			}

			if(m_format == Project.FORMAT_UNKNOWN) 
			{
				Brush brush = new SolidBrush(m_colorBright);
				string msg = m_messageInitial;
				int len = msg.Length;
				int msgWidth = m_charWidthLarge * len;
				int msgPos = (m_currSize.Width - msgWidth)/2;
				if(len > 0) 
				{
					g.DrawString(msg, m_fontLarge, brush, msgPos, 16);
				}
				if(!m_iconMode) 
				{
					msgWidth = m_charWidthSmall * m_messageSecondary.Length;
					msgPos = (m_currSize.Width - msgWidth)/2;
					brush = new SolidBrush(Color.Yellow);
					g.DrawString(m_messageSecondary, m_fontSmall, brush, msgPos, m_yMarginTop - 3);
				}
				return;
			}

			if(m_allEvents == null || m_allEvents.Count < 1 || m_graphEvents == null)
			{
				// can't draw any more until events are defined
				m_messageMain = "        No data to display";
				m_messageSecondary = "";
				drawMessages(g);
				return;
			}

			if(!markSelectedArea(g)) 
			{
				drawMessages(g);
			} 
			else 
			{
				m_messageMain = "        " + m_selHint;
				m_messageSecondary = "";
				m_enDis.disable(ED_ZOOM);
				m_enDis.disable(ED_BROWSE);
				drawMessages(g);
			}

			if(m_selected > 0 && m_selected < m_graphEvents.Length) 
			{
				pen = new Pen(Color.Red);
				int x = m_xZeroMark + m_selected;
				g.DrawLine(pen, x, m_yMarginTop, x, m_yZeroMark + 1);
			}

			if(!m_iconMode) 
			{
				drawTitle(g);
				drawLegend(g);
				drawGrid(g);
			}

			int indexCap = m_graphEvents.Length;
			int lastGoodEvent = indexCap - 1;
			while(lastGoodEvent >= 0 && m_graphEvents[lastGoodEvent--] == null)
			{
				;
			}

			int nullEventCount = 0;
			for(int i=0; i < m_graphsCount ;i++) 
			{
				pen = new Pen(m_colors[i]);
				m_lastEvent = null;
				int lastX = -10000;
				int lastY = -10000;
				bool startplot = false;
				for (int x=0; x < indexCap ;x++) 
				{
					DatedEvent currEvent = m_graphEvents[x];
					if(currEvent != null) 
					{
						startplot = true;
					} 
					else 
					{
						nullEventCount++;
					}
					int currValue = graphValue(currEvent, i);
					DatedEvent nextEvent = m_graphEvents[x == indexCap-1 ? x : x + 1];
					int nextValue = graphValue(nextEvent, i);
					switch(m_graphMode) 
					{
						case GRAPH_MODE_INTERPOLATE:
						{
							if(currEvent != null && nextEvent != null)
							{
								int xx = x + m_xZeroMark;
								int xxx = x + m_xZeroMark + 1;
								g.DrawLine(pen, xx, currValue, xxx, nextValue);
								lastX = xxx;
								lastY = nextValue;
							}
							else if(nextEvent != null)
							{
								int xxx = x + m_xZeroMark + 1;
								if(lastX != -10000 && lastY != -10000)
								{
									g.DrawLine(pen, lastX, lastY, xxx, nextValue);
								}
								lastX = xxx;
								lastY = nextValue;
							}
						}
							break;
						case GRAPH_MODE_PEAK:
							if(currValue != m_yZeroMark)
							{
								g.DrawLine(pen, x + m_xZeroMark, currValue,	x + m_xZeroMark, m_yZeroMark);
							}
							break;
						case GRAPH_MODE_STEPPED:
							if(startplot) 
							{
								g.DrawLine(pen, x + m_xZeroMark, currValue,
									x + m_xZeroMark + 1, currValue);
								if(currValue != nextValue) 
								{
									g.DrawLine(pen, x + m_xZeroMark + 1, currValue,
										x + m_xZeroMark + 1, nextValue);
								}
								if(currEvent != null && nullEventCount > 20) 
								{
									g.DrawEllipse(pen, x + m_xZeroMark - 1, currValue - 1, 3, 3);
								}
							}
							break;
					}
					if(currEvent != null) 
					{
						nullEventCount = 0;
					} 
					if(m_graphMode == GRAPH_MODE_STEPPED) 
					{
						if( x >= lastGoodEvent ) 
						{
							// all events till end are null. stop drawing lines.
							m_lastEvent = null;
							break;	// the inner "for" loop
						}
					}
				}
			}
		}

		protected int graphValue(DatedEvent ev, int index)
		{
			// the Server Availability graph requires interpolated (stepped) line;
			// others require peaks where the data actually is: 
			if(ev == null) 
			{		// no data at this point.
				switch(m_graphMode) 
				{
					case GRAPH_MODE_STEPPED:
						ev = m_lastEvent;	// may be null too
						break;
				}
			}

			int value;
			if(ev == null || (index+1) > ev.nValues()) 
			{
				value = m_yZeroMark;
			} 
			else 
			{
				double evValue = ev.values(m_format)[index];
				if(m_format == Project.FORMAT_EARTHQUAKES_STRONGEST && evValue == 0.0d) 
				{
					evValue = 0.5d; // quakes with unknown magnitude will be small bumps on the graph
				}
				value = m_yZeroMark - (int)(evValue * m_valueFactor);
			}
			m_lastEvent = ev;
			return value;
		}

		public DatedEvent getStartSelectedEvent()
		{
			if(m_startSelection >= 0 
				&& m_endSelection >= 0
				&& m_startSelection < m_xAxisSize
				&& m_endSelection < m_xAxisSize
				&& m_startSelection <= m_endSelection)
			{
				for(int i=m_startSelection; i < m_endSelection ;i++) 
				{
					if(m_graphEvents[i] != null) 
					{
						return m_graphEvents[i];
					}
				}
			} 
			else 
			{
				// nothing selected - just return the first non-null displayed event
				for(int i=0; i < m_graphEvents.Length ;i++) 
				{
					if(m_graphEvents[i] != null) 
					{
						return m_graphEvents[i];
					}
				}
			}
			return null;
		}

		public DatedEvent getEndSelectedEvent()
		{
			if(m_startSelection >= 0 
				&& m_endSelection >= 0
				&& m_startSelection < m_xAxisSize
				&& m_endSelection < m_xAxisSize
				&& m_startSelection <= m_endSelection)
			{
				for(int i=m_endSelection; i >= m_startSelection ;i--) 
				{
					if(m_graphEvents[i] != null) 
					{
						return m_graphEvents[i];
					}
				}
			} 
			else 
			{
				// nothing selected - just return the last non-null displayed event
				for(int i=m_graphEvents.Length-1; i >= 0  ;i--) 
				{
					if(m_graphEvents[i] != null) 
					{
						return m_graphEvents[i];
					}
				}
			}
			return null;
		}

		public bool markSelectedArea(Graphics g)
		{
			if(m_startSelection < 0 
				|| m_endSelection < 0
				|| m_startSelection >= m_xAxisSize
				|| m_endSelection >= m_xAxisSize
				|| m_startSelection >= m_endSelection) 
			{
				if(m_enDis != null)
				{
					m_enDis.disable(ED_SELECTED);
				}
				return false;
			}
			int xStart = m_xZeroMark + m_startSelection;
			int xEnd = m_xZeroMark + m_endSelection;
			int yStart = 3;
			int yEnd = m_currSize.Height - yStart - 1;
			g.FillRectangle(m_selectionBrush, xStart, yStart, xEnd - xStart, yEnd - yStart);
			for(int i=m_startSelection; i < m_endSelection ;i++) 
			{
				if(m_graphEvents[i] != null) 
				{
					m_enDis.enable(ED_SELECTED);	// at least one event in selection
					break;
				}
			}
			return true;
		}

		public void drawGrid(Graphics g)
		{
			drawYGrid(g);
			drawXGrid(g);
		}

		public void drawXGrid(Graphics g)
		{
			drawXTimeGrid(g);
		}

		public void drawTitle(Graphics g)
		{
			// for each format we have a title:
			string[] titles = {
								  "Earthquake data availability by server",
								  "Earthquakes Intensity (quakes per time interval)",
								  "Strongest Earthquake per time interval",
								  "Track Elevation By Time"
			};
			int[] formats = new int[]	{
											Project.FORMAT_SERVER_AVAILABILITY,
											Project.FORMAT_EARTHQUAKES_INTENSITY,
											Project.FORMAT_EARTHQUAKES_STRONGEST,
											Project.FORMAT_TRACK_ELEVATION
										};

			string title = null;
			for(int f=0; f < formats.Length ;f++) 
			{
				if(m_format == formats[f]) 
				{
					title = titles[f];
					break;
				}
			}

			if(title != null) 
			{
				Brush brush = new SolidBrush(m_colorBright);
				int titleWidth = m_charWidthLarge * title.Length;
				int titlePos = (m_currSize.Width - titleWidth)/2;
				g.DrawString(title, m_fontLarge, brush, titlePos, 20);
			}
		}

		private Hashtable legendStrings = new Hashtable();
 
		public void drawLegend(Graphics g)
		{
			foreach(int key in legendStrings.Keys)
			{
				string legendStr = (string)legendStrings[key];
				int x = m_currSize.Width - 100;
				int y = 10 + 12 * key;
				Color legendColor = m_allColors[key % m_allColors.Length];
				Brush brush = new SolidBrush(legendColor);
				g.DrawString(legendStr, m_fontLarge, brush, x, y);
			}
		}

		public void resetLegends()
		{
			legendStrings.Clear();
		}

		public void setLegend(int position, string legendStr)
		{
			legendStrings[position] = legendStr;
		}

		public void drawMessages(Graphics g)
		{
			Brush brush = new SolidBrush(m_colorBright);
			int leftShift;
			int topShift;
			if(m_twoLineMessage) 
			{
				leftShift = m_iconMode ? 5 : 20;
				string msg = m_messageMain;
				if(msg.Length == 0) 
				{
					brush = new SolidBrush(m_colorDim);
					msg = initialHint;
				}
				if(!m_iconMode) 
				{
					g.DrawString(m_messageSecondary, m_fontMedium, brush, m_xMarginLeft + leftShift, m_yMarginTop - 3);
				}
				g.DrawString(msg, m_fontMedium, brush, m_xMarginLeft + leftShift, m_yMarginTop + 18);
			} 
			else 
			{
				leftShift = m_iconMode ? 5 : 20;
				topShift = m_iconMode ? 3 : (-MARGIN_TOP_EXTRA);
				if(m_messageMain.Length > 0) 
				{
					g.DrawString(m_messageMain, m_fontMedium, brush, m_xMarginLeft + leftShift, m_yMarginTop + topShift);
				} 
				else if(m_messageSecondary.Length > 0) 
				{
					g.DrawString(m_messageSecondary, m_fontMedium, brush, m_xMarginLeft + leftShift, m_yMarginTop + topShift);
					//g.DrawString(m_messageMain, m_fontMedium, brush, m_xMarginLeft + leftShift, m_yMarginTop + topShift + 13);
				} 
				else 
				{
					brush = new SolidBrush(m_colorDim);
					g.DrawString(initialHint, m_fontMedium, brush, m_xMarginLeft + leftShift, m_yMarginTop + topShift);
				}
			}
		}

		public void drawYGrid(Graphics g)
		{
			int i;
			double y;
			Pen pen = new Pen(m_colorBright);
			switch(m_format) 
			{
				case Project.FORMAT_EARTHQUAKES_INTENSITY:
				case Project.FORMAT_EARTHQUAKES_STRONGEST:
					g.DrawLine(pen, m_xZeroMark - 1, m_yMarginTop, m_xZeroMark - 1, m_yZeroMark + 1);
					for(y=0, i=0; y <= m_maxValue ;i++,y += 1.0) 
					{
						string sValue = "" + y;
						markYGrid(g, y, i % 2 == 0, sValue, i==5);
					}
					break;

				case Project.FORMAT_SERVER_AVAILABILITY:
					g.DrawLine(pen, m_xZeroMark - 1, m_yMarginTop, m_xZeroMark - 1, m_yZeroMark + 1);
					if(m_yLabels == null) 
					{
						for(y=0; y <= m_maxValue ;y += 1.0d) 
						{
							string sValue = "" + y;
							markYGrid(g, y, true, sValue, false); // ((int)y) % 5 == 0);
						}
					} 
					else 
					{
						int j=0;
						for(y=0; y <= m_maxValue ;j++, y += 1.0) 
						{
							string sValue = j < m_yLabels.Length ? m_yLabels[j] : ""; 
							markYGrid(g, y, true, sValue, false); // ((int)y) % 5 == 0);
						}
					}
					break;

				case Project.FORMAT_TRACK_ELEVATION:
					g.DrawLine(pen, m_xZeroMark - 1, m_yMarginTop, m_xZeroMark - 1, m_yZeroMark + 1);
					for(y=0, i=0; y <= m_maxValue ;i++,y += m_stepY) 
					{
						string sValue = "" + y;
						markYGrid(g, y, i % 2 == 0, sValue, i==5);
					}
					break;
			}
		}

		protected void markYGrid(Graphics g, double value, bool doText, string text, bool brightLine)
		{
			int yValue = (m_yZeroMark - (int)(value * m_valueFactor));
			Brush brushBright = new SolidBrush(m_colorBright);
			Pen penBright = new Pen(m_colorBright);
			Pen pen;
			switch(m_format) 
			{
				case Project.FORMAT_EARTHQUAKES_INTENSITY:
				case Project.FORMAT_EARTHQUAKES_STRONGEST:
					g.DrawLine(penBright, m_xMarginLeft - 5, yValue, m_xMarginLeft + 5, yValue);
					pen = new Pen(brightLine ? m_colorMoreDim : m_colorVeryDim);
					g.DrawLine(pen, m_xZeroMark - 1, yValue, m_xZeroMark + m_xAxisSize, yValue);
					if(doText) 
					{
						int posShift = text.Length * m_charWidthSmall + 12;
						g.DrawString(text, m_fontSmall, brushBright, m_xZeroMark - posShift, yValue - 8);
					}
					if(brightLine) 
					{
						g.DrawString(text, m_fontSmall, brushBright, m_xZeroMark + m_xAxisSize - 3, yValue - 8);
					}
					break;

				case Project.FORMAT_SERVER_AVAILABILITY:
					g.DrawLine(penBright, m_xMarginLeft - 5, yValue, m_xMarginLeft + 5, yValue);
					pen = new Pen(m_colorVeryDim);
					g.DrawLine(pen, m_xZeroMark - 1, yValue, m_xZeroMark + m_xAxisSize, yValue);
					if(doText) 
					{
						int posShift = text.Length * m_charWidthSmall + 10;
						g.DrawString(text, m_fontSmall, brushBright, m_xZeroMark - posShift,	yValue - 7);
					}
					break;

				case Project.FORMAT_TRACK_ELEVATION:
					g.DrawLine(penBright, m_xMarginLeft - 5, yValue, m_xMarginLeft + 5, yValue);
					pen = new Pen(brightLine ? m_colorMoreDim : m_colorVeryDim);
					g.DrawLine(pen, m_xZeroMark - 1, yValue, m_xZeroMark + m_xAxisSize, yValue);
					if(doText) 
					{
						int posShift = text.Length * m_charWidthSmall + 12;
						g.DrawString(text, m_fontSmall, brushBright, m_xZeroMark - posShift, yValue - 8);
					}
					if(brightLine) 
					{
						g.DrawString(text, m_fontSmall, brushBright, m_xZeroMark + m_xAxisSize - 3, yValue - 8);
					}
					break;
			}
		}

		// we do not keep our own useUtcTime flag here, but we need to do some stuff when we set the Project's flag:
		public bool UseUtcTime { 
			get { return Project.useUtcTime; } 
			set 
			{ 
				Project.useUtcTime = value;
				if(m_selectedId != -1 && m_descrById != null)
				{
					m_messageMain = m_descrById(m_selectedId);
				}
				else if(m_selected != -1)
				{
					selectNonEvent(m_selected);
				}
				this.Invalidate();
			}
		}

		private static string[] months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

		protected void drawXTimeGrid(Graphics g)
		{
			Pen penBright = new Pen(m_colorBright);
			g.DrawLine(penBright, m_xZeroMark - 1, m_yZeroMark + 1, m_xZeroMark + m_xAxisSize, m_yZeroMark + 1);

			Brush brushBright = new SolidBrush(m_colorBright);
			Brush brushDim = new SolidBrush(m_colorDim);
			Pen pen = new Pen(m_colorDim);
#if DEBUG
			// draw time per pixel value in the corner:
			string sTimeStep = "" + m_timeStep/3600000 + ":" 
								  + (m_timeStep/60000)%60 + ":"
								  + (m_timeStep/1000)%60;
			g.DrawString(sTimeStep, m_fontSmall, brushDim, 5, 16);
#endif
			if(m_graphEvents == null)
			{
				return;
			}

			Font font;
			Brush brush;
			int maxX = m_graphEvents.Length;
			int lastCentury = -1;
			int lastYear = -1;
			int lastMonth = -1;
			int lastDay = -1;
			int lastHour = -1;
			int lastMinuteTens = -1;
			int lastDrawnX = -1;
			int xShift, yShift, xValue;
			int charWidth;
			bool yearsMany = (m_timeStep > 3600000L * 24L * 365L);
			bool yearsOnly = (m_timeStep > 3600000L * 24L);
			bool yearsAndMonths = (m_timeStep > 4000000L);
			bool minutes = (m_timeStep < 6000L);
			bool drawSmall;
			bool drawSmallLines = !yearsOnly;
			for (int x=0; x < maxX ;x++) 
			{
				long time = m_startTime + m_timeSpan * x / maxX;
				DateTime d;		// UTC
				if(time * 10000 < DateTime.MinValue.Ticks)	// the BC dates may break constructor, protect it.
				{
					d = DateTime.MinValue;
				}
				else
				{
					d = new DateTime(time * 10000);
				}
				if(!UseUtcTime)
				{
					d = Project.zuluToLocal(d);
				}
				string str;
				int year = d.Year;
				int month = d.Month;
				int day = d.Day;
				int hour = d.Hour;
				int minuteTens = d.Minute / 10;
				bool doXShift = true;

				if(year == lastYear && month == lastMonth && day == lastDay
					&& hour == lastHour && (!minutes || minuteTens == lastMinuteTens))
				{
					continue;
				} 

				if(yearsOnly)
				{
					if((!yearsMany && year != lastYear && x - lastDrawnX > 30) || (yearsMany && year/100 != lastCentury))
					{
						str = "" + (yearsMany ? (year/100*100) : year);
						yShift = 10;
						brush = brushBright;
						font = m_fontSmall;
						charWidth = m_charWidthSmall;
						drawSmall = false;
					}
					else
					{
						lastYear = year;
						lastCentury = year / 100;
						lastMonth = month;
						lastDay = day;
						lastHour = hour;
						lastMinuteTens = minuteTens;
						continue;
					}
				}
				else if(yearsAndMonths)
				{
					if(year != lastYear) 
					{
						str = "" + year;
						if(lastDrawnX == -1)
						{
							yShift = 22;		// starting year in the corner, not interfering with the normally printed
							brush = brushDim;
							font = m_fontSmall;
							charWidth = m_charWidthSmall;
						}
						else
						{
							yShift = 10;
							brush = brushBright;
							font = m_fontLarge;
							charWidth = m_charWidthLarge;
						}
						drawSmall = false;
					}
					else if(month != lastMonth) 
					{
						str = months[d.Month - 1];
						doXShift = false;
						yShift = 5;
						brush = brushDim;
						font = m_fontSmall;
						charWidth = m_charWidthSmall;
						drawSmall = true;
					}
					else
					{
						lastYear = year;
						lastCentury = year / 100;
						lastMonth = month;
						lastDay = day;
						lastHour = hour;
						lastMinuteTens = minuteTens;
						continue;
					}
				}
				else
				{
					if(day != lastDay) 
					{
						str = "" + d.Day;
						yShift = 10;
						brush = brushBright;
						font = m_fontLarge;
						charWidth = m_charWidthLarge;
						drawSmall = false;
					} 
					else 
					{
						str = minutes ? String.Format("{0:D02}:{1:D02}", d.Hour, d.Minute)
										: String.Format("{0:D02}:00", d.Hour);
						yShift = 5;
						brush = brushDim;
						font = m_fontSmall;
						charWidth = m_charWidthSmall;
						drawSmall = true;
					}

					if(month != lastMonth) 
					{
						str = d.ToShortDateString();
						yShift = 24;
						font = m_fontLarge;
						charWidth = m_charWidthLarge;
					}
				}

				xValue = x + m_xZeroMark;
				if(drawSmall)
				{
					if(x < 10 && x - lastDrawnX < 5)
					{
						drawSmallLines = false;
					}
					if(drawSmallLines)
					{
						g.DrawLine(pen, xValue, m_yZeroMark, xValue, m_yZeroMark + yShift);
					}
				}
				else if (x > 2)
				{
					g.DrawLine(pen, xValue, m_yZeroMark, xValue, m_yZeroMark + yShift);
				}
				xShift = doXShift ? str.Length * charWidth / 2 : 0;
				if(drawSmall)
				{
					if(drawSmallLines && x - lastDrawnX > 40)
					{
						g.DrawString(str, font, brush, xValue - xShift, m_yZeroMark + yShift);
						lastDrawnX = x;
					}
				} 
				else
				{
					//if(!(lastDrawnX == -1 && (yearsAndMonths || yearsOnly)))
					{
						g.DrawString(str, font, brush, xValue - xShift, m_yZeroMark + yShift);
					}
					lastDrawnX = x;
				}
				lastYear = year;
				lastCentury = year / 100;
				lastMonth = month;
				lastDay = day;
				lastHour = hour;
				lastMinuteTens = minuteTens;
			}
			brush = brushDim;
			font = m_fontSmall;
			yShift = -14;
			xShift = -5;
			int xPos = maxX + m_xZeroMark + xShift;
			if(UseUtcTime)
			{
				g.DrawString("UTC", font, brush, xPos, m_yZeroMark + yShift);
			}
			else
			{
				g.DrawString("Loc", font, brush, xPos, m_yZeroMark + yShift);
			}
		}
		#endregion

		#region Keystrokes processing

		private void selectEvent(DatedEvent ev, int x)
		{
			if(m_descrById != null && ev.ids() != null)
			{
				m_selectedId = ev.ids()[0];
				m_messageMain = m_descrById(m_selectedId);
				m_messageSecondary = ev.toTableString();
			}
			else
			{
				m_selectedId = -1;
				m_messageMain = ev.toTableString1();
				m_messageSecondary = ev.toTableString();
			}
			string[] properties = ev.sValues();
			if(m_selectedId != -1)
			{
				if(properties != null && properties[0].IndexOf("w") != -1)
				{
					m_enDis.enable(ED_BROWSE);
				} 
				else 
				{
					m_enDis.disable(ED_BROWSE);
				}
				m_enDis.enable(ED_ZOOM);
			}
			m_selected = x;
			m_lastX = m_selected;
			this.Invalidate();
		}

		private void selectNonEvent(int x)
		{
			// no event in the vicinity; just display time:
			long timeClickedMs = m_startTime + x * m_timeStep;
			DateTime clickTime = new DateTime(timeClickedMs * 10000);
			if(!UseUtcTime)
			{
				clickTime = Project.zuluToLocal(clickTime);
			}
			m_messageMain = "";
			m_messageSecondary = clickTime.ToLongDateString() + "  " + clickTime.ToLongTimeString();
			m_enDis.disable(ED_ZOOM);
			m_enDis.disable(ED_BROWSE);
			m_selected = x;
			m_selectedId = -1;
			m_lastX = m_selected;
			this.Invalidate(this.ClientRectangle);
		}

		private void GraphByTimeControl_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			//LibSys.StatusBar.Trace("Key=" + keyCode);
			if (keystrokeProcessed) 
			{
				return;
			}

			//if(e.KeyChar.Equals('w')) 
			//{
			//}
		}

		private bool keystrokeProcessed;
		private bool controlDown = false;
		private bool shiftDown = false;
		private bool altDown = false;

		protected override bool ProcessDialogKey(Keys keyData)
		{
			//LibSys.StatusBar.Trace("ProcessDialogKey: " + keyData);
			keystrokeProcessed = true;

			int x;

			if(keyData == Keys.Home)
			{
				m_lastX = 0;
			}
			else if(keyData == Keys.End)
			{
				m_lastX = m_graphEvents.Length;
			}

			switch (keyData)
			{
				case Keys.Enter:
					browse();
					break;
				case Keys.Space:
					zoom();
					break;
				case Keys.Home:			// Home
				case Keys.Right:		// right arrow
				case Keys.Up:			// up arrow
				case Keys.PageUp:		// page up
					if(m_endSelection != -1) 
					{
						m_lastX = m_endSelection;
					}
					x = m_lastX + 1;

					m_startSelection = -1;
					m_endSelection = -1;

					if(inRange(x)) 
					{
						for(int i=0; i < m_graphEvents.Length - x ; i++) 
						{
							DatedEvent ev;
							if(inRange(x + i)) 
							{
								ev = m_graphEvents[x + i];
								if(ev != null) 
								{
									selectEvent(ev, x + i);
									return true;
								}
							}
						}
						// no event in the vicinity; just display time:
						selectNonEvent(x);
					}
					Invalidate();
					break;
				case Keys.End:			// End
				case Keys.Left:			// left arrow
				case Keys.Down:			// down arrow
				case Keys.PageDown:		// page down
					if(m_startSelection != -1) 
					{
						m_lastX = m_startSelection;
					}
					x = m_lastX - 1;

					m_startSelection = -1;
					m_endSelection = -1;

					if(inRange(x)) 
					{
						for(int i=0; i < x ; i++) 
						{
							DatedEvent ev;
							if(inRange(x - i)) 
							{
								ev = m_graphEvents[x - i];
								if(ev != null) 
								{
									selectEvent(ev, x - i);
									return true;
								}
							}
						}
						// no event in the vicinity; just display time:
						selectNonEvent(x);
					}
					Invalidate();
					break;
				default:
					if ((int)(Keys.Control & keyData) != 0) 
					{
						//LibSys.StatusBar.Trace("ProcessDialogKey: Control down");
						controlDown = true;
					}
					else if ((int)(Keys.Shift & keyData) != 0) 
					{
						//The Shift key is pressed. Do something here if you want.
						//LibSys.StatusBar.Trace("ProcessDialogKey: Shift down");
						shiftDown = true;
					}
					else if ((int)(Keys.Alt & keyData) != 0) 
					{
						//The Alt key is pressed. Do something here if you want.
						//LibSys.StatusBar.Trace("ProcessDialogKey: Alt down");
						altDown = true;
					}
					else 
					{
						keystrokeProcessed = false; // let KeyPress event handler handle this keystroke.
					}
					break;
			}			 
			if(keystrokeProcessed) 
			{
				return true;
			} 
			else 
			{
				return base.ProcessDialogKey(keyData);
			}
		}
		#endregion

		#region Mouse / Keystrokes processing helpers

		private bool inRange(int x)
		{
			return x >= 0 && x < m_graphEvents.Length;
		}

		private void toLastEvent()
		{
			m_lastX = m_graphEvents.Length;
			if(m_startSelection != -1) 
			{
				m_lastX = m_startSelection;
			}
			int x = m_lastX - 1;

			m_startSelection = -1;
			m_endSelection = -1;

			if(inRange(x)) 
			{
				for(int i=0; i < x ; i++) 
				{
					DatedEvent ev;
					if(inRange(x - i)) 
					{
						ev = m_graphEvents[x - i];
						if(ev != null) 
						{
							selectEvent(ev, x - i);
							return;
						}
					}
				}
				// no event in the vicinity; just display time:
				selectNonEvent(x);
			}
			Invalidate();
		}
		#endregion

		#region Mouse clicks / move / drag  processing

		private Point mousePosition;
		private bool dragging = false;
		private int m_dragStart;		// in terms of graph X coordinate
		private int m_dragEnd;			// in terms of graph X coordinate
		private MouseButtons btnDown = MouseButtons.None;

		private void GraphByTimeControl_Click(object sender, System.EventArgs e)
		{
			if(dragging || m_allEvents == null || m_allEvents.Count < 1)
			{
				return;
			}

			int x = mousePosition.X;
			int y = mousePosition.Y;

			x -= m_xZeroMark;

			if(m_startSelection != -1 && m_endSelection != -1 && x > m_startSelection && x < m_endSelection)
			{
				// clicked into a valid selection - zoom in.
				m_showSelected();
				return;
			}

			m_startSelection = -1;
			m_endSelection = -1;

			if(inRange(x)) 
			{
				for(int i=0; i < 10 ; i++) 
				{
					DatedEvent ev;
					if(inRange(x + i)) 
					{
						ev = m_graphEvents[x + i];
						if(ev != null) 
						{
							selectEvent(ev, x + i);
							return;
						}
					}
					if(inRange(x - i)) 
					{
						ev = m_graphEvents[x - i];
						if(ev != null) 
						{
							selectEvent(ev, x - i);
							return;
						}
					}
				}
				// no event in the vicinity; just display time:
				selectNonEvent(x);
			}
		}

		private void GraphByTimeControl_DoubleClick(object sender, System.EventArgs e)
		{
		}

		private void GraphByTimeControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			this.mousePosition = new Point(e.X, e.Y);	// for possible drag start in MouseMove
			if(e.Button == MouseButtons.Right)
			{
				GraphSelectedArea();
			}
		}

		private void startDrag(int x)
		{
			this.Invalidate(this.ClientRectangle);
			m_startSelection = -1;
			m_endSelection = -1;

			m_dragStart = x - m_xZeroMark;		// in terms of graph X coordinate
			if(m_dragStart < 0)
			{
				m_dragStart = 0;
			}
			if(m_dragStart >= m_xAxisSize)
			{
				m_dragStart = m_xAxisSize - 1;
			}
			dragging = true;
			//LibSys.StatusBar.Trace("start drag: " + m_dragStart);
		}

		private void endDrag(int x)
		{
			m_dragEnd = x - m_xZeroMark;		// in terms of graph X coordinate
			if(m_dragEnd < 0)
			{
				m_dragEnd = 0;
			}
			if(m_dragEnd >= m_xAxisSize)
			{
				m_dragEnd = m_xAxisSize - 1;
			}
			dragging = false;
			//LibSys.StatusBar.Trace("end drag: " + m_dragStart);

			this.Invalidate(this.ClientRectangle);
			prevRubberRect = Rectangle.Empty;
			currentRubberRect = Rectangle.Empty;

			m_startSelection = Math.Min(m_dragStart, m_dragEnd);
			m_endSelection = Math.Max(m_dragStart, m_dragEnd);
		}

		private Rectangle prevRubberRect;
		private Rectangle currentRubberRect;

		private void GraphByTimeControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(!dragging && btnDown == MouseButtons.None && e.Button == MouseButtons.Left)
			{
				// did we skip a MouseDown? make up for it (using previious mouse pos from click)
				startDrag(mousePosition.X);
			}
			btnDown = e.Button;
			this.mousePosition = new Point(e.X, e.Y);

			if(dragging) 
			{
				Graphics g = this.CreateGraphics();
				// draw a "rubber band" to highlight the selected area:
				int x1 = e.X - m_xZeroMark;
				if(x1 < 0)
				{
					x1 = 0;
				}
				if(x1 >= m_xAxisSize)
				{
					x1 = m_xAxisSize - 1;
				}
				int x2 = m_dragStart;
				int xStart = Math.Min(x1, x2);
				int xEnd = Math.Max(x1, x2);

				if(xStart != xEnd) 
				{
					currentRubberRect = new Rectangle(xStart + m_xZeroMark, 0, xEnd - xStart, m_currSize.Height);
					Rectangle invRect;
					if(prevRubberRect.IsEmpty)
					{
						invRect = currentRubberRect;
					}
					else
					{
						invRect = new Rectangle(Math.Min(xStart + m_xZeroMark, prevRubberRect.X) - 1, 0,
							Math.Max(currentRubberRect.Width, prevRubberRect.Width) + 2, m_currSize.Height);			
					}
					this.Invalidate(invRect);	// actual drawing in Paint()
					prevRubberRect = Rectangle.Empty;
				}
			}
		}

		private void GraphByTimeControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			this.mousePosition = new Point(e.X, e.Y);

			if(e.Button == MouseButtons.Left)
			{
				btnDown = MouseButtons.None;
				if(dragging)
				{
					endDrag(mousePosition.X);
				}
			}
		}

		#endregion

	}
}
