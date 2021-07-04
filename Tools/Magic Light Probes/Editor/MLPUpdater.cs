using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Networking;

namespace MagicLightProbes
{
    [InitializeOnLoad]
    public static class MLPUpdater
    {         
        private static IEnumerator downloadNewVersionRoutine;
        private static IEnumerator checkForUpdatesRoutine;

        public static string installedVersion = "1.94";
        public static bool authorization;
        public static bool updateChecking;
        public static bool updateCheckingLoop;
        public static bool forceUpdateChecking;
        public static bool downloading;
        public static float downloadingProgress;
        public static bool forceCheck;

        [DidReloadScripts]
        public static void StartCheckForUpdatesLoop()
        {
            checkForUpdatesRoutine = null;
            checkForUpdatesRoutine = CheckForUpdatesEnumerator();
            EditorApplication.update -= CheckForUpdatesLoop;
            EditorApplication.update += CheckForUpdatesLoop;
        }

        private static void CheckForUpdatesLoop()
        {
            updateCheckingLoop = true;

            if (checkForUpdatesRoutine != null && checkForUpdatesRoutine.MoveNext())
            {
                return;
            }

            checkForUpdatesRoutine = null;
            checkForUpdatesRoutine = CheckForUpdatesEnumerator();
        }

        private static IEnumerator CheckForUpdatesEnumerator()
        {
            if (forceCheck)
            {
                forceUpdateChecking = true;
            }
            
            if (System.DateTime.Now.Hour != EditorPrefs.GetInt("MLP_lastCheck") || forceCheck)
            {
                forceCheck = false;
                EditorPrefs.SetInt("MLP_lastCheck", System.DateTime.Now.Hour);
            }
            else
            {
                yield break;
            }

            Dictionary<string, string> form = new Dictionary<string, string>();

            form.Add("checkForUpdates", "1");

            using (UnityWebRequest checkUpdate = UnityWebRequest.Post("https://www.motiongamesstudio.com/mlp-update/checkForUpdates.php", form))
            {
                checkUpdate.SendWebRequest();

                while (!checkUpdate.isDone)
                {
                    yield return null;
                }
#if UNITY_2020_1_OR_NEWER
                if (checkUpdate.result == UnityWebRequest.Result.ConnectionError || 
                    checkUpdate.result == UnityWebRequest.Result.DataProcessingError ||
                    checkUpdate.result == UnityWebRequest.Result.ProtocolError)
#else
                if (checkUpdate.isNetworkError || checkUpdate.isHttpError)
#endif
                {
                    if (forceCheck)
                    {
                        Debug.LogFormat("<color=yellow>MLP Check For Updates:</color> Network Error: " + checkUpdate.error);
                    }
                }
                else
                {
                    if (float.Parse(installedVersion, CultureInfo.InvariantCulture) < float.Parse(checkUpdate.GetResponseHeader("Latest-Version"), CultureInfo.InvariantCulture))
                    {
                        EditorPrefs.SetBool("MLP_newVersionAvailable", true);
                        EditorPrefs.SetString("MLP_installedVersion", installedVersion);
                        EditorPrefs.SetString("MLP_latestVersion", checkUpdate.GetResponseHeader("Latest-Version"));
                    } 
                    else
                    {
                        EditorPrefs.SetBool("MLP_newVersionAvailable", false);
                        EditorPrefs.SetString("MLP_installedVersion", installedVersion);
                    }
                }
            }

            forceUpdateChecking = false;
        }

        public static void StartDownload(string uName, string uInvoice)
        {
            downloadNewVersionRoutine = null;
            downloadNewVersionRoutine = DownloadUpdateEnumerator(uName, uInvoice);
            EditorApplication.update -= DownloadUpdate;
            EditorApplication.update += DownloadUpdate;
        }

        private static void DownloadUpdate()
        {
            updateChecking = true;

            if (downloadNewVersionRoutine != null && downloadNewVersionRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= DownloadUpdate;
            updateChecking = false;
        }

        private static IEnumerator DownloadUpdateEnumerator(string uName, string uInvoice)
        {
            Dictionary<string, string> form = new Dictionary<string, string>();

            form.Add("user_name", uName);
            form.Add("invoice_num", uInvoice);
            form.Add("installedVersion", EditorPrefs.GetString("MLP_installedVersion"));

            string savePath = string.Format("{0}/{1}", Application.dataPath, "MagicLightProbesTmp");

            using (UnityWebRequest download = UnityWebRequest.Post("https://www.motiongamesstudio.com/mlp-update/checkForUpdates.php", form))
            {
                download.SendWebRequest();
                
                downloadingProgress = 0;

                while (!download.isDone)
                {
                    if (download.downloadProgress > 0)
                    {
                        downloading = true;
                        downloadingProgress = download.downloadProgress;
                    }

                    yield return null;
                }

                downloading = false;

#if UNITY_2020_1_OR_NEWER
                if (download.result == UnityWebRequest.Result.ConnectionError ||
                    download.result == UnityWebRequest.Result.DataProcessingError ||
                    download.result == UnityWebRequest.Result.ProtocolError)
#else
                if (download.isNetworkError || download.isHttpError)
#endif
                {
                    Debug.LogFormat("<color=yellow>MLP:</color> Network Error: " + download.error);
                }
                else
                {
                    if (download.GetResponseHeader("Content-Description") == "File Transfer")
                    {
                        authorization = false;

                        EditorPrefs.SetString("MLP_uName", uName);
                        EditorPrefs.SetString("MLP_uInvoice", uInvoice);
                        EditorPrefs.SetBool("MLP_Authorized", true);

                        System.IO.File.WriteAllBytes(savePath, download.downloadHandler.data);

                        AssetDatabase.ImportPackage(savePath, true);
                        AssetDatabase.importPackageCompleted += ImportPackageCompleted;
                    }
                    else
                    {
                        Debug.LogFormat("<color=yellow>MLP:</color> Authorization Error: " + download.downloadHandler.text);
                    }
                }
            }
        }

        private static void ImportPackageCompleted(string packageName)
        {
            EditorPrefs.SetBool("MLP_newVersionAvailable", false);
            EditorPrefs.SetString("MLP_installedVersion", installedVersion);
        }
    }
}
