using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GlitchSDK.IntegrationManager.Editor
{
    public class UIElementsController
    {
        [Header("Error")]
        readonly VisualElement errorContainer;
        readonly Label errorMessage;

        [Header("Login")]
        readonly VisualElement loginContainer;
        readonly TextField tokenField;
        readonly Button loginButton;

        [Header("Loading")]
        readonly VisualElement loadingContainer;

        [Header("Main")]
        readonly VisualElement mainContainer;
        readonly Label integrationCurrentVersion;
        readonly ScrollView packagesScrollView;
        readonly VisualElement packagesGroupContainer;
        readonly Label detailsPackageName;
        readonly Label detailsPackageVersion;

        readonly Texture2D installedMarkerTexture;

        readonly IntegrationManager manager;
        readonly Dictionary<string, PackageElement> packagesButton;

        Color buttonColor;
        Color selectedColor;

        string activePackageId;

        public UIElementsController(IntegrationManager manager, VisualElement root)
        {
            this.manager = manager;

            errorContainer = root.Query<VisualElement>("error_container");
            errorMessage = root.Query<Label>("error_message");

            loginContainer = root.Query<VisualElement>("login_container");
            tokenField = root.Query<TextField>("token_field");
            loginButton = root.Query<Button>("login_submit_button");
            loginButton.clicked += OnLogin;

            loadingContainer = root.Query<VisualElement>("loading_container");

            mainContainer = root.Query<VisualElement>("main_container");
            integrationCurrentVersion = root.Query<Label>("integration_current_version");
            packagesScrollView = root.Query<ScrollView>("packages_list_container");
            packagesGroupContainer = packagesScrollView.Query<VisualElement>("unity-content-container");
            detailsPackageName = root.Query<Label>("details_package_name");
            detailsPackageVersion = root.Query<Label>("details_package_version");

            mainContainer.parent.style.flexGrow = 1;

            installedMarkerTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Joyglitch/IntegrationManager/Editor/Sprites/Star");

            packagesButton = new();

            buttonColor = new Color32(87, 87, 87, 255);
            selectedColor = new Color32(62, 180, 137, 255);
        }

        public void OpenErrorContainer(string error)
        {
            errorMessage.text = error;

            errorContainer.visible = true;
            loginContainer.visible = false;
            loadingContainer.visible = false;
            mainContainer.visible = false;
        }

        public void OpenLoginContainer(string currentToken)
        {
            tokenField.value = currentToken;

            errorContainer.visible = false;
            loginContainer.visible = true;
            loadingContainer.visible = false;
            mainContainer.visible = false;
        }

        public void OpenLoadingContainer()
        {
            errorContainer.visible = false;
            loginContainer.visible = false;
            loadingContainer.visible = true;
            mainContainer.visible = false;
        }

        public void OpenMainContainer()
        {
            errorContainer.visible = false;
            loginContainer.visible = false;
            loadingContainer.visible = false;
            mainContainer.visible = true;

            UpdateMainContainer();
        }

        public void UpdateMainContainer()
        {
            string firstPackageName = null;
            foreach (string packageName in manager.LoadedReleases.Keys)
            {
                if (packageName == IntegrationManager.Stamp)
                {
                    integrationCurrentVersion.text = manager.LoadedInstalls[packageName].version;
                }
                else
                {
                    firstPackageName = packageName;
                    string installedVersion = string.Empty;
                    if (manager.LoadedInstalls[packageName] != null) installedVersion = manager.LoadedInstalls[packageName].version;
                    string latestReleaseVersion = manager.LoadedReleases[packageName][^1].tag_name;
                    UpdatePackageButton(packageName, installedVersion, latestReleaseVersion);
                }
            }
            if (!string.IsNullOrEmpty(firstPackageName)) OnPackageSelected(firstPackageName);
        }

        void OnLogin()
        {
            manager.LoginPlayer(tokenField.value);
        }

        void UpdatePackageButton(string packageName, string installedVersion, string latestReleaseVersion)
        {
            PackageElement packageElement;
            if (!packagesButton.ContainsKey(packageName))
            {
                Button button = new();

                button.style.height = 25;
                button.style.flexDirection = FlexDirection.Row;
                button.text = string.Empty;

                button.style.marginTop = 0;
                button.style.marginRight = 0;
                button.style.marginBottom = 0;
                button.style.marginLeft = 0;

                button.style.borderTopLeftRadius = 2;
                button.style.borderTopRightRadius = 2;
                button.style.borderBottomRightRadius = 2;
                button.style.borderBottomLeftRadius = 2;

                button.clicked += () => OnPackageSelected(packageName);

                Label label = new(packageName);
                label.style.width = new(new Length(50, LengthUnit.Percent));
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                button.Add(label);

                Label version = new();
                version.style.unityTextAlign = TextAnchor.MiddleRight;
                button.Add(version);

                VisualElement installedMarker = new();
                installedMarker.style.backgroundImage = new StyleBackground(installedMarkerTexture);
                button.Add(installedMarker);

                packagesGroupContainer.Add(button);

                packageElement = new(button, version, installedMarker);
                packagesButton.Add(packageName, packageElement);
            }
            else packageElement = packagesButton[packageName];

            if (!string.IsNullOrEmpty(installedVersion))
            {
                packageElement.version.text = installedVersion;
                packageElement.version.style.width = new(new Length(45, LengthUnit.Percent));
                packageElement.installedMarker.style.width = new(new Length(5, LengthUnit.Percent));
                packageElement.installedMarker.visible = true;
            }
            else
            {
                packageElement.version.text = latestReleaseVersion;
                packageElement.version.style.width = new(new Length(50, LengthUnit.Percent));
                packageElement.installedMarker.style.width = new(new Length(0, LengthUnit.Percent));
                packageElement.installedMarker.visible = false;
            }
        }

        void OnPackageSelected(string packageName)
        {
            if (activePackageId != packageName)
            {
                UpdatePackageDetails(packageName);
                if (!string.IsNullOrEmpty(activePackageId) && packagesButton.ContainsKey(activePackageId))
                {
                    packagesButton[activePackageId].button.style.unityBackgroundImageTintColor = Color.white;
                }
                packagesButton[packageName].button.style.backgroundColor = selectedColor;
                activePackageId = packageName;
            }
        }

        void UpdatePackageDetails(string packageName)
        {
            detailsPackageName.text = packageName;
            
        }
        
        class PackageElement
        {
            public Button button;
            public Label version;
            public VisualElement installedMarker;

            public PackageElement(Button button, Label version, VisualElement installedMarker)
            {
                this.button = button;
                this.version = version;
                this.installedMarker = installedMarker;
            }
        }
    }
}
