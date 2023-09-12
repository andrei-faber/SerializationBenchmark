using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Cbor;
using System.IO;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json;

namespace SerializationBenchmark;

class Program
{
	static string _dataPath;

	static void Main(string[] args)
	{
		_dataPath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\..\..\..\Data\");
		Directory.CreateDirectory(_dataPath);

		var data = new int[4_000_000];
		for (var i = 0; i < data.Length; i++)
		{
			data[i] = i;
		}

		Measure(WriteJson, "JSON", data);
		Measure(WriteCbor, "CBOR", data);
	}

	static void Measure<T>(Action<string, T> action, string file, T data)
	{
		Console.Write($"{file}:\t");

		var watch = Stopwatch.StartNew();
		action(file, data);
		watch.Stop();

		var size = new FileInfo(_dataPath + file).Length;

		Console.WriteLine($"{watch.Elapsed.TotalSeconds} secs\t\t{size:n0}");
	}

	static void WriteJson(string fileName, object data)
	{
		using var stream = File.Create(_dataPath + fileName);
		var serializer = new JsonSerializer();
		using var writer = new StreamWriter(stream);
		serializer.Serialize(writer, data);
	}

	static void WriteCbor(string fileName, int[] data)
	{
		var writer = new CborWriter();

		writer.WriteStartArray(data.Length);
		foreach (var cur in data)
		{
			writer.WriteInt32(cur);
		}
		writer.WriteEndArray();

		var bytes = writer.Encode();
		File.WriteAllBytes(_dataPath + fileName, bytes);
	}
}
