﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;

internal static class NativeMethods
{
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern Boolean ChangeServiceConfig(
        IntPtr hService,
        uint nServiceType,
        uint nStartType,
        uint nErrorControl,
        string? lpBinaryPathName,
        string? lpLoadOrderGroup,
        IntPtr lpdwTagId,
        [In] char[]? lpDependencies,
        string? lpServiceStartName,
        string? lpPassword,
        string? lpDisplayName);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr OpenService(IntPtr hScManager, string lpServiceName, uint dwDesiredAccess);

    [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr OpenSCManager(string? machineName, string? databaseName, uint dwAccess);

    [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
    public static extern int CloseServiceHandle(IntPtr hScObject);

    private const uint ServiceNoChange = 0xFFFFFFFF;
    private const uint ServiceQueryConfig = 0x00000001;
    private const uint ServiceChangeConfig = 0x00000002;
    private const uint ScManagerAllAccess = 0x000F003F;

    public static bool ChangeStartMode(ServiceController svc, ServiceStartMode mode, out int error)
    {
        error = 0;

        var scManagerHandle = OpenSCManager(null, null, ScManagerAllAccess);
        Debug.Assert(scManagerHandle != IntPtr.Zero);

        var serviceHandle = OpenService(scManagerHandle, svc.ServiceName, ServiceQueryConfig | ServiceChangeConfig);
        Debug.Assert(serviceHandle != IntPtr.Zero);

        bool result = ChangeServiceConfig(
            serviceHandle,
            ServiceNoChange,
            (uint)mode,
            ServiceNoChange,
            null,
            null,
            IntPtr.Zero,
            null,
            null,
            null,
            null);

        if (!result)
            error = Marshal.GetLastWin32Error();

        int hResult = CloseServiceHandle(serviceHandle);
        Debug.Assert(hResult == 0);
        hResult = CloseServiceHandle(scManagerHandle);
        Debug.Assert(hResult == 0);

        return result;
    }
}
