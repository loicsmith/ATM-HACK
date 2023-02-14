using Newtonsoft.Json;
using System;
using System.IO;

namespace atmHack
{
    [Serializable]
    public class atmhackConfig
    {
        public long hackCooldown;
        public int minMoney;
        public int maxMoney;
        public int hackDuration;

        public void Save()
        {
            string contents = JsonConvert.SerializeObject((object)this);
            File.WriteAllText(atmHack.ConfPath, contents);
        }
    }
}
