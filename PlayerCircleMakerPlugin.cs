// User defined Circle drawing
// Call inputbox : Numpad *(multiply)
// User input contents: Circle_radius, rgb_r, rgb_g, rgb_b, StrokeWidth <-- Should be seperated by space(" ") or comma(",")
// Error-proof user input
using System;
using Turbo.Plugins.Default;
using System.Windows.Forms;
using SharpDX.DirectInput;
using System.Drawing;
using System.Diagnostics;

namespace Turbo.Plugins.James
{
    public class PlayerCircleMakerPlugin : BasePlugin, IKeyEventHandler, IInGameWorldPainter
    {
        public WorldDecoratorCollection UserRadiusDashDecorator { get; set; }
        public WorldDecoratorCollection UserRadiusSolidDecorator { get; set; }
    	   public IKeyEvent PressKeyEvent { get; set; }		// Numpad *
    	   public int UserRadius { get; set; }
    	   public int UserRGB_R { get; set; }
    	   public int UserRGB_G { get; set; }
    	   public int UserRGB_B { get; set; }
    	   public bool UserDashStyle { get; set; }
    	   public float UserStrokeWidth { get; set; }
    	   private bool Circle { get; set; }
    	   private static System.Timers.Timer ClickTimer;

        public PlayerCircleMakerPlugin()
        {
            Enabled = true;
            Circle = false;
            UserRadius = 20;
            UserRGB_R = 255;
            UserRGB_G = 0;
            UserRGB_B = 0;
            UserStrokeWidth = 2.0f;
            UserDashStyle = false;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

            PressKeyEvent = Hud.Input.CreateKeyEvent(true, Key.Multiply, false, false, false);

            UserRadiusDashDecorator = new WorldDecoratorCollection(new GroundCircleDecorator(Hud)
            {
                Brush = Hud.Render.CreateBrush(155, UserRGB_R, UserRGB_G, UserRGB_B, UserStrokeWidth, SharpDX.Direct2D1.DashStyle.Dash),
                Radius = UserRadius,
                Enabled = true
            });
            UserRadiusSolidDecorator = new WorldDecoratorCollection(new GroundCircleDecorator(Hud)
            {
                Brush = Hud.Render.CreateBrush(155, UserRGB_R, UserRGB_G, UserRGB_B, UserStrokeWidth),
                Radius = UserRadius,
                Enabled = true
            });            
        }

        public void PaintWorld(WorldLayer layer)
        {
            var player = Hud.Game.Me;

            if (Circle)
            {
               if (UserDashStyle)
                  UserRadiusDashDecorator.Paint(layer, player, player.FloorCoordinate, string.Empty);
               else
               	   UserRadiusSolidDecorator.Paint(layer, player, player.FloorCoordinate, string.Empty);
            }
        }

	     public void DoClick(Object source, System.Timers.ElapsedEventArgs e)
	     {
               Cursor.Position = new Point(Hud.Window.Size.Width / 2, Hud.Window.Size.Height / 2 - 30);
	          Process.Start("D:\\Game\\click.exe");
	     }
	     
        public void OnKeyEvent(IKeyEvent keyEvent)
        {
            if (keyEvent.IsPressed && PressKeyEvent.Matches(keyEvent))
            {
            	ClickTimer = new System.Timers.Timer();
			ClickTimer.Interval = 50;
			ClickTimer.Elapsed += DoClick;
			ClickTimer.AutoReset = false;
			ClickTimer.Enabled = true;

			string value = "0 to clear circle";
			if(InputBox("Drawing Circle", "Yard,R,G,B,SW,Dash(d)", ref value) == DialogResult.OK)
			{
			    string sep = ", ";
			    value = value.Trim();
			    string[] CircleInfoList = value.Split(sep.ToCharArray());
			    var cnt = 0;
			    bool Success;
			    int number;
			    UserDashStyle = false;

			    foreach (string CircleInfo in CircleInfoList)
			    {
			    		cnt++;
			    		string tmp = CircleInfo;
			    		switch(cnt)
			    		{
			    			case 1:
			    				Success = Int32.TryParse(tmp, out number);
			    				if ((Success && number == 0) || !Success)
			    					Circle = false;
			    				else
			    					Circle = true;
			    				if (Success && number >= 1 && number <= 60)
			    					UserRadius = number;
			    				else
			    				   	UserRadius = 20;
			    				break;
			    			case 2:
			    				Success = Int32.TryParse(tmp, out number);
			    				if (Success)
			    					UserRGB_R = number;
			    				else
			    				   	UserRGB_R = 255;
			    				break;
			    			case 3:
			    				Success = Int32.TryParse(tmp, out number);
			    				if (Success)
			    					UserRGB_G = number;
			    				else
			    				   	UserRGB_G = 0;
			    				break;
			    			case 4:
			    				Success = Int32.TryParse(tmp, out number);
			    				if (Success)
			    					UserRGB_B = number;
			    				else
			    				   	UserRGB_B = 0;
			    				break;
			    			case 5:
			    			     float fnum;
			    				Success = float.TryParse(tmp, out fnum);
			    				if (Success && (fnum >= 1.0f && fnum <= 20.0f))
			    					UserStrokeWidth = fnum;
			    				else
			    				   	UserStrokeWidth = 2.0f;
			    				break;
			    			case 6:
			    			     var style = tmp.ToLower();
			    				if (style == "d")
			    					UserDashStyle = true;
			    				break;				    				
			    		}
			    }

    			    if (UserRGB_R < 0 ||  UserRGB_R > 255 || UserRGB_G < 0 || UserRGB_G > 255 || UserRGB_B < 0 || UserRGB_B > 255)
    			    {
	    				UserRGB_R = 255;
	    				UserRGB_G = 0;
	    				UserRGB_B = 0;
	    			}

			    Load(Hud);
			}
            }
        }

		public static DialogResult InputBox(string title, string content, ref string value)
		{
		    Form form = new Form();
		    Label label = new Label();
		    TextBox textBox = new TextBox();
		    Button buttonOk = new Button();
		    Button buttonCancel = new Button();

		    form.ClientSize = new Size(180, 100);
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

		    label.SetBounds(20, 17, 140, 20);	//(int x, int y, int width, int height);
		    textBox.SetBounds(20, 40, 140, 20);
		    buttonOk.SetBounds(20, 70, 65, 20);
		    buttonCancel.SetBounds(90, 70, 70, 20);

		    DialogResult dialogResult = form.ShowDialog();

		    value = textBox.Text;
		    return dialogResult;
		}
   }
}