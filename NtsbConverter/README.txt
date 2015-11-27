This is a sample NTSB data converter.

More information is available at http://www.quakemap.com (click on "Sample NTSB data importer" link at the bottom of the left navigation pane).

This program demonstrates how to use the QuakeMap API to create data which can later be exported in a convenient form (for example, GPX format).
 The result is a data sample for the WEST of the United States (saved in GPX format after conversion).

This program opens original NTSB files (Access95 format, freely available at http://www.ntsb.gov/ntsb/query.asp,
 follow link "Downloadable Datasets", "Access95" - ftp://www.ntsb.gov/avdata/Access95/) and commands QuakeMap to
 create waypoints for the records that contain coordinates. Only records with specified coordinates are actually useful,
 which limits Access95 file set to 1983, 1989, 1990, 1995, 2000-2004(March).
 You can easily create your own datasets using this sample code. 
 
Quick Start:

Compile and run the solution; in the form browse to the actual location of downloaded and unpacked .MDB file.
Click "Open", and watch the center pane fill with waypoints (NTSB crash points).
Select "West" in the Filter section.
Make sure QuakeMap is running, and click "Pass the data to QuakeMap" button.
Watch the waypoints appear in QuakeMap.
 
 
 

===========================================================================================
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


