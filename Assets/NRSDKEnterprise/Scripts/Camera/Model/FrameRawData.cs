/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal.Enterprise
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary> A frame raw data extension. </summary>
    public class FrameRawDataExtension
    {
        /// <summary> Makes a safe. </summary>
        /// <param name="lefttextureptr">  The lefttextureptr.</param>
        /// <param name="righttextureptr"> The righttextureptr.</param>
        /// <param name="size">            The size.</param>
        /// <param name="timestamp">       The timestamp.</param>
        /// <param name="gain">            The gain.</param>
        /// <param name="exposureTime">    The exposure time.</param>
        /// <param name="frame">           [in,out] The frame.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public static bool MakeSafe(IntPtr lefttextureptr, IntPtr righttextureptr, int size, UInt64 timestamp, UInt32 gain, UInt32 exposureTime, ref FrameRawData frame)
        {
            if (lefttextureptr == IntPtr.Zero || righttextureptr == IntPtr.Zero || size <= 0 || size % 2 != 0)
            {
                NRDebugger.Error(string.Format("lefttextureptr:{0} righttextureptr:{1} size:{2} timestamp:{3}",
                     lefttextureptr.ToInt32(), righttextureptr.ToInt32(), size, timestamp));
                return false;
            }
            if (frame.data == null || frame.data.Length != size)
            {
                frame.data = new byte[size];
            }
            frame.timeStamp = timestamp;
            frame.gain = gain;
            frame.exposureTime = exposureTime;
            Marshal.Copy(lefttextureptr, frame.data, 0, size / 2);
            Marshal.Copy(righttextureptr, frame.data, size / 2, size / 2);
            return true;
        }
    }
}
