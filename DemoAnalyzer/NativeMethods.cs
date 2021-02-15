using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace DemoAnalyzer
{
    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    internal class HeatmapSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Create a SafeHandle, informing the base class
        // that this SafeHandle instance "owns" the handle,
        // and therefore SafeHandle should call
        // our ReleaseHandle method when the SafeHandle
        // is no longer in use.
        private HeatmapSafeHandle()
            : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        override protected bool ReleaseHandle()
        {
            // Here, we must obey all rules for constrained execution regions.
            NativeMethods.FreeHeatmap(handle);
            return true;
            // If ReleaseHandle failed, it can be reported via the
            // "releaseHandleFailed" managed debugging assistant (MDA).  This
            // MDA is disabled by default, but can be enabled in a debugger
            // or during testing to diagnose handle corruption problems.
            // We do not throw an exception because most code could not recover
            // from the problem.
        }
    }

    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    internal class HeatmapStampSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Create a SafeHandle, informing the base class
        // that this SafeHandle instance "owns" the handle,
        // and therefore SafeHandle should call
        // our ReleaseHandle method when the SafeHandle
        // is no longer in use.
        private HeatmapStampSafeHandle()
            : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        override protected bool ReleaseHandle()
        {
            // Here, we must obey all rules for constrained execution regions.
            NativeMethods.FreeStamp(handle);
            return true;
            // If ReleaseHandle failed, it can be reported via the
            // "releaseHandleFailed" managed debugging assistant (MDA).  This
            // MDA is disabled by default, but can be enabled in a debugger
            // or during testing to diagnose handle corruption problems.
            // We do not throw an exception because most code could not recover
            // from the problem.
        }
    }

    [SuppressUnmanagedCodeSecurity()]
    internal static class NativeMethods
    {
        // Allocate a file object in the kernel, then return a handle to it.
        [DllImport("HeatmapDll.dll", ExactSpelling = true)]
        internal extern static HeatmapSafeHandle AllocHeatmap(uint w, uint h);

        // Allocate a file object in the kernel, then return a handle to it.
        [DllImport("HeatmapDll.dll", ExactSpelling = true)]
        internal extern static void AddPointToHeatmap(HeatmapSafeHandle h, uint x, uint y);

        // Allocate a file object in the kernel, then return a handle to it.
        [DllImport("HeatmapDll.dll", ExactSpelling = true)]
        internal extern static void AddPointToHeatmapWithStamp(HeatmapSafeHandle h, uint x, uint y, HeatmapStampSafeHandle stamp);

        // Allocate a file object in the kernel, then return a handle to it.
        [DllImport("HeatmapDll.dll", ExactSpelling = true)]
        internal extern static void WriteHeatmap(HeatmapSafeHandle h, byte[] buffer);

        // Free the kernel's file object (close the file).
        [DllImport("HeatmapDll.dll", ExactSpelling = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal extern static void FreeHeatmap(IntPtr handle);

        // Allocate a file object in the kernel, then return a handle to it.
        [DllImport("HeatmapDll.dll", ExactSpelling = true)]
        internal extern static HeatmapStampSafeHandle CreateStamp(uint r);

        // Free the kernel's file object (close the file).
        [DllImport("HeatmapDll.dll", ExactSpelling = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal extern static void FreeStamp(IntPtr handle);
    }
}
