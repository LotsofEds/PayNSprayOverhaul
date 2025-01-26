using IVSDKDotNet;
using System.Collections.Generic;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;
using IVSDKDotNet.Enums;
using System.IO;
using System.Diagnostics;
using System;
using System.Numerics;
using System.Runtime;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace ImprovedPayNSpray.ivsdk
{
    public class Main : Script
    {
        public static IVPed PlayerPed { get; set; }
        public static int PlayerIndex { get; set; }
        public static int PlayerHandle { get; set; }

        public static bool enable;
        public static int CarInitCost;
        public static int EngInitCost;
        public static float CarCostMult;
        public static float EngCostMult;
        public static int ColorCost;

        private static bool dontCrash;
        private static bool gotColor;
        private static int PrimColor1;
        private static int PrimColor2;
        private static int SecColor1;
        private static int SecColor2;
        private static int NumOfResprays;
        private static bool CheckDateTime;
        private static DateTime currentDateTime;
        public static IVVehicle playerVehicle;
        public static IVVehicle pVehicle;
        private static int vehHandle;
        private static int pVeh;
        private static int cams;
        private static bool inMenu;
        private static bool sprayCar;
        private static bool FastScroll;
        private static bool changeColor;
        private static bool isCamKeyDown;
        private static bool isLeftKeyDown;
        private static bool isRightKeyDown;
        private static bool isUpKeyDown;
        private static bool isDownKeyDown;
        private static bool isEnterKeyDown;
        private static bool isCancelKeyDown;
        private static bool Broke;
        private static bool HasWantedLvl;
        private static bool hasGotWanted;
        private static bool GoToConfirm;
        private static bool GoBack;
        private static bool confirmation;
        private static int colorType;
        private static int pColor1;
        private static int pColor2;
        private static int sColor1;
        private static int sColor2;
        private static int damageCost;
        private static int engineCost;
        private static uint currEp;
        private static uint pMoney;
        private static uint pCarHealth;
        private static float pEngineHealth;
        private static uint cHealth;
        private static float eHealth;
        private static string colorTypeString;
        private static Random rnd;
        private static Vector3 offset;
        private static NativeCamera cam;
        private static int randomNum;
        private static int vehHash;
        
        public static int GenerateRandomNumber(int x, int y)
        {
            return rnd.Next(x, y);
        }
        public Main()
        {
            rnd = new Random();
            Initialized += Main_Initialized;
            Tick += new EventHandler(MainTick);
        }

        private void Main_Initialized(object sender, EventArgs e)
        {
            LoadSettings(Settings);
            cHealth = 1000;
            eHealth = 1000;
            colorType = 4;
            cams = 1;
            colorTypeString = "Primary Color";
        }

        private void MainTick(object sender, EventArgs e)
        {
            if (!enable)
                return;

            PlayerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            PlayerIndex = (int)GET_PLAYER_ID();
            PlayerHandle = PlayerPed.GetHandle();

            if (PlayerPed.IsInVehicle())
                dontCrash = true;

            if (dontCrash)
            {
                currEp = GET_CURRENT_EPISODE();
                if (cam == null)
                {
                    cam = NativeCamera.Create();
                    cam.Deactivate();
                }
                playerVehicle = IVVehicle.FromUIntPtr(PlayerPed.GetVehicle());
                vehHandle = playerVehicle.GetHandle();
                if (!IS_CHAR_DEAD(PlayerHandle))
                {
                    if (playerVehicle != null && PlayerPed.IsInVehicle())
                    {
                        if (!gotColor)
                        {
                            GET_CAR_COLOURS(vehHandle, out PrimColor1, out PrimColor2);
                            GET_EXTRA_CAR_COLOURS(vehHandle, out SecColor1, out SecColor2);
                            NumOfResprays = GET_INT_STAT(282);
                            gotColor = true;
                        }

                        if (PLAYER_IS_INTERACTING_WITH_GARAGE())
                        {
                            if (IS_WANTED_LEVEL_GREATER(PlayerIndex, 0) && !hasGotWanted)
                            {
                                HasWantedLvl = true;
                                hasGotWanted = true;
                            }
                            else if (!hasGotWanted)
                                HasWantedLvl = false;
                            
                            GET_CAR_HEALTH(vehHandle, out pCarHealth);
                            pEngineHealth = GET_ENGINE_HEALTH(vehHandle);
                            pMoney = IVPlayerInfoExtensions.GetMoney(PlayerPed.PlayerInfo);
                            if (pCarHealth < 1000 && pCarHealth >= 0)
                                cHealth = pCarHealth;
                            if (pEngineHealth < 1000 && pEngineHealth >= 0)
                                eHealth = pEngineHealth;

                            else if (pCarHealth >= 1000 && !IS_SCREEN_FADING())
                            {
                                cHealth = 1000;
                                eHealth = 1000;
                            }

                            if (1000 - (int)cHealth > (CarInitCost / CarCostMult))
                                damageCost = (int)((1000 - (int)cHealth) * CarCostMult);
                            else
                                damageCost = CarInitCost;

                            if (1000 - (int)eHealth > (EngInitCost / EngCostMult))
                                engineCost = (int)((1000 - (int)eHealth) * EngCostMult);
                            else
                                engineCost = EngInitCost;

                            if (pMoney < (int)(damageCost + engineCost))
                            {
                                IVGame.ShowSubtitleMessage("You need $" + ((int)(damageCost + engineCost)).ToString() + " to pay for the repairs");

                                IVGarages.NoResprays = true;
                            }
                            else
                                IVGarages.NoResprays = false;

                            pVeh = vehHandle;
                            pVehicle = playerVehicle;
                        }

                        else if (!PLAYER_IS_INTERACTING_WITH_GARAGE())
                            hasGotWanted = false;

                        if (NumOfResprays < GET_INT_STAT(282) && !inMenu)
                        {
                            CHANGE_CAR_COLOUR(vehHandle, PrimColor1, PrimColor2);
                            SET_EXTRA_CAR_COLOURS(vehHandle, SecColor1, SecColor2);
                            gotColor = false;

                            pColor1 = PrimColor1;
                            pColor2 = PrimColor2;
                            sColor1 = SecColor1;
                            sColor2 = SecColor2;

                            FREEZE_CAR_POSITION(pVeh, true);
                            WARP_CHAR_INTO_CAR(PlayerHandle, pVeh);
                            LOCK_CAR_DOORS(pVeh, 4);

                            IVPlayerInfoExtensions.RemoveMoney(PlayerPed.PlayerInfo, (int)(damageCost + engineCost));
                            inMenu = true;
                        }
                    }
                    if (isLeftKeyDown || isRightKeyDown)
                    {
                        if (CheckDateTime == false)
                        {
                            currentDateTime = DateTime.Now;
                            CheckDateTime = true;
                        }
                        if (DateTime.Now.Subtract(currentDateTime).TotalMilliseconds > 500)
                        {
                            CheckDateTime = false;

                            FastScroll = true;
                        }
                    }
                    else if (!isLeftKeyDown && !isRightKeyDown)
                    {
                        CheckDateTime = false;
                        FastScroll = false;
                    }
                    if (FastScroll)
                    {
                        if (CheckDateTime == false)
                        {
                            currentDateTime = DateTime.Now;
                            CheckDateTime = true;
                        }
                        if (DateTime.Now.Subtract(currentDateTime).TotalMilliseconds > 50)
                        {
                            CheckDateTime = false;
                            PickColors();
                        }
                    }

                    if (inMenu)
                    {
                        ColorMenu();
                        if (!confirmation)
                        {
                            if (NativeControls.IsGameKeyPressed(0, GameKey.NavLeft) && !isLeftKeyDown && !isRightKeyDown && !isUpKeyDown && !isDownKeyDown)
                            {
                                isLeftKeyDown = true;
                                PickColors();
                            }
                            if (NativeControls.IsGameKeyPressed(0, GameKey.NavRight) && !isLeftKeyDown && !isRightKeyDown && !isUpKeyDown && !isDownKeyDown)
                            {
                                isRightKeyDown = true;
                                PickColors();
                            }
                            if (NativeControls.IsGameKeyPressed(0, GameKey.NavUp) && !isLeftKeyDown && !isRightKeyDown && !isUpKeyDown && !isDownKeyDown)
                            {
                                if (colorType < 4)
                                    colorType += 1;
                                else
                                    colorType = 1;

                                isUpKeyDown = true;
                                PickColors();
                            }
                            if (NativeControls.IsGameKeyPressed(0, GameKey.NavDown) && !isLeftKeyDown && !isRightKeyDown && !isUpKeyDown && !isDownKeyDown)
                            {
                                if (colorType > 1)
                                    colorType -= 1;
                                else
                                    colorType = 4;
                                isDownKeyDown = true;
                                PickColors();
                            }
                        }
                        if (NativeControls.IsGameKeyPressed(0, GameKey.Action) && !isCamKeyDown)
                        {
                            if (cams < 4)
                                cams += 1;
                            else
                                cams = 1;

                            isCamKeyDown = true;
                        }
                        if (NativeControls.IsGameKeyPressed(0, GameKey.NavEnter) && !isEnterKeyDown && !isCancelKeyDown)
                        {
                            GoToConfirm = true;
                            isEnterKeyDown = true;
                        }

                        if (NativeControls.IsGameKeyPressed(0, GameKey.NavBack) && !isEnterKeyDown && !isCancelKeyDown)
                        {
                            GoBack = true;
                            isCancelKeyDown = true;
                        }

                        if (!NativeControls.IsGameKeyPressed(0, GameKey.NavLeft) && isLeftKeyDown)
                            isLeftKeyDown = false;

                        if (!NativeControls.IsGameKeyPressed(0, GameKey.NavRight) && isRightKeyDown)
                            isRightKeyDown = false;

                        if (!NativeControls.IsGameKeyPressed(0, GameKey.NavUp) && isUpKeyDown)
                            isUpKeyDown = false;

                        if (!NativeControls.IsGameKeyPressed(0, GameKey.NavDown) && isDownKeyDown)
                            isDownKeyDown = false;

                        if (!NativeControls.IsGameKeyPressed(0, GameKey.Action) && isCamKeyDown)
                            isCamKeyDown = false;

                        if (!NativeControls.IsGameKeyPressed(0, GameKey.NavEnter) && isEnterKeyDown)
                            isEnterKeyDown = false;

                        if (!NativeControls.IsGameKeyPressed(0, GameKey.NavBack) && isCancelKeyDown)
                            isCancelKeyDown = false;
                    }
                }
                if (playerVehicle != null && !PlayerPed.IsInVehicle() && gotColor)
                {
                    gotColor = false;
                    NumOfResprays = GET_INT_STAT(282);
                }
            }
        }
        private void ColorMenu()
        {
            if (!sprayCar)
            {
                pMoney = IVPlayerInfoExtensions.GetMoney(PlayerPed.PlayerInfo);
                if (confirmation)
                    IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "Press ~INPUT_PICKUP~ to change cycle through cameras.~n~Press ~INPUT_FRONTEND_ACCEPT~ to confirm.~n~Press ~INPUT_PHONE_CANCEL~ to go back.");

                else if (!Broke)
                {
                    if (!IS_USING_CONTROLLER())
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "Use ~INPUT_KB_UP~ and ~INPUT_KB_DOWN~ to change color types.~n~Use ~INPUT_KB_LEFT~ and ~INPUT_KB_RIGHT~ to browse colors.~n~Press ~INPUT_PICKUP~ to change cycle through cameras.~n~Press ~INPUT_FRONTEND_ACCEPT~ to accept.~n~Press ~INPUT_PHONE_CANCEL~ to cancel.~n~Changing colors will cost an extra $" + ColorCost.ToString() + ".");
                    else if (IS_USING_CONTROLLER())
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "Use ~PAD_DPAD_UP~ and ~PAD_DPAD_DOWN~ to change color types.~n~Use ~PAD_DPAD_LEFT~ and ~PAD_DPAD_RIGHT~ to browse colors.~n~Press ~INPUT_PICKUP~ to change cycle through cameras.~n~Press ~INPUT_FRONTEND_ACCEPT~ to accept.~n~Press ~INPUT_PHONE_CANCEL~ to cancel.~n~Changing colors will cost an extra $" + ColorCost.ToString() + ".");
                }
                else
                {
                    if (!IS_USING_CONTROLLER())
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "You cannot afford a respray!~n~Press ~INPUT_PHONE_CANCEL~ to continue.");
                    else if (IS_USING_CONTROLLER())
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "You cannot afford a respray!~n~Press ~INPUT_PHONE_CANCEL~ to continue.");
                }

                PRINT_HELP_WITH_STRING_NO_SOUND("PLACEHOLDER_1", "");
                cam.PointAtVehicle(vehHandle);
                cam.Activate();
                if (cams == 1)
                    GET_OFFSET_FROM_CAR_IN_WORLD_COORDS(vehHandle, new Vector3(0, 2.5f, 2.5f), out offset);
                else if (cams == 2)
                    GET_OFFSET_FROM_CAR_IN_WORLD_COORDS(vehHandle, new Vector3(2.5f, 0f, 2.5f), out offset);
                else if (cams == 3)
                    GET_OFFSET_FROM_CAR_IN_WORLD_COORDS(vehHandle, new Vector3(-2.5f, 0f, 2.5f), out offset);
                else if (cams == 4)
                    GET_OFFSET_FROM_CAR_IN_WORLD_COORDS(vehHandle, new Vector3(0, -2.5f, 2.5f), out offset);
                cam.Position = offset;

                if (colorType == 4)
                    IVGame.ShowSubtitleMessage(colorTypeString + " " + pColor1.ToString());
                else if (colorType == 3)
                    IVGame.ShowSubtitleMessage(colorTypeString + " " + pColor2.ToString());
                else if (colorType == 2)
                    IVGame.ShowSubtitleMessage(colorTypeString + " " + sColor1.ToString());
                else if (colorType == 1)
                    IVGame.ShowSubtitleMessage(colorTypeString + " " + sColor2.ToString());

                if (!isLeftKeyDown && !isRightKeyDown && !isUpKeyDown && !isDownKeyDown)
                {
                    if (pMoney >= ColorCost && !confirmation && GoToConfirm)
                    {
                        confirmation = true;
                        GoToConfirm = false;
                    }

                    else if (pMoney >= ColorCost && confirmation && GoToConfirm)
                    {
                        DO_SCREEN_FADE_OUT(1000);
                        changeColor = true;
                        sprayCar = true;
                        GoToConfirm = false;
                    }
                    else if (pMoney < ColorCost && GoToConfirm)
                    {
                        Broke = true;
                        GoToConfirm = false;
                    }

                    else if (confirmation && GoBack)
                    {
                        confirmation = false;
                        GoBack = false;
                    }

                    else if (!confirmation && GoBack)
                    {
                        DO_SCREEN_FADE_OUT(1000);
                        sprayCar = true;
                        GoBack = false;
                    }
                }
            }

            else if (sprayCar && IS_SCREEN_FADED_OUT())
            {
                vehHash = (pVehicle.ModelIndex.GetHashCode());
                randomNum = GenerateRandomNumber(1, 6);
                DO_SCREEN_FADE_IN(1000);

                if (!changeColor || (PrimColor1 == pColor1 && PrimColor2 == pColor2 && SecColor1 == sColor1 && SecColor2 == sColor2))
                {
                    CHANGE_CAR_COLOUR(pVeh, PrimColor1, PrimColor2);
                    SET_EXTRA_CAR_COLOURS(pVeh, SecColor1, SecColor2);
                    if (((currEp == 0 || currEp == 2) && (vehHash == 10616994 || vehHash == 7077996 || vehHash == 11927734)) || (currEp == 1 && (vehHash == 35389980 || vehHash == 38928978 || vehHash == 40239718)))
                        IVGame.ShowSubtitleMessage("This is the best I can do with this wreck.", 4000);
                    else if (((currEp == 0 || currEp == 2) && vehHash == 10682531 && pColor1 == 51 && pColor2 == 51) || (currEp == 1 && vehHash == 38994515 && pColor1 == 51 && pColor2 == 51))
                        IVGame.ShowSubtitleMessage("That's the motherfucking green sabre!", 4000);
                    else if (HasWantedLvl && randomNum > 3)
                        IVGame.ShowSubtitleMessage("They won't be looking for these plates.", 4000);
                    else if (HasWantedLvl && randomNum <= 3)
                        IVGame.ShowSubtitleMessage("These plates will throw them off the scent.", 4000);
                    else if (randomNum > 3)
                        IVGame.ShowSubtitleMessage("As good as new.", 4000);
                    else if (randomNum <= 3)
                        IVGame.ShowSubtitleMessage("Not a scratch.", 4000);
                }
                else
                {
                    if (((currEp == 0 || currEp == 2) && (vehHash == 10616994 || vehHash == 7077996 || vehHash == 11927734)) || (currEp == 1 && (vehHash == 35389980 || vehHash == 38928978 || vehHash == 40239718)))
                        IVGame.ShowSubtitleMessage("This is the best I can do with this wreck.", 4000);
                    else if (((currEp == 0 || currEp == 2) && vehHash == 10682531 && pColor1 == 51 && pColor2 == 51) || (currEp == 1 && vehHash == 38994515 && pColor1 == 51 && pColor2 == 51))
                        IVGame.ShowSubtitleMessage("That's the motherfucking green sabre!", 4000);
                    else if (HasWantedLvl && randomNum <= 2)
                        IVGame.ShowSubtitleMessage("They won't be looking for this color.", 4000);
                    else if (HasWantedLvl && randomNum > 4)
                        IVGame.ShowSubtitleMessage("People aren't going to screw with this paintjob.", 4000);
                    else if (HasWantedLvl && randomNum > 2)
                        IVGame.ShowSubtitleMessage("They'll be looking for a different color.", 4000);
                    else if (randomNum <= 2)
                        IVGame.ShowSubtitleMessage("Nice color.", 4000);
                    else if (randomNum > 4)
                        IVGame.ShowSubtitleMessage("The new color is much better.", 4000);
                    else if (randomNum > 2)
                        IVGame.ShowSubtitleMessage("Hope you like the new color.", 4000);
                    IVPlayerInfoExtensions.RemoveMoney(PlayerPed.PlayerInfo, ColorCost);
                }

                cam.Deactivate();
                CLEAR_HELP();
                Broke = false;
                FREEZE_CAR_POSITION(pVeh, false);
                LOCK_CAR_DOORS(pVeh, 0);
                colorType = 4;
                cams = 1;

                changeColor = false;
                sprayCar = false;
                inMenu = false;
                gotColor = false;
                hasGotWanted = false;
                confirmation = false;
            }
        }
        private void PickColors()
        {
            if (inMenu)
            {
                if (colorType == 4)
                {
                    colorTypeString = "Primary Color:";
                    if (isLeftKeyDown)
                    {
                        if (pColor1 > 0)
                            pColor1 -= 1;
                        else
                            pColor1 = 133;
                    }
                    else if (isRightKeyDown)
                    {
                        if (pColor1 < 133)
                            pColor1 += 1;
                        else
                            pColor1 = 0;
                    }
                }
                else if (colorType == 3)
                {
                    colorTypeString = "Secondary Color:";
                    if (isLeftKeyDown)
                    {
                        if (pColor2 > 0)
                            pColor2 -= 1;
                        else
                            pColor2 = 133;
                    }
                    else if (isRightKeyDown)
                    {
                        if (pColor2 < 133)
                            pColor2 += 1;
                        else
                            pColor2 = 0;
                    }
                }
                else if (colorType == 2)
                {
                    colorTypeString = "Pearlescent Color:";
                    if (isLeftKeyDown)
                    {
                        if (sColor1 > 0)
                            sColor1 -= 1;
                        else
                            sColor1 = 133;
                    }
                    else if (isRightKeyDown)
                    {
                        if (sColor1 < 133)
                            sColor1 += 1;
                        else
                            sColor1 = 0;
                    }
                }
                else if (colorType == 1)
                {
                    colorTypeString = "Tertiary Color:";
                    if (isLeftKeyDown)
                    {
                        if (sColor2 > 0)
                            sColor2 -= 1;
                        else
                            sColor2 = 133;
                    }
                    else if (isRightKeyDown)
                    {
                        if (sColor2 < 133)
                            sColor2 += 1;
                        else
                            sColor2 = 0;
                    }
                }
                CHANGE_CAR_COLOUR(pVeh, pColor1, pColor2);
                SET_EXTRA_CAR_COLOURS(pVeh, sColor1, sColor2);
            }
        }
        private static void LoadSettings(SettingsFile settings)
        {
            enable = settings.GetBoolean("MAIN", "Enable", true);
            CarInitCost = settings.GetInteger("MAIN", "Initial Deformation Cost", 100);
            EngInitCost = settings.GetInteger("MAIN", "Initial Engine Cost", 100);
            CarCostMult = settings.GetFloat("MAIN", "Deformation Damage Cost Multiplier", 2.0f);
            EngCostMult = settings.GetFloat("MAIN", "Engine Damage Cost Multiplier", 2.0f);
            ColorCost = settings.GetInteger("MAIN", "Color Cost", 200);
        }
    }
}