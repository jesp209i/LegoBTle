using BluetoothAbstraction;
using System;

namespace LegoBTle.ConsolePlayground
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Hello World!");

            var watcher = new LegoBTleBluetoothLEAdvertisementWatcher();

            watcher.StartedListening += () => Console.WriteLine("Started Listening");
            watcher.StoppedListening += () => 
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Stopped Listening"); 
            };
            watcher.NewDeviceDiscovered += (device) => 
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"New Device discovered: {device}"); 
            };
            //watcher.DeviceAdvertisementHappened += (device) => 
            //{
            //    Console.ForegroundColor = ConsoleColor.White;
            //    Console.WriteLine($"Advertisement: {device}"); 
            //};
            watcher.DeviceNameChanged += (device) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Device name changed: {device}");
            };
            watcher.DeviceTimedOut += (device) => 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Device timed out: {device}"); 
            };
            watcher.StartListening();

            Console.ReadLine();
            watcher.StopListening();
            Console.ReadLine();

        }
    }
}
