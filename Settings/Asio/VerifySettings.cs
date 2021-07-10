using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Windows.Forms;

namespace RSDiagnostics.Settings.Asio
{
    /// <summary>
    /// Verify the user's RS_ASIO Settings
    /// </summary>
    public class VerifySettings
    {
        /// <summary>
        /// Run verification steps on the user's RS_ASIO Settings.
        /// </summary>
        public VerifySettings()
        {
            VerifyDevicesAreActualASIODevices();
            FixFocusriteBuffer();
            CheckWasapiState();
        }

        /// <summary>
        /// If the user is using a Focusrite device, make sure they have the proper buffer size set. 48 | 96 | 196 | 256 | any multiple of 48 as explained in the RS_ASIO documentation.
        /// </summary>
        void FixFocusriteBuffer()
        {
            List<Settings> devices = new List<Settings>() { // A list of all the devices set in the user's RS_ASIO settings file.
                Settings.Where("Asio.Output", "Driver").Count > 0 ? Settings.Where("Asio.Output", "Driver").First() : null,
                Settings.Where("Asio.Input.0", "Driver").Count > 0 ? Settings.Where("Asio.Input.0", "Driver").First() : null,
                Settings.Where("Asio.Input.1", "Driver").Count > 0 ? Settings.Where("Asio.Input.1", "Driver").First() : null,
                Settings.Where("Asio.Input.Mic", "Driver").Count > 0 ? Settings.Where("Asio.Input.Mic", "Driver").First() : null
            };

            Settings bufferSizeMode = Settings.Where("Asio", "BufferSizeMode").Count > 0 ? Settings.Where("Asio", "BufferSizeMode").First() : null; // BufferSizeMode. Options are "driver", "host", and "custom".
            Settings customBufferSize = Settings.Where("Asio", "CustomBufferSize").Count > 0 ? Settings.Where("Asio", "CustomBufferSize").First() : null;

            // Check if buffer size and custom buffer size settings even exist. If these don't exist, then we can't fix the buffer.
            if (bufferSizeMode == null || customBufferSize == null)
                return;

            foreach (Settings device in devices)
            {
                if (device == null || !device.Value.ToString().ToLower().Contains("focusrite")) // Make sure there is even a device for us to test, and see if it includes "focusrite" in it's driver name.
                    continue;

                // See if the buffer issue is a potential problem.
                if (bufferSizeMode.Value.ToString().ToLower() == "custom" && ((int)customBufferSize.Value % 48) != 0)
                {
                    if (MessageBox.Show("We detected that your RS_ASIO settings could have the wrong buffer size.\n" +
                                        "This is because you are using a Focusrite device.\n" +
                                        "Some Focusrite devices have been reported to only output sound properly when using ASIO buffer sizes of 48, 96 or 192.\n" +
                                        "Do you want us to try one of these settings?",
                                        "Possible RS_ASIO Error",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Warning)
                        == DialogResult.Yes)
                    {
                        customBufferSize.Value = 48;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Verify that all the devices in the RS_ASIO settings file are actually ASIO devices.
        /// </summary>
        void VerifyDevicesAreActualASIODevices()
        {
            // Load Settings
            Dictionary<string, string> devices = new Dictionary<string, string>();
            Settings outputDriver = Settings.Where("Asio.Output", "Driver").Count > 0 ? Settings.Where("Asio.Output", "Driver").First() : null;
            Settings input0Driver = Settings.Where("Asio.Input.0", "Driver").Count > 0 ? Settings.Where("Asio.Input.0", "Driver").First() : null;
            Settings input1Driver = Settings.Where("Asio.Input.1", "Driver").Count > 0 ? Settings.Where("Asio.Input.1", "Driver").First() : null;
            Settings inputMicDriver = Settings.Where("Asio.Input.Mic", "Driver").Count > 0 ? Settings.Where("Asio.Input.Mic", "Driver").First() : null;

            // Verify that the settings exist AND we aren't placing blank devices into the list.
            if (outputDriver != null && outputDriver.Value.ToString() != string.Empty)
                devices.Add("output", outputDriver.Value.ToString());
            if (input0Driver != null && input0Driver.Value.ToString() != string.Empty)
                devices.Add("input0", input0Driver.Value.ToString());
            if (input1Driver != null && input1Driver.Value.ToString() != string.Empty)
                devices.Add("input1", input1Driver.Value.ToString());
            if (inputMicDriver != null && inputMicDriver.Value.ToString() != string.Empty)
                devices.Add("inputMic", inputMicDriver.Value.ToString());

            List<Devices.DriverInfo> asioDevices = Devices.FindDevices(); // A list of all ASIO devices 

            // Verify the ASIO devices actually exist.
            foreach(KeyValuePair<string, string> device in devices)
            {
                bool foundDevice = false;

                if (device.Value.ToLower().Contains("asio4all")) // If the user is using ASIO4All, warn them about it.
                    WarnAboutASIO4All();

                foreach (Devices.DriverInfo asioDevice in asioDevices) // Run through all the ASIO devices to see if the user's RS_ASIO output / input settings are actually pointed to an ASIO interface. 
                {
                    if (asioDevice.deviceName == device.Value)
                    {
                        foundDevice = true;
                        break;
                    }
                }

                // The user's RS_ASIO settings file says that a device supports ASIO, but it doesn't.
                if (!foundDevice)
                    MessageBox.Show("We couldn't find the ASIO " + device.Key + " device you have set in RS_ASIO! " +
                                    "Please double check your drivers to make sure you are using the correct ASIO device!",
                                    "ASIO Device Not Found!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Verify the user's WASAPI state is set correctly (True / on = Use headphones connected to PC; False / off = Use Headphones through interface).
        /// </summary>
        void CheckWasapiState()
        {
            Settings wasapiOuputSetting = Settings.Where("Config", "EnableWasapiOutputs").Count > 0 ? Settings.Where("Config", "EnableWasapiOutputs").First() : null;

            if (MessageBox.Show("How are your headphones / speakers connected to your computer?\n" +
                                "Press \"Yes\" if they are directly into your computer (via AUX, USB, etc)\n" +
                                "Press \"No\" if they are connected to your audio interface.",
                                "RS_ASIO Output Question",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question)
                == DialogResult.Yes)
            {
                // DialogResult.Yes | Uses Headphones connected to PC
                if (wasapiOuputSetting != null && (int)wasapiOuputSetting.Value != 1) // Make sure the setting exists, and make sure we don't write the same value over itself.
                    wasapiOuputSetting.Value = 1;
            }
            else // DialogResult.No | Uses Headphones connected to interface
            {
                if (wasapiOuputSetting != null && (int)wasapiOuputSetting.Value != 0) // Make sure the setting exists, and make sure we don't write the same value over itself.
                    wasapiOuputSetting.Value = 0;
            }
        }

        /// <summary>
        /// ASIO4All is not a good software to be using and can cause issues with RS_ASIO.
        /// </summary>
        void WarnAboutASIO4All()
        {
            ManagementObjectCollection wasapiDevices = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice").Get(); // Gets a list of WASAPI devices

            Dictionary<string, string> potentialAsio4AllDevices = new Dictionary<string, string>() { { "BEHRINGER", "https://www.youtube.com/watch?v=S3QHbhtknH8" } }; // List of blacklisted devices

            foreach (ManagementObject device in wasapiDevices)
            {
                foreach(string potentialAsio4AllDevice in potentialAsio4AllDevices.Keys)
                {
                    if (potentialAsio4AllDevice.Contains(device.Properties["Name"].Value.ToString()))
                    {
                        if (MessageBox.Show("We detected you are using ASIO4All.\n" +
                                            "We suspect you are using it on your " + potentialAsio4AllDevice + " device. \n" +
                                            "There is a better way of utilizing this device in Rocksmith2014 that doesn't require ASIO4All.\n" +
                                            "Press \"OK\" if you want to try the other way, or press \"Cancel\" if you want to continue using ASIO4All.",
                                            "ASIO4All Conflict",
                                            MessageBoxButtons.OKCancel,
                                            MessageBoxIcon.Information)
                            == DialogResult.OK)
                        {
                            Process.Start(potentialAsio4AllDevices[potentialAsio4AllDevice]);
                        }
                        break;
                    }
                }
            }
        }
    }
}
