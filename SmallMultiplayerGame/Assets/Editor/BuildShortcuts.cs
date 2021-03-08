using UnityEditor;
using UnityEngine;

public class BuildShortcuts : MonoBehaviour
{
	[MenuItem("BuildShortcuts/BuildClient")]
	public static void BuildClient()
	{
		Build("Client", false);
	}

	[MenuItem("BuildShortcuts/BuildAndRunClient")]
	public static void BuildAndRunClient()
	{
		Build("Client", true);
	}

	[MenuItem("BuildShortcuts/BuildServer")]
	public static void BuildServer()
	{
		Build("Server", false);
	}

	[MenuItem("BuildShortcuts/BuildAndRunServer")]
	public static void BuildAndRunServer()
	{
		Build("Server", true);
	}

	private static void Build(string str, bool autoRun)
	{
		BuildPlayerOptions bpo = new BuildPlayerOptions();

		if (str == "Client")
		{
			PlayerSettings.productName = "Client";
			bpo.scenes = new[] { "Assets/Scenes/Main.unity" };
			bpo.locationPathName = "C:/Users/beatb/Downloads/ClientBuild/Client.exe";
			bpo.target = BuildTarget.StandaloneWindows;
			bpo.options = autoRun ? BuildOptions.AutoRunPlayer : BuildOptions.None;
		}
		else
		{
			PlayerSettings.productName = "Server";
			bpo.scenes = new[] { "Assets/Scenes/Server.unity" };
			bpo.locationPathName = "C:/Users/beatb/Downloads/ServerBuild/Server.exe";
			bpo.target = BuildTarget.StandaloneWindows;
			bpo.options = autoRun ? BuildOptions.EnableHeadlessMode | BuildOptions.AutoRunPlayer : BuildOptions.EnableHeadlessMode;
		}

		BuildPipeline.BuildPlayer(bpo);
	}

}

