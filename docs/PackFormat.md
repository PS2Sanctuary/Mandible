# Pack - File Information/Structure

*These notes are built off [Rhett's technical breakdown](https://github.com/RhettVX/forgelight-toolbox/blob/master/docs/rhett-pack1-notes.txt).*

**Extension**: `.pack`\
**Endianness**: Big

### Description

Pack files were the method of storing asset data for games that run on the ForgeLight engine. This format has since been replaced by [Pack2](Pack2Format.md).

### Format

Pack files are comprised of chunks. Each chunk has an 8 byte header and then a block of asset headers describing the stored assets. The chunks are *not* terminated, so you will have to utilise the `Next Chunk` offset in the chunk header to know when you have reached the next chunk. However, the headers are terminated by a block of `0x00` values before the chunk asset data begins.

#### Chunk Header

Name       | Type   |   Example   | Description
---------- | ------ | ----------- | ---
Next Chunk | `uint_32` | `00 83 7e d1` | The offset of the next chunk in the pack
Asset Count | `uint_32` | `00 00 00 75` | The number of assets in the chunk

#### Asset Header

Name          | Type   |   Example   | Description
------------- | ------ | ----------- | ---
Name Length   | `uint_32` | `00 00 00 1a` | The length of the asset name
Asset Name    | `char_8[]` | `AMB_HOSSIN_NIGHT_OS_16.fsb` | The name of the asset file. Not `NUL` terminated.
Asset Offset  | `uint_32` | `00 05 aa 3c` | The offset within the pack of the asset data
Data Length   | `uint_32` | `00 00 57 80` | The length of the asset data
Checksum      | `uint_32` | `31 4c 45 61` | A CRC32 checksum of the asset data

#### Overall read process

1. Read a chunk header.
2. Read the asset headers contained in the chunk, stopping when either you've read the specified number of headers or the `0x00` block is encountered.
3. Repeat steps 1-2 by following the `Next Chunk Offset` in the last chunk header. Terminate this when the offset either returns to `0` or is larger than the length of the pack file.
4. Read the asset data using the extracted headers.