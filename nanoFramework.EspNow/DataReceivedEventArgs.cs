//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.EspNow
{
    /// <summary>
    /// Container for DataReceived event data.
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        private readonly byte[] _peerMac;
        private readonly byte[] _data;
        private readonly int _dataLen;

        internal DataReceivedEventArgs(byte[] peerMac, byte[] data, int dataLen)
        {
            _peerMac = peerMac;
            _data = data;
            _dataLen = dataLen;
        }

        /// <summary>
        /// MAC address of peer data was received from.
        /// </summary>
        public byte[] PeerMac { get => _peerMac; }

        /// <summary>
        /// Data received.
        /// </summary>
        public byte[] Data { get => _data; }

        /// <summary>
        /// Length of received data.
        /// </summary>
        public int DataLen { get => _dataLen; }
    }
}
