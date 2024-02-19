using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace GlitchSDK.IntegrationManager.Editor
{
    public class IntegrationManager : EditorWindow
    {
        public const string Stamp = "IntegrationManager";

        public Dictionary<string, List<ReleaseDTO>> LoadedReleases { get; private set; }
        public Dictionary<string, PackageDetails> LoadedInstalls { get; private set; }

        static EditorWindow window;

        IntegrationData data;
        VisualElement root;
        AuthenticationData authData;
        UIElementsController elementsController;
        RequestController requestController;

        bool getNextPackage;
       
        [MenuItem("Tools/GlitchSDK/IntegrationManager")]
        public static void ShowWindow()
        {
            window = GetWindow(typeof(IntegrationManager), true, "GlitchSDK - Integration Manager");
            Vector2 windowSize = new(700, 700);
            window.minSize = windowSize;
            window.maxSize = windowSize;
            window.position = new Rect(new(100, 100), windowSize);
            window.Focus();
        }

        void OnEnable()
        {
            root = rootVisualElement;

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/GlitchSDK/IntegrationManager/Editor/IntegrationWindow.uxml");
            VisualElement labelFromUXML = visualTree.CloneTree();
            root.Add(labelFromUXML);

            elementsController = new(this, root);
            requestController = new(this);

            data = IntegrationUtilities.GetIntegrationData();
            if (data == null || data.packages.Count == 0)
            {
                string message = "Please create the Integration Data first!";
                elementsController.OpenErrorContainer(message);
            }
            else
            {
                authData = IntegrationUtilities.GetAuthData();
                elementsController.OpenLoginContainer(authData.token);
            }
        }

        void Update()
        {
            if (getNextPackage)
            {
                getNextPackage = false;
                string packageName = data.packages[LoadedReleases.Count].repoName;
                requestController.Get($"https://api.github.com/repos/Octakull/{packageName}/releases");
            }
        }

        public void LoginPlayer(string token)
        {
            elementsController.OpenLoadingContainer();

            authData.token = token;
            EditorUtility.SetDirty(authData);
            AssetDatabase.SaveAssetIfDirty(authData);

            requestController.SetAuthToken(token);
            LoadedReleases = new();
            getNextPackage = true;
        }

        public void OnGetRequestCompleted(UnityWebRequest result)
        {
            if (result.result != UnityWebRequest.Result.Success || result.error != null)
            {
                Debug.Log($"[{Stamp}] Error: {result.error}");
                elementsController.OpenLoginContainer(authData.token);
            }
            else
            {
                string packageName = data.packages[LoadedReleases.Count].repoName;
                Debug.Log($"[{Stamp}] Successfully loaded releases for '{packageName}'.");
                
                try
                {
                    List<ReleaseDTO>  releases = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ReleaseDTO>>(result.downloadHandler.text);
                    if (releases != null) LoadedReleases.Add(packageName, releases);
                }
                catch (Exception ex)
                {
                    elementsController.OpenErrorContainer($"Failed to download {packageName} data. Exception: {ex.Message}");
                }

                if (LoadedReleases.Count == data.packages.Count)
                {
                    LoadInstalledPackagesDetails();
                    elementsController.OpenMainContainer();
                    Debug.Log($"[{Stamp}] Finished packages' loading process.");
                }
                else getNextPackage = true;
            }
        }

        void LoadInstalledPackagesDetails()
        {
            LoadedInstalls ??= new();
            foreach (PackageData package in data.packages)
            {
                PackageDetails packageDetails = IntegrationUtilities.GetInstalledPackageDetails(package.repoName);
                if (!LoadedInstalls.ContainsKey(package.repoName)) LoadedInstalls.Add(package.repoName, packageDetails);
                else LoadedInstalls[package.repoName] = packageDetails;
            }
        }
    }
}
