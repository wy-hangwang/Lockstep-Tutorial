using Lockstep.Game;
using UnityEditor;
using UnityEngine;

namespace Lockstep
{
    [CustomEditor(typeof(GameEntry))]
    public class GameEntryInspector : Editor
    {
        private GameEntry mOwner;
        public int rollbackTickCount = 60;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            mOwner = target as GameEntry;
            if(mOwner == null)
                return;
            
            if(!mOwner.hasInit ) 
                return;
            
            var world = (mOwner.GetService<ISimulatorService>() as SimulatorService)?.World;
            EditorGUILayout.LabelField("CurTick " + world.Tick);
            
            rollbackTickCount = EditorGUILayout.IntField("RollbackTickCount", rollbackTickCount);
            if (GUILayout.Button("Rollback")) 
            {
                ((SimulatorService)mOwner.GetService<ISimulatorService>()).DebugRollbackToTick = world.Tick - rollbackTickCount;
            }
            
            if (GUILayout.Button("Resume")) 
            {
                mOwner.GetService<ICommonStateService>().IsPause = false;
            }
        }
    }
}
