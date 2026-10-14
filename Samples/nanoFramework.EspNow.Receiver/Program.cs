using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace nanoFramework.EspNow.Receiver
{
    public static class Program
    {
        public static void Main()
        {
            byte[] localMasterKey = new byte[]
            {
                0x10, 0x11, 0x12, 0x13,
                0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x1A, 0x1B,
                0x1C, 0x1D, 0x1E, 0x1F
            };

            Debug.WriteLine("ESP-NOW receiver starting...");
            try
            {
                var senderMacAddress = new byte[] { 0xF4, 0x12, 0xFA, 0x5A, 0x24, 0xE0 };
                var controller = new EspNowController();
                controller.DataReceived += Controller_DataReceived;
                controller.DataSent += Controller_DataSent;
                controller.AddPeer(senderMacAddress, 0, true, localMasterKey);
                Debug.WriteLine("Receiver ready");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
            
            Thread.Sleep(Timeout.Infinite);
        }

        private static void Controller_DataReceived(object sender, DataReceivedEventArgs e)
        {
            string text = Encoding.UTF8.GetString(e.Data, 0, e.DataLen);

            Debug.WriteLine(
                "RX from " +
                BitConverter.ToString(e.PeerMac) +
                " len=" + e.DataLen +
                " data=" + text);
        }

        private static void Controller_DataSent(object sender, DataSentEventArgs e)
        {
            Debug.WriteLine(
                "TX status to " +
                BitConverter.ToString(e.PeerMac) +
                ": " + e.Status);
        }
    }
}
