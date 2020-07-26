using LegoBTle.Bluetooth;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Bluetooth.Advertisement;

namespace BluetoothAbstraction
{
    /// <summary>
    /// Wraps and uses the <see cref="BluetoothLeAdvertisementWatcher" />
    /// </summary>
    public class LegoBTleBluetoothLEAdvertisementWatcher
    {
        #region Private Menbers

        /// <summary>
        /// The underlying bluetooth watcher class
        /// </summary>
        private readonly BluetoothLEAdvertisementWatcher mWatcher;

        /// <summary>
        /// A list of discovered devices
        /// </summary>
        private readonly Dictionary<ulong, LegoBluetoothLEDevice> mDiscoveredDevices = new Dictionary<ulong, LegoBluetoothLEDevice>();

        private object mThreadLock = new object();
        #endregion


        #region Public Properties
        /// <summary>
        /// Indicates if this watchers is listening for advertisements
        /// </summary>
        public bool Listening => mWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;

        public IReadOnlyCollection<LegoBluetoothLEDevice> DiscoveredDevices
        {
            get
            {
                CleanUpTimeOuts();

                lock (mThreadLock)
                {
                    return mDiscoveredDevices.Values.ToList().AsReadOnly();
                }
            }
        }
        /// <summary>
        /// The timeout in seconds that a device is removed from the <see cref="DiscoveredDevices"/>
        /// list if it is not re-advertised within this time
        /// </summary>
        public int HeartbeatTimeout { get; set; } = 30;
        #endregion

        #region Public Events

        public event Action StoppedListening = () => { };
        public event Action StartedListening = () => { };

        /// <summary>
        /// Fired when a device is discovered
        /// </summary>
        public event Action<LegoBluetoothLEDevice> DeviceAdvertisementHappened = (device) => { };
        /// <summary>
        /// Fired when a new device is discovered
        /// </summary>
        public event Action<LegoBluetoothLEDevice> NewDeviceDiscovered = (device) => { };
        /// <summary>
        /// Fired when device name is changed
        /// </summary>
        public event Action<LegoBluetoothLEDevice> DeviceNameChanged = (device) => { };        
        /// <summary>
        /// Fired when a device is removed for timing out
        /// </summary>
        public event Action<LegoBluetoothLEDevice> DeviceTimedOut = (device) => { };
        #endregion

        #region Constructor
        /// <summary>
        /// The default constructor
        /// Sets up the BluetoothAdvertisementWatcher - but doesn't start it.
        /// </summary>
        public LegoBTleBluetoothLEAdvertisementWatcher()
        {
            mWatcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };
            
            // listens for new advertisements
            mWatcher.Received += MWatcher_Received;

            // listens for when the watcher stops listening
            mWatcher.Stopped += (watcher, e) =>
            {
                StoppedListening();
            };
        }
        #endregion

        #region Private Methods
        private void MWatcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            // Cleanup Timeouts
            CleanUpTimeOuts();

            LegoBluetoothLEDevice device = null;

            // if new discovery?
            var newDiscovery = !mDiscoveredDevices.ContainsKey(args.BluetoothAddress);

            // name changed?
            var nameChanged = 
                !newDiscovery && 
                !string.IsNullOrEmpty(args.Advertisement.LocalName) &&
                mDiscoveredDevices[args.BluetoothAddress].Name != args.Advertisement.LocalName;

            lock (mThreadLock)
            {
                var name = args.Advertisement.LocalName;

                if (string.IsNullOrEmpty(name) && !newDiscovery)
                    name = mDiscoveredDevices[args.BluetoothAddress].Name;

                device = new LegoBluetoothLEDevice(
                    address: args.BluetoothAddress,
                    name: name,
                    rssi: args.RawSignalStrengthInDBm,
                    broadcastTime: args.Timestamp
                    );
                // Add/update the device in the dictionary
                mDiscoveredDevices[args.BluetoothAddress] = device;
            }
            
            DeviceAdvertisementHappened(device);

            // If name changed
            if (nameChanged)
                DeviceNameChanged(device);

            // If new device
            if (newDiscovery)
                NewDeviceDiscovered(device);
            
        }
        private void CleanUpTimeOuts()
        {
            lock (mThreadLock)
            {
                var threshold = DateTime.UtcNow - TimeSpan.FromSeconds(HeartbeatTimeout);

                mDiscoveredDevices.Where(f => f.Value.BroadcastTime < threshold).ToList().ForEach(device =>
                {
                    mDiscoveredDevices.Remove(device.Key);

                    DeviceTimedOut(device.Value);
                });
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the Watcher
        /// </summary>
        public void StartListening()
        {
            lock (mThreadLock)
            {
                // if already listening
                if (Listening)
                    return;

                mWatcher.Start();
            }
        }

        /// <summary>
        /// Stops the Watcher
        /// </summary>
        public void StopListening()
        {
            lock (mThreadLock) { 
                // if already listening
                if (!Listening)
                    return;

                mWatcher.Stop();
            
                mDiscoveredDevices.Clear();
            }
        }

        #endregion
    }
}
