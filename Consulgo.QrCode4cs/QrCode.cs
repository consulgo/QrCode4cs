//---------------------------------------------------------------------
// QRCode for C#4.0 is translation of QRCode for JavaScript
// https://github.com/jeromeetienne/jquery-qrcode/
//
// Copyright (c) 2009 Kazuhiko Arase
//
// URL: http://www.d-project.com/
//
// Licensed under the MIT license:
//   http://www.opensource.org/licenses/mit-license.php
//
// The word "QR Code" is registered trademark of 
// DENSO WAVE INCORPORATED
//   http://www.denso-wave.com/qrcode/faqpatent-e.html
//
// This code initially was published here:
// https://qrcode4cs.codeplex.com/
//---------------------------------------------------------------------
namespace Consulgo.QrCode4cs
{
    using System;
    using System.Collections.Generic;

    public enum ErrorCode
    {
        UnknownError = 0,
        UnknownMode = 1,
        BadMaskPattern = 2,
        CodeLengthOverflow = 4,
        BadBlock = 8,

    }

    public class ErrorException : Exception
    {
        public ErrorCode Code { get; private set; }
        public ErrorException() { }
        public ErrorException(string message) : base(message) { }
        public ErrorException(string message, Exception inner) : base(message, inner) { }
        public ErrorException(string message, ErrorCode code)
            : base(message)
        {
            Code = code;
        }
    }

    public enum QRMode : int
    {
        MODE_NUMBER = 1 << 0,
        MODE_ALPHA_NUM = 1 << 1,
        MODE_8BIT_BYTE = 1 << 2,
        MODE_KANJI = 1 << 3
    }

    public enum QRErrorCorrectLevel : int
    {
        L = 1,
        M = 0,
        Q = 3,
        H = 2
    }

    public enum QRMaskPattern : int
    {
        PATTERN000 = 0,
        PATTERN001 = 1,
        PATTERN010 = 2,
        PATTERN011 = 3,
        PATTERN100 = 4,
        PATTERN101 = 5,
        PATTERN110 = 6,
        PATTERN111 = 7
    }

    public struct Options
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public QRErrorCorrectLevel CorrectLevel { get; set; }
        public int TypeNumber { get; set; }
        public string Text { get; set; }

        public Options(string text)
            : this()
        {
            Width = 256;
            Height = 256;
            TypeNumber = 4;
            CorrectLevel = QRErrorCorrectLevel.H;
            Text = text;
        }
    }

    internal static class QRUtil
    {
        internal const int G15 = (1 << 10) | (1 << 8) | (1 << 5) | (1 << 4) | (1 << 2) | (1 << 1) | (1 << 0);
        internal const int G18 = (1 << 12) | (1 << 11) | (1 << 10) | (1 << 9) | (1 << 8) | (1 << 5) | (1 << 2) | (1 << 0);
        internal const int G15_MASK = (1 << 14) | (1 << 12) | (1 << 10) | (1 << 4) | (1 << 1);
        internal static readonly int[][] PATTERN_POSITION_TABLE = new int[][] {
            new int[] {},
	        new int [] {6, 18},
	        new int [] {6, 22},
	        new int [] {6, 26},
	        new int [] {6, 30},
	        new int [] {6, 34},
	        new int [] {6, 22, 38},
	        new int [] {6, 24, 42},
	        new int [] {6, 26, 46},
	        new int [] {6, 28, 50},
	        new int [] {6, 30, 54},
	        new int [] {6, 32, 58},
	        new int [] {6, 34, 62},
	        new int [] {6, 26, 46, 66},
	        new int [] {6, 26, 48, 70},
	        new int [] {6, 26, 50, 74},
	        new int [] {6, 30, 54, 78},
	        new int [] {6, 30, 56, 82},
	        new int [] {6, 30, 58, 86},
	        new int [] {6, 34, 62, 90},
	        new int [] {6, 28, 50, 72, 94},
	        new int [] {6, 26, 50, 74, 98},
	        new int [] {6, 30, 54, 78, 102},
	        new int [] {6, 28, 54, 80, 106},
	        new int [] {6, 32, 58, 84, 110},
	        new int [] {6, 30, 58, 86, 114},
	        new int [] {6, 34, 62, 90, 118},
	        new int [] {6, 26, 50, 74, 98, 122},
	        new int [] {6, 30, 54, 78, 102, 126},
	        new int [] {6, 26, 52, 78, 104, 130},
	        new int [] {6, 30, 56, 82, 108, 134},
	        new int [] {6, 34, 60, 86, 112, 138},

	        new int [] {6, 30, 58, 86, 114, 142},
	        new int [] {6, 34, 62, 90, 118, 146},
	        new int [] {6, 30, 54, 78, 102, 126, 150},
	        new int [] {6, 24, 50, 76, 102, 128, 154},
	        new int [] {6, 28, 54, 80, 106, 132, 158},
	        new int [] {6, 32, 58, 84, 110, 136, 162},
	        new int [] {6, 26, 54, 82, 110, 138, 166},
	        new int [] {6, 30, 58, 86, 114, 142, 170}
        };
        internal static int GetLengthInBits(QRMode mode, int type)
        {

            if (1 <= type && type < 10)
            {
                // 1 - 9
                switch (mode)
                {
                    case QRMode.MODE_NUMBER: return 10;
                    case QRMode.MODE_ALPHA_NUM: return 9;
                    case QRMode.MODE_8BIT_BYTE: return 8;
                    case QRMode.MODE_KANJI: return 8;
                    default:
                        throw new ErrorException("mode:" + mode, ErrorCode.UnknownMode);
                }
            }
            else if (type < 27)
            {
                // 10 - 26
                switch (mode)
                {
                    case QRMode.MODE_NUMBER: return 12;
                    case QRMode.MODE_ALPHA_NUM: return 11;
                    case QRMode.MODE_8BIT_BYTE: return 16;
                    case QRMode.MODE_KANJI: return 10;
                    default:
                        throw new ErrorException("mode:" + mode, ErrorCode.UnknownMode);
                }
            }
            else if (type < 41)
            {
                // 27 - 40
                switch (mode)
                {
                    case QRMode.MODE_NUMBER: return 14;
                    case QRMode.MODE_ALPHA_NUM: return 13;
                    case QRMode.MODE_8BIT_BYTE: return 16;
                    case QRMode.MODE_KANJI: return 12;
                    default:
                        throw new ErrorException("mode:" + mode, ErrorCode.UnknownMode);
                }
            }
            else
            {
                throw new ErrorException("type:" + type, ErrorCode.UnknownMode);
            }
        }

        internal static double GetLostPoint(QRCode qrCode)
        {
            var moduleCount = qrCode.GetModuleCount();
            var lostPoint = 0.0;

            for (var row = 0; row < moduleCount; row++)
            {
                for (var col = 0; col < moduleCount; col++)
                {

                    var sameCount = 0;
                    var dark = qrCode.IsDark(row, col);

                    for (var r = -1; r <= 1; r++)
                    {

                        if (row + r < 0 || moduleCount <= row + r)
                        {
                            continue;
                        }

                        for (var c = -1; c <= 1; c++)
                        {

                            if (col + c < 0 || moduleCount <= col + c)
                            {
                                continue;
                            }

                            if (r == 0 && c == 0)
                            {
                                continue;
                            }

                            if (dark == qrCode.IsDark((int)((int)row + r), (int)((int)col + c)))
                            {
                                sameCount++;
                            }
                        }
                    }

                    if (sameCount > 5)
                    {
                        lostPoint += (3 + sameCount - 5);
                    }
                }
            }

            // LEVEL2

            for (var row = 0; row < moduleCount - 1; row++)
            {
                for (var col = 0; col < moduleCount - 1; col++)
                {
                    var count = 0;

                    if (qrCode.IsDark(row, col))
                    {
                        count++;
                    }

                    if (qrCode.IsDark(row + 1, col))
                    {
                        count++;
                    }

                    if (qrCode.IsDark(row, col + 1))
                    {
                        count++;
                    }

                    if (qrCode.IsDark(row + 1, col + 1))
                    {
                        count++;
                    }

                    if (count == 0 || count == 4)
                    {
                        lostPoint += 3;
                    }
                }
            }

            // LEVEL3

            for (var row = 0; row < moduleCount; row++)
            {
                for (var col = 0; col < moduleCount - 6; col++)
                {
                    if (qrCode.IsDark(row, col)
                            && !qrCode.IsDark(row, col + 1)
                            && qrCode.IsDark(row, col + 2)
                            && qrCode.IsDark(row, col + 3)
                            && qrCode.IsDark(row, col + 4)
                            && !qrCode.IsDark(row, col + 5)
                            && qrCode.IsDark(row, col + 6))
                    {
                        lostPoint += 40.0;
                    }
                }
            }

            for (var col = 0; col < moduleCount; col++)
            {
                for (var row = 0; row < moduleCount - 6; row++)
                {
                    if (qrCode.IsDark(row, col)
                            && !qrCode.IsDark(row + 1, col)
                            && qrCode.IsDark(row + 2, col)
                            && qrCode.IsDark(row + 3, col)
                            && qrCode.IsDark(row + 4, col)
                            && !qrCode.IsDark(row + 5, col)
                            && qrCode.IsDark(row + 6, col))
                    {
                        lostPoint += 40;
                    }
                }
            }

            // LEVEL4
            var darkCount = 0;

            for (var col = 0; col < moduleCount; col++)
            {
                for (var row = 0; row < moduleCount; row++)
                {
                    if (qrCode.IsDark(row, col))
                    {
                        darkCount++;
                    }
                }
            }

            var ratio = Math.Abs(100.0 * Convert.ToDouble(darkCount) / Convert.ToDouble(moduleCount) / Convert.ToDouble(moduleCount) - 50.0) / 5.0;
            lostPoint += ratio * 10.0;
            return lostPoint;
        }

        internal static int GetBCHTypeInfo(int data)
        {
            var d = (data << 10);
            var s = 0;

            while ((s = (int)(QRUtil.GetBCHDigit(d) - QRUtil.GetBCHDigit(QRUtil.G15))) >= 0)
            {
                d ^= (Convert.ToInt32(QRUtil.G15) << s);
            }

            return ((data << 10) | d) ^ QRUtil.G15_MASK;
        }

        internal static int GetBCHTypeNumber(int data)
        {
            var d = data << 12;

            while (QRUtil.GetBCHDigit(d) - QRUtil.GetBCHDigit(QRUtil.G18) >= 0)
            {
                d ^= (QRUtil.G18 << (QRUtil.GetBCHDigit(d) - QRUtil.GetBCHDigit(QRUtil.G18)));
            }

            return (data << 12) | d;
        }

        internal static int GetBCHDigit(int dataInt)
        {
            var digit = 0;
            var data = Convert.ToUInt32(dataInt);

            while (data != 0)
            {
                digit++;
                data >>= 1;
            }

            return digit;
        }

        internal static int[] GetPatternPosition(int typeNumber)
        {
            return QRUtil.PATTERN_POSITION_TABLE[typeNumber - 1];
        }

        internal static bool GetMask(QRMaskPattern maskPattern, int i, int j)
        {
            switch (maskPattern)
            {

                case QRMaskPattern.PATTERN000: return (i + j) % 2 == 0;
                case QRMaskPattern.PATTERN001: return i % 2 == 0;
                case QRMaskPattern.PATTERN010: return j % 3 == 0;
                case QRMaskPattern.PATTERN011: return (i + j) % 3 == 0;
                case QRMaskPattern.PATTERN100: return (Math.Floor(Convert.ToDouble(i) / 2.0) + Math.Floor(Convert.ToDouble(j) / 3.0)) % 2 == 0;
                case QRMaskPattern.PATTERN101: return (i * j) % 2 + (i * j) % 3 == 0;
                case QRMaskPattern.PATTERN110: return ((i * j) % 2 + (i * j) % 3) % 2 == 0;
                case QRMaskPattern.PATTERN111: return ((i * j) % 3 + (i + j) % 2) % 2 == 0;

                default:
                    throw new ErrorException("bad maskPattern:" + maskPattern, ErrorCode.BadMaskPattern);
            }
        }

        internal static QRPolynomial GetErrorCorrectPolynomial(int errorCorrectLength)
        {
            var a = new QRPolynomial(new DataCache() { 1 }, 0);

            for (var i = 0; i < errorCorrectLength; i++)
            {
                a = a.Multiply(new QRPolynomial(new DataCache() { 1, QRMath.GExp(i) }, 0));
            }

            return a;
        }
    }

    internal struct QRPolynomial
    {
        private int[] _num;

        public QRPolynomial(DataCache num, int shift)
            : this()
        {
            if (num == null)
            {
                throw new ArgumentNullException("num");
            }

            var offset = 0;

            while (offset < num.Count && num[offset] == 0)
            {
                offset++;
            }

            _num = new int[num.Count - offset + shift];

            for (var i = 0; i < num.Count - offset; i++)
            {
                _num[i] = num[i + offset];
            }
        }

        public int Get(int index)
        {
            return _num[index];
        }

        public int GetLength()
        {
            return _num.Length;
        }

        public QRPolynomial Multiply(QRPolynomial e)
        {
            var num = new DataCache(GetLength() + e.GetLength() - 1);

            for (var i = 0; i < GetLength(); i++)
            {
                for (var j = 0; j < e.GetLength(); j++)
                {
                    num[i + j] ^= QRMath.GExp(QRMath.GLog(Get(i)) + QRMath.GLog(e.Get(j)));
                }
            }

            return new QRPolynomial(num, 0);
        }

        public QRPolynomial Mod(QRPolynomial e)
        {
            if (Convert.ToInt64(GetLength()) - Convert.ToInt64(e.GetLength()) < 0L)
            {
                return this;
            }

            var ratio = QRMath.GLog(Get(0)) - QRMath.GLog(e.Get(0));
            var num = new DataCache(GetLength());

            for (var i = 0; i < GetLength(); i++)
            {
                num[i] = Get(i);
            }

            for (var i = 0; i < e.GetLength(); i++)
            {
                num[i] ^= QRMath.GExp(QRMath.GLog(e.Get(i)) + ratio);
            }

            // recursive call
            return new QRPolynomial(num, 0).Mod(e);
        }
    }


    internal static class QRMath
    {
        private static readonly int[] _expTable;
        private static readonly int[] _logTable;

        static QRMath()
        {
            _expTable = new int[256];
            _logTable = new int[256];

            for (var i = 0; i < 8; i++)
            {
                QRMath._expTable[i] = (1 << i);
            }

            for (var i = 8; i < 256; i++)
            {
                QRMath._expTable[i] = QRMath._expTable[i - 4]
                    ^ QRMath._expTable[i - 5]
                    ^ QRMath._expTable[i - 6]
                    ^ QRMath._expTable[i - 8];
            }

            for (var i = 0; i < 255; i++)
            {
                QRMath._logTable[QRMath._expTable[i]] = i;
            }
        }

        internal static int GLog(int n)
        {
            if (n < 1)
            {
                throw new ArgumentOutOfRangeException("n");
            }

            return QRMath._logTable[n];
        }

        internal static int GExp(int n)
        {
            while (n < 0)
            {
                n += 255;
            }

            while (n >= 256)
            {
                n -= 255;
            }

            return QRMath._expTable[n];
        }
    }

    public struct QR8bitByte
    {
        public QRMode Mode { get; private set; }
        private string _data { get; set; }

        public QR8bitByte(string data)
            : this()
        {
            _data = data;
            Mode = QRMode.MODE_8BIT_BYTE;
        }

        public int Length
        {
            get
            {
                return _data.Length;
            }
        }

        public void Write(QRBitBuffer buffer)
        {
            for (var i = 0; i < _data.Length; ++i)
            {
                //item
                buffer.Put(_data[i], 8);
            }
            ///buffer = Data;
        }
    }

    internal class DataCache : List<int>
    {
        public DataCache(int capacity)
            : base()
        {
            for (int i = 0; i < capacity; i++)
            {
                base.Add(0);
            }
        }

        public DataCache()
            : base()
        {

        }
    }

    internal struct QRRSBlock
    {
        private static readonly int[][] RS_BLOCK_TABLE = new int[][] {
            // L
            // M
            // Q
            // H

            // 1
	            new int [] {1, 26, 19},
	            new int [] {1, 26, 16},
	            new int [] {1, 26, 13},
	            new int [] {1, 26, 9},

            // 2
	            new int [] {1, 44, 34},
	            new int [] {1, 44, 28},
	            new int [] {1, 44, 22},
	            new int [] {1, 44, 16},

            // 3
	            new int [] {1, 70, 55},
	            new int [] {1, 70, 44},
	            new int [] {2, 35, 17},
	            new int [] {2, 35, 13},

            // 4		
	            new int [] {1, 100, 80},
	            new int [] {2, 50, 32},
	            new int [] {2, 50, 24},
	            new int [] {4, 25, 9},

            // 5
	            new int [] {1, 134, 108},
	            new int [] {2, 67, 43},
	            new int [] {2, 33, 15, 2, 34, 16},
	            new int [] {2, 33, 11, 2, 34, 12},

            // 6
	            new int [] {2, 86, 68},
	            new int [] {4, 43, 27},
	            new int [] {4, 43, 19},
	            new int [] {4, 43, 15},

            // 7		
	            new int [] {2, 98, 78},
	            new int [] {4, 49, 31},
	            new int [] {2, 32, 14, 4, 33, 15},
	            new int [] {4, 39, 13, 1, 40, 14},

            // 8
	            new int [] {2, 121, 97},
	            new int [] {2, 60, 38, 2, 61, 39},
	            new int [] {4, 40, 18, 2, 41, 19},
	            new int [] {4, 40, 14, 2, 41, 15},

            // 9
	            new int [] {2, 146, 116},
	            new int [] {3, 58, 36, 2, 59, 37},
	            new int [] {4, 36, 16, 4, 37, 17},
	            new int [] {4, 36, 12, 4, 37, 13},

            // 10		
	            new int [] {2, 86, 68, 2, 87, 69},
	            new int [] {4, 69, 43, 1, 70, 44},
	            new int [] {6, 43, 19, 2, 44, 20},
	            new int [] {6, 43, 15, 2, 44, 16}
                };


        public int DataCount { get; private set; }
        public int TotalCount { get; set; }

        public QRRSBlock(int totalCount, int dataCount)
            : this()
        {
            TotalCount = totalCount;
            DataCount = dataCount;
        }

        public static List<QRRSBlock> GetRSBlocks(int typeNumber, QRErrorCorrectLevel errorCorrectLevel)
        {
            var rsBlock = GetRsBlockTable(typeNumber, errorCorrectLevel);

            if (rsBlock == null)
            {
                throw new ErrorException(string.Format("Bad rs block @ typeNumber: {0} and /errorCorrectLevel: {1}", typeNumber, errorCorrectLevel), ErrorCode.BadBlock);
            }

            var length = rsBlock.Length / 3;
            var list = new List<QRRSBlock>();

            for (var i = 0; i < length; i++)
            {
                var count = rsBlock[i * 3 + 0];
                var totalCount = rsBlock[i * 3 + 1];
                var dataCount = rsBlock[i * 3 + 2];

                for (var j = 0; j < count; j++)
                {
                    list.Add(new QRRSBlock(totalCount, dataCount));
                }
            }

            return list;
        }

        private static int[] GetRsBlockTable(int typeNumber, QRErrorCorrectLevel errorCorrectLevel)
        {
            switch (errorCorrectLevel)
            {
                case QRErrorCorrectLevel.L:
                    return QRRSBlock.RS_BLOCK_TABLE[(typeNumber - 1) * 4 + 0];
                case QRErrorCorrectLevel.M:
                    return QRRSBlock.RS_BLOCK_TABLE[(typeNumber - 1) * 4 + 1];
                case QRErrorCorrectLevel.Q:
                    return QRRSBlock.RS_BLOCK_TABLE[(typeNumber - 1) * 4 + 2];
                case QRErrorCorrectLevel.H:
                    return QRRSBlock.RS_BLOCK_TABLE[(typeNumber - 1) * 4 + 3];
                default:
                    return null;
            }
        }
    }

    public class QRCode
    {
        private const int PAD0 = 0xEC;
        private const int PAD1 = 0x11;
        private List<QR8bitByte> _dataList = new List<QR8bitByte>();
        private int _typeNumber;
        private DataCache _dataCache;
        private int _moduleCount;
        private bool?[][] _modules;
        private QRErrorCorrectLevel _errorCorrectLevel;

        public QRCode(Options options)
            : this(options.TypeNumber, options.CorrectLevel)
        {
            AddData(options.Text);
        }

        public QRCode(int typeNumber, QRErrorCorrectLevel level)
        {
            _typeNumber = typeNumber;
            _errorCorrectLevel = level;
            _dataCache = null;
        }

        public void AddData(string data)
        {
            _dataCache = null;
            _dataList.Add(new QR8bitByte(data));
        }

        public void Make()
        {
            MakeImpl(false, GetBestMaskPattern());
        }

        private QRMaskPattern GetBestMaskPattern()
        {
            var minLostPoint = 0.0;
            var pattern = QRMaskPattern.PATTERN000;

            for (var i = 0; i < 8; i++)
            {
                MakeImpl(true, (QRMaskPattern)i);
                var lostPoint = QRUtil.GetLostPoint(this);

                if (i == 0 || minLostPoint > lostPoint)
                {
                    minLostPoint = lostPoint;
                    pattern = (QRMaskPattern)i;
                }
            }

            return pattern;
        }

        private void MakeImpl(bool test, QRMaskPattern maskPattern)
        {
            _moduleCount = _typeNumber * 4 + 17;
            _modules = new bool?[_moduleCount][];

            for (int row = 0; row < _moduleCount; row++)
            {
                _modules[row] = new bool?[(_moduleCount)];

                for (var col = 0; col < _moduleCount; col++)
                {
                    _modules[row][col] = null; //(col + row) % 3;
                }
            }

            SetupPositionProbePattern(0, 0);
            SetupPositionProbePattern(_moduleCount - 7, 0);
            SetupPositionProbePattern(0, _moduleCount - 7);
            SetupPositionAdjustPattern();
            SetupTimingPattern();
            SetupTypeInfo(test, maskPattern);

            if (_typeNumber >= 7)
            {
                SetupTypeNumber(test);
            }

            if (_dataCache == null)
            {
                _dataCache = CreateData(_typeNumber, _errorCorrectLevel, _dataList);
            }

            MapData(_dataCache, maskPattern);
        }

        public bool IsDark(int row, int col)
        {
            return _modules[row][col].Value;
        }

        private void SetupTimingPattern()
        {
            for (var r = 8; r < _moduleCount - 8; r++)
            {
                if (_modules[r][6] != null)
                {
                    continue;
                }

                _modules[r][6] = (r % 2 == 0);
            }

            for (var c = 8; c < _moduleCount - 8; c++)
            {
                if (_modules[6][c] != null)
                {
                    continue;
                }

                _modules[6][c] = (c % 2 == 0);
            }
        }

        private void SetupTypeNumber(bool test)
        {
            var bits = QRUtil.GetBCHTypeNumber(_typeNumber);

            for (var i = 0; i < 18; i++)
            {
                var mod = (!test && ((bits >> i) & 1) == 1);
                _modules[(int)Math.Floor(Convert.ToDouble(i) / 3.0)][i % 3 + _moduleCount - 8 - 3] = mod;
            }

            for (var i = 0; i < 18; i++)
            {
                var mod = (!test && ((bits >> i) & 1) == 1);
                _modules[i % 3 + _moduleCount - 8 - 3][(int)Math.Floor(Convert.ToDouble(i) / 3.0)] = mod;
            }
        }

        private void SetupPositionAdjustPattern()
        {
            var pos = QRUtil.GetPatternPosition(_typeNumber);

            for (var i = 0; i < pos.Length; i++)
            {
                for (var j = 0; j < pos.Length; j++)
                {
                    var row = pos[i];
                    var col = pos[j];

                    if (_modules[row][col] != null)
                    {
                        continue;
                    }

                    for (var r = -2; r <= 2; r++)
                    {
                        for (var c = -2; c <= 2; c++)
                        {
                            if (r == -2 || r == 2 || c == -2 || c == 2
                                    || (r == 0 && c == 0))
                            {
                                _modules[row + r][col + c] = true;
                            }
                            else
                            {
                                _modules[row + r][col + c] = false;
                            }
                        }
                    }
                }
            }
        }

        private void SetupTypeInfo(bool test, QRMaskPattern maskPattern)
        {
            var data = ((int)_errorCorrectLevel << 3) | (int)maskPattern;
            var bits = QRUtil.GetBCHTypeInfo(data);

            // vertical		
            for (var i = 0; i < 15; i++)
            {
                var mod = (!test && ((bits >> i) & 1) == 1);

                if (i < 6)
                {
                    _modules[i][8] = mod;
                }
                else if (i < 8)
                {
                    _modules[i + 1][8] = mod;
                }
                else
                {
                    _modules[_moduleCount - 15 + i][8] = mod;
                }
            }

            // horizontal
            for (var i = 0; i < 15; i++)
            {
                var mod = (!test && ((bits >> i) & 1) == 1);

                if (i < 8)
                {
                    _modules[8][_moduleCount - i - 1] = mod;
                }
                else if (i < 9)
                {
                    _modules[8][15 - i - 1 + 1] = mod;
                }
                else
                {
                    _modules[8][15 - i - 1] = mod;
                }
            }

            // fixed module
            _modules[_moduleCount - 8][8] = (!test);
        }

        private void MapData(DataCache data, QRMaskPattern maskPattern)
        {
            var inc = -1;
            var row = _moduleCount - 1;
            var bitIndex = 7;
            var byteIndex = 0;

            for (var col = _moduleCount - 1; col > 0; col -= 2)
            {
                if (col == 6)
                {
                    col--;
                }

                while (true)
                {
                    for (int c = 0; c < 2; c++)
                    {
                        if (_modules[row][col - c] == null)
                        {
                            var dark = false;

                            if (byteIndex < data.Count)
                            {
                                dark = (((Convert.ToUInt32(data[byteIndex]) >> bitIndex) & 1) == 1);
                            }

                            var mask = QRUtil.GetMask(maskPattern, row, col - c);

                            if (mask)
                            {
                                dark = !dark;
                            }

                            _modules[row][col - c] = dark;
                            bitIndex--;

                            if (bitIndex == -1)
                            {
                                byteIndex++;
                                bitIndex = 7;
                            }
                        }
                    }

                    row += inc;

                    if (row < 0 || _moduleCount <= row)
                    {
                        row -= inc;
                        inc = -inc;
                        break;
                    }
                }
            }
        }

        private DataCache CreateData(int typeNumber, QRErrorCorrectLevel errorCorrectLevel, List<QR8bitByte> dataList)
        {
            var rsBlocks = QRRSBlock.GetRSBlocks(typeNumber, errorCorrectLevel);
            var buffer = new QRBitBuffer();

            for (var i = 0; i < dataList.Count; i++)
            {
                QR8bitByte data = dataList[i];
                buffer.Put((int)data.Mode, 4);
                buffer.Put(data.Length, QRUtil.GetLengthInBits(data.Mode, typeNumber));
                data.Write(buffer);
            }

            // calc num max data.
            int totalDataCount = 0;

            for (var i = 0; i < rsBlocks.Count; i++)
            {
                totalDataCount += rsBlocks[i].DataCount;
            }

            if (buffer.GetLengthInBits() > totalDataCount * 8)
            {
                throw new ErrorException(string.Format(
                    "code length overflow ({0} > {1})",
                    buffer.GetLengthInBits(),
                    totalDataCount * 8), ErrorCode.CodeLengthOverflow);
            }

            // end code
            if (buffer.GetLengthInBits() + 4 <= totalDataCount * 8)
            {
                buffer.Put(0, 4);
            }

            // padding
            while (buffer.GetLengthInBits() % 8 != 0)
            {
                buffer.PutBit(false);
            }

            // padding
            while (true)
            {
                if (buffer.GetLengthInBits() >= totalDataCount * 8)
                {
                    break;
                }

                buffer.Put(QRCode.PAD0, 8);

                if (buffer.GetLengthInBits() >= totalDataCount * 8)
                {
                    break;
                }

                buffer.Put(QRCode.PAD1, 8);
            }

            return CreateBytes(buffer, rsBlocks);
        }

        private DataCache CreateBytes(QRBitBuffer buffer, List<QRRSBlock> rsBlocks)
        {
            var offset = 0;
            var maxDcCount = 0;
            var maxEcCount = 0;
            var dcdata = new DataCache[(rsBlocks.Count)];
            var ecdata = new DataCache[(rsBlocks.Count)];

            for (var r = 0; r < rsBlocks.Count; r++)
            {
                var dcCount = rsBlocks[r].DataCount;
                var ecCount = rsBlocks[r].TotalCount - dcCount;
                maxDcCount = Math.Max(maxDcCount, dcCount);
                maxEcCount = Math.Max(maxEcCount, ecCount);
                dcdata[r] = new DataCache(dcCount);

                for (var i = 0; i < dcdata[r].Count; i++)
                {
                    dcdata[r][i] = 0xff & buffer._buffer[i + offset];
                }

                offset += dcCount;
                var rsPoly = QRUtil.GetErrorCorrectPolynomial(ecCount);
                var rawPoly = new QRPolynomial(dcdata[r], rsPoly.GetLength() - 1);
                var modPoly = rawPoly.Mod(rsPoly);
                ecdata[r] = new DataCache(rsPoly.GetLength() - 1);

                for (var i = 0; i < ecdata[r].Count; i++)
                {
                    int modIndex = i + modPoly.GetLength() - ecdata[r].Count;
                    ecdata[r][i] = (modIndex >= 0) ? modPoly.Get(modIndex) : 0;
                }
            }

            var totalCodeCount = 0;

            for (var i = 0; i < rsBlocks.Count; i++)
            {
                totalCodeCount += rsBlocks[i].TotalCount;
            }

            var data = new DataCache(totalCodeCount);
            var index = 0;

            for (var i = 0; i < maxDcCount; i++)
            {
                for (var r = 0; r < rsBlocks.Count; r++)
                {
                    if (i < dcdata[r].Count)
                    {
                        data[index++] = dcdata[r][i];
                    }
                }
            }

            for (var i = 0; i < maxEcCount; i++)
            {
                for (var r = 0; r < rsBlocks.Count; r++)
                {
                    if (i < ecdata[r].Count)
                    {
                        data[index++] = ecdata[r][i];
                    }
                }
            }

            return data;
        }

        private void SetupPositionProbePattern(int row, int col)
        {
            for (var r = -1; r <= 7; r++)
            {
                if (row + r <= -1 || _moduleCount <= row + r)
                {
                    continue;
                }

                for (var c = -1; c <= 7; c++)
                {
                    if (col + c <= -1 || _moduleCount <= col + c)
                    {
                        continue;
                    }

                    if ((0 <= r && r <= 6 && (c == 0 || c == 6))
                            || (0 <= c && c <= 6 && (r == 0 || r == 6))
                            || (2 <= r && r <= 4 && 2 <= c && c <= 4))
                    {
                        _modules[row + r][col + c] = true;
                    }
                    else
                    {
                        _modules[row + r][col + c] = false;
                    }
                }
            }
        }

        public int GetModuleCount()
        {
            return _moduleCount;
        }
    }

    public class QRBitBuffer
    {
        internal List<int> _buffer = new List<int>();
        private int _length = 0;

        public bool Get(int index)
        {
            int bufIndex = Convert.ToInt32(Math.Floor(Convert.ToDouble(index) / 8.0));
            return ((Convert.ToUInt32(_buffer[bufIndex]) >> (7 - index % 8)) & 1) == 1;
        }

        public void Put(int num, int length)
        {
            for (var i = 0; i < length; i++)
            {
                this.PutBit(((Convert.ToUInt32(num) >> (length - i - 1)) & 1) == 1);
            }
        }

        public int GetLengthInBits()
        {
            return _length;
        }

        public void PutBit(bool bit)
        {
            var bufIndex = Convert.ToInt32(Math.Floor(Convert.ToDouble(_length) / 8.0));

            if (_buffer.Count <= bufIndex)
            {
                _buffer.Add(0);
            }

            if (bit)
            {
                _buffer[bufIndex] |= (int)(Convert.ToUInt32(0x80) >> (_length % 8));
            }

            _length++;
        }
    }
}