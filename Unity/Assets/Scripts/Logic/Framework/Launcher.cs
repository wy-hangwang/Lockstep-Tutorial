using System.Threading;
using Lockstep.Game;
using Lockstep.Math;
using Lockstep.Network;
using Lockstep.Util;
using NetMsg.Common;

namespace Lockstep
{
    public class Launcher
    {
        private bool IsRunVideo => mServiceContainer.GetService<IConstStateService>().IsRunVideo;
        private bool IsVideoMode => mServiceContainer.GetService<IConstStateService>().IsVideoMode;
        private bool IsClientMode => mServiceContainer.GetService<IConstStateService>().IsClientMode;
        
        private int CurTick => mServiceContainer.GetService<ICommonStateService>().Tick;
        private const int k_MaxRunTick = int.MaxValue;
        private const int k_JumpToTick = 10;

        private IEventRegisterService mEventRegisterService;
        private ITimeMachineContainer mTimeMachineContainer;
        private IManagerContainer mManagerContainer;
        private IServiceContainer mServiceContainer;
        private OneThreadSynchronizationContext mSyncContext;
        
        private Msg_G2C_GameStartInfo mGameStartInfo;
        private Msg_RepMissFrame mFramesInfo;
        
        public void Init(IServiceContainer serviceContainer, 
            ITimeMachineContainer timeMachineContainer, 
            IManagerContainer managerContainer, 
            IEventRegisterService eventRegisterService)
        {
            mSyncContext = new OneThreadSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(mSyncContext);

            Utils.StartServices();
            
            mServiceContainer = serviceContainer;
            mTimeMachineContainer = timeMachineContainer;
            mManagerContainer = managerContainer;
            mEventRegisterService = eventRegisterService;

            var allServices = serviceContainer.GetAllServices();
            foreach (var service in allServices)
            {
                if (service is ITimeMachine timeMachineService)
                {
                    mTimeMachineContainer.RegisterTimeMachine(timeMachineService);
                }

                if (service is BaseService baseService)
                {
                    mManagerContainer.RegisterManager(baseService);
                }
            }
            
            serviceContainer.RegisterService(mTimeMachineContainer);
            serviceContainer.RegisterService(mEventRegisterService);
        }

        public void Start()
        {
            foreach (var mgr in mManagerContainer.AllMgrs)
            {
                mgr.InitReference(mServiceContainer, mManagerContainer);
            }
            
            foreach (var mgr in mManagerContainer.AllMgrs) 
            {
                mEventRegisterService.RegisterEvent<EEvent, GlobalEventHandler>("OnEvent_", "OnEvent_".Length, EventHelper.AddListener, mgr);
            }
            
            foreach (var mgr in mManagerContainer.AllMgrs) 
            {
                mgr.DoAwake(mServiceContainer);
            }

            doAwake();
            
            foreach (var mgr in mManagerContainer.AllMgrs) 
            {
                mgr.DoStart();
            }
            
            doStart();
        }

        public void Update(float fDeltaTime)
        {
            mSyncContext.Update();
            Utils.UpdateServices();
            
            var deltaTime = fDeltaTime.ToLFloat();
            mServiceContainer.GetService<INetworkService>()?.DoUpdate(deltaTime);

            var simulatorService = mServiceContainer.GetService<ISimulatorService>();
            if (IsVideoMode && IsRunVideo && CurTick < k_MaxRunTick) {
                simulatorService.RunVideo();
                return;
            }
            if (IsVideoMode && !IsRunVideo) {
                simulatorService.JumpTo(k_JumpToTick);
            }
            simulatorService.DoUpdate(fDeltaTime);
        }

        public void Destroy(bool isApplicationQuit)
        {
            foreach (var mgr in mManagerContainer.AllMgrs) 
            {
                mgr.DoDestroy();
            }
        }
        
        private void doAwake()
        {
            if(IsVideoMode)
            {
                mServiceContainer.GetService<IConstStateService>().SnapshotFrameInterval = 20;
            }
        }

        private void doStart()
        {
            if (IsVideoMode) 
            {
                EventHelper.Trigger(EEvent.BorderVideoFrame, mFramesInfo);
                EventHelper.Trigger(EEvent.OnGameCreate, mGameStartInfo);
            }
            else if (IsClientMode) 
            {
                mGameStartInfo = mServiceContainer.GetService<IGameConfigService>().ClientModeInfo;
                EventHelper.Trigger(EEvent.OnGameCreate, mGameStartInfo);
                EventHelper.Trigger(EEvent.LevelLoadDone, mGameStartInfo);
            }
        }
    }
}