//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.EspNow
{
    /// <summary>
    /// EspNow related exception
    /// </summary>
    public class EspNowException : Exception
    {
        private readonly int _espErr;

        /// <summary>
        /// Native ESP-NOW error code (a raw esp_err_t value from the ESP-IDF).
        /// </summary>
        /// <remarks>
        /// Common values returned by the ESP-IDF ESP-NOW APIs:
        /// <list type="bullet">
        /// <item><description>ESP-NOW is not initialized (ESP_ERR_ESPNOW_NOT_INIT = 0x3065).</description></item>
        /// <item><description>Invalid argument (ESP_ERR_ESPNOW_ARG = 0x3066).</description></item>
        /// <item><description>Out of memory (ESP_ERR_ESPNOW_NO_MEM = 0x3067).</description></item>
        /// <item><description>ESP-NOW peer list is full (ESP_ERR_ESPNOW_FULL = 0x3068).</description></item>
        /// <item><description>ESP-NOW peer is not found (ESP_ERR_ESPNOW_NOT_FOUND = 0x3069).</description></item>
        /// <item><description>Internal error (ESP_ERR_ESPNOW_INTERNAL = 0x306A).</description></item>
        /// <item><description>ESP-NOW peer has already been added (ESP_ERR_ESPNOW_EXIST = 0x306B).</description></item>
        /// <item><description>Interface error, Wi-Fi and ESP-NOW must run on the same interface (ESP_ERR_ESPNOW_IF = 0x306C).</description></item>
        /// <item><description>Channel error (ESP_ERR_ESPNOW_CHAN = 0x306D).</description></item>
        /// <item><description>Out of memory, generic ESP-IDF error (ESP_ERR_NO_MEM = 0x101).</description></item>
        /// <item><description>Invalid state, e.g. the controller is already initialized, generic ESP-IDF error (ESP_ERR_INVALID_STATE = 0x103).</description></item>
        /// </list>
        /// </remarks>
        public int EspErr { get => _espErr; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="espErr">Native ESP-NOW error code.</param>
        public EspNowException(int espErr)
        {
            _espErr = espErr;
        }
    }
}
