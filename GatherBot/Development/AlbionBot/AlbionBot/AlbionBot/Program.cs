using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using AOKore.Concurrent;
using AOKore.Game;
using AOKore.Script;
using System.ComponentModel;

namespace Script
{
    public class PathData
    {
        public Vector3[] Path { get; set; }
    }
    public class Configuration
    {
        internal double[] WoodHarvestTier = new double[] { };
        internal double[] RockHarvestTier = new double[] { };
        internal double[] OreHarvestTier = new double[] { };
        internal double[] FiberHarvestTier = new double[] { };
        internal double[] HideHarvestTier = new double[] { };
        internal double[] RoughDiamondHarvestTier = new double[] { };

        [Category("1. CONFIG")]
        [DisplayName("Search Range")]
        public float RangeSearch { get; set; }

        [Category("2. WAYPOINTS")]
        [DisplayName("(Gather area)")]
        public string PathWayPoint { get; set; }

        [Category("2. WAYPOINTS")]
        [DisplayName("(Gather area > City)")]
        public string PathWayPointCity { get; set; }

        [Category("2. WAYPOINTS")]
        [DisplayName("(City > Gather area)")]
        public string PathWayPointHunt { get; set; }

       /* [Category("2. WAYPOINTS")]
        [DisplayName("(City > Repair)")]
        public string PathWayPointRepair { get; set; }*/

        //WOOD
        [Category("3. WOOD")]
        [DisplayName("Tier")]
        public string _woodharvestTier
        {
            get { return string.Join("; ", WoodHarvestTier.Select(i => i.ToString()).ToArray()); }
            set { WoodHarvestTier = value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(i => double.Parse(i)).ToArray(); }
        }

        [Category("3. WOOD")]
        [DisplayName("Collect")]
        public bool WoodHarvestCondition { get; set; }

        //ROCK
        [Category("4. ROCK")]
        [DisplayName("Tier")]
        public string _rockharvestTier
        {
            get { return string.Join("; ", RockHarvestTier.Select(i => i.ToString()).ToArray()); }
            set { RockHarvestTier = value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(i => double.Parse(i)).ToArray(); }
        }

        [Category("4. ROCK")]
        [DisplayName("Collect")]
        public bool RockHarvestCondition { get; set; }

        //ORE
        [Category("5. ORE")]
        [DisplayName("Tier")]
        public string _oreharvestTier
        {
            get { return string.Join("; ", OreHarvestTier.Select(i => i.ToString()).ToArray()); }
            set { OreHarvestTier = value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(i => double.Parse(i)).ToArray(); }
        }

        [Category("5. ORE")]
        [DisplayName("Collect")]
        public bool OreHarvestCondition { get; set; }

        //FIBER
        [Category("6. FIBER")]
        [DisplayName("Tier")]
        public string _fiberharvestTier
        {
            get { return string.Join("; ", FiberHarvestTier.Select(i => i.ToString()).ToArray()); }
            set { FiberHarvestTier = value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(i => double.Parse(i)).ToArray(); }
        }

        [Category("6. FIBER")]
        [DisplayName("Collect")]
        public bool FiberHarvestCondition { get; set; }

        //HIDE
        [Category("7. HIDE")]
        [DisplayName("Tier")]
        public string _hideharvestTier
        {
            get { return string.Join("; ", HideHarvestTier.Select(i => i.ToString()).ToArray()); }
            set { HideHarvestTier = value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(i => double.Parse(i)).ToArray(); }
        }

        [Category("7. HIDE")]
        [DisplayName("Collect")]
        public bool HideHarvestCondition { get; set; }

    }
    public class Program
    {
        [Configuration("Automatic Gathering Settings")]
        public static Configuration Settings { get; set; }

        private static ConcurrentList<KeyValuePair<Func<bool>, int>> PlayerData = new ConcurrentList<KeyValuePair<Func<bool>, int>>();
        public static string path = System.Environment.CurrentDirectory + "\\Plugins\\Cache\\WayPoint\\";
        public static bool pcity;
        public static bool IsMount = false;
        public static float AtkRanger;
        public static float Ep;
        public static float Hp;
        public static float MEp;
        public static float MHp;
        public static long PId;
        public static long IsSelect;
        public static bool IsAtck = false;
        public static bool iRecovery = false;
        public static bool Scape = false;
        public static Vector3 Area = Vector3.Zero;
        public static bool can = false;
        public static Vector3 rd = Vector3.Zero;
        public static long LastMobId;
        public static bool Dead;
        public static bool IsChann;
        public static float WayDist;
        public static float atkd;
        public static bool isMelee;
        public static bool Canleave = false;
        public static bool city;
        public static bool conn;
        public static long selected;
        const string ScriptName = "Automatic Gathering";
        public static PathData map;
        public static PathData mapCity;
        public static PathData mapHunt;
        public static PathData mapRepair;

        // Conditions

        public static Vector3 PointReference = null;
        public static int timeout = 0;
        public static bool Stop_Harvest = false;
        public static long Last_ID = 0;
        public static float Weight = 0;
        public static bool forward = true;
        public static bool ToCity = false;
        public static bool ToRepair = false;
        public static bool ToGathering = true;
        public static bool MEquiped = false;
        public static bool Debugger = true;



        public static void Main()
        {
            ClearLog();
            Log("Automatic Gathering Started!");
            try
            {
                Log("Loading Waypoints...");
                map = FromXml<PathData>(File.ReadAllText(@path + Settings.PathWayPoint));
                mapCity = FromXml<PathData>(File.ReadAllText(@path + Settings.PathWayPointCity));
                mapHunt = FromXml<PathData>(File.ReadAllText(@path + Settings.PathWayPointHunt));
                //mapRepair = FromXml<PathData>(File.ReadAllText(@path + Settings.PathWayPointRepair));
            }
            catch (Exception ex)
            {
                Log("File not found. [" + ex.Message + "]");
                return;
            }

            PlayerData.Add(new KeyValuePair<Func<bool>, int>(Action_Check, 0));
            PlayerData.Add(new KeyValuePair<Func<bool>, int>(Action_Defense, 450));
            PlayerData.Add(new KeyValuePair<Func<bool>, int>(Action_Harvest, 500));
            PlayerData.Add(new KeyValuePair<Func<bool>, int>(Action_Combat, 550));
            PlayerData.Add(new KeyValuePair<Func<bool>, int>(Action_Move, 600));
            PlayerData.Add(new KeyValuePair<Func<bool>, int>(Action_To_City, 625));
            PlayerData.Add(new KeyValuePair<Func<bool>, int>(Action_To_Bank, 635));
            //PlayerData.Add(new KeyValuePair<Func<bool>, int>(Action_To_Repair, 650));
            PlayerData.Add(new KeyValuePair<Func<bool>, int>(Action_To_Gathering, 700));
            // PlayerData.Add(new KeyValuePair<Func<bool>, int>(Action_Idle, int.MaxValue));

            ConcurrentTaskManager.CreateService(() => {
                foreach (var action in PlayerData.OrderBy(i => i.Value).Select(i => i.Key))
                {
                    if (action()) break;
                }
            }, "Automatic Gathering", 1);
        }

        public static bool Action_Check()
        {

            bool Condition = false;
            ConcurrentTaskManager.GameRun(() =>
            {
                if (Client.ClientState.ToString() != (string)"Connected") return; // se nao estiver conectado
                    if (!Player.HasPlayer) return; //se não estiver jogando
                    if (Player.IsDead) return; //se estiver morto
                    Condition = true;
            });

            if (!Condition) return false;
            MEquiped = MountEquiped;
            Weight = _Update_Weight;
            return false; //false por que é apenas uma verificação

        } //OK

        public static bool Action_Defense()
        {
            MobInstance TargetMob = null;
            Vector3[] path = mapCity.Path;
            Vector3 nextPoint = null;
            Vector3 playerLocation = null;
            bool Condition = false;

            ConcurrentTaskManager.GameRun(() =>
            {
                if (Client.ClientState.ToString() != (string)"Connected") return; // se nao estiver conectado
                if (!Player.HasPlayer) return; //se não estiver jogando
                if (Player.IsPlayerInCity) return; //se estiver na cidade
                if (Player.IsHarvesting) return; //se estiver colhendo
                if (Mount.IsCasting) return; //se estiver castando montaria
                if (Player.IsDead) return; //se estiver morto
                if (Weight > 99) return; //se o peso estiver acima de 99%

                Condition = true;
            });
            if (!Condition) return false;

            ConcurrentTaskManager.GameRun(() =>
            {
                playerLocation = Player.Location;

                TargetMob = Mob.Mobs
                    .OrderBy(i => Player.Location.DistanceXZ(i.Location))
                    .Where(i => i.TargetId == Player.ID)
                    .Where(i => !i.IsDead)
                    .FirstOrDefault();
            });

            if (TargetMob == null) return false;

            if (Debugger) Log("Action_Defense");

            ConcurrentTaskManager.GameRun(() =>
            {

                if (Player.SelectedtId == TargetMob.Id)
                {
                    if (Mount.IsMounted)
                        Mount.MountDismount();

                    Player.DefaultAttack();
                }
                else
                    Player.Select(TargetMob.Id);
            });
            Thread.Sleep(500);
            return true;
        } //Ok

        public static bool Action_Harvest()
        {
            if (Settings.WoodHarvestCondition || Settings.RockHarvestCondition || Settings.OreHarvestCondition || Settings.FiberHarvestCondition || Settings.HideHarvestCondition)
            {
                //(re)declara valores nulos
                HarvestableInstance[] TargetHarv = null;
                Vector3 PlayerLocation = null;
                Vector3 LocationAdjusted = null;
                Vector3[] path = map.Path;
                float Adjuste = 1.25f;
                bool Condition = false;

                ConcurrentTaskManager.GameRun(() =>
                {
                    if (Client.ClientState.ToString() != (string)"Connected") return; // se nao estiver conectado
                    if (!Player.HasPlayer) return; //se não estiver jogando
                    if (Player.IsPlayerInCity) return; //se estiver na cidade
                    if (Player.IsHarvesting) return; //se estiver colhendo
                    if (Mount.IsCasting) return; //se estiver castando montaria
                    if (Player.IsDead) return; //se estiver morto
                    if (Weight > 99) return; //se o peso estiver acima de 99%

                    Condition = true;
                });

                if (!Condition || _Check_IsCombat) return false;

                if (Debugger) Log("Action_Harvest");

                ConcurrentTaskManager.GameRun(() =>
                {
                    PlayerLocation = Player.Location;

                        PointReference = path.OrderBy(i => i.DistanceXZ(PlayerLocation)).FirstOrDefault();

                    TargetHarv = Harvest.Harvestables
                        .OrderBy(i => PointReference.DistanceXZ(i.Location))
                        .Where(i => PointReference.DistanceXZ(i.Location) <= Settings.RangeSearch)
                        //.Where(i => Player.CanMove(Vector3.DistanceAdjust(PlayerLocation, i.Location, 4f)))
                        .Where(i => i.Charges > 0)
                        .Where(i => i.ResourceType == "WOOD" || i.ResourceType == "ROCK" || i.ResourceType == "ORE" || i.ResourceType == "HIDE" || i.ResourceType == "FIBER")
                        .Where(i => i.Id != Last_ID)
                        .ToArray();
                });

                if (TargetHarv == null)
                {
                    return false;
                }

                _Mount();

                foreach (var TargetHarv_2 in TargetHarv)
                {
                    //Wood
                    if (Settings.WoodHarvestCondition && TargetHarv_2.ResourceType == "WOOD")
                    {
                        for (int check = 0; check < Settings.WoodHarvestTier.Length; check++)
                            if (ExtTier(Settings.WoodHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                && ExtRareState(Settings.WoodHarvestTier[check]) == TargetHarv_2.RareState)
                            {
                                //LocationAdjusted = Adjust(PlayerLocation, TargetHarv_2.Location);

                                MoveAndWait(Vector3.DistanceAdjust(PlayerLocation, TargetHarv_2.Location, 2f));

                                Thread.Sleep(500);

                                ConcurrentTaskManager.GameRun(() =>
                                {
                                    Last_ID = TargetHarv_2.Id;
                                    Player.ObjectInteraction(TargetHarv_2.ObjectView); //Inicia a Interação (Colher)
                                    });

                                if (_Check_Harvested) Log("[" + TargetHarv_2.ResourceType + "] Tier[" + TargetHarv_2.Tier + "." + TargetHarv_2.RareState + "] Harvested Successful");

                                Thread.Sleep(500);
                                break;
                            }
                    }
                    //Rock	
                    if (Settings.RockHarvestCondition && TargetHarv_2.ResourceType == "ROCK")
                    {
                        for (int check = 0; check < Settings.RockHarvestTier.Length; check++)
                            if (ExtTier(Settings.RockHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                && ExtRareState(Settings.RockHarvestTier[check]) == TargetHarv_2.RareState)
                            {
                                //LocationAdjusted = Adjust(PlayerLocation, TargetHarv_2.Location);

                                MoveAndWait(Vector3.DistanceAdjust(PlayerLocation, TargetHarv_2.Location, 2f));
                                Thread.Sleep(500);

                                ConcurrentTaskManager.GameRun(() =>
                                {
                                    Last_ID = TargetHarv_2.Id;
                                    Player.ObjectInteraction(TargetHarv_2.ObjectView); //Inicia a Interação (Colher)
                                });

                                if (_Check_Harvested) Log("[" + TargetHarv_2.ResourceType + "] Tier[" + TargetHarv_2.Tier + "." + TargetHarv_2.RareState + "] Harvested Successful");

                                Thread.Sleep(500);
                                break;
                            }

                    }
                    //Ore		
                    if (Settings.OreHarvestCondition && TargetHarv_2.ResourceType == "ORE")
                    {
                        for (int check = 0; check < Settings.OreHarvestTier.Length; check++)
                            if (ExtTier(Settings.OreHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                && ExtRareState(Settings.OreHarvestTier[check]) == TargetHarv_2.RareState)
                            {
                                //LocationAdjusted = Adjust(PlayerLocation, TargetHarv_2.Location);

                                MoveAndWait(Vector3.DistanceAdjust(PlayerLocation, TargetHarv_2.Location, 2f));
                                Thread.Sleep(500);

                                ConcurrentTaskManager.GameRun(() =>
                                {
                                    Last_ID = TargetHarv_2.Id;
                                    Player.ObjectInteraction(TargetHarv_2.ObjectView); //Inicia a Interação (Colher)
                                });

                                if (_Check_Harvested) Log("[" + TargetHarv_2.ResourceType + "] Tier[" + TargetHarv_2.Tier + "." + TargetHarv_2.RareState + "] Harvested Successful");

                                Thread.Sleep(500);
                                break;
                            }

                    }
                    //Fiber	
                    if (Settings.FiberHarvestCondition && TargetHarv_2.ResourceType == "FIBER")
                    {
                        for (int check = 0; check < Settings.FiberHarvestTier.Length; check++)
                            if (ExtTier(Settings.FiberHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                && ExtRareState(Settings.FiberHarvestTier[check]) == TargetHarv_2.RareState)
                            {
                                //LocationAdjusted = Adjust(PlayerLocation, TargetHarv_2.Location);

                                MoveAndWait(Vector3.DistanceAdjust(PlayerLocation, TargetHarv_2.Location, 2f));
                                Thread.Sleep(500);

                                ConcurrentTaskManager.GameRun(() =>
                                {
                                    Last_ID = TargetHarv_2.Id;
                                    Player.ObjectInteraction(TargetHarv_2.ObjectView); //Inicia a Interação (Colher)
                                });

                                if (_Check_Harvested) Log("[" + TargetHarv_2.ResourceType + "] Tier[" + TargetHarv_2.Tier + "." + TargetHarv_2.RareState + "] Harvested Successful");

                                Thread.Sleep(500);
                                break;
                            }

                    }
                    //Hide
                    if (Settings.HideHarvestCondition && TargetHarv_2.ResourceType == "HIDE")
                    {
                        for (int check = 0; check < Settings.HideHarvestTier.Length; check++)
                            if (ExtTier(Settings.HideHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                && ExtRareState(Settings.HideHarvestTier[check]) == TargetHarv_2.RareState)
                            {
                                //LocationAdjusted = Adjust(PlayerLocation, TargetHarv_2.Location);

                                MoveAndWait(Vector3.DistanceAdjust(PlayerLocation, TargetHarv_2.Location, 2f));
                                Thread.Sleep(500);

                                ConcurrentTaskManager.GameRun(() =>
                                {
                                    Last_ID = TargetHarv_2.Id;
                                    Player.ObjectInteraction(TargetHarv_2.ObjectView); //Inicia a Interação (Colher)
                                });

                                if (_Check_Harvested) Log("[" + TargetHarv_2.ResourceType + "] Tier[" + TargetHarv_2.Tier + "." + TargetHarv_2.RareState + "] Harvested Successful");

                                Thread.Sleep(500);
                                break;
                            }

                    }
                        //Nenhum caso
                }
                return false;
            }
            return false;
        } //OK

        public static bool Action_Combat()
        {
            if (Settings.HideHarvestCondition)
            {
                MobInstance TargetMob = null;
                Vector3[] path = map.Path;
                Vector3 PlayerLocation = null;
                bool Condition = false;

                ConcurrentTaskManager.GameRun(() =>
                {
                    if (Client.ClientState.ToString() != (string)"Connected") return; // se nao estiver conectado
                    if (!Player.HasPlayer) return; //se não estiver jogando
                    if (Player.IsPlayerInCity) return; //se estiver na cidade
                    if (Player.IsHarvesting) return; //se estiver colhendo
                    if (Mount.IsCasting) return; //se estiver castando montaria
                    if (Player.IsDead) return; //se estiver morto
                    if (Player.IsInCombat) return; //se estiver morto
                    if (Weight > 99) return; //se o peso estiver acima de 99%

                    Condition = true;
                });
                if (!Condition || _Check_IsCombat) return false;

                ConcurrentTaskManager.GameRun(() =>
                {
                    PlayerLocation = Player.Location;

                    PointReference = path.OrderBy(i => i.DistanceXZ(PlayerLocation)).FirstOrDefault();

                    TargetMob = Mob.Mobs
                        .OrderBy(i => PointReference.DistanceXZ(i.Location))
                        .Where(i => PointReference.DistanceXZ(i.Location) <= Settings.RangeSearch)
                        .Where(i => !i.IsDead)
                        .FirstOrDefault();
                });

                if (TargetMob == null) return false;

                if (Debugger) Log("Action_Combat");

                for (int check = 0; check < Settings.HideHarvestTier.Length; check++)
                    if (ExtTier(Settings.HideHarvestTier[check]) == TargetMob.Tier //Verifica se o tier está na lista
                        && ExtRareState(Settings.HideHarvestTier[check]) == TargetMob.RareState)
                    {
                        ConcurrentTaskManager.GameRun(() =>
                        {
                            Player.Select(TargetMob.Id);
                            if (Mount.IsMounted)
                                Mount.MountDismount();

                            Player.DefaultAttack();
                        });

                        Thread.Sleep(750);

                        ConcurrentTaskManager.GameRun(() =>
                        {
                            Player.DefaultAttack();
                        });

                        for (int TimerExit = 0; TimerExit < 40; TimerExit++)
                        {
                            if (_Check_IsCombat) break;
                            Thread.Sleep(100);
                        }

                        return true;
                    }

                return false;
            }
            return false;
        }

        public static bool Action_Move() //OK
        {
            Vector3[] path = map.Path;
            Vector3 nextPoint = null;
            Vector3 playerLocation = null;
            bool Condition = false;

            ConcurrentTaskManager.GameRun(() =>
            {
                if (Client.ClientState.ToString() != (string)"Connected") return; // se nao estiver conectado
                if (!Player.HasPlayer) return; //se não estiver jogando
                if (Player.IsPlayerInCity) return; //se estiver na cidade
                if (Player.IsHarvesting) return; //se estiver colhendo
                if (Mount.IsCasting) return; //se estiver castando montaria
                if (Player.IsDead) return; //se estiver morto
                if (Weight > 99) return; //se o peso estiver acima de 99%

                //ele so vai passar se tiver fora da cidade e com peso inferior a 99%
                Condition = true;
            });

            if (!Condition || _Check_IsCombat) return false;

            if (Debugger) Log("Action_Move");

            _Mount();

            MoveAndWait(path);

            return true;
        }

        public static bool Action_To_City()
        {
            Vector3[] path_Start = map.Path;
            Vector3[] path = mapCity.Path;
            Vector3 PlayerLocation = null;
            Vector3 BankLocation = null;
            bool inCity = false;
            bool Condition = false;

            ConcurrentTaskManager.GameRun(() =>
            {
                if (Client.ClientState.ToString() != (string)"Connected") return; // se nao estiver conectado
                if (!Player.HasPlayer) return; //se não estiver jogando
                if (Player.IsHarvesting) return; //se estiver colhendo
                if (Mount.IsCasting) return; //se estiver castando montaria
                if (Player.IsDead) return; //se estiver morto
                if (Weight <= 99) return; //se o peso estiver acima de 99%
                if (ToRepair) return; //se o peso estiver acima de 99%

                Condition = true;
            });
            if (!Condition || _Check_IsCombat) return false;

            if (Debugger) Log("Action_To_City");

            _Mount();
            ConcurrentTaskManager.GameRun(() =>
            {
                inCity = Player.IsPlayerInCity;
            });

            if (!inCity)
                MoveAndWait(path_Start, false);

            MoveAndWait(path, true);

            ConcurrentTaskManager.GameRun(() =>
            {
                PlayerLocation = Player.Location;
                BankLocation = Bank.Location;
            });

            if (BankLocation.DistanceXZ(PlayerLocation) < 25f)
            {
                ToRepair = true;
            }

            return true;
        }

        public static bool Action_To_Bank()
        {
            Vector3 PlayerLocation = null;
            Vector3 BankLocation = null;
            Vector3 LocationAdjusted = null;
            ItemInstance[] Itens = null;
            bool Condition = false;

            ConcurrentTaskManager.GameRun(() =>
            {

                if (Client.ClientState.ToString() != (string)"Connected") return; // se nao estiver conectado
                if (!Player.HasPlayer) return; //se não estiver jogando
                if (Player.IsHarvesting) return; //se estiver colhendo
                if (!Player.IsPlayerInCity) return; //se estiver fora da cidade
                if (Mount.IsCasting) return; //se estiver castando montaria
                if (Player.IsDead) return; //se estiver morto
                if (Weight <= 99) return; //se o peso estiver acima de 99%

                Condition = true;
                PlayerLocation = Player.Location;
                BankLocation = Bank.Location;
            });

            if (!Condition) return false;

            if (Debugger) Log("Action_To_Bank");

            _Mount();

            ConcurrentTaskManager.GameRun(() =>
            {
                PlayerLocation = Player.Location;
                BankLocation = Bank.Location;
            });

            if (BankLocation.DistanceXZ(PlayerLocation) < 25f)
            {
                //LocationAdjusted = Adjust(PlayerLocation, BankLocation);

                MoveAndWait(Vector3.DistanceAdjust(PlayerLocation, BankLocation, 2.5f));
        

                ConcurrentTaskManager.GameRun(() =>
                {
                    Bank.Open();
                    Itens = Item.Items;
                });

                bool BankIsOpen = false;
                ConcurrentTaskManager.GameRun(() =>
                {
                    BankIsOpen = Npc.BankIsOpen;
                });

                if (BankIsOpen)
                {
                    foreach (var It in Itens)
                    {
                        if ((bool)It.InInventory && (short)It.Count > 5 && Npc.BankIsOpen)
                        {
                            ConcurrentTaskManager.GameRun(() =>
                            {
                                Log("Keeping... [" + It.Name + "][" + It.Count + "]");
                                Item.MoveToBank((long)It.Id);
                            });
                            Thread.Sleep(1000);
                        }
                        Thread.Sleep(250);
                    }

                    ConcurrentTaskManager.GameRun(() =>
                    {
                        Itens = Item.Items;
                    });
                    foreach (var It in Itens)
                    {
                        if ((bool)It.InInventory && (short)It.Count > 5 && Npc.BankIsOpen)
                        {
                            ConcurrentTaskManager.GameRun(() =>
                            {
                                Log("Keeping... [" + It.Name + "][" + It.Count + "]");
                                Item.MoveToBank((long)It.Id);
                            });
                            Thread.Sleep(1000);
                        }
                        Thread.Sleep(250);
                    }

                    ConcurrentTaskManager.GameRun(() =>
                    {
                        Bank.Close();
                    });

                    return true; //se eu der false ele pula pro prx
                }
                else
                { return false; }

               
            }
            else
                return false;


        }

        public static bool Action_To_Repair()
        {
            Vector3[] path = mapRepair.Path;
            Vector3 RepairLocation = null;
            Vector3 PlayerLocation = null;
            Vector3 LocationAdjusted = null;
            bool Condition = false;

            ConcurrentTaskManager.GameRun(() =>
            {
                if (Client.ClientState.ToString() != (string)"Connected") return; // se nao estiver conectado
                if (!Player.HasPlayer) return; //se não estiver jogando
                if (Player.IsHarvesting) return; //se estiver colhendo
                if (!Player.IsPlayerInCity) return; //se estiver fora da cidade
                if (Mount.IsCasting) return; //se estiver castando montaria
                if (Player.IsDead) return; //se estiver morto
                if (Weight > 99) return; //se o peso estiver acima de 99%
                if (!ToRepair) return;

                Condition = true;
            });
            if (!Condition) return false;

            if (Debugger) Log("Action_To_Repair");

            _Mount();

            MoveAndWait(path, true);

            ConcurrentTaskManager.GameRun(() =>
            {
                PlayerLocation = Player.Location;
                RepairLocation = Repair.Location;
            });

            Thread.Sleep(1000);

            if (RepairLocation.DistanceXZ(PlayerLocation) < 25f)
            {
                LocationAdjusted = Adjust(PlayerLocation, RepairLocation);

                MoveAndWait(LocationAdjusted);

                ConcurrentTaskManager.GameRun(() =>
                {
                    Repair.Open();
                });

                Thread.Sleep(500);

                ConcurrentTaskManager.GameRun(() =>
                {
                    Repair.RepairAll();
                });

                Log("Repairing...");
                Thread.Sleep(2500);

                ConcurrentTaskManager.GameRun(() =>
                {
                    Repair.Close();
                });

                Thread.Sleep(500);
            }

            _Mount();

            //MoveAndWait(path, false);

            ToRepair = false;

            return true;
        }

        public static bool Action_To_Gathering()
        {
            Vector3[] path = mapHunt.Path;
            Vector3 nextPoint = null;
            Vector3 playerLocation = null;
            bool Condition = false;
            bool Montar = false;
            ConcurrentTaskManager.GameRun(() =>
            {
                if (Client.ClientState.ToString() != (string)"Connected") return; // se nao estiver conectado
                if (!Player.HasPlayer) return; //se não estiver jogando
                if (Player.IsHarvesting) return; //se estiver colhendo
                if (!Player.IsPlayerInCity) return; //se estiver na cidade
                if (Mount.IsCasting) return; //se estiver castando montaria
                if (Player.IsDead) return; //se estiver morto
                if (Weight > 99) return; //se o peso estiver acima de 99%
                //if (ToRepair) return;
                Condition = true;

            });
            if (!Condition) return false;

            if (Debugger) Log("Action_To_Gathering");

            if (Montar)
                _Mount();

            MoveAndWait(path, true);

            return true;
        }




        public static bool _Check_IsCombat
        {
            get
            {
                MobInstance TargetMob = null;
                ConcurrentTaskManager.GameRun(() =>
                {
                    TargetMob = Mob.Mobs
                        .OrderBy(i => Player.Location.DistanceXZ(i.Location))
                        .Where(i => i.TargetId == Player.ID)
                        .Where(i => !i.IsDead)
                        .FirstOrDefault();
                });
                if (TargetMob == null)
                    return false;

                return true;
            }
        }

        public static bool _Check_Harvested
        {
            get
            {
                HarvestableInstance TargetHarv = null;
                bool IsHarvesting = false;
                bool IsCombat = false;
                int TimerExit = 0;
                while (true)
                {
                    ConcurrentTaskManager.GameRun(() =>
                    {
                        if (Client.ClientState.ToString() != (string)"Connected" || !Player.HasPlayer) return;
                        IsCombat = Player.IsInCombat;
                        IsHarvesting = Player.IsHarvesting;
                        TargetHarv = Harvest.Harvestables
                                 .Where(i => i.Id == Last_ID)
                                 .FirstOrDefault();
                    });
                    Thread.Sleep(100);

                    if (!IsHarvesting)
                        TimerExit++;
                    else
                        TimerExit = 0;

                    if (TargetHarv.Charges == 0) return true;
                    if (!IsHarvesting && TimerExit >= 30) return false;
                    if (IsCombat) return false;
                }
            }
        }

        public static float _Update_Weight
        {
            get
            {
                float _Weight = 0f;
                if (MEquiped)
                {
                    ConcurrentTaskManager.GameRun(() =>
                    {
                        if (Mount.IsMounted)
                            _Weight = (float)Player.Weight;
                    });
                    return _Weight;
                }
                else
                {
                    ConcurrentTaskManager.GameRun(() =>
                    {
                        _Weight = (float)Player.Weight;
                    });
                    return _Weight;
                }
            }
        }

        public static bool _Check_Harvest(Vector3 PointReference)
        {
            Vector3 PlayerLocation = null;
            MobInstance TargetMob = null;
            HarvestableInstance[] TargetHarv = null;
            ConcurrentTaskManager.GameRun(() =>
            {
                PlayerLocation = Player.Location;

                TargetHarv = Harvest.Harvestables
                .OrderBy(i => PointReference.DistanceXZ(i.Location))
                .Where(i => PointReference.DistanceXZ(i.Location) <= Settings.RangeSearch)
                //.Where(i => Player.CanMove(Vector3.DistanceAdjust(PlayerLocation, i.Location, 4f)))
                .Where(i => i.Charges > 0)
                .ToArray();

                TargetMob = Mob.Mobs
                    .OrderBy(i => PointReference.DistanceXZ(i.Location))
                    .Where(i => PointReference.DistanceXZ(i.Location) <= Settings.RangeSearch)
                    .Where(i => !i.IsDead)
                    .FirstOrDefault();
            });

            if (TargetHarv != null)
            {
                foreach (var TargetHarv_2 in TargetHarv)
                {
                    //Wood
                    if (Settings.WoodHarvestCondition && TargetHarv_2.ResourceType == "WOOD")
                    {
                        for (int check = 0; check < Settings.WoodHarvestTier.Length; check++)
                            if (ExtTier(Settings.WoodHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                && ExtRareState(Settings.WoodHarvestTier[check]) == TargetHarv_2.RareState)
                            {
                                return true;
                            }

                        return false;
                    }
                    //Rock	
                    if (Settings.RockHarvestCondition && TargetHarv_2.ResourceType == "ROCK")
                    {
                        for (int check = 0; check < Settings.RockHarvestTier.Length; check++)
                            if (ExtTier(Settings.RockHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                && ExtRareState(Settings.RockHarvestTier[check]) == TargetHarv_2.RareState)
                            {
                                return true;
                            }
                        return false;
                    }
                    //Ore		
                    if (Settings.OreHarvestCondition && TargetHarv_2.ResourceType == "ORE")
                    {
                        for (int check = 0; check < Settings.OreHarvestTier.Length; check++)
                            if (ExtTier(Settings.OreHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                && ExtRareState(Settings.OreHarvestTier[check]) == TargetHarv_2.RareState)
                            {
                                return true;
                            }
                        return false;
                    }
                    //Fiber	
                    if (Settings.FiberHarvestCondition && TargetHarv_2.ResourceType == "FIBER")
                    {
                        for (int check = 0; check < Settings.FiberHarvestTier.Length; check++)
                            if (ExtTier(Settings.FiberHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                && ExtRareState(Settings.FiberHarvestTier[check]) == TargetHarv_2.RareState)
                            {
                                return true;
                            }
                        return false;
                    }
                    //Hide
                    if (Settings.HideHarvestCondition && TargetHarv_2.ResourceType == "HIDE")
                    {
                        for (int check = 0; check < Settings.HideHarvestTier.Length; check++)
                            if (ExtTier(Settings.HideHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                && ExtRareState(Settings.HideHarvestTier[check]) == TargetHarv_2.RareState)
                            {
                                return true;
                            }
                        return false;
                    }
                }
            }

            if (TargetMob != null)
            {
                if (Settings.HideHarvestCondition)
                {
                    for (int check = 0; check < Settings.HideHarvestTier.Length; check++)
                        if (ExtTier(Settings.HideHarvestTier[check]) == TargetMob.Tier //Verifica se o tier está na lista
                            && ExtRareState(Settings.HideHarvestTier[check]) == TargetMob.RareState)
                        {
                            return true;
                        }
                    return false;
                }

            }

            return false;
        }

        public static void _Mount() //Method para mountar, verificando tudo...
        {
            bool TimerNext = false;
            if (MEquiped)  //Verifica se tem a montaria equipada e nao esta montado
            {
                ConcurrentTaskManager.GameRun(() =>
                {
                    if (!Mount.IsMounted)
                    {
                        if (Mount.Location.X != 0f && Mount.Location.X != 0f)
                        {
                            Mount.MountIfCasted();
                            TimerNext = true;
                        }
                        else
                        {
                            Mount.MountDismount();
                            TimerNext = true;
                        }
                    }
                });
                if (TimerNext)
                    Thread.Sleep(2200);
            }
        }

        public static bool MountEquiped
        {
            get
            {
                //Define a variavel como nulo
                ItemInstance Check_Mount = null;
                ConcurrentTaskManager.GameRun(() => {
                    //apos coletar as informações checa se esta dentro das condiçoes
                    Check_Mount = Item.Items
                        .Where(i => (int)i.SlotOfInventory == 7)
                        .Where(i => !String.IsNullOrEmpty(i.Name))
                        /*.Where(i => (bool)i.IsEquiped)*/
                        .FirstOrDefault();
                });
                //caso o Check_Mount seja null é por que as condições não foram atendidas
                if (Check_Mount != null)
                    return true;
                return false;
            }
        }



        public static Vector3 Adjust(Vector3 Start, Vector3 End)
        {
            Vector3 LocationAdjust = null;
            ConcurrentTaskManager.GameRun(() =>
            {
                for (float adjust = 0; adjust < 10; adjust = adjust + 0.1f)
                {
                    if (Player.CanMove(Vector3.DistanceAdjust(Start, End, adjust)))
                        LocationAdjust = Vector3.DistanceAdjust(Start, End, adjust);
                }
            });
            return LocationAdjust;
        }

        public static bool Action_Idle()
        {
            //Log("Idle")	;
            //«/Thread.Sleep(500);
            return false;
        }

        public static int ExtTier(double TierCompost)
        {
            if (TierCompost.ToString().Length == 1) //Verifica se é maior que 0
            {
                return int.Parse(TierCompost.ToString());
            }
            else
            {
                return int.Parse(Left(TierCompost.ToString(), 1));
            }
        }

        public static int ExtRareState(double TierCompost)
        {
            if (TierCompost.ToString().Length == 1) //Verifica se é maior que 0
            {
                return 0;
            }
            else
            {
                return int.Parse(Right(TierCompost.ToString(), 1));
            }
        }

        public static string Left(string str, int count)
        {
            if (string.IsNullOrEmpty(str) || count < 1)
                return string.Empty;
            else
                return str.Substring(0, Math.Min(count, str.Length));
        }

        public static string Right(string str, int length)
        {
            return str.Substring(str.Length - length, length);
        }

        public static void Log(string message)
        {
            Output.Log(message, ScriptName);
        }

        public static void ClearLog()
        {
            Output.ClearLog(ScriptName);
        }

        public static T FromXml<T>(string data)
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(data))
            {
                object obj = s.Deserialize(reader);
                return (T)obj;
            }
        }

        public static void MoveAndWait(Vector3[] path, bool sentido) //true = vai // false = volta
        {
            Vector3 NextPoint = null;
            Vector3 PlayerLocation = null;
            bool HasPlayer = false;
            int TimerExit = 0;

            while (true)
            {

                ConcurrentTaskManager.GameRun(() =>
                {
                    HasPlayer = Player.HasPlayer;
                    if (HasPlayer)
                    {
                        PlayerLocation = Player.Location;
                        NextPoint = path
                        .OrderBy(i => i.DistanceXZ(PlayerLocation)).FirstOrDefault();
                    }
                });
                if (HasPlayer)
                {
                    if (NextPoint == null) return;

                    if (PlayerLocation.DistanceXZ(NextPoint) > 7f)
                        MoveAndWait(NextPoint);

                    var index = Array.IndexOf(path, NextPoint) + (sentido ? 1 : -1);

                    if ((index < 0) || (index >= path.Count()))
                    {
                        return;
                    }

                    NextPoint = path[index];

                    if (NextPoint == null) return;

                    while (true)
                    {
                        ConcurrentTaskManager.GameRun(() =>
                        {
                            HasPlayer = Player.HasPlayer;
                            if (HasPlayer)
                            {
                                Player.Move(NextPoint);

                                PlayerLocation = Player.Location;


                                TimerExit++;
                            }
                        });
                        Thread.Sleep(10);

                        if (HasPlayer)
                        {
                            if (TimerExit >= 150) break; //timeout
                            if (PlayerLocation.DistanceXZ(NextPoint) < 2.5f) break; //quando a distancia for menor que 2.0f ele vai para o proximo.
                        }
                    }
                }
            }
        }

        public static void MoveAndWait(Vector3[] path)
        {
            Vector3 NextPoint = null;
            Vector3 PlayerLocation = null;
            bool HasPlayer = false;
            bool IsCasting = false;
            int TimerExit = 0;

            while (true)
            {

                ConcurrentTaskManager.GameRun(() =>
                {
                    HasPlayer = Player.HasPlayer;
                    IsCasting = Mount.IsCasting;
                    if (HasPlayer)
                    {
                        PlayerLocation = Player.Location;
                        NextPoint = path
                        .OrderBy(i => i.DistanceXZ(PlayerLocation)).FirstOrDefault();
                    }
                });


                if (HasPlayer && !IsCasting)
                {
                    _Mount();

                    if (NextPoint == null) return;

                    if (PlayerLocation.DistanceXZ(NextPoint) > 7f)
                        MoveAndWait(NextPoint);

                    var index = Array.IndexOf(path, NextPoint) + (forward ? 1 : -1);

                    if ((index < 0) || (index >= path.Count()))
                    {
                        forward = !forward;
                        return;
                    }

                    NextPoint = path[index];

                    if (NextPoint == null) return;

                    while (true)
                    {
                        ConcurrentTaskManager.GameRun(() =>
                        {
                            HasPlayer = Player.HasPlayer;
                            if (HasPlayer)
                            {
                                Player.Move(NextPoint);
                                PlayerLocation = Player.Location;
                                TimerExit++;
                            }
                        });
                        Thread.Sleep(10);

                        if (HasPlayer)
                        {
                            if (_Check_IsCombat) return;

                            HarvestableInstance[] TargetHarv = null;

                            ConcurrentTaskManager.GameRun(() =>
                            {
                                PlayerLocation = Player.Location;

                                TargetHarv = Harvest.Harvestables
                                    .Where(i => NextPoint.DistanceXZ(i.Location) <= Settings.RangeSearch)
                                    //.Where(i => Player.CanMove(Vector3.DistanceAdjust(PlayerLocation, i.Location, 4f)))
                                    .Where(i => i.Charges > 0)
                                    .Where(i => i.ResourceType == "WOOD" || i.ResourceType == "ROCK" || i.ResourceType == "ORE" || i.ResourceType == "HIDE" || i.ResourceType == "FIBER")
                                     .OrderBy(i => NextPoint.DistanceXZ(i.Location))
                                    .ToArray();

                            });

                            foreach (var TargetHarv_2 in TargetHarv)
                            {
                                //Wood
                                if (Settings.WoodHarvestCondition && TargetHarv_2.ResourceType == "WOOD")
                                {
                                    for (int check = 0; check < Settings.WoodHarvestTier.Length; check++)
                                        if (ExtTier(Settings.WoodHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                            && ExtRareState(Settings.WoodHarvestTier[check]) == TargetHarv_2.RareState)
                                        {
                                            //Log("[" + TargetHarv_2.ResourceType + "] Tier[" + TargetHarv_2.Tier + "." + TargetHarv_2.RareState + "] " + TargetHarv_2.Location.DistanceXZ(PlayerLocation));

                                            return;
                                        }
                                    break;
                                }
                                //Rock	
                                if (Settings.RockHarvestCondition && TargetHarv_2.ResourceType == "ROCK")
                                {
                                    for (int check = 0; check < Settings.RockHarvestTier.Length; check++)
                                        if (ExtTier(Settings.RockHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                            && ExtRareState(Settings.RockHarvestTier[check]) == TargetHarv_2.RareState)
                                        {
                                            return;
                                        }
                                    break;
                                }
                                //Ore		
                                if (Settings.OreHarvestCondition && TargetHarv_2.ResourceType == "ORE")
                                {
                                    for (int check = 0; check < Settings.OreHarvestTier.Length; check++)
                                        if (ExtTier(Settings.OreHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                            && ExtRareState(Settings.OreHarvestTier[check]) == TargetHarv_2.RareState)
                                        {
                                            return;
                                        }
                                    break;
                                }
                                //Fiber	
                                if (Settings.FiberHarvestCondition && TargetHarv_2.ResourceType == "FIBER")
                                {
                                    for (int check = 0; check < Settings.FiberHarvestTier.Length; check++)
                                        if (ExtTier(Settings.FiberHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                            && ExtRareState(Settings.FiberHarvestTier[check]) == TargetHarv_2.RareState)
                                        {
                                            return;
                                        }
                                    break;
                                }
                                //Hide
                                if (Settings.HideHarvestCondition && TargetHarv_2.ResourceType == "HIDE")
                                {
                                    for (int check = 0; check < Settings.HideHarvestTier.Length; check++)
                                        if (ExtTier(Settings.HideHarvestTier[check]) == TargetHarv_2.Tier //Verifica se o tier está na lista
                                            && ExtRareState(Settings.HideHarvestTier[check]) == TargetHarv_2.RareState)
                                        {
                                            return;
                                        }
                                    break;
                                }
                            }

                            if (TimerExit >= 150) break; //timeout
                            if (PlayerLocation.DistanceXZ(NextPoint) < 3.5f) break; //quando a distancia for menor que 2.0f ele vai para o proximo.

                        }
                    }
                }
            }
        }

        public static void MoveAndWait(Vector3 destiny)
        {
            //Cria e Seta a variavel de localização do player
            Vector3 PlayerLocation = Vector3.Zero;
            Vector3 PlayerLocationNew = Vector3.Zero;
            int TimerExit = 0;
            do
            {
                //Coleta informações de dentro do Game
                ConcurrentTaskManager.GameRun(() =>
                {
                    //Define a Movimentação destino
                    Player.Move(destiny);

                    //Atribui valor da Localização ao Player
                    PlayerLocation = Player.Location;

                    //Adiciona 1 ao timer atual
                    TimerExit++;
                    //Log((PlayerLocation == destiny).ToString()) ;
                });
                Thread.Sleep(10);
                if (TimerExit >= 150) return; //timeout
                if (PlayerLocation.DistanceXZ(destiny) < 1.5f) return; //quando a distancia for menor que 2.0f ele vai para o proximo.
            } while (true);
        }

        public static void MoveAndWait2(Vector3 destiny)
        {
            int timer = 0;
            WayDist = 0f;

            Vector3 playerLocation = Vector3.Zero;
            do
            {
                ConcurrentTaskManager.GameRun(() =>
                {

                    if (!Player.IsMoving)
                    {
                        timer++;
                    }

                    playerLocation = Player.Location;
                    Player.Move(destiny);
                });
                WayDist = playerLocation.DistanceXZ(destiny);

                if (WayDist > 30f) break;
                if (timer > 300) break;

                Thread.Sleep(10);
            } while (playerLocation.DistanceXZ(destiny) > 4.0f);
        }

        public static void Attack(MobInstance mob)
        {
            ConcurrentTaskManager.GameRun(() =>
            {
                if (mob.IsDead) return; //se tiver morto ele sai

                var types = Player.AttackType;

                if (types == "Melee") //verifica se é meele
                    atkd = (float)Player.AttackRange; //12f ?
                else
                    atkd = (float)Player.AttackRange;

                float Mobx = mob.Location.X;
                float Mobz = mob.Location.Z;
                var Plocation = Player.Location;
                var Moblocation = mob.Location;
                var CurId = mob.Id;

                if (Player.SelectedtId != CurId) Player.Select(CurId); //seleciona o id

                if (CurId != LastMobId)
                {
                    LastMobId = CurId;
                    Log("Attacking " + mob.Name);
                }

                if (Player.Location.DistanceXZ(Moblocation) > (float)atkd)
                {
                    if (Player.CanMove(Vector3.DistanceAdjust(Plocation, Moblocation, (float)atkd)))
                    {
                        //Player.StopAllActions();
                        Player.Move(Vector3.DistanceAdjust(Plocation, Moblocation, (float)atkd));
                        return;
                    }
                }

                if (Spell.CanCast(0) && Player.Location.DistanceXZ(Moblocation) <= Spell.Range(0)) Spell.CastSlotXZ(0, Mobx, Mobz);

                if (Spell.CanCast(4) && Player.Location.DistanceXZ(Moblocation) <= Spell.Range(4)) Spell.CastSlotXZ(4, Mobx, Mobz);

                if (Spell.IsActived(Spell.Name(2)) == false && Spell.CanCast(2)) Spell.CastSlot(2);

                if (Spell.IsActived(Spell.Name(1)) == false && Spell.CanCast(1)) Spell.CastSlot(1);

                Player.DefaultAttack();
            });

        }
    }
}


