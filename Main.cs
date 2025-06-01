using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace LobbyLog
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.ProjectName, PluginInfo.Version)]
    public class Main : BaseUnityPlugin
    {
        private string currentLobby;
        private string txtpath;
        private GameObject text;
        private TextMeshPro textComponent;

        private ConfigEntry<string> colorHexConfig;

        private bool isOutdatedVersion = false;

        void Awake()
        {
            colorHexConfig = Config.Bind("Text Color", "Hex", "#FFFFFF", "Hex color code for the lobby logger UI");
            StartCoroutine(runTheTask());
        }

        private void Init()
        {
            NetworkSystem.Instance.OnMultiplayerStarted += OnJoinRoom;

            var dir = BepInEx.Paths.BepInExRootPath;
            txtpath = Path.Combine(dir, "lobbylog.txt");

            if (!File.Exists(txtpath))
                File.Create(txtpath).Close();

            text = new GameObject("LobbyLog");
            text.AddComponent<TextMeshPro>();
            textComponent = text.GetComponent<TextMeshPro>();

            text.transform.position = new Vector3(-68.7667f, 12.1527f, -83.2612f);
            text.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            text.transform.rotation = Quaternion.Euler(0f, 243.4958f, 0f);

            textComponent.font = GorillaTagger.Instance.offlineVRRig.playerText1.font;

            if (ColorUtility.TryParseHtmlString(colorHexConfig.Value, out Color parsedColor))
                textComponent.color = parsedColor;
            else
            {
                textComponent.color = Color.white;
                Logger.LogWarning($"Not valid hex color in config {colorHexConfig.Value}. Defaulting back to white.");
            }

            textComponent.fontSize = 5f;
            textComponent.text = "Lobby Logger";
        }

        void OnJoinRoom()
        {
            if (isOutdatedVersion)
            {
                textComponent.text = $"Installed version: {PluginInfo.Version}\nPlease update to the newest version on GitHub.\n-ariel and elliot";
                return;
            }
            if (currentLobby != null)
            {
                textComponent.text = $"Lobby Logger\nLast lobby: {currentLobby}";
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
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        IEnumerator runTheTask()
        {
            Task checkTask = CheckVersionAndDecide();
            yield return new WaitUntil(() => checkTask.IsCompleted);
            GorillaTagger.OnPlayerSpawned(Init);
        }
        
        private async Task CheckVersionAndDecide()
        {
            string githubUrl = "https://raw.githubusercontent.com/elliotsilly/LobbyLogVersion/refs/heads/main/version";
            string currentVersion = PluginInfo.Version;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string latestVersion = (await client.GetStringAsync(githubUrl)).Trim();

                    if (latestVersion != currentVersion)
                    {
                        isOutdatedVersion = true;
                        Logger.LogWarning($"LobbyLog is outdated! Current: {currentVersion}, Latest: {latestVersion}");
                    }
                    else
                    {
                        isOutdatedVersion = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to fetch GitHub version: " + ex.Message);
                isOutdatedVersion = false;
            }
        }
    }
}