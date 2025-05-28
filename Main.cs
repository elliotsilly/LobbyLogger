using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using Photon.Pun;
using UnityEngine;
using TMPro;

namespace LobbyLog
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.ProjectName, PluginInfo.Version)]
    public class Main : BaseUnityPlugin
    {
        private string currentLobby;
        private string txtpath;
        private GameObject text;
        private string lastLobby;
        private TextMeshPro textComponent;

        private ConfigEntry<string> colorHexConfig;

        void Awake()
        {

            colorHexConfig = Config.Bind("Text Color", "Hex", "#FFFFFF", "Hex color code for the lobby logger UI");

            GorillaTagger.OnPlayerSpawned(Init);
        }

        private void Init()
        {
            NetworkSystem.Instance.OnMultiplayerStarted += OnJoinRoom;
            var dllpath = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(dllpath);
            txtpath = Path.Combine(dir, "lobbylog.txt");

            if (!File.Exists(txtpath))
            {
                File.Create(txtpath).Close();
            }

            text = new GameObject("LobbyLog");
            text.AddComponent<TextMeshPro>();
            textComponent = text.GetComponent<TextMeshPro>();
            text.transform.position = new Vector3(-68.7667f, 12.1527f, -83.2612f);
            text.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            text.transform.rotation = Quaternion.Euler(0f, 243.4958f, 0f);
            textComponent.font = GorillaTagger.Instance.offlineVRRig.playerText1.font;

            if (ColorUtility.TryParseHtmlString(colorHexConfig.Value, out Color parsedColor))
            {
                textComponent.color = parsedColor;
            }
            else
            {
                textComponent.color = Color.white;
                Logger.LogWarning($"Not valid hex color in config {colorHexConfig.Value}. Defaulting back to white.");
            }

            textComponent.fontSize = 5f;
        }

        void OnJoinRoom()
        {
            if (currentLobby != null)
            {
                lastLobby = currentLobby;
                textComponent.text = $"Lobby Logger\n{lastLobby}";
            }
            else
            {
                textComponent.text = "Lobby Logger\nJoin another lobby to display last lobby";
            }

            try
            {
                currentLobby = PhotonNetwork.CurrentRoom.Name;
                File.AppendAllText(txtpath, currentLobby + Environment.NewLine);
                Debug.Log($"joined: {currentLobby}");
            }
            catch (Exception anException)
            {
                Debug.LogException(anException);
            }
        }
    }
}

//Config changes made by elliot :3 
