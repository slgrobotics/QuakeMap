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
	public unsafe class WebsafeGrayscaleQuantizer : WebsafeQuantizer
	{
		/// <summary>
		/// Construct the palette quantizer
		/// </summary>
		/// <param name="palette">The color palette to quantize to</param>
		/// <remarks>
		/// Palette quantization only requires a single quantization step
		/// </remarks>
		public WebsafeGrayscaleQuantizer () : base()
		{
		}

		/// <summary>
		/// Override this to process the pixel in the second pass of the algorithm
		/// -- overriden here to limit distance analysis to 216 web-safe colors
		/// </summary>
		/// <param name="pixel">The pixel to quantize</param>
		/// <returns>The quantized value</returns>
		protected override byte QuantizePixel ( Color32* pixel )
		{
			// indexes of the B/W and grey area of the websafe palette:
			int[] indexes = new int[] { 0x00, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF, 0xE0, 0xE1 };

			byte	colorIndex = 0 ;
			int		colorHash = pixel->ARGB ;	

			// Check if the color is in the lookup table
			if ( _colorMap.ContainsKey ( colorHash ) )
				colorIndex = (byte)_colorMap[colorHash] ;
			else
			{
				// Not found - loop through the palette and find the nearest match.
				int	leastDistance = int.MaxValue ;
				int red = pixel->Red ;
				int green = pixel->Green;
				int blue = pixel->Blue;

				// Loop through the entire palette, looking for the closest color match
				foreach ( int index in indexes )
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

				// Now I have the color, pop it into the hashtable for next time
				_colorMap.Add ( colorHash , colorIndex ) ;
			}

			return colorIndex ;
		}
	}
}
