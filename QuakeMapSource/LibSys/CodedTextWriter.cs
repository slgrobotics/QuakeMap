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
using System.IO;

namespace LibSys
{
	/// <summary>
	/// CodedTextWriter scrambles output.
	/// </summary>
	public class CodedTextWriter : StreamWriter	// TextWriter
	{
		private byte[] m_codeKey;
		private int byteCount;

		public CodedTextWriter(Stream stream, byte[] codeKey)
			: base(stream)
		{
			m_codeKey = codeKey;
			byteCount = 0;
		}

		public override void Write(char ch)
		{
			byte b = (byte)ch;
			b = (byte)(b ^ m_codeKey[(byteCount++) % m_codeKey.Length]);
			BaseStream.WriteByte(b);
		}

		public override void Write(string str)
		{
			char[] chars = str.ToCharArray();
			Write(chars);
		}

		public override void Write(char[] chars)
		{
			foreach(char ch in chars)
			{
				Write(ch);
			}
		}

		public override void Write(char[] chars, int index, int count)
		{
			for(int i=0; i < count ;i++)
			{
				Write(chars[index + i]);
			}
		}

		public override void Close()
		{
			base.Close();
		}
	}
}
