/* 
 * http://dotnetjunkies.com/weblog/bsblog/archive/01262004.aspx
 * 
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF 
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
  THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
  PARTICULAR PURPOSE. 
  
    This is sample code and is freely distributable. 
*/ 

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

namespace LibSys //ImageQuantization
{
	/// <summary>
	/// This is really a SuperWaba Palette Quantizer, it relies on the distance-finding QuantizePixel() in PaletteQuantizer.
	/// </summary>
	public unsafe class WebsafeQuantizer : PaletteQuantizer
	{
		private static Color[] m_colors = null;

		/// <summary>
		/// Construct the palette quantizer
		/// </summary>
		/// <param name="palette">The color palette to quantize to</param>
		/// <remarks>
		/// Palette quantization only requires a single quantization step
		/// </remarks>
		public WebsafeQuantizer () : base( new ArrayList() )
		{
			if(m_colors == null)
			{
				// the following table is taken from SuperWaba Palette class docs:
				int[] pal = new int[] {
										  0xFFFFFF, 0xFFCCFF, 0xFF99FF, 0xFF66FF, 0xFF33FF, 0xFF00FF,
										  0xFFFFCC, 0xFFCCCC, 0xFF99CC, 0xFF66CC, 0xFF33CC, 0xFF00CC,
										  0xFFFF99, 0xFFCC99, 0xFF9999, 0xFF6699, 0xFF3399, 0xFF0099,
										  0xCCFFFF, 0xCCCCFF, 0xCC99FF, 0xCC66FF, 0xCC33FF, 0xCC00FF,
										  0xCCFFCC, 0xCCCCCC, 0xCC99CC, 0xCC66CC, 0xCC33CC, 0xCC00CC,
										  0xCCFF99, 0xCCCC99, 0xCC9999, 0xCC6699, 0xCC3399, 0xCC0099,
										  0x99FFFF, 0x99CCFF, 0x9999FF, 0x9966FF, 0x9933FF, 0x9900FF,
										  0x99FFCC, 0x99CCCC, 0x9999CC, 0x9966CC, 0x9933CC, 0x9900CC,
										  0x99FF99, 0x99CC99, 0x999999, 0x996699, 0x993399, 0x990099,
										  0x66FFFF, 0x66CCFF, 0x6699FF, 0x6666FF, 0x6633FF, 0x6600FF,
										  0x66FFCC, 0x66CCCC, 0x6699CC, 0x6666CC, 0x6633CC, 0x6600CC,
										  0x66FF99, 0x66CC99, 0x669999, 0x666699, 0x663399, 0x660099,
										  0x33FFFF, 0x33CCFF, 0x3399FF, 0x3366FF, 0x3333FF, 0x3300FF,
										  0x33FFCC, 0x33CCCC, 0x3399CC, 0x3366CC, 0x3333CC, 0x3300CC,
										  0x33FF99, 0x33CC99, 0x339999, 0x336699, 0x333399, 0x330099,
										  0x00FFFF, 0x00CCFF, 0x0099FF, 0x0066FF, 0x0033FF, 0x0000FF,
										  0x00FFCC, 0x00CCCC, 0x0099CC, 0x0066CC, 0x0033CC, 0x0000CC,
										  0x00FF99, 0x00CC99, 0x009999, 0x006699, 0x003399, 0x000099,
										  0xFFFF66, 0xFFCC66, 0xFF9966, 0xFF6666, 0xFF3366, 0xFF0066,
										  0xFFFF33, 0xFFCC33, 0xFF9933, 0xFF6633, 0xFF3333, 0xFF0033,
										  0xFFFF00, 0xFFCC00, 0xFF9900, 0xFF6600, 0xFF3300, 0xFF0000,
										  0xCCFF66, 0xCCCC66, 0xCC9966, 0xCC6666, 0xCC3366, 0xCC0066,
										  0xCCFF33, 0xCCCC33, 0xCC9933, 0xCC6633, 0xCC3333, 0xCC0033,
										  0xCCFF00, 0xCCCC00, 0xCC9900, 0xCC6600, 0xCC3300, 0xCC0000,
										  0x99FF66, 0x99CC66, 0x999966, 0x996666, 0x993366, 0x990066,
										  0x99FF33, 0x99CC33, 0x999933, 0x996633, 0x993333, 0x990033,
										  0x99FF00, 0x99CC00, 0x999900, 0x996600, 0x993300, 0x990000,
										  0x66FF66, 0x66CC66, 0x669966, 0x666666, 0x663366, 0x660066,
										  0x66FF33, 0x66CC33, 0x669933, 0x666633, 0x663333, 0x660033,
										  0x66FF00, 0x66CC00, 0x669900, 0x666600, 0x663300, 0x660000,
										  0x33FF66, 0x33CC66, 0x339966, 0x336666, 0x333366, 0x330066,
										  0x33FF33, 0x33CC33, 0x339933, 0x336633, 0x333333, 0x330033,
										  0x33FF00, 0x33CC00, 0x339900, 0x336600, 0x333300, 0x330000,
										  0x00FF66, 0x00CC66, 0x009966, 0x006666, 0x003366, 0x000066,
										  0x00FF33, 0x00CC33, 0x009933, 0x006633, 0x003333, 0x000033,
										  0x00FF00, 0x00CC00, 0x009900, 0x006600, 0x003300,
										  0x111111,		// this one belongs to greyscale, but it is the closest one to black and it is good to have it.
// Warning: the rest of the SuperWaba palette makes PaletteQuantizer:QuantizePixel() think that the closest match is one of the greyscale pixels below.
//  Either keep the below commented out, or make a better judgement on color distance in QuantizePixel().
										  0x222222, 0x444444, 0x555555, 0x777777, 0x888888, 0xAAAAAA, 0xBBBBBB, 0xDDDDDD, 0xEEEEEE,
										  0xC0C0C0, 0x800000, 0x800080, 0x008000, 0x008080,
										  0x000000, 0x000000, 0x000000, 0x000000,
										  0x000000, 0x000000, 0x000000, 0x000000, 0x000000, 0x000000,
										  0x000000, 0x000000, 0x000000, 0x000000, 0x000000, 0x000000,
										  0x000000, 0x000000, 0x000000, 0x000000, 0x000000, 0x000000,
										  0x000000, 0x000000, 0x000000, 0x060003
									  };

				m_colors = new Color[pal.Length];

				for(int cnt=0; cnt < pal.Length; cnt++)
				{
					int rgb = pal[cnt];
					int rr = (rgb & 0xFF0000) >> 16;
					int gg = (rgb & 0xFF00) >> 8;
					int bb = rgb & 0xFF;

					m_colors[cnt] = Color.FromArgb(255, rr, gg, bb);

#if DEBUG
					Color color = m_colors[cnt];
					LibSys.StatusBar.Trace("" + string.Format("{0:X02}", cnt) + " ---> " 
						+ string.Format("{0:X02}", color.R) + string.Format("{0:X02}", color.G) + string.Format("{0:X02}", color.B) + "    " + color);
#endif
				}
			}
			_colors = m_colors;

			// set up palette with colours
			// web safe palette use six values = 00 51 102 153 204 255 

			//SortedList oleColours = new SortedList();

			/* this is a real websafe:
			int[] safe = new int[] {255, 204, 153, 102, 51, 0};
			int cR, cG, cB;
			int cnt = 0;
			for(cR=0; cR < safe.Length; cR++)
			{
				for(cG=0; cG < safe.Length; cG++)
				{
					for(cB=0; cB < safe.Length; cB++)
					{
						//pal.Entries(cnt) = Color.FromArgb(255, safe(cR), safe(cG), safe(cB));
						_colors[cnt] = Color.FromArgb(255, safe[cR], safe[cG], safe[cB]);
						//oleColours[ColorTranslator.ToOle(pal.Entries[cnt])] = cnt;
						cnt++;
					}
				}
			}

			// we have 216 entries filled, fill in the rest:
			while(cnt < _colors.Length)
			{
				// set last entry to be transparent
				_colors[cnt] = Color.FromArgb(0, 255, 255, 255);
				cnt++;
			}

			// set last entry to be transparent
			// pal.Entries(nColors - 1) = Color.FromArgb(0, 255, 255, 255)
			*/
		}

		/// <summary>
		/// Override this to process the pixel in the second pass of the algorithm
		/// -- overriden here to limit distance analysis to 216 web-safe colors
		/// </summary>
		/// <param name="pixel">The pixel to quantize</param>
		/// <returns>The quantized value</returns>
		protected override byte QuantizePixel ( Color32* pixel )
		{
			int maxIndex = 216;
			byte	colorIndex = 0 ;
			int		colorHash = pixel->ARGB ;	

			// Check if the color is in the lookup table
			if ( _colorMap.ContainsKey ( colorHash ) )
				colorIndex = (byte)_colorMap[colorHash] ;
			else
			{
				// Not found - loop through the palette and find the nearest match.
				// Firstly check the alpha value - if 0, lookup the transparent color
				if ( 0 == pixel->Alpha )
				{
					// Transparent. Lookup the first color with an alpha value of 0
					for ( int index = 0 ; index < maxIndex ; index++ )
					{
						if ( 0 == _colors[index].A )
						{
							colorIndex = (byte)index ;
							break ;
						}
					}
				}
				else
				{
					// Not transparent...
					int	leastDistance = int.MaxValue ;
					int red = pixel->Red ;
					int green = pixel->Green;
					int blue = pixel->Blue;

					// Loop through the entire palette, looking for the closest color match
					for ( int index = 0 ; index < maxIndex ; index++ )
					{
						Color	paletteColor = _colors[index];
						
						int	redDistance = paletteColor.R - red ;
						int	greenDistance = paletteColor.G - green ;
						int	blueDistance = paletteColor.B - blue ;

						int		distance = ( redDistance * redDistance ) + 
							( greenDistance * greenDistance ) + 
							( blueDistance * blueDistance ) ;

						if ( distance < leastDistance )
						{
							colorIndex = (byte)index ;
							leastDistance = distance ;

							// And if it's an exact match, exit the loop
							if ( 0 == distance )
								break ;
						}
					}
				}

				// Now I have the color, pop it into the hashtable for next time
				_colorMap.Add ( colorHash , colorIndex ) ;
			}

			return colorIndex ;
		}
	}
}
