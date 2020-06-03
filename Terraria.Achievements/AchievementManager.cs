using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Terraria.Social;
using Terraria.Utilities;

namespace Terraria.Achievements
{
	public class AchievementManager
	{
		private class StoredAchievement
		{
			public Dictionary<string, JObject> Conditions;
		}

		private string _savePath;

		private bool _isCloudSave;

		private Dictionary<string, Achievement> _achievements = new Dictionary<string, Achievement>();

		private readonly JsonSerializerSettings _serializerSettings = (JsonSerializerSettings)(object)new JsonSerializerSettings();

		private byte[] _cryptoKey;

		private Dictionary<string, int> _achievementIconIndexes = new Dictionary<string, int>();

		private static object _ioLock = new object();

		public event Achievement.AchievementCompleted OnAchievementCompleted;

		public AchievementManager()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			if (SocialAPI.Achievements != null)
			{
				_savePath = SocialAPI.Achievements.GetSavePath();
				_isCloudSave = true;
				_cryptoKey = SocialAPI.Achievements.GetEncryptionKey();
			}
			else
			{
				_savePath = Main.SavePath + Path.DirectorySeparatorChar + "achievements.dat";
				_isCloudSave = false;
				_cryptoKey = Encoding.ASCII.GetBytes("RELOGIC-TERRARIA");
			}
		}

		public void Save()
		{
			Save(_savePath, _isCloudSave);
		}

		private void Save(string path, bool cloud)
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Expected O, but got Unknown
			lock (_ioLock)
			{
				if (SocialAPI.Achievements != null)
				{
					SocialAPI.Achievements.StoreStats();
				}
				try
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						using (CryptoStream cryptoStream = new CryptoStream(memoryStream, new RijndaelManaged().CreateEncryptor(_cryptoKey, _cryptoKey), CryptoStreamMode.Write))
						{
							BsonWriter val = (BsonWriter)(object)new BsonWriter((Stream)cryptoStream);
							try
							{
								JsonSerializer val2 = JsonSerializer.Create(_serializerSettings);
								val2.Serialize((JsonWriter)(object)val, (object)_achievements);
								((JsonWriter)val).Flush();
								cryptoStream.FlushFinalBlock();
								FileUtilities.Write(path, memoryStream.GetBuffer(), (int)memoryStream.Length, cloud);
							}
							finally
							{
								((IDisposable)val)?.Dispose();
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}
		}

		public List<Achievement> CreateAchievementsList()
		{
			return _achievements.Values.ToList();
		}

		public void Load()
		{
			Load(_savePath, _isCloudSave);
		}

		private void Load(string path, bool cloud)
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			bool flag = false;
			lock (_ioLock)
			{
				if (!FileUtilities.Exists(path, cloud))
				{
					return;
				}
				byte[] buffer = FileUtilities.ReadAllBytes(path, cloud);
				Dictionary<string, StoredAchievement> dictionary = null;
				try
				{
					using (MemoryStream stream = new MemoryStream(buffer))
					{
						using (CryptoStream cryptoStream = new CryptoStream(stream, new RijndaelManaged().CreateDecryptor(_cryptoKey, _cryptoKey), CryptoStreamMode.Read))
						{
							BsonReader val = (BsonReader)(object)new BsonReader((Stream)cryptoStream);
							try
							{
								JsonSerializer val2 = JsonSerializer.Create(_serializerSettings);
								dictionary = val2.Deserialize<Dictionary<string, StoredAchievement>>((JsonReader)(object)val);
							}
							finally
							{
								((IDisposable)val)?.Dispose();
							}
						}
					}
				}
				catch (Exception)
				{
					FileUtilities.Delete(path, cloud);
					return;
				}
				if (dictionary == null)
				{
					return;
				}
				foreach (KeyValuePair<string, StoredAchievement> item in dictionary)
				{
					if (_achievements.ContainsKey(item.Key))
					{
						_achievements[item.Key].Load(item.Value.Conditions);
					}
				}
				if (SocialAPI.Achievements != null)
				{
					foreach (KeyValuePair<string, Achievement> achievement in _achievements)
					{
						if (achievement.Value.IsCompleted && !SocialAPI.Achievements.IsAchievementCompleted(achievement.Key))
						{
							flag = true;
							achievement.Value.ClearProgress();
						}
					}
				}
			}
			if (flag)
			{
				Save();
			}
		}

		private void AchievementCompleted(Achievement achievement)
		{
			Save();
			if (this.OnAchievementCompleted != null)
			{
				this.OnAchievementCompleted(achievement);
			}
		}

		public void Register(Achievement achievement)
		{
			_achievements.Add(achievement.Name, achievement);
			achievement.OnCompleted += AchievementCompleted;
		}

		public void RegisterIconIndex(string achievementName, int iconIndex)
		{
			_achievementIconIndexes.Add(achievementName, iconIndex);
		}

		public void RegisterAchievementCategory(string achievementName, AchievementCategory category)
		{
			_achievements[achievementName].SetCategory(category);
		}

		public Achievement GetAchievement(string achievementName)
		{
			if (_achievements.TryGetValue(achievementName, out Achievement value))
			{
				return value;
			}
			return null;
		}

		public T GetCondition<T>(string achievementName, string conditionName) where T : AchievementCondition
		{
			return GetCondition(achievementName, conditionName) as T;
		}

		public AchievementCondition GetCondition(string achievementName, string conditionName)
		{
			if (_achievements.TryGetValue(achievementName, out Achievement value))
			{
				return value.GetCondition(conditionName);
			}
			return null;
		}

		public int GetIconIndex(string achievementName)
		{
			if (_achievementIconIndexes.TryGetValue(achievementName, out int value))
			{
				return value;
			}
			return 0;
		}
	}
}
