﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatAsset.Runtime
{
    /// <summary>
    /// 任务组
    /// </summary>
    public class TaskGroup
    {
        private static List<ITask> tempTaskList = new List<ITask>();

        /// <summary>
        /// 任务列表
        /// </summary>
        private List<ITask> mainTaskList = new List<ITask>();

        /// <summary>
        /// 当前任务索引
        /// </summary>
        private int curTaskIndex;
        
        /// <summary>
        /// 此任务组的优先级
        /// </summary>
        public TaskPriority Priority { get; }

        /// <summary>
        /// 任务组是否能运行
        /// </summary>
        public bool CanRun => curTaskIndex < tempTaskList.Count;

        public TaskGroup(TaskPriority priority)
        {
            Priority = priority;
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        public void AddTask(ITask task)
        {
            mainTaskList.Add(task);
            task.Group = this;
        }

        /// <summary>
        /// 移除任务
        /// </summary>
        public void RemoveTask(ITask task)
        {
            mainTaskList.Remove(task);
            task.Group = null;
        }

        /// <summary>
        /// 任务组运行前
        /// </summary>
        public void PreRun()
        {
            if (mainTaskList.Count > 0)
            {
                foreach (ITask task in mainTaskList)
                {
                    tempTaskList.Add(task);
                }
            }
        }

        /// <summary>
        /// 任务组运行后
        /// </summary>
        public void PostRun()
        {
            if (tempTaskList.Count > 0)
            {
                tempTaskList.Clear();
            }
            curTaskIndex = 0;
        }
        
        /// <summary>
        /// 运行任务组
        /// </summary>
        public bool Run()
        {

            int index = curTaskIndex;
            curTaskIndex++;
            
            ITask task = tempTaskList[index];

            try
            {
                if (task.State == TaskState.Free)
                {
                    //运行空闲状态的任务
                    task.Run();
                }

                //轮询任务
                task.Update();
            }
            catch (Exception e)
            {
                //任务出现异常 视为任务结束处理
                task.State = TaskState.Finished;
                throw;
            }
            finally
            {
                switch (task.State)
                {
                    case TaskState.Finished:
                        //任务运行结束 需要删除
                        RemoveTask(task);
                        TaskRunner.MainTaskDict.Remove(task.Name);
                        ReferencePool.Release(task);
                        break;
                };
            }

            switch (task.State)
            {
                case TaskState.Running:
                case TaskState.Finished:
                    return true;
            }

            return false;

        }


        
        
        
    }
}