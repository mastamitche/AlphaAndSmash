using System;
using System.Collections.Generic;
using System.Text;
using AOKore.Concurrent;
using AOKore.Game;
using AOKore.Script;
using System.ComponentModel;
using System.Linq;

namespace AlbionBot.AlbionBot
{
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
}
