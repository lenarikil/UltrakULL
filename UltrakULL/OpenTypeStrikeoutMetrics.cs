using System;
using System.IO;

namespace UltrakULL
{
    internal struct OpenTypeStrikeoutMetrics
    {
        public readonly ushort UnitsPerEm;
        public readonly short Position;
        public readonly ushort Thickness;
        public readonly short Ascender;
        public readonly short XHeight;
        public readonly short CapHeight;

        public OpenTypeStrikeoutMetrics(ushort unitsPerEm, short position, ushort thickness, short ascender, short xHeight, short capHeight)
        {
            UnitsPerEm = unitsPerEm;
            Position = position;
            Thickness = thickness;
            Ascender = ascender;
            XHeight = xHeight;
            CapHeight = capHeight;
        }
    }

    internal static class OpenTypeStrikeoutMetricsReader
    {
        private const uint TrueTypeCollectionTag = 0x74746366; // "ttcf"
        private const uint HeadTag = 0x68656164; // "head"
        private const uint Os2Tag = 0x4F532F32; // "OS/2"
        private const uint HheaTag = 0x68686561; // "hhea"

        private const int SfntHeaderLength = 12;
        private const int TableRecordLength = 16;
        private const int HeadUnitsPerEmOffset = 18;
        private const int Os2StrikeoutSizeOffset = 26;
        private const int Os2StrikeoutPositionOffset = 28;
        private const int Os2VersionOffset = 0;
        private const int Os2CapHeightOffset = 88;
        private const int Os2XHeightOffset = 86;
        private const int HheaAscenderOffset = 4;

        // OS/2 table version 0/1 size (without sxHeight and sCapHeight)
        private const int Os2TableSizeV0 = 78;
        private const int Os2TableSizeV1 = 86;

        public static bool TryRead(string fontPath, out OpenTypeStrikeoutMetrics metrics, out string reason)
        {
            metrics = default(OpenTypeStrikeoutMetrics);
            reason = null;

            try
            {
                byte[] data = File.ReadAllBytes(fontPath);
                int sfntOffset;
                if (!TryGetSfntOffset(data, out sfntOffset, out reason))
                    return false;

                uint headOffset;
                uint headLength;
                uint os2Offset;
                uint os2Length;
                uint hheaOffset;
                uint hheaLength;
                if (!TryGetTable(data, sfntOffset, HeadTag, out headOffset, out headLength) ||
                    !TryGetTable(data, sfntOffset, Os2Tag, out os2Offset, out os2Length) ||
                    !TryGetTable(data, sfntOffset, HheaTag, out hheaOffset, out hheaLength))
                {
                    reason = "required head, OS/2 or hhea table is missing";
                    return false;
                }

                if (!HasRange(data, headOffset + HeadUnitsPerEmOffset, 2) ||
                    !HasRange(data, os2Offset + Os2StrikeoutPositionOffset, 2) ||
                    !HasRange(data, hheaOffset + HheaAscenderOffset, 2) ||
                    headLength < HeadUnitsPerEmOffset + 2 ||
                    os2Length < Os2StrikeoutPositionOffset + 2 ||
                    hheaLength < HheaAscenderOffset + 2)
                {
                    reason = "required font metrics are truncated";
                    return false;
                }

                ushort unitsPerEm = ReadUInt16(data, (int)headOffset + HeadUnitsPerEmOffset);
                ushort thickness = ReadUInt16(data, (int)os2Offset + Os2StrikeoutSizeOffset);
                short position = ReadInt16(data, (int)os2Offset + Os2StrikeoutPositionOffset);
                short ascender = ReadInt16(data, (int)hheaOffset + HheaAscenderOffset);

                // Read OS/2 table version to determine which fields are available.
                ushort os2Version = ReadUInt16(data, (int)os2Offset + Os2VersionOffset);

                // sxHeight (FWORD) at offset 86, available in OS/2 version 2+ (table size >= 86).
                // sCapHeight (FWORD) at offset 88, available in OS/2 version 2+ (table size >= 88).
                short xHeight = 0;
                short capHeight = 0;

                if (os2Version >= 2 && os2Length >= Os2XHeightOffset + 2)
                {
                    xHeight = ReadInt16(data, (int)os2Offset + Os2XHeightOffset);
                }
                if (os2Version >= 2 && os2Length >= Os2CapHeightOffset + 2)
                {
                    capHeight = ReadInt16(data, (int)os2Offset + Os2CapHeightOffset);
                }

                if (unitsPerEm == 0 || thickness == 0)
                {
                    reason = "strikeout metrics contain a zero units-per-em or thickness value";
                    return false;
                }

                metrics = new OpenTypeStrikeoutMetrics(unitsPerEm, position, thickness, ascender, xHeight, capHeight);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
            {
                reason = exception.Message;
                return false;
            }
        }

        private static bool TryGetSfntOffset(byte[] data, out int sfntOffset, out string reason)
        {
            sfntOffset = 0;
            reason = null;
            if (!HasRange(data, 0, 4))
            {
                reason = "file is too short for an OpenType header";
                return false;
            }

            if (ReadUInt32(data, 0) == TrueTypeCollectionTag)
            {
                if (!HasRange(data, 0, 16))
                {
                    reason = "TTC header is truncated";
                    return false;
                }

                uint fontCount = ReadUInt32(data, 8);
                if (fontCount == 0)
                {
                    reason = "TTC does not contain a font face";
                    return false;
                }

                uint firstFaceOffset = ReadUInt32(data, 12);
                if (firstFaceOffset > int.MaxValue || !HasRange(data, firstFaceOffset, SfntHeaderLength))
                {
                    reason = "first TTC font face is outside the file";
                    return false;
                }

                sfntOffset = (int)firstFaceOffset;
            }

            if (!HasRange(data, sfntOffset, SfntHeaderLength))
            {
                reason = "SFNT header is truncated";
                return false;
            }

            return true;
        }

        private static bool TryGetTable(byte[] data, int sfntOffset, uint requestedTag, out uint offset, out uint length)
        {
            offset = 0;
            length = 0;
            ushort tableCount = ReadUInt16(data, sfntOffset + 4);
            long directoryLength = SfntHeaderLength + (long)tableCount * TableRecordLength;
            if (directoryLength > int.MaxValue || !HasRange(data, sfntOffset, (int)directoryLength))
                return false;

            for (int index = 0; index < tableCount; index++)
            {
                int recordOffset = sfntOffset + SfntHeaderLength + index * TableRecordLength;
                if (ReadUInt32(data, recordOffset) != requestedTag)
                    continue;

                offset = ReadUInt32(data, recordOffset + 8);
                length = ReadUInt32(data, recordOffset + 12);
                return offset <= int.MaxValue && length <= int.MaxValue && HasRange(data, offset, length);
            }

            return false;
        }

        private static bool HasRange(byte[] data, uint offset, uint length)
        {
            return offset <= data.Length && length <= data.Length - offset;
        }

        private static bool HasRange(byte[] data, int offset, int length)
        {
            return offset >= 0 && length >= 0 && offset <= data.Length && length <= data.Length - offset;
        }

        private static ushort ReadUInt16(byte[] data, int offset)
        {
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        private static short ReadInt16(byte[] data, int offset)
        {
            return unchecked((short)ReadUInt16(data, offset));
        }

        private static uint ReadUInt32(byte[] data, int offset)
        {
            return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) |
                   ((uint)data[offset + 2] << 8) | data[offset + 3];
        }
    }
}
