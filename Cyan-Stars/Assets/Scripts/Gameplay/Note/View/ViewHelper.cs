using System.Collections.Generic;
using CyanStars.Gameplay.Note;
using UnityEngine;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// 视图层辅助类
    /// </summary>
    public static class ViewHelper
    {
        private static Dictionary<NoteData, float> viewStartTimeDict = new Dictionary<NoteData, float>();
        private static Dictionary<NoteData, float> viewHoldEndTimeDict = new Dictionary<NoteData, float>();

        /// <summary>
        /// 视图层物体创建倒计时时间（是受速率影响的时间）
        /// </summary>
        public const float ViewObjectCreateTime = 200;

        /// <summary>
        /// 计算受速率影响的视图层音符开始时间和结束时间，用于视图层物体计算位置和长度
        /// </summary>
        public static void CalViewTime(MusicTimelineData data)
        {
            viewStartTimeDict.Clear();
            viewHoldEndTimeDict.Clear();

            float timelineSpeedRate = data.BaseSpeed * data.SpeedRate;

            foreach (LayerData layerData in data.LayerDatas)
            {
                //从第一个clip到前一个clip 受流速缩放影响后的总时间值（毫秒）
                float scaledTime = 0;

                for (int i = 0; i < layerData.ClipDatas.Count; i++)
                {
                    ClipData curClipData = layerData.ClipDatas[i];
                    float speedRate = curClipData.SpeedRate * timelineSpeedRate;

                    for (int j = 0; j < curClipData.NoteDatas.Count; j++)
                    {
                        NoteData noteData = curClipData.NoteDatas[j];

                        //之前的clip累计下来的受缩放影响的时间值，再加上当前clip到当前note这段时间缩放后的时间值
                        //就能得到当前note缩放后的开始时间，因为是毫秒所以要/1000转换为秒
                        float scaledNoteStartTime =
                            scaledTime + ((noteData.StartTime - curClipData.StartTime)) * speedRate;
                        viewStartTimeDict.Add(noteData, scaledNoteStartTime / 1000);

                        if (noteData.Type == NoteType.Hold)
                        {
                            //hold结束时间同理
                            float scaledHoldNoteEndTime =
                                scaledTime + ((noteData.HoldEndTime - curClipData.StartTime)) * speedRate;
                            viewHoldEndTimeDict.Add(noteData, scaledHoldNoteEndTime / 1000);
                        }
                    }

                    float curClipEndTime;
                    if (i < layerData.ClipDatas.Count - 1)
                    {
                        //并非最后一个clip
                        //将下一个clip的开始时间作为当前clip的结束时间
                        curClipEndTime = layerData.ClipDatas[i + 1].StartTime;
                    }
                    else
                    {
                        //最后一个clip
                        //将timeline结束时间作为最后一个clip的结束时间
                        curClipEndTime = data.Time;
                    }

                    float scaledTimeLength = curClipEndTime - curClipData.StartTime;


                    //将此clip缩放后的时间值 累加到总时间值上
                    scaledTime += scaledTimeLength * speedRate;
                }
            }
        }

        /// <summary>
        /// 获取受速率影响的视图层音符开始时间
        /// </summary>
        public static float GetViewStartTime(NoteData data)
        {
            return viewStartTimeDict[data];
        }

        /// <summary>
        /// 创建视图层物体
        /// </summary>
        public static IView CreateViewObject(NoteData data, float viewCreateTime)
        {
            GameObject go = null;
            switch (data.Type)
            {
                case NoteType.Tap:
                    go = Object.Instantiate(GameManager.Instance.TapPrefab);
                    break;
                case NoteType.Hold:
                    go = Object.Instantiate(GameManager.Instance.HoldPrefab);
                    break;
                case NoteType.Drag:
                    go = Object.Instantiate(GameManager.Instance.DragPrefab);
                    break;
                case NoteType.Click:
                    go = Object.Instantiate(GameManager.Instance.ClickPrefab);
                    break;
                case NoteType.Break:
                    go = Object.Instantiate(GameManager.Instance.BreakPrefab);
                    break;
            }

            go.transform.SetParent(GameManager.Instance.viewRoot);
            go.transform.position = GetViewObjectPos(data, viewCreateTime);
            go.transform.localScale = GetViewObjectScale(data);
            go.transform.localEulerAngles = GetViewObjectRotation(data);

            var view = go.GetComponent<ViewObject>();

            if (data.Type == NoteType.Hold)
            {
                var startTime = viewStartTimeDict[data];
                var endTime = viewHoldEndTimeDict[data];
                //(view as HoldViewObject).SetMesh(1f, endTime - startTime);
                (view as HoldViewObject).SetLength(endTime - startTime);
            }

            return view;
        }

        /// <summary>
        /// 根据音符数据获取映射后的视图层位置
        /// </summary>
        private static Vector3 GetViewObjectPos(NoteData data, float viewCreateTime)
        {
            Vector3 pos = default;

            pos.z = viewCreateTime;

            pos.y = Endpoint.Instance.leftTrans.position.y;
            if (data.Type == NoteType.Break)
            {
                if (Mathf.Abs(data.Pos - (-1)) < float.Epsilon)
                {
                    //左侧break
                    pos.x = -15;
                }
                else
                {
                    //右侧break
                    pos.x = 15;
                }

                pos.y = 4;
            }
            else
            {
                pos.x = Endpoint.Instance.GetPosWithRatio(data.Pos);
            }

            return pos;
        }

        /// <summary>
        /// 根据音符数据获取映射后的视图层缩放
        /// </summary>
        private static Vector3 GetViewObjectScale(NoteData data)
        {
            Vector3 scale = Vector3.one;

            if (data.Type != NoteType.Break)
            {
                //非Break音符需要缩放宽度
                scale.x = data.Width * Endpoint.Instance.Length;
                scale.y = 2;
            }
            else
            {
                scale.x = 1;
                scale.z = 1;
            }

            return scale;
        }

        private static Vector3 GetViewObjectRotation(NoteData data)
        {
            Vector3 rotation = Vector3.zero;
            if (data.Type == NoteType.Break)
            {
                if (Mathf.Abs(data.Pos - (-1)) < float.Epsilon)
                {
                    //左侧break
                    rotation.z = -28;
                }
                else
                {
                    //右侧break
                    rotation.z = 28;
                }
            }

            return rotation;
        }
    }
}
