﻿using CyanStars.Framework.Logger;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Logger;
using CyanStars.Gameplay.Evaluate;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// Hold音符
    /// </summary>
    public class HoldNote : BaseNote
    {

        /// <summary>
        /// Hold音符的检查输入结束时间
        /// </summary>
        private float holdCheckInputEndTime;

        /// <summary>
        /// Hold音符长度
        /// </summary>
        private float holdLength;

        /// <summary>
        /// 头判是否成功
        /// </summary>
        private bool headSucess;

        /// <summary>
        /// 累计有效时长值(0-1)
        /// </summary>
        private float value;


        private int pressCount;
        private float pressTime;
        private float pressStartTime;

        public override void Init(NoteData data, NoteLayer layer)
        {
            base.Init(data, layer);

            holdLength = (data.HoldEndTime - data.StartTime) / 1000f;
            //hold结束时间点与长度相同
            holdCheckInputEndTime = -holdLength;
        }

        public override bool CanReceiveInput()
        {
            return LogicTimer <= EvaluateHelper.CheckInputStartTime && LogicTimer >= holdCheckInputEndTime;
        }

        public override void OnUpdate(float deltaTime, float noteSpeedRate)
        {
            base.OnUpdate(deltaTime, noteSpeedRate);

            if (pressCount > 0 && LogicTimer <= 0 || dataModule.IsAutoMode)
            {
                //只在音符区域内计算有效时间
                pressTime += deltaTime;
            }

            if (LogicTimer < holdCheckInputEndTime)
            {
                if (!headSucess)
                {
                    //被漏掉了 miss
                    //Debug.LogError($"Hold音符miss：{data}");
                    LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteJudgeLogArgs(data, EvaluateType.Miss, 0, 0));
                    dataModule.MaxScore += 2;
                    dataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue);
                }
                else
                {
                    viewObject.DestroyEffectObj();
                    if (pressStartTime < 0) value = pressTime / (pressStartTime - LogicTimer);
                    else value = pressTime / holdLength;

                    EvaluateType et = EvaluateHelper.GetHoldEvaluate(value);
                    //Debug.LogError($"Hold音符命中，百分比:{value},评价:{et},{data}");
                    LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteJudgeLogArgs(data, et, pressTime, value));
                    dataModule.MaxScore++;
                    if (et == EvaluateType.Exact)
                        dataModule.RefreshPlayingData(0, 1, et, float.MaxValue);
                    else if (et == EvaluateType.Great)
                        dataModule.RefreshPlayingData(0, 0.75f, et, float.MaxValue);
                    else if (et == EvaluateType.Right)
                        dataModule.RefreshPlayingData(0, 0.5f, et, float.MaxValue);
                    else
                        dataModule.RefreshPlayingData(-1, -1, et, float.MaxValue);
                }

                DestroySelf();
            }
        }

        public override void OnUpdateInAutoMode(float deltaTime, float noteSpeedRate)
        {
            base.OnUpdateInAutoMode(deltaTime, noteSpeedRate);

            if (EvaluateHelper.GetTapEvaluate(LogicTimer) == EvaluateType.Exact && !headSucess)
            {
                headSucess = true;
                dataModule.MaxScore++;
                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteHeadJudgeLogArgs(data, EvaluateType.Exact));
                dataModule.RefreshPlayingData(1, 1, EvaluateType.Exact, 0);
                viewObject.CreateEffectObj(NoteData.NoteWidth);
            }

            if (LogicTimer < holdCheckInputEndTime)
            {
                viewObject.DestroyEffectObj();
                dataModule.MaxScore++;
                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteJudgeLogArgs(data, EvaluateType.Exact, holdLength, 1));
                dataModule.RefreshPlayingData(0, 1, EvaluateType.Exact, float.MaxValue);
                DestroySelf();
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            switch (inputType)
            {
                case InputType.Down:

                    if (!headSucess)
                    {
                        //判断头判评价
                        EvaluateType et = EvaluateHelper.GetTapEvaluate(LogicTimer);
                        if (et == EvaluateType.Bad || et == EvaluateType.Miss)
                        {
                            //头判失败直接销毁
                            DestroySelf(false);
                            //Debug.LogError($"Hold头判失败,时间：{LogicTimer}，{data}");
                            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteHeadJudgeLogArgs(data, et));
                            dataModule.MaxScore += 2;
                            dataModule.RefreshPlayingData(-1, -1, et, float.MaxValue);
                            return;
                        }

                        //Debug.LogError($"Hold头判成功,时间：{LogicTimer}，{data}");
                        LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteHeadJudgeLogArgs(data, et));
                        dataModule.MaxScore++;
                        if (et == EvaluateType.Exact)
                            dataModule.RefreshPlayingData(1, 1, et, LogicTimer);
                        else if (et == EvaluateType.Great)
                            dataModule.RefreshPlayingData(1, 0.75f, et, LogicTimer);
                        else if (et == EvaluateType.Right)
                            dataModule.RefreshPlayingData(1, 0.5f, et, LogicTimer);
                        pressStartTime = LogicTimer;
                    }

                    //头判成功
                    headSucess = true;
                    if (pressCount == 0) viewObject.CreateEffectObj(NoteData.NoteWidth);
                    pressCount++;
                    break;

                case InputType.Up:

                    if (pressCount > 0)
                    {
                        pressCount--;
                        if (pressCount == 0) viewObject.DestroyEffectObj();
                    }

                    break;
            }

        }
    }
}