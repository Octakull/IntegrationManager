using System.IO;
using UnityEditor;
using UnityEngine;

namespace GlitchSDK.IntegrationManager.Editor
{
    public class IntegrationUtilities
    {
        const string DataPath = "Assets/GlitchSDK/IntegrationManager/Editor/IntegrationData.asset";

        const string AuthDataName = "JoyIntegrationAuthData";
        const string AuthDataDirectoryPath = "Assets/Resources/IntegrationManager";

        public static IntegrationData GetIntegrationData()
        {
            return AssetDatabase.LoadAssetAtPath<IntegrationData>(DataPath);
        }

        public static AuthenticationData GetAuthData()
        {
            string path = $"{AuthDataDirectoryPath}/{AuthDataName}.asset";
            AuthenticationData authData = AssetDatabase.LoadAssetAtPath<AuthenticationData>(path);
            if (authData == null)
            {
                if (!Directory.Exists(AuthDataDirectoryPath)) Directory.CreateDirectory(AuthDataDirectoryPath);

                authData = ScriptableObject.CreateInstance<AuthenticationData>();
                AssetDatabase.CreateAsset(authData, path);
                AssetDatabase.Refresh();
            }

            return authData;
        }

        public static PackageDetails GetInstalledPackageDetails(string packageName)
        {
            string directory = $"Assets/GlitchSDK/{packageName}";
            if (Directory.Exists(directory))
            {
                string filePath = $"{directory}/Details.json";
                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);
                    try
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<PackageDetails>(content);
                    }
                    catch
                    {
                        return null;
                    }
                }
                else return null;
            }
            else return null;
        }
    }
}
