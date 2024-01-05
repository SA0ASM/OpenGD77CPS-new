using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenGD77CPS.Models
{
    #region Public Enums

    public enum ChannelType
    {
        Analogue,
        Digital
    }

    public enum ChannelBandwidth
    {
        Narrow,
        Wide
    }

    public enum TimeSlot
    {
        TS1,
        TS2
    }

    public enum ChannelPower
    {
        Power_NoOverride,
        Power_50mW,
        Power_250mW,
        Power_500mW,
        Power_750mW,
        Power_1W,
        Power_2W,
        Power_3W,
        Power_4W,
        Power_5W,
        Power_5W_Plus,
    }

    public enum ChannelSquelch
    {
        No_Override,
        Open,
        p5 ,
        p10,
        p15,
        p20,
        p25,
        p30,
        p35,
        p40,
        p45,
        p50,
        p55,
        p60,
        p65,
        p70,
        p75,
        p80,
        p85,
        p90,
        p95,
        Closed
    }

    public enum ChannelTOT
    {
        No_TimeOut,
        TOT_15s,
        TOT_30s,
        TOT_45s,
        TOT_60s,
        TOT_75s,
        TOT_90s,
        TOT_105s,
        TOT_120s,
        TOT_135s,
        TOT_150s,
        TOT_165s,
        TOT_180s,
        TOT_195s,
        TOT_210s,
        TOT_225s,
        TOT_240s,
        TOT_255s,
        TOT_270s,
        TOT_285s,
        TOT_300s,
        TOT_315s,
        TOT_330s,
        TOT_345s,
        TOT_360s,
        TOT_375s,
        TOT_390s,
        TOT_405s,
        TOT_420s,
        TOT_435s,
        TOT_450s,
        TOT_465s,
        TOT_480s,
        TOT_495s,
    }


    public enum SubTone : ushort
    {
        None  = 0,
        C67_0 = 0x670,
        C69_3 = 0x693,
        C71_9 = 0x719,
        C74_4 = 0x744,
        C77_0 = 0x770,
        C79_7 = 0x797,
        C82_5 = 0x825,
        C85_4 = 0x854,
        C88_5 = 0x885,
        C91_5 = 0x915,
        C94_8 = 0x948,
        C97_4 = 0x974,
        C100_0 = 0x1000,
        C103_5 = 0x1035,
        C107_2 = 0x1072,
        C110_9 = 0x1109,
        C114_8 = 0x1148,
        C118_8 = 0x1188,
        C123_0 = 0x1230,
        C127_3 = 0x1273,
        C131_8 = 0x1318,
        C136_5 = 0x1365,
        C141_3 = 0x1413,
        C146_2 = 0x1462,
        C151_4 = 0x1514,
        C156_7 = 0x1567,
        C159_8 = 0x1598,
        C162_2 = 0x1622,
        C165_5 = 0x1655,
        C167_9 = 0x1679,
        C171_3 = 0x1713,
        C173_8 = 0x1738,
        C177_3 = 0x1773,
        C179_9 = 0x1799,
        C183_5 = 0x1835,
        C186_2 = 0x1862,
        C189_9 = 0x1899,
        C192_8 = 0x1928,
        C196_6 = 0x1966,
        C199_5 = 0x1995,
        C203_5 = 0x2035,
        C206_5 = 0x2065,
        C210_7 = 0x2107,
        C218_1 = 0x2181,
        C225_7 = 0x2257,
        C229_1 = 0x2291,
        C233_6 = 0x2336,
        C241_8 = 0x2418,
        C250_3 = 0x2503,
        C254_1 = 0x2541,
        D023N = 0x8000 + 0x023,
        D025N = 0x8000 + 0x025,
        D026N = 0x8000 + 0x026,
        D031N = 0x8000 + 0x031,
        D032N = 0x8000 + 0x032,
        D043N = 0x8000 + 0x043,
        D047N = 0x8000 + 0x047,
        D051N = 0x8000 + 0x051,
        D054N = 0x8000 + 0x054,
        D065N = 0x8000 + 0x065,
        D071N = 0x8000 + 0x071,
        D072N = 0x8000 + 0x072,
        D073N = 0x8000 + 0x073,
        D074N = 0x8000 + 0x074,
        D114N = 0x8000 + 0x114,
        D115N = 0x8000 + 0x115,
        D116N = 0x8000 + 0x116,
        D125N = 0x8000 + 0x125,
        D131N = 0x8000 + 0x131,
        D132N = 0x8000 + 0x132,
        D134N = 0x8000 + 0x134,
        D143N = 0x8000 + 0x143,
        D152N = 0x8000 + 0x152,
        D155N = 0x8000 + 0x155,
        D156N = 0x8000 + 0x156,
        D162N = 0x8000 + 0x162,
        D165N = 0x8000 + 0x165,
        D172N = 0x8000 + 0x172,
        D174N = 0x8000 + 0x174,
        D205N = 0x8000 + 0x205,
        D223N = 0x8000 + 0x223,
        D226N = 0x8000 + 0x226,
        D243N = 0x8000 + 0x243,
        D244N = 0x8000 + 0x244,
        D245N = 0x8000 + 0x245,
        D251N = 0x8000 + 0x251,
        D261N = 0x8000 + 0x261,
        D263N = 0x8000 + 0x263,
        D265N = 0x8000 + 0x265,
        D271N = 0x8000 + 0x271,
        D306N = 0x8000 + 0x306,
        D311N = 0x8000 + 0x311,
        D315N = 0x8000 + 0x315,
        D331N = 0x8000 + 0x331,
        D343N = 0x8000 + 0x343,
        D346N = 0x8000 + 0x346,
        D351N = 0x8000 + 0x351,
        D364N = 0x8000 + 0x364,
        D365N = 0x8000 + 0x365,
        D371N = 0x8000 + 0x371,
        D411N = 0x8000 + 0x411,
        D412N = 0x8000 + 0x412,
        D413N = 0x8000 + 0x413,
        D423N = 0x8000 + 0x423,
        D431N = 0x8000 + 0x431,
        D432N = 0x8000 + 0x432,
        D445N = 0x8000 + 0x445,
        D464N = 0x8000 + 0x464,
        D465N = 0x8000 + 0x465,
        D466N = 0x8000 + 0x466,
        D503N = 0x8000 + 0x503,
        D506N = 0x8000 + 0x506,
        D516N = 0x8000 + 0x516,
        D532N = 0x8000 + 0x532,
        D546N = 0x8000 + 0x546,
        D565N = 0x8000 + 0x565,
        D606N = 0x8000 + 0x606,
        D612N = 0x8000 + 0x612,
        D624N = 0x8000 + 0x624,
        D627N = 0x8000 + 0x627,
        D631N = 0x8000 + 0x631,
        D632N = 0x8000 + 0x632,
        D654N = 0x8000 + 0x654,
        D662N = 0x8000 + 0x662,
        D664N = 0x8000 + 0x664,
        D703N = 0x8000 + 0x703,
        D712N = 0x8000 + 0x712,
        D723N = 0x8000 + 0x723,
        D731N = 0x8000 + 0x731,
        D732N = 0x8000 + 0x732,
        D734N = 0x8000 + 0x734,
        D743N = 0x8000 + 0x743,
        D754N = 0x8000 + 0x754,
        D023I = 0xC000 + 0x023,
        D025I = 0xC000 + 0x025,
        D026I = 0xC000 + 0x026,
        D031I = 0xC000 + 0x031,
        D032I = 0xC000 + 0x032,
        D043I = 0xC000 + 0x043,
        D047I = 0xC000 + 0x047,
        D051I = 0xC000 + 0x051,
        D054I = 0xC000 + 0x054,
        D065I = 0xC000 + 0x065,
        D071I = 0xC000 + 0x071,
        D072I = 0xC000 + 0x072,
        D073I = 0xC000 + 0x073,
        D074I = 0xC000 + 0x074,
        D114I = 0xC000 + 0x114,
        D115I = 0xC000 + 0x115,
        D116I = 0xC000 + 0x116,
        D125I = 0xC000 + 0x125,
        D131I = 0xC000 + 0x131,
        D132I = 0xC000 + 0x132,
        D134I = 0xC000 + 0x134,
        D143I = 0xC000 + 0x143,
        D152I = 0xC000 + 0x152,
        D155I = 0xC000 + 0x155,
        D156I = 0xC000 + 0x156,
        D162I = 0xC000 + 0x162,
        D165I = 0xC000 + 0x165,
        D172I = 0xC000 + 0x172,
        D174I = 0xC000 + 0x174,
        D205I = 0xC000 + 0x205,
        D223I = 0xC000 + 0x223,
        D226I = 0xC000 + 0x226,
        D243I = 0xC000 + 0x243,
        D244I = 0xC000 + 0x244,
        D245I = 0xC000 + 0x245,
        D251I = 0xC000 + 0x251,
        D261I = 0xC000 + 0x261,
        D263I = 0xC000 + 0x263,
        D265I = 0xC000 + 0x265,
        D271I = 0xC000 + 0x271,
        D306I = 0xC000 + 0x306,
        D311I = 0xC000 + 0x311,
        D315I = 0xC000 + 0x315,
        D331I = 0xC000 + 0x331,
        D343I = 0xC000 + 0x343,
        D346I = 0xC000 + 0x346,
        D351I = 0xC000 + 0x351,
        D364I = 0xC000 + 0x364,
        D365I = 0xC000 + 0x365,
        D371I = 0xC000 + 0x371,
        D411I = 0xC000 + 0x411,
        D412I = 0xC000 + 0x412,
        D413I = 0xC000 + 0x413,
        D423I = 0xC000 + 0x423,
        D431I = 0xC000 + 0x431,
        D432I = 0xC000 + 0x432,
        D445I = 0xC000 + 0x445,
        D464I = 0xC000 + 0x464,
        D465I = 0xC000 + 0x465,
        D466I = 0xC000 + 0x466,
        D503I = 0xC000 + 0x503,
        D506I = 0xC000 + 0x506,
        D516I = 0xC000 + 0x516,
        D532I = 0xC000 + 0x532,
        D546I = 0xC000 + 0x546,
        D565I = 0xC000 + 0x565,
        D606I = 0xC000 + 0x606,
        D612I = 0xC000 + 0x612,
        D624I = 0xC000 + 0x624,
        D627I = 0xC000 + 0x627,
        D631I = 0xC000 + 0x631,
        D632I = 0xC000 + 0x632,
        D654I = 0xC000 + 0x654,
        D662I = 0xC000 + 0x662,
        D664I = 0xC000 + 0x664,
        D703I = 0xC000 + 0x703,
        D712I = 0xC000 + 0x712,
        D723I = 0xC000 + 0x723,
        D731I = 0xC000 + 0x731,
        D732I = 0xC000 + 0x732,
        D734I = 0xC000 + 0x734,
        D743I = 0xC000 + 0x743,
        D754I = 0xC000 + 0x754,
    }

    #endregion

    public class Channel : ObservableObject, IComparable<Channel>
    {
        static int max_name_len = 16;

        #region Fields

        int _number;
        String _name;
        ChannelType _type;
        UInt64 _rxfreq;
        UInt64 _txfreq;
        ChannelBandwidth _bw;
        int _cc;
        TimeSlot _ts;

        Contact? _txContact;
        ContactGroup? _rxGroup;
        int _dmrid;

        // TS1_TA_Tx
        // TS2_TA_Tx

        SubTone _rxtone;
        SubTone _txtone;

        ChannelSquelch _sql;
        ChannelPower _power;
        
        bool _rxonly = false;
        bool _zoneskip = false;
        bool _allskip = false;
        ChannelTOT _tot;
        bool _vox = false;
        bool _nobeep = false;
        bool _noeco = false;
        
        // APRS
        
        decimal _lon = 0;
        decimal _lat = 0;

        #endregion

        public Channel(String name, ChannelType type)
        {
            _name = name;
            _type = type;

            _rxfreq = _txfreq = 144000000;
            _bw = ChannelBandwidth.Narrow;
        }

        #region Public Properties

        public int Number
        {
            get { return _number; }
            // Read only property to prevent "accidental" changes
        }

        // Sort and unique Channel on the channel number property
        int IComparable<Channel>.CompareTo(Channel? other)
        {
            if (other != null)
                return this._number - other._number;
            return -1;
        }

        public String Name
        {
            get { return _name; }
            set {
                String n = value;

                // truncate names
                if (n.Length > max_name_len)
                    n = n.Substring(0, max_name_len);
                _name = n;
                RaisePropertyChanged("Name");
            }
        }

        public ChannelType Type
        {
            get { return _type;  }
            set {
                if (value != _type)
                {
                    _type = value;
                    RaisePropertyChanged("Type");
                }
            }
        }

        public String RxFrequency
        {
            get { return FreqToString(_rxfreq); }
            set
            {
                UInt64 f = FreqFromString(value);
                if (f != _rxfreq)
                {
                    _rxfreq = f;
                    RaisePropertyChanged("RxFrequency");
                }
            }
        }

        public String TxFrequency
        {
            get { return FreqToString(_txfreq); }
            set
            {
                UInt64 f = FreqFromString(value);
                if (f != _txfreq)
                {
                    _txfreq = f;
                    RaisePropertyChanged("TxFrequency");
                }
            }
        }

        public ChannelBandwidth Bandwidth
        {
            get { return _bw; }
            set
            {
                if (value != _bw)
                {
                    _bw = (ChannelBandwidth)value;
                    RaisePropertyChanged("Bandwidth");
                }
            }
        }
        public int Colorcode
        {
            get { return _cc; }
            set
            {
                if (value != _cc && value > -1 && value < 16)
                {
                    _cc = value;
                    RaisePropertyChanged("Colorcode");
                }
            }
        }
        public TimeSlot Timeslot
        {
            get { return _ts; }
            set
            {
                if (value != _ts)
                {
                    _ts = value;
                    RaisePropertyChanged("Timeslot");
                }
            }
        }

        public Contact? txContact
        {
            get { return _txContact; }
            set
            {
                _txContact = value;
                RaisePropertyChanged("txContact");
            }

        }

        public ContactGroup? rxGroup
        {
            get { return _rxGroup; }
            set
            {
                _rxGroup = value;
                RaisePropertyChanged("rxGroup");
            }

        }

        public int DMRId
        {
            get { return _dmrid; }
            set
            {
                if (value != _dmrid && value > -1 && value < 0xffffff)
                {
                    _dmrid = value;
                    RaisePropertyChanged("DMRId");
                }
            }
        }

        public SubTone RxTone
        {
            get { return _rxtone; }
            set
            {
                if (value != _rxtone)
                {
                    _rxtone = value;
                    RaisePropertyChanged("RxTone");
                }
            }
        }
        public SubTone TxTone
        {
            get { return _txtone; }
            set
            {
                if (value != _txtone)
                {
                    _txtone = value;
                    RaisePropertyChanged("TxTone");
                }
            }
        }

        public ChannelSquelch Squelch
        {
            get { return _sql; }
            set
            {
                if (value != _sql)
                {
                    _sql = value;
                    RaisePropertyChanged("Squelch");
                }
            }
        }

        public ChannelPower Power
        {
            get { return _power; }
            set
            {
                if (value != _power)
                {
                    _power = value;
                    RaisePropertyChanged("Power");
                }
            }
        }


        public bool RxOnly
        {
            get { return _rxonly; }
            set
            {
                if (value != _rxonly)
                {
                    _rxonly = value;
                    RaisePropertyChanged("RxOnly");
                }
            }
        }
        public bool ZoneSkip
        {
            get { return _zoneskip; }
            set
            {
                if (value != _zoneskip)
                {
                    _zoneskip = value;
                    RaisePropertyChanged("ZoneSkip");
                }
            }
        }
        public bool AllSkip
        {
            get { return _allskip; }
            set
            {
                if (value != _allskip)
                {
                    _allskip = value;
                    RaisePropertyChanged("AllSkip");
                }
            }
        }

        public ChannelTOT TOT
        {
            get { return _tot; }
            set
            {
                if (value != _tot)
                {
                    _tot = value;
                    RaisePropertyChanged("TOT");
                }
            }
        }

        public bool Vox
        {
            get { return _vox; }
            set
            {
                if (value != _vox)
                {
                    _vox = value;
                    RaisePropertyChanged("Vox");
                }
            }
        }

        public bool NoBeep
        {
            get { return _nobeep; }
            set
            {
                if (value != _nobeep)
                {
                    _nobeep = value;
                    RaisePropertyChanged("NoBeep");
                }
            }
        }

        public bool NoEco
        {
            get { return _noeco; }
            set
            {
                if (value != _noeco)
                {
                    _noeco = value;
                    RaisePropertyChanged("NoEco");
                }
            }
        }

        public decimal Latitude
        {
            get { return _lat; }
            set
            {
                if (value != _lat)
                {
                    _lat = value;
                    RaisePropertyChanged("Latitude");
                }
            }
        }

        public decimal Longitude
        {
            get { return _lon; }
            set
            {
                if (value != _lon)
                {
                    _lon = value;
                    RaisePropertyChanged("Longitude");
                }
            }
        }

        #endregion

        #region Private Helpers

        private String FreqToString(UInt64 freq)
        {
            // show frequencies with 5 decimal precision
            return String.Format("{0}.{1,5:D5}", freq / 1000000, (freq % 1000000) / 10);
        }

        private UInt64 FreqFromString(String str)
        {
            UInt64 f = 0;
            if (str.Contains('.') || str.Contains(','))
            {
                // parse decimal string
                var v = str.Split(['.', ',']);

                // discard all but 5 decimal places
                if (v[1].Length > 4)
                    v[1] = v[1].Substring(0, 5);
                
                f = UInt64.Parse(v[0] + v[1].PadRight(6, '0'));
            } else
            {
                UInt64.TryParse(str, out f); 
            }

            // Clamp values to 100Mhz to 1Ghz
            if (f > 1000000000 || f < 100000000)
                f = 0;
            return f;
        }

        #endregion

        // Exposes Channel Number for writing
        public void SetNumber(int num)
        {
            _number = num;
        }

        // Exposes frequencies in hertz
        public UInt64 GetRxFrequency()
        {
            return _rxfreq;
        }
        public UInt64 GetTxFrequency()
        {
            return _txfreq;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
