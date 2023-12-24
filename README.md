# LethalCompany_ModelReplacementSDK
A SDK to simplify creating character model replacements in Lethal Company. [See the API repo](https://github.com/BunyaPineTree/LethalCompany_ModelReplacementAPI) to learn how model replacement works. 
* Note: Not an actual SDK yet, just a tool for creating ModelReplacementAPI compliant assetbundles

How to Install
-
- Have Unity 2022.3.9f1 installed, with a new HDRP project open.
- Open the Package Manager within Unity by hitting Window->Package Manager at the top.
- Hit *"Add package from git url..."*, then punch the following in: `https://github.com/BunyaPineTree/LethalCompany_ModelReplacementSDK.git#upm`
- Make sure to update it regularly by hitting the "Update" button!

Current Functionality
-
A unity editor based workflow that:
- Has a "Setup Model" button that automatically assigns all of the necessary components to a humanoid model
- Automatically calculates your model's position offset, scale, and rotation offsets on each bone to match the scavenger.
- Has interaction handles to position a held item's position offset.
- Has an inspector window that allows you to set assetbundle names, prefab names, and has buttons for reinitializing the character and building assetbundles
- Has native support for a modified version of [UnityJigglePhysics](https://github.com/naelstrof/UnityJigglePhysics) to simplify the creation of models with bone physics. 

Planned Functionality
-
A "Setup Project" button that performs the following
- Requests a directory to place a project folder.
- Searches for the default Lethal Company directory `C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\`, if missing it will prompt the user to enter this directory.
- Contains a basic model replacement mod template with .csproj, Plugin.cs, BasicModelReplacements.cs
  - .csproj: Auto-populates the LC directory, auto-embeds the assetbundle into resources, contains all other necessary items to build the project. This can be done with replaceable strings \\$assetbundlename$ \\$LCdir$, etc... in the template.
  - Plugin.cs: Auto-populates the assetbundle name into the Assets class and mod info in the PluginInfo class, contains basic config.
  - BasicModelReplacements.cs: For each gameObject with OffsetBuilder in the designated build folder, create a ModelReplacement class with name derived from the asset name, and populate the asset name in `LoadAssetsAndReturnModel()`
- Copies all scripts from a designated Scripts folder into a Scripts folder in the VS project folder.

The Build AssetBundles button should automatically overwrite the assetbundle in the project folder 

These steps should be sufficient to create a mod project that is immediately buildable by the user.

Difficulties in Implementing This Solution
-
- Updating modified asset names or asset bundle names is nearly impossible with this solution
- If we allow multiple assetbundles, do we make them separate projects or place them all in a single Assets class with multiple assetbundles?
- Besides replacing assetbundles on build, should any other functionality be implemented using the project folder?

Possible Solutions to Above
-
- A designated settings menu for ModelReplacementAPI that allows you to manage individual projects. A project would contain:
  - One single asset bundle name
  - A project folder location
  - Assembly Name, Mod GUID, name, possibly version
- Under above, each OffsetBuilder component would not allow you to type an asset bundle name, but instead select a project.
- This solution may be overbuilt

