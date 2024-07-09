using System;
using TMPro;
using UnityEngine.UI;
using CyanStars.Framework;
using CyanStars.Framework.Event;
using CyanStars.Framework.UI;
using CyanStars.Gameplay.Base;
using UnityEngine;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音游主界面
    /// </summary>
    [UIData(UIGroupName = UIConst.UIGroupButtom,
        UIPrefabName = "Assets/BundleRes/Prefabs/MusicGameUI/MusicGameMainPanel.prefab")]
    public class MusicGameMainPanel : BaseUIPanel
    {
        public Image ImgProgress;
        public TextMeshProUGUI TxtCombo;
        public TextMeshProUGUI TxtScoreDebug;
        public Image ImgFrame;
        public Button BtnStart;
        public TextMeshProUGUI TxtLrc;
        public Button BtnPause;
        public TextMeshProUGUI TxtVisibleScore;
        public TextMeshProUGUI TxtAccuracy;

        private MusicGameModule dataModule;

        protected override void OnCreate()
        {
            dataModule = GameRoot.GetDataModule<MusicGameModule>();

            BtnStart.onClick.AddListener(() =>
            {
                GameRoot.Event.Dispatch(EventConst.MusicGameStartEvent, this, EmptyEventArgs.Create());
                BtnStart.gameObject.SetActive(false);
            });

            BtnPause.onClick.AddListener(() =>
            {
                GameRoot.UI.OpenUIPanel<MusicGamePausePanel>(null);
            });
        }

        public override void OnOpen()
        {
            ImgProgress.fillAmount = 0;
            TxtCombo.text = "0";
            TxtScoreDebug.text = "SCORE(DEBUG):0";
            BtnStart.gameObject.SetActive(true);
            TxtLrc.text = null;
            Color color = ImgFrame.color;
            color.a = 0;
            ImgFrame.color = color;
            TxtVisibleScore.text = "000000";
            TxtAccuracy.text = $"{0:00.0000}";
            TxtAccuracy.color = Color.yellow;

            GameRoot.Event.AddListener(EventConst.MusicGameDataRefreshEvent, OnMusicGameDataRefresh);
            GameRoot.Timer.UpdateTimer.Add(OnUpdate);
        }

        public override void OnClose()
        {
            GameRoot.Event.RemoveListener(EventConst.MusicGameDataRefreshEvent, OnMusicGameDataRefresh);
            GameRoot.Timer.UpdateTimer.Remove(OnUpdate);
        }

        private void OnUpdate(float deltaTime,object userdata)
        {
            if (dataModule.RunningTimeline != null)
            {
                ImgProgress.fillAmount = dataModule.RunningTimeline.CurrentTime / dataModule.RunningTimeline.Length;
            }
        }

        /// <summary>
        /// 音游数据刷新监听
        /// </summary>
        private void OnMusicGameDataRefresh(object sender, EventArgs args)
        {
            TxtCombo.text = dataModule.Combo < 2 ? string.Empty : dataModule.Combo.ToString();
            TxtScoreDebug.text = "SCORE(DEBUG):" + dataModule.Score;
            TxtVisibleScore.text = ((int)(dataModule.Score / dataModule.FullScore * 100000)).ToString().PadLeft(6, '0');


            //刷新杂率
            float accuracy = 0, sum = 0;
            if (dataModule.DeviationList.Count > 0)
            {
                foreach (var item in dataModule.DeviationList)
                {
                    sum += Mathf.Abs(item);
                }

                accuracy = sum / dataModule.DeviationList.Count;
            }

            TxtAccuracy.text = $"{accuracy * 1000:00.0000}";    //将s转为ms表示

            if (accuracy < 0.03)
            {
                TxtAccuracy.color = Color.yellow;
            }
            else if (accuracy < 0.05)
            {
                TxtAccuracy.color = Color.blue;
            }
            else
            {
                TxtAccuracy.color = Color.white;
            }
        }




    }
}
