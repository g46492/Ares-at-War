﻿using System;
using System.Collections.Generic;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using System.Text;
using VRage.Utils;
using VRageMath;
using VRage.Game.ModAPI;
using Sandbox.Game.GameSystems;
using Sandbox.Game.World;
using Sandbox.Game.Entities.Planet;
using VRage.ModAPI;
using AresSystem.API;

namespace RespawnSystem
{
    public struct RespawnData
    {
        public long shipEntityId;
        public ulong steamId;
        public string RespawnShipPrefabName;
        public long TLL;
        public bool Enabled;
    }

    public struct FadeOutData
    {
        public ulong steamId;
        public long TLL;
    }


    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation)]
    public class RespawnSystem : MySessionComponentBase
    {
        public static ushort modId = 15108;
        private static Random _rnd = new Random();

        int count = 0;
        public static int runCount = 0;
        public static int loadWait = 120;

        public static List<RespawnData> _RespawnData = new List<RespawnData>();
        public static List<FadeOutData> _FadeOutData = new List<FadeOutData>();

        public static RespawnSystem Instance;
        //public MyVoxelBase entity;

        public static MatrixD TargetMatrix = new MatrixD();
        //public static MatrixD FinalMatrix = new MatrixD();


        public static Vector3D PlanetCenter = new Vector3D(0, 0, 0);
        public static Vector3D AgarisCenter = new Vector3D(-3663015, -1309985, -2583899);


        public static Vector3D Carcosa = new Vector3D(-1174736.16, 106319.66, 1325749.58);
        public static Vector3D Thora4Ice = new Vector3D(-677376.160958611, -1056251.32781255, -3498327.91690333);
        public static Vector3D Azuris = new Vector3D(-1080910.05, 135510.47, 1258013.98);
        public static Vector3D LezunoCoords = new Vector3D(-1266529.78125, -314814.913085938, -1422814.98828125);

        public static Vector3D LezunoNorth = new Vector3D(-1312209.11, -264051.53, -1470507.23);
        private static string PlanetName = "Planet Lezuno";

        public static MESApi MESApi;


        public override void LoadData()
        {
            MESApi = new MESApi();
        }

        public override void BeforeStart()
        {
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                MyVisualScriptLogicProvider.RespawnShipSpawned += RespawnShipSpawned;
            }



        }

        public static void RespawnShipSpawned(long shipEntityId, long playerId, string RespawnShipPrefabName)
        {
            ulong steamId = MyAPIGateway.Players.TryGetSteamId(playerId);
            var respawndata = new RespawnData();
            respawndata.shipEntityId = shipEntityId;
            respawndata.steamId = steamId;
            respawndata.RespawnShipPrefabName = RespawnShipPrefabName;
            respawndata.TLL = MyAPIGateway.Session.GameplayFrameCounter + 360;
            respawndata.Enabled = true;

            IMyPlayer player = GetPlayer(playerId);
            //MyAPIGateway.Utilities.ShowMessage("AaW Debug",$"{player.DisplayName} with {player.IdentityId} == {playerId} spawned {RespawnShipPrefabName}");


            if (RespawnShipPrefabName == "RespawnShip-S27")
            {
                Vector3D position = new Vector3D(-1971126.63, -1015993.3, -2313164.75);
                MESApi.ProcessStaticEncountersAtLocation(position);



                if (player.IdentityId == MyVisualScriptLogicProvider.GetLocalPlayerId())
                    Fade.fade(true);
                else if (MyAPIGateway.Multiplayer.IsServer)
                    MyAPIGateway.Multiplayer.SendMessageTo(modId, Encoding.ASCII.GetBytes("start"), MyVisualScriptLogicProvider.GetSteamId(player.IdentityId), true);


            }


            if (RespawnShipPrefabName == "RespawnShip-Starlight")
            {
                Vector3D position = new Vector3D(-712702.987357875, -1112964.03188904, -3487951.1276429);
                MESApi.ProcessStaticEncountersAtLocation(position);


                if (player.IdentityId == MyVisualScriptLogicProvider.GetLocalPlayerId())
                    Fade.fade(true);
                else if (MyAPIGateway.Multiplayer.IsServer)
                    MyAPIGateway.Multiplayer.SendMessageTo(modId, Encoding.ASCII.GetBytes("start"), MyVisualScriptLogicProvider.GetSteamId(player.IdentityId), true);
            }


            if (RespawnShipPrefabName == "Thora4-EscapePod")
            {

                bool ActivePlaceHolder = false;
                if (MyAPIGateway.Utilities.GetVariable<bool>("FrostboundDisabled", out ActivePlaceHolder) && ActivePlaceHolder)
                {
                    MyCubeGrid grid = MyEntities.GetEntityById(shipEntityId) as MyCubeGrid;
                    var block = grid.GetFirstBlockOfType<MyCockpit>();
                    

                    MyVisualScriptLogicProvider.SendChatMessage("Frostbound spawn is disabled, because some players have progressed beyond a certain point.", "AaW", playerId, "Red");
                    respawndata.Enabled = false;
                }
                else
                {
                    if (player.IdentityId == MyVisualScriptLogicProvider.GetLocalPlayerId())
                        Fade.fade(true);
                    else if (MyAPIGateway.Multiplayer.IsServer)
                        MyAPIGateway.Multiplayer.SendMessageTo(modId, Encoding.ASCII.GetBytes("start"), MyVisualScriptLogicProvider.GetSteamId(player.IdentityId), true);
                }
            }

            if (RespawnShipPrefabName == "ITC Sollieman (Crashed)")
            {
                respawndata.TLL = MyAPIGateway.Session.GameplayFrameCounter;
            }



            _RespawnData.Add(respawndata);


        }


        public static void ProssesRespawnShip(long shipEntityId, long playerId, string RespawnShipPrefabName, bool enabled = true)
        {
            MyCubeGrid grid = MyEntities.GetEntityById(shipEntityId) as MyCubeGrid;
            IMyPlayer player = GetPlayer(playerId);


            var block = grid.GetFirstBlockOfType<MyCockpit>();

            if (!enabled)
            {
                block.RemovePilot();
                MyVisualScriptLogicProvider.SendChatMessage("You can press Backspace to return to the spawn options. We apologize for the inconvenience", "AaW", playerId, "Red");
                return;
            }

            if (RespawnShipPrefabName == "ITC Mallard (Crashed)")
            {
                var HeightOffSet = 2f;

                var planet = MyGamePruningStructure.GetClosestPlanet(LezunoCoords);


                Vector3D normal = ((MyUtils.GetRandomVector3Normalized()* 15000 + LezunoNorth)- LezunoCoords).Normalized();


                Vector3D position = planet.GetClosestSurfacePointGlobal(LezunoCoords + (normal * planet.AverageRadius));
                
                Vector3 vector = MyAPIGateway.GravityProviderSystem.CalculateNaturalGravityInPoint(position);
                Vector3D up = -Vector3D.Normalize(vector);
                position += (HeightOffSet * up);

                //MyVisualScriptLogicProvider.SetWeatherP("/Weather SandStormHeavy", "SandStormHeavy", 4000, position);
                Vector3D forward = MyUtils.GetRandomPerpendicularVector(up);
                MatrixD matrix = MatrixD.CreateWorld(position, forward, up);

                var gridWorldMatrix = MatrixD.Invert(block.PositionComp.LocalMatrixRef) * matrix;

                
                MatrixD pitch = MatrixD.CreateFromAxisAngle(gridWorldMatrix.Right, MathHelper.ToRadians(2));
                MatrixD roll = MatrixD.CreateFromAxisAngle(gridWorldMatrix.Forward, MathHelper.ToRadians(-10));

                MatrixD finalMatrix = pitch * roll * gridWorldMatrix;



                MyVoxelMaterialDefinition vox = new MyVoxelMaterialDefinition();
                if (MyDefinitionManager.Static.TryGetVoxelMaterialDefinition("DustyRocks", out vox))
                {
                    //planet.PerformCutOutSphereFast(position, 40,true);

                    
                    //MyAPIGateway.Session.VoxelMaps.RequestVoxelCutoutSphere();
                    //planet.RequestVoxelCutoutSphere(position, 5, false, false);
                    //planet.RootVoxel.CreateVoxelMeteorCrater(position, 20, -up, vox); 
                }
                
                grid.Teleport(finalMatrix);
                grid.PositionComp.SetWorldMatrix(ref finalMatrix, skipTeleportCheck: true);
                grid.ConvertToStatic();
                return;
            }


            if (RespawnShipPrefabName == "ITC Sollieman (Crashed)")
            {

                Vector3 up = new Vector3D(0.624786874144575, -0.11728583489358, 0.771936133906146);
                Vector3 forward = new Vector3D(0.528042385064626, 0.791749788939157, -0.307088767769291);
                Vector3D position = new Vector3D(-3636316.10199224, -1307380.4688901, -2531247.5201753);
                MatrixD matrix = MatrixD.CreateWorld(position, forward, up);



                grid.Teleport(matrix);
                grid.PositionComp.SetWorldMatrix(ref matrix, skipTeleportCheck: true);
                grid.ConvertToStatic();
                return;
            }



            if (RespawnShipPrefabName == "Pod-A26")
            {
               
                Vector3 up = new Vector3D(0.498514652252197, 0.561518847942352, 0.66043895483017);
                Vector3 forward = new Vector3D(-0.0680525004863739, 0.784854471683502, -0.615931928157806);
                Vector3D position = new Vector3D(-1283164.04550319, -286696.288185582, -1426821.56150548);
                MatrixD matrix = MatrixD.CreateWorld(position, forward, up);


                grid.Teleport(matrix);
                grid.PositionComp.SetWorldMatrix(ref matrix, skipTeleportCheck: true);

                MyVisualScriptLogicProvider.ScreenColorFadingStart(1, true, playerId);
                MyVisualScriptLogicProvider.ScreenColorFadingMinimalizeHUD(true, playerId);

                //MatrixD Player = MatrixD.CreateWorld(position + forward * _rnd.Next(3, 5), forward, up);
                //player.Character.Teleport(Player);
            }


            if (RespawnShipPrefabName == "RespawnShip-S27")
            {

                block.RemovePilot();

                //MyAPIGateway.Utilities.ShowMessage("AaW Debug", $"remove pilot");
                Vector3 up = new Vector3D(0, 1, 0);
                Vector3 forward = new Vector3D(-1, 0, 0);
                Vector3D position = new Vector3D(-1971126.63, -1015993.3, -2313164.75);
                MatrixD matrix = MatrixD.CreateWorld(position, forward, up);

                player.Character.Teleport(matrix);


                var fadeoutdata = new FadeOutData();
                fadeoutdata.steamId = player.SteamUserId; 
                fadeoutdata.TLL = MyAPIGateway.Session.GameplayFrameCounter + 120;
                _FadeOutData.Add(fadeoutdata);



            }


            if(RespawnShipPrefabName == "RespawnShip-Starlight")
            {
                //GPS:CaptainArthur #1::::#FF75C9F1:

                Vector3 up = new Vector3D(0, 1, 0);
                Vector3 forward = new Vector3D(-1, 0, 0);
                Vector3D position = new Vector3D(-712702.987357875, -1112964.03188904, -3487951.1276429);
                MatrixD matrix = MatrixD.CreateWorld(position, forward, up);

                block.RemovePilot();
                player.Character.Teleport(matrix);

                var fadeoutdata = new FadeOutData();
                fadeoutdata.steamId = player.SteamUserId;
                fadeoutdata.TLL = MyAPIGateway.Session.GameplayFrameCounter + 120;
                _FadeOutData.Add(fadeoutdata);
            }





            if (RespawnShipPrefabName == "Thora4-EscapePod")
            {

                var planet = MyGamePruningStructure.GetClosestPlanet(Thora4Ice);

                Vector3 up = -MyAPIGateway.GravityProviderSystem.CalculateNaturalGravityInPoint(Thora4Ice).Normalized();
                Vector3 forward = MyUtils.GetRandomPerpendicularVector(up);
                Vector3D position = Thora4Ice + forward * _rnd.Next(10, 100);


                Vector3D newposition = planet.GetClosestSurfacePointGlobal(position);


                MatrixD matrix = MatrixD.CreateWorld(newposition + 3 * up, forward, up);
                var gridWorldMatrix = MatrixD.Invert(block.PositionComp.LocalMatrixRef) * matrix;
                grid.Teleport(gridWorldMatrix);
                grid.PositionComp.SetWorldMatrix(ref gridWorldMatrix, skipTeleportCheck: true);


                var fadeoutdata = new FadeOutData();
                fadeoutdata.steamId = player.SteamUserId;
                fadeoutdata.TLL = MyAPIGateway.Session.GameplayFrameCounter + 1140;
                _FadeOutData.Add(fadeoutdata);



                /*
                Vector3 up = new Vector3D(-0.608709931373596, 0.718726575374603, -0.336012244224548);
                Vector3 forward = new Vector3D(0.775614082813263, 0.449913620948792, -0.442719399929047);
                Vector3D position = new Vector3D(-679555.319760225, -1057490.48809076, -3499426.287517);
                MatrixD matrix = MatrixD.CreateWorld(position, forward, up);



                var gridWorldMatrix = MatrixD.Invert(block.PositionComp.LocalMatrixRef) * matrix;
                grid.Teleport(gridWorldMatrix);
                grid.PositionComp.SetWorldMatrix(ref gridWorldMatrix, skipTeleportCheck: true);
                */

            }

            




            if (RespawnShipPrefabName == "CarcosaRespawnRover")
            {
                Vector3 up = -MyAPIGateway.GravityProviderSystem.CalculateNaturalGravityInPoint(Carcosa).Normalized();
                Vector3 forward = MyUtils.GetRandomPerpendicularVector(up);
                Vector3D position = Carcosa + forward * _rnd.Next(50, 6000);
                MatrixD matrix = MatrixD.CreateWorld(position, forward, up);
                var gridWorldMatrix = MatrixD.Invert(block.PositionComp.LocalMatrixRef) * matrix;
                grid.Teleport(gridWorldMatrix);
                grid.PositionComp.SetWorldMatrix(ref gridWorldMatrix, skipTeleportCheck: true);
            }

            if (RespawnShipPrefabName == "AzurisRespawnRover")
            {
                Vector3 up = -MyAPIGateway.GravityProviderSystem.CalculateNaturalGravityInPoint(Azuris).Normalized();
                Vector3 forward = MyUtils.GetRandomPerpendicularVector(up);
                Vector3D position = Azuris + forward * _rnd.Next(50, 1000);
                MatrixD matrix = MatrixD.CreateWorld(position, forward, up);
                var gridWorldMatrix = MatrixD.Invert(block.PositionComp.LocalMatrixRef) * matrix;
                grid.Teleport(gridWorldMatrix);
                grid.PositionComp.SetWorldMatrix(ref gridWorldMatrix, skipTeleportCheck: true);
            }


        }


        public static void DrawOrigin(MatrixD matrix)
        {
            const float lineLen = 5;
            const float lineThick = 0.05f;
            MyStringId material = MyStringId.GetOrCompute("Square");

            MyTransparentGeometry.AddLineBillboard(material, Color.Red, matrix.Translation, (Vector3)matrix.Right, lineLen, lineThick);
            MyTransparentGeometry.AddLineBillboard(material, Color.Lime, matrix.Translation, (Vector3)matrix.Up, lineLen, lineThick);
            MyTransparentGeometry.AddLineBillboard(material, Color.Blue, matrix.Translation, (Vector3)matrix.Backward, lineLen, lineThick);
        }


        public override void UpdateAfterSimulation()
        {
            //DrawOrigin(TargetMatrix);
            //DrawOrigin(FinalMatrix);

            for (int i = _RespawnData.Count - 1; i >= 0; i--)
            {
                var data = _RespawnData[i];
                if (data.TLL < MyAPIGateway.Session.GameplayFrameCounter)
                {
                    long playerid = MyAPIGateway.Players.TryGetIdentityId(data.steamId);
                    ProssesRespawnShip(data.shipEntityId, playerid, data.RespawnShipPrefabName, data.Enabled);
                    _RespawnData.RemoveAtFast(i);
                }
            }

            for (int i = _FadeOutData.Count - 1; i >= 0; i--)
            {
                var data = _FadeOutData[i];

                if (data.TLL < MyAPIGateway.Session.GameplayFrameCounter)
                {
                    long playerid = MyAPIGateway.Players.TryGetIdentityId(data.steamId);

                    MyVisualScriptLogicProvider.ScreenColorFadingStartSwitch(8, playerid);
                    _FadeOutData.RemoveAtFast(i);
                }
            }




            try
            {
                if (++runCount % 15 > 0) // Run every quarter of a second
                    return;



                // START BLINK STUFF
                // Blinking during load screen causes crash, don't load messagehandler on clients for 30s
                if (!MyAPIGateway.Multiplayer.IsServer && loadWait > 0)
                {
                    if (--loadWait == 0)
                        MyAPIGateway.Multiplayer.RegisterMessageHandler(modId, Fade.getFade);
                    return;
                }


                foreach (var data in _FadeOutData)
                {
                    if (data.TLL > MyAPIGateway.Session.GameplayFrameCounter)
                    {
                        continue;
                    }

                    long playerid = MyAPIGateway.Players.TryGetIdentityId(data.steamId);
                    if (playerid == MyVisualScriptLogicProvider.GetLocalPlayerId())
                        Fade.fade(false);
                    else
                        MyAPIGateway.Multiplayer.SendMessageTo(modId, Encoding.ASCII.GetBytes("unblink"), MyVisualScriptLogicProvider.GetSteamId(playerid), true);
                }



                if (runCount > 299)
                    runCount = 0;
            }
            catch (Exception ex)
            {
                //to bad
                MyAPIGateway.Utilities.ShowMessage("AaW respawnsystem", "error it crashed");
            }





        }





        public static IMyPlayer GetPlayer(long playerId)
        {
            IMyPlayer return_player = null;

            List<IMyPlayer> all_players = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(all_players);

            foreach (var p in all_players)
            {
                if (p.IdentityId == playerId)
                {
                    return_player = p;
                    break;
                }
            }

            return return_player;
        }


        protected override void UnloadData()
        {
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                MyVisualScriptLogicProvider.RespawnShipSpawned -= RespawnShipSpawned;
            }


            MyAPIGateway.Multiplayer.UnregisterMessageHandler(modId, Fade.getFade);


            MESApi = null;
        }


        public static class Fade
        {
            public static void getFade(byte[] poke)
            {
                try
                {
                    var msg = ASCIIEncoding.ASCII.GetString(poke);
                    fade(msg == "start");
                }
                catch (Exception ex)
                {
                    Echo("Fatigue exception", ex.ToString());
                }
            }

            public static void fade(bool start)
            {
                if (start)
                {
                    MyVisualScriptLogicProvider.ScreenColorFadingSetColor(Color.Black, 0L);
                    MyVisualScriptLogicProvider.ScreenColorFadingStart(0.25f, start, 0L);
                }
                else
                {
                    MyVisualScriptLogicProvider.ScreenColorFadingSetColor(Color.Black, 0L);
                    MyVisualScriptLogicProvider.ScreenColorFadingStart(8, start, 0L);
                }

            }

            public static void Echo(string msg1, string msg2 = "")
            {
                MyLog.Default.WriteLineAndConsole(msg1 + ": " + msg2);
            }
        }


    }
}
