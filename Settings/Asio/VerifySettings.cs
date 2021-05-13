﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Windows.Forms;

namespace RSDiagnostics.Settings.Asio
{
    public class VerifySettings
    {
        public VerifySettings()
        {
            VerifyDevicesAreActualASIODevices();
            FixFocusriteBuffer();
            CheckWasapiState();
        }

        void FixFocusriteBuffer()
        {
            List<Settings> devices = new List<Settings>() {
                Settings.Where("Asio.Output", "Driver").Count > 0 ? Settings.Where("Asio.Output", "Driver").First() : null,
                Settings.Where("Asio.Input.0", "Driver").Count > 0 ? Settings.Where("Asio.Input.0", "Driver").First() : null,
                Settings.Where("Asio.Input.1", "Driver").Count > 0 ? Settings.Where("Asio.Input.1", "Driver").First() : null,
                Settings.Where("Asio.Input.Mic", "Driver").Count > 0 ? Settings.Where("Asio.Input.Mic", "Driver").First() : null
            };

            foreach (Settings driver in devices)
            {
                // Make sure there is even a device for us to test, and see if it includes "focusrite" in it's driver name.
                if (driver == null || !driver.Value.ToString().ToLower().Contains("focusrite"))
                    continue;

                Settings bufferSizeMode = Settings.Where("Asio", "BufferSizeMode").Count > 0 ? Settings.Where("Asio", "BufferSizeMode").First() : null;
                Settings customBufferSize = Settings.Where("Asio", "CustomBufferSize").Count > 0 ? Settings.Where("Asio", "CustomBufferSize").First() : null;

                // Check if buffer size and custom buffer size settings even exist. If these don't exist, then we can't fix the buffer.
                if (bufferSizeMode == null || customBufferSize == null)
                    return;

                // See if the buffer issue is a potential problem.
                if (bufferSizeMode.Value.ToString().ToLower() == "custom" && ((int)customBufferSize.Value % 48) != 0)
                {
                    if (MessageBox.Show("We detected that your RS_ASIO settings could have the wrong buffer size.\n" +
                                        "This is because you are using a focusrite device.\n" +
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

        void VerifyDevicesAreActualASIODevices()
        {
            // Load Settings
            Dictionary<string, string> drivers = new Dictionary<string, string>();
            Settings outputDriver = Settings.Where("Asio.Output", "Driver").Count > 0 ? Settings.Where("Asio.Output", "Driver").First() : null;
            Settings input0Driver = Settings.Where("Asio.Input.0", "Driver").Count > 0 ? Settings.Where("Asio.Input.0", "Driver").First() : null;
            Settings input1Driver = Settings.Where("Asio.Input.1", "Driver").Count > 0 ? Settings.Where("Asio.Input.1", "Driver").First() : null;
            Settings inputMicDriver = Settings.Where("Asio.Input.Mic", "Driver").Count > 0 ? Settings.Where("Asio.Input.Mic", "Driver").First() : null;

            // Verify that the settings exist AND we aren't placing blank devices into the list.
            if (outputDriver != null && outputDriver.Value.ToString() != string.Empty)
                drivers.Add("output", outputDriver.Value.ToString());
            if (input0Driver != null && input0Driver.Value.ToString() != string.Empty)
                drivers.Add("input0", input0Driver.Value.ToString());
            if (input1Driver != null && input1Driver.Value.ToString() != string.Empty)
                drivers.Add("input1", input1Driver.Value.ToString());
            if (inputMicDriver != null && inputMicDriver.Value.ToString() != string.Empty)
                drivers.Add("inputMic", inputMicDriver.Value.ToString());

            List<Devices.DriverInfo> asioDevices = Devices.FindDevices();

            // Verify the ASIO devices actually exist.
            foreach(KeyValuePair<string, string> device in drivers)
            {
                bool foundDevice = false;

                if (device.Value.ToLower().Contains("asio4all"))
                    WarnAboutASIO4All();

                foreach (Devices.DriverInfo driver in asioDevices)
                {
                    if (driver.deviceName == device.Value)
                    {
                        foundDevice = true;
                        break;
                    }
                }

                // Yeah, this ASIO device doesn't exist.
                if (!foundDevice)
                    MessageBox.Show("We couldn't find the ASIO " + device.Key + " device you have set in RS_ASIO! " +
                                    "Please double check your drivers to make sure you are using the correct ASIO device!",
                                    "ASIO Device Not Found!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
            }
        }

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
                if (wasapiOuputSetting != null && (int)wasapiOuputSetting.Value != 1)
                    wasapiOuputSetting.Value = 1;
            }
            else // DialogResult.No | Uses Interface
            {
                if (wasapiOuputSetting != null && (int)wasapiOuputSetting.Value != 0)
                    wasapiOuputSetting.Value = 0;
            }
        }

        void WarnAboutASIO4All()
        {
            ManagementObjectCollection devices = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice").Get();

            Dictionary<string, string> potentialAsio4AllDevices = new Dictionary<string, string>() { { "BEHRINGER USB AUDIO", "https://www.youtube.com/watch?v=S3QHbhtknH8" } };

            foreach (ManagementObject device in devices)
            {
                foreach(string potentialAsio4AllDevice in potentialAsio4AllDevices.Keys)
                {
                    if (potentialAsio4AllDevice == device.Properties["Name"].Value.ToString())
                    {
                        if (MessageBox.Show("We detected you are using ASIO4All.\n" +
                                            "We suspect you are using it on your " + potentialAsio4AllDevice + "\n" +
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
