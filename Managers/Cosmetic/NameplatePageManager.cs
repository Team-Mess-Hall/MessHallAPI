/*
using Il2CppSG.Airlock.Minigames;
using Il2CppSG.Airlock.XR;
using Il2CppSG.LightUI;
using Il2CppTMPro;
using MessHallAPI.Debugger;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MessHallAPI.Base.References;

namespace MessHallAPI.Managers.Cosmetic
{
   public static class NameplatePageManager
   {
        private static GameObject _nameplatesGrid;
        private static string _sidebarPath = "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/CustomizationMenuParent/Customization_Menu/Menu_Parent/UI_Customization_Platform/UI_PlayerCustomization_StoreWardrobe_3D/Main Panel/HorizontalLayout/Sidebars/Wardrobe Sidebar";
        private static string _hatsGridPath = "-------- VR MANAGEMENT --------/XRRig_Gameplay/UI/3DHUD_Canvas/CustomizationMenuParent/Customization_Menu/Menu_Parent/UI_Customization_Platform/UI_PlayerCustomization_StoreWardrobe_3D/Main Panel/HorizontalLayout/Main Section/Hats Grid/4x4 Grid";

        public static void Setup()
        {
           Logging.Log("NameplatePageManager: Setup started");
           SetupNameplatesGrid();
           SetupButtons();
           Logging.Log("NameplatePageManager: Setup complete");
        }

        private static void SetupNameplatesGrid()
        {
           GameObject hatsGrid = GameObject.Find(_hatsGridPath);
           if (hatsGrid == null) { Logging.Log("NameplatePageManager: hatsGrid NULL"); return; }
           Logging.Log("NameplatePageManager: hatsGrid FOUND");

           _nameplatesGrid = GameObject.Instantiate(hatsGrid, hatsGrid.transform.parent);
           _nameplatesGrid.name = "Nameplates Grid";
           _nameplatesGrid.SetActive(false);

           var nameplates = NameplateRegistry.GetAll().ToList();
           var buttons = _nameplatesGrid.GetComponentsInChildren<LUIButton>().ToList();
           Logging.Log($"NameplatePageManager: {nameplates.Count} nameplates, {buttons.Count} buttons");

           for (int i = 0; i < buttons.Count; i++)
           {
               if (i >= nameplates.Count)
               {
                   buttons[i].gameObject.SetActive(false);
                   continue;
               }

               string capturedMod = nameplates[i].Key.Item1;
               string capturedId = nameplates[i].Key.Item2;
               Sprite sprite = nameplates[i].Value;
               Logging.Log($"NameplatePageManager: Setting up button {i} -> {capturedMod}/{capturedId}");

               MeshRenderer mesh = buttons[i].GetComponentInChildren<MeshRenderer>();
               if (mesh != null)
               {
                   mesh.sharedMaterial.mainTexture = sprite.texture;
                   Logging.Log($"NameplatePageManager: Mesh texture set for button {i}");
               }
               else Logging.Log($"NameplatePageManager: No MeshRenderer on button {i}");

               buttons[i].OnPressed.RemoveAllListeners();
               Action onPressed = () =>
               {
                   Logging.Log($"NameplatePageManager: Nameplate button pressed {capturedMod}/{capturedId}");
                   CustomNameplateManager.RPC_SetNameplate(Client.PState.PlayerId, capturedMod, capturedId);
               };
               buttons[i].OnPressed.AddListener(onPressed);
           }
        }

        private static void SetupButtons()
        {
           Logging.Log("NameplatePageManager: SetupButtons started");

           GameObject hatsBtn = GameObject.Find($"{_sidebarPath}/HatsSectionButton");
           if (hatsBtn == null) { Logging.Log("NameplatePageManager: HatsSectionButton NULL"); return; }

           GameObject nameplateBtn = GameObject.Instantiate(hatsBtn, hatsBtn.transform.parent);
           nameplateBtn.transform.localPosition = new Vector3(0.2f, -0.4283f, 0);
           nameplateBtn.name = "NameplatesSectionButton";
           Logging.Log("NameplatePageManager: NameplatesSectionButton created");

           var tmp = nameplateBtn.transform.Find("LUI_Button_Frame")?.GetComponentInChildren<TextMeshPro>();
           if (tmp != null) { tmp.text = "NAMEPLATES"; Logging.Log("NameplatePageManager: Label set"); }
           else Logging.Log("NameplatePageManager: No TextMeshPro found on button");

           // LUIButton on nameplates tab
           LUIButton nameplateLUIBtn = GameObject.Find($"{_sidebarPath}/NameplatesSectionButton/LUI_Button_Frame")?.GetComponent<LUIButton>();
           if (nameplateLUIBtn != null)
           {
               nameplateLUIBtn.OnPressed.RemoveAllListeners();
               Action onLUIPressed = () =>
               {
                   Logging.Log("NameplatePageManager: Nameplates tab clicked (LUIButton)");
                   GameObject.Find(_hatsGridPath)?.SetActive(false);
                   if (_nameplatesGrid != null) _nameplatesGrid.SetActive(true);
               };
               nameplateLUIBtn.OnPressed.AddListener(onLUIPressed);
               Logging.Log("NameplatePageManager: LUIButton listener added");
           }
           else Logging.Log("NameplatePageManager: No LUIButton on nameplates tab");

           // MinigameButton on nameplates tab
           MinigameButton nameplateMinigameBtn = GameObject.Find($"{_sidebarPath}/NameplatesSectionButton/LUI_Button_Frame")?.GetComponent<MinigameButton>();
           if (nameplateMinigameBtn != null)
           {
               nameplateMinigameBtn.OnButtonPressed.RemoveAllListeners();
               Action<XRHand> onMinigamePressed = (XRHand hand) =>
               {
                   Logging.Log("NameplatePageManager: Nameplates tab clicked (MinigameButton)");
                   GameObject.Find(_hatsGridPath)?.SetActive(false);
                   if (_nameplatesGrid != null) _nameplatesGrid.SetActive(true);
               };
               nameplateMinigameBtn.OnButtonPressed.AddListener(onMinigamePressed);
               Logging.Log("NameplatePageManager: MinigameButton listener added");
           }
           else Logging.Log("NameplatePageManager: No MinigameButton on nameplates tab");

           // Restore listeners on other tab buttons
           foreach (string btnName in new[] { "HatsSectionButton", "GlovesSectionButton", "SkinsSectionButton", "ColorsSectionButton" })
           {
               MinigameButton tabBtn = GameObject.Find($"{_sidebarPath}/{btnName}/LUI_Button_Frame")?.GetComponent<MinigameButton>();
               if (tabBtn == null) { Logging.Log($"NameplatePageManager: {btnName} MinigameButton NULL"); continue; }
               Action<XRHand> onTabPressed = (XRHand hand) =>
               {
                   Logging.Log($"NameplatePageManager: {btnName} clicked, restoring hats grid");
                   GameObject.Find(_hatsGridPath)?.SetActive(true);
                   if (_nameplatesGrid != null) _nameplatesGrid.SetActive(false);
               };
               tabBtn.OnButtonPressed.AddListener(onTabPressed);
               Logging.Log($"NameplatePageManager: {btnName} restore listener added");
           }
        }
   }
}
*/
