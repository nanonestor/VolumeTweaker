using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private const byte VK_VOLUME_UP = 0xAF;
    private const byte VK_VOLUME_DOWN = 0xAE;
    private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    enum EDataFlow
    {
        eRender = 0,
        eCapture = 1,
        eAll = 2
    }

    enum ERole
    {
        eConsole = 0,
        eMultimedia = 1,
        eCommunications = 2
    }

    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumerator { }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        [PreserveSig]
        int EnumAudioEndpoints(int dataFlow, int dwStateMask, out IntPtr ppDevices);

        [PreserveSig]
        int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppDevice);

        [PreserveSig]
        int GetDevice(string pwstrId, out IMMDevice ppDevice);

        [PreserveSig]
        int RegisterEndpointNotificationCallback(IntPtr pClient);

        [PreserveSig]
        int UnregisterEndpointNotificationCallback(IntPtr pClient);
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
    }

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        int RegisterControlChangeNotify(IntPtr pNotify);
        int UnregisterControlChangeNotify(IntPtr pNotify);
        int GetChannelCount(out uint pnChannelCount);
        int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
        int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
        int GetMasterVolumeLevel(out float pfLevelDB);
        int GetMasterVolumeLevelScalar(out float pfLevel);
        int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);
        int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);
        int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
        int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
        int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, ref Guid pguidEventContext);
        int GetMute(out bool pbMute);
        int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
        int VolumeStepUp(ref Guid pguidEventContext);
        int VolumeStepDown(ref Guid pguidEventContext);
        int QueryHardwareSupport(out uint pdwHardwareSupportMask);
        int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
    }

    static void Main(string[] args)
    {
        IMMDeviceEnumerator deviceEnumerator = null;
        IMMDevice device = null;
        IAudioEndpointVolume endpointVolume = null;

        try
        {
            if (args.Length != 1 || !float.TryParse(args[0], out float volumeChangePercent))
            {
                return;
            }

            // Clamp the percent change range to -100 to 100
            volumeChangePercent = Math.Max(-100, Math.Min(100, volumeChangePercent));

            deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
            if (deviceEnumerator == null)
                return;

            int hr = deviceEnumerator.GetDefaultAudioEndpoint(
                (int)EDataFlow.eRender,
                (int)ERole.eMultimedia,
                out device);

            if (hr != 0 || device == null)
                return;

            Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
            object o;
            hr = device.Activate(ref IID_IAudioEndpointVolume, 1, IntPtr.Zero, out o);
            if (hr != 0 || o == null)
                return;

            endpointVolume = (IAudioEndpointVolume)o;

            float currentVolume;
            hr = endpointVolume.GetMasterVolumeLevelScalar(out currentVolume);
            if (hr != 0)
                return;

            // Calculate new volume: current + (percent / 100)
            float targetVolume = currentVolume + (volumeChangePercent / 100f);

            // Clamp to [0, 1]
            targetVolume = Math.Max(0f, Math.Min(1f, targetVolume));

            // SET THE NEW VOLUME
            Guid guid = Guid.Empty;


            // Simulate Volume Up key press and release - to trigger the Windows on-screen display of volume
            keybd_event(VK_VOLUME_DOWN, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
            keybd_event(VK_VOLUME_DOWN, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);

            // Sets the new adjusted valume
            hr = endpointVolume.SetMasterVolumeLevelScalar(targetVolume, ref guid);

            // Small delay to ensure OSD appears
            System.Threading.Thread.Sleep(20);

            // Simulate Volume Down key press and release to restore volume
            keybd_event(VK_VOLUME_UP, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
            keybd_event(VK_VOLUME_UP, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);

            System.Threading.Thread.Sleep(100);

            // Sets the new adjusted volume - AGAIN - to reset the even-numbed setting which the last key event set.
            // Windows needs make an API trigger to on-screen display, so that fake key events aren't needed!
            hr = endpointVolume.SetMasterVolumeLevelScalar(targetVolume, ref guid);
        }
        finally
        {
            if (endpointVolume != null)
                Marshal.ReleaseComObject(endpointVolume);
            if (device != null)
                Marshal.ReleaseComObject(device);
            if (deviceEnumerator != null)
                Marshal.ReleaseComObject(deviceEnumerator);
        }
    }
}