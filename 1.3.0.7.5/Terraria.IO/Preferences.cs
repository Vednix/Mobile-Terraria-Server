using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Terraria.IO
{
	public class Preferences
	{
		private Dictionary<string, object> _data = new Dictionary<string, object>();

		private readonly string _path;

		private readonly JsonSerializerSettings _serializerSettings;

		public readonly bool UseBson;

		private readonly object _lock = new object();

		public bool AutoSave;

		public event Action<Preferences> OnSave;

		public event Action<Preferences> OnLoad;

		public Preferences(string path, bool parseAllTypes = false, bool useBson = false)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Expected O, but got Unknown
			_path = path;
			UseBson = useBson;
			if (parseAllTypes)
			{
				JsonSerializerSettings val = (JsonSerializerSettings)(object)new JsonSerializerSettings();
				val.TypeNameHandling = ((TypeNameHandling)4);
				val.MetadataPropertyHandling = ((MetadataPropertyHandling)1);
				_serializerSettings = val;
			}
			else
			{
				_serializerSettings = (JsonSerializerSettings)(object)new JsonSerializerSettings();
			}
		}

		public bool Load()
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Expected O, but got Unknown
			lock (_lock)
			{
				if (File.Exists(_path))
				{
					try
					{
						if (!UseBson)
						{
							string text = File.ReadAllText(_path);
							_data = JsonConvert.DeserializeObject<Dictionary<string, object>>(text, _serializerSettings);
						}
						else
						{
							using (FileStream fileStream = File.OpenRead(_path))
							{
								BsonReader val = (BsonReader)(object)new BsonReader((Stream)fileStream);
								try
								{
									JsonSerializer val2 = JsonSerializer.Create(_serializerSettings);
									_data = val2.Deserialize<Dictionary<string, object>>((JsonReader)(object)val);
								}
								finally
								{
									((IDisposable)val)?.Dispose();
								}
							}
						}
						if (_data == null)
						{
							_data = new Dictionary<string, object>();
						}
						if (this.OnLoad != null)
						{
							this.OnLoad(this);
						}
						return true;
					}
					catch (Exception)
					{
						return false;
					}
				}
				return false;
			}
		}

		public bool Save(bool createFile = true)
		{
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Expected O, but got Unknown
			lock (_lock)
			{
				try
				{
					if (this.OnSave != null)
					{
						this.OnSave(this);
					}
					if (!createFile && !File.Exists(_path))
					{
						return false;
					}
					Directory.GetParent(_path).Create();
					if (!createFile)
					{
						File.SetAttributes(_path, FileAttributes.Normal);
					}
					if (!UseBson)
					{
						File.WriteAllText(_path, JsonConvert.SerializeObject((object)_data, (Formatting)1, _serializerSettings));
						File.SetAttributes(_path, FileAttributes.Normal);
					}
					else
					{
						using (FileStream fileStream = File.Create(_path))
						{
							BsonWriter val = (BsonWriter)(object)new BsonWriter((Stream)fileStream);
							try
							{
								File.SetAttributes(_path, FileAttributes.Normal);
								JsonSerializer val2 = JsonSerializer.Create(_serializerSettings);
								val2.Serialize((JsonWriter)(object)val, (object)_data);
							}
							finally
							{
								((IDisposable)val)?.Dispose();
							}
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Unable to write file at: " + _path);
					Console.WriteLine(ex.ToString());
					Monitor.Exit(_lock);
					return false;
				}
				return true;
			}
		}

		public void Put(string name, object value)
		{
			lock (_lock)
			{
				_data[name] = value;
				if (AutoSave)
				{
					Save();
				}
			}
		}

		public T Get<T>(string name, T defaultValue)
		{
			lock (_lock)
			{
				try
				{
					if (_data.TryGetValue(name, out object value))
					{
						if (value is T)
						{
							return (T)value;
						}
						return (T)Convert.ChangeType(value, typeof(T));
					}
					return defaultValue;
				}
				catch
				{
					return defaultValue;
				}
			}
		}

		public void Get<T>(string name, ref T currentValue)
		{
			currentValue = Get(name, currentValue);
		}
	}
}
