using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Common;
using UnityEngine;

namespace FogControl
{
	internal class BetterFogManager : SurvivalFogManager
	{
		private GameObject maCaveAudio;
		private Color mAmbient;
		private Material mFakeFogMat;
		private Color mFogCol;
		private float mrCurrentAmbience;
		private Color mSnowFogCol = new Color(0.75f, 0.85f, 1f, 1f);

		private Single customFogDensity;
		private Ini _settings;

		private void Start()
		{
			this.MorningAmbientLight = UniStormWeatherSystem_C.instance.MorningAmbientLight;
			this.MiddayAmbientLight = UniStormWeatherSystem_C.instance.MiddayAmbientLight;
			this.DuskAmbientLight = UniStormWeatherSystem_C.instance.DuskAmbientLight;
			this.NightAmbientLight = UniStormWeatherSystem_C.instance.NightAmbientLight;
			this.MorningSkyboxTint = UniStormWeatherSystem_C.instance.MorningSkyboxTint;
			this.MiddaySkyboxTint = UniStormWeatherSystem_C.instance.MiddaySkyboxTint;
			this.DuskSkyboxTint = UniStormWeatherSystem_C.instance.DuskSkyboxTint;
			this.NightSkyboxTint = UniStormWeatherSystem_C.instance.NightSkyboxTint;
			this.mFakeFogMat = GameObject.Find("FuckedOffWithUniStorm").GetComponent<Renderer>().material;

			var location = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.ini");

			try
			{
				if (!File.Exists(location))
				{
					using (var fs = File.Create(location))
					using (var stream = new StreamWriter(fs))
					{
						stream.WriteLine("[FogControl]");
						stream.WriteLine("FogLevel=50");
						stream.WriteLine("# Whoever wrote this damn ini parser is a special kind of person.");
						stream.WriteLine("# I mean seriously, who the hell throws an exception because the file is empty!?");
						stream.WriteLine("# And why the hell does it send all text to lowercase!? I don't even....");
					}
				}


				this._settings = new Ini(location);
				if (!this._settings.ContainsSection("FogControl"))
					this._settings.AddSection("FogControl");
				this._settings.SetSection("FogControl");

				if (!this._settings.ContainsKey("FogLevel"))
					this._settings.SetFloat("FogLevel", 50);
				this.customFogDensity = this._settings.GetFloat("FogLevel", 50);

				if (this._settings.ContainsSection("ClearView"))
				{
					this._settings.SetSection("ClearView");
					CamDetail.mbEnableBloom = this._settings.GetBoolean("DisableBloom", false);
					CamDetail.mbDisableSunAndShafts = this._settings.GetBoolean("DisableSunShafts", false);

					CamDetail.instance.ControlledCam.GetComponent<BloomAndLensFlares>().enabled = CamDetail.mbEnableBloom;
				}

				this._settings.WriteToDisk();
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}

			global::Console.AddCommand(new ConsoleCommand("foglevel", "Set the Density of the Fog. Usage: foglevel 50", CmdParameterType.Float, this.gameObject, "FogLevel"));
			global::Console.AddCommand(new ConsoleCommand("resetfog", "Resets the fog density to the default level. Usage: resetfog",
				CmdParameterType.None, this.gameObject, "FogReset"));

		}

		private void FogLevel(float density)
		{
			this.customFogDensity = density;
			this._settings.SetSection("FogControl");
			this._settings.SetFloat("FogLevel", density);
			this._settings.WriteToDisk();
			global::Console.LogTargetFunction($"Fog density updated to \"{density}\".", ConsoleMessageType.Log);
		}

		private void FogReset()
		{
			this.customFogDensity = 50;
			this._settings.SetSection("FogControl");
			this._settings.SetFloat("FogLevel", 50);
			this._settings.WriteToDisk();
			global::Console.LogTargetFunction($"Fog density reset to \"50\".", ConsoleMessageType.Log);
		}

		private void Update()
		{
			if (WorldScript.instance == null)
			{
				return;
			}
			if (WorldScript.instance.localPlayerInstance == null)
			{
				return;
			}
			//			this.mnUpdates++;
			this.mrFogDensity = this.customFogDensity;
			if (DropShipScript.DropshipSequenceActive)
			{
				this.mrFogDensity = 99999f;
			}
			RenderSettings.fogDensity += (1f / this.mrFogDensity - RenderSettings.fogDensity) * Time.deltaTime;
			long num = WorldScript.instance.localPlayerInstance.mWorldY - 4611686017890516992L;
			int num2 = (int)num;
			SurvivalFogManager.GlobalDepth = num2;
			float num3 = (float)num2 / 16f;
			num3 += 2f;
			if (num3 > 1f)
			{
				num3 = 1f;
			}
			if (num3 < 0f)
			{
				num3 = 0f;
			}
			SurvivalFogManager.mrLerpedDepth = num3;
			float num4 = num3;
			float num5 = SurvivalWeatherManager.mrSunAngle;
			if (num5 < 0.05f)
			{
				num5 = 0.05f;
			}
			if (num4 > num5)
			{
				num4 = num5;
			}
			this.mrCurrentAmbience += (num4 - this.mrCurrentAmbience) * Time.deltaTime;
			num4 = this.mrCurrentAmbience;
			this.mFogCol = Color.Lerp(Color.black, this.mSnowFogCol, num4);
			if (DifficultySettings.mbConstantDay)
			{
				UniStormWeatherSystem_C.instance.currentTimeOfDay = 0.5f;
			}
			if (DifficultySettings.mbConstantNight)
			{
				UniStormWeatherSystem_C.instance.currentTimeOfDay = 0f;
			}
			RenderSettings.fogColor = this.mFogCol;
			UniStormWeatherSystem_C.instance.MorningAmbientLight = Color.Lerp(Color.black, this.MorningAmbientLight, num4);
			UniStormWeatherSystem_C.instance.MiddayAmbientLight = Color.Lerp(Color.black, this.MiddayAmbientLight, num4);
			UniStormWeatherSystem_C.instance.DuskAmbientLight = Color.Lerp(Color.black, this.DuskAmbientLight, num4);
			UniStormWeatherSystem_C.instance.MorningAmbientLight = Color.black;
			UniStormWeatherSystem_C.instance.MiddayAmbientLight = Color.black;
			UniStormWeatherSystem_C.instance.DuskAmbientLight = Color.black;
			UniStormWeatherSystem_C.instance.NightAmbientLight = Color.black;
			UniStormWeatherSystem_C.instance.maxLightSnowIntensity = 4000f * num3;
			UniStormWeatherSystem_C.instance.maxLightSnowDustIntensity = 6f * num3;
			float num6 = WorldScript.instance.mWorldData.mrWorldTimePlayed / 3600f;
			num6 = 1f - num6;
			if (num6 < 0f)
			{
				num6 = 0f;
			}
			float num7 = num6 * 10000f * num3;
			if (num7 < 0f)
			{
				num7 = 0f;
			}
			UniStormWeatherSystem_C.instance.maxSnowStormIntensity = num7;
			UniStormWeatherSystem_C.instance.maxHeavySnowDustIntensity = 18f * num3;
			UniStormWeatherSystem_C.instance.MorningSkyboxTint = Color.Lerp(Color.black, this.MorningSkyboxTint, num4);
			UniStormWeatherSystem_C.instance.MiddaySkyboxTint = Color.Lerp(Color.black, this.MiddaySkyboxTint, num4);
			UniStormWeatherSystem_C.instance.DuskSkyboxTint = Color.Lerp(Color.black, this.DuskSkyboxTint, num4);
			UniStormWeatherSystem_C.instance.NightSkyboxTint = Color.Lerp(Color.black, this.NightSkyboxTint, num4);
			UniStormWeatherSystem_C.instance.MorningSkyboxTint = this.mFogCol;
			UniStormWeatherSystem_C.instance.MiddaySkyboxTint = this.mFogCol;
			UniStormWeatherSystem_C.instance.DuskSkyboxTint = this.mFogCol;
			UniStormWeatherSystem_C.instance.NightSkyboxTint = this.mFogCol;
			this.mFakeFogMat.SetColor("_Color", this.mFogCol);
			float num8 = num3;
			if (UniStormWeatherSystem_C.instance.rainSound.GetComponent<AudioSource>().volume > num8)
			{
				UniStormWeatherSystem_C.instance.rainSound.GetComponent<AudioSource>().volume = num8;
			}
			if (UniStormWeatherSystem_C.instance.windSnowSound.GetComponent<AudioSource>().volume > num8)
			{
				UniStormWeatherSystem_C.instance.windSnowSound.GetComponent<AudioSource>().volume = num8;
			}
			if (UniStormWeatherSystem_C.instance.windSound.GetComponent<AudioSource>().volume > num8)
			{
				UniStormWeatherSystem_C.instance.windSound.GetComponent<AudioSource>().volume = num8;
			}
			if (this.maCaveAudio == null)
			{
				this.maCaveAudio = GameObject.Find("CaveSound");
			}
			else
			{
				AudioSource[] components = this.maCaveAudio.GetComponents<AudioSource>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].volume = (1f - num3) / 2f;
				}
			}
			if (num4 > 0.4f)
			{
				num4 = 0.4f;
			}
			RenderSettings.ambientLight = Color.Lerp(new Color32(8, 8, 32, 0), Color.white, num4);
			if (SurvivalParticleManager.instance != null)
			{
				SurvivalParticleManager.instance.Snow_Mist.startColor = RenderSettings.ambientLight;
			}
		}

	}
}
