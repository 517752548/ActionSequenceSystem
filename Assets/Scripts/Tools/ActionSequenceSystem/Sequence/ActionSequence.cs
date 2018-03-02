// ***************************************************************************
// Copyright (c) 2018 ZhongShan KPP Technology Co
// Copyright (c) 2018 Karsion
//   
// https://github.com/karsion
// Date: 2018-03-02 9:34
// ***************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnrealM
{
    //������������
    public class ActionSequence
    {
        internal static readonly ObjectPool<ActionSequence> opSequences = new ObjectPool<ActionSequence>(64);

        //�ڵ��б�Ĭ�ϰ�����������Ϊ8
        public readonly List<ActionNode> nodes = new List<ActionNode>(8);

        //��ǰִ�еĽڵ�����
        private int curNodeIndex = 0;

        //ʱ����
        public float timeAxis { get; private set; }

        //Ŀ�������������ٵ�ʱ�򣬱���������Ҳ��Ӧ����
        public Component id { get; private set; }
        private bool hasID = false;

        //��Ҫѭ���Ĵ���
        public int loopTime { get; private set; }

        //�Ѿ����еĴ���
        public int cycles { get; private set; }

        //�Ƿ��Ѿ�������
        public bool isFinshed { get; private set; }

        private bool bSetStop = false;

#if UNITY_EDITOR
        public static void GetObjectPoolInfo(out int countActive, out int countAll)
        {
            countActive = opSequences.countActive;
            countAll = opSequences.countAll;
        }
#endif

        internal static ActionSequence GetInstance(Component component)
        {
            return opSequences.Get().Start(component);
        }

        internal static ActionSequence GetInstance()
        {
            return opSequences.Get().Start();
        }

        #region Chaining
        //private ActionSequence StratFirstNode()
        //{
        //    if (nodes.Count == 1)
        //    {
        //        nodes[0].Start(this);
        //    }

        //    return this;
        //}

        //����һ�����нڵ�
        public ActionSequence Interval(float interval)
        {
            nodes.Add(ActionNodeInterval.Get(interval));
            //return StratFirstNode();
            return this;
        }

        //����һ�������ڵ�
        public ActionSequence Action(Action action)
        {
            nodes.Add(ActionNodeAction.Get(action));
            //return StratFirstNode();
            return this;
        }

        //����һ����ѭ�������Ķ����ڵ�
        public ActionSequence Action(Action<int> action)
        {
            ActionNodeAction actionNodeAction = ActionNodeAction.Get(action);
            nodes.Add(actionNodeAction);
            //return StratFirstNode();
            return this;
        }

        //����һ�������ڵ�
        public ActionSequence Condition(Func<bool> condition)
        {
            nodes.Add(ActionNodeCondition.Get(condition));
            //return StratFirstNode();
            return this;
        }

        //����ѭ��
        public ActionSequence Loop(int loopTime = -1)
        {
            if (loopTime > 0)
            {
                this.loopTime = loopTime - 1;
                return this;
            }

            this.loopTime = loopTime;
            return this;
        }
        #endregion

        //��������
        private ActionSequence Start(Component id)
        {
            this.id = id;
            hasID = true;

            curNodeIndex = 0;
            isFinshed = false;
            cycles = 0;
            timeAxis = 0;
            loopTime = 0;
            bSetStop = false;
            //isStopTimeAxis = false;
            return this;
        }

        private ActionSequence Start()
        {
            hasID = false;

            curNodeIndex = 0;
            isFinshed = false;
            cycles = 0;
            timeAxis = 0;
            loopTime = 0;
            bSetStop = false;
            //isStopTimeAxis = false;
            return this;
        }

        //�ⲿ����ֹͣ���ڲ�ִ�е�ʱ��Ż���ɱ��Ϊ�˱��������б��뻺���ͬ��
        internal void Stop()
        {
            bSetStop = true;
        }

        //������ɱ
        private void Kill()
        {
            if (isFinshed)
            {
                return;
            }

            id = null;
            hasID = false;

            curNodeIndex = 0;
            isFinshed = true;
            cycles = 0;
            timeAxis = 0;
            loopTime = 0;
            bSetStop = false;
            //isStopTimeAxis = false;
            Release();
        }

        //internal bool isStopTimeAxis = false;
        //���и���
        internal bool Update(float deltaTime)
        {
            //SetStop to kill || û��id��Auto kill || ��������û�м��κνڵ�
            if (bSetStop || (hasID && id == null) || (nodes.Count == 0))
            {
                Kill();
                return false;
            }

            //if (!isStopTimeAxis)
            //{
            //}

            //���������½ڵ�
            if (nodes[curNodeIndex].Update(this, deltaTime))
            {
                curNodeIndex++;
                if (curNodeIndex >= nodes.Count)
                {
                    //���д���>=ѭ�������ˣ���ֹͣ
                    if (loopTime > -1 && cycles >= loopTime)
                    {
                        Kill();
                        return false;
                    }

                    //����ѭ���Ľڵ� �� ���ޣ����д���++
                    NextLoop();
                }

                //nodes[curNodeIndex].Start(this);
            }

            return true;
        }

        //�������У����������еĽڵ�
        private void Release()
        {
            opSequences.Release(this);
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Release();
            }

            nodes.Clear();
        }

        //��������
        private void NextLoop()
        {
            cycles++;
            curNodeIndex = 0;
            //timeAxis = 0;
        }

        internal void UpdateTimeAxis(float deltaTime)
        {
            timeAxis += deltaTime;
        }
    }
}