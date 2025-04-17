using GTA;
using GTA.Native;
using System;
using System.IO;
using System.Windows.Forms;
using System.Media;

public class AudioScript : Script
{
    private static readonly string[] AudioFiles = { "1.wav", "2.wav", "3.wav", "4.wav", "5.wav", "6.wav" };
    private static readonly float[] AudioDurations = { 1.8f, 1.2f, 1.1f, 1.7f, 1.4f, 2.0f };
    private SoundPlayer[] soundPlayers;
    private DateTime lastKeyPress = DateTime.MinValue;
    private bool isWaitingForDoubleClick = false;
    private bool wasKeyPressedLastFrame = false;
    private const double DoubleClickTime = 300;
    private int currentAudioIndex = 0;
    private bool isAudioPlaying = false;
    private float audioEndTime = 0f;

    public AudioScript()
    {
        string audioDirectory = Path.Combine("scripts", "RonShoutAudio");
        soundPlayers = new SoundPlayer[AudioFiles.Length];

        for (int i = 0; i < AudioFiles.Length; i++)
        {
            string audioPath = Path.Combine(audioDirectory, AudioFiles[i]);
            if (File.Exists(audioPath))
            {
                soundPlayers[i] = new SoundPlayer(audioPath);
            }
            else
            {
                GTA.UI.Notify("~r~Audio file not found: " + AudioFiles[i]);
            }
        }

        Tick += OnTick;
    }

    private void OnTick(object sender, EventArgs e)
    {
        DateTime now = DateTime.Now;
        bool isFKeyPressed = Game.IsKeyPressed(Keys.F);
        bool isRightMousePressed = Game.IsControlPressed(0, GTA.Control.Aim);
        bool isAiming = isRightMousePressed; 

        if (!isRightMousePressed)
        {
            wasKeyPressedLastFrame = isFKeyPressed;
            return;
        }

        if (isAudioPlaying && Game.GameTime >= audioEndTime)
        {
            isAudioPlaying = false;
        }

        Game.DisableControlThisFrame(0, GTA.Control.Enter);
        Game.DisableControlThisFrame(0, (GTA.Control)Function.Call<int>(Hash.GET_HASH_KEY, "VEHICLE_ENTER"));

        if (isFKeyPressed && !wasKeyPressedLastFrame)
        {
            double timeSinceLastPress = (now - lastKeyPress).TotalMilliseconds;

            if (!isWaitingForDoubleClick)
            {
                isWaitingForDoubleClick = true;
                lastKeyPress = now;
            }
            else if (timeSinceLastPress <= DoubleClickTime && !isAudioPlaying)
            {
                PlayCurrentAudio();
                isWaitingForDoubleClick = false;
            }
        }
        else if (!isFKeyPressed && isWaitingForDoubleClick)
        {
            double timeSinceLastPress = (now - lastKeyPress).TotalMilliseconds;
            if (timeSinceLastPress > DoubleClickTime)
            {
                isWaitingForDoubleClick = false;
            }
        }

        wasKeyPressedLastFrame = isFKeyPressed;
    }

    private void PlayCurrentAudio()
    {
        if (currentAudioIndex < soundPlayers.Length && soundPlayers[currentAudioIndex] != null)
        {
            try
            {
                isAudioPlaying = true;
                soundPlayers[currentAudioIndex].Play();
                audioEndTime = Game.GameTime + (AudioDurations[currentAudioIndex] * 1000f);
                currentAudioIndex = (currentAudioIndex + 1) % soundPlayers.Length;
            }
            catch (Exception ex)
            {
                GTA.UI.Notify("~r~播放失败: " + ex.Message);
                isAudioPlaying = false;
            }
        }
    }
}
