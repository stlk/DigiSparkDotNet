using DigiSparkDotNet;
using System;
using System.Text;
using System.Windows.Forms;

namespace DigiSpark
{
    class Program
    {
        static void Main(string[] args)
        {
            var digiSpark = new ArduinoUsbDevice();
            digiSpark.ArduinoUsbDeviceChangeNotifier += digiSpark_ArduinoUsbDeviceChangeNotifier;

            while (true)
            {
                if (Console.KeyAvailable) // If the key was pressed
                {
                    var data = (byte)(Console.ReadKey().Key == ConsoleKey.O ? 1 : 0); // If it was O send 1 else 0
                    digiSpark.WriteBytes(new[] { data });
                }

                byte[] value;
                while (digiSpark.ReadByte(out value))
                {
                    Console.Write(Encoding.Default.GetString(value));
                }

                Application.DoEvents(); // Gather USB events
            }
        }

        static void digiSpark_ArduinoUsbDeviceChangeNotifier(object sender, EventArgs e)
        {
            Console.WriteLine("Device status changed: {0}", sender);
        }
    }
}
