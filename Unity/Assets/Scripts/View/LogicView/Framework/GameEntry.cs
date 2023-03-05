using Lockstep.Game;
using UnityEngine;

namespace Lockstep
{
    public class GameEntry : MonoBehaviour
    {
        private readonly Launcher mLauncher = new Launcher();
        private IServiceContainer mServiceContainer;
        
        [SerializeField] private int maxEnemyCount = 10;
        [SerializeField] private bool isClientMode;
        [SerializeField] private bool isVideoMode;

#if UNITY_EDITOR
        public bool hasInit = false;  
        public T GetService<T>() where T : IService
        {
            return mServiceContainer.GetService<T>();
        }
#endif
        
        private void Awake()
        {
            mServiceContainer = new UnityServiceContainer();
            mServiceContainer.GetService<IConstStateService>().GameName = "FPSDemo";
            mServiceContainer.GetService<IConstStateService>().IsClientMode = isClientMode;
            mServiceContainer.GetService<IConstStateService>().IsVideoMode = isVideoMode;
            mServiceContainer.GetService<IGameStateService>().MaxEnemyCount = maxEnemyCount;
         
            Logging.Logger.OnMessage += UnityLogHandler.OnLog;
            Screen.SetResolution(1024, 768, false);
            
            mLauncher.Init(mServiceContainer, new TimeMachineContainer(), new ManagerContainer(), new EventRegisterService());
            
            gameObject.AddComponent<PingMono>();
            gameObject.AddComponent<InputMono>();
        }

        private void Start()
        {
            var stateService = mServiceContainer.GetService<IConstStateService>();
            string path = Application.dataPath;
#if UNITY_EDITOR
            path = Application.dataPath + "/../../../";
#elif UNITY_STANDALONE_OSX
        path = Application.dataPath + "/../../../../../";
#elif UNITY_STANDALONE_WIN
        path = Application.dataPath + "/../../../";
#endif
            stateService.RelPath = path;
            
            mLauncher.Start();
            
#if UNITY_EDITOR
        hasInit = true;
#endif
        }

        private void Update()
        {
            mServiceContainer.GetService<IConstStateService>().IsRunVideo = isVideoMode;
            
            mLauncher.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            mLauncher.Destroy(false);
        }

        private void OnApplicationQuit()
        {
            mLauncher.Destroy(true);
        }
    }
}