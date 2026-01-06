using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KursovaRabotaCS20252026.Services
{
	public static class FileService
	{
		private const string FilePath = "Data/data.json";

		public static async Task<T> LoadAsync<T>() where T : new()
		{
			if (!File.Exists(FilePath))
				return new T();

			var json = await File.ReadAllTextAsync(FilePath);
			return JsonSerializer.Deserialize<T>(json) ?? new T();
		}

		public static async Task SaveAsync<T>(T data)
		{
			var dir = Path.GetDirectoryName(FilePath);
			if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
			await File.WriteAllTextAsync(FilePath, json);
		}
	}
}
