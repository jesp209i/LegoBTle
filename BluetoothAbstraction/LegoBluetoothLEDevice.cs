using System;
using System.Collections.Generic;
using System.Text;

namespace LegoBTle.Bluetooth
{
    public class LegoBluetoothLEDevice
    {
        public DateTimeOffset BroadcastTime { get; }
        public ulong Address { get; }
        public string Name { get; }
        public short SignalStrengthInDB { get; }
        public LegoBluetoothLEDevice(ulong address, string name, short rssi, DateTimeOffset broadcastTime)
        {
            Address = address;
            Name = name;
            SignalStrengthInDB = rssi;
            BroadcastTime = broadcastTime;
        }
        public override string ToString()
        {
            return $"{ (string.IsNullOrEmpty(Name) ? "[No Name]" : Name ) } {Address} ({SignalStrengthInDB})";
        }
    }
}
