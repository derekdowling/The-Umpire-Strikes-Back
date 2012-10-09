using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;


namespace NLAS_game
{
    class SoundManager
    {
        
        private ContentManager contentManager;

        //keep track of last game state to update music
        private GameState lastGameState;
        
        //sound effect variables
        public SoundEffect throwNoise;
        public SoundEffect umpireHit;
        public SoundEffect objectHit;
        public SoundEffect landInTrash;
        public SoundEffect crowdCheer;
        public SoundEffect upgradePurchased;
        public SoundEffect playBall;
        public SoundEffect youreOut;
        public SoundEffect crowdBoo;
       
        //songs sound variables here
        public Song menuSong;
        public Song gameSong;
        public Song bonusSong;
        public Song gameOverSong;

        public SoundManager(ContentManager conMan)
        {

            contentManager = conMan;
            lastGameState = GameState.MainMenu;

            //set MediaPlayer to loop song
            bool gameHasControl = false;

            try
            {
                gameHasControl = MediaPlayer.GameHasControl;
            }
            catch
            {

            }

            if (gameHasControl)
            {
                try
                {
                    MediaPlayer.IsRepeating = true;
                }
                catch
                {

                }
                
            }

        }

        public void initializeSound()
        {

            //load sound effects, MAKE SURE ALL SOUND EFFECTS ARE SET TO SOUND EFFECT IN PROPERTIES
            throwNoise = contentManager.Load<SoundEffect>("Audio/Effects/whoosh");
            crowdCheer = contentManager.Load<SoundEffect>("Audio/Effects/crowdCheer");
            landInTrash = contentManager.Load<SoundEffect>("Audio/Effects/trashHit");
            objectHit = contentManager.Load<SoundEffect>("Audio/Effects/bonk");
            umpireHit = contentManager.Load<SoundEffect>("Audio/Effects/umpireHit");
            upgradePurchased = contentManager.Load<SoundEffect>("Audio/Effects/upgradeBought");
            playBall = contentManager.Load<SoundEffect>("Audio/Effects/playBall");
            youreOut = contentManager.Load<SoundEffect>("Audio/Effects/youreOut");
            crowdBoo = contentManager.Load<SoundEffect>("Audio/Effects/crowdBoo");

            //load song content, MAKE SURE ALL SONGS ARE SET TO SONG IN PROPERTIES
            menuSong = contentManager.Load<Song>("Audio/Songs/menuSong");
            gameSong = contentManager.Load<Song>("Audio/Songs/gameSong");
            bonusSong = contentManager.Load<Song>("Audio/Songs/bonusSong");
            gameOverSong = contentManager.Load<Song>("Audio/Songs/menuSong");

            try
            {
                MediaPlayer.Play(menuSong);
            }
            catch { }
            

        }
    
        public void unloadSoundContent()
        {
            //contentManager.Unload(throwNoise);

        }

        public void update(GameState currentGameState)
        {

            if (lastGameState != currentGameState)
            {
                //resume sound if coming from pausemenu
                /*
                if (lastGameState == GameState.Paused && currentGameState == GameState.InGame)
                {
                    try
                    {
                        MediaPlayer.Resume();
                    }
                    catch(Exception) { }
                    Debug.WriteLine("Song Resumed.");
                }
                 */
                //otherwise handle song change
                //else
                {
                    lastGameState = currentGameState;
                    updateMusic();
                }

            }

        }

        public void playSound(SoundEffect soundEffect)
        {

            soundEffect.Play();

            Debug.WriteLine("Sound effect played: " + soundEffect.Name);

        }
        
        public void updateMusic()
        {

            bool gameHasControl = false;

            try
            {
                gameHasControl = MediaPlayer.GameHasControl;
            }
            catch
            {

            }

            if (gameHasControl)
            {

                // Which game state are we updating?
                switch (lastGameState)
                {

                    // Update Logic for the Main Menu
                    case GameState.MainMenu:
                        if(lastGameState != GameState.HowToPlay)
                            try { MediaPlayer.Play(menuSong); }
                            catch (Exception) { }
                        break;
                    // Update Logic for the How To Play Screen
                    case GameState.HowToPlay:
                        break;
                    // Update Logic for the Debriefing Screen
                    case GameState.Debriefing:
                        playSound(crowdCheer);
                        break;
                    // Update Logic for in Game
                    case GameState.InGame:
                        playSound(playBall);
                        try { MediaPlayer.Play(gameSong); }
                        catch (Exception) { }
                        break;
                    // Update Logic for the Shop Screen
                    case GameState.Shop:
                        break;
                        /*
                    case GameState.Paused:
                        try { MediaPlayer.Pause(); }
                        catch (Exception e) { }
                        Debug.WriteLine("Song Paused.");
                        break;
                         */
                    case GameState.Failed:
                        playSound(youreOut);
                        try { MediaPlayer.Pause(); }
                        catch(Exception) {}
                        playSound(crowdCheer);
                        break;
                    //case GameState.Bonus:
                      //  MediaPlayer.Play(bonusSong);
                        //break;
                }

            } 
 
        }

        /// <summary>
        /// Value between 0.0f and 1.0f. This number is equivalent to User Volume level - (96db * float value). 
        /// </summary>
        /// <param name="vol"></param>
        public void setVolume(float vol)
        {

            MediaPlayer.Volume = vol;

        }

        /// <summary>
        /// Volume adjustment is based on a decibel, not multiplicative, scale. For example, when the device volume is 
        /// half of maximum (about 7 in the Zune user interface), setting Volume to 0.6f or less is silent or nearly so,
        /// not volume 4 as you would expect from a multiplicative adjustment. Setting Volume to 0.0 subtracts 96 dB from 
        /// the volume. Setting Volume to 1.0 subtracts 0 dB from the volume. Values in between 0.0f and 1.0f subtract dB 
        /// from the volume proportionally.
        /// </summary>
        /// <returns></returns>
        public float getVolume()
        {

            return MediaPlayer.Volume;

        }
    }
}

    

