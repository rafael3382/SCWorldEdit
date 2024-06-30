using System.Collections.Generic;
using System.Xml.Linq;
using TemplatesDatabase;

namespace Game
{
	public class ModifyWorldDialog : Dialog
	{
		public TextBoxWidget m_nameTextBox;

		public TextBoxWidget m_textSeedTextBox;
		
		public TextBoxWidget m_numSeedTextBox;
		
		public ButtonWidget m_gameModeButton;

		public ButtonWidget m_worldOptionsButton;

		public LabelWidget m_descriptionLabel;

		public ButtonWidget m_applyButton;

		public ButtonWidget m_cancelButton;

		public WorldSettings m_worldSettings;

		public ValuesDictionary m_currentWorldSettingsData = new ValuesDictionary();

		public ValuesDictionary m_originalWorldSettingsData = new ValuesDictionary();

		public bool m_changingGameModeAllowed;
		
		public int modifiedWorldSeed;

		public ModifyWorldDialog(WorldSettings worldSettings)
		{
			XElement node = ContentManager.Get<XElement>("WE/DialogsWE/ModifyWorldDialog");
			LoadContents(this, node);
			m_nameTextBox = Children.Find<TextBoxWidget>("Name");
			m_textSeedTextBox = Children.Find<TextBoxWidget>("TextSeed");
			m_numSeedTextBox = Children.Find<TextBoxWidget>("NumSeed");
			m_gameModeButton = Children.Find<ButtonWidget>("GameMode");
			m_worldOptionsButton = Children.Find<ButtonWidget>("WorldOptions");
			m_descriptionLabel = Children.Find<LabelWidget>("Description");
			m_applyButton = Children.Find<ButtonWidget>("Dialog.OK");
			m_cancelButton = Children.Find<ButtonWidget>("Dialog.Cancel");
			m_nameTextBox.TextChanged += delegate
			{
				m_worldSettings.Name = m_nameTextBox.Text;
			};
			m_textSeedTextBox.TextChanged += delegate
			{
				m_worldSettings.Seed = m_textSeedTextBox.Text;
			};
			m_numSeedTextBox.TextChanged += delegate
			{
			    if (int.TryParse(m_numSeedTextBox.Text, out int seed))
			    {
				    modifiedWorldSeed = seed;
				}
			};
			// Soft copy, some things may still be shared though...
			worldSettings.Save(m_originalWorldSettingsData, false);
			m_worldSettings = new WorldSettings();
			m_worldSettings.Load(m_originalWorldSettingsData);
			
			modifiedWorldSeed = GameManager.Project.FindSubsystem<SubsystemGameInfo>().WorldSeed;
		}
		
		public GameMode? lastGameMode;

		public override void Update()
		{
		    if (lastGameMode.HasValue)
		    {
		        m_worldSettings.GameMode = lastGameMode.Value;
		        lastGameMode = null;
		    }
			if (m_gameModeButton.IsClicked)
			{
				DialogsManager.ShowDialog(null, new SelectGameModeDialog(string.Empty, allowAdventure: true, delegate(GameMode gameMode)
				{
					m_worldSettings.GameMode = gameMode;
				}));
			}
			m_currentWorldSettingsData.Clear();
			m_worldSettings.Save(m_currentWorldSettingsData, liveModifiableParametersOnly: false);
			bool flag = !CompareValueDictionaries(m_originalWorldSettingsData, m_currentWorldSettingsData);
			m_nameTextBox.Text = m_worldSettings.Name;
			m_textSeedTextBox.Text = m_worldSettings.Seed;
			m_numSeedTextBox.Text = modifiedWorldSeed.ToString();
			m_gameModeButton.Text = LanguageControl.Get("GameMode", m_worldSettings.GameMode.ToString());
			m_descriptionLabel.IsVisible = true;
			m_applyButton.IsEnabled = true;
			m_descriptionLabel.Text = StringsManager.GetString("GameMode." + m_worldSettings.GameMode.ToString() + ".Description");
			if (m_worldOptionsButton.IsClicked)
			{
			    lastGameMode = m_worldSettings.GameMode;
			    m_worldSettings.GameMode = GameMode.Creative;
				ScreensManager.SwitchScreen("WorldOptions", m_worldSettings, false);
			}
			if (m_applyButton.IsClicked && flag)
            {
			    var componentGameInfo = GameManager.Project.FindSubsystem<SubsystemGameInfo>();
			    DialogsManager.HideDialog(this);
			    componentGameInfo.WorldSettings = m_worldSettings;
			    componentGameInfo.WorldSeed = modifiedWorldSeed;
			    GameManager.SaveProject(true, true);
			    GameManager.DisposeProject();
			    WorldInfo worldInfo = WorldsManager.GetWorldInfo(componentGameInfo.DirectoryName);
		        ScreensManager.SwitchScreen("GameLoading", worldInfo, null);
		        
			}
			if (base.Input.Back || base.Input.Cancel || m_cancelButton.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
			
		}

		public static bool CompareValueDictionaries(ValuesDictionary d1, ValuesDictionary d2)
		{
			if (d1.Count != d2.Count)
			{
				return false;
			}
			foreach (KeyValuePair<string, object> item in d1)
			{
				object value = d2.GetValue<object>(item.Key, null);
				if (value is ValuesDictionary d3)
				{
					if (!(item.Value is ValuesDictionary d4) || !CompareValueDictionaries(d3, d4))
					{
						return false;
					}
				}
				else if (!object.Equals(value, item.Value))
				{
					return false;
				}
			}
			return true;
		}
	}
}
