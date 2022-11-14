using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Discord;
using System;

/* I made this script using MarshMello0's code : https://github.com/MarshMello0/Editor-Rich-Presence 
 * It's basically a light version of their code, and without the stacking RichPresence problem
 */

/*
 * I didn't find a way to save the Discord instance to dynamically update the Activty.
 * Since entering play mode recompile scripts the Discord instance is lost.
 * Same problem occurs when scripts are being recompiled by something else.
 * Without the check to run the script only once it would create a new RichPresence Activity every recompile,
 * without disposing of the old instance, leading to RichPresence stacking up on your discord profile.
 * So this will just run once to start the RichPresence and not update the Activity.
 */

[InitializeOnLoad]
public static class SimpleUERP
{
#if UNITY_EDITOR
    /* Edit this to change the first line under the application name 
     * By default this shows the name of the project */
    private static string details = Application.productName;

    /* Edit this to change the line underneath the details 
     * Blank by default, you can input whatever you want */
    private static string state = "";

    /* Edit this to change the start time (in Unix time) 
     * By default it grabs the current time, so timer will start at 00:00 */
    private static long timeStart = DateTimeOffset.Now.ToUnixTimeSeconds();

    /* This is the main image people will see in the Activity
     * Change this to "unitylogo_dark" to use a dark unity logo on white background
     * By default this uses the white unity logo on a dark background */
    private static string largeImage = "unitylogo_white";

    /* Edit this to change the text that appears when the cursor is over the main image
     * By default it shows the Unity version the project uses */
    private static string largeText = "Unity " + Application.unityVersion;


    private const string applicationID = "1041250607073341470";
    private const string k_ProjectOpened = "ProjectOpened";

    private static Discord.Discord discord;

    static SimpleUERP()
    {
        DelayStart();
    }

    public static async void DelayStart(int delay = 1000)
    {
        await Task.Delay(delay);

        Init();
    }

    private static void Init()
    {
        if (!DiscordRunning())
            return;

        if (!SessionState.GetBool(k_ProjectOpened, false))
        {
            SessionState.SetBool(k_ProjectOpened, true);
            try
            {
                discord = new Discord.Discord(long.Parse(applicationID), (long)CreateFlags.Default);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex.ToString());
                return;
            }
        }

        EditorApplication.update += Update;

        UpdateActivity();
    }

    private static void Update()
    {
        if (discord == null)
            return;

        discord.RunCallbacks();
    }

    private static void UpdateActivity()
    {
        if (discord == null)
            return;

        Activity activity = new Activity
        {
            Details = details,
            State = state,
            Timestamps = { Start = timeStart },
            Assets =
            {
                LargeImage = largeImage,
                LargeText = largeText
            }
        };

        discord.GetActivityManager().UpdateActivity(activity, result =>
        {
            if (result != Result.Ok) UnityEngine.Debug.LogError(result.ToString());
        });
    }

    private static bool DiscordRunning()
    {
        Process[] processes = Process.GetProcessesByName("Discord");

        if (processes.Length == 0)
        {
            processes = Process.GetProcessesByName("DiscordPTB");

            if (processes.Length == 0)
            {
                processes = Process.GetProcessesByName("DiscordCanary");
            }
        }
        return processes.Length != 0;
    }
#endif
}