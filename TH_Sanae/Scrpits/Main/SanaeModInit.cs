using BaseLib.Config;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Saves;
using System.Reflection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace TH_Sanae.Scripts.Main
{
	[ModInitializer("Init")]
	public class SanaeInit
	{
   private const string ModSfxPrefix = "mod_sfx://";

		public static string ToModSfxPath(string localPath)
		{
			return ModSfxPrefix + localPath;
		}
	private static Harmony? _harmony;
	public static void Init()
	{
		 TryRegisterGodotScriptAssembly();
		ModConfigRegistry.Register("TH_Sanae", new SanaeModConfig());
		_harmony = new Harmony("TH_Sanae");
		_harmony.PatchAll();
		Log.Debug("Sanae mod has been loaded successfully");
	}
    
	private static void TryRegisterGodotScriptAssembly()
	{
		try
		{
			Assembly modAssembly = typeof(SanaeInit).Assembly;
			Type? scriptManagerBridgeType = Type.GetType("Godot.Bridge.ScriptManagerBridge, GodotSharp");

			if (scriptManagerBridgeType == null)
			{
				return;
			}

			MethodInfo? lookupMethod = scriptManagerBridgeType.GetMethod(
				"LookupScriptsInAssembly",
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
				binder: null,
				types: [typeof(Assembly)],
				modifiers: null
			);

			lookupMethod ??= scriptManagerBridgeType
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
				.FirstOrDefault(m =>
				{
					ParameterInfo[] ps = m.GetParameters();
					return ps.Length == 1
						&& ps[0].ParameterType == typeof(Assembly)
						&& (m.Name.Contains("Lookup", StringComparison.OrdinalIgnoreCase)
							|| m.Name.Contains("Load", StringComparison.OrdinalIgnoreCase)
							|| m.Name.Contains("Register", StringComparison.OrdinalIgnoreCase));
				});

			lookupMethod?.Invoke(null, [modAssembly]);
		}
		catch (Exception e)
		{
			Log.Error($"Failed to register Godot scripts for TH_Sanae: {e}");
		}
	}
	}

	[HarmonyPatch]
	public static class ModSfxCmdPatch
	{
		static IEnumerable<MethodBase> TargetMethods()
		{
			return typeof(SfxCmd)
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
				.Where(m =>
				{
					if (m.Name != "Play")
					{
						return false;
					}

					ParameterInfo[] ps = m.GetParameters();
					return ps.Any(p => p.ParameterType == typeof(string));
				});
		}

		static bool Prefix(MethodBase __originalMethod, object[] __args)
		{
			return ModSfxPatch.HandlePlay(__originalMethod, __args);
		}
	}

	[HarmonyPatch]
	public static class ModSfxPatch
	{
		private const string ModSfxPrefix = "mod_sfx://";
		private const float DefaultGain = 0.45f;
		private static readonly Dictionary<string, float> GainOverrides = new()
		{
			["TH_Sanae/ArtWorks/SFX/characterselect.wav"] = 2.8f,
			["TH_Sanae/ArtWorks/SFX/cast.wav"] = 3f
		};

		static IEnumerable<MethodBase> TargetMethods()
		{
			return typeof(NAudioManager)
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
				.Where(m =>
				{
					if (m.Name != "PlayOneShot")
					{
						return false;
					}

					ParameterInfo[] ps = m.GetParameters();
					return ps.Any(p => p.ParameterType == typeof(string));
				});
		}

		static bool Prefix(MethodBase __originalMethod, object[] __args)
		{
			return HandlePlay(__originalMethod, __args);
		}

		public static bool HandlePlay(MethodBase __originalMethod, object[] __args)
		{
			int pathIndex = -1;
			string? path = null;
			for (int i = 0; i < __args.Length; i++)
			{
				if (__args[i] is string s && s.StartsWith(ModSfxPrefix, StringComparison.OrdinalIgnoreCase))
				{
					pathIndex = i;
					path = s;
					break;
				}
			}

			if (pathIndex < 0 || string.IsNullOrEmpty(path))
			{
				return true;
			}

			float volume = 1f;
			ParameterInfo[] ps = __originalMethod.GetParameters();
			for (int i = 0; i < __args.Length && i < ps.Length; i++)
			{
				if (i == pathIndex)
				{
					continue;
				}

				if (__args[i] is float f && ps[i].ParameterType == typeof(float) && ps[i].Name != null && ps[i].Name.Contains("volume", StringComparison.OrdinalIgnoreCase))
				{
					volume = f;
					break;
				}
			}
			if (volume == 1f)
			{
				for (int i = 0; i < __args.Length; i++)
				{
					if (i == pathIndex)
					{
						continue;
					}

					if (__args[i] is float f)
					{
						volume = f;
					}
				}
			}

			try
			{
				PlayModSfxPath(path, volume);
			}
			catch (System.Exception e)
			{
				Log.Error($"Failed to play mod sfx: {path}. Error: {e.Message}");
			}

			return false;
		}

		public static void PlayModSfxPath(string path, float volume = 1f)
		{
			if (!path.StartsWith(ModSfxPrefix, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			string localPath = path.Substring(ModSfxPrefix.Length);
			string resPath = "res://" + localPath;
			AudioStream? stream = ResourceLoader.Load<AudioStream>(resPath);
			if (stream == null)
			{
				return;
			}

			var player = new AudioStreamPlayer();
			player.Stream = stream;
			player.Bus = FindSfxBusName();

			float gain = DefaultGain;
			if (GainOverrides.TryGetValue(localPath, out float overrideGain))
			{
				gain *= overrideGain;
			}
			player.VolumeDb = Mathf.LinearToDb(Mathf.Max(0.0001f, volume * gain));

			if (NGame.Instance != null)
			{
				NGame.Instance.AddChild(player);
			}
			else
			{
				Log.Error($"TH_Sanae mod_sfx can't play because NGame.Instance is null. Path: {path}");
				player.QueueFree();
				return;
			}

			player.Play();
			player.Connect("finished", Callable.From(player.QueueFree));
		}

		private static string FindSfxBusName()
		{
			int count = AudioServer.BusCount;
			for (int i = 0; i < count; i++)
			{
				string bus = AudioServer.GetBusName(i);
				if (string.Equals(bus, "SFX", StringComparison.OrdinalIgnoreCase))
				{
					return bus;
				}
			}

			for (int i = 0; i < count; i++)
			{
				string bus = AudioServer.GetBusName(i);
				string lower = bus.ToLowerInvariant();
				if (lower.Contains("sfx") || lower.Contains("soundeffect") || lower.Contains("sound_effect") || lower == "se")
				{
					return bus;
				}
			}

			return "Master";
		}
	}

	
}
