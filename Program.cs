/////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// This project demonstrates how to write a simple vJoy feeder in C#
//
// You can compile it with either #define ROBUST OR #define EFFICIENT
// The fuctionality is similar - 
// The ROBUST section demonstrate the usage of functions that are easy and safe to use but are less efficient
// The EFFICIENT ection demonstrate the usage of functions that are more efficient
//
// Functionality:
//	The program starts with creating one joystick object. 
//	Then it petches the device id from the command-line and makes sure that it is within range
//	After testing that the driver is enabled it gets information about the driver
//	Gets information about the specified virtual device
//	This feeder uses only a few axes. It checks their existence and 
//	checks the number of buttons and POV Hat switches.
//	Then the feeder acquires the virtual device
//	Here starts and endless loop that feedes data into the virtual device
//
/////////////////////////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;




// Don't forget to add thisS
using vJoyInterfaceWrap;

namespace FeederDemoCS
{
    class Program
    {
        // Declaring one joystick (Device id 1) and a position structure. 
        static public vJoy joystick;
        static public vJoy.JoystickState iReport;
        static public uint id = 1;
       // static public char input;



        [STAThread]
        static void Main(string[] args)
        {

            // Create one joystick object and a position structure.
            joystick = new vJoy();
            iReport = new vJoy.JoystickState();


            // Device ID can only be in the range 1-16
            if (args.Length > 0 && !String.IsNullOrEmpty(args[0]))
                id = Convert.ToUInt32(args[0]);
            if (id <= 0 || id > 16)
            {
                Console.WriteLine("Illegal device ID {0}\nExit!", id);
                return;
            }

            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!joystick.vJoyEnabled())
            {
                Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            }
            else
                Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n", joystick.GetvJoyManufacturerString(), joystick.GetvJoyProductString(), joystick.GetvJoySerialNumberString());

            // Get the state of the requested device
            VjdStat status = joystick.GetVJDStatus(id);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    Console.WriteLine("vJoy Device {0} is already owned by this feeder\n", id);
                    break;
                case VjdStat.VJD_STAT_FREE:
                    Console.WriteLine("vJoy Device {0} is free\n", id);
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
                    return;
                case VjdStat.VJD_STAT_MISS:
                    Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                    return;
                default:
                    Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                    return;
            };

            // Check which axes are supported
            bool AxisX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X);
            bool AxisY = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Y);
            bool AxisZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Z);
            bool AxisRX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RX);
            bool AxisRZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RZ);

            // Get the number of buttons and POV Hat switchessupported by this vJoy device
            int nButtons = joystick.GetVJDButtonNumber(id);
            int ContPovNumber = joystick.GetVJDContPovNumber(id);
            int DiscPovNumber = joystick.GetVJDDiscPovNumber(id);

            // Print results
            Console.WriteLine("\nvJoy Device {0} capabilities:\n", id);
            Console.WriteLine("Numner of buttons\t\t{0}\n", nButtons);
            Console.WriteLine("Numner of Continuous POVs\t{0}\n", ContPovNumber);
            Console.WriteLine("Numner of Descrete POVs\t\t{0}\n", DiscPovNumber);
            Console.WriteLine("Axis X\t\t{0}\n", AxisX ? "Yes" : "No");
            Console.WriteLine("Axis Y\t\t{0}\n", AxisX ? "Yes" : "No");
            Console.WriteLine("Axis Z\t\t{0}\n", AxisX ? "Yes" : "No");
            Console.WriteLine("Axis Rx\t\t{0}\n", AxisRX ? "Yes" : "No");
            Console.WriteLine("Axis Rz\t\t{0}\n", AxisRZ ? "Yes" : "No");

            // Acquire the target
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(id))))
            {
                Console.WriteLine("Failed to acquire vJoy device number {0}.\n", id);
                return;
            }
            else
                Console.WriteLine("Acquired: vJoy device number {0}.\n", id);

            Console.WriteLine("\npress enter to stat feeding");
            Console.ReadKey(true);

            int X, Y, Z, ZR, XR;
           // uint count = 0;
            long maxval = 0;

            X = 16383;
            Y = 16383;
            Z = 40;
            XR = 60;
            ZR = 80;

            joystick.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref maxval);
            // GattUtils.get

            //bool res;
            // Reset this device to default values
            joystick.ResetVJD(id);

            // Feed the device in endless loop
            while (true)
            {
                // Set position of 4 axes
                joystick.SetAxis(X, id, HID_USAGES.HID_USAGE_X);
                joystick.SetAxis(Y, id, HID_USAGES.HID_USAGE_Y);
                joystick.SetAxis(Z, id, HID_USAGES.HID_USAGE_Z);
                joystick.SetAxis(XR, id, HID_USAGES.HID_USAGE_RX);
                joystick.SetAxis(ZR, id, HID_USAGES.HID_USAGE_RZ);

                // the purpose of the XY resets is so the counter does not go infinitely in one direction or the other
                // gradually slowing down the speed at which the pointer travels due to scaling.
                //up down left right
                if (Keyboard.IsKeyDown(Key.F)) // Right
                {
                    X = 32767; //if (X > 200) X = 100;
                    Y = 16383;

                }
                else if ((Keyboard.IsKeyDown(Key.A))) // Left
                {
                    X = 0; // if (X < 0) X = 100;
                    Y = 16383;
                }
                else if ((Keyboard.IsKeyDown(Key.D))) // Up
                {
                    Y = 0; // if (Y < 0) Y = 100;
                    X = 16383;
                }
                else if ((Keyboard.IsKeyDown(Key.S))) // Down
                {
                    Y = 32767;// if (Y > 200) Y = 100;
                    X = 16383;
                }
                else if ((Keyboard.IsKeyDown(Key.D1)))
                {
                    while ((Keyboard.IsKeyDown(Key.D1)))
                    {
                        joystick.SetBtn(true, id, 1);

                    }
                    joystick.SetBtn(false, id, 1);
                }
                else if ((Keyboard.IsKeyDown(Key.D2)))
                {
                    while ((Keyboard.IsKeyDown(Key.D2)))
                    {
                        joystick.SetBtn(true, id, 2);

                    }
                    joystick.SetBtn(false, id, 2);
                }
                else if ((Keyboard.IsKeyDown(Key.D3)))
                {
                    while ((Keyboard.IsKeyDown(Key.D3)))
                    {
                        joystick.SetBtn(true, id, 3);

                    }
                    joystick.SetBtn(false, id, 3);
                }
                else if ((Keyboard.IsKeyDown(Key.D4)))
                {
                    while ((Keyboard.IsKeyDown(Key.D4)))
                    {
                        joystick.SetBtn(true, id, 4);

                    }
                    joystick.SetBtn(false, id, 4);
                }
                else if ((Keyboard.IsKeyDown(Key.D5)))
                {
                    while ((Keyboard.IsKeyDown(Key.D5)))
                    {
                        joystick.SetBtn(true, id, 5);

                    }
                    joystick.SetBtn(false, id, 5);
                }
                else if ((Keyboard.IsKeyDown(Key.D6)))
                {
                    while ((Keyboard.IsKeyDown(Key.D6)))
                    {
                        joystick.SetBtn(true, id, 6);

                    }
                    joystick.SetBtn(false, id, 6);
                }
                else if ((Keyboard.IsKeyDown(Key.D7)))
                {
                    while ((Keyboard.IsKeyDown(Key.D7)))
                    {
                        joystick.SetBtn(true, id, 7);

                    }
                    joystick.SetBtn(false, id, 7);
                }
                else if ((Keyboard.IsKeyDown(Key.D8)))
                {
                    while ((Keyboard.IsKeyDown(Key.D8)))
                    {
                        joystick.SetBtn(true, id, 8);

                    }
                    joystick.SetBtn(false, id, 8);
                }
                else if (!Keyboard.IsKeyDown(Key.Down) || !Keyboard.IsKeyDown(Key.Up)
                    || !Keyboard.IsKeyDown(Key.Left) || !Keyboard.IsKeyDown(Key.Right))
                {
                    X = 16383;
                    Y = 16383;
                }




            } // While (Robust)



        }

        
       

        
    } // class Program
} // namespace FeederDemoCS
