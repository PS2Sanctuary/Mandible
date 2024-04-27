# Pack2 - File Information/Structure

*These notes are built off [Rhett's technical breakdown](https://github.com/RhettVX/forgelight-toolbox/blob/master/docs/rhett-pack2-notes.txt).*

**Extension**: `.pack2`\
**Endianness**: Mixed: Headers and maps are in *little* endian, while data is in *big* endian

### Description

Pack2 files store asset data for games that run on the ForgeLight engine. This format supersedes the [Pack](PackFormat.md) format,
and includes better support for detecting modified game files. Furthermore, some data is compressed using `zlib`. It was
introduced in PlanetSide 2's DX11 update in April 2019.

### Format

Pack2 files have an [initial header](#pack-header) with basic info, such as the asset count and pack length. A buffer containing
only `0x00` values is then written up to an offset of `0x200`.

Next, the [asset data](#asset-data) is written. The asset data blocks also contain compression information, if applicable. Note
that this data is in **big endian** format.

Finally, an *asset map* is written, which contains numerous sequential [asset headers](#asset-header) that hold information about
the stored asset data.

#### Pack Header

`Little Endian`

| Name         | Type        | Example                   | Description                                                                          |
|--------------|-------------|---------------------------|--------------------------------------------------------------------------------------|
| Signature    | `char_8[3]` | `PAK`                     | The magic identifier for pack2 files.                                                |
| Version      | `byte`      | `01`                      | The version of the pack.                                                             |
| Asset Count  | `uint_32`   | `29 00 00 00`             | The number of assets contained within the pack.                                      |
| Pack Length  | `uint_64`   | `20 88 0e 00 00 00 00 00` | The byte length of the pack.                                                         |
| Map Offset   | `uint_64`   | `00 83 0e 00 00 00 00 00` | The byte offset into the pack at which the asset map begins.                         |
| Unknown      | `uint_64`   | `00 01 00 00 00 00 00 00` | Unknown - every pack at the time of writing uses the same value of `256` here.       |
| Checksum     | `byte[128]` | `...`                     | Unknown how this is calculated. Presumably used to verify the integrity of the pack. |

#### Asset Data

`Big Endian`

| Name                  | Type      | Example       | Description                                                                          |
|-----------------------|-----------|---------------|--------------------------------------------------------------------------------------|
| Compression Indicator | `byte[4]` | `a1 b2 c3 d4` | A magic indicator to show zlib compression. Only present if the data is compressed.  |
| Unpacked Size         | `uint_32` | `00 00 01 dd` | The unpacked/decompressed size of the asset. Only present if the data is compressed. |
| Data                  | `byte[]`  | `...`         | The asset data.                                                                      |

#### Asset Header

`Little Endian`

| Name                  | Type      | Example                   | Description                                                                                                                                                             |
|-----------------------|-----------|---------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Name Hash             | `uint_64` | `f3 62 01 0d 88 db 26 00` | A CRC-64 hash of the UPPERCASE asset name. This CRC variant uses "Jones" coefficients and a value of `0xffffffffffffffff` for both the initial value and final XOR out. |
| Asset Offset          | `uint_64` | `00 94 02 00 00 00 00 00` | The offset of the asset data within the pack.                                                                                                                           |    
| Stored data size      | `uint_64` | `97 01 00 00 00 00 00 00` | The size of the stored asset data.                                                                                                                                      |
| Compression Indicator | `uint_32` | `11 00 00 00`             | Compressed: `0x11/0x01`; Uncompressed: `0x10/0x00`.                                                                                                                     |
| CRC-32 Data Hash      | `uint_32` | `FF 44 AF 89`             | A CRC-32 hash of the stored data.                                                                                                                                       |
