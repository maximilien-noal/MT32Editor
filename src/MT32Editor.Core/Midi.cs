using NAudio.Midi;
namespace MT32Edit;

/// <summary>
/// Simple tools to interface with NAudio MIDI library.
/// </summary>

internal static class Midi
{
    // MT32Edit: Midi class (static)
    // S.Fryers Feb 2025

    public const string MUNT_DEVICE_NAME = "MT-32 Synth Emulator";

    private static int OutDeviceIndex = 0;
    private static int InDeviceIndex = 0;
    private static MidiIn? In;
    private static MidiOut? Out;

    /// <summary>
    /// Displays an error message if invalid MIDI In data is received.
    /// </summary>
    private static void InputErrorReceived(object? sender, MidiInMessageEventArgs e)
    {
        PlatformServices.Notification.ShowMessage(string.Format("MIDI Input error: Message 0x{0:X8} Event {1}", e.RawMessage, e.MidiEvent));
    }

    /// <summary>
    /// Returns the name of the MIDI In device specified by deviceNo.
    /// </summary>
    public static string GetInputDeviceName(int deviceNo)
    {
        if (deviceNo <=0 || deviceNo > MidiIn.NumberOfDevices)
        {
            return MT32Strings.NONE;
        }
        string deviceName = MidiIn.DeviceInfo(deviceNo - 1).ProductName;
        return deviceName;
    }

    /// <summary>
    /// Returns the name of the MIDI Out device specified by deviceNo.
    /// </summary>
    public static string GetOutputDeviceName(int deviceNo)
    {
        if (deviceNo <=0 || deviceNo > MidiOut.NumberOfDevices)
        {
            return MT32Strings.NONE;
        }
        string deviceName = MidiOut.DeviceInfo(deviceNo - 1).ProductName;
        return deviceName;
    }

    /// <summary>
    /// Returns the name of the open MIDI In device.
    /// </summary>
    public static string GetCurrentInputDeviceName()
    {
        return GetInputDeviceName(InDeviceIndex);
    }

    /// <summary>
    /// Returns the name of the open MIDI Out device.
    /// </summary>
    public static string GetCurrentOutputDeviceName()
    {
        return GetOutputDeviceName(OutDeviceIndex);
    }

    /// <summary>
    /// Returns the number of identified MIDI In devices on the current system.
    /// </summary>
    public static int CountInputDevices()
    {
        return MidiIn.NumberOfDevices;
    }

    /// <summary>
    /// Returns the number of identified MIDI In devices on the current system.
    /// </summary>
    public static int CountOutputDevices()
    {
        return MidiOut.NumberOfDevices;
    }

    /// <summary>
    /// Forwards any MIDI In data to the active MIDI Out port, unless a SysEx upload is in progress.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void InputMessageReceived(object? sender, MidiInMessageEventArgs e)

    //echo any note data received from MIDI In port thru to MIDI Out port
    {
        if (MT32SysEx.uploadInProgress)
        {
            return; // only echo data if a sysEx upload is not in progress
        }

        if (OutDeviceIndex > 0 && Out is not null)
        {
            try
            {
                Out.Send(e.RawMessage); //send MIDI In data to MIDI Out
            }
            catch (Exception)
            {
                ShowMidiOutErrorMessage();
                Out = null;
            }
        }
    }

    /// <summary>
    /// Opens the specified MIDI In device
    /// </summary>
    /// <returns>True if successful, false if unsuccessful</returns>
    public static bool OpenInputDevice(int device)
    {
        if (InDeviceIndex > 0 && In is not null) 
        {
            try
            {
                //close any existing MIDI In connection
                In.Stop();
                In.Dispose();
            }
            catch (Exception)
            {
                ConsoleMessage.SendLine("MIDI In device error.");
                return false;
            }
        }
        InDeviceIndex = device;
        if (device <= 0)
        {
            ConsoleMessage.SendLine($"No MIDI In device connected.");
            return true;
        }
        try
        {
            In = new MidiIn(InDeviceIndex - 1); //open new MIDI In connection
            In.MessageReceived += InputMessageReceived;
            In.ErrorReceived += InputErrorReceived;
            In.Start(); //MIDI handler will start and continue running in background
            ConsoleMessage.SendLine($"MIDI In device connected: {GetInputDeviceName(device)}");
            return true;
        }
        catch (Exception) 
        {
            ConsoleMessage.SendLine($"Cannot open MIDI In device: {GetInputDeviceName(device)}");
            return false; 
        }
    }

    /// <summary>
    /// Opens the specified MIDI Out device
    /// </summary>
    /// <returns>True if successful, false if unsuccessful</returns>
    public static bool OpenOutputDevice(int device)
    {
        if (OutDeviceIndex > 0 && Out is not null)
        {
            if (In is not null && InDeviceIndex > 0)
            {
                In.Stop(); //pause existing MIDI In connection
            }  
            Out.Dispose(); //close any existing MIDI Out connection
        }
        OutDeviceIndex = device;
        if (device <= 0)
        {
            ConsoleMessage.SendLine($"No MIDI Out device connected.");
            return true;
        }
        try                //test for errors
        {
            Out = new MidiOut(OutDeviceIndex - 1);
            ConsoleMessage.SendLine($"MIDI Out device connected: {GetOutputDeviceName(device)}");
            if (InDeviceIndex > 0 && In is not null)
            {
                In.Start(); //restart existing MIDI In connection
            }
            return true;
        }
        catch (Exception)
        {
            ConsoleMessage.SendLine($"Cannot open MIDI Out device: {GetOutputDeviceName(device)}");
            return false; 
        }
    }

    /// <summary>
    /// Lists all available MIDI In devices on the current system.
    /// </summary>
    /// <returns>An array of strings containing device names.</returns>
    public static string[] ListInputDevices() 
    {
        var deviceList = new List<string> { MT32Strings.NONE };
        for (int device = 0; device < MidiIn.NumberOfDevices; device++)
        {
            deviceList.Add(MidiIn.DeviceInfo(device).ProductName);
        }
        return deviceList.ToArray();
    }

    /// <summary>
    /// Lists all available MIDI Out devices on the current system.
    /// </summary>
    /// <returns>An array of strings containing device names.</returns>
    public static string[] ListOutputDevices() //List available MIDI Out devices
    {
        var deviceList = new List<string> { MT32Strings.NONE };
        for (int device = 0; device < MidiOut.NumberOfDevices; device++)
        {
            deviceList.Add(MidiOut.DeviceInfo(device).ProductName);
        }
        return deviceList.ToArray();
    }

    /// <summary>
    /// Returns true if the current MIDI Out device is a MUNT MT-32 Emulator.
    /// </summary>
    public static bool EmulatorPresent(int deviceIndex)
    {
        return Out is not null && GetOutputDeviceName(deviceIndex) == MUNT_DEVICE_NAME;
    }

    /// <summary>
    /// Closes the connection to the active MIDI Out device.
    /// </summary>
    public static void CloseOutputDevice()
    {
        if (OutDeviceIndex > 0 && Out is not null)
        {
            try
            {
                Out.Dispose();
            }
            catch (Exception)
            {
                ConsoleMessage.SendLine("MIDI Out device already closed.");
                Out = null;
            }
        }
    }

    /// <summary>
    /// Closes the connection to the active MIDI In device.
    /// </summary>
    public static void CloseInputDevice()
    {
        if (InDeviceIndex > 0 && In is not null)
        {
            try
            {
                In.Stop();
                In.Dispose();
                In = null;
            }
            catch (Exception)
            {
                ConsoleMessage.SendLine("MIDI In device already closed.");
                In = null;
            }
        }
    }

    /// <summary>
    /// Sends a MIDI 'note on' message.
    /// </summary>
    /// <param name="note"></param>
    /// <param name="midiChannel"></param>

    public static void NoteOn(int note, int midiChannel, int volume = 100)
    {
        if (midiChannel == 16)
        {
            return; //Part is disabled
        }
        LogicTools.ValidateRange("Midi Channel", midiChannel, 0, 15, autoCorrect: false);
        try
        {
            if (Out is not null)
            {
                Out.Send(MidiMessage.StartNote(note, volume, midiChannel + 1).RawData);
            }
        }
        catch (Exception)
        {
            ShowMidiOutErrorMessage();
            Out = null;
        }
    }

    /// <summary>
    /// Sends a MIDI 'note off' message.
    /// </summary>
    /// <param name="note"></param>
    /// <param name="midiChannel"></param>
    public static void NoteOff(int note, int midiChannel, int volume = 100)
    {
        if (midiChannel == 16)
        {
            return; //Part is disabled
        }
        LogicTools.ValidateRange("Midi Channel", midiChannel, 0, 15, autoCorrect: false);
        try
        {
            if (Out is not null)
            {
                Out.Send(MidiMessage.StopNote(note, volume, midiChannel + 1).RawData);
            }
        }
        catch (Exception)
        {
            ShowMidiOutErrorMessage();
            Out = null;
        }
    }

    /// <summary>
    /// Sends a MIDI program change message.
    /// </summary>
    /// <param name="patchNo"></param>
    /// <param name="midiChannel"></param>
    public static void SendProgramChange(int patchNo, int channelNo)
    {
        //program change
        byte status = (byte)(0xC0 + channelNo);
        if (patchNo < 0 || patchNo > 127)
        {
            return;
        }

        byte programNo = (byte)patchNo;
        byte[] message = { status, programNo };
        try
        {
            if (Out is not null)
            {
                Out.SendBuffer(message);
            }
        }
        catch (Exception)
        {
            ShowMidiOutErrorMessage();
            Out = null;
        }
    }

    /// <summary>
    /// Sends an array of bytes as a MIDI SysEx message.
    /// </summary>
    /// <param name="sysExMessage"></param>
    public static void SendSysExMessage(byte[] sysExMessage)
    {
        try
        {
            if (Out is not null)
            {
                Out.SendBuffer(sysExMessage);
            }
        }
        catch (Exception)
        {
            ShowMidiOutErrorMessage();
            Out = null;
        }
    }

    private static void ShowMidiOutErrorMessage()
    {
        ConsoleMessage.SendLine("Error: Cannot open selected MIDI Out device. Please close any conflicting MIDI applications and restart MT-32 Editor.");
        PlatformServices.Notification.ShowMessage($"Error: Cannot open selected MIDI Out device.{Environment.NewLine}Please close any conflicting MIDI applications and restart MT-32 Editor.", "MT32 Editor");
    }
}
