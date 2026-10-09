using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace FireGolem
{
    /// <summary>
    /// Entry point. Hooks creature spawns and attaches FireGolemController
    /// to any creature whose id is "GolemFire".
    /// </summary>
    public class FireGolemModule : LevelModule
    {
        public override IEnumerator OnLoadCoroutine()
        {
            EventManager.onCreatureSpawn += OnCreatureSpawn;
            yield break;
        }

        public override void OnUnload()
        {
            EventManager.onCreatureSpawn -= OnCreatureSpawn;
            base.OnUnload();
        }

        private void OnCreatureSpawn(Creature creature)
        {
            if (creature?.data?.id == "GolemFire")
            {
                creature.gameObject.AddComponent<FireGolemController>();
            }
        }
    }
}
