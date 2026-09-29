[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_nanoFramework.EspNow&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_nanoFramework.EspNow) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_nanoFramework.EspNow&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_nanoFramework.EspNow) [![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.md) [![NuGet](https://img.shields.io/nuget/dt/nanoFramework.EspNow.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.EspNow/) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://raw.githubusercontent.com/nanoframework/Home/main/resources/logo/nanoFramework-repo-logo.png)

-----

### Welcome to the .NET **nanoFramework** ESP-NOW Class Library repository

## Build status

| Component | Build Status | NuGet Package |
|:-|---|---|
| nanoFramework.EspNow | [![Build Status](https://dev.azure.com/nanoframework/nanoFramework.EspNow/_apis/build/status%2Fnanoframework.EspNow?branchName=main)](https://dev.azure.com/nanoframework/nanoFramework.EspNow/_build/latest?definitionId=129&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.EspNow.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.EspNow/)  |

## Feedback and documentation

For documentation, providing feedback, issues and finding out how to contribute please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).

## Credits

The list of contributors to this project can be found at [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).

## License

The **nanoFramework** Class Libraries are licensed under the [MIT license](LICENSE.md).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behaviour in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).


## Samples

Minimal ESP-NOW sender and receiver samples are included below.

### Sender

```csharp
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using nanoFramework.EspNow;

namespace nanoFramework.EspNow.Sender
{
    public class Program
    {
        private static readonly byte[] TargetMac = new byte[] { 0x24, 0x6F, 0x28, 0x11, 0x22, 0x33 };

        public static void Main()
        {
            const byte Channel = 0;

            using (var controller = new EspNowController())
            {
                controller.AddPeer(TargetMac, Channel, false, null);

                var counter = 0;
                while (true)
                {
                    var payload = Encoding.UTF8.GetBytes("ping " + counter++);
                    controller.Send(TargetMac, payload, payload.Length);
                    Debug.WriteLine("sent");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
```

### Receiver

```csharp
using System.Diagnostics;
using System.Text;
using System.Threading;
using nanoFramework.EspNow;

namespace nanoFramework.EspNow.Receiver
{
    public class Program
    {
        private static readonly byte[] SenderMac = new byte[] { 0x24, 0x6F, 0x28, 0x44, 0x55, 0x66 };

        public static void Main()
        {
            const byte Channel = 0;

            using (var controller = new EspNowController())
            {
                controller.AddPeer(SenderMac, Channel, false, null);
                controller.DataReceived += (s, e) =>
                {
                    Debug.WriteLine(
                        "rx " +
                        BitConverter.ToString(e.PeerMac) +
                        " len=" + e.DataLen +
                        " data=" + Encoding.UTF8.GetString(e.Data, 0, e.DataLen));
                };

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
```

## Repository

- Package: `nanoFramework.EspNow`
- Native target support: ESP32 images with ESP-NOW enabled
