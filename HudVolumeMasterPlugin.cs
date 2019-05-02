// This plugin is to set turboHUD master volume.
// To set the volume, put your cursor on the chat edit line by pressing "Enter" and then "/volume n/" (n is from 0 to 100.
using System;
using Turbo.Plugins.Default;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Turbo.Plugins.James
{
    public class HudVolumeMasterPlugin : BasePlugin
    {
		private string chatEditLine = "Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline";
		private static System.Timers.Timer ReadEditLineTimer;
		private int MasterVolume;
		private int savedVolume;
		private string culture;

		public HudVolumeMasterPlugin()
        	{
        		Enabled = true;
        	}

        	public override void Load(IController hud)
        	{
            	base.Load(hud);

			Hud.Sound.VolumeMode = VolumeMode.Constant;
			MasterVolume = 80;
			savedVolume = 0;
			culture = System.Globalization.CultureInfo.CurrentCulture.ToString().Substring(0, 2);

		     ReadEditLineTimer = new System.Timers.Timer();
			ReadEditLineTimer.Interval = 500;		// edit line filtering interval
			ReadEditLineTimer.Elapsed += ReadEditLine;
			ReadEditLineTimer.AutoReset = true;
			ReadEditLineTimer.Enabled = true;
	   	}

		public void ReadEditLine(Object source, System.Timers.ElapsedEventArgs e)
        	{
        		// chat edit line
        		if (!Hud.Render.GetUiElement(chatEditLine).Visible)
        			return;

			int tmp = 0;
			string defaultVal = string.Empty;
        		string lineStr = Hud.Render.GetUiElement(chatEditLine).ReadText(System.Text.Encoding.UTF8, false).Trim();	// if error, change "UTF8" with "Default"...not tested though
        		lineStr = lineStr.Trim().ToLower();
	        	if (String.Equals(lineStr, "/volume/"))
			{
				//Hud.Sound.VolumeMode = VolumeMode.AutoMasterAndEffects;
				int vol = 0;
				if (savedVolume == 0)
				{
					vol = (int)(Hud.Sound.IngameMasterVolume * Hud.Sound.IngameEffectsVolume * Hud.Sound.VolumeMultiplier / 100);
					Hud.Sound.ConstantVolume = vol;
				} else
					vol = savedVolume;
				if (Hud.Sound.LastSpeak.TimerTest(5000))
					Hud.Sound.Speak("Current Hud volume is " + vol);
				return;
			}
			
        		Match match = Regex.Match(lineStr, @"(?<=/volume ).+(?=/)");
			if (match.Success)	// in the edit line, should type "/volume n/" <- n is from 0 to 100.
			{
				if (Char.IsDigit(match.Value[0]))
				{
					tmp = Int32.Parse(match.Value);
					if (tmp < 1 || tmp > 100)
					{
						MasterVolume = 80;			// default volume
						defaultVal = (culture == "ko") ? "�⺻�� " : "default value ";
					} else
						MasterVolume = tmp;
				} else
				{
					if (Hud.Sound.LastSpeak.TimerTest(5000))
	        			{
	        				Console.Beep(300, 200);
	        				if (culture == "ko")
	        					Hud.Sound.Speak("��� ���� ���� ����!");
	        				else
	        					Hud.Sound.Speak("Hud volume setting error!");
	        			}
	        		}

				if (MasterVolume != savedVolume)
				{
					savedVolume = MasterVolume;
					Hud.Sound.ConstantVolume = MasterVolume; //0 .. 100

	        			if (culture == "ko")
	        				Hud.Sound.Speak("��� ������ " + defaultVal + Convert.ToString(MasterVolume) + "���� ���� �Ǿ����ϴ�..");
	        			else
	        				Hud.Sound.Speak("Current Hud volume is set to " + defaultVal + Convert.ToString(MasterVolume));
	        		}
        		}
        	}
	}
}