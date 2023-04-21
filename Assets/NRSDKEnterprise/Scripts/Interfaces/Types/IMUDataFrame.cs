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

    /// <summary> An imu data frame. </summary>
    public struct IMUDataFrame
    {
        /// <summary> The time stamp. </summary>
        public UInt64 timeStamp;
        /// <summary> The gyroscope. </summary>
        public NativeVector3f gyroscope;
        /// <summary> The accelerometer. </summary>
        public NativeVector3f accelerometer;

        /// <summary> Copies to described by copy. </summary>
        /// <param name="copy"> The copy.</param>
        public void CopyTo(IMUDataFrame copy)
        {
            copy.timeStamp = this.timeStamp;
            copy.gyroscope = this.gyroscope;
            copy.accelerometer = this.accelerometer;
        }

        /// <summary> Copies from described by clone. </summary>
        /// <param name="clone"> The clone.</param>
        public void CopyFrom(IMUDataFrame clone)
        {
            this.timeStamp = clone.timeStamp;
            this.gyroscope = clone.gyroscope;
            this.accelerometer = clone.accelerometer;
        }

        /// <summary> Convert this object into a string representation. </summary>
        /// <returns> A string that represents this object. </returns>
        public override string ToString()
        {
            return string.Format("timestamp:{0} gyroscope:{1} accelerometer:{2}", timeStamp, gyroscope.ToString(), accelerometer.ToString());
        }
    }
}
