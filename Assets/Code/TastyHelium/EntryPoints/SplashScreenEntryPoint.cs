using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TastyHelium
{
    public class SplashScreenEntryPoint : SceneEntryPoint
    {
        public SceneReference HomeScreen;
        public SplashScreenUI SplashScreenUI;

        public bool UseSimulatedLoad = true;
        public int MessagesToUse = 5;
        public List<ArtificialLoadingMessage> ArtificialMessages;

        public override async Task OnSceneEnter(LoadingBatch batch)
        {
            await base.OnSceneEnter(batch);
        }

        public override async Task OnReady()
        {
            await base.OnReady();

            LoadingBatch batch = new LoadingBatch();

            for(int i = 0; i < MessagesToUse; i++) {
                int rndMsgIndex = UnityEngine.Random.Range(0, ArtificialMessages.Count);
                ArtificialLoadingMessage msg = ArtificialMessages[rndMsgIndex];
                batch.Add(async () => {
                    await Task.Delay(msg.Duration <= 0 ? 500 : msg.Duration);
                }, msg.Message);
            }
            
            await batch.ProcessBatch((progress) => {
                SplashScreenUI.UpdateProgress(progress.Progress, progress.Description);
            });

            await SceneOrchestrator.LoadScene(LoadSceneMode.Single, HomeScreen);
        }

        [Serializable]
        public struct ArtificialLoadingMessage
        {
            public string Message;
            public int Duration;
        }
    }
}
