using System;
using System.IO;
using System.Text;

namespace RevitPackageDocumentation.Models;

/// <summary>
/// Минимальный читатель файлов формата Compound File Binary (OLE structured storage), в котором хранятся .rfa.
/// Умеет только читать поток по имени. Не использует Revit API и COM.
/// </summary>
/// <remarks>Описание формата: [MS-CFB] Compound File Binary File Format</remarks>
internal sealed class CompoundFileReader {
    private const uint _endOfChain = 0xFFFFFFFE;
    private const uint _freeSector = 0xFFFFFFFF;
    private const int _headerSize = 512;
    private const int _headerDifatCount = 109;
    private const int _directoryEntrySize = 128;
    private const byte _streamObjectType = 2;
    private const long _maxStreamSize = 64 * 1024 * 1024;

    private static readonly byte[] _signature = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];

    private readonly FileStream _file;
    private int _sectorSize;
    private int _miniSectorSize;
    private ushort _majorVersion;
    private uint _miniStreamCutoff;
    private uint[] _fat;

    private CompoundFileReader(FileStream file) {
        _file = file;
    }

    /// <summary>
    /// Возвращает содержимое потока с указанным именем или null, если такого потока в файле нет
    /// </summary>
    /// <exception cref="InvalidDataException">Файл не является Compound File или поврежден</exception>
    public static byte[] ReadStream(string filePath, string streamName) {
        using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return new CompoundFileReader(file).ReadStreamCore(streamName);
    }

    private byte[] ReadStreamCore(string streamName) {
        byte[] header = ReadBytes(0, _headerSize);
        for(int i = 0; i < _signature.Length; i++) {
            if(header[i] != _signature[i]) {
                throw new InvalidDataException("Файл не является Compound File");
            }
        }

        _majorVersion = BitConverter.ToUInt16(header, 0x1A);
        ushort sectorShift = BitConverter.ToUInt16(header, 0x1E);
        ushort miniSectorShift = BitConverter.ToUInt16(header, 0x20);
        if(sectorShift != 9 && sectorShift != 12 || miniSectorShift != 6) {
            throw new InvalidDataException("Неподдерживаемый размер сектора");
        }

        _sectorSize = 1 << sectorShift;
        _miniSectorSize = 1 << miniSectorShift;
        uint fatSectorCount = BitConverter.ToUInt32(header, 0x2C);
        uint firstDirectorySector = BitConverter.ToUInt32(header, 0x30);
        _miniStreamCutoff = BitConverter.ToUInt32(header, 0x38);
        uint firstMiniFatSector = BitConverter.ToUInt32(header, 0x3C);
        uint firstDifatSector = BitConverter.ToUInt32(header, 0x44);

        _fat = ReadFat(header, fatSectorCount, firstDifatSector);

        byte[] directory = ReadFullChain(firstDirectorySector);
        if(directory.Length < _directoryEntrySize) {
            throw new InvalidDataException("Пустой каталог");
        }

        for(int offset = 0; offset + _directoryEntrySize <= directory.Length; offset += _directoryEntrySize) {
            if(directory[offset + 0x42] != _streamObjectType) {
                continue;
            }

            int nameLength = BitConverter.ToUInt16(directory, offset + 0x40);
            if(nameLength < 2 || nameLength > 64) {
                continue;
            }

            string name = Encoding.Unicode.GetString(directory, offset, nameLength - 2);
            if(!string.Equals(name, streamName, StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            uint startSector = BitConverter.ToUInt32(directory, offset + 0x74);
            long size = GetEntrySize(directory, offset);

            if(size < _miniStreamCutoff) {
                // Маленькие потоки хранятся в мини-потоке, который принадлежит корневой записи (индекс 0)
                uint miniStreamStart = BitConverter.ToUInt32(directory, 0x74);
                long miniStreamSize = GetEntrySize(directory, 0);
                byte[] miniStream = ReadChain(miniStreamStart, miniStreamSize, _fat, _sectorSize, ReadSector);
                uint[] miniFat = ToUInt32Array(ReadFullChain(firstMiniFatSector));
                return ReadChain(startSector, size, miniFat, _miniSectorSize,
                    sector => ReadMiniSector(miniStream, sector));
            }

            return ReadChain(startSector, size, _fat, _sectorSize, ReadSector);
        }

        return null;
    }

    private uint[] ReadFat(byte[] header, uint fatSectorCount, uint firstDifatSector) {
        var fatSectors = new uint[fatSectorCount];
        int found = 0;

        for(int i = 0; i < _headerDifatCount && found < fatSectorCount; i++) {
            uint sector = BitConverter.ToUInt32(header, 0x4C + i * 4);
            if(sector != _freeSector) {
                fatSectors[found++] = sector;
            }
        }

        // Остальные номера секторов FAT лежат в цепочке секторов DIFAT
        int entriesPerDifatSector = _sectorSize / 4 - 1;
        uint difatSector = firstDifatSector;
        long guard = 0;
        while(found < fatSectorCount && difatSector != _endOfChain && difatSector != _freeSector) {
            if(++guard > MaxSectorCount) {
                throw new InvalidDataException("Зацикленная цепочка DIFAT");
            }

            byte[] data = ReadSector(difatSector);
            for(int i = 0; i < entriesPerDifatSector && found < fatSectorCount; i++) {
                uint sector = BitConverter.ToUInt32(data, i * 4);
                if(sector != _freeSector) {
                    fatSectors[found++] = sector;
                }
            }

            difatSector = BitConverter.ToUInt32(data, entriesPerDifatSector * 4);
        }

        int entriesPerSector = _sectorSize / 4;
        var fat = new uint[found * entriesPerSector];
        for(int i = 0; i < found; i++) {
            byte[] data = ReadSector(fatSectors[i]);
            Buffer.BlockCopy(data, 0, fat, i * _sectorSize, _sectorSize);
        }

        return fat;
    }

    private byte[] ReadFullChain(uint startSector) {
        using var result = new MemoryStream();
        uint sector = startSector;
        long guard = 0;
        while(sector != _endOfChain && sector != _freeSector) {
            if(sector >= _fat.Length || ++guard > _fat.Length) {
                throw new InvalidDataException("Поврежденная цепочка секторов");
            }

            byte[] data = ReadSector(sector);
            result.Write(data, 0, data.Length);
            sector = _fat[sector];
        }

        return result.ToArray();
    }

    private static byte[] ReadChain(uint startSector, long length, uint[] table, int unitSize,
        Func<uint, byte[]> readUnit) {
        if(length < 0 || length > _maxStreamSize) {
            throw new InvalidDataException("Недопустимый размер потока");
        }

        var result = new byte[length];
        long offset = 0;
        uint sector = startSector;
        long guard = 0;
        while(offset < length) {
            if(sector >= table.Length || ++guard > table.Length) {
                throw new InvalidDataException("Поврежденная цепочка секторов");
            }

            byte[] data = readUnit(sector);
            int count = (int) Math.Min(unitSize, length - offset);
            Buffer.BlockCopy(data, 0, result, (int) offset, count);
            offset += count;
            sector = table[sector];
        }

        return result;
    }

    private long GetEntrySize(byte[] directory, int entryOffset) {
        // В версии 3 значимы только младшие 32 бита размера
        return _majorVersion == 3
            ? BitConverter.ToUInt32(directory, entryOffset + 0x78)
            : BitConverter.ToInt64(directory, entryOffset + 0x78);
    }

    private byte[] ReadSector(uint sector) {
        // Сектор 0 начинается сразу после заголовка, размер заголовка равен размеру сектора
        return ReadBytes(((long) sector + 1) * _sectorSize, _sectorSize);
    }

    private byte[] ReadMiniSector(byte[] miniStream, uint sector) {
        long position = (long) sector * _miniSectorSize;
        if(position >= miniStream.Length) {
            throw new InvalidDataException("Мини-сектор за пределами мини-потока");
        }

        var data = new byte[_miniSectorSize];
        int count = (int) Math.Min(_miniSectorSize, miniStream.Length - position);
        Buffer.BlockCopy(miniStream, (int) position, data, 0, count);
        return data;
    }

    private byte[] ReadBytes(long position, int count) {
        if(position >= _file.Length) {
            throw new InvalidDataException("Сектор за пределами файла");
        }

        var data = new byte[count];
        _file.Position = position;
        int read = 0;
        while(read < count) {
            int chunk = _file.Read(data, read, count - read);
            if(chunk == 0) {
                break; // последний сектор файла может быть неполным
            }

            read += chunk;
        }

        return data;
    }

    private static uint[] ToUInt32Array(byte[] data) {
        var result = new uint[data.Length / 4];
        Buffer.BlockCopy(data, 0, result, 0, result.Length * 4);
        return result;
    }

    private long MaxSectorCount => _file.Length / _sectorSize + 1;
}
