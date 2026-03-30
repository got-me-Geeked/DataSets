using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using MAX_PAIR_IN_GRAPH.Models;


namespace MAX_PAIR_IN_GRAPH.Services
{
    public static class JsonService
    {
        private static readonly JsonSerializerOptions _options =
           new JsonSerializerOptions
           {
               PropertyNameCaseInsensitive = true,
               WriteIndented = true
           };

        public static async Task<BipartiteGraph?> LoadGraphAsync(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine("Файл не найден.");
                    return null;
                }

                await using var stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    4096,
                    useAsync: true);

                var graph = await JsonSerializer.DeserializeAsync<BipartiteGraph>(stream, _options);


                if (graph == null)
                {
                    Console.WriteLine("Ошибка десериализации.");
                    return null;
                }

                return graph;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                return null;
            }
        }

        public static async Task SaveAsync<T>(string path, T data)
        {
            try
            {
                EnsureDirectoryExists(path);

                await using var stream = new FileStream(
                    path,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    useAsync: true);

                await JsonSerializer.SerializeAsync(stream, data, _options);

                Console.WriteLine($"Файл сохранён: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        private static void EnsureDirectoryExists(string path)
        {
            string? dir = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}
