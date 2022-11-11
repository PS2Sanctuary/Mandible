using Mandible.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Pack2.Names;

/// <summary>
/// Represents a mapping of CRC-64 file name hashes to the original file names.
/// </summary>
public class Namelist
{
    private readonly Dictionary<ulong, string> _hashedNamePairs;

    /// <summary>
    /// Gets the namelist.
    /// </summary>
    public IReadOnlyDictionary<ulong, string> Map => _hashedNamePairs;

    /// <summary>
    /// Initializes a new instance of the <see cref="Namelist"/> class.
    /// </summary>
    public Namelist()
    {
        _hashedNamePairs = new Dictionary<ulong, string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Namelist"/> class.
    /// </summary>
    /// <param name="existing">An existing namelist to copy into this <see cref="Namelist"/> instance.</param>
    public Namelist(IReadOnlyDictionary<ulong, string> existing)
    {
        _hashedNamePairs = new Dictionary<ulong, string>(existing);
    }

    /// <summary>
    /// Constructs a <see cref="Namelist"/> from a file.
    /// The file must contain purely names, separated by a newline.
    /// </summary>
    /// <param name="filePath">The path to the namelist file.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The constructed namelist.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the namelist file could not be found.</exception>
    public static async Task<Namelist> FromFileAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Unable to locate the namelist file.", filePath);

        Namelist nl = new();
        await using FileStream masterFS = new(filePath, FileMode.Open);
        await nl.Append(masterFS, ct: ct).ConfigureAwait(false);

        return nl;
    }

    /// <summary>
    /// Gets a name.
    /// </summary>
    /// <param name="hash">The CRC-64 hash of the name.</param>
    /// <param name="name">The name, or null if it doesn't exist in this <see cref="Namelist"/></param>
    /// <returns>A value indicating whether or not the name could be retrieved.</returns>
    public bool TryGet(ulong hash, [NotNullWhen(true)] out string? name)
        => _hashedNamePairs.TryGetValue(hash, out name);

    /// <summary>
    /// Appends an existing namelist to this one.
    /// </summary>
    /// <param name="namelist">The namelist to append.</param>
    public void Append(Namelist namelist)
    {
        foreach (KeyValuePair<ulong, string> element in namelist.Map)
        {
            if (!_hashedNamePairs.ContainsKey(element.Key))
                _hashedNamePairs.Add(element.Key, element.Value);
        }
    }

    /// <summary>
    /// Appends a name-hash pair to the namelist.
    /// </summary>
    /// <param name="hash">The CRC-64 hash of the name.</param>
    /// <param name="name">The name.</param>
    public void Append(ulong hash, string name)
    {
        if (_hashedNamePairs.ContainsKey(hash))
            return;

        _hashedNamePairs.Add(hash, name);
    }

    /// <summary>
    /// Appends a name to the namelist.
    /// </summary>
    /// <param name="name">The name.</param>
    public void Append(string name)
    {
        ulong hash = PackCrc64.Calculate(name);
        Append(hash, name);
    }

    /// <summary>
    /// Appends a list of names to the namelist.
    /// </summary>
    /// <remarks>Extremely long lists could take some time to hash and append.</remarks>
    /// <param name="names"></param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <exception cref="TaskCanceledException">Thrown if the operation is canceled.</exception>
    public async Task Append(IEnumerable<string> names, CancellationToken ct = default)
       => await Task.Run
           (
               () =>
               {
                   foreach (string name in names)
                   {
                       if (ct.IsCancellationRequested)
                           throw new TaskCanceledException();

                       Append(name);
                   }
               },
               ct
           ).ConfigureAwait(false);

    /// <summary>
    /// Appends a stream of names to the namelist.
    /// Names are expected to be separated by a newline sequence.
    /// </summary>
    /// <param name="stream">The stream to read the names from.</param>
    /// <param name="endPosition">
    /// The position in the stream at which to stop reading names.
    /// Set to <c>-1</c> to read to the end of the stream.
    /// </param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="TaskCanceledException"></exception>
    public async Task Append(Stream stream, int endPosition = -1, CancellationToken ct = default)
    {
        using StreamReader sr = new(stream, null, true, -1, true);
        List<string> names = new();

        while (!sr.EndOfStream && (stream.Position < endPosition || endPosition < 0))
        {
            if (ct.IsCancellationRequested)
                throw new TaskCanceledException();

            string? name = await sr.ReadLineAsync().ConfigureAwait(false);
            if (name is null)
                continue;

            names.Add(name);
        }

        await Append(names, ct).ConfigureAwait(false);
        names.Clear();
    }

    /// <summary>
    /// Appends a buffer of names to the namelist.
    /// Names are expected to be separated by a newline sequence.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    public void Append(ReadOnlySpan<byte> buffer)
    {
        int startIndex = 0;

        for (int i = 0; i < buffer.Length; i++)
        {
            byte cur = buffer[i];
            int endIndex;

            if (cur is (byte)'\r' or (byte)'\n')
                endIndex = i;
            else
                continue;

            // Check that we haven't encountered a \r\n sequence
            if (endIndex == startIndex)
                continue;

            string name = Encoding.ASCII.GetString(buffer[startIndex..endIndex]);
            Append(name);

            startIndex = endIndex + 1;
        }
    }

    /// <summary>
    /// Writes the namelist to a stream.
    /// Only names are written out, separated by a newline.
    /// </summary>
    /// <param name="outputStream">The stream to write the names to.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <exception cref="TaskCanceledException">Thrown if the operation is canceled.</exception>
    public async Task WriteAsync(Stream outputStream, CancellationToken ct = default)
    {
        await using StreamWriter sw = new(outputStream, null, -1, true);
        sw.NewLine = "\n";

        foreach (string name in _hashedNamePairs.Values.OrderBy(s => s))
        {
            if (ct.IsCancellationRequested)
                throw new TaskCanceledException();

            await sw.WriteLineAsync(name).ConfigureAwait(false);
        }
    }
}
