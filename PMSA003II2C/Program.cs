//---------------------------------------------------------------------------------
// Copyright (c) June 2023, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// nanoff --update --platform ESP32 --serialport com15
//
// nanoff --update --target FEATHER_S2 --serialport com14
//
//---------------------------------------------------------------------------------
#define ADAFRUIT_FEATHER_S2
//#define SPARKFUN_ESP32_THING_PLUS
namespace devMobile.IoT.PMSA003I
{
    using System;
    using System.Device.I2c;
    using System.Threading;

#if ADAFRUIT_FEATHER_S2 || SPARKFUN_ESP32_THING_PLUS
    using nanoFramework.Hardware.Esp32;
#endif

    public class Program
    {
        public static void Main()
        {
#if SPARKFUN_ESP32_THING_PLUS
            Configuration.SetPinFunction(Gpio.IO23, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);
#endif
#if ADAFRUIT_FEATHER_S2
            Configuration.SetPinFunction(Gpio.IO08, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(Gpio.IO09, DeviceFunction.I2C1_CLOCK);
#endif
            Thread.Sleep(1000);

            I2cConnectionSettings i2cConnectionSettings = new(1, 0x12, I2cBusSpeed.StandardMode);


            using (I2cDevice i2cDevice = I2cDevice.Create(i2cConnectionSettings))
            {
                {
                    SpanByte writeBuffer = new byte[1];
                    SpanByte readBuffer = new byte[1];

                    writeBuffer[0] = 0x0;

                    i2cDevice.WriteRead(writeBuffer, readBuffer);

                    Console.WriteLine($"0x0 {readBuffer[0]:X2}");
                }

                while (true)
                {
                    SpanByte writeBuffer = new byte[1];
                    SpanByte readBuffer = new byte[32];

                    writeBuffer[0] = 0x0;

                    i2cDevice.WriteRead(writeBuffer, readBuffer);

                    //Console.WriteLine(System.BitConverter.ToString(readBuffer.ToArray()));
                    Console.WriteLine($"Length:{ReadInt16BigEndian(readBuffer.Slice(0x2, 2))}");

                    if ((readBuffer[0] == 0x42) || (readBuffer[1] == 0x4d))
                    {
                        Console.WriteLine($"PM    1.0:{ReadInt16BigEndian(readBuffer.Slice(0x4, 2))}, 2.5:{ReadInt16BigEndian(readBuffer.Slice(0x6, 2))}, 10.0:{ReadInt16BigEndian(readBuffer.Slice(0x8, 2))} std");
                        Console.WriteLine($"PM    1.0:{ReadInt16BigEndian(readBuffer.Slice(0x0A, 2))}, 2.5:{ReadInt16BigEndian(readBuffer.Slice(0x0C, 2))}, 10.0:{ReadInt16BigEndian(readBuffer.Slice(0x0E, 2))} env");
                        Console.WriteLine($"µg/m3 0.3:{ReadInt16BigEndian(readBuffer.Slice(0x10, 2))}, 0.5:{ReadInt16BigEndian(readBuffer.Slice(0x12, 2))}, 1.0:{ReadInt16BigEndian(readBuffer.Slice(0x14, 2))}, 2.5:{ReadInt16BigEndian(readBuffer.Slice(0x16, 2))}, 5.0:{ReadInt16BigEndian(readBuffer.Slice(0x18, 2))}, 10.0:{ReadInt16BigEndian(readBuffer.Slice(0x1A, 2))}");

                        // Don't need to display these values everytime
                        //Console.WriteLine($"Version:{readBuffer[0x1c]}");
                        //Console.WriteLine($"Error:{readBuffer[0x1d]}");
                    }
                    else
                    {
                        Console.WriteLine(".");
                    }

                    Thread.Sleep(5000);
                }
            }
        }

        private static ushort ReadInt16BigEndian(SpanByte source)
        {
            if (source.Length != 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            ushort result = (ushort)(source[0] << 8);

            return result |= source[1];
        }
    }
}

