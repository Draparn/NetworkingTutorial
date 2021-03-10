using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class BuildShortcuts : MonoBehaviour
{
	[MenuItem("BuildShortcuts/BuildAndRunClient")]
	public static void BuildAndRunClient()
	{
		Build("Client", true);
	}

	[MenuItem("BuildShortcuts/BuildAndRunServer")]
	public static void BuildAndRunServer()
	{
		Build("Server", true);
	}

	[MenuItem("BuildShortcuts/BuildClient")]
	public static void BuildClient()
	{
		Build("Client", false);
	}

	[MenuItem("BuildShortcuts/BuildServer")]
	public static void BuildServer()
	{
		Build("Server", false);
	}

	[MenuItem("BuildShortcuts/RunClient")]
	public static void RunClient()
	{
		Process.Start("C:/Users/beatb/Downloads/ClientBuild/Client.exe");
	}

	[MenuItem("BuildShortcuts/RunServer")]
	public static void RunServer()
	{
		Process.Start("C:/Users/beatb/Downloads/ServerBuild/Server.exe");
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

