using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using OpenGD77CPS.Models;
using System.Windows.Shapes;

namespace OpenGD77CPS
{
    internal class CPFormatOGD77
    {
        #region  CodePlug Defines
        // Codeplug offsets copied from firmware file codeplug.c
        // (flash offset of 0x70000 removed where necessary)

        static int CODEPLUG_ADDR_EX_ZONE_BASIC = 0x8000;
        static int CODEPLUG_ADDR_EX_ZONE_INUSE_PACKED_DATA = 0x8010;
        static int CODEPLUG_ADDR_EX_ZONE_LIST = 0x8030;

        static int CODEPLUG_ZONE_MAX_COUNT = 250;
        static int CODEPLUG_ADDR_CHANNEL_EEPROM = 0x3790;
        static int CODEPLUG_ADDR_CHANNEL_HEADER_EEPROM = 0x3780; // CODEPLUG_ADDR_CHANNEL_EEPROM - 16
        static int CODEPLUG_ADDR_CHANNEL_FLASH = 0xB1C0;
        static int CODEPLUG_ADDR_CHANNEL_HEADER_FLASH = 0xB1B0; // CODEPLUG_ADDR_CHANNEL_FLASH - 16

        static int CODEPLUG_ADDR_SIGNALLING_DTMF = 0x1400;
        static int CODEPLUG_ADDR_SIGNALLING_DTMF_DURATIONS = (CODEPLUG_ADDR_SIGNALLING_DTMF + 0x72); // offset to grab the DTMF durations
        static int CODEPLUG_SIGNALLING_DTMF_DURATIONS_SIZE = 4;

        static int CODEPLUG_ADDR_RX_GROUP_LEN = 0x1D620;  // 76 TG lists
        static int CODEPLUG_ADDR_RX_GROUP = 0x1D6A0;//

        static int CODEPLUG_ADDR_CONTACTS = 0x17620;

        static int CODEPLUG_ADDR_DTMF_CONTACTS = 0x02f88;

        static int CODEPLUG_ADDR_USER_DMRID = 0x00E8;
        static int CODEPLUG_ADDR_USER_CALLSIGN = 0x00E0;

        static int CODEPLUG_ADDR_GENERAL_SETTINGS = 0x00E0;

        static int CODEPLUG_ADDR_BOOT_INTRO_SCREEN = 0x7518;// 0x01 = Chars 0x00 = Picture
        static int CODEPLUG_ADDR_BOOT_PASSWORD_ENABLE = 0x7519;// 0x00 = password disabled 0x01 = password enable
        static int CODEPLUG_ADDR_BOOT_PASSWORD_AREA = 0x751C;// Seems to be 3 bytes coded as BCD e.f. 0x12 0x34 0x56
        static int CODEPLUG_BOOT_PASSWORD_LEN = 3;
        static int CODEPLUG_ADDR_BOOT_LINE1 = 0x7540;
        static int CODEPLUG_ADDR_BOOT_LINE2 = 0x7550;
        static int CODEPLUG_ADDR_VFO_A_CHANNEL = 0x7590;

        static int codeplugChannelsPerZone = 16;

        static int[] VFO_FREQ_STEP_TABLE = [250, 500, 625, 1000, 1250, 2500, 3000, 5000];

        static int CODEPLUG_MAX_VARIABLE_SQUELCH = 21;
        static int CODEPLUG_MIN_VARIABLE_SQUELCH = 1;

        static int CODEPLUG_MIN_PER_CHANNEL_POWER = 1;

        static int CODEPLUG_ADDR_DEVICE_INFO = 0x80;
        static int CODEPLUG_ADDR_DEVICE_INFO_READ_SIZE = 96;// (sizeof struct_codeplugDeviceInfo_t)

        static int CODEPLUG_ADDR_BOOT_PASSWORD_PIN = 0x7518;

        static int CODEPLUG_CONTACT_DATA_SIZE = 24;

        static int CODEPLUG_RXGROUP_DATA_STRUCT_SIZE = 80;

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct codeplugChannel
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] name;
            public uint rxFreq;
            public uint txFreq;
            public byte chMode;
            public byte libreDMR_Power;
            public byte locationLat0;
            public byte tot;
            public byte locationLat1;
            public byte locationLat2;
            public byte locationLon0;
            public byte locationLon1;
            public ushort rxTone;
            public ushort txTone;
            public byte locationLon2;
            public byte _UNUSED_1;
            public byte LibreDMR_flag1;
            public byte dmrID2;
            public byte dmrID1;
            public byte dmrID0;
            public byte _UNUSED_2;
            public byte rxGroupList;
            public byte txColor;
            public byte aprsSystem;
            public ushort contact;
            public byte flag1;
            public byte flag2;
            public byte flag3; // bits... 0x20 = DisableAllLeds
            public byte flag4; // bits... 0x80 = Power, 0x40 = Vox, 0x20 = ZoneSkip, 0x10 = AllSkip, 0x08 = AllowTalkaround, 0x04 = OnlyRx, 0x02 = Channel width, 0x01 = Squelch
            public ushort reserve2;
            public byte reserve;
            public byte sql;
        }
        #endregion

        private static byte[] getBytes<T>(T obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] arr = new byte[size];

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr<T>(obj, ptr, false);
                Marshal.Copy(ptr, arr, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }
        
        private static String ReadString(Stream stream, int maxlen)
        {
            // read name bytes
            byte[] name = new byte[maxlen];
            stream.ReadExactly(name, 0, maxlen);

            // count length of name
            int name_len = 0;
            foreach (byte b in name)
                if (b != 0 && b != 255)
                    name_len++;
                else
                    break;

            // decode string
            String result = "";
            if (name_len > 0)
                result = Encoding.Latin1.GetString(name, 0, name_len);

            return result;
        }

        private static void WriteString(Stream stream, String name, int maxlen)
        {
            byte[] ba = new byte[maxlen];
            Array.Fill(ba, (byte)0);
            Encoding.Latin1.GetBytes(name, 0, Math.Min(maxlen, name.Length), ba, 0);
            stream.Write(ba);
        }

        private static UInt32 ReadBCD(Stream stream)
        {
            // read name bytes
            byte[] ba = new byte[4];
            stream.ReadExactly(ba, 0, 4);
            if (ba[0] == 0xff && ba[1] == 0xff && ba[2] == 0xff && ba[3] == 0xff)
                return 0xffffffff;
            return UInt32.Parse(String.Format("{0:x2}{1:x2}{2:x2}{3:x2}", ba[0], ba[1], ba[2], ba[3]));
        }

        private static void WriteBCD(Stream stream, UInt32 value)
        {
            UInt32 n = UInt32.Parse(value.ToString(), NumberStyles.HexNumber);
            var ba = BitConverter.GetBytes(n);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ba);
            stream.Write(ba);
        }

        private static GeneralSettings ReadGeneralSettings(Stream stream)
        {
            var settings = new GeneralSettings();
            using (var reader = new BinaryReader(stream, Encoding.Latin1, true))
            {
                stream.Seek(CODEPLUG_ADDR_USER_CALLSIGN, SeekOrigin.Begin);
                settings.Callsign = ReadString(stream, 8);
                settings.DMRId = (int)ReadBCD(stream);
                settings.cpVersion = (int)reader.ReadByte();

                stream.Seek(CODEPLUG_ADDR_BOOT_INTRO_SCREEN, SeekOrigin.Begin);
                var b = stream.ReadByte();
                settings.CustomBootScreen = (b & 1) == 1;

                stream.Seek(CODEPLUG_ADDR_BOOT_LINE1, SeekOrigin.Begin);
                settings.BootTextLine1 = ReadString(stream, 16);
                settings.BootTextLine2 = ReadString(stream, 16);
            }

            return settings;
        }

        private static Dictionary<String, List<ushort>> ReadZones(Stream stream)
        {
            var zones = new Dictionary<String, List<ushort>>();

            // read zone header
            stream.Seek(CODEPLUG_ADDR_EX_ZONE_INUSE_PACKED_DATA, SeekOrigin.Begin);
            byte[] hdr = new byte[32];
            stream.ReadExactly(hdr, 0, 32);
            BitArray hdrBits = new BitArray(hdr);

            using (var reader = new BinaryReader(stream, Encoding.Latin1, true))
            {
                // loop through all zones
                for (int z = 0; z < 68; z++)
                {
                    var name = ReadString(stream, 16);

                    // check if zone is valid
                    if (hdrBits[z] == true)
                    {
                        // read the list of channel numbers
                        var channels = new List<ushort>();
                        for (int c = 0; c < 80; c++)
                        {
                            var channel = reader.ReadUInt16();
                            if (channel > 0 && channel < 0xffff)
                                channels.Add(channel);
                        }
                        zones.Add(name, channels);
                    }
                }
            }

           return zones;
        }

        private static List<Channel> ReadChannelBank(Stream stream, int bank, List<Contact> contacts, List<ContactGroup> groups)
        {
            List<Channel> channels = new List<Channel>();

            // read bank header
            byte[] hdr = new byte[codeplugChannelsPerZone];
            stream.ReadExactly(hdr, 0, 16);
            BitArray hdrBits = new BitArray(hdr);

            for (int chnum = 0; chnum < 128; chnum++)
            {
                if (hdrBits[chnum])
                {
                    using (var reader = new BinaryReader(stream, Encoding.Latin1, true))
                    {
                        var c = new codeplugChannel();

                        String name = ReadString(stream, 16);
                        c.rxFreq = reader.ReadUInt32();
                        c.txFreq = reader.ReadUInt32();
                        c.chMode = (byte)stream.ReadByte();
                        c.libreDMR_Power = (byte)stream.ReadByte();
                        c.locationLat0 = (byte)stream.ReadByte();
                        c.tot = (byte)stream.ReadByte();
                        c.locationLat1 = (byte)stream.ReadByte();
                        c.locationLat2 = (byte)stream.ReadByte();
                        c.locationLon0 = (byte)stream.ReadByte();
                        c.locationLon1 = (byte)stream.ReadByte();
                        c.rxTone = reader.ReadUInt16();
                        c.txTone = reader.ReadUInt16();
                        c.locationLon2 = (byte)stream.ReadByte();
                        c._UNUSED_1 = (byte)stream.ReadByte();
                        c.LibreDMR_flag1 = (byte)stream.ReadByte();
                        c.dmrID2 = (byte)stream.ReadByte();
                        c.dmrID1 = (byte)stream.ReadByte();
                        c.dmrID0 = (byte)stream.ReadByte();
                        c._UNUSED_2 = (byte)stream.ReadByte();
                        c.rxGroupList = (byte)stream.ReadByte();
                        c.txColor = (byte)stream.ReadByte();
                        c.aprsSystem = (byte)stream.ReadByte();
                        c.contact = reader.ReadUInt16();
                        c.flag1 = (byte)stream.ReadByte();
                        c.flag2 = (byte)stream.ReadByte();
                        c.flag3 = (byte)stream.ReadByte();
                        c.flag4 = (byte)stream.ReadByte();
                        c.reserve2 = reader.ReadUInt16();
                        c.reserve = (byte)stream.ReadByte();
                        c.sql = (byte)stream.ReadByte();

                        if (!Enum.IsDefined(typeof(ChannelBandwidth), (int)c.chMode))
                            c.chMode = 0;

                        Channel channel = new Channel(name, (ChannelType)c.chMode);
                        channel.SetNumber(bank * 128 + chnum + 1);

                        channel.RxFrequency = String.Format("{0:x}0", c.rxFreq);
                        channel.TxFrequency = String.Format("{0:x}0", c.txFreq);

                        int bw = ((int)c.flag4 & 2) >> 1;
                        if (Enum.IsDefined(typeof(ChannelBandwidth), bw))
                            channel.Bandwidth =  (ChannelBandwidth)bw;

                        int ts = ((int)c.flag2 & 64) >> 6;
                        if (Enum.IsDefined(typeof(ChannelBandwidth), ts))
                            channel.Timeslot = (TimeSlot)ts;

                        channel.Colorcode = c.txColor;

                        if (c.contact > 0)
                            channel.txContact = contacts.ElementAtOrDefault(c.contact - 1);

                        if (c.rxGroupList > 0)
                            channel.rxGroup = groups.ElementAtOrDefault(c.rxGroupList - 1);

                        if ((c.LibreDMR_flag1 & 128) == 128)
                            channel.DMRId = ((c.dmrID2 << 16) + (c.dmrID1 << 8) + c.dmrID0);

                        if (Enum.IsDefined(typeof(SubTone), c.rxTone))
                            channel.RxTone = (SubTone)c.rxTone;
                        if (Enum.IsDefined(typeof(SubTone), c.txTone))
                            channel.TxTone = (SubTone)c.txTone;

                        if (Enum.IsDefined(typeof(ChannelSquelch), (int)c.sql))
                            channel.Squelch = (ChannelSquelch)c.sql;

                        if (Enum.IsDefined(typeof(ChannelPower), (int)c.libreDMR_Power))
                            channel.Power = (ChannelPower)c.libreDMR_Power;

                        if (Enum.IsDefined(typeof(ChannelTOT), (int)c.tot))
                            channel.TOT = (ChannelTOT)c.tot;

                        channel.RxOnly = (c.flag4 & 4) == 4;
                        channel.AllSkip = (c.flag4 & 16) == 16;
                        channel.ZoneSkip = (c.flag4 & 32) == 32;

                        int lat24 = (((c.locationLat2 & 0x7f) << 16) + (c.locationLat1 << 8) + c.locationLat0);
                        Decimal lat = (Decimal)(uint)(lat24 & 0x7fff) / 10000M + (Decimal)(lat24 >>> 15);
                        if ((c.locationLat2 & 0x80) == 0x80)
                            lat = -1M * lat;

                        int lon24 = (((c.locationLon2 & 0x7f) << 16) + (c.locationLon1 << 8) + c.locationLon0);
                        Decimal lon = (Decimal)(uint)(lon24 & 0x7fff) / 10000M + (Decimal)(lon24 >>> 15);
                        if ((c.locationLon2 & 0x80) == 0x80)
                            lon = -1M * lon;

                        channel.Latitude = lat;
                        channel.Longitude = lon;

                        channels.Add(channel);
                    }
                }
                else
                {
                    // skip invalid channel
                    stream.Seek(0x38, SeekOrigin.Current);
                }
            }

            return channels;
        }

        private static Dictionary<String, List<ushort>> ReadContactGroups(Stream stream)
        {
            var zones = new Dictionary<String, List<ushort>>();

            stream.Seek(CODEPLUG_ADDR_RX_GROUP, SeekOrigin.Begin);
            using (var reader = new BinaryReader(stream, Encoding.Latin1, true))
            {
                // loop through all groups
                for (int z = 0; z < 76; z++)
                {
                    var name = ReadString(stream, 16);
                    var contacts = new List<ushort>();

                    // read the contact list
                    for (int c = 0; c < 32; c++)
                    {
                        var id = reader.ReadUInt16();
                        if (id > 0 && id < 1025)
                            contacts.Add(id);
                    }

                    // add group if name is valid
                    if (name.Length > 0)
                        zones.Add(name, contacts);
                }
            }

            return zones;
        }

        private static Dictionary<int, Contact> ReadContacts(Stream stream)
        {
            var contacts = new Dictionary<int, Contact>();

            stream.Seek(CODEPLUG_ADDR_CONTACTS, SeekOrigin.Begin);
            using (var reader = new BinaryReader(stream, Encoding.Latin1, true))
            {
                for (int i = 0; i < 1024; i++)
                {
                    var name = ReadString(stream, 16);
                    var number = ReadBCD(stream);

                    var c = new Contact(name, number);

                    var t = stream.ReadByte();
                    if (Enum.IsDefined(typeof(ContactType), (int)t))
                        c.Type = (ContactType)t;

                    // not used in firmware?
                    var rxTone = stream.ReadByte();
                    var ringStyle = stream.ReadByte();

                    // read and parse flags
                    var reserve1 = stream.ReadByte();
                    if ((reserve1 & 1) == 0)
                        c.OverrideTS = (TimeSlot)((reserve1 & 2) >> 1);

                    // add valid contacts
                    if (name.Length > 0)
                        contacts.Add(i+1, c);
                }
            }

            return contacts;
        }

        public static (bool, GeneralSettings, List<Channel>, List<Zone>, List<Contact>, List<ContactGroup>) Load(Stream stream)
        {
            bool res = false;
            GeneralSettings settings = new GeneralSettings();
            List<Channel> channels = new List<Channel>();
            List<Zone> zones = new List<Zone>();
            List<Contact> contacts = new List<Contact>();
            List<ContactGroup> contactGroups = new List<ContactGroup>();

                
            try {
                // read radio settings
                settings = ReadGeneralSettings(stream);

                // read contacts
                var contactDict = ReadContacts(stream);
                foreach (var c in contactDict.Values)
                    contacts.Add(c);

                // read groups and map contacts
                var groups = ReadContactGroups(stream);
                foreach (var item in groups)
                {
                    var g = new ContactGroup(item.Key);
                    foreach (var num in item.Value)
                    {
                        if (contactDict.ContainsKey(num))
                            g.AddContact(contactDict[num]);
                    }
                    contactGroups.Add(g);
                }

                // read zone list
                var zonedata = ReadZones(stream);

                // read eeprom channels
                stream.Seek(CODEPLUG_ADDR_CHANNEL_HEADER_EEPROM, SeekOrigin.Begin);
                List<Channel> bank0 = ReadChannelBank(stream, 0, contacts, contactGroups);
                channels.AddRange(bank0);

                // read flash channels
                stream.Seek(CODEPLUG_ADDR_CHANNEL_HEADER_FLASH, SeekOrigin.Begin);
                for (int nbank = 1; nbank < 8; nbank++)
                {
                    List<Channel> bank = ReadChannelBank(stream, nbank, contacts, contactGroups);
                    channels.AddRange(bank);
                }

                // map channels to zones
                foreach (var item in zonedata)
                {
                    var z = new Zone(item.Key);
                    foreach (var num in item.Value)
                    {
                        z.AddChannel(channels.Single(m => m.Number == num));
                    }
                    zones.Add(z);
                }
            
                res = true;
            }
            catch (Exception e)
            {
                // Feed back exception info here?
            }

            return (res, settings, channels, zones, contacts, contactGroups);
        }

        private static bool WriteGeneralSettings(Stream stream, GeneralSettings settings)
        {
            bool res = true;

            using (var writer = new BinaryWriter(stream, Encoding.Latin1, true))
            {
                stream.Seek(CODEPLUG_ADDR_USER_CALLSIGN, SeekOrigin.Begin);
                WriteString(stream, settings.Callsign, 8);
                WriteBCD(stream, (uint)settings.DMRId);
                stream.WriteByte((byte)settings.cpVersion);

                stream.Seek(CODEPLUG_ADDR_BOOT_INTRO_SCREEN, SeekOrigin.Begin);
                byte b = 0;
                if (settings.CustomBootScreen)
                    b |= 1;
                else
                    b = (byte)(b & 0xfe);
                stream.WriteByte(b);

                stream.Seek(CODEPLUG_ADDR_BOOT_LINE1, SeekOrigin.Begin);
                WriteString(stream, settings.BootTextLine1, 16);
                WriteString(stream, settings.BootTextLine2, 16);
            }

            return res;
        }

        private static bool WriteZones(Stream stream, IEnumerable<Zone> zones)
        {
            // create zone header
            stream.Seek(CODEPLUG_ADDR_EX_ZONE_INUSE_PACKED_DATA, SeekOrigin.Begin);
            byte[] hdr = new byte[32];
            BitArray hdrBits = new BitArray(hdr);
            for (int i = 0; i < zones.Count(); i++)
                hdrBits.Set(i, true);
            hdrBits.CopyTo(hdr, 0);
            stream.Write(hdr);

            // write the zones
            using (var writer = new BinaryWriter(stream, Encoding.Latin1, true))
            {
                foreach (var zone in zones)
                {
                    byte[] name = new byte[16];
                    Encoding.Latin1.GetBytes(zone.Name, 0, Math.Min(16, zone.Name.Length), name, 0);
                    stream.Write(name);

                    for (int i = 0; i < 80; i++)
                    {
                        if (i < zone.Channels.Count)
                            writer.Write((ushort)zone.Channels.ElementAt(i).Number);
                        else
                            writer.Write((ushort)0);
                    }
                }
            }
            return true;
        }

        private static bool WriteChannelBank(Stream stream, IEnumerable<Channel> channels, int bank, List<Contact> contacts, List<ContactGroup> groups)
        {
            bool res = true;
            var chanLookup = channels.ToLookup(c => c.Number, c => c);

            // create bank header
            byte[] hdr = new byte[codeplugChannelsPerZone];
            BitArray hdrBits = new BitArray(hdr);
            for (int bidx = 0; bidx < 128; bidx++)
            {
                if (chanLookup.Contains(bidx + 1 + 128 * bank))
                {
                    hdrBits.Set(bidx, true);
                }
            }
            hdrBits.CopyTo(hdr, 0);
            stream.Write(hdr);

            // write channels
            for (int chnum = 1 + 128 * bank; chnum <= 128 * (bank + 1); chnum++)
            {
                if (chanLookup.Contains(chnum))
                {
                    Channel channel = chanLookup[chnum].First();
                    var c = new codeplugChannel();

                    c.name = new byte[16];
                    Encoding.Latin1.GetBytes(channel.Name, 0, Math.Min(16, channel.Name.Length), c.name, 0);
                    c.rxFreq = UInt32.Parse((channel.GetRxFrequency() / 10).ToString(), NumberStyles.HexNumber);
                    c.txFreq = UInt32.Parse((channel.GetTxFrequency() / 10).ToString(), NumberStyles.HexNumber);
                    c.chMode = (byte)channel.Type;
                    c.libreDMR_Power = (byte)channel.Power;
                    c.locationLat0 = 0;
                    c.tot = (byte)channel.TOT;
                    c.locationLat1 = 0;
                    c.locationLat2 = 0;
                    c.locationLon0 = 0;
                    c.locationLon1 = 0;
                    c.rxTone = (ushort)channel.RxTone;
                    c.txTone = (ushort)channel.TxTone;
                    c.locationLon2 = 0;
                    c._UNUSED_1 = 0;
                    c.LibreDMR_flag1 = 0;
                    c.dmrID2 = (byte)(channel.DMRId >> 16);
                    c.dmrID1 = (byte)(channel.DMRId >> 8);
                    c.dmrID0 = (byte)(channel.DMRId & 0xff);
                    c._UNUSED_2 = 0;
                    c.rxGroupList = (byte)(1 + groups.IndexOf(channel.rxGroup));
                    c.txColor = (byte)channel.Colorcode;
                    c.aprsSystem = 0;
                    c.contact = (ushort)(1 + contacts.IndexOf(channel.txContact));
                    c.flag1 = 0;
                    c.flag2 = 0;
                    c.flag3 = 0;
                    c.flag4 = 0;
                    c.reserve2 = 0;
                    c.reserve = 0;
                    c.sql = (byte)channel.Squelch;

                    if (channel.DMRId != 0)
                        c.LibreDMR_flag1 |= 128;

                    if (channel.Timeslot == TimeSlot.TS2)
                        c.flag2 |= 64;

                    if (channel.Bandwidth == ChannelBandwidth.Wide)
                        c.flag4 |= 2;
                    if (channel.RxOnly)
                        c.flag4 |= 4;
                    if (channel.AllSkip)
                        c.flag4 |= 16;
                    if (channel.ZoneSkip)
                        c.flag4 |= 32;

                    stream.Write(getBytes(c));
                }
                else
                {
                    // skip invalid channel
                    stream.Seek(0x38, SeekOrigin.Current);
                }
            }

            return res;
        }

        private static bool WriteContacts(Stream stream, List<Contact> contacts)
        {
            bool res = true;

            stream.Seek(CODEPLUG_ADDR_CONTACTS, SeekOrigin.Begin);
            using (var writer = new BinaryWriter(stream, Encoding.Latin1, true))
            {
                for (int i = 0; i < 1024; i++)
                {
                    var contact = contacts.ElementAtOrDefault(i);
                    if (contact != null)
                    {
                        WriteString(stream, contact.Name, 16);
                        WriteBCD(stream, (UInt32)contact.Number);
                        stream.WriteByte((byte)contact.Type);

                        // not used in firmware?
                        stream.WriteByte((byte)0xff);
                        stream.WriteByte((byte)0xff);

                        // read and parse flags
                        byte reserve1 = 0;
                        if (contact.OverrideTS == null)
                            reserve1 |= 1;
                        else
                            reserve1 |= (byte)((byte)contact.OverrideTS << 1);

                        stream.WriteByte(reserve1);
                    } else
                    {
                        // write empty contact
                        var ba = new byte[CODEPLUG_CONTACT_DATA_SIZE];
                        Array.Fill(ba, (byte)0xff);
                        stream.Write(ba);
                    }
                }
            }

            return res;
        }

        private static bool WriteContactGroups(Stream stream, IEnumerable<ContactGroup> groups, List<Contact> contacts)
        {
            // validate groups list
            if (groups.Count() > 76)
                return false;

            stream.Seek(CODEPLUG_ADDR_RX_GROUP, SeekOrigin.Begin);
            using (var writer = new BinaryWriter(stream, Encoding.Latin1, true))
            {
                // loop through all groups
                int groupsWritten = 0;
                foreach (var group in groups)
                {
                    WriteString(stream, group.Name, 16);

                    // validate contacts list
                    if (group.Contacts.Count() > 32)
                        return false;

                    // write out contact indexes
                    int contactsWritten = 0;
                    foreach (var contact in group.Contacts)
                    {
                        var idx = contacts.IndexOf(contact);
                        if (idx >= 0)
                        {
                            writer.Write((UInt16)(idx + 1));
                            contactsWritten++;
                        }
                    }

                    // write empty contacts to fill space
                    stream.Write(new byte[(32 - contactsWritten) * 2]);
                    groupsWritten++;
                }

                // write empty groups to fill space
                stream.Write(new byte[(76 - groupsWritten) * CODEPLUG_RXGROUP_DATA_STRUCT_SIZE]);
            }

            return true;
        }

        public static bool Save(Stream stream, GeneralSettings settings, List<Channel> channels, List<Zone> zones,
                                            List<Contact> contacts, List<ContactGroup> contactGroups)
        {
            bool err = false;
            try
            {
                // write general radio settings
                err = (err || !WriteGeneralSettings(stream, settings));

                // write eeprom channels
                stream.Seek(CODEPLUG_ADDR_CHANNEL_HEADER_EEPROM, SeekOrigin.Begin);
                err = (err || !WriteChannelBank(stream, channels, 0, contacts, contactGroups));

                // write zones
                err = (err || !WriteZones(stream, zones));

                // write flash channels
                stream.Seek(CODEPLUG_ADDR_CHANNEL_HEADER_FLASH, SeekOrigin.Begin);
                for (int nbank = 1; nbank < 8; nbank++)
                {
                    err = (err || !WriteChannelBank(stream, channels, nbank, contacts, contactGroups));
                }

                // Write contacts and groups
                err = (err || !WriteContacts(stream, contacts));
                err = (err || !WriteContactGroups(stream, contactGroups, contacts));

            } catch (Exception e ) {
                // Feed back exception info here?
                err = true;
            }

            return !err;
        }

    }
}
