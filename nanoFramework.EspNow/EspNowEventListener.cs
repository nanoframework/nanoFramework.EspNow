//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Runtime.Events;
using System;

namespace nanoFramework.EspNow
{
    /// <summary>
    /// Bridges native ESP-NOW notifications to the managed <see cref="EspNowController"/> events.
    /// There is a single instance for the whole assembly, matching the fact that only one <see cref="EspNowController"/> can exist on a device at a time.
    /// </summary>
    internal sealed class EspNowEventListener : IEventProcessor, IEventListener
    {
        private EspNowController _controller;

        public EspNowEventListener()
        {
            EventSink.AddEventProcessor(EventCategory.EspNow, this);
            EventSink.AddEventListener(EventCategory.EspNow, this);
        }

        internal void SetController(EspNowController controller)
        {
            _controller = controller;
        }

        internal void ClearController()
        {
            _controller = null;
        }

        public void InitializeForEventSource()
        {
            // This method has to exist.
        }

        public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time) => new EspNowEvent { EventType = (EspNowEventType)data2 };

        public bool OnEvent(BaseEvent ev)
        {
            var espNowEvent = (EspNowEvent)ev;
            EspNowController controller = _controller;

            if (controller == null)
            {
                return false;
            }

            switch (espNowEvent.EventType)
            {
                case EspNowEventType.DataReceived:
                    controller.OnDataReceivedInternal();
                    return true;

                case EspNowEventType.DataSent:
                    controller.OnDataSentInternal();
                    return true;

                default:
                    return false;
            }
        }
    }
}
