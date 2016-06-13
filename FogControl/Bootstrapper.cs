using System;
using UnityEngine;

namespace FogControl
{
	public class Bootstrap : FortressCraftMod
	{
		private Boolean hooked;

		private void Update()
		{
			if (!this.hooked && WorldScript.instance.localPlayerInstance != null && WorldScript.meGameMode == eGameMode.eSurvival)
			{
				var weatherManager = GameManager.instance.gameObject.GetComponent<SurvivalWeatherManager>();
				if (weatherManager == null)
					throw new Exception("Failed to find SurvivalWeatherManager");
				weatherManager.instance.gameObject.AddComponent<BetterFogManager>();
				Destroy(weatherManager.instance.gameObject.GetComponent<SurvivalFogManager>());
				this.hooked = true;
				Debug.Log("Replaced SurvivalFogManager in SurvivalWeatherManager.");
			}
		}
	}
}
