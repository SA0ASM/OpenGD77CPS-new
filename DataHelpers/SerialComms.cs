using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Ports;
using System.Printing.IndexedProperties;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace OpenGD77CPS
{
    internal class SerialComms
    {
        char _writeCommand = 'W';
        int _comBufferSize = 32;
        bool _hasEmulatedEEPROM = false;
        SerialPort _port;

        public SerialComms()
        {
            _port = new SerialPort(SerialPort.GetPortNames()[0]);
            _port.BaudRate = 9600;
            _port.DataBits = 8;
            _port.Parity = Parity.None;
            _port.StopBits = StopBits.One;
            _port.ReadTimeout = _port.WriteTimeout = 2000;
            _port.Open();
        }

        ~SerialComms()
        {
            _port.Close();
        }

        public enum RadioType: uint
        {
            GD77 = 0,
            GD77S = 1,
            DM1801 = 2,
            RD5R = 3,
            DM1801A = 4,
            MD9600 = 5,
            MDUV380 = 6,
            MD380 = 7,
            DM1701_BGR = 8,
            MD2017 = 9,
            DM1701_RGB = 10,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct RadioInfo
        {
            public uint structVersion;
            public RadioType radioType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string gitRevision;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string buildDateTime;
            public uint flashId;
            public ushort features;
        }

        public enum MemType
        {
            FLASH = 1,
            EEPROM = 2,
            MCU_ROM = 5,
            DISPLAY_BUFFER = 6,
            WAV_BUFFER = 7,
            COMPRESSED_AMBE_BUFFER = 8,
            RADIO_INFO = 9,
            SECURITY_REGISTERS = 10,
        }

        private bool sendCmd(byte cmd, byte[] bytes = null)
        {
            var sendBytes = new byte[2 + (bytes ?? new byte[0]).Length];
            sendBytes[0] = (byte)'C';
            sendBytes[1] = (byte)cmd;
            if (bytes != null)
                bytes.CopyTo(sendBytes, 2);

            try
            {
                _port.Write(sendBytes, 0, sendBytes.Length);

                while (_port.BytesToRead < 1)
                    Thread.Sleep(5);

                var ack = new byte[1];
                _port.Read(ack, 0, ack.Length);

                return (ack[0] == (byte)'-');

            } catch (System.TimeoutException)
            {
                return false;
            }
        }

        public RadioInfo identifyRadio()
        {
            RadioInfo radioInfo = new RadioInfo();

            try
            {
                var req = new byte[8];
                req[0] = (byte)'R';
                req[1] = (byte)MemType.RADIO_INFO;
                req[2] = 0;
                req[3] = 0;
                req[4] = 0;
                req[5] = 0;
                req[6] = 0;
                req[7] = 0;
                _port.Write(req, 0, req.Length);

                byte[] resp = new byte[128];
                var nBytes = 0;
                try
                {
                    while (!_port.BaseStream.CanRead)
                        Thread.Sleep(5);

                    nBytes = _port.BaseStream.ReadAtLeast(resp, 3);
                }
                catch (IOException)
                {
                    return radioInfo;
                }

                if (nBytes < 3 || resp[0] != req[0] || nBytes - 3 < ((resp[1] << 8) + resp[2]))
                    return radioInfo;

                var riStruct = new byte[nBytes - 3];
                Array.Copy(resp, 3, riStruct, 0, nBytes - 3);
                GCHandle gcHandle = GCHandle.Alloc(riStruct, GCHandleType.Pinned);
                try
                {
                    radioInfo = (RadioInfo)Marshal.PtrToStructure<RadioInfo>(gcHandle.AddrOfPinnedObject());
                }
                finally
                {
                    gcHandle.Free();
                }
            }
            catch (System.TimeoutException)
            {
                return radioInfo;
            }

            // update radio-dependent values
            try
            {
                if (radioInfo.buildDateTime.Length > 8 && uint.Parse(radioInfo.buildDateTime.Substring(0, 8)) > 20211002)
                    _comBufferSize = 1024;
            }
            catch (FormatException ex)
            { }

            switch (radioInfo.radioType)
            {
                case RadioType.MD9600:
                case RadioType.MDUV380:
                case RadioType.MD380:
                case RadioType.DM1701_BGR:
                case RadioType.MD2017:
                    _writeCommand = 'X';
                    _hasEmulatedEEPROM = true;
                    break;
                default:
                    _writeCommand = 'W';
                    _hasEmulatedEEPROM = false;
                    break;
            }

            return radioInfo;
        }


        public byte[] readMemory(MemType type, UInt32 baseAddr, int len)
        {
            // set up the read operation
            if ((MemType.FLASH == type) && _hasEmulatedEEPROM)
            {
                // STM32-based radio with emulated EEPROM, flash data starts at 0x20000
                baseAddr += 0x20000;
            }

            var data = new byte[len];
            try
            {
                int bytesRead = 0;
                while (len > bytesRead || MemType.RADIO_INFO == type)
                {
                    int bytesToRead = Math.Min(len - bytesRead, _comBufferSize - 3);
                    UInt32 addr = baseAddr + (uint)bytesRead;

                    var req = new byte[8];
                    req[0] = (byte)'R';
                    req[1] = (byte)type;
                    req[2] = (byte)((addr >> 24) & 0xff);
                    req[3] = (byte)((addr >> 16) & 0xff);
                    req[4] = (byte)((addr >> 8) & 0xff);
                    req[5] = (byte)(addr & 0xff);
                    req[6] = (byte)((bytesToRead >> 8) & 0xff);
                    req[7] = (byte)(bytesToRead & 0xff);
                    _port.Write(req, 0, req.Length);

                    byte[] resp = new byte[_comBufferSize];
                    var nBytes = 0;
                    try
                    {
                        while (!_port.BaseStream.CanRead)
                            Thread.Sleep(5);

                        nBytes = _port.BaseStream.ReadAtLeast(resp, 3);
                    }
                    catch (IOException)
                    {
                        return new byte[0];
                    }
                    if (nBytes < 3 || resp[0] != req[0] || nBytes - 3 < ((resp[1] << 8) + resp[2]))
                        return new byte[0];

                    // save the data
                    Array.Copy(resp, 3, data, bytesRead, nBytes-3);
                    bytesRead += nBytes-3;
                }
            }
            catch (System.TimeoutException)
            {
                return new byte[0];
            }
            return data;
        }

        private bool doWriteOperation(byte[] data, bool delay = false)
        {
            try
            {
                _port.Write(data, 0, data.Length);
                while (_port.BytesToWrite > 0)
                    Thread.Sleep(1);
                
                if (delay)
                    Thread.Sleep(100);
                
                var resp = new byte[2];
                while (_port.BytesToRead < 2)
                    Thread.Sleep(1);

                var res = _port.Read(resp, 0, 2);
                if (res != 2 || resp[0] != data[0] || resp[1] != data[1])
                    return false;

            } catch (System.TimeoutException)
            {
                return false;
            }

            return true;
        }

        private enum WriteOperation
        {
            FLASH_PREPARE = 1,
            FLASH = 2,
            FLASH_COMMIT = 3,
            EEPROM = 4,
        }

        private bool uploadData(WriteOperation operation, UInt32 baseAddr, byte[] data)
        {

            int bytesWritten = 0;
            while (data.Length > bytesWritten)
            {
                int bytesToWrite = Math.Min(data.Length - bytesWritten, _comBufferSize-8);
                UInt32 addr = baseAddr + (uint)bytesWritten;

                var req = new byte[8 + bytesToWrite];
                req[0] = (byte)_writeCommand;
                req[1] = (byte)operation;
                req[2] = (byte)((addr >> 24) & 0xff);
                req[3] = (byte)((addr >> 16) & 0xff);
                req[4] = (byte)((addr >> 8) & 0xff);
                req[5] = (byte)(addr & 0xff);
                req[6] = (byte)((bytesToWrite >> 8) & 0xff);
                req[7] = (byte)(bytesToWrite & 0xff);
                Array.Copy(data, bytesWritten, req, 8, bytesToWrite);
                if (!doWriteOperation(req))
                    return false;
                
                bytesWritten += bytesToWrite;
            }

            return true;
        }

        private bool writeMemory(MemType type, UInt32 baseAddr, byte[] data, int offset, int len)
        {
            // validate parameters
            if (type != MemType.EEPROM && type != MemType.FLASH)
                return false;
            if (offset + len > data.Length)
                return false;

            // set up the write operation
            WriteOperation operation = WriteOperation.FLASH;
            if ((MemType.FLASH == type) && _hasEmulatedEEPROM)
            {
                // STM32-based radio with emulated EEPROM, flash data starts at 0x20000
                baseAddr += 0x20000;
            }
            if ((MemType.EEPROM == type) && !_hasEmulatedEEPROM)
            {
                // MK22-based radio with real EEPROM
                operation = WriteOperation.EEPROM;
            }

            int bytesWritten = 0;
            while (len > bytesWritten)
            {
                UInt32 addr = baseAddr + (UInt32)bytesWritten;
                int bytesToWrite = len - bytesWritten;

                if (operation == WriteOperation.FLASH)
                {
                    // write data sector by sector
                    if (bytesToWrite > 4096)
                        bytesToWrite = 4096;

                    // handle unaligned writes
                    if ((addr % 4096) != 0)
                    {
                        var remainder = 4096 - (addr % 4096);
                        if (bytesToWrite > remainder)
                            bytesToWrite = (int)remainder;
                    }

                    // prepare sector for writing
                    var req = new byte[5];
                    int sector = (int)addr / 4096; 
                    req[0] = (byte)_writeCommand;
                    req[1] = (byte)WriteOperation.FLASH_PREPARE;
                    req[2] = (byte)((sector >> 16) & 0xff);
                    req[3] = (byte)((sector >> 8) & 0xff);
                    req[4] = (byte)(sector & 0xff);
                    if (!doWriteOperation(req, delay: true))
                        return false;
                }

                // upload the data to write
                var dataToWrite = new byte[bytesToWrite];
                Array.Copy(data, offset + bytesWritten, dataToWrite, 0, bytesToWrite);
                if (!uploadData(operation, addr, dataToWrite))
                    return false;

                if (operation == WriteOperation.FLASH)
                    {
                    // commit data to flash
                    var req = new byte[2];
                    req[0] = (byte)_writeCommand;
                    req[1] = (byte)WriteOperation.FLASH_COMMIT;
                    if (!doWriteOperation(req, delay: true))
                        return false;
                }

                bytesWritten += bytesToWrite;
            }

            return true;
        }


        public byte[] readCodeplug()
        {
            var res = sendCmd(0); // start CPS screen
            res = res && sendCmd(1); // clear screen
            res = res && sendCmd(2, Encoding.ASCII.GetBytes("\x00\x20\x03\x01\x00OpenGD77CPS\x00\x00\x00\x00\x00"));
            res = res && sendCmd(2, Encoding.ASCII.GetBytes("\x00\x40\x03\x01\x00Reading Codeplug"));
            res = res && sendCmd(3); // render screen
            res = res && sendCmd(4); // turn on backlight
            res = res && sendCmd(6, new byte[] { 3 }); // flash green

            if (!res)
            {
                sendCmd(5); // close CPS screen
                _port.Close();
                return new byte[0];
            }

            var eeprom = readMemory(MemType.EEPROM, 0, 0xb000);
            var flash0 = readMemory(MemType.FLASH, 0x7b000, 0x13e60);
            var flash1 = readMemory(MemType.FLASH, 0, 0x11a0);

            sendCmd(5); // close CPS screen

            if (eeprom.Length == 0xb000 && flash0.Length == 0x13e60 && flash1.Length == 0x11a0)
            {
                var cpdata = new byte[0x20000];
                eeprom.CopyTo(cpdata, 0);
                flash0.CopyTo(cpdata, 0xb000);
                flash1.CopyTo(cpdata, 0x1ee60);
                return cpdata;
            }

            return new byte[0];
        }

        public bool writeCodeplug(byte[] data)
        {
            var res = sendCmd(0); // show CPS screen
            res = res && sendCmd(1); // clear screen
            res = res && sendCmd(2, Encoding.ASCII.GetBytes("\x00\x20\x03\x01\x00OpenGD77CPS\x00\x00\x00\x00\x00"));
            res = res && sendCmd(2, Encoding.ASCII.GetBytes("\x00\x40\x03\x01\x00Writing Codeplug"));
            res = res && sendCmd(3); // render screen
            res = res && sendCmd(4); // turn on backlight
            res = res && sendCmd(6, new byte[] { 4 }); // flash red
            res = res && sendCmd(6, new byte[] { 2 }); // save

            if (!res)
            {
                sendCmd(5); // close CPS screen
                return false;
            }

            res = res && writeMemory(MemType.EEPROM, 0xe0, data, 0xe0, 0x6000-0xe0);
            res = res && writeMemory(MemType.EEPROM, 0x7500, data, 0x7500, 0xb000 - 0x7500);
            res = res && writeMemory(MemType.FLASH, 0x7b000, data, 0xb000, 0x13e60);
            res = res && writeMemory(MemType.FLASH, 0, data, 0x1ee60, 0x11a0);

            res = res && sendCmd(5); // close CPS screen
            res = res && sendCmd(6, new byte[] { 0 }); // save and reboot
            _port.Close();

            return res;
        }
    }
}
