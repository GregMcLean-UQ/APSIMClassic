﻿using System;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Runtime.InteropServices;
using CMPServices;

namespace CMPComp
{
    //=========================================================================
    /// <summary>
    /// 
    /// </summary>
    //=========================================================================
    [ComVisible(true)]
    internal class TComponentInstance : TAPSIMHost
    {
        private static String _STYPE = "manager2";
        private static String _SVERSION = "1.0";
        private static String _SAUTHOR = "APSIM";
        
        //=========================================================================
        /// <summary>
        /// Create an instance of a component here
        /// </summary>
        /// <param name="compID"></param>
        /// <param name="parentCompID"></param>
        /// <param name="messageCallback"></param>
        //=========================================================================
        public TComponentInstance(uint compID, uint parentCompID, MessageFromLogic messageCallback)
            : base(compID, parentCompID, messageCallback, _STYPE, _SVERSION, _SAUTHOR)
        {
        }
    }
    //============================================================================
    // Component developers should not need to change the code below this point.
    // It exists to provide a wrapping class which enables the component to be
    // loaded and used by native applications.
    //============================================================================

    [ComVisible(true)]
    internal class TGCComponent : TComponentInstance
    {
        //============================================================================
        /// <summary>
        /// This class is a thin wrapper over the TComponentInstance class, allowing
        /// the caller to provide the callback as a function pointer rather than as
        /// a delegate. This makes it relative easy to call the constructor from
        /// unmanaged code, without requiring mixed-mode dlls.
        /// </summary>
        /// <param name="compID"></param>
        /// <param name="parentCompID"></param>
        /// <param name="messageCallback"></param>
        //============================================================================
        // Note that we are assuming that an "IntPtr" can be stored within a uint
        // value. That is, we're assuming the native caller is 32-bit.
        // This value is cast to an IntPtr, then mashalled to a delegate
        public TGCComponent(uint compID, uint parentCompID, uint messageCallback)
            : base(compID, parentCompID,
            messageCallback == 0 ? null :
            (MessageFromLogic)Marshal.GetDelegateForFunctionPointer((IntPtr)messageCallback, typeof(MessageFromLogic)))
        {
            if (messageCallback != 0)
                msgNativeDestFunction = (NativeMessageFromLogic)Marshal.GetDelegateForFunctionPointer((IntPtr)messageCallback, typeof(NativeMessageFromLogic));
        }

        //[UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void NativeMessageFromLogic(ref uint dummy, ref TNativeMsgHeader inMsg);
        protected NativeMessageFromLogic msgNativeDestFunction = null;

        //==============================================================================
        /// <summary>
        /// Send a message up to the owning system so it can be routed throughout
        /// the simulation. Overridden to allow the message data to be converted
        /// to a native pointer.
        /// </summary>
        /// <param name="msg">Message that will be sent to the engine.</param>
        //==============================================================================
        protected override void sendMessageToEngine(TMsgHeader msg)
        {
            // Copy from TMsgHeader to TNativeMsgHeader.
            // The difference is in the nature of the data pointer.
            try
            {
                TNativeMsgHeader msgPtr = new TNativeMsgHeader();
                uint nBytes = msg.nDataBytes;
                try
                {
                    msgPtr.version = msg.version;
                    msgPtr.msgType = msg.msgType;
                    msgPtr.from = msg.from;
                    msgPtr.to = msg.to;
                    msgPtr.msgID = msg.msgID;
                    msgPtr.toAck = msg.toAck;
                    msgPtr.nDataBytes = nBytes;

                    if (nBytes == 0)
                    {
                        msgPtr.dataPtr = IntPtr.Zero;
                    }
                    else
                    {
                        /*
                        if (nBytes > memAllocSize)
                        {

                            if (memAllocSize > 0)
                                Marshal.FreeHGlobal(nativeMem);
                            nativeMem = Marshal.AllocHGlobal((int)nBytes);
                            memAllocSize = nBytes;
                        }
                        msgPtr.dataPtr = nativeMem; */
                        msgPtr.dataPtr = Marshal.AllocHGlobal((int)nBytes);
                        Marshal.Copy(msg.dataPtr, 0, msgPtr.dataPtr, (int)nBytes);
                    }
                    uint dummy = 0;
                    msgNativeDestFunction(ref dummy, ref msgPtr);	//will take a copy
                }
                finally
                {
                    if (nBytes > 0)
                        Marshal.FreeHGlobal(msgPtr.dataPtr);
                }
            }
            catch (Exception e)
            {
                StringBuilder exmsg = new StringBuilder("sendMessageToEngine() failed ");
                exmsg.Append(e.Message);
                Console.WriteLine(exmsg);
            }
        }

        //private uint memAllocSize = 0;
        //private IntPtr nativeMem;

        //==============================================================================
        /// <summary>
        /// Send a message up to the owning system so it can be routed throughout
        /// the simulation. Overridden to allow the message data to be converted
        /// to a native pointer. This enables calling from embedded Microsoft Frameworks or Mono.
        // The message itself then needs to be converted from a TNativeMsgHeader to a TMsgHeader,
        // which includes taking a copy of the data referenced by the data pointer
        /// </summary>
        /// <param name="msg">Message that will be sent to the engine.
        /// The value passed in, although described as a "uint", is actually
        /// a pointer to a native TMsgHeader
        /// </param>
        //==============================================================================
        public void handleMessage(uint inVal)
        {
            // We need to copy the "native" message point into a managed object.
            TNativeMsgHeader src = (TNativeMsgHeader)Marshal.PtrToStructure((IntPtr)inVal, typeof(TNativeMsgHeader));

            TMsgHeader msgPtr = TMessageInterpreter.NativeMsgToManagedMsg(ref src);

            handleMessage(msgPtr);  //calls the base class function
        }
    }
}