using Game;
using System;
using Engine.Input;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Engine.Graphics;
using Engine;
using Engine.Media;

namespace API_WE_Mod
{
    public class WEAction
    {
        public enum ActionType
        {
            Functionality,
            Operation,
            Extra
        }
        
        private bool m_checked;

        public string Name;
        public bool Checkable = false;
        public bool Checked
        {
            get
            {
                if (ButtonWidget == null)
                    return m_checked;
                return ButtonWidget.IsChecked;
            }
            set
            {
                if (ButtonWidget == null)
                    m_checked = value;
                else
                    ButtonWidget.IsChecked = value && Checkable;
            }
        }
        public ActionType Type;
        public Action Activate;
        public bool ActivateWhenUnchecked;
        public Action Deactivate = null;
        public Key Key = (Key) (-1);

        public Func<string> Problem = null;

        public Func<bool, string> Prefix = null;

        public string ButtonText;

        public Texture2D Icon;
        public Texture2D PressedIcon;

        private BitmapButtonWidget buttonWidget;
        public BitmapButtonWidget ButtonWidget
        {
            get => buttonWidget;
            set
            {
                buttonWidget = value;
                if (value != null)
                    ApplyOnButton(buttonWidget);
            }
        }

        public void ApplyOnButton(BitmapButtonWidget btn)
        {
            btn.IsAutoCheckingEnabled = Deactivate != null;
            btn.NormalSubtexture = new Subtexture(Icon, Vector2.Zero, Vector2.One);
            btn.ClickedSubtexture = new Subtexture(PressedIcon, Vector2.Zero, Vector2.One);
            btn.IsChecked = Checkable && Checked;
            btn.IsAutoCheckingEnabled = Checkable;
        }

        public void Interact()
        {
            if (!Checkable || !Checked || ActivateWhenUnchecked)
                Activate?.Invoke();
            if (Checkable && Checked)
                Deactivate?.Invoke();
            if (ButtonWidget == null && Checkable)
                Checked = !Checked;
        }

        public void RenderIcons(Texture2D logo)
        {
            var oldRenderTarget = Display.RenderTarget;
            PrimitivesRenderer2D renderer2D = new PrimitivesRenderer2D();
            Display.RenderTarget = new RenderTarget2D(68, 64, 1, ColorFormat.Rgba8888, DepthFormat.None);
            Display.Clear(Color.Transparent, 1f);
            renderer2D.TexturedBatch(ContentManager.Get<Texture2D>("WE/Button"), blendState: BlendState.AlphaBlend).QueueQuad(Vector2.Zero, new Vector2(68, 64), 0f, Vector2.Zero, Vector2.One, Color.White);
            renderer2D.TexturedBatch(logo, blendState: BlendState.AlphaBlend).QueueQuad(Vector2.Zero, new Vector2(68, 64), 0f, Vector2.Zero, Vector2.One, Color.White);
            renderer2D.Flush();
            Icon = Display.RenderTarget;

            Display.RenderTarget = new RenderTarget2D(68, 64, 1, ColorFormat.Rgba8888, DepthFormat.None);
            Display.Clear(Color.Transparent, 1f);
            renderer2D.TexturedBatch(ContentManager.Get<Texture2D>("WE/PressedButton"), blendState: BlendState.AlphaBlend).QueueQuad(Vector2.Zero, new Vector2(68, 64), 0f, Vector2.Zero, Vector2.One, Color.White);
            renderer2D.TexturedBatch(logo, blendState: BlendState.AlphaBlend, layer: 1).QueueQuad(Vector2.Zero, new Vector2(68, 64), 0f, Vector2.Zero, Vector2.One, Color.White);
            renderer2D.Flush();
            PressedIcon = Display.RenderTarget;
            
            Display.RenderTarget = oldRenderTarget;
        }

        public static BitmapFont font;

        public void RenderIcons(string text)
        {
            var oldRenderTarget = Display.RenderTarget;
            if (font == null)
            {
                font = ContentManager.Get<BitmapFont>("WE/Fonts/Pericles");
                /*for (int i = 0; i < font.MaxGlyphCode; i++)
                {
                    BitmapFont.Glyph glyph = font.GetGlyph((char)i);
                    typeof(BitmapFont.Glyph).GetField("TexCoord1", BindingFlags.Instance | BindingFlags.Public).SetValue(glyph, glyph.TexCoord1 + new Vector2(0f, 17f / 4624f));
                }*/
            }
            Vector2 spaceAvailable = new Vector2(48f, 36f);
            float scale = 10f;
            Vector2 textSize = font.MeasureText(text, new Vector2(scale), Vector2.Zero);
            while (textSize.X > spaceAvailable.X || (textSize.Y/62f)*40f > spaceAvailable.Y)
            {
                scale -= 1f/40f;
                textSize = font.MeasureText(text, new Vector2(scale), Vector2.Zero);
            }

            PrimitivesRenderer2D renderer2D = new PrimitivesRenderer2D();
            Display.RenderTarget = new RenderTarget2D(68, 64, 1, ColorFormat.Rgba8888, DepthFormat.None);
            Display.Clear(Color.Transparent, 1f);
            renderer2D.TexturedBatch(ContentManager.Get<Texture2D>("WE/Button"), blendState: BlendState.AlphaBlend).QueueQuad(Vector2.Zero, new Vector2(68, 64), 0f, Vector2.Zero, Vector2.One, Color.White);
            renderer2D.FontBatch(font, layer: 1, samplerState: SamplerState.LinearClamp).QueueText(text, new Vector2(34, 30), 0f, Color.White, TextAnchor.HorizontalCenter | TextAnchor.VerticalCenter, new Vector2(scale), Vector2.Zero);
            renderer2D.Flush();
            Icon = Display.RenderTarget;

            Display.RenderTarget = new RenderTarget2D(68, 64, 1, ColorFormat.Rgba8888, DepthFormat.None);
            renderer2D.TexturedBatch(ContentManager.Get<Texture2D>("WE/PressedButton"), blendState: BlendState.AlphaBlend).QueueQuad(Vector2.Zero, new Vector2(68, 64), 0f, Vector2.Zero, Vector2.One, Color.White);
            renderer2D.FontBatch(font, layer: 1, samplerState: SamplerState.LinearClamp).QueueText(text, new Vector2(34, 30), 0f, Color.White, TextAnchor.HorizontalCenter | TextAnchor.VerticalCenter, new Vector2(scale), Vector2.Zero);
            renderer2D.Flush();
            PressedIcon = Display.RenderTarget;
            Display.RenderTarget = oldRenderTarget;
        }

        public WEAction(
			string name,
			string buttonText,
			ActionType actionType,
			Action activate,
			Action deactivate=null,
			Func<string> problem=null,
			bool activateWhenUnchecked=false,
			bool checkable=false,
			bool defaultCheck=false,
			Func<bool, string> prefix=null
		)
        {
            Name = name;
            Type = actionType;
            Activate = activate;
            Deactivate = deactivate;
            ButtonText = buttonText;
            Problem = problem;
            Prefix = prefix ?? ((bool enabled) => enabled ? LanguageControl.Disable : LanguageControl.Enable);
            Checkable = checkable;
            ActivateWhenUnchecked = activateWhenUnchecked;
            Checked = defaultCheck;
            RenderIcons(buttonText);
        }

        public WEAction(
            string name,
            Texture2D logo,
            ActionType actionType,
            Action activate,
            Action deactivate = null,
            Func<string> problem=null,
            bool activateWhenUnchecked = false,
            bool checkable = false,
            bool defaultCheck = false,
            Func<bool, string> prefix = null
        )
        {
            Name = name;
            Type = actionType;
            Activate = activate;
            Deactivate = deactivate;
            Problem = problem;
            Prefix = prefix ?? ((bool enabled) => enabled ? LanguageControl.Disable : LanguageControl.Enable);
            Checkable = checkable;
            ActivateWhenUnchecked = activateWhenUnchecked;
            Checked = defaultCheck;
            RenderIcons(logo);
        }
    }
}
