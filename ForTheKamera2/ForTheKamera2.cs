using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System.IO;
using System.Globalization;
using static PlayfabHelper;

namespace ForTheKamera2
{
    [BepInPlugin("com.plurchi.ftk2.cameramod", "ForTheKamera2 Mod", "1.0.0")]
    public class ForTheKamera2 : BaseUnityPlugin
    {
        private static ManualLogSource log;
        private static Harmony harmony;

        internal static string pluginFolder;
        internal static string configPath;
        internal static Vector3 originPos = Vector3.zero;
        internal static Vector3[] positions = new Vector3[3];
        internal static int toggleIndex = -1;
        internal static bool originSaved = false;

        public static void Log(string msg) => log?.LogInfo(msg);
        public static void LogError(string msg) => log?.LogError(msg);


        private void Awake()
        {
            log = Logger;
            log.LogInfo("ForTheKamera2 Mod gestartet - Version 0.7.0...");


            pluginFolder = Path.Combine(Paths.PluginPath, "ForTheKamera2");
            Directory.CreateDirectory(pluginFolder);
            configPath = Path.Combine(pluginFolder, "campos.cfg");
            EnsureConfigExists();




            try
            {
                harmony = new Harmony("com.plurchi.ftk2.cameramod");
                harmony.PatchAll();
                log.LogInfo("Harmony-Patches erfolgreich angewendet.");
            }
            catch (Exception ex)
            {
                log.LogError($"Fehler beim Patchen: {ex.Message}");
            }
        }

        #region config file area methods

        internal static void EnsureConfigExists()
        {
            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath,
                    "# Kamera-Positions-Config\n" +
                    "Pos1=1,8,12\n" +
                    "Pos2=0,8,12\n" +
                    "Pos3=0,11,11\n"
                );
                log.LogInfo($"Config written at: {configPath}");
            }
            else
            {
                log.LogInfo($"Config found at: {configPath}");
            }
        }

        internal static void LoadPositionsFromFile()
        {
            try
            {
                string[] lines = File.ReadAllLines(configPath);
                for (int i = 0; i < 3; i++)
                    positions[i] = Vector3.zero;

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    string[] parts = line.Split('=');
                    if (parts.Length != 2) continue;

                    string key = parts[0].Trim();
                    string[] coords = parts[1].Split(',');
                    if (coords.Length != 3) continue;

                    if (float.TryParse(coords[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                        float.TryParse(coords[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                        float.TryParse(coords[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                    {
                        if (key.Equals("Pos1", StringComparison.OrdinalIgnoreCase)) positions[0] = new Vector3(x, y, z);
                        if (key.Equals("Pos2", StringComparison.OrdinalIgnoreCase)) positions[1] = new Vector3(x, y, z);
                        if (key.Equals("Pos3", StringComparison.OrdinalIgnoreCase)) positions[2] = new Vector3(x, y, z);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Error while loading from campos.cfg: " + ex.Message);
            }
        }

        /// <summary>
        /// Save original camera position to campos.cfg. That it can be called back when requested by user.
        /// </summary>
        /// <param name="origin">Original camera position</param>
        internal static void SaveOriginToFile(Vector3 origin)
        {
            try
            {
                string[] lines = File.ReadAllLines(configPath);
                using (StreamWriter sw = new StreamWriter(configPath, false))
                {
                    sw.WriteLine($"# Original camera position.");
                    sw.WriteLine($"Origin={origin.x.ToString(CultureInfo.InvariantCulture)},{origin.y.ToString(CultureInfo.InvariantCulture)},{origin.z.ToString(CultureInfo.InvariantCulture)}");
                    foreach (string line in lines)
                    {
                        if (!line.StartsWith("Origin="))
                            sw.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Error while saving original camera position in campos.cfg: " + ex.Message);
            }
        }

        #endregion config file area methods


    }




    [HarmonyPatch(typeof(InputController), "_anyButtonPerformed")]
    public static class InputPatch
    {
        internal static string lastTurnCamName = null;
        internal static int lastTurnCamInstanceID = -1;
        internal static Vector3? lastTurnCamPosition = null;

        [HarmonyPostfix]
        public static void AnyButtonPerformedPostfix(InputAction.CallbackContext pCallback)
        {
            try
            {
                var inputName = pCallback.control?.displayName ?? "Unknown key";

                // React whan middle mouse button is pressed and active main camera is named "Default_Camera"
                if (inputName == "Middle Button" && Camera.main != null && Camera.main.name == "Default_Camera")
                {
                    var brain = Camera.main.GetComponent<CinemachineBrain>();
                    if (brain != null && brain.ActiveVirtualCamera != null)
                    {
                        var vcam = brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
                        if (vcam != null)
                        {
                            // read in config file campos.cfg
                            ForTheKamera2.LoadPositionsFromFile();

                            // if there is no origin position, save it and active new position 0
                            if (!ForTheKamera2.originSaved)
                            {
                                ForTheKamera2.originPos = vcam.transform.position;
                                ForTheKamera2.SaveOriginToFile(ForTheKamera2.originPos);
                                ForTheKamera2.originSaved = true;
                                ForTheKamera2.Log($"Original position saved. {vcam.transform.position}\"");
                            }

                            ForTheKamera2.toggleIndex++;

                            if (ForTheKamera2.toggleIndex >= 0 && ForTheKamera2.toggleIndex <= 2)
                            {
                                MoveCamera(vcam, ForTheKamera2.positions[ForTheKamera2.toggleIndex]);
                            }
                            else
                            {
                                MoveCamera(vcam, ForTheKamera2.originPos);
                                ForTheKamera2.toggleIndex = -1;
                            }



                        }
                        else
                        {
                            ForTheKamera2.Log("No CinemachineVirtualCamera found.");
                        }
                    }
                    else
                    {
                        ForTheKamera2.Log("No active VirtualCamera exists.");
                    }
                }
            }
            catch (Exception ex)
            {
                ForTheKamera2.LogError("Error in ftk2 AnyButtonPerformedPostfix: " + ex.Message);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CinemachineBrain), "LateUpdate")]
        public static void SyncOtherTurnCamIfNeeded(CinemachineBrain __instance)
        {
            if (lastTurnCamPosition == null)
                return;

            var vcam = __instance?.ActiveVirtualCamera?.VirtualCameraGameObject?.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) return;

            if (vcam.name == "Group1Turn_VCam" || vcam.name == "Group2Turn_VCam")
            {
                int currentID = vcam.gameObject.GetInstanceID();

                if (currentID != lastTurnCamInstanceID)
                {
                    // Sync position from last known camera
                    MoveCamera(vcam, lastTurnCamPosition.Value);

                    // Update tracking
                    lastTurnCamName = vcam.name;
                }
            }
        }


        private static void MoveCamera(CinemachineVirtualCamera vcam, Vector3 targetPos)
        {
            Vector3 oldPos = vcam.transform.position;
            vcam.transform.position = targetPos;

            // adjust x-achses for enemy view
            if (vcam.name == "Group2Turn_VCam")
            {
                Vector3 enemyTargetPos = targetPos;
                enemyTargetPos.x = 0 - targetPos.x;

                vcam.transform.position = enemyTargetPos;
            }
            else // no adjustment for player view
            {
                vcam.transform.position = targetPos;
            }
            
            vcam.transform.LookAt(Vector3.zero);

            // save current settings for turn camera
            lastTurnCamName = vcam.name;
            lastTurnCamPosition = targetPos;


            ForTheKamera2.Log($"Active VirtualCamera '{vcam.Name}' moved: Old={oldPos} -> New {ForTheKamera2.toggleIndex}={vcam.transform.position}");
        }

        private static void MoveBothVCams(Vector3 targetPos)
        {
            string[] vcamNames = { "Group1Turn_VCam", "Group2Turn_VCam" };

            foreach (string name in vcamNames)
            {
                var go = GameObject.Find(name);
                if (go == null)
                {
                    ForTheKamera2.Log($"VirtualCamera '{name}' not found.");
                    continue;
                }

                var vcam = go.GetComponent<CinemachineVirtualCamera>();
                if (vcam == null)
                {
                    ForTheKamera2.Log($"'{name}' does not have a CinemachineVirtualCamera component.");
                    continue;
                }

                Vector3 oldPos = vcam.transform.position;
                vcam.transform.position = targetPos;
                vcam.transform.LookAt(Vector3.zero);
                ForTheKamera2.Log($"Moved VirtualCamera '{name}': Old={oldPos} -> New={targetPos}");
            }
        }


    }
}
