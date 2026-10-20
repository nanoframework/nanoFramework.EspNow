//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace nanoFramework.EspNow
{
    /// <summary>
    /// ESP-NOW controller class.
    /// </summary>
    public sealed class EspNowController : IDisposable
    {
        private const int MacAddressLength = 6;
        private const byte BroadcastMacByte = 0xff;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static EspNowController s_instance;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly object s_syncLock = new object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly EspNowEventListener s_eventListener = new EspNowEventListener();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _disposed;

        // this is used as the lock object
        // a lock is required because multiple threads can access the EspNowController
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object _syncLock = new object();

        private DataReceivedEventHandler _callbacksDataReceivedEvent = null;
        private DataSentEventHandler _callbacksDataSentEvent = null;

        /// <summary>
        /// Event raised when data is received.
        /// </summary>
        public event DataReceivedEventHandler DataReceived
        {
            add
            {
                lock (_syncLock)
                {
                    if (_disposed)
                    {
#pragma warning disable S3877 // OK to throw this here
                        throw new ObjectDisposedException();
#pragma warning restore S3877 // Exceptions should not be thrown from unexpected methods
                    }

                    DataReceivedEventHandler callbacksOld = _callbacksDataReceivedEvent;
                    DataReceivedEventHandler callbacksNew = (DataReceivedEventHandler)Delegate.Combine(callbacksOld, value);

                    try
                    {
                        _callbacksDataReceivedEvent = callbacksNew;
                    }
                    catch
                    {
                        _callbacksDataReceivedEvent = callbacksOld;

                        throw;
                    }
                }
            }

            remove
            {
                lock (_syncLock)
                {
                    if (_disposed)
                    {
#pragma warning disable S3877 // OK to throw this here
                        throw new ObjectDisposedException();
#pragma warning restore S3877 // Exceptions should not be thrown from unexpected methods
                    }

                    DataReceivedEventHandler callbacksOld = _callbacksDataReceivedEvent;
                    DataReceivedEventHandler callbacksNew = (DataReceivedEventHandler)Delegate.Remove(callbacksOld, value);

                    try
                    {
                        _callbacksDataReceivedEvent = callbacksNew;
                    }
                    catch
                    {
                        _callbacksDataReceivedEvent = callbacksOld;

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Event raised when a previously sent packet completes at the MAC layer (either delivered or failed). 
        /// Only one send is expected to be in flight at a time.
        /// </summary>
        public event DataSentEventHandler DataSent
        {
            add
            {
                lock (_syncLock)
                {
                    if (_disposed)
                    {
#pragma warning disable S3877 // OK to throw this here
                        throw new ObjectDisposedException();
#pragma warning restore S3877 // Exceptions should not be thrown from unexpected methods
                    }

                    DataSentEventHandler callbacksOld = _callbacksDataSentEvent;
                    DataSentEventHandler callbacksNew = (DataSentEventHandler)Delegate.Combine(callbacksOld, value);

                    try
                    {
                        _callbacksDataSentEvent = callbacksNew;
                    }
                    catch
                    {
                        _callbacksDataSentEvent = callbacksOld;

                        throw;
                    }
                }
            }

            remove
            {
                lock (_syncLock)
                {
                    if (_disposed)
                    {
#pragma warning disable S3877 // OK to throw this here
                        throw new ObjectDisposedException();
#pragma warning restore S3877 // Exceptions should not be thrown from unexpected methods
                    }

                    DataSentEventHandler callbacksOld = _callbacksDataSentEvent;
                    DataSentEventHandler callbacksNew = (DataSentEventHandler)Delegate.Remove(callbacksOld, value);

                    try
                    {
                        _callbacksDataSentEvent = callbacksNew;
                    }
                    catch
                    {
                        _callbacksDataSentEvent = callbacksOld;

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Represents an ESP-NOW controller.
        /// </summary>
        /// <exception cref="InvalidOperationException">Only one <see cref="EspNowController"/> instance is allowed per device.</exception>"
        /// <exception cref="EspNowException">Native ESP-NOW initialization failed.</exception>
        public EspNowController()
        {
            lock (s_syncLock)
            {
                // Only allow one controller instance per device
                if (s_instance != null)
                {
                    throw new InvalidOperationException();
                }

                // call native init to allow HAL/PAL inits related with ESP-NOW hardware
                var initResult = NativeInitialize();

                if (initResult != 0)
                {
                    throw new EspNowException(initResult);
                }

                // Set this as the singleton instance
#pragma warning disable S3010 // Static fields should not be updated in constructors
                s_instance = this;
#pragma warning restore S3010 // Static fields should not be updated in constructors

                // Register with the event listener to receive callbacks from native interrupts
                s_eventListener.SetController(this);
            }
        }

        /// <summary>
        /// Add peer to which data will be sent.
        /// </summary>
        /// <param name="peerMac">MAC address of peer.</param>
        /// <param name="channel">WiFi channel to be used.</param>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="peerMac"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="peerMac"/> is not 6 bytes long.</para>
        /// <para>-or-</para>
        /// <para>Trying to enable encryption for a broadcast peer [FF-FF-FF-FF-FF-FF].</para>
        /// </exception>
        /// <exception cref="EspNowException">Native ESP-NOW peer registration failed.</exception>
        public void AddPeer(
            byte[] peerMac,
            byte channel)
        {
            AddPeer(peerMac, channel, false, null);
        }

        /// <summary>
        /// Add peer to which data will be sent.
        /// </summary>
        /// <param name="peerMac">MAC address of peer.</param>
        /// <param name="channel">WiFi channel to be used.</param>
        /// <param name="encrypted"><see langword="true"/> to enable ESP-NOW encryption for this peer.</param>
        /// <param name="localMasterKey">16-byte local master key used when encryption is enabled.</param>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="peerMac"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="peerMac"/> is not 6 bytes long.</para>
        /// <para>-or-</para>
        /// <para><paramref name="localMasterKey"/> is not 16 bytes long.</para>
        /// <para>-or-</para>
        /// <para>Trying to enable encryption for a broadcast peer [FF-FF-FF-FF-FF-FF].</para>
        /// </exception>
        /// <exception cref="EspNowException">Native ESP-NOW peer registration failed.</exception>
        public void AddPeer(
            byte[] peerMac,
            byte channel,
            bool encrypted,
            byte[] localMasterKey)
        {
            if (peerMac == null)
            {
                throw new ArgumentException();
            }

            if (peerMac.Length != MacAddressLength)
            {
                throw new ArgumentException();
            }

            if (localMasterKey != null
                && localMasterKey.Length != 16)
            {
                throw new ArgumentException();
            }

            if (encrypted
                && IsBroadcastMac(peerMac))
            {
                throw new ArgumentException();
            }

            var addResult = NativeEspNowAddPeer(
                peerMac,
                channel,
                encrypted,
                localMasterKey);

            if (addResult != 0)
            {
                throw new EspNowException(addResult);
            }
        }

        /// <summary>
        /// Send data to already registered peer. Fire-and-forget: this call only queues the frame for transmission. 
        /// Subscribe to <see cref="DataSent"/> to be notified of the MAC-layer delivery result.
        /// </summary>
        /// <param name="peerMac">MAC address of already added peer.</param>
        /// <param name="data">Data to be sent.</param>
        /// <param name="dataLen">Length of data.</param>
        /// <exception cref="EspNowException">Native ESP-NOW send failed.</exception>
        public void Send(byte[] peerMac, byte[] data, int dataLen)
        {
            var nret = NativeEspNowSend(peerMac, data, dataLen);
            if (nret != 0)
            {
                throw new EspNowException(nret);
            }
        }

        /// <summary>
        /// Reads the next complete packet from the native receive queue.
        /// </summary>
        /// <param name="timeout">
        /// Milliseconds to wait for a packet. <c>0</c> polls the queue and returns immediately.
        /// </param>
        /// <returns>
        /// The next received packet, or <see langword="null"/> when <paramref name="timeout"/>
        /// is <c>0</c> and no packet was queued.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is negative.</exception>
        /// <exception cref="TimeoutException">No packet arrived within <paramref name="timeout"/>.</exception>
        public DataReceivedEventArgs ReadPacket(int timeout)
        {
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            lock (_syncLock)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException();
                }

                var peerMac = new byte[MacAddressLength];
                var data = new byte[NativeGetMaximumDataLength()];
                var dataLen = NativeReadPacket(peerMac, data, timeout);

                if (dataLen == 0)
                {
                    // non-blocking poll (timeout == 0) found nothing queued
                    return null;
                }

                if (dataLen != data.Length)
                {
                    var packetData = new byte[dataLen];
                    for (var index = 0; index < dataLen; index++)
                    {
                        packetData[index] = data[index];
                    }

                    data = packetData;
                }

                return new DataReceivedEventArgs(peerMac, data, dataLen);
            }
        }

        /// <summary>
        /// Gets the number of packets discarded because the receive queue was full.
        /// </summary>
        public int ReceiveOverflowCount => NativeGetReceiveOverflowCount();

        internal void OnDataReceivedInternal()
        {
            DataReceivedEventHandler callbacks;

            lock (_syncLock)
            {
                if (_disposed || _callbacksDataReceivedEvent == null)
                {
                    return;
                }

                callbacks = _callbacksDataReceivedEvent;
            }

            try
            {
                // Avoid calling this under a lock to prevent a potential lock inversion.
                var packet = ReadPacket(0);

                if (packet != null)
                {
                    callbacks.Invoke(this, packet);
                }
            }
            catch (ObjectDisposedException)
            {
                // Dispose() raced with this notification.
            }
        }

        internal void OnDataSentInternal()
        {
            DataSentEventHandler callbacks;

            lock (_syncLock)
            {
                if (_disposed || _callbacksDataSentEvent == null)
                {
                    return;
                }

                callbacks = _callbacksDataSentEvent;
            }

            var peerMac = new byte[MacAddressLength];
            var status = (EspNowSendStatus)NativeReadSendStatus(peerMac);

            // Avoid calling this under a lock to prevent a potential lock inversion.
            callbacks.Invoke(this, new DataSentEventArgs(peerMac, status));
        }

        private void Dispose(bool isDisposing)
        {
            if (!_disposed)
            {
                if (isDisposing)
                {
                    lock (s_syncLock)
                    {
                        // Clear the singleton instance
#pragma warning disable S2696 // Instance members should not write to "static" fields
                        s_instance = null;
#pragma warning restore S2696 // Instance members should not write to "static" fields

                        s_eventListener.ClearController();
                    }
                }

                NativeDispose(isDisposing);

                _disposed = true;
            }
        }

        /// <summary>
        /// Destructor to assure Dispose will be called.
        /// </summary>
        ~EspNowController()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose()
        /// </summary>
        public void Dispose()
        {
            lock (_syncLock)
            {
                if (!_disposed)
                {
                    Dispose(true);

                    GC.SuppressFinalize(this);
                }
            }
        }

        private static bool IsBroadcastMac(byte[] mac)
        {
            if (mac == null || mac.Length != MacAddressLength)
            {
                return false;
            }

            for (int i = 0; i < mac.Length; i++)
            {
                if (mac[i] != BroadcastMacByte)
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeInitialize();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeDispose(bool isDisposing);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeEspNowSend(byte[] peerMac, byte[] data, int dataLen);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeEspNowAddPeer(byte[] peerMac, byte channel, bool encrypted, byte[] localMasterKey);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeGetMaximumDataLength();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeReadPacket(byte[] peerMac, byte[] data, int timeout);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeGetReceiveOverflowCount();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeReadSendStatus(byte[] peerMac);
    }
}
