// ���3�κ� ������Ƽ���� ����ǿ��� ã���� �ϴ� �ܾ ��Ÿ���� �ش� ���� ������ �˷��ְ� Numpad4�� ������ ���� ����� ��Ʋ�±׸� list ���·� �����ָ� �����ϸ� �ش� ������ Ŭ�����忡 �ڵ� �����Ͽ� ģ�߽� ctrl_v�� ������ ������ �ڵ� �����
// Alarm on finding the filtered words(or conditions) on a website bulletin board for a want ad of party matching and auto clipboard copy of the BattleTag so that "Add friend" can be done easily

using System;
using System.Linq;
using Turbo.Plugins.Default;
using System.Windows.Forms;
using SharpDX.DirectInput;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Media;
using System.Net;
using System.Collections;
using System.Threading;

namespace Turbo.Plugins.James
{
    public class PartyMatchingWebsiteMonitorPlugin : BasePlugin, IKeyEventHandler
    {
        // �Ʒ� �� ���� url �߿��� ������ ���ϴ� �͸� ����ϰ� �������� �ڸ�Ʈ ó���Ͻø� �˴ϴ�. (���� �ܴ̿� �� Ȯ���� �� �غ����� ���� ������ �˷��ּ���.)
        private string WebsiteUrl = "http://www.inven.co.kr/board/diablo3/4622?category=%EB%AA%A8%EC%A7%91%EC%A4%91"; // �κ����3 ������Ƽ����[������]
        //private string WebsiteUrl = "http://www.inven.co.kr/board/diablo3/4738?category=%EB%AA%A8%EC%A7%91%EC%A4%91"; // ������Ƽ����
	   //private string WebsiteUrl = "http://www.inven.co.kr/board/diablo3/4623";	//�ϵ��ھ� ��Ƽ ����
	   private string[] ChatWatchListAnd = new string[5];
	   private string[] ChatWatchListOr = new string[5];
	   private string[] WebBBList = new string[3];		// �κ� ���� ���� ����
	   private string[] WebDate = new string[3];		// ���� ������ �ð�
	   private string[] WebHREF = new string[3];		// ������ ������ ����ִ� �������� �ּ�
	   private string[,] WebAds = new string[3, 3];		// (������, ��Ʋ�±�) * 3�� - 3�� �̻��� ���������� ������ ����
	   private bool InputOK;
	   private string savedValue;
	   private string oldValue;
	   private int ChatPopupNo;
	   private SoundPlayer ChatFind = new SoundPlayer();
	   private WebClient webClient = new WebClient();
	   private static System.Timers.Timer WebBBSearchTimer;
	   private static System.Timers.Timer ClickTimer;
	   private int WebBBSearchInterval = 7000;		//5�ʸ��� �κ� �˻�
	   private string BaTag;
	   private bool InputChanged;

        public PartyMatchingWebsiteMonitorPlugin()
        {
            Enabled = true;
            ChatFind.SoundLocation = "D:/Game/TurboD3/sounds/notification_1.wav";	// sound when finding conditions on chat
            ChatFind.LoadAsync();
            BaTag = string.Empty;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
            InputOK = false;
            InputChanged = false;
            ChatPopupNo = 0;
            oldValue = string.Empty;
            webClient.Encoding = System.Text.Encoding.UTF8;
            // ListView ���� �ʱ�ȭ
            for (int i = 0; i < WebAds.GetLength(0); i++)
		  {
		  	for (int j = 0; j < WebAds.GetLength(1); j++)
			{
				WebAds[i, j] = string.Empty;
			}
		  }
        }

        public void WebBBListSearch(Object source, System.Timers.ElapsedEventArgs e)
        {
			if (!InputOK) return;

			WebBBSearchTimer.Interval = WebBBSearchInterval;
			for (int i = 0; i < WebBBList.GetLength(0); i++)
			{
				WebBBList[i] = string.Empty;
			}
			string WebBBStr = webClient.DownloadString(WebsiteUrl);
			string filteredStr = string.Empty;

			Match match = Regex.Match(WebBBStr, @"(?<='bbsNo'>).+(?=</TD><)");	// ���� ������ �߰� �Ǿ����� ������ ���� ù bbsNo�� Ȯ��
			if (match.Success)
			{
				if (match.Value == oldValue)
				{
					if (!InputChanged)		// ����� ������ �� �ٲ�� �˻� �ܾ �ٲ�� ����� �ٽ� �˻�
						return;
					else
						InputChanged = false;
				} else
					oldValue = match.Value;
			} else
			{
				Console.Beep(1000, 300);
				return;		//exception
			}

			// ���� �� ���� 3�� ���� : ����� 3��. �� �̻��� ���� �ð��� ������ �� �ǹ̰� ����
			for (int i = 0; i <  WebBBList.GetLength(0); i++)
			{
				if (i == 0)
					match = Regex.Match(WebBBStr, @"(?<=bbsNo.+\]).+(?=</A)");
				else
					match = match.NextMatch();
				if (match.Success)
				{
					WebBBList[i] = Regex.Replace(match.Value, @"&nbsp;&nbsp;", string.Empty).Trim();
				} else
					break;
			}

			// ���� �� Date ����
			for (int i = 0; i <  WebDate.GetLength(0); i++)
			{
				if (i == 0)
					match = Regex.Match(WebBBStr, @"(?<='date'>).+(?=&nbsp;)");
				else
					match = match.NextMatch();

				if (match.Success)
				{
					WebDate[i] = match.Value;
				} else
					break;
			}

			// ���� �� ���� HREF(�� �ּ�) ���� (���ǿ� �´� ������� ������ ����ִ� ���� �� ������ �ּ�)
			for (int i = 0; i <  WebBBList.GetLength(0); i++)
			{
				if (i == 0)
					match = Regex.Match(WebBBStr, @"(?<=HREF="").+(?="">&nbsp;)");
				else
					match = match.NextMatch();

				if (match.Success)
				{
					WebHREF[i] = match.Value;
				} else
					break;
			}

			if (WebBBList[0] == string.Empty) return;

			bool found = false;
			var cnt = 0;
			// �κ� ���� ������� ������ �Է��� �˻� ���ǿ� �������� Ȯ���ϴ� �۾�
			foreach (string chatLine in WebBBList)
			{
				if (ChatWatchListAnd[0] != string.Empty)
				{
					foreach (string x in ChatWatchListAnd)
					{
					    if (chatLine.Contains(x))
					    // if (chatLine.ToLower().Contains(x))
					    {
					    		found = true;
					    } else
					    {
					    		found = false;
					    		break;
					    }
					}
				}

				if (!found)
				{
				     if (ChatWatchListOr[0] != string.Empty)
				     {
						foreach (string x in ChatWatchListOr)
						{
						    if (chatLine.Contains(x))
						    // if (chatLine.ToLower().Contains(x))
						    {
						        found = true;
						        break;
						    }
						}
					}
				}

				if (found)
				{
					ChatPopupNo++;
					if (ChatPopupNo > 3) ChatPopupNo = 1;
					var pTitle = "�κ� ������Ƽ ����";
					var pDuration = WebBBSearchInterval;
					var tmp = chatLine.Trim() + "("+WebDate[cnt]+")";
					WebAds[cnt, 0] = tmp;
					GetBattleTag(cnt);

					Hud.RunOnPlugin<PopupMsgPlugin>(plugin =>
	                	{
			          		switch(ChatPopupNo)
			          		{
			          			case(1):
								plugin.Show(tmp, pTitle, pDuration, "", PopupMsgPlugin.EnumPopupDecoratorToUse.WebBB1);
								break;
			          			case(2):
								plugin.Show(tmp, pTitle, pDuration, "", PopupMsgPlugin.EnumPopupDecoratorToUse.WebBB2);
								break;
			          			case(3):
								plugin.Show(tmp, pTitle, pDuration, "", PopupMsgPlugin.EnumPopupDecoratorToUse.WebBB3);
								break;
						}
	                     });
					ChatFind.PlaySync();
					if (Hud.Sound.LastSpeak.TimerTest(3000))
						Hud.Sound.Speak("�κ� ���� ��Ƽ ���� Ȯ��!");		// Words show up on the chat box

					found = false;
				}
				cnt++;
			}
	     }

		// �κ� ������Ƽã�� ����� ������� ���ǿ� ������ ���� �ø� ����� ������ ����ִ� ���� ���������� ���� ������ ������
		public void GetBattleTag(int Aindex)
		{
			string WebBBStr = webClient.DownloadString(WebHREF[Aindex]);
			Match match = Regex.Match(WebBBStr, @"(?<=""description"" content="").+\d{4,}");	// BattleTag ����
			if (match.Success)
			{
				string output = Regex.Replace(match.Value, @".+\.", string.Empty);
				output = Regex.Replace(output, @" ", string.Empty).Trim();
				WebAds[Aindex, 1] = SubstringReverse(output, 1, 20);
				//WebAds[Aindex, 1] = Regex.Replace(match.Value, @" ", string.Empty).Trim();
			} else
			{
				Console.Beep(1000, 300);
				return;		//exception
			}
		}

		public static string SubstringReverse(string str, int reverseIndex, int length)
		{
		    return string.Join("", str.Reverse().Skip(reverseIndex - 1).Take(length).Reverse());
		}
		
		// �κ� �������� �˻� ��� �����ִ� ListView �� �� ���� �ۼ�
		public DialogResult listView_Doit(string title, string content)
		{
			var ForceReturn = true;
			for (int i = 0; i < WebAds.GetLength(0); i++)
			{
				if (WebAds[i, 0] != string.Empty)
				{
					ForceReturn = false;
					break;
				}
			}
			if (ForceReturn)
			{
				Hud.Sound.Speak("������ ���� ������ �����ϴ�!");
				Console.Beep(500, 250);
				return DialogResult.Cancel;
			}
			Form form = new Form();
			Label label = new Label();
			ListView listView = new ListView();
			form.ClientSize = new Size(490, 190);
			listView.Bounds = new Rectangle(new Point(20,40), new Size(450,100));
			//listView.Sorting = SortOrder.Descending;
			Button buttonOk = new Button();
		     Button buttonCancel = new Button();
			form.StartPosition = FormStartPosition.CenterScreen;
			form.MaximizeBox = false;
			form.MinimizeBox = false;
			form.TopMost = true;
			form.FormBorderStyle = FormBorderStyle.FixedDialog;
			form.AcceptButton = buttonOk;
		     form.CancelButton = buttonCancel;

			form.Text = title;
			label.Text = content;
			buttonOk.Text = "OK";
		     buttonCancel.Text = "Cancel";
		     buttonOk.DialogResult = DialogResult.OK;
		     buttonCancel.DialogResult = DialogResult.Cancel;

		     label.SetBounds(20, 17, 450, 20);	//(int x, int y, int width, int height);
		     buttonOk.SetBounds(20, 155, 200, 20);
		     buttonCancel.SetBounds(270, 155, 200, 20);

			listView.Name = "�κ� ��Ƽ ������";
			// Select the item and subitems when selection is made.
			listView.FullRowSelect = true;
			listView.BackColor = Color.Black;
			listView.ForeColor = Color.FromArgb(0, 255, 0);
			listView.View = View.Details;
			//listView.Font = new Font("Arial", 10, FontStyle.Bold);
			form.Controls.AddRange(new Control[] { label, buttonOk, buttonCancel, listView });

			listView.BeginUpdate();

			 //Add columns to the ListView:
			listView.Columns.Add(">>> ���� ��Ƽ ���� ���� ���� [������] <<<", 300, HorizontalAlignment.Center);
			listView.Columns.Add(">>> BattleTag <<<", 150, HorizontalAlignment.Center);
			listView.OwnerDraw = false;
			// ������ �κ� �������� ���� ���� �� �ߺ� ����
			int loopcnt = WebAds.GetLength(0)-1;
			try {
			for (int i = 0; i < WebAds.GetLength(0)-1; i++)
	          {
		          for (int j = 0; j < loopcnt; j++)
		          {
		          		int c = string.Compare(WebAds[j, 0], WebAds[j+1, 0]);
		               if (c == -1)	// ���ڰ� ���ں��� ������
		               {
						string temp1 = WebAds[j, 0];
						string temp2 = WebAds[j, 1];
						WebAds[j, 0] = WebAds[j+1, 0];
						WebAds[j, 1] = WebAds[j+1, 1];
						WebAds[j+1, 0] = temp1;
						WebAds[j+1, 1] = temp2;
		               } if (c == 0)		// �� ����� ������
		               {
		               		c = string.Compare(WebAds[j, 1], WebAds[j+1, 1]);
		               		if (c == 0)
		               		{
		               			WebAds[j+1, 0] = string.Empty;
	                	     		WebAds[j+1, 1] = string.Empty;
						}
		               }
		          }
		          loopcnt--;
		          if (loopcnt == 0) break;
		      }
		      }
		      catch {}
		      
			// ���ǿ� �´� ����� ����� �� ����� �ø� ������ listView�� item �߰�
			for (int i = 0; i <= WebAds.GetUpperBound(0); i++)	// WebAds.GetLength(0)�� ����
		     {
				if (WebAds[i, 0] != string.Empty)
				{
					listView.Items.Add(WebAds[i, 0]);
					listView.Items[i].SubItems.Add(WebAds[i, 1]);
				} else
				{

					WebAds[i, 1] = string.Empty;
				}
			}
			listView.EndUpdate();
			listView.SelectedIndexChanged += new System.EventHandler(listView_SelectedIndexChanged);

			DialogResult dialogResult = form.ShowDialog();
		     return dialogResult;
		}

		private void listView_SelectedIndexChanged(object sender, EventArgs e)
		{
			ListView listView = (ListView) sender;

			if(listView.SelectedItems.Count == 0)
			    return;

			BaTag = listView.SelectedItems[0].SubItems[1].Text;
		}

	     public void DoClick(Object source, System.Timers.ElapsedEventArgs e)
	     {
               Cursor.Position = new Point(Hud.Window.Size.Width / 2, Hud.Window.Size.Height / 2 - 30);
	          Process.Start("D:\\Game\\click.exe");
	     }

         public void OnKeyEvent(IKeyEvent keyEvent)
         {
            if (Hud.Input.IsKeyDown(Keys.NumPad2))	// �κ� ���� ���� �˻� ���� �Է� â ȣ��
            {
			string value = string.Empty;
			string output = string.Empty;
			// And �� Or �˻� ���� ���� �ʱ�ȭ
			for (int i = 0; i < ChatWatchListOr.Length; i++ )
			{
				ChatWatchListOr[i] = string.Empty;
			}
			for (int i = 0; i < ChatWatchListAnd.Length; i++ )
			{
				ChatWatchListAnd[i] = string.Empty;
			}
			if (InputOK)	// �� ���� �˻� �Է��� �� ���¶��
				value = savedValue;

			// ���� �ð� �� �� �ڵ� Ȱ��ȭ
			ClickTimer = new System.Timers.Timer();
			ClickTimer.Interval = 50;
			ClickTimer.Elapsed += DoClick;
			ClickTimer.AutoReset = false;
			ClickTimer.Enabled = true;

			if(InputBox("�κ� ��Ƽ ���� �˻���", "Or : comma/space, And : ( Or )", ref value) == DialogResult.OK)
			{
				Console.Beep(200, 120);
			     string sep = ", ";
			     value = value.Trim();
			     if (value == string.Empty)
			     {
			     		InputOK = false;
			     		try {
						WebBBSearchTimer.Enabled = false;
						WebBBSearchTimer.AutoReset = false;
					}
					catch {}
			     		return;
			     } else if (savedValue != value)
			     {
			     		InputChanged = true;
					for (int i = 0; i < WebAds.GetLength(0); i++) // �Է� ������ �ٲ�� �κ� �������� ���� �� ���� Array �ʱ�ȭ
		         		{
			        		WebAds[i, 0] = string.Empty;
		                	WebAds[i, 1] = string.Empty;
					}
				}

			     savedValue = value;
			     Match match = Regex.Match(savedValue, @"(?<=\().+(?=\))");		// extract "And" condition words
			     if (match.Success)
				{
					ChatWatchListAnd = match.Value.Split(sep.ToCharArray());
					output = Regex.Replace(value, @"\(.+\) ", string.Empty);	// delete And condition for Or processing
				} else
					output = value;

			     ChatWatchListOr = output.Split(sep.ToCharArray());
			     InputOK = true;

				// �κ� ��Ƽ������ �������� ���� �ֱ�� ��� �������
				WebBBSearchTimer = new System.Timers.Timer();
				WebBBSearchTimer.Interval = 1000;		// first in 1 sec and then search Web bulletin boards every WebBBSearchInterval
				WebBBSearchTimer.Elapsed += WebBBListSearch;
				WebBBSearchTimer.AutoReset = true;
				WebBBSearchTimer.Enabled = true;
			 }
             }
            if (Hud.Input.IsKeyDown(Keys.NumPad4))	// �κ� ���� ���� �� ���� �����ִ� ListView ȣ��
            {
            	ClickTimer = new System.Timers.Timer();
			ClickTimer.Interval = 50;
			ClickTimer.Elapsed += DoClick;
			ClickTimer.AutoReset = false;
			ClickTimer.Enabled = true;

            	if(listView_Doit("�κ� ������Ƽ ����", "�����ϸ� ������ ��Ʋ�±� Ŭ�����忡 �ڵ� ����(ģ�� �� ctrl_v)") == DialogResult.OK)
            	{
            		Clipboard.SetText(BaTag);
            		// �ڵ����� ģ�� �߰� �������� �̵� �� ��Ʋ �±� �ڵ� ������ �ֱ�
            		SendKeys.SendWait("+(i)");	// ģ�� â ����Ű
            		Cursor.Position = new Point(1560, 900);	//ģ�� â ȭ�鿡�� "ģ�� �߱�" ��ư�� ��ġ
	          		Process.Start("D:\\Game\\click.exe");	// Just click the button
	          		Thread.Sleep(500);						// 0.5�� �����
	          		SendKeys.SendWait("^(v)");				// Ŭ������ ������ ��ĭ�� ������ �־��
            		Hud.Sound.Speak("ģ�� �߰� ��û�� ��������!");
            		//Hud.Sound.Speak("�ش� ��Ʋ�ױװ� Ŭ�����忡 ����Ǿ����ϴ�!");
            	}
            }
          }

		// �κ� �������� �˻� ���� �Է� ��
		public DialogResult InputBox(string title, string content, ref string value)
		{
		    Form form = new Form();
		    Label label = new Label();
		    TextBox textBox = new TextBox();
		    Button buttonOk = new Button();
		    Button buttonCancel = new Button();

		    form.ClientSize = new Size(250, 100);		// 250, 100
		    form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
		    form.FormBorderStyle = FormBorderStyle.FixedDialog;
		    form.StartPosition = FormStartPosition.CenterScreen;
		    form.MaximizeBox = false;
		    form.MinimizeBox = false;
		    form.TopMost = true;
		    form.AcceptButton = buttonOk;
		    form.CancelButton = buttonCancel;

		    form.Text = title;
		    label.Text = content;
		    textBox.Text = value;
		    buttonOk.Text = "OK";
		    buttonCancel.Text = "Cancel";

		    buttonOk.DialogResult = DialogResult.OK;
		    buttonCancel.DialogResult = DialogResult.Cancel;

		    label.SetBounds(20, 17, 210, 20);	//(int x, int y, int width, int height);
		    textBox.SetBounds(20, 40, 210, 20);
		    buttonOk.SetBounds(20, 70, 90, 20);
		    buttonCancel.SetBounds(140, 70, 90, 20);

		    DialogResult dialogResult = form.ShowDialog();
		    value = textBox.Text;

		    return dialogResult;
		}
   }
}