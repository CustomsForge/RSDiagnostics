using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Win32;

namespace RSDiagnostics.Settings.Asio
{
    public class Devices
    {
        /// <summary>
        /// Scans the user's registry for ASIO Devices
        /// </summary>
        /// <returns>A list of ASIO Devices</returns>
        public static List<DriverInfo> FindDevices()
        {
            List<DriverInfo> availableDevices = new List<DriverInfo>();

            try
            {
                RegistryKey registry_ASIO = Registry.LocalMachine.OpenSubKey("Software\\ASIO"); // Open registry to "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\ASIO".

                if (registry_ASIO == null) // If we don't find the "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\ASIO" registry key
                    return availableDevices;

                string[] subKeyNames = registry_ASIO.GetSubKeyNames(); // Get a list of all the devices / sub-keys in the "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\ASIO" registry key.

                foreach (string asioDevice in subKeyNames)
                {
                    // Setup variables to access registry values.
                    DriverInfo deviceInfo = new DriverInfo();
                    RegistryKey registry_device = Registry.LocalMachine.OpenSubKey($"Software\\ASIO\\{asioDevice}");

                    // Set device information from Software\ASIO
                    deviceInfo.clsID = (string)registry_device.GetValue("CLSID");
                    deviceInfo.deviceDescription = (string)registry_device.GetValue("Description");
                    deviceInfo.deviceName = asioDevice;

                    registry_device.Close();

                    // Verify we have a real device and not just a fake key
                    if (deviceInfo.clsID == null || deviceInfo.deviceDescription == null || deviceInfo.deviceName == null)
                        continue;

                    // Put device into list
                    availableDevices.Add(deviceInfo);
                }

                registry_ASIO.Close();
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show($"ASIO Error: {ex.Message}", "ASIO Error");
            }

            return availableDevices;
        }

        public struct DriverInfo
        {
            public string clsID;
            public string deviceName;
            public string deviceDescription;
        }
    }
}
