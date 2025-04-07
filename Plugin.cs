using BepInEx;
using BepInEx.Logging;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using BepInEx.Bootstrap;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace GFModMenu;

[BepInDependency(DependencyGUID:"GFApi", Flags:BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public static string path = Path.Combine(Paths.PluginPath, "GFModMenu", "modmenu");
    public static AssetBundle bundle = AssetBundle.LoadFromFile(path);
    GameObject menu = (GameObject)bundle.LoadAsset("assets/mod-menu.prefab");
    GameObject mod = (GameObject)bundle.LoadAsset("assets/mod.prefab");
    Dictionary<string, PluginInfo> info = Chainloader.PluginInfos;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Logger.LogInfo(Path.Combine(Path.GetDirectoryName(info["GFModMenu"].Location), "icon.png"));
    }

    private void OnEnable(){
        GFApi.MainPlugin.OnGameLoad.AddListener(Init);
    }

    private void Init()
    {
        if(GFApi.MainPlugin.currentScene == GameData.biomeList.MainMenu){
            GameObject settingsButton = GameObject.Find("Train-Button-Settings").GetComponentInChildren<VanillaButton>().gameObject;
            GameObject hud = GameObject.Find("HUD");
            GameObject settingsMenu = hud.transform.GetChild(0).gameObject;
            GameObject backButton = settingsMenu.transform.GetChild(0).GetChild(0).gameObject;
            Menu modMenuMenu = Instantiate(backButton.GetComponent<VanillaButton>().menu);
            GameObject modButton = Instantiate(settingsButton, new Vector3(0, 0, 0), Quaternion.identity, GameObject.Find("Main-Section").transform);
            GameObject modMenu = Instantiate(menu, hud.transform);
            GameObject modBackButton = Instantiate(backButton, modMenu.transform);
            ModMenuOpen modMenuAction = new ModMenuOpen();

            modMenuMenu.name = "Mod-Menu";
            modMenuMenu.closeAction = modBackButton.GetComponent<ChangeMenu>();
            GFApi.Helper.HelperFunctions.CopyComponent(modMenuMenu, modMenu);

            modButton.name = "Mod-Menu";
            modButton.GetComponentInChildren<TextMeshPro>().text = "Mods";
            modButton.GetComponent<ChangeMenu>().menuToOpen = modMenu;
            modButton.GetComponent<ChangeMenu>().secondaryAction = modMenuAction;
            modButton.GetComponent<VanillaButton>().leftSelectable = null;
            modButton.GetComponent<VanillaButton>().rightSelectable = null;
            modButton.GetComponent<VanillaButton>().downSelectable = settingsButton.GetComponent<VanillaButton>();

            settingsButton.GetComponent<VanillaButton>().upSelectable = modButton.GetComponent<VanillaButton>();

            modBackButton.GetComponent<ChangeMenu>().menuToClose = modMenu;
            modBackButton.GetComponent<StopMainMenu>().menu = modMenuMenu;
            modBackButton.GetComponent<VanillaButton>().menu = modMenu.GetComponent<Menu>();

            modMenu.transform.GetComponentInChildren<TextMeshProUGUI>().font = settingsButton.GetComponentInChildren<TextMeshPro>().font;
            modMenu.transform.GetComponentInChildren<TextMeshProUGUI>().characterSpacing = -15;

            modMenu.SetActive(false);
            //modBackButton.SetActive(false);

            foreach(KeyValuePair<string, PluginInfo> plugin in info){
                var modPanel = Instantiate(mod, modMenu.transform.GetChild(1).GetChild(0));
                var icon = modPanel.transform.GetChild(1).GetComponent<Image>();
                var text = modPanel.GetComponentInChildren<TextMeshProUGUI>();
                text.text = plugin.Value.Metadata.Name + " v" + plugin.Value.Metadata.Version;
                text.font = settingsButton.GetComponentInChildren<TextMeshPro>().font;
                text.characterSpacing = -15;
                if(GFApi.Helper.HelperFunctions.LoadTexFromFile(Path.Combine(Path.GetDirectoryName(plugin.Value.Location), "icon.png")))
                    icon.sprite = GFApi.Helper.HelperFunctions.LoadSpriteFromFile(Path.Combine(Path.GetDirectoryName(plugin.Value.Location), "icon.png"));
                else
                    icon.sprite = GFApi.Helper.HelperFunctions.LoadSpriteFromFile(Path.Combine(Paths.PluginPath, "GFModMenu", "default_icon.png"));
            }
        }
    }
}

public class ModMenuOpen : ButtonAction{
    public override void Action()
    {
        base.Action();
        GameObject slider = GameObject.Find("Mod-Menu").transform.GetChild(0).gameObject;
        EventSystem.current.SetSelectedGameObject(slider);
    }
}
