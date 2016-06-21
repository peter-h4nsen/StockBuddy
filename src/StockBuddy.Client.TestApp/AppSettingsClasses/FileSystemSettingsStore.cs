using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Client.TestApp.AppSettingsClasses
{
    //public sealed class FileSystemSettingsStoreOld : ISettingsStoreOld
    //{
    //    private readonly string _filePath;

    //    public FileSystemSettingsStoreOld(string filename)
    //    {
    //        if (string.IsNullOrWhiteSpace(filename))
    //            throw new ArgumentException("Value must be set", nameof(filename));

    //        var exeLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
    //        _filePath = Path.Combine(Path.GetDirectoryName(exeLocation), filename);
    //    }

    //    public TSettings Load<TSettings>()
    //    {
    //        using (var fileStream = File.OpenRead(_filePath))
    //        using (var streamReader = new StreamReader(fileStream))
    //        using (var jsonReader = new JsonTextReader(streamReader))
    //        {
    //            var serializer = new JsonSerializer();
    //            var appSettings = serializer.Deserialize<TSettings>(jsonReader);
    //            return appSettings;
    //        }
    //    }

    //    public void Save<TSettings>(TSettings settings)
    //    {
    //        using (var fileStream = File.Open(_filePath, FileMode.Truncate, FileAccess.ReadWrite))
    //        using (var streamWriter = new StreamWriter(fileStream))
    //        {
    //            var serializer = new JsonSerializer();
    //            serializer.Formatting = Formatting.Indented;
    //            serializer.Serialize(streamWriter, settings);
    //        }
    //    }

    //    public bool IsStoreCreated()
    //    {
    //        return File.Exists(_filePath);
    //    }

    //    public void CreateStore<TSettings>(TSettings settings)
    //    {
    //        File.Create(_filePath).Dispose();
    //        Save(settings);
    //    }
    //}
}
