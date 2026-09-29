using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using nanoFramework.EspNow;

namespace nanoFramework.EspNow.Sender
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

            Debug.WriteLine("ESP-NOW sender starting...");

            try
            {
                var reciverMacAddress = new byte[] { 0x84, 0xFC, 0xE6, 0x65, 0xA5, 0x10 };
                var controller = new EspNowController();
                controller.DataSent += Controller_DataSent;
                controller.AddPeer(reciverMacAddress, 0, true, localMasterKey);

                int counter = 0;
                while (true)
                {
                    try
                    {
                        string message = "ping " + counter++;
                        byte[] payload = Encoding.UTF8.GetBytes(message);

                        controller.Send(reciverMacAddress, payload, payload.Length);
                        Debug.WriteLine("Sent: " + message);
                    }
                    catch (Exception sendEx)
                    {
                        Debug.WriteLine("Send error: " + sendEx.Message);
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Controller_DataSent(object sender, DataSentEventArgs e)
        {
            Debug.WriteLine("DataSent status: " + e.Status);
        }
    }
}
