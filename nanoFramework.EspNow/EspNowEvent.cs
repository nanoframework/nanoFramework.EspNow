//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;

namespace nanoFramework.EspNow
{
    /// <summary>
    /// Managed event object produced by <see cref="EspNowEventListener.ProcessEvent"/> posted by the native ESP-NOW driver. 
    /// </summary>
    internal class EspNowEvent : BaseEvent
    {
        public EspNowEventType EventType;
    }
}
