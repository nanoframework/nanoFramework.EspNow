//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace nanoFramework.EspNow
{
    /// <summary>
    /// Represents the method that will handle the <see cref="EspNowController.DataReceived"/> event
    /// of an <see cref="EspNowController"/> object.
    /// </summary>
    /// <param name="sender">The sender of the event, which is the <see cref="EspNowController"/> object.</param>
    /// <param name="e">A <see cref="DataReceivedEventArgs"/> object that contains the event data.</param>
    public delegate void DataReceivedEventHandler(
        object sender,
        DataReceivedEventArgs e);
}
